using Axis.Ion.Types;
using Axis.Ion.Utils;
using Axis.Luna.Common.Numerics;
using Axis.Luna.Extensions;
using System;
using System.IO;
using System.Linq;
using System.Numerics;

namespace Axis.Ion.IO.Axion.Payload
{
    public readonly struct IonDecimalPayload : ITypePayload
    {
        public const byte Decimal16Mask = 63;
        public const byte BigDecimalMask = 64;

        public const byte Decimal16Value = 0;
        public const byte BigDecimalValue = 1;

        public IonDecimalPayload(IonDecimal @decimal)
        {
            IonValue = @decimal;
            Metadata = SerializeDecimalMetadata(@decimal);
        }

        private static byte SerializeDecimalMetadata(IonDecimal @decimal)
        {
            var metadata = TypeMetadata.SerializeMetadata(@decimal);
            return (@decimal.Value <= decimal.MaxValue) switch
            {
                true => (byte)(metadata & Decimal16Mask),
                false => (byte)(metadata | BigDecimalMask)
            };
        }

        #region Read
        public static bool TryRead(
            Stream stream,
            TypeMetadata metadata,
            SerializerOptions options,
            SymbolHashList symbolTable,
            out IonDecimalPayload payload)
        {
            try
            {
                payload = Read(stream, metadata, options, symbolTable);
                return true;
            }
            catch
            {
                payload = default;
                return false;
            }
        }

        public static IonDecimalPayload Read(
            Stream stream,
            TypeMetadata metadata,
            SerializerOptions options,
            SymbolHashList symbolTable)
        {
            if (metadata.IonType != IonTypes.Decimal)
                throw new ArgumentException($"Invalid type-metadata: {metadata.IonType}. Expected type is: {IonTypes.Decimal}");

            // annotations
            var annotations = metadata.HasAnnotations
                ? TypeMetadata.ReadAnnotations(stream, options, symbolTable)
                : Array.Empty<IIonValue.Annotation>();

            // null?
            if (metadata.IsNull)
                return new IonDecimalPayload((IonDecimal)IIonValue.NullOf(metadata.IonType, annotations));

            // non-null
            else
            {
                var @decimal = metadata.CustomMetadata switch
                {
                    Decimal16Value => !stream.TryReadExactBytes(16, out var output)
                        ? throw new EndOfStreamException()
                        : output
                            .Batch(4)
                            .Select(batch => BitConverter.ToInt32(batch.Batch.ToArray()))
                            .ApplyTo(ints => new decimal(ints.ToArray()))
                            .ApplyTo(dec => new BigDecimal(dec)),

                    BigDecimalValue => NewBigDecimal((
                        !stream.TryReadExactBytes(4, out var scale)
                            ? throw new EndOfStreamException()
                            : BitConverter.ToInt32(scale),
                        !stream.TryReadVarByteInteger(out var significand)
                            ? throw new EndOfStreamException()
                            : significand)),

                    _ => throw new InvalidOperationException($"Invalid decimal type: {metadata.CustomMetadata}")
                };

                return new IonDecimalPayload(new IonDecimal(@decimal, annotations));
            }
        }
        #endregion

        #region ITypePayload

        public TypeMetadata Metadata { get; }

        public IIonValue IonValue { get; }

        public byte[] SerializeData(
            SerializerOptions options,
            SymbolHashList hashList)
        {
            if (IonValue.IsNull)
                return Array.Empty<byte>();

            var ionDecimal = (IonDecimal)IonValue;

            if (ionDecimal.Value <= decimal.MaxValue)
                return ionDecimal.Value!.Value.ToDecimal().GetBytes();

            // else
            var scaleBytes = BitConverter.GetBytes(ionDecimal.Value!.Value.Scale);
            var significandBytes = ionDecimal.Value!.Value.Significand.ToVarBytes();
            var ionBytes = new byte[scaleBytes.Length + significandBytes.Length];

            Buffer.BlockCopy(scaleBytes, 0, ionBytes, 0, scaleBytes.Length);
            Buffer.BlockCopy(significandBytes, 0, ionBytes, scaleBytes.Length, significandBytes.Length);
            return ionBytes;
        }

        #endregion

        private static BigDecimal NewBigDecimal(
            (int scale, BigInteger significand) components)
            => new BigDecimal((components.significand, components.scale));
    }
}
