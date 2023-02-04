using Axis.Ion.Types;
using Axis.Luna.Common;
using Axis.Luna.Extensions;
using Axis.Pulsar.Grammar.CST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axis.Ion.IO.Text.Streamers
{

    public class IonSexpStreamer : AbstractIonTypeStreamer<IonSexp>
    {
        public const string IonSexpSymbol = "ion-sexp";
        public const string NullSexpSymbol = "null-sexp";
        public const string SexpValueSymbol = "sexp-value";
        public const string IonValueSymbol = "ion-value";

        protected override string IonValueSymbolName => IonSexpSymbol;

        public override string GetIonValueText(IonSexp ionValue, StreamingContext context)
        {
            return ionValue.Value switch
            {
                null => "null.sexp",
                _ => ionValue.Value
                    .Select(value => TextSerializer.StreamText(value, context.IndentContext()))
                    .JoinUsing($" ")
                    .WrapIn("( ", " )")
            };
        }

        public override bool TryParse(CSTNode tokenNode, out IResult<IonSexp> result)
        {
            if (tokenNode is null)
                throw new ArgumentNullException(nameof(tokenNode));

            try
            {
                if (!IonSexpSymbol.Equals(tokenNode.SymbolName))
                    return $"Invalid token node name: {tokenNode.SymbolName}".FailWithMessage(out result);

                var annotationToken = tokenNode.FirstNode();
                (var annotations, var sexpToken) = DeconstructAnnotations(tokenNode);

                return sexpToken.SymbolName switch
                {
                    NullSexpSymbol => new IonSexp(annotations).PassWithValue(out result),

                    SexpValueSymbol => sexpToken
                        .FindNodes(IonValueSymbol)
                        .Select(node => node.FirstNode())
                        .Select(TextSerializer.Parse)
                        .ApplyTo(ionValues => new IonSexp(
                            new IonSexp.Initializer(annotations, ionValues.ToArray())))
                        .PassWithValue(out result),

                    _ => $"Invalid ion bool token: {tokenNode.FirstNode().SymbolName}".FailWithMessage(out result)
                };
            }
            catch (Exception e)
            {
                result = Result.Of<IonSexp>(e);
                return false;
            }
        }
    }
}
