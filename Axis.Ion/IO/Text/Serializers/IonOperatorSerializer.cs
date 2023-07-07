using Axis.Ion.Types;
using Axis.Luna.Common.Results;
using Axis.Luna.Extensions;
using Axis.Pulsar.Grammar.CST;
using System;

namespace Axis.Ion.IO.Text.Serializers
{
    public class IonOperatorSerializer : IIonTextSerializer<IonOperator>
    {
        public static string GrammarSymbol => IonSymbol;

        #region Symbols
        public const string IonSymbol = "ion-symbol";
        public const string NullSymbol = "null-symbol";
        public const string OperatorSymbol = "operator-symbol";
        #endregion

        public static IResult<IonOperator> Parse(CSTNode symbolNode)
        {
            ArgumentNullException.ThrowIfNull(symbolNode);

            try
            {
                if (!IonSymbol.Equals(symbolNode.SymbolName))
                    return Result.Of<IonOperator>(new ArgumentException(
                        $"Invalid symbol name: '{symbolNode.SymbolName}'. "
                        + $"Expected '{IonSymbol}'"));

                (var annotations, var symbolTokens) = IonTextSerializer.DeconstructAnnotations(symbolNode);

                return symbolTokens.SymbolName switch
                {
                    NullSymbol => Result.Of(new IonOperator(null, annotations)),

                    OperatorSymbol => symbolTokens
                        .TokenValue()
                        .ApplyTo(@string => IonOperator.Parse(@string, annotations)),

                    _ => Result.Of<IonOperator>(new ArgumentException(
                        $"Invalid symbol encountered: '{symbolTokens.SymbolName}'. "
                        + $"Expected '{NullSymbol}', or '{OperatorSymbol}'."))
                };
            }
            catch (Exception e)
            {
                return Result.Of<IonOperator>(e);
            }
        }

        public static string Serialize(IonOperator value) => Serialize(value, new SerializingContext());

        public static string Serialize(
            IonOperator value,
            SerializingContext context)
            => value.ToString();
    }
}
