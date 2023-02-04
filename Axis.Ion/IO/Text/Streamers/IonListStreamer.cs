using Axis.Ion.Types;
using Axis.Luna.Common;
using Axis.Luna.Extensions;
using Axis.Pulsar.Grammar.CST;
using System;
using System.Linq;

namespace Axis.Ion.IO.Text.Streamers
{
    public class IonListStreamer : AbstractIonTypeStreamer<IonList>
    {
        public const string IonListSymbol = "ion-list";
        public const string NullListSymbol = "null-list";
        public const string ListValueSymbol = "list-value";
        public const string IonValueSymbol = "ion-value";

        protected override string IonValueSymbolName => IonListSymbol;

        public override string GetIonValueText(IonList ionValue, StreamingContext context)
        {
            var valueIndentation = context.Indentation(1);
            return ionValue.Value switch
            {
                null => "null.list",
                _ => ionValue.Value
                    .Select(value => TextSerializer.StreamText(value, context.IndentContext()))
                    .Select(text => $"{valueIndentation}{text}")
                    .Append(context.Indentation())
                    .JoinUsing($",{Environment.NewLine}")
                    .WrapIn("[", "]")
            };
        }

        public override bool TryParse(CSTNode tokenNode, out IResult<IonList> result)
        {
            if (tokenNode is null)
                throw new ArgumentNullException(nameof(tokenNode));

            try
            {
                if (!IonListSymbol.Equals(tokenNode.SymbolName))
                    return $"Invalid token node name: {tokenNode.SymbolName}".FailWithMessage(out result);

                var annotationToken = tokenNode.FirstNode();
                (var annotations, var listToken) = DeconstructAnnotations(tokenNode);

                return listToken.SymbolName switch
                {
                    NullListSymbol => new IonList(annotations).PassWithValue(out result),

                    ListValueSymbol => listToken
                        .FindNodes(IonValueSymbol)
                        .Select(node => node.FirstNode())
                        .Select(TextSerializer.Parse)
                        .ApplyTo(ionValues => new IonList(
                            new IonList.Initializer(annotations, ionValues.ToArray())))
                        .PassWithValue(out result),

                    _ => $"Invalid ion bool token: {tokenNode.FirstNode().SymbolName}".FailWithMessage(out result)
                };
            }
            catch (Exception e)
            {
                result = Result.Of<IonList>(e);
                return false;
            }
        }
    }
}
