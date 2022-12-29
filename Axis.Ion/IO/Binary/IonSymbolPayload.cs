using Axis.Ion.Types;
using Axis.Luna.Extensions;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axis.Ion.IO.Binary
{
    public readonly struct IonSymbolPayload : IPayloadWriter
    {
        public const int OperatorSymbolType = 1;
        public const int QuotedSymbolType = 2;
        public const int IdentifierSymbolType = 3;

        public const byte OperatorSymbolMask = 64;
        public const byte QuotedSymbolMask = 128;
        public const byte IdentifierSymbolMask = 192;

        public TypeMetadata Metadata { get; }

        public IIonType IonValue { get; }


        public IonSymbolPayload(IIonSymbol symbol)
        {
            IonValue = symbol ?? throw new ArgumentNullException(nameof(symbol));
            Metadata = TypeMetadata.ToMetadataByte(symbol);
        }

        #region Write

        public void Write(Stream outputStream)
        {
            var annotations = TypeMetadata.ToAnnotationData(IonValue);
            outputStream.WriteByte(Metadata.Metadata);
            outputStream.Write(annotations);
            outputStream.Write(SerializeText());
            outputStream.Flush();
        }

        public async Task WriteAsync(Stream outputStream)
        {
            var annotations = TypeMetadata.ToAnnotationData(IonValue);
            outputStream.WriteByte(Metadata.Metadata);
            await outputStream.WriteAsync(annotations);
            await outputStream.WriteAsync(SerializeText());
            await outputStream.FlushAsync();
        }
        #endregion

        #region Read
        public static bool TryRead(
            TypeMetadata metadata,
            Stream stream,
            out IonSymbolPayload payload)
        {
            try
            {
                payload = Read(metadata, stream);
                return true;
            }
            catch
            {
                payload = default;
                return false;
            }
        }

        public static IonSymbolPayload Read(
            TypeMetadata metadata,
            Stream stream)
        {
            if (metadata.IonType != IonTypes.OperatorSymbol
                && metadata.IonType != IonTypes.IdentifierSymbol
                && metadata.IonType != IonTypes.QuotedSymbol)
                throw new ArgumentException($"Invalid symbol type-metadata: {metadata.IonType}.");

            IIonSymbol ionSymbol;

            // annotations
            var annotations = metadata.HasAnnotations
                ? TypeMetadata.ReadAnnotations(stream)
                : Array.Empty<IIonType.Annotation>();

            // null?
            if (metadata.IsNull)
                ionSymbol = (IIonSymbol)IIonType.NullOf(metadata.IonType, annotations);

            // non-null?
            else
            {
                // symbol text
                var charCount = stream.ReadVarByteInteger();

                // will fail if the number is larger than an int. this is deliberate since streams read byte arrays
                // who are themselves constrained by the size of an integer
                var unicodeBytes = new byte[(int)(charCount * 2)];

                if (stream.Read(unicodeBytes) < unicodeBytes.Length)
                    throw new EndOfStreamException("");

                var text = Encoding.Unicode.GetString(unicodeBytes);

                ionSymbol = metadata.CustomMetadata switch
                {
                    1 => IIonSymbol.Operator.Parse(text, annotations),
                    2 => IIonSymbol.QuotedSymbol.Parse(text.WrapIn("\'"), annotations),
                    3 => IIonSymbol.Identifier.Parse(text, annotations),
                    _ => throw new ArgumentException($"Invalid symbol type id: {metadata.CustomMetadata}") // should get here though
                };
            }

            return new IonSymbolPayload(ionSymbol);
        }

        public static async Task<IonSymbolPayload> ReadAsync(
            TypeMetadata metadata,
            Stream stream)
        {
            if (metadata.IonType != IonTypes.OperatorSymbol
                && metadata.IonType != IonTypes.IdentifierSymbol
                && metadata.IonType != IonTypes.QuotedSymbol)
                throw new ArgumentException($"Invalid symbol type-metadata: {metadata.IonType}.");

            IIonSymbol ionSymbol;

            // annotations
            var annotations = metadata.HasAnnotations
                ? TypeMetadata.ReadAnnotations(stream)
                : Array.Empty<IIonType.Annotation>();

            // null?
            if (metadata.IsNull)
                ionSymbol = (IIonSymbol)IIonType.NullOf(metadata.IonType, annotations);

            // non-null?
            else
            {
                // symbol text
                var charCount = stream.ReadVarByteInteger();

                // will fail if the number is larger than an int. this is deliberate since streams read byte arrays
                // who are themselves constrained by the size of an integer
                var unicodeBytes = new byte[(int)(charCount * 2)];

                if (await stream.ReadAsync(unicodeBytes) < unicodeBytes.Length)
                    throw new EndOfStreamException("");

                var text = Encoding.Unicode.GetString(unicodeBytes);

                ionSymbol = metadata.CustomMetadata switch
                {
                    1 => IIonSymbol.Operator.Parse(text, annotations),
                    2 => IIonSymbol.QuotedSymbol.Parse(text.WrapIn("\'"), annotations),
                    3 => IIonSymbol.Identifier.Parse(text, annotations),
                    _ => throw new ArgumentException($"Invalid symbol type id: {metadata.CustomMetadata}") // should get here though
                };
            }

            return new IonSymbolPayload(ionSymbol);
        }
        #endregion

        private byte[] SerializeText()
        {
            if (IonValue.IsNull)
                return Array.Empty<byte>();

            var text = IonValue
                .ToIonText()
                .UnwrapFrom("\'");

            var charCountBytes = text.Length.ToVarBytes();
            var charBytes = Encoding.Unicode.GetBytes(text);

            return charCountBytes
                .Concat(charBytes)
                .ToArray();
        }
    }
}
