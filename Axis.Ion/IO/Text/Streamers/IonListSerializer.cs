using Axis.Ion.Types;
using Axis.Luna.Common;
using Axis.Luna.Extensions;
using Axis.Pulsar.Grammar.CST;
using System;
using System.Linq;

namespace Axis.Ion.IO.Text.Streamers
{
    public class IonListSerializer : AbstractIonTypeSerializer<IonList>
    {
        public const string IonListSymbol = "ion-list";
        public const string NullListSymbol = "null-list";
        public const string ListValueSymbol = "list-value";
        public const string IonValueSymbol = "ion-value";

        protected override string IonValueSymbolName => IonListSymbol;

        public override string GetIonValueText(IonList ionValue, SerializingContext context)
        {
            var indentedContext = context.IndentContext();
            (var ldelimiter, var valueSeparator, var rdelimiter) = context.Options.Lists.UseMultipleLines switch
            {
                false => ("[", ", ", "]"),
                true => (
                    $"[{Environment.NewLine}{indentedContext.Indentation()}",
                    $",{Environment.NewLine}{indentedContext.Indentation()}",
                    $"{Environment.NewLine}{context.Indentation()}]")
            };

            return ionValue.Value switch
            {
                null => "null.list",
                _ => ionValue.Value
                    .Select(value => TextSerializer.SerializeText(value, indentedContext))
                    .JoinUsing(valueSeparator)
                    .WrapIn(ldelimiter, rdelimiter)
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
                        .Select(TextSerializer.ParseIonValueToken)
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
