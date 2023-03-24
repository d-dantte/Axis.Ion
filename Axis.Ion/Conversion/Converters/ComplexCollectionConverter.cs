using Axis.Ion.Conversion.ClrReflection;
using Axis.Ion.Types;
using Axis.Luna.Extensions;
using Axis.Luna.FInvoke;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Axis.Ion.Conversion.Converters
{
    public class ComplexCollectionConverter : IConverter
    {
        private static readonly ConcurrentDictionary<Type, ObjectConverter> ObjectConverters = new ConcurrentDictionary<Type, ObjectConverter>();
        private static readonly ConcurrentDictionary<Type, StaticInvoker> PopulateComplexListInvokerMap = new ConcurrentDictionary<Type, StaticInvoker>();
        private static readonly ConcurrentDictionary<Type, PropertyReflectionInfo[]> ReflectionInfoMap = new ConcurrentDictionary<Type, PropertyReflectionInfo[]>();
        internal static readonly string ItemListPropertyName = "$$ItemList";
        private static readonly PropertyInfo[] ExcludedMapProperties = new[]
        {
            typeof(List<object>)
                .GetProperty(nameof(List<object>.Count))
                ?? throw new InvalidOperationException($"Property not found: {nameof(List<object>.Count)}"),

            typeof(List<object>)
                .GetProperty(nameof(List<object>.Capacity))
                ?? throw new InvalidOperationException($"Property not found: {nameof(List<object>.Capacity)}"),

            typeof(ICollection<object>)
                .GetProperty(nameof(ICollection<object>.IsReadOnly))
                ?? throw new InvalidOperationException($"Property not found: {nameof(ICollection<object>.IsReadOnly)}")
        };

        #region IClrConverter
        public bool CanConvert(Type destinationType, IIonType ion)
        {
            return
                TypeCategory.ComplexCollection == ConversionUtils.CategoryOf(destinationType)
                && ion is IonStruct @struct
                && (@struct.IsNull || @struct.Properties.Contains(ItemListPropertyName));
        }

        public object? ToClr(Type destinationType, IIonType ion, ConversionContext context)
        {
            if (destinationType is null)
                throw new ArgumentNullException(nameof(destinationType));

            var @struct = (IonStruct)ion;

            // Get the ItemList
            if (!@struct.Properties.TryGetvalue(ItemListPropertyName, out var _value))
                throw new ArgumentException("Invalid complex list: ItemList not present");

            if (_value is not IonList ionItemList)
                throw new ArgumentException("Invalid compex list: ItemList is not a struct");

            var clrItemType = ListItemTypeOf(destinationType);
            var clrItemListType = typeof(List<>).MakeGenericType(clrItemType);
            var clrItemList = ionItemList.IsNull
                ? Activator.CreateInstance(clrItemListType)
                : Ionizer.ToClr(clrItemListType, ionItemList, context.Next(context.Depth - 1));

            if (clrItemList is null)
                throw new InvalidOperationException("Could not convert ItemMap to Dictionary");

            // Get the complex map
            var objectConverter = ObjectConverters.GetOrAdd(destinationType, _type => new ObjectConverter(_type));
            var clrComplexList = objectConverter.ToClr(destinationType, @struct, CustomizeDeserializationContext(context));

            if (clrComplexList is null)
                return null;

            // populate the complex map with the item map
            var itemType = ListItemTypeOf(destinationType);
            _ = PopulateComplexListInvokerMap
                .GetOrAdd(itemType, GetPopulateComplexMapInvoker)
                .Invoke(new[] { clrComplexList, clrItemList });

            return clrComplexList;
        }
        #endregion

        #region IIonConverter
        public bool CanConvert(Type sourceType, object? instance)
        {
            return TypeCategory.ComplexCollection == ConversionUtils.CategoryOf(instance?.GetType() ?? sourceType);
        }

        public IIonType ToIon(Type sourceType, object? instance, ConversionContext context)
        {
            var targetType = instance?.GetType() ?? sourceType;

            if (targetType is null)
                throw new ArgumentException("Target type could not be resolved: null");

            if (instance is null)
                return IonStruct.Null();

            var reflectionInfoList = ReflectionInfoMap.GetOrAdd(targetType, GetReflectionInfo);
            var excludedPropertySet = ExcludedMapProperties.Select(prop => prop.Name).ToHashSet();
            var objectMap = reflectionInfoList
                .Where(info => !excludedPropertySet.Contains(info.Member.Name))
                .Where(info => info.Getter is not null)
                .Select(info => (name: info.Member.Name, value: NormalizeNull(info, info.Getter.Invoke(instance))))
                .ToDictionary(kvp => kvp.name, kvp => kvp.value);

            var itemList = ToItemEnumerable(instance);
            var newContext = context.Next(context.Depth - 1);

            var objectIonStruct = (IonStruct)Ionizer.ToIon(typeof(Dictionary<string, object>), objectMap, newContext);
            var itemIonList = (IonList)Ionizer.ToIon(typeof(IEnumerable<object>), itemList, newContext);

            var structProperties = objectIonStruct.Properties;
            structProperties[ItemListPropertyName] = itemIonList;

            return objectIonStruct;
        }
        #endregion

        private static Type ListItemTypeOf(Type listType)
        {
            if (listType is null)
                throw new ArgumentNullException(nameof(listType));

            return listType
                .GetGenericInterface(typeof(ICollection<>))?
                .GetGenericArguments()[0]
                ?? throw new ArgumentException($"Supplied type {listType} does not implement {typeof(IList<>)}");
        }

        private static ConversionContext CustomizeDeserializationContext(ConversionContext context)
        {
            var ignoredProperties = context.Options.IgnoredProperties
                .SelectMany(propMap => propMap.Value)
                .Concat(ExcludedMapProperties)
                .Distinct()
                .ToArray();

            var newContext = ConversionOptionsBuilder
                .FromOptions(context.Options)
                .WithIgnoredProperties(ignoredProperties)
                .Build()
                .ApplyTo(options => context.Next(context.Depth, options));

            return newContext;
        }

        private static void PopulateComplexList<TItem>(
            IEnumerable<TItem> complexList,
            IEnumerable<TItem> itemList)
        {
            foreach (var item in itemList)
            {
                switch(complexList)
                {
                    // cant implement array, so no need for an array case.

                    case IList<TItem> l:
                        l.Add(item);
                        break;

                    case ISet<TItem> s:
                        s.Add(item);
                        break;

                    case Stack<TItem> st:
                        st.Push(item);
                        break;

                    case Queue<TItem> q:
                        q.Enqueue(item);
                        break;

                    default: throw new ArgumentException($"Unknown Collection type: {complexList.GetType()}");
                }
            }
        }

        private static StaticInvoker GetPopulateComplexMapInvoker(Type itemType)
        {
            if (itemType is null)
                throw new ArgumentNullException(nameof(itemType));

            var bindingFlags = BindingFlags.Static | BindingFlags.NonPublic;
            var param0Signature = Type.MakeGenericSignatureType(
                typeof(IEnumerable<>),
                Type.MakeGenericMethodParameter(0));
            var method = typeof(ComplexCollectionConverter)
                .GetMethod(nameof(PopulateComplexList), 1, bindingFlags, null, new[] { param0Signature, param0Signature }, null)
                ?? throw new InvalidOperationException($"Method not found: {nameof(PopulateComplexList)}");

            var genericMethod = method.MakeGenericMethod(itemType);
            return genericMethod.StaticInvoker();
        }

        private static PropertyReflectionInfo[] GetReflectionInfo(Type targetType)
        {
            return targetType
                .GetProperties()
                .Where(prop => PropertyReflectionInfo.IsProfileable(prop))
                .Select(property => new PropertyReflectionInfo(property))
                .ToArray();
        }

        private static IEnumerable<object?> ToItemEnumerable(object listInstance)
        {
            var enm = (IEnumerable)listInstance;
            return enm.Cast<object?>();
        }

        private static object NormalizeNull(PropertyReflectionInfo info, object value)
        {
            if (value is not null)
                return value;

            var ionType = ConversionUtils.CompatibleIonType(info.Member.PropertyType);
            return new MapConverter.TypedNull(ionType);
        }

    }
}
