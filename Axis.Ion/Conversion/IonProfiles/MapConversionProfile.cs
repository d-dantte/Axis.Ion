using Axis.Ion.Conversion.ClrReflection;
using Axis.Ion.Types;
using Axis.Luna.Extensions;
using Axis.Luna.FInvoke;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using static Axis.Ion.Conversion.IonProfiles.IConversionProfile;

namespace Axis.Ion.Conversion.IonProfiles
{
    /// <summary>
    /// Represents a converter for all basic maps that implement <see cref="IDictionary{TKey, TValue}"/>,
    /// where <c>TKey</c> is a <see cref="String"/>, and has a zero-arg constructor
    /// </summary>
    public class MapConversionProfile : IConversionProfile
    {
        private static readonly ConcurrentDictionary<Type, MapReflectionInfo> ReflectionInfoMap = new();

        public bool CanConvert(Type type) => TypeCategory.Map == CategoryOf(type);

        public object? FromIon(Type type, IIonType ion, ConversionOptions options) => FromIon(type, ion, null, options);

        internal object? FromIon(Type type, IIonType ion, object? map, ConversionOptions options)
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
                var value = Ionizer.FromIon(clrType, property.Value, options);
                reflectionInfo.SetValue.Invoke(map, new[] { property.NameText, value });
            }

            return map;
        }

        public IIonType ToIon(Type type, object? instance, ConversionOptions options)
        {
            IConversionProfile.ValidateSourceTypeCompatibility(type, instance?.GetType());

            if (instance is null)
                return IonStruct.Null();

            var ion = IonStruct.Empty();
            var mapValueType = MapItemTypeOf(type);
            var reflectionInfo = ReflectionInfoMap.GetOrAdd(mapValueType, ReflectionInfoFor);
            var props = ion.Properties;
            var keys = (IEnumerable<string>)reflectionInfo.Keys.Invoke(instance);
            
            foreach (var key in keys)
            {
                var value = reflectionInfo.GetValue.Invoke(instance, new[] { key });
                props[key] = Ionizer.ToIon(
                    value?.GetType() ?? mapValueType,
                    value,
                    options);
            }

            return ion;
        }

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
    }
}
