using Axis.Ion.Types;
using Axis.Luna.Extensions;
using Axis.Luna.FInvoke;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using static Axis.Ion.Conversion.IonProfiles.IConversionProfile;

namespace Axis.Ion.Conversion.IonProfiles
{
    /// <summary>
    /// Represents a converter for all basic collections that implement <see cref="IList{TValue}"/>, and have a no-arg constructor.
    /// </summary>
    public class CollectionConversionProfile : IConversionProfile
    {
        private static readonly ConcurrentDictionary<Type, InstanceInvoker> CollectionAdderMap = new();

        private static readonly ConcurrentDictionary<Type, StaticInvoker> IonListInvokerMap = new();

        private static readonly MethodInfo ToIonListMethod = typeof(CollectionConversionProfile)
            .GetMethod(nameof(ToIonList), BindingFlags.Static | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException($"'{nameof(ToIonList)}' method not found");

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool CanConvert(Type type)
        {
            var category = CategoryOf(type);
            return
                category == TypeCategory.Collection
                || category == TypeCategory.SingleDimensionArray;
        }

        /// <summary>
        /// TODO
        /// <para>
        ///     PS: this method assumes that <see cref="CollectionConversionProfile.CanConvert(Type)"/> returns true if called on <paramref name="type"/>.
        /// </para>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="ion"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidCastException"></exception>
        public object? FromIon(Type type, IIonType ion, ConversionOptions options)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            if (ion is null)
                throw new ArgumentNullException(nameof(ion));

            // putting this step here so we fail-fast if "type" is not valid
            var collectionItemType = CollectionItemTypeOf(type);
            var ionList = (IonList)ion;

            if (ionList.IsNull)
                return null;

            var newableCollectionType = type.IsInterface
                ? NewableCollectionTypeOf(type, collectionItemType)
                : type;

            var collection = newableCollectionType.IsSZArray
                ? Array.CreateInstance(collectionItemType, ionList.Count)
                : Activator.CreateInstance(newableCollectionType);

            var addInvoker = CollectionAdderMap.GetOrAdd(newableCollectionType,  ReflectionInfoFor);
            var index = 0;
            foreach (var ionItem in ionList.ValueOrThrow())
            {
                var clrType = CompatibleClrType(ionItem.Type, collectionItemType);
                var clrItem = Ionizer.FromIon(clrType, ionItem, options);
                var args = newableCollectionType.IsSZArray switch
                {
                    true => new[] { clrItem, index++ },
                    false => new[] { clrItem }
                };

                _ = addInvoker.Invoke(collection, args);
            }

            return collection;
        }

        /// <summary>
        /// TODO
        /// <para>
        ///     PS: this method assumes that <see cref="CollectionConversionProfile.CanConvert(Type)"/> returns true if called on <paramref name="type"/>.
        /// </para>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="instance"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public IIonType ToIon(Type type, object? instance, ConversionOptions options)
        {
            ValidateSourceTypeCompatibility(type, instance?.GetType());

            if (instance is null)
                return IonList.Null();

            var valueType = CollectionItemTypeOf(instance?.GetType() ?? type);
            var invoker = IonListInvokerMap.GetOrAdd(valueType, IonListInvokerFor);
            var ionList = invoker.Invoke(new[] { instance, options });
            return (IonList)ionList;
        }

        private static IonList ToIonList<TValue>(IEnumerable<TValue> items, ConversionOptions options)
        {
            var initializer = new IonList.Initializer();

            foreach (var item in items)
            {
                var ionValue = Ionizer.ToIon(item?.GetType() ?? typeof(TValue), item, options);
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

            var addValueInvoker = collectionType.IsSZArray
                ? collectionType
                    .GetMethod(nameof(Array.SetValue), new[] { typeof(object), typeof(int)})?
                    .InstanceInvoker()
                    ?? throw new InvalidOperationException($"'SetValue' method not found in the array type: {collectionType}")
                : collectionType
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
