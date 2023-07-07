using Axis.Ion.Types;
using Axis.Luna.Extensions;
using Axis.Luna.FInvoke;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static Axis.Ion.Conversion.ConversionUtils;

namespace Axis.Ion.Conversion.Converters
{
    /// <summary>
    /// Represents a converter for all basic collections that implement <see cref="IList{TValue}"/>, and have a no-arg constructor.
    /// </summary>
    public class CollectionConverter : IConverter
    {
        private static readonly ConcurrentDictionary<Type, InstanceInvoker> CollectionAdderMap = new();

        private static readonly ConcurrentDictionary<Type, StaticInvoker> IonListInvokerMap = new();

        private static readonly MethodInfo ToIonListMethod = typeof(CollectionConverter)
            .GetMethod(nameof(ToIonList), BindingFlags.Static | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException($"'{nameof(ToIonList)}' method not found");

        #region IClrConverter
        public bool CanConvert(Type destinationType, IIonValue ion)
        {
            if (ion is null)
                throw new ArgumentNullException(nameof(ion));

            if (IonTypes.List != ion.Type)
                return false;

            var category = CategoryOf(destinationType);
            return
                category == TypeCategory.Collection
                || category == TypeCategory.SingleDimensionArray;
        }

        public object? ToClr(Type destinationType, IIonValue ion, ConversionContext context)
        {
            if (destinationType is null)
                throw new ArgumentNullException(nameof(destinationType));

            if (ion is null)
                throw new ArgumentNullException(nameof(ion));

            // putting this step here so we fail-fast if "type" is not valid
            var collectionItemType = CollectionItemTypeOf(destinationType);
            var ionList = (IonList)ion;

            if (ionList.IsNull)
                return null;

            var newableCollectionType = destinationType.IsInterface
                ? NewableCollectionTypeOf(destinationType, collectionItemType)
                : destinationType;

            var collection = newableCollectionType.IsSZArray
                ? Array.CreateInstance(collectionItemType, ionList.Count)
                : Activator.CreateInstance(newableCollectionType);

            var addInvoker = CollectionAdderMap.GetOrAdd(newableCollectionType, ReflectionInfoFor);
            var index = 0;
            var ionItems = ionList.ValueOrThrow();

            if (destinationType.IsOrExtendsGenericBase(typeof(Stack<>)))
                ionItems = ionItems.Reverse().ToArray();

            foreach (var ionItem in ionItems)
            {
                if (ionItem is null)
                    throw new ArgumentNullException(nameof(ionItem));

                var clrType = CompatibleClrType(ionItem.Type, collectionItemType);
                var clrItem = ionItem.ToClrObject(clrType, context);
                var args = newableCollectionType.IsSZArray switch
                {
                    true => new[] { clrItem, index++ },
                    false => new[] { clrItem }
                };

                _ = addInvoker.Invoke(collection, args);
            }

            return collection;
        }
        #endregion

        #region IIonConverter
        public bool CanConvert(Type type, object? instance)
        {
            if (!type.IsCongruentWith(instance?.GetType()))
                return false;

            var category = CategoryOf(type);
            return
                category == TypeCategory.Collection
                || category == TypeCategory.SingleDimensionArray;
        }

        public IIonValue ToIon(Type type, object? instance, ConversionContext options)
        {
            type.ValidateCongruenceWith(instance?.GetType());

            if (instance is null)
                return IonList.Null();

            var valueType = CollectionItemTypeOf(instance?.GetType() ?? type);
            var invoker = IonListInvokerMap.GetOrAdd(valueType, IonListInvokerFor);
            var ionList = invoker.Invoke(new[] { instance, options });
            return (IonList)ionList;
        }
        #endregion

        private static IonList ToIonList<TValue>(IEnumerable<TValue> items, ConversionContext context)
        {
            var initializer = new IonList.Initializer();

            foreach (var item in items)
            {
                var ionValue = item.ToIonValue(item?.GetType() ?? typeof(TValue), context);
                var wrapper = new IonValueWrapper(ionValue);
                initializer.Add(wrapper);
            }

            return new IonList(initializer);
        }

        private static Type CollectionItemTypeOf(Type collectionType)
        {
            var genericListType = collectionType.HasGenericInterfaceDefinition(typeof(IEnumerable<>))
                ? collectionType
                : collectionType
                    .GetGenericInterface(typeof(IEnumerable<>))
                    .ThrowIfNull(new ArgumentException($"Supplied type {collectionType} does not implement {typeof(IEnumerable<>)}"));

            return genericListType.GetGenericArguments()[0];
        }

        private static InstanceInvoker ReflectionInfoFor(Type? collectionType)
        {
            if (collectionType is null)
                throw new ArgumentNullException(nameof(collectionType));

            var addValueInvoker = 
                collectionType.IsSZArray ? collectionType
                    .GetMethod(nameof(Array.SetValue), new[] { typeof(object), typeof(int)})?
                    .InstanceInvoker()
                    ?? throw new InvalidOperationException($"'SetValue' method not found in the array type: {collectionType}") :

                collectionType.IsOrImplementsGenericInterface(typeof(ISet<>)) ? collectionType
                    .GetMethod(nameof(ISet<object>.Add))?
                    .InstanceInvoker()
                    ?? throw new InvalidOperationException($"'Add' method not found in the array type: {collectionType}") :

                collectionType.IsOrExtendsGenericBase(typeof(Stack<>)) ? collectionType
                    .GetMethod(nameof(Stack<object>.Push))?
                    .InstanceInvoker()
                    ?? throw new InvalidOperationException($"'Push' method not found in the array type: {collectionType}") :

                collectionType.IsOrExtendsGenericBase(typeof(Queue<>)) ? collectionType
                    .GetMethod(nameof(Queue<object>.Enqueue))?
                    .InstanceInvoker()
                    ?? throw new InvalidOperationException($"'Enqueue' method not found in the array type: {collectionType}") :

                collectionType
                    .GetMethod(nameof(ICollection<object>.Add))?
                    .InstanceInvoker()
                    ?? throw new InvalidOperationException($"'Add' method not found in the collection type: {collectionType}");

            return addValueInvoker;
        }

        private static StaticInvoker IonListInvokerFor(Type itemType)
        {
            var method = ToIonListMethod.MakeGenericMethod(itemType);
            return StaticInvoker.InvokerFor(method);
        }

        private static Type NewableCollectionTypeOf(Type baseInterface, Type collectionItemType)
        {
            if (baseInterface.HasGenericInterfaceDefinition(typeof(IEnumerable<>))
                || baseInterface.HasGenericInterfaceDefinition(typeof(ICollection<>))
                || baseInterface.HasGenericInterfaceDefinition(typeof(IList<>))
                || baseInterface.HasGenericInterfaceDefinition(typeof(IReadOnlyCollection<>))
                || baseInterface.HasGenericInterfaceDefinition(typeof(IReadOnlyList<>)))
                return typeof(List<>).MakeGenericType(collectionItemType);

            if (baseInterface.HasGenericInterfaceDefinition(typeof(ISet<>)))
                return typeof(HashSet<>).MakeGenericType(collectionItemType);

            throw new ArgumentException($"Unknown collection interface supplied: {baseInterface}");
        }
    }
}
