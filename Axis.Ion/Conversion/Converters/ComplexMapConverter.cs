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
    public class ComplexMapConverter : IConverter
    {
        private static readonly ConcurrentDictionary<Type, ObjectConverter> ObjectConverters = new ConcurrentDictionary<Type, ObjectConverter>();
        private static readonly ConcurrentDictionary<Type, PropertyReflectionInfo[]> ReflectionInfoMap = new ConcurrentDictionary<Type, PropertyReflectionInfo[]>();
        internal static readonly string ItemMapPropertyName = "$$ItemMap";
        private static readonly PropertyInfo[] ExcludedMapProperties = new[]
        {
            typeof(Dictionary<object, object>)
                .GetProperty(nameof(Dictionary<object, object>.Comparer))
                ?? throw new InvalidOperationException($"Property not found: {nameof(Dictionary<object, object>.Comparer)}"),

            typeof(Dictionary<object, object>)
                .GetProperty(nameof(Dictionary<object, object>.Values))
                ?? throw new InvalidOperationException($"Property not found: {nameof(Dictionary<object, object>.Values)}"),

            typeof(Dictionary<object, object>)
                .GetProperty(nameof(Dictionary<object, object>.Keys))
                ?? throw new InvalidOperationException($"Property not found: {nameof(Dictionary<object, object>.Keys)}"),

            typeof(Dictionary<object, object>)
                .GetProperty(nameof(Dictionary<object, object>.Count))
                ?? throw new InvalidOperationException($"Property not found: {nameof(Dictionary<object, object>.Count)}")
        };

        #region IClrConverter
        public bool CanConvert(Type destinationType, IIonType ion)
        {
            return
                TypeCategory.ComplexMap == ConversionUtils.CategoryOf(destinationType)
                && ion is IonStruct @struct
                && (@struct.IsNull || @struct.Properties.Contains(ItemMapPropertyName));
        }

        public object? ToClr(Type destinationType, IIonType ion, ConversionContext context)
        {
            if (destinationType is null)
                throw new ArgumentNullException(nameof(destinationType));

            var @struct = (IonStruct)ion;

            // Get the ItemMap
            if (!@struct.Properties.TryGetvalue(ItemMapPropertyName, out var _value))
                throw new ArgumentException("Invalid complex map: ItemMap not present");

            if (_value is not IonStruct ionItemMap)
                throw new ArgumentException("Invalid compex map: ItemMap is not a struct");

            var clrItemType = MapItemTypeOf(destinationType);
            var clrItemMapType = typeof(Dictionary<,>).MakeGenericType(
                typeof(string),
                clrItemType);
            var clrItemMap = ionItemMap.IsNull
                ? Activator.CreateInstance(clrItemMapType)
                : Ionizer.ToClr(clrItemMapType, ionItemMap, context.Next(context.Depth - 1));

            if (clrItemMap is null)
                throw new InvalidOperationException("Could not convert ItemMap to Dictionary");

            // Get the complex map
            var objectConverter = ObjectConverters.GetOrAdd(destinationType, _type => new ObjectConverter(_type));
            var clrComplexMap = objectConverter.ToClr(destinationType, @struct, CustomizeDeserializationContext(context));

            if (clrComplexMap is null)
                return null;

            // populate the complex map with the item map
            PopulateComplexMap(clrComplexMap, clrItemMap);

            return clrComplexMap;
        }
        #endregion

        #region IIonConverter
        public bool CanConvert(Type sourceType, object? instance)
        {
            return TypeCategory.ComplexMap == ConversionUtils.CategoryOf(instance?.GetType() ?? sourceType);
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

            var itemMap = ToItemMap(instance);
            var newContext = context.Next(context.Depth - 1);
            var newTargetType = typeof(Dictionary<string, object>);
            var objectIonStruct = (IonStruct)Ionizer.ToIon(newTargetType, objectMap, newContext);
            var itemMapStruct = Ionizer.ToIon(newTargetType, itemMap, newContext);
            var structProperties = objectIonStruct.Properties;
            structProperties[ItemMapPropertyName] = itemMapStruct;

            return objectIonStruct;
        }
        #endregion

        private static PropertyReflectionInfo[] GetReflectionInfo(Type targetType)
        {
            return targetType
                .GetProperties()
                .Where(prop => PropertyReflectionInfo.IsProfileable(prop))
                .Select(property => new PropertyReflectionInfo(property))
                .ToArray();
        }

        public static IDictionary<string, object?> ToItemMap(object mapInstance)
        {
            var map = (IDictionary)mapInstance;
            return map.Cast<string, object?>();
        }

        private static void PopulateComplexMap(object complexMap, object itemMap)
        {
            var mainDict = (IDictionary)complexMap;
            var itemDict = (IDictionary)itemMap;

            foreach (var item in itemDict)
            {
                var entry = (DictionaryEntry)(item ?? throw new InvalidOperationException());
                mainDict[entry.Key] = entry.Value;
            }
        }

        private static Type MapItemTypeOf(Type mapType)
        {
            if (mapType is null)
                throw new ArgumentNullException(nameof(mapType));

            return mapType
                .GetGenericInterface(typeof(IDictionary<,>))?
                .GetGenericArguments()[1]
                ?? throw new ArgumentException($"Supplied type {mapType} does not implement {typeof(IDictionary<,>)}");
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

        private static object NormalizeNull(PropertyReflectionInfo info, object value)
        {
            if (value is not null)
                return value;

            var ionType = ConversionUtils.CompatibleIonType(info.Member.PropertyType);
            return new MapConverter.TypedNull(ionType);
        }
    }
}
