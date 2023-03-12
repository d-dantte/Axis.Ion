using Axis.Ion.Types;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Axis.Ion.Conversion.IonProfiles
{
    public class ComplexMapConversionProfile : IConversionProfile
    {
        public static readonly string ItemMapPropertyName = "$$ItemMap";
        private static readonly ConcurrentDictionary<Type, IConversionProfile> ObjectConverters = new ConcurrentDictionary<Type, IConversionProfile>();

        public bool CanConvert(Type type) => TypeCategory.ComplexMap == IConversionProfile.CategoryOf(type);

        public IIonType ToIon(Type type, object? instance, ConversionOptions options)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            var mapConverter = Ionizer.SingletonDefaultTypeConverters
                .Where(converter => typeof(MapConversionProfile).Equals(converter.GetType()))
                .FirstOrThrow(new Exception($"{nameof(MapConversionProfile)} is missing from the {nameof(Ionizer.SingletonDefaultTypeConverters)} list"));

            var objectConverter = ObjectConverters.GetOrAdd(type, targetType => new ObjectConversionProfile(targetType));

            var objectIon = objectConverter.ToIon(type, instance, options);

            if (objectIon.IsNull)
                return objectIon;

            var mapIon = mapConverter.ToIon(type, instance, options);

            var objectIonStruct = (IonStruct)objectIon;
            var properties = objectIonStruct.Properties;
            properties[ItemMapPropertyName] = mapIon;

            return objectIon;
        }

        public object? FromIon(Type type, IIonType ion, ConversionOptions options)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            var mapConverter = Ionizer.SingletonDefaultTypeConverters
                .Where(converter => typeof(MapConversionProfile).Equals(converter.GetType()))
                .FirstOrDefault()
                as MapConversionProfile
                ?? throw new Exception($"{nameof(MapConversionProfile)} is missing from the {nameof(Ionizer.SingletonDefaultTypeConverters)} list");

            var objectConverter = ObjectConverters.GetOrAdd(type, targetType => new ObjectConversionProfile(targetType));

            var obj = objectConverter.FromIon(type, ion, options);

            if (obj is null)
                return null;

            obj = mapConverter.FromIon(type, ion, obj, options);
            return obj;
        }
    }
}
