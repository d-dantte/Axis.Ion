using Axis.Ion.Types;
using Axis.Ion.Utils;
using System;
using System.IO;

namespace Axis.Ion.IO.Axion.Payload
{
    public readonly struct IonDecimalPayload : ITypePayload
    {
        public IonDecimalPayload(IonDecimal @decimal)
        {
            IonType = @decimal;
            Metadata = TypeMetadata.SerializeMetadata(@decimal);
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
                : Array.Empty<IIonType.Annotation>();

            // null?
            if (metadata.IsNull)
                return new IonDecimalPayload((IonDecimal)IIonType.NullOf(metadata.IonType, annotations));

            // non-null
            else
            {
                if (!stream.TryReadExactBytes(16, out var decimalBytes))
                    throw new EndOfStreamException();

                return new IonDecimalPayload(new IonDecimal(decimalBytes.ToDecimal(), annotations));
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
            var ionDecimal = (IonDecimal)IonType;

            return ionDecimal.Value?
                .GetBytes()
                ?? Array.Empty<byte>();
        }

        #endregion
    }
}
