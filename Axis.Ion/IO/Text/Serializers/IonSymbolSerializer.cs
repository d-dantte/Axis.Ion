using Axis.Ion.Types;
using Axis.Luna.Common.Results;
using Axis.Pulsar.Grammar.CST;
using System;
using static System.Net.Mime.MediaTypeNames;

namespace Axis.Ion.IO.Text.Serializers
{
    public class IonSymbolSerializer : IIonTextSerializer<IIonValue>
    {
        public static string GrammarSymbol => IonSymbol;

        #region Symbols
        public const string IonSymbol = "ion-symbol";
        public const string NullSymbol = "null-symbol";
        public const string QuotedSymbol = "quoted-symbol";
        public const string IdentifierSymbol = "identifier";
        #endregion

        public static IResult<IIonValue> Parse(CSTNode symbolNode)
        {
            ArgumentNullException.ThrowIfNull(symbolNode);

            try
            {
                if (!IonSymbol.Equals(symbolNode.SymbolName))
                    return Result.Of<IIonValue>(new ArgumentException(
                        $"Invalid symbol name: '{symbolNode.SymbolName}'. "
                        + $"Expected '{IonSymbol}'"));

                (var annotations, var symbolTokens) = IonTextSerializer.DeconstructAnnotations(symbolNode);

                return symbolTokens.SymbolName switch
                {
                    NullSymbol => Result
                        .Of(new IonTextSymbol(null, annotations))
                        .Map(value => (IIonValue)value),

                    QuotedSymbol
                    or IdentifierSymbol => IonTextSymbolSerializer
                        .Parse(symbolNode)
                        .Map(symbol => (IIonValue)symbol),

                    _ => Result.Of<IIonValue>(new ArgumentException(
                        $"Invalid symbol encountered: '{symbolTokens.SymbolName}'. "
                        + $"Expected '{NullSymbol}', '{IdentifierSymbol}', etc"))
                };
            }
            catch (Exception e)
            {
                return Result.Of<IIonValue>(e);
            }
        }

        public static string Serialize(IIonValue value) => Serialize(value, new SerializingContext());

        public static string Serialize(IIonValue value, SerializingContext context)
        {
            var text = value switch
            {
                IonTextSymbol textSymbol => IonTextSymbolSerializer.Serialize(textSymbol, context),
                IonOperator ionOperator => IonOperatorSerializer.Serialize(ionOperator, context),
                _ => throw new ArgumentException($"Invalid value type: '{value?.GetType()}'")
            };

            var annotationText = IonAnnotationSerializer.Serialize(value.Annotations, context);
            return $"{annotationText}{text}";
        }
    }
}
