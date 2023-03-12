using Axis.Ion.Types;
using Axis.Ion.Utils;
using System;
using System.IO;

namespace Axis.Ion.IO.Axion.Payload
{
    public readonly struct IonNullPayload : ITypePayload
    {
        public IonNullPayload(IonNull @null)
        {
            IonType = @null;
            Metadata = TypeMetadata.SerializeMetadata(@null);
        }

        #region Read
        public static bool TryRead(
            Stream stream,
            TypeMetadata metadata,
            SerializerOptions options,
            SymbolHashList symbolTable,
            out IonNullPayload payload)
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

        public static IonNullPayload Read(
            Stream stream,
            TypeMetadata metadata,
            SerializerOptions options,
            SymbolHashList symbolTable)
        {
            if (metadata.IonType != IonTypes.Null)
                throw new ArgumentException($"Invalid type-metadata: {metadata.IonType}. Expected type is: {IonTypes.Null}");

            var annotations = metadata.HasAnnotations
                ? TypeMetadata.ReadAnnotations(stream, options, symbolTable)
                : Array.Empty<IIonType.Annotation>();

            return new IonNullPayload(new IonNull(annotations));
        }
        #endregion

        #region ITypePayload

        public TypeMetadata Metadata { get; }

        public IIonType IonType { get; }

        public byte[] SerializeData(
            SerializerOptions options,
            SymbolHashList symbolTable)
            => Array.Empty<byte>();
        #endregion
    }
}
