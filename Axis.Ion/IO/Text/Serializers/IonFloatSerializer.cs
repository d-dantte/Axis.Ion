using Axis.Ion.Types;
using Axis.Luna.Common.Results;
using Axis.Pulsar.Grammar.CST;
using System;

namespace Axis.Ion.IO.Text.Serializers
{
    public class IonFloatSerializer : IIonTextSerializer<IonFloat>
    {
        public static string GrammarSymbol => IonFloatSymbol;

        #region Symbols
        public const string IonFloatSymbol = "ion-float";
        public const string NullFloatSymbol = "null-float";
        public const string NANSymbol = "nan";
        public const string PositiveInfinitySymbol = "pinf";
        public const string NegativeInfinitySymbol = "ninf";
        public const string ScientificSymbol = "scientific-float";
        #endregion

        public static IResult<IonFloat> Parse(CSTNode symbolNode)
        {
            ArgumentNullException.ThrowIfNull(symbolNode);

            try
            {
                if (!IonFloatSymbol.Equals(symbolNode.SymbolName))
                    return Result.Of<IonFloat>(new ArgumentException(
                        $"Invalid symbol name: '{symbolNode.SymbolName}'. "
                        + $"Expected '{IonFloatSymbol}'"));

                var annotationToken = symbolNode.FirstNode();
                (var annotations, var floatToken) = IonTextSerializer.DeconstructAnnotations(symbolNode);

                return floatToken.SymbolName switch
                {
                    NullFloatSymbol => Result.Of(new IonFloat(null, annotations)),
                    NANSymbol => Result.Of(new IonFloat(double.NaN, annotations)),
                    PositiveInfinitySymbol => Result.Of(new IonFloat(double.PositiveInfinity, annotations)),
                    NegativeInfinitySymbol => Result.Of(new IonFloat(double.NegativeInfinity, annotations)),

                    ScientificSymbol => Result.Of(
                        new IonFloat(
                                double.Parse(floatToken.TokenValue().Replace("_", "")),
                                annotations)),

                    _ => Result.Of<IonFloat>(new ArgumentException(
                        $"Invalid symbol encountered: '{floatToken.SymbolName}'. "
                        + $"Expected '{NullFloatSymbol}', '{NANSymbol}', etc"))
                };
            }
            catch (Exception e)
            {
                return Result.Of<IonFloat>(e);
            }
        }

        public static string Serialize(IonFloat value) => Serialize(value, new SerializingContext());

        public static string Serialize(IonFloat value, SerializingContext context)
        {
            var text = value.Value switch
            {
                null => "null.float",
                double.NaN => "nan",
                double.PositiveInfinity => "+inf",
                double.NegativeInfinity => "-inf",
                _ => value.Value.Value.ToExponentNotation()
            };

            var annotationText = IonAnnotationSerializer.Serialize(value.Annotations, context);
            return $"{annotationText}{text}";
        }
    }
}
