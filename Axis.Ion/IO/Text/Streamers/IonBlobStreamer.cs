using Axis.Ion.Types;
using Axis.Luna.Common;
using Axis.Luna.Extensions;
using Axis.Pulsar.Grammar.CST;
using System;

namespace Axis.Ion.IO.Text.Streamers
{
    public class IonBlobStreamer : AbstractIonTypeStreamer<IonBlob>
    {
        public const string IonBlobSymbol = "ion-blob";
        public const string NullBlobSymbol = "null-blob";
        public const string BlobValueSymbol = "blob-value";
        public const string Base64Symbol = "base-64";

        protected override string IonValueSymbolName => IonBlobSymbol;

        override public string GetIonValueText(IonBlob ionValue, StreamingContext context)
        {
            return ionValue.Value switch
            {
                null => "null.blob",
                _ => Convert
                    .ToBase64String(ionValue.Value)
                    .WrapIn("{{ ", " }}")
            };
        }

        override public bool TryParse(CSTNode tokenNode, out IResult<IonBlob> result)
        {
            if (tokenNode is null)
                throw new ArgumentNullException(nameof(tokenNode));

            try
            {
                if (!IonBlobSymbol.Equals(tokenNode.SymbolName))
                    return $"Invalid token node name: {tokenNode.SymbolName}".FailWithMessage(out result);

                var annotationToken = tokenNode.FirstNode();
                (var annotations, var blobToken) = DeconstructAnnotations(tokenNode);

                return blobToken.SymbolName switch
                {
                    NullBlobSymbol => new IonBlob(null, annotations).PassWithValue(out result),

                    BlobValueSymbol => blobToken
                        .FindNode(Base64Symbol)
                        .TokenValue()
                        .ApplyTo(b64 => Convert.FromBase64String(b64))
                        .ApplyTo(bytes => new IonBlob(bytes, annotations))
                        .PassWithValue(out result),

                    _ => $"Invalid ion bool token: {tokenNode.FirstNode().SymbolName}".FailWithMessage(out result)
                };
            }
            catch (Exception e)
            {
                result = Result.Of<IonBlob>(e);
                return false;
            }
        }
    }
}
