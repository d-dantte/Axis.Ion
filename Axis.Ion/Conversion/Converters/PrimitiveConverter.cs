using Axis.Ion.Types;
using System;
using System.Numerics;

namespace Axis.Ion.Conversion.Converters
{
    /// <summary>
    /// Converts clr primitives into the ion primitives.
    /// For the sake of ion, DateTime and DateTimeOffset are regarded as Primitives
    /// </summary>
    internal class PrimitiveConverter : IConverter
    {
        #region IClrConverter
        public bool CanConvert(Type destinationType, IIonType ion)
        {
            if (destinationType is null)
                throw new ArgumentNullException(nameof(destinationType));

            if (ion is null)
                throw new ArgumentNullException(nameof(ion));

            return ion.Type switch
            {
                IonTypes.Int => destinationType.IsIntegral(out _),
                IonTypes.Float => destinationType.IsReal(out _),
                IonTypes.Decimal => destinationType.IsDecimal(out _),
                IonTypes.Timestamp => destinationType.IsDateTime(out _),
                IonTypes.Bool => destinationType.IsBoolean(out _),
                IonTypes.String => destinationType.IsString(),
                // For converting to clr maps or lists, a clr-enum type may be lost, and so the best
                // possible conversion will be to a string.
                IonTypes.IdentifierSymbol => destinationType.IsString(),
                _ => false
            };
        }

        /// <summary>
        /// Converts the given <paramref name="ion"/> instance to a clr value.
        /// </summary>
        /// <param name="destinationType">The type to convert the given ion instance into</param>
        /// <param name="ion">The ion instance</param>
        /// <param name="context">The conversion context</param>
        /// <returns>The converted clr value</returns>
        /// <exception cref="ArgumentNullException">
        ///     If either <paramref name="destinationType"/> or <paramref name="ion"/> are null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     If the conversion fails
        /// </exception>
        public object? ToClr(Type destinationType, IIonType ion, ConversionContext context)
        {
            if (destinationType is null)
                throw new ArgumentNullException(nameof(destinationType));

            if (ion is null)
                throw new ArgumentNullException(nameof(ion));

            if (destinationType.IsBoolean(out _))
            {
                if (ion.Type != IonTypes.Bool)
                    throw new ArgumentException($"Invalid ion type: {ion.Type}, expected: {IonTypes.Bool}");

                return ion.IonValue();
            }

            if (destinationType.IsString())
            {
                return ion switch
                {
                    IonString @string => @string.Value,
                    IonIdentifier identifier => identifier.Value,
                    IonQuotedSymbol quoted => quoted.Value,
                    IonOperator @operator => @operator.AsString(),
                    _ => throw new ArgumentException($"Invalid ion type: {ion.Type}, expected: {IonTypes.String}")
                };
            }

            if (destinationType.IsIntegral(out var type))
            {
                if (ion.Type != IonTypes.Int)
                    throw new ArgumentException($"Invalid ion type: {ion.Type}, expected: {IonTypes.Int}");

                var value = ion.IonValue();
                if (value is null)
                    return null;

                var bigint = (BigInteger)value;
                return Type.GetTypeCode(type) switch
                {
                    TypeCode.Int16 => (short)bigint,
                    TypeCode.UInt16 => (ushort)bigint,
                    TypeCode.Int32 => (int)bigint,
                    TypeCode.UInt32 => (uint)bigint,
                    TypeCode.Int64 => (long)bigint,
                    TypeCode.UInt64 => (ulong)bigint,
                    _ => value
                };
            }

            if (destinationType.IsReal(out type))
            {
                if (ion.Type != IonTypes.Float)
                    throw new ArgumentException($"Invalid ion type: {ion.Type}, expected: {IonTypes.Float}");

                var value = ion.IonValue();
                if (value is null)
                    return null;

                var @double = (double)value;
                return Type.GetTypeCode(type) switch
                {
                    TypeCode.Single => (float)@double,
                    _ => value
                };
            }

            if (destinationType.IsDateTime(out type))
            {
                if (ion.Type != IonTypes.Timestamp)
                    throw new ArgumentException($"Invalid ion type: {ion.Type}, expected: {IonTypes.Timestamp}");

                var value = ion.IonValue();
                if (value is null)
                    return null;

                if (typeof(DateTime).Equals(type))
                    return ((DateTimeOffset)value).DateTime;

                else return value;
            }

