using Axis.Ion.Types;
using Axis.Ion.Utils;
using System;
using System.IO;
using System.Linq;

namespace Axis.Ion.IO.Axion.Payload
{
    public readonly struct IonClobPayload : ITypePayload
    {
        public IonClobPayload(IonClob @string)
        {
            IonValue = @string;
            Metadata = TypeMetadata.SerializeMetadata(@string);
        }

        #region Read
        public static bool TryRead(
            Stream stream,
            TypeMetadata metadata,
            SerializerOptions options,
            SymbolHashList symbolTable,
            out IonClobPayload payload)
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

        public static IonClobPayload Read(
            Stream stream,
            TypeMetadata metadata,
            SerializerOptions options,
            SymbolHashList symbolTable)
        {
            if (metadata.IonType != IonTypes.Clob)
                throw new ArgumentException($"Invalid symbol type-metadata: {metadata.IonType}.");

            // annotations
            var annotations = metadata.HasAnnotations
                ? TypeMetadata.ReadAnnotations(stream, options, symbolTable)
                : Array.Empty<IIonValue.Annotation>();

            // null?
            if (metadata.IsNull)
                return new IonClobPayload((IonClob)IIonValue.NullOf(IonTypes.Clob, annotations));

            // non-null?
            else
            {
                var charCount = stream.ReadVarByteInteger();
                return !stream.TryReadExactBytes((int)charCount, out var bytes)
                    ? throw new EndOfStreamException()
                    : new IonClobPayload(
                        new IonClob(
                            bytes,
                            annotations));
            }
        }
        #endregion

        #region ITypePayload

        public TypeMetadata Metadata { get; }

        public IIonValue IonValue { get; }

        public byte[] SerializeData(
            SerializerOptions options,
            SymbolHashList symbolTable)
        {
            if (IonValue.IsNull)
                return Array.Empty<byte>();

            var ionClob = (IonClob)IonValue;
            var arrayValue = ionClob.Value ?? Array.Empty<byte>();

            return arrayValue.Length
                .ToVarBytes()
                .Concat(arrayValue)
                .ToArray();
        }

        #endregion

    }
}
