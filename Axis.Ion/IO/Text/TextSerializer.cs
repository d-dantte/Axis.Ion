using Axis.Ion.IO.Text.Streamers;
using Axis.Ion.Types;
using Axis.Luna.Common;
using Axis.Pulsar.Grammar.CST;
using Axis.Pulsar.Grammar.Language;
using Axis.Pulsar.Grammar.Language.Rules.CustomTerminals;
using Axis.Pulsar.Languages.xBNF;
using System;
using System.IO;
using static Axis.Pulsar.Grammar.Language.Rules.CustomTerminals.DelimitedString;

namespace Axis.Ion.IO.Text
{
    public class TextSerializer : IIonSerializer
    {
        public const string IonValueSymbol = "ion-value";
        public const string IonNullSymbol = "ion-null";
        public const string IonBoolSymbol = "ion-bool";
        public const string IonIntSymbol = "ion-int";
        public const string IonFloatSymbol = "ion-float";
        public const string IonDecimalSymbol = "ion-decimal";
        public const string IonTimestampSymbol = "ion-timestamp";
        public const string IonStringSymbol = "ion-string";
        public const string IonSymbolSymbol = "ion-symbol";
        public const string IonOperatorSymbol = "operator-symbol";
        public const string IonQuotedSymbol = "quoted-symbol";
        public const string IonIdentifierSymbol = "identifier";
        public const string IonBlobSymbol = "ion-blob";
        public const string IonClobSymbol = "ion-clob";
        public const string IonListSymbol = "ion-list";
        public const string IonSexpSymbol = "ion-sexp";
        public const string IonStructSymbol = "ion-struct";

        internal static readonly Grammar IonGrammar;

        private static readonly string MultilineSingleQuoteDelimitedString = "Multiline-3SQDString";
        private static readonly string SinglelineSingleQuoteDelimitedString = "Singleline-SQDString";
        private static readonly string SinglelineDoubleQuoteDelimitedString = "Singleline-DQDString";

        static TextSerializer()
        {
            using var ionXbnfStream = typeof(TextSerializer).Assembly
                .GetManifestResourceStream($"{typeof(TextSerializer).Namespace}.IonGrammar.xbnf");

            var importer = new Importer();

            // register multiline-3sqdstring
            _ = importer.RegisterTerminal(
                new DelimitedString(
                    MultilineSingleQuoteDelimitedString,
                    "'''",
                    new BSolGeneralEscapeMatcher()));

            // register singleline-sqdstring
            _ = importer.RegisterTerminal(
                new DelimitedString(
                    SinglelineSingleQuoteDelimitedString,
                    "\'",
                    new[] { "\n", "\r" },
                    new BSolGeneralEscapeMatcher()));

            // register singleline-dqdstring
            _ = importer.RegisterTerminal(
                new DelimitedString(
                    SinglelineDoubleQuoteDelimitedString,
                    "\"",
                    new[] { "\n", "\r" },
                    new BSolGeneralEscapeMatcher()));

            IonGrammar = importer.ImportGrammar(ionXbnfStream);
        }

        public SerializerOptions SerializerOptions { get; }

        public TextSerializer(SerializerOptions options)
        {
            SerializerOptions = options ?? new SerializerOptions();
        }

        public IonPacket Deserialize(Stream ionStream)
        {
            throw new NotImplementedException();
        }

        public byte[] Serialize(IonPacket ionPacket)
        {
            throw new NotImplementedException();
        }


        internal static string StreamText(IIonType ionValue, StreamingContext context)
        {
            return ionValue switch
            {
                IonNull @null => new IonNullStreamer().StreamText(@null, context),
                IonBool @bool => new IonBoolStreamer().StreamText(@bool, context),
                IonInt @int => new IonIntStreamer().StreamText(@int, context),
                IonFloat @float => new IonFloatStreamer().StreamText(@float, context),
                IonDecimal @decimal => new IonDecimalStreamer().StreamText(@decimal, context),
                IonTimestamp timestamp => new IonTimestampStreamer().StreamText(timestamp, context),
                IonString @string => new IonStringStreamer().StreamText(@string, context),
                IonQuotedSymbol quoted => new IonQuotedSymbolStreamer().StreamText(quoted, context),
                IonIdentifier identifier => new IonIdentifierStreamer().StreamText(identifier, context),
                IonOperator @operator => new IonOperatorStreamer().StreamText(@operator, context),
                IonBlob blob => new IonBlobStreamer().StreamText(blob, context),
                IonClob clob => new IonClobStreamer().StreamText(clob, context),
                IonList list => new IonListStreamer().StreamText(list, context),
                IonSexp sexp => new IonSexpStreamer().StreamText(sexp, context),
                IonStruct @struct => new IonStructStreamer().StreamText(@struct, context),
                _ => throw new ArgumentException($"Invalid ion value: {ionValue}")
            };
        }

        internal static bool TryParse(string ionText, out IResult<IIonType> result)
        {
            var recognition = IonGrammar
                .GetRecognizer(IonValueSymbol)
                .Recognize(ionText);

            return recognition.TryParseRecognition(TryParse, out result);
        }

