using Axis.Ion.Types;
using Axis.Luna.Common.Results;
using Axis.Luna.Extensions;
using Axis.Pulsar.Grammar.CST;
using System;

namespace Axis.Ion.IO.Text.Serializers
{
    public class IonTextSymbolSerializer : IIonTextSerializer<IonTextSymbol>
    {
        public static string GrammarSymbol => IonSymbol;

        #region Symbols
        public const string IonSymbol = "ion-symbol";
        public const string NullSymbol = "null-symbol";
        public const string QuotedSymbol = "quoted-symbol";
        public const string IdentifierSymbol = "identifier";
        #endregion

        public static IResult<IonTextSymbol> Parse(CSTNode symbolNode)
        {
            ArgumentNullException.ThrowIfNull(symbolNode);

            try
            {
                if (!IonSymbol.Equals(symbolNode.SymbolName))
                    return Result.Of<IonTextSymbol>(new ArgumentException(
                        $"Invalid symbol name: '{symbolNode.SymbolName}'. "
                        + $"Expected '{IonSymbol}'"));

                (var annotations, var symbolTokens) = IonTextSerializer.DeconstructAnnotations(symbolNode);

                return symbolTokens.SymbolName switch
                {
                    NullSymbol => Result.Of(new IonTextSymbol(null, annotations)),

                    QuotedSymbol
                    or IdentifierSymbol => symbolTokens
                        .TokenValue()
                        .ApplyTo(ProcessEscapes)
                        .ApplyTo(@string => IonTextSymbol.Parse(@string, annotations)),

                    _ => Result.Of<IonTextSymbol>(new ArgumentException(
                        $"Invalid symbol encountered: '{symbolTokens.SymbolName}'. "
                        + $"Expected '{NullSymbol}', '{IdentifierSymbol}', etc"))
                };
            }
            catch (Exception e)
            {
                return Result.Of<IonTextSymbol>(e);
            }
        }

        public static string Serialize(IonTextSymbol value) => Serialize(value, new SerializingContext());

        public static string Serialize(
            IonTextSymbol value,
            SerializingContext context)
            => value.ToString();

        public static string ProcessEscapes(string escapeSequence)
        {
            throw new NotImplementedException();
        }
    }
}
