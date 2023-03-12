using Axis.Ion.Types;
using Axis.Ion.Utils;
using Axis.Luna.Common;
using Axis.Pulsar.Grammar.CST;
using System;
using System.Globalization;

namespace Axis.Ion.IO.Text.Streamers
{
    public class IonDecimalSerializer : AbstractIonTypeSerializer<IonDecimal>
    {
        public const string IonDecimalSymbol = "ion-decimal";
        public const string NullDecimalSymbol = "null-decimal";
        public const string ScientificDecimalSymbol = "scientific-decimal";
        public const string RegularDecimalSymbol = "regular-decimal";

        protected override string IonValueSymbolName => IonDecimalSymbol;

        public override string GetIonValueText(IonDecimal ionValue, SerializingContext context)
        {
            return ionValue.Value switch
            {
                null => "null.decimal",
                _ => context.Options.Decimals.UseExponentNotation switch
                {
                    false => MakeDecimal(ionValue.Value.Value.ToString()),
                    true => ionValue.Value.Value.ToExponentNotation("D"),
                }
            };
        }

        public override bool TryParse(CSTNode tokenNode, out IResult<IonDecimal> result)
        {
            if (tokenNode is null)
                throw new ArgumentNullException(nameof(tokenNode));

            try
            {
                if (!IonDecimalSymbol.Equals(tokenNode.SymbolName))
                    return $"Invalid token node name: {tokenNode.SymbolName}".FailWithMessage(out result);

                (var annotations, var decimalToken) = DeconstructAnnotations(tokenNode);

                return decimalToken.SymbolName switch
                {
                    NullDecimalSymbol => new IonDecimal(null, annotations).PassWithValue(out result),
                    ScientificDecimalSymbol or RegularDecimalSymbol => new IonDecimal(
                        decimal.Parse(decimalToken.TokenValue().Replace("D", "E"), NumberStyles.Float),
                        annotations)
                        .PassWithValue(out result),
                    _ => $"Invalid ion bool token: {tokenNode.FirstNode().SymbolName}".FailWithMessage(out result)
                };
            }
            catch (Exception e)
            {
                result = Result.Of<IonDecimal>(e);
                return false;
            }
        }

        private string MakeDecimal(string value)
        {
            if (!value.Contains("."))
                return $"{value}.0";

            return value;
        }
    }
}
