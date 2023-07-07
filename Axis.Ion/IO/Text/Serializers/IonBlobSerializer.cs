using Axis.Ion.Types;
using Axis.Luna.Common.Results;
using Axis.Luna.Extensions;
using Axis.Pulsar.Grammar.CST;
using System;

namespace Axis.Ion.IO.Text.Serializers
{
    public class IonBlobSerializer : IIonTextSerializer<IonBlob>
    {
        public static string GrammarSymbol => IonBlobSymbol;

        #region Symbols
        public const string IonBlobSymbol = "ion-blob";
        public const string NullBlobSymbol = "null-blob";
        public const string BlobValueSymbol = "blob-text-value";
        #endregion

        public static IResult<IonBlob> Parse(CSTNode symbolNode)
        {
            ArgumentNullException.ThrowIfNull(symbolNode);

            try
            {
                if (!IonBlobSymbol.Equals(symbolNode.SymbolName))
                    return Result.Of<IonBlob>(new ArgumentException(
                        $"Invalid symbol name: '{symbolNode.SymbolName}'. "
                        + $"Expected '{IonBlobSymbol}'"));

                (var annotations, var blobToken) = IonTextSerializer.DeconstructAnnotations(symbolNode);

                return blobToken.SymbolName switch
                {
                    NullBlobSymbol => Result.Of(new IonBlob(null, annotations)),

                    BlobValueSymbol => blobToken
                        .TokenValue()
                        .UnwrapFrom("{{", "}}")
                        .Trim()
                        .ApplyTo(b64 => Convert.FromBase64String(b64))
                        .ApplyTo(bytes => new IonBlob(bytes, annotations))
                        .ApplyTo(Result.Of),

                    _ => Result.Of<IonBlob>(new ArgumentException(
                        $"Invalid symbol encountered: '{blobToken.SymbolName}'. "
                        + $"Expected '{NullBlobSymbol}', or '{BlobValueSymbol}'"))
                };
            }
            catch (Exception e)
            {
                return Result.Of<IonBlob>(e);
            }
        }

        public static string Serialize(IonBlob value) => Serialize(value, new SerializingContext());

        public static string Serialize(IonBlob value, SerializingContext context)
        {
            var text = value.Value switch
            {
                null => "null.blob",
                _ => Convert
                    .ToBase64String(value.Value)
                    .WrapIn("{{ ", " }}")
            };

            var annotationText = IonAnnotationSerializer.Serialize(value.Annotations, context);
            return $"{annotationText}{text}";
        }
    }
}
