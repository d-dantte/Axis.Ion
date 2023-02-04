using Axis.Ion.Types;
using Axis.Luna.Common;
using Axis.Pulsar.Grammar.CST;
using System;

namespace Axis.Ion.IO.Text.Streamers
{
    public class IonNullStreamer : AbstractIonTypeStreamer<IonNull>
    {
        public const string IonNullSymbolName = "ion-null";
        public const string NullSymbolName = "null";
        public const string NullNullSymbolName = "null-null";
        public const string AnnotationListSymbolName = "annotation-list";

        protected override string IonValueSymbolName => IonNullSymbolName;

        override public string GetIonValueText(IonNull ionValue, StreamingContext context)
        {
            return context.Options.Nulls.UseLongFormNulls ? "null.null" : "null";
        }

        override public bool TryParse(CSTNode tokenNode, out IResult<IonNull> result)
        {
            if (tokenNode is null)
                throw new ArgumentNullException(nameof(tokenNode));

            try
            {
                if (!IonNullSymbolName.Equals(tokenNode.SymbolName))
                    return $"Invalid token node name: {tokenNode.SymbolName}".FailWithMessage(out result);

                (var annotations, var nullToken) = DeconstructAnnotations(tokenNode);

                return nullToken.SymbolName switch
                {
                    NullSymbolName => new IonNull(annotations).PassWithValue(out result),
                    NullNullSymbolName => new IonNull(annotations).PassWithValue(out result),
                    _ => $"Invalid ion bool token: {tokenNode.FirstNode().SymbolName}".FailWithMessage(out result)
                };
            }
            catch (Exception e)
            {
                result = Result.Of<IonNull>(e);
                return false;
            }
        }
    }
}
