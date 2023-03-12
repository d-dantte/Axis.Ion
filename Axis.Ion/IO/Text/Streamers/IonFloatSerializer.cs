using Axis.Ion.Types;
using Axis.Luna.Common;
using Axis.Pulsar.Grammar.CST;
using System;

namespace Axis.Ion.IO.Text.Streamers
{
    public class IonFloatSerializer : AbstractIonTypeSerializer<IonFloat>
    {
        public const string IonFloatSymbol = "ion-float";
        public const string NullFloatSymbol = "null-float";
        public const string NANSymbol = "nan";
        public const string PositiveInfinitySymbol = "pinf";
        public const string NegativeInfinitySymbol = "ninf";
        public const string ScientificSymbol = "scientific-float";

        protected override string IonValueSymbolName => IonFloatSymbol;

        public override string GetIonValueText(IonFloat ionValue, SerializingContext context)
        {
            return ionValue.Value switch
            {
                null => "null.float",
                double.NaN => "nan",
                double.PositiveInfinity => "+inf",
                double.NegativeInfinity => "-inf",
                _ => ionValue.Value.Value.ToExponentNotation()
            };
        }

        public override bool TryParse(CSTNode tokenNode, out IResult<IonFloat> result)
        {
            if (tokenNode is null)
                throw new ArgumentNullException(nameof(tokenNode));

            try
            {
                if (!IonFloatSymbol.Equals(tokenNode.SymbolName))
                    return $"Invalid token node name: {tokenNode.SymbolName}".FailWithMessage(out result);

                var annotationToken = tokenNode.FirstNode();
                (var annotations, var floatToken) = DeconstructAnnotations(tokenNode);

                return floatToken.SymbolName switch
                {
                    NullFloatSymbol => new IonFloat(null, annotations).PassWithValue(out result),
                    NANSymbol => new IonFloat(double.NaN, annotations).PassWithValue(out result),
                    PositiveInfinitySymbol => new IonFloat(
                        double.PositiveInfinity,
                        annotations)
                        .PassWithValue(out result),
                    NegativeInfinitySymbol => new IonFloat(
                        double.NegativeInfinity,
                        annotations)
                        .PassWithValue(out result),
                    ScientificSymbol => new IonFloat(
                        double.Parse(floatToken.TokenValue()),
                        annotations)
                        .PassWithValue(out result),
                    _ => $"Invalid ion bool token: {tokenNode.FirstNode().SymbolName}".FailWithMessage(out result)
                };
            }
            catch (Exception e)
            {
                result = Result.Of<IonFloat>(e);
                return false;
            }
        }
    }
}
