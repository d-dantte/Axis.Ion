using Axis.Ion.Types;
using Axis.Luna.Common;
using Axis.Luna.Extensions;
using Axis.Pulsar.Grammar.CST;
using System;
using System.Linq;

namespace Axis.Ion.IO.Text.Streamers
{
    /// <summary>
    /// Operator symbol streamer
    /// </summary>
    public class IonOperatorStreamer : AbstractIonTypeStreamer<IonOperator>
    {
        public const string IonSymbol = "ion-symbol";
        public const string NullSymbol = "null-symbol";
        public const string OperatorSymbol = "operator-symbol";

        protected override string IonValueSymbolName => IonSymbol;

        public override string GetIonValueText(IonOperator ionValue, StreamingContext context)
        {
            return ionValue.Value switch
            {
                null => "null.symbol",
                _ => ionValue.Value
                    .Select(op => (char)op)
                    .ToArray()
                    .ApplyTo(charArray => new string(charArray))
            };
        }

        public override bool TryParse(CSTNode tokenNode, out IResult<IonOperator> result)
        {
            if (tokenNode is null)
                throw new ArgumentNullException(nameof(tokenNode));

            try
            {
                if (!IonSymbol.Equals(tokenNode.SymbolName))
                    return $"Invalid token node name: {tokenNode.SymbolName}".FailWithMessage(out result);

                var annotationToken = tokenNode.FirstNode();
                (var annotations, var operatorToken) = DeconstructAnnotations(tokenNode);

                return operatorToken.SymbolName switch
                {
                    NullSymbol => new IonOperator(null, annotations).PassWithValue(out result),

                    OperatorSymbol => operatorToken
                        .TokenValue()
                        .ApplyTo(@string => IonOperator.Parse(@string, annotations))
                        .PassWithValue(out result),

                    _ => $"Invalid ion bool token: {tokenNode.FirstNode().SymbolName}".FailWithMessage(out result)
                };
            }
            catch (Exception e)
            {
                result = Result.Of<IonOperator>(e);
                return false;
            }
        }
    }

    /// <summary>
    /// Ion Identifier streamer
    /// </summary>
    public class IonIdentifierStreamer : AbstractIonTypeStreamer<IonIdentifier>
    {
        public const string IonSymbol = "ion-symbol";
        public const string NullSymbol = "null-symbol";
        public const string IdentifierSymbol = "identifier";

        protected override string IonValueSymbolName => IonSymbol;

        public override string GetIonValueText(IonIdentifier ionValue, StreamingContext context)
        {
            return ionValue.Value switch
            {
                null => "null.symbol",
                _ => ionValue.Value
                    .Select(op => (char)op)
                    .ToArray()
                    .ApplyTo(charArray => new string(charArray))
            };
        }

        public override bool TryParse(CSTNode tokenNode, out IResult<IonIdentifier> result)
        {
            if (tokenNode is null)
                throw new ArgumentNullException(nameof(tokenNode));

            try
            {
                if (!IonSymbol.Equals(tokenNode.SymbolName))
                    return $"Invalid token node name: {tokenNode.SymbolName}".FailWithMessage(out result);

                var annotationToken = tokenNode.FirstNode();
                (var annotations, var identifierToken) = DeconstructAnnotations(tokenNode);

                return identifierToken.SymbolName switch
                {
                    NullSymbol => new IonIdentifier(null, annotations).PassWithValue(out result),

                    IdentifierSymbol => identifierToken
                        .TokenValue()
                        .ApplyTo(@string => IonIdentifier.Parse(@string, annotations))
                        .PassWithValue(out result),

                    _ => $"Invalid ion bool token: {tokenNode.FirstNode().SymbolName}".FailWithMessage(out result)
                };
            }
            catch (Exception e)
            {
                result = Result.Of<IonIdentifier>(e);
                return false;
            }
        }
    }

    /// <summary>
    /// Ion Quoted symbol streamer
    /// </summary>
    public class IonQuotedSymbolStreamer : AbstractIonTypeStreamer<IonQuotedSymbol>
    {
        public const string IonSymbol = "ion-symbol";
        public const string NullSymbol = "null-symbol";
        public const string QuotedSymbol = "quoted-symbol";

        protected override string IonValueSymbolName => IonSymbol;

        public override string GetIonValueText(IonQuotedSymbol ionValue, StreamingContext context)
        {
            return ionValue.Value switch
            {
                null => "null.symbol",
                _ => $"'{ionValue.Value}'"
            };
        }

        public override bool TryParse(CSTNode tokenNode, out IResult<IonQuotedSymbol> result)
        {
            if (tokenNode is null)
                throw new ArgumentNullException(nameof(tokenNode));

            try
            {
                if (!IonSymbol.Equals(tokenNode.SymbolName))
                    return $"Invalid token node name: {tokenNode.SymbolName}".FailWithMessage(out result);

                var annotationToken = tokenNode.FirstNode();
                (var annotations, var quoteToken) = DeconstructAnnotations(tokenNode);

                return quoteToken.SymbolName switch
                {
                    NullSymbol => new IonQuotedSymbol(null, annotations).PassWithValue(out result),

                    QuotedSymbol => quoteToken
                        .TokenValue()
                        .ApplyTo(@string => IonQuotedSymbol.Parse(@string, annotations))
                        .PassWithValue(out result),

                    _ => $"Invalid ion bool token: {tokenNode.FirstNode().SymbolName}".FailWithMessage(out result)
                };
            }
            catch (Exception e)
            {
                result = Result.Of<IonQuotedSymbol>(e);
                return false;
            }
        }
    }
}
