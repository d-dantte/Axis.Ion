using Axis.Ion.Types;
using Axis.Ion.Utils;
using Axis.Luna.Extensions;
using System;
using System.IO;
using System.Linq;
using System.Numerics;

namespace Axis.Ion.IO.Axion.Payload
{
    public readonly struct IonIntPayload : ITypePayload
    {
        public const byte Int8Mask = 63;
        public const byte Int16Mask = 64;
        public const byte Int32Mask = 128;
        public const byte IntUnlimitedMask = 192;

        public const byte Int8Value = 0;
        public const byte Int16Value = 1;
        public const byte Int32Value = 2;
        public const byte IntUnlimitedValue = 3;

        public IonIntPayload(IonInt @int)
        {
            IonValue = @int;
            Metadata = SerializeIntMetadata(@int);
        }

        private static byte SerializeIntMetadata(IonInt @int)
        {
            var metadata = TypeMetadata.SerializeMetadata(@int);
            return @int.Value?.GetByteCount() switch
            {
                null => metadata,
                1 => (byte)(metadata & Int8Mask),
                2 => (byte)(metadata | Int16Mask),
                3 or 4 => (byte)(metadata | Int32Mask),
                > 4 => (byte)(metadata | IntUnlimitedMask),
                _ => throw new InvalidOperationException($"Invalid int byte-count: {@int.Value?.GetByteCount()}")
            };
        }

        #region Read
        public static bool TryRead(
            Stream stream,
            TypeMetadata metadata,
            SerializerOptions options,
            SymbolHashList symbolTable,
            out IonIntPayload payload)
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

        public static IonIntPayload Read(
            Stream stream,
            TypeMetadata metadata,
            SerializerOptions options,
            SymbolHashList symbolTable)
        {
            if (metadata.IonType != IonTypes.Int)
                throw new ArgumentException($"Invalid type-metadata: {metadata.IonType}. Expected type is: {IonTypes.Int}");

            // annotations
            var annotations = metadata.HasAnnotations
                ? TypeMetadata.ReadAnnotations(stream, options, symbolTable)
                : Array.Empty<IIonValue.Annotation>();

            // null?
            if (metadata.IsNull)
                return new IonIntPayload((IonInt)IIonValue.NullOf(metadata.IonType, annotations));

            // non-null
            else
            {
                var intBytes = metadata.CustomMetadata switch
                {
                    Int8Value => stream.TryReadByte(out var output)
                        ? new[] { output }
                        : throw new EndOfStreamException(),

                    Int16Value => stream.TryReadExactBytes(2, out var output)
                        ? output
                        : throw new EndOfStreamException(),

                    Int32Value => stream.TryReadExactBytes(4, out var output)
                        ? output
                        : throw new EndOfStreamException(),

                    IntUnlimitedValue => stream.TryReadVarByteInteger(out var output)
                        ? output.ToByteArray()
                        : throw new EndOfStreamException(),

                    _ => throw new InvalidOperationException($"Invalid int type: {metadata.CustomMetadata}")
                };

                return new IonIntPayload(new IonInt(new BigInteger(intBytes), annotations));
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

            var ionInt = (IonInt)IonValue;

            return Metadata.CustomMetadata switch
            {
                0 or 1 => ionInt.Value?
                    .ToByteArray()
                    ?? Array.Empty<byte>(),

                2 => ionInt.Value?
                    .ToByteArray()
                    .ApplyTo(array => array.Length == 3 ? array.Concat((byte)0) : array)
                    .ToArray()
                    ?? Array.Empty<byte>(),

                3 => ionInt.Value?
                    .ToVarBytes()
                    ?? Array.Empty<byte>(),

                _ => throw new InvalidOperationException($"Invalid IonInt custom-metadata: {Metadata.CustomMetadata}")
            };
        }

        #endregion
    }
}
