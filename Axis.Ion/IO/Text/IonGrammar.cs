using Axis.Ion.IO.Text.Pulsar.CustomRules;
using Axis.Pulsar.Grammar.Language;
using Axis.Pulsar.Grammar.Language.Rules.CustomTerminals;
using Axis.Pulsar.Languages.xBNF;
using System;
using System.Linq;
using Axis.Ion.IO.Text.Pulsar.EscapeMatchers;
using Axis.Ion.IO.Text.Pulsar.Validators;

namespace Axis.Ion.IO.Text
{
    internal static class IonGrammar
    {
        #region Custom Terminals
        private static readonly string MultilineSingleQuoteDelimitedString = "Multiline-3SQDString";
        private static readonly string SinglelineDoubleQuoteDelimitedString = "Singleline-DQDString";
        private static readonly string SinglelineSingleQuoteDelimitedString = "Singleline-SQDString";
        private static readonly string ClobMultilineSingleQuoteDelimitedString = "Clob-Multiline-3SQDString";
        private static readonly string ClobSinglelineDoubleQuoteDelimitedString = "Clob-Singleline-DQDString";
        private static readonly string BlockCommentDelimitedString = "BlockCommentString";
        private static readonly string Blob = "blob-value";
        private static readonly string BlobText = "blob-text-value";
        //private static readonly string Clob = "clob-value";
        #endregion

        public static Grammar Grammar { get; }

        static IonGrammar()
        {
            using var ionXbnfStream = typeof(IonGrammar).Assembly
                .GetManifestResourceStream($"{typeof(IonGrammar).Namespace}.IonGrammar.xbnf");

            var importer = new Importer();

            #region Register Custom Terminals
            // register multiline-3sqdstring
            _ = importer.RegisterTerminal(
                new DelimitedString(
                    MultilineSingleQuoteDelimitedString,
                    "'''",
                    new IonStringEscapeMatcher()));

            // register singleline-sqdstring
            _ = importer.RegisterTerminal(
                new DelimitedString(
                    SinglelineSingleQuoteDelimitedString,
                    "\'",
                    new[] { "\n", "\r" },
                    new IonStringEscapeMatcher()));

            // register singleline-dqdstring
            _ = importer.RegisterTerminal(
                new DelimitedString(
                    SinglelineDoubleQuoteDelimitedString,
                    "\"",
                    new[] { "\n", "\r" },
                    new IonStringEscapeMatcher()));

            // register clob-multiline-3sqdstring
            _ = importer.RegisterTerminal(
                new DelimitedString(
                    ClobMultilineSingleQuoteDelimitedString,
                    "'''",
                    ClobMultilineStringLegalCharacters(),
                    Array.Empty<string>(),
                    new IonClobEscapeMatcher()));

            // register clob-singleline-dqdstring
            _ = importer.RegisterTerminal(
                new DelimitedString(
                    ClobSinglelineDoubleQuoteDelimitedString,
                    "\"",
                    ClobSinglelineStringLegalCharacters(),
                    Array.Empty<string>(),
                    new IonClobEscapeMatcher()));

            // register blob string
            _ = importer.RegisterTerminal(
                new DelimitedString(
                    Blob,
                    "{{",
                    "}}",
                    Base64LegalCharacters(),
                    Array.Empty<string>()));

            // register block comment
            _ = importer.RegisterTerminal(
                new DelimitedString(
                    BlockCommentDelimitedString,
                    "/*",
                    "*/"));

            // register identifier rule
            _ = importer.RegisterTerminal(new IonIdentifierRule());
            #endregion

            #region Register Validators

            // register blob validator
            _ = importer.RegisterValidator(BlobText, new BlobSymbolValidator());

            #endregion

            Grammar = importer.ImportGrammar(ionXbnfStream);
        }


        private static char[] CharRange(char start, char end)
        {
            if (end < start)
                throw new ArgumentOutOfRangeException(nameof(end));

            return Enumerable
                .Range(start, (end - start) + 1)
                .Select(c => (char)c)
                .ToArray();
        }

        private static string[] Base64LegalCharacters()
        {
            return IonGrammar
                .CharRange('A', 'Z')
                .Concat(CharRange('a', 'z'))
                .Concat(CharRange('0', '9'))
                .Append('+')
                .Append('=')
                .Append('/')
                .Append(' ')
                .Append('\t')
                .Append('\n')
                .Append('\r')
                .Select(c => c.ToString())
                .ToArray();
        }

        internal static string[] ClobSinglelineStringLegalCharacters()
        {
            return new[] { '\u0009', '\u000B', '\u000C' }
                .Concat(CharRange('\u0020', '\u0021'))
                .Concat(CharRange('\u0023', '\u005B'))
                .Concat(CharRange('\u005D', '\u007F'))
                .Select(c => c.ToString())
                .ToArray();
        }

        private static string[] ClobMultilineStringLegalCharacters()
        {
            return IonGrammar
                .CharRange('\u0009', '\u000D')
                .Concat(CharRange('\u0020', '\u005B'))
                .Concat(CharRange('\u005D', '\u007F'))
                .Select(c => c.ToString())
                .ToArray();
        }
    }
}
