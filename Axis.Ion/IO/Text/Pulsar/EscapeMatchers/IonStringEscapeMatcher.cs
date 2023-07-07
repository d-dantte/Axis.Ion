using Axis.Pulsar.Grammar;
using System.Text.RegularExpressions;
using static Axis.Pulsar.Grammar.Language.Rules.CustomTerminals.DelimitedString;

namespace Axis.Ion.IO.Text.Pulsar.EscapeMatchers
{
    public class IonStringEscapeMatcher : IEscapeSequenceMatcher
    {
        private static readonly Regex B1HexPattern = new(@"^x[a-fA-F0-9]{2}$", RegexOptions.Compiled);
        private static readonly Regex B2HexPattern = new(@"^u[a-fA-F0-9]{4}$", RegexOptions.Compiled);
        private static readonly Regex B4HexPattern = new(@"^U[a-fA-F0-9]{8}$", RegexOptions.Compiled);
        private static readonly string MSNewLine = "\r\n";

        public string EscapeDelimiter => "\\";

        public bool TryMatchEscapeArgument(BufferedTokenReader reader, out char[] tokens)
        {
            var position = reader.Position;

            // utf-b4
            if (reader.TryNextTokens(9, out tokens)
                && B4HexPattern.IsMatch(new string(tokens)))
                return true;

            // utf-b2
            if (reader.Reset(position).TryNextTokens(5, out tokens)
                && B2HexPattern.IsMatch(new string(tokens)))
                return true;

            // utf-b1
            if (reader.Reset(position).TryNextTokens(3, out tokens)
                && B1HexPattern.IsMatch(new string(tokens)))
                return true;

            // ms new line
            if (reader.Reset(position).TryNextTokens(2, out tokens)
                && MSNewLine.Equals(new string(tokens)))
                return true;

            // regular
            if (reader.Reset(position).TryNextTokens(1, out tokens)
                && (IsBasicEscapeArg(tokens[0])
                || "\n".Equals(new string(tokens))
                || "\r".Equals(new string(tokens))))
                return true;

            reader.Reset(position);
            return false;
        }

        private static bool IsBasicEscapeArg(char ch) => ch switch
        {
            '\'' => true,
            '\"' => true,
            '\\' => true,
            '/' => true,
            '?' => true,
            'n' => true,
            'r' => true,
            'f' => true,
            'b' => true,
            't' => true,
            'v' => true,
            '0' => true,
            'a' => true,
            _ => false
        };
    }
}