        internal static bool TryParse(CSTNode node, out IResult<IIonType> result)
        {
            var ionToken = node.FirstNode();
            result = ionToken.SymbolName switch
            {
                IonNullSymbol => Parse<IonNull>(ionToken),
                IonBoolSymbol => Parse<IonBool>(ionToken),
                IonIntSymbol => Parse<IonInt>(ionToken),
                IonFloatSymbol => Parse<IonFloat>(ionToken),
                IonDecimalSymbol => Parse<IonDecimal>(ionToken),
                IonTimestampSymbol => Parse<IonTimestamp>(ionToken),
                IonStringSymbol => Parse<IonString>(ionToken),
                IonBlobSymbol => Parse<IonBlob>(ionToken),
                IonClobSymbol => Parse<IonClob>(ionToken),
                IonListSymbol => Parse<IonList>(ionToken),
                IonSexpSymbol => Parse<IonSexp>(ionToken),
                IonStructSymbol => Parse<IonStruct>(ionToken),
                _ => SymbolToken(ionToken, out var symbolToken) switch
                {
                    IonQuotedSymbol => Parse<IonQuotedSymbol>(symbolToken),
                    IonIdentifierSymbol => Parse<IonIdentifier>(symbolToken),
                    IonOperatorSymbol => Parse<IonOperator>(symbolToken),
                    _ => throw new ArgumentException($"Invalid ion token: {ionToken?.SymbolName ?? ("null")}")
                }
            };

            return result is IResult<IIonType>.DataResult;
        }

        internal static IIonType Parse(string ionText)
        {
            _ = TryParse(ionText, out var result);
            return result.Resolve();
        }

        internal static IIonType Parse(CSTNode node)
        {
            _ = TryParse(node, out var result);
            return result.Resolve();
        }

        private static IResult<IIonType> Parse<TIonType>(CSTNode? node)
        {
            if (node is null)
                return Result.Of<IIonType>(new ArgumentNullException(nameof(node)));

            if (typeof(TIonType).Equals(typeof(IonNull)))
            {
                _ = new IonNullStreamer().TryParse(node, out var result);
                return result.Map<IIonType>(v => v);
            }

            if (typeof(TIonType).Equals(typeof(IonBool)))
            {
                _ = new IonBoolStreamer().TryParse(node, out var result);
                return result.Map<IIonType>(v => v);
            }

            if (typeof(TIonType).Equals(typeof(IonInt)))
            {
                _ = new IonIntStreamer().TryParse(node, out var result);
                return result.Map<IIonType>(v => v);
            }

            if (typeof(TIonType).Equals(typeof(IonFloat)))
            {
                _ = new IonFloatStreamer().TryParse(node, out var result);
                return result.Map<IIonType>(v => v);
            }

            if (typeof(TIonType).Equals(typeof(IonDecimal)))
            {
                _ = new IonDecimalStreamer().TryParse(node, out var result);
                return result.Map<IIonType>(v => v);
            }

            if (typeof(TIonType).Equals(typeof(IonTimestamp)))
            {
                _ = new IonTimestampStreamer().TryParse(node, out var result);
                return result.Map<IIonType>(v => v);
            }

            if (typeof(TIonType).Equals(typeof(IonString)))
            {
                _ = new IonStringStreamer().TryParse(node, out var result);
                return result.Map<IIonType>(v => v);
            }

            if (typeof(TIonType).Equals(typeof(IonOperator)))
            {
                _ = new IonOperatorStreamer().TryParse(node, out var result);
                return result.Map<IIonType>(v => v);
            }

            if (typeof(TIonType).Equals(typeof(IonIdentifier)))
            {
                _ = new IonIdentifierStreamer().TryParse(node, out var result);
                return result.Map<IIonType>(v => v);
            }

            if (typeof(TIonType).Equals(typeof(IonQuotedSymbol)))
            {
                _ = new IonQuotedSymbolStreamer().TryParse(node, out var result);
                return result.Map<IIonType>(v => v);
            }

            if (typeof(TIonType).Equals(typeof(IonBlob)))
            {
                _ = new IonBlobStreamer().TryParse(node, out var result);
                return result.Map<IIonType>(v => v);
            }

            if (typeof(TIonType).Equals(typeof(IonClob)))
            {
                _ = new IonClobStreamer().TryParse(node, out var result);
                return result.Map<IIonType>(v => v);
            }

            if (typeof(TIonType).Equals(typeof(IonList)))
            {
                _ = new IonListStreamer().TryParse(node, out var result);
                return result.Map<IIonType>(v => v);
            }

            if (typeof(TIonType).Equals(typeof(IonSexp)))
            {
                _ = new IonSexpStreamer().TryParse(node, out var result);
                return result.Map<IIonType>(v => v);
            }

            if (typeof(TIonType).Equals(typeof(IonStruct)))
            {
                _ = new IonStructStreamer().TryParse(node, out var result);
                return result.Map<IIonType>(v => v);
            }

            else return Result.Of<IIonType>(new ArgumentException($"Invalid ion type: {typeof(TIonType)}"));
        }

        private static string? SymbolToken(CSTNode ionToken, out CSTNode? symbolToken)
        {
            symbolToken = null;

            if (ionToken is null)
                return null;

            symbolToken = ionToken.FindNode($"{IonOperatorSymbol}|{IonIdentifierSymbol}|{IonQuotedSymbol}");
            return symbolToken?.SymbolName;
        }
    }
}
