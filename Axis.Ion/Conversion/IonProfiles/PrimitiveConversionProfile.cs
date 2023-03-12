using Axis.Ion.Types;
using System;
using System.Numerics;

namespace Axis.Ion.Conversion.IonProfiles
{
    /// <summary>
    /// Converts clr primitives into the ion primitives.
    /// For the sake of ion, DateTime and DateTimeOffset are regarded as Primitives
    /// </summary>
    internal class PrimitiveConversionProfile : IConversionProfile
    {
        public bool CanConvert(Type type)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            return type.IsIntegral(out _)
                || type.IsReal(out _)
                || type.IsDecimal(out _)
                || type.IsDateTime(out _)
                || type.IsBoolean(out _)
                || type.IsString();
        }

        public object? FromIon(Type clrType, IIonType ion, ConversionOptions options)
        {
            if (clrType is null)
                throw new ArgumentNullException(nameof(clrType));

            if (ion is null)
                throw new ArgumentNullException(nameof(ion));

            if (clrType.IsBoolean(out _))
            {
                if (ion.Type != IonTypes.Bool)
                    throw new ArgumentException($"Invalid ion type: {ion.Type}, expected: {IonTypes.Bool}");

                return ion.IonValue();
            }

            if (clrType.IsString())
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

            if (clrType.IsIntegral(out var type))
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

            if (clrType.IsReal(out type))
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

            if (clrType.IsDateTime(out type))
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

            if (clrType.IsDecimal(out _))
            {
                if (ion.Type != IonTypes.Decimal)
                    throw new ArgumentException($"Invalid ion type: {ion.Type}, expected: {IonTypes.Decimal}");

                var value = ion.IonValue();
                if (value is null)
                    return null;

                return (decimal)value;
            }

            throw new ArgumentException($"Invalid primitive destination type: {clrType}");
        }

        public IIonType ToIon(Type clrType, object? instance, ConversionOptions options)
        {
            IConversionProfile.ValidateSourceTypeCompatibility(clrType, instance?.GetType());

            if (clrType is null)
                throw new ArgumentNullException(nameof(clrType));

            if (clrType.IsBoolean(out _))
                return instance switch
                {
                    null => IIonType.NullOf(IonTypes.Bool),
                    bool @bool => new IonBool(@bool),
                    _ => throw new ArgumentException($"Invalid instance: {instance.GetType()}")
                };

            if (clrType.IsString())
                return instance switch
                {
                    null => IIonType.NullOf(IonTypes.String),
                    string @string => new IonString(@string),
                    _ => throw new ArgumentException($"Invalid instance: {instance.GetType()}")
                };

            if (clrType.IsIntegral(out _))
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

            if (clrType.IsReal(out _))
                return instance switch
                {
                    null => IIonType.NullOf(IonTypes.Float),
                    float @float => new IonFloat(@float),
                    double @double => new IonFloat(@double),
                    _ => throw new ArgumentException($"Invalid instance: {instance.GetType()}")
                };

            if (clrType.IsDecimal(out _))
                return instance switch
                {
                    null => IIonType.NullOf(IonTypes.Decimal),
                    decimal @decimal => new IonDecimal(@decimal),
                    _ => throw new ArgumentException($"Invalid instance: {instance.GetType()}")
                };

            if (clrType.IsDateTime(out _))
                return instance switch
                {
                    null => IIonType.NullOf(IonTypes.Timestamp),
                    DateTimeOffset datetime => new IonTimestamp(datetime),
                    DateTime datetime => new IonTimestamp(datetime),
                    _ => throw new ArgumentException($"Invalid instance: {instance.GetType()}")
                };

            throw new ArgumentException($"Invalid instance: {instance?.GetType()}");
        }
    }
}
