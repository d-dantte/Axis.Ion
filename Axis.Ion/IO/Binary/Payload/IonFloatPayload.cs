using Axis.Ion.Types;
using Axis.Ion.Utils;
using Axis.Luna.Extensions;
using System;
using System.IO;

namespace Axis.Ion.IO.Binary.Payload
{
    public readonly struct IonFloatPayload : ITypePayload
    {
        public const byte RegularFloatMask = 63;
        public const byte NANMask = 64;
        public const byte PositiveInfinityMask = 128;
        public const byte NegativeInfinityMask = 192;

        public const byte RegularFloatValue = 0;
        public const byte NANValue = 1;
        public const byte PositiveInfinityValue = 2;
        public const byte NegativeInfinityValue = 3;

        public IonFloatPayload(IonFloat @float)
        {
            IonType = @float;
            Metadata = SerializeFloatMetadata(@float);
        }

        private static byte SerializeFloatMetadata(IonFloat @float)
        {
            var metadata = TypeMetadata.SerializeMetadata(@float);
            return @float.Value switch
            {
                double.NaN => (byte)(metadata | NANMask),
                double.PositiveInfinity => (byte)(metadata | PositiveInfinityMask),
                double.NegativeInfinity => (byte)(metadata | NegativeInfinityMask),
                _ => (byte)(metadata & RegularFloatMask),
            };
        }

        #region Read
        public static bool TryRead(
            Stream stream,
            TypeMetadata metadata,
            SerializerOptions options,
            SymbolHashList symbolTable,
            out IonFloatPayload payload)
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

        public static IonFloatPayload Read(
            Stream stream,
            TypeMetadata metadata,
            SerializerOptions options,
            SymbolHashList symbolTable)
        {
            if (metadata.IonType != IonTypes.Float)
                throw new ArgumentException($"Invalid type-metadata: {metadata.IonType}. Expected type is: {IonTypes.Float}");

            // annotations
            var annotations = metadata.HasAnnotations
                ? TypeMetadata.ReadAnnotations(stream, options, symbolTable)
                : Array.Empty<IIonType.Annotation>();

            // null?
            if (metadata.IsNull)
                return new IonFloatPayload((IonFloat)IIonType.NullOf(metadata.IonType, annotations));

            // non-null
            else
            {
                var @double = metadata.CustomMetadata switch
                {
                    NANValue => double.NaN,
                    PositiveInfinityValue => double.PositiveInfinity,
                    NegativeInfinityValue => double.NegativeInfinity,
                    RegularFloatValue => stream.TryReadExactBytes(8, out var floatBytes)
                        ? BitConverter.ToDouble(floatBytes)
                        : throw new EndOfStreamException(),
                    _ => throw new InvalidOperationException($"Invalid float custom-metadata value: {metadata.CustomMetadata}")
                };

                return new IonFloatPayload(new IonFloat(@double, annotations));
            }
        }
        #endregion

        #region ITypePayload

        public TypeMetadata Metadata { get; }

        public IIonType IonType { get; }

        public byte[] SerializeData(
            SerializerOptions options,
            SymbolHashList hashList)
        {
            var ionFloat = (IonFloat)IonType;

            return ionFloat.Value switch
            {
                double.NaN => Array.Empty<byte>(),
                double.PositiveInfinity => Array.Empty<byte>(),
                double.NegativeInfinity => Array.Empty<byte>(),
                _ => ionFloat.Value?
                    .ApplyTo(BitConverter.GetBytes)
                    ?? Array.Empty<byte>()
            };
        }

        #endregion
    }
}
