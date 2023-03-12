using Axis.Ion.Types;
using Axis.Ion.Utils;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Axis.Ion.IO.Axion.Payload
{
    public readonly struct IonStringPayload : ITypePayload
    {
        public IonStringPayload(IonString @string)
        {
            IonType = @string;
            Metadata = TypeMetadata.SerializeMetadata(@string);
        }

        #region Read
        public static bool TryRead(
            Stream stream,
            TypeMetadata metadata,
            SerializerOptions options,
            SymbolHashList symbolTable,
            out IonStringPayload payload)
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

        public static IonStringPayload Read(
            Stream stream,
            TypeMetadata metadata,
            SerializerOptions options,
            SymbolHashList symbolTable)
        {
            // annotations
            var annotations = metadata.HasAnnotations
                ? TypeMetadata.ReadAnnotations(stream, options, symbolTable)
                : Array.Empty<IIonType.Annotation>();

            // null?
            if (metadata.IsNull)
                return new IonStringPayload((IonString)IIonType.NullOf(IonTypes.String, annotations));

            // non-null?
            else
            {
                var charCount = stream.ReadVarByteInteger();
                return !stream.TryReadExactBytes(2 * (int)charCount, out var bytes)
                    ? throw new EndOfStreamException()
                    : new IonStringPayload(
                        new IonString(
                            Encoding.Unicode.GetString(bytes),
                            annotations));
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

            var ionString = (IonString)IonType;
            var stringValue = ionString.Value ?? "";

            return stringValue.Length
                .ToVarBytes() // storing char size, not byte size
                .Concat(Encoding.Unicode.GetBytes(ionString.Value ?? ""))
                .ToArray();
        }

        #endregion

    }
}
