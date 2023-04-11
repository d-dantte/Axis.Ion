using Axis.Ion.Types;
using Axis.Ion.Utils;
using System;
using System.IO;
using System.Linq;

namespace Axis.Ion.IO.Axion.Payload
{
    public readonly struct IonBlobPayload : ITypePayload, ITypePayloadReader<IonBlobPayload>
    {
        public IonBlobPayload(IonBlob blob)
        {
            IonType = blob;
            Metadata = TypeMetadata.SerializeMetadata(blob);
        }

        #region Read
        public static bool TryRead(
            Stream stream,
            TypeMetadata metadata,
            SerializerOptions options,
            SymbolHashList symbolTable,
            out IonBlobPayload payload)
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

        public static IonBlobPayload Read(
            Stream stream,
            TypeMetadata metadata,
            SerializerOptions options,
            SymbolHashList symbolTable)
        {
            if (metadata.IonType != IonTypes.Blob)
                throw new ArgumentException($"Invalid symbol type-metadata: {metadata.IonType}.");

            // annotations
            var annotations = metadata.HasAnnotations
                ? TypeMetadata.ReadAnnotations(stream, options, symbolTable)
                : Array.Empty<IIonType.Annotation>();

            // null?
            if (metadata.IsNull)
                return new IonBlobPayload(IonBlob.Null(annotations));

            // non-null?
            else
            {
                var charCount = stream.ReadVarByteInteger();
                return !stream.TryReadExactBytes((int)charCount, out var bytes)
                    ? throw new EndOfStreamException()
                    : new IonBlobPayload(new IonBlob(bytes, annotations));
            }
        }
        #endregion

        #region ITypePayload

        public TypeMetadata Metadata { get; }

        public IIonType IonType { get; }

        public byte[] SerializeData(
            SerializerOptions options,
            SymbolHashList symbolTable)
        {
            if (IonType.IsNull)
                return Array.Empty<byte>();

            var ionBlob = (IonBlob)IonType;
            var arrayValue = ionBlob.Value ?? Array.Empty<byte>();

            return arrayValue.Length
                .ToVarBytes()
                .Concat(arrayValue)
                .ToArray();
        }

        #endregion

    }
}
