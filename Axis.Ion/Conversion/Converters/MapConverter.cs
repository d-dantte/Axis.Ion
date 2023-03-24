using Axis.Ion.Conversion.ClrReflection;
using Axis.Ion.Types;
using Axis.Luna.Extensions;
using Axis.Luna.FInvoke;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using static Axis.Ion.Conversion.ConversionUtils;

namespace Axis.Ion.Conversion.Converters
{
    /// <summary>
    /// Represents a converter for all basic maps that implement <see cref="IDictionary{TKey, TValue}"/>,
    /// where <c>TKey</c> is a <see cref="String"/>, and has a zero-arg constructor
    /// </summary>
    public class MapConverter : IConverter
    {
        private static readonly ConcurrentDictionary<Type, MapReflectionInfo> ReflectionInfoMap = new();

        #region IClrConverter
        public bool CanConvert(Type destinationType, IIonType ion)
        {
            if (destinationType is null)
                throw new ArgumentNullException(nameof(destinationType));

            if (ion is null)
                throw new ArgumentNullException(nameof(ion));

            return
                IonTypes.Struct == ion.Type
                && TypeCategory.Map == CategoryOf(destinationType);
        }

        public object? ToClr(Type type, IIonType ion, ConversionContext context) => ToClr(type, ion, null, context);

        internal object? ToClr(Type type, IIonType ion, object? map, ConversionContext context)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            if (ion is null)
                throw new ArgumentNullException(nameof(ion));

            // putting this step here so we fail-fast if "type" is not valid
            var mapItemType = MapItemTypeOf(type);
            var ionStruct = (IonStruct)ion;

            if (ionStruct.IsNull)
                return null;

            var reflectionInfo = ReflectionInfoMap.GetOrAdd(mapItemType, ReflectionInfoFor);
            map ??= Activator.CreateInstance(type.IsInterface
                ? NewableMapTypeOf(type, mapItemType)
                : type);

            foreach (var property in ionStruct.ValueOrThrow())
            {
                var clrType = CompatibleClrType(property.Value.Type, mapItemType);
                var value = property.Value.ToClrObject(clrType, context);
                reflectionInfo.SetValue.Invoke(map, new[] { property.NameText, value });
            }

            return map;
        }
        #endregion

        #region IIonConverter
        public bool CanConvert(Type sourceType, object? instance)
        {
            if (sourceType is null)
                throw new ArgumentNullException(nameof(sourceType));

            return 
                sourceType.IsCongruentWith(instance?.GetType())
                && TypeCategory.Map == CategoryOf(instance?.GetType() ?? sourceType);
        }

        public IIonType ToIon(Type sourceType, object? instance, ConversionContext context)
        {
            if (sourceType is null)
                throw new ArgumentNullException(nameof(sourceType));

            sourceType.ValidateCongruenceWith(instance?.GetType());

            if (instance is null)
                return IonStruct.Null();

            var ion = IonStruct.Empty();
            var mapItemType = MapItemTypeOf(sourceType);
            var reflectionInfo = ReflectionInfoMap.GetOrAdd(mapItemType, ReflectionInfoFor);
            var props = ion.Properties;
            var keys = (IEnumerable<string>)reflectionInfo.Keys.Invoke(instance);

            foreach (var key in keys)
            {
                var value = reflectionInfo.GetValue.Invoke(instance, new[] { key });
                props[key] = value is TypedNull typedNull
                    ? IIonType.NullOf(typedNull.IonType)
                    : value.ToIonValue(value?.GetType() ?? mapItemType, context);
            }

            return ion;
        }
        #endregion

        private static Type MapItemTypeOf(Type mapType)
        {
            var genericMapType = mapType.HasGenericInterfaceDefinition(typeof(IDictionary<,>))
                ? mapType
                : mapType
                    .GetGenericInterface(typeof(IDictionary<,>))
                    .ThrowIfNull(new ArgumentException($"Supplied type {mapType} does not implement {typeof(IDictionary<,>)}"));

            return genericMapType.GetGenericArguments()[1];
        }

        private static MapReflectionInfo ReflectionInfoFor(Type mapValueType)
        {
            var genericMapType = typeof(IDictionary<,>);
            var mapType = genericMapType.MakeGenericType(typeof(string), mapValueType);

            var keysInvoker = mapType
                .GetProperty(nameof(IDictionary<object, object>.Keys))?
                .GetGetMethod()
                .InstanceInvoker()
                ?? throw new InvalidOperationException($"'Keys' property not found in the map type: {mapType}");

            var itemProperty = mapType
                .GetProperty("Item")
                ?? throw new InvalidOperationException($"'Item' property not found in the map type: {mapType}");

            var getItemInvoker = itemProperty
                .GetGetMethod()
                .InstanceInvoker();

            var setItemInvoker = itemProperty
                .GetSetMethod()
                .InstanceInvoker();

            return new MapReflectionInfo(keysInvoker, getItemInvoker, setItemInvoker);
        }

        private static Type NewableMapTypeOf(Type baseInterface, Type mapItemType)
        {
            if (baseInterface.HasGenericInterfaceDefinition(typeof(IDictionary<,>)))
                return typeof(Dictionary<,>).MakeGenericType(typeof(string), mapItemType);

            throw new ArgumentException($"Unknown collection interface supplied: {baseInterface}");
        }


        #region Nested Types

        /// <summary>
        /// In certain scenarios, e.g IDictionary{string, object}, a null entry in the dictionary will have its
        /// type lost when transforming to Ion. To avoid this, one can replace the null values with a
        /// <see cref="TypedNull"/> encapsulating the appropriate <see cref="IonTypes"/> value.
        /// </summary>
        public struct TypedNull
        {
            public IonTypes IonType { get; }

            public TypedNull(IonTypes ionType)
            {
                IonType = ionType;
            }
        }

        #endregion
    }
}