            if (destinationType.IsDecimal(out _))
            {
                if (ion.Type != IonTypes.Decimal)
                    throw new ArgumentException($"Invalid ion type: {ion.Type}, expected: {IonTypes.Decimal}");

                var value = ion.IonValue();
                if (value is null)
                    return null;

                return (decimal)value;
            }

            throw new ArgumentException($"Invalid primitive destination type: {destinationType}");
        }
        #endregion

        #region IIonConverter
        public bool CanConvert(Type sourceType, object? instance)
        {
            if (sourceType is null)
                throw new ArgumentNullException(nameof(sourceType));

            return instance switch
            {
                BigInteger
                or int or uint
                or long or ulong
                or short or ushort => sourceType.IsIntegral(out _),

                float or double => sourceType.IsReal(out _),

                decimal => sourceType.IsDecimal(out _),

                bool => sourceType.IsBoolean(out _),

                string => sourceType.IsString(),

                DateTime or DateTimeOffset => sourceType.IsDateTime(out _),

                null => sourceType.IsIonPrimitive(),

                _ => false
            };
        }

        /// <summary>
        /// Converts the given clr <paramref name="instance"/> into a <see cref="IIonType"/> instance.
        /// </summary>
        /// <param name="sourceType">The type from which the conversion is made</param>
        /// <param name="instance">The instance to be converted</param>
        /// <param name="context">The conversion context</param>
        /// <returns>The converted ion value</returns>
        /// <exception cref="ArgumentNullException">
        ///     If either <paramref name="destinationType"/> or <paramref name="ion"/> are null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     If the conversion fails
        /// </exception>
        public IIonType ToIon(Type sourceType, object? instance, ConversionContext context)
        {
            if (sourceType is null)
                throw new ArgumentNullException(nameof(sourceType));

            if (sourceType.IsBoolean(out _))
                return instance switch
                {
                    null => IIonType.NullOf(IonTypes.Bool),
                    bool @bool => new IonBool(@bool),
                    _ => throw new ArgumentException($"Invalid instance: {instance.GetType()}")
                };

            if (sourceType.IsString())
                return instance switch
                {
                    null => IIonType.NullOf(IonTypes.String),
                    string @string => new IonString(@string),
                    _ => throw new ArgumentException($"Invalid instance: {instance.GetType()}")
                };

            if (sourceType.IsIntegral(out _))
                return instance switch
                {
                    null => IIonType.NullOf(IonTypes.Int),
                    short @short => new IonInt(@short),
                    ushort @ushort => new IonInt(@ushort),
                    int @int => new IonInt(@int),
                    uint @uint => new IonInt(@uint),
                    long @long => new IonInt(@long),
                    ulong @ulong => new IonInt(@ulong),
                    BigInteger bi => new IonInt(bi),
                    _ => throw new ArgumentException($"Invalid instance: {instance.GetType()}")
                };

            if (sourceType.IsReal(out _))
                return instance switch
                {
                    null => IIonType.NullOf(IonTypes.Float),
                    float @float => new IonFloat(@float),
                    double @double => new IonFloat(@double),
                    _ => throw new ArgumentException($"Invalid instance: {instance.GetType()}")
                };

            if (sourceType.IsDecimal(out _))
                return instance switch
                {
                    null => IIonType.NullOf(IonTypes.Decimal),
                    decimal @decimal => new IonDecimal(@decimal),
                    _ => throw new ArgumentException($"Invalid instance: {instance.GetType()}")
                };

            if (sourceType.IsDateTime(out _))
                return instance switch
                {
                    null => IIonType.NullOf(IonTypes.Timestamp),
                    DateTimeOffset datetime => new IonTimestamp(datetime),
                    DateTime datetime => new IonTimestamp(datetime),
                    _ => throw new ArgumentException($"Invalid instance: {instance.GetType()}")
                };

            throw new ArgumentException(
                string.Format(
                    "Conversion failed for type: {0}, instance: {1}",
                    instance?.GetType() ?? sourceType,
                    instance?.ToString() ?? "null"));
        }
        #endregion
    }
}
