using Axis.Luna.Extensions;
using Axis.Pulsar.Grammar.CST;
using Axis.Pulsar.Grammar.Language.Rules;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Axis.Ion.IO.Text.Pulsar.Validators
{
    [Obsolete]
    public class ClobSymbolValidator : IProductionValidator
    {
        private const string Clob = "clob-value";
        private const string SingleLineString = "singleline-string";
        private const string MultilineString = "clob-multiline-string";
        private const string MLString = "ml-string";

        private static readonly Regex B1HexPattern = new(@"^\\x[a-fA-F0-9]{2}$", RegexOptions.Compiled);
        private static readonly Regex B2HexPattern = new(@"^\\u[a-fA-F0-9]{4}$", RegexOptions.Compiled);
        private static readonly Regex B4HexPattern = new(@"^\\U[a-fA-F0-9]{8}$", RegexOptions.Compiled);

        public ProductionValidationResult ValidateCSTNode(ProductionRule rule, CSTNode node)
        {
            if (!Clob.Equals(rule.SymbolName))
                return new ProductionValidationResult.Error(
                    "Symbol name mismatch: "
                    + $"expected: {Clob}, "
                    + $"rule: {rule.SymbolName}, "
                    + $"node: {node.SymbolName}");

            return node
                .FindNode($"{SingleLineString}|{MultilineString}")
                .ApplyTo(stringType => stringType.SymbolName switch
                {
                    SingleLineString => ValidateSingleLineString(stringType),
                    MultilineString => ValidateMultiLineString(stringType),
                    _ => new ProductionValidationResult.Error(
                        $"Invalid clob string type encountered: {stringType.SymbolName}")
                });
        }

        private static ProductionValidationResult ValidateSingleLineString(CSTNode node)
        {
            return node
                .TokenValue()
                .ApplyTo(ValidateTokens);
        }

        private static ProductionValidationResult ValidateMultiLineString(CSTNode node)
        {
            return node
                .FindNodes(MLString)
                .Select(node => node.TokenValue())
                .JoinUsing("")
                .ApplyTo(ValidateTokens);
        }

        private static ProductionValidationResult ValidateTokens(string tokens)
        {
            return 
                tokens.Any(c => c > 0x7f || c < 0x20)
                || B4HexPattern.IsMatch(tokens)
                || B2HexPattern.IsMatch(tokens)
                || B1HexPattern.IsMatch(tokens)
                ? new ProductionValidationResult.Error("Non-Ascii characters found in the tokens")
                : new ProductionValidationResult.Success();
        }
    }
}
