using Axis.Ion.Conversion.IonProfiles;
using Axis.Ion.Types;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Axis.Ion.Conversion
{
    public static class Ionizer
    {
        private static readonly ConcurrentDictionary<Type, IConversionProfile> ObjectConverters = new ConcurrentDictionary<Type, IConversionProfile>();
        internal static readonly List<IConversionProfile> SingletonDefaultTypeConverters = new List<IConversionProfile>
        {
            new EnumConversionProfile(), // enum come before primitive because some enum types will pass the IsIntegral(...) test
            new PrimitiveConversionProfile(),
            new MapConversionProfile(),
            //new ComplexMapProfile(),
            new CollectionConversionProfile(),
            //new ComplexCollectionProfile()
        };

        public static IIonType ToIon<T>(T value, ConversionOptions? options = null)
        {
            if (value is null)
                return new IonNull();

            return ToIon(typeof(T), value, options);
        }

        public static T? FromIon<T>(IIonType ion, ConversionOptions? options = null)
        {
            if (ion is null)
                throw new ArgumentNullException(nameof(ion));

            var obj = FromIon(typeof(T), ion, options);

            if (obj is null)
                return default;

            return (T)obj;
        }

        public static IIonType ToIon(Type sourceType, object? value, ConversionOptions? options = null)
        {
            IConversionProfile.ValidateSourceTypeCompatibility(sourceType, value?.GetType());

            options ??= new ConversionOptions();
            var converters = options.Converters
                .Concat(SingletonDefaultTypeConverters)
                .Concat(
                    ObjectConverters.TryGetValue(sourceType, out var profile)
                    ? new[] { profile }
                    : Enumerable.Empty<IConverter>());

            var converter = converters
                .Where(profile => profile.CanConvert(sourceType))
                .FirstOrDefault()
                ?? (ObjectConverters[sourceType] = new ObjectConversionProfile(sourceType));

            return converter.ToIon(sourceType, value, options);
        }

        public static object? FromIon(Type destinationType, IIonType ion, ConversionOptions? options = null)
        {
            if (destinationType is null)
                throw new ArgumentNullException(nameof(destinationType));

            if (ion is null)
                throw new ArgumentNullException(nameof(ion));

            if (ion is IonNull)
                return null;

            options ??= new ConversionOptions();
            var converters = options.Converters
                .Concat(SingletonDefaultTypeConverters)
                .Concat(
                    ObjectConverters.TryGetValue(destinationType, out var profile)
                    ? new[] { profile }
                    : Enumerable.Empty<IConverter>());

            var converter = converters
                .Where(profile => profile.CanConvert(destinationType))
                .FirstOrDefault()
                ?? (ObjectConverters[destinationType] = new ObjectConversionProfile(destinationType));

            return converter.FromIon(destinationType, ion, options);
        }
    }
}
