using Axis.Ion.Types;
using Axis.Luna.Common.Results;
using Axis.Pulsar.Grammar.CST;
using System;
using System.Globalization;

namespace Axis.Ion.IO.Text.Serializers
{
    public class IonDecimalSerializer : IIonTextSerializer<IonDecimal>
    {
        public static string GrammarSymbol => IonDecimalSymbol;

        #region Symbols
        public const string IonDecimalSymbol = "ion-decimal";
        public const string NullDecimalSymbol = "null-decimal";
        public const string ScientificDecimalSymbol = "scientific-decimal";
        public const string RegularDecimalSymbol = "regular-decimal";
        #endregion

        public static IResult<IonDecimal> Parse(CSTNode symbolNode)
        {
            ArgumentNullException.ThrowIfNull(symbolNode);

            try
            {
                if (!IonDecimalSymbol.Equals(symbolNode.SymbolName))
                    return Result.Of<IonDecimal>(new ArgumentException(
                        $"Invalid symbol name: '{symbolNode.SymbolName}'. "
                        + $"Expected '{IonDecimalSymbol}'"));

                (var annotations, var decimalToken) = IonTextSerializer.DeconstructAnnotations(symbolNode);

                return decimalToken.SymbolName switch
                {
                    NullDecimalSymbol => Result.Of(new IonDecimal(null, annotations)),
                    ScientificDecimalSymbol
                    or RegularDecimalSymbol => Result.Of(
                        new IonDecimal(
                            annotations: annotations,
                            value: decimal.Parse(
                                style: NumberStyles.Float,
                                s: decimalToken
                                    .TokenValue()
                                    .ToUpper()
                                    .Replace("D", "E")
                                    .Replace("_", "")))),

                    _ => Result.Of<IonDecimal>(new ArgumentException(
                        $"Invalid symbol encountered: '{decimalToken.SymbolName}'. "
                        + $"Expected '{NullDecimalSymbol}', '{RegularDecimalSymbol}', etc"))
                };
            }
            catch (Exception e)
            {
                return Result.Of<IonDecimal>(e);
            }
        }

        public static string Serialize(IonDecimal value) => Serialize(value, new SerializingContext());

        public static string Serialize(IonDecimal value, SerializingContext context)
        {
            var text = value.Value switch
            {
                null => "null.decimal",
                _ => context.Options.Decimals.UseExponentNotation switch
                {
                    false => MakeDecimal(value.Value.Value.ToString()),
                    true => value.Value!.Value.ToString(),
                }
            };

            var annotationText = IonAnnotationSerializer.Serialize(value.Annotations, context);
            return $"{annotationText}{text}";
        }

        private static string MakeDecimal(string value)
        {
            if (!value.Contains('.'))
                return $"{value}.0";

            return value;
        }
    }
}
