using Axis.Ion.IO.Text.Pulsar.CustomTerminals;
using Axis.Pulsar.Grammar.Language;
using Axis.Pulsar.Grammar.Language.Rules.CustomTerminals;
using Axis.Pulsar.Grammar.Recognizers;
using System.Linq;
using System.Text.RegularExpressions;

namespace Axis.Ion.IO.Text.Pulsar.CustomRules
{
    public record IonIdentifierRule : ICustomTerminal
    {
        public static readonly Regex IdentifierPattern = new Regex(
            "^[a-zA-Z_\\$][a-zA-Z0-9_\\$]*\\z",
            RegexOptions.Compiled);

        public static string[] KeyWords => _keyWords.ToArray();

        private static readonly string[] _keyWords = new string[]
        {
            "null", "true", "false", "nan"
        };

        public string SymbolName => "ion-identifier";

        public IRecognizer ToRecognizer(Grammar grammar) => new IonIdentifierRecognizer(grammar, this);
    }
}
