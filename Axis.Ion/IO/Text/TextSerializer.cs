using Axis.Ion.IO.Text.Streamers;
using Axis.Ion.Types;
using Axis.Luna.Common;
using Axis.Luna.Extensions;
using Axis.Pulsar.Grammar;
using Axis.Pulsar.Grammar.CST;
using Axis.Pulsar.Grammar.Language;
using Axis.Pulsar.Grammar.Language.Rules.CustomTerminals;
using Axis.Pulsar.Languages.xBNF;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static Axis.Pulsar.Grammar.Language.Rules.CustomTerminals.DelimitedString;

namespace Axis.Ion.IO.Text
{
    /// <summary>
    /// Ion Text serializer type.
    /// </summary>
    public class TextSerializer : IIonSerializer
    {
        public const string IonSymbol = "ion";
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

        /// <summary>
        /// Deserializes a stream of <see cref="Encoding.Unicode"/> bytes into an <see cref="IonPacket"/> instance
        /// </summary>
        /// <param name="ionStream">The <see cref="Encoding.Unicode"/> byte stream</param>
        /// <returns>The ion packet</returns>
        public IonPacket Deserialize(Stream ionStream)
        {
            if (ionStream is null)
                throw new ArgumentNullException(nameof(ionStream));

            var bufferedReader = new BufferedTokenReader(ToCharacterEnumerable(ionStream));
            var recognition = IonGrammar
                .GetRecognizer(IonSymbol)
                .Recognize(bufferedReader);

            _ = recognition.TryParseRecognition<IonPacket>(TryParseIonPacket, out var result);
            return result.Resolve();
        }

        /// <summary>
        /// Serializes an ion packet instance into an array of <see cref="Encoding.Unicode"/> encoded bytes
        /// </summary>
        /// <param name="ionPacket">The <see cref="IonPacket"/> instance to serialize</param>
        /// <returns>The byte array</returns>
        public byte[] Serialize(IonPacket ionPacket)
        {
            var context = new SerializingContext(SerializerOptions);
            var ionText = ionPacket.IonValues
                .Select(value => SerializeText(value, context))
                .JoinUsing(Environment.NewLine);

            return Encoding.Unicode.GetBytes(ionText);
        }

        /// <summary>
        /// Serialize the given value into text
        /// </summary>
        /// <param name="ionValue">the ion value</param>
        /// <param name="context">the context to serialize with</param>
        /// <returns>the serialized string</returns>
        /// <exception cref="ArgumentException">if the ion value is invalid</exception>
        internal static string SerializeText(IIonType ionValue, SerializingContext context)
        {
            return ionValue switch
            {
                IonNull @null => new IonNullSerializer().SerializeText(@null, context),
                IonBool @bool => new IonBoolSerializer().SerializeText(@bool, context),
                IonInt @int => new IonIntSerializer().SerializeText(@int, context),
                IonFloat @float => new IonFloatSerializer().SerializeText(@float, context),
                IonDecimal @decimal => new IonDecimalSerializer().SerializeText(@decimal, context),
                IonTimestamp timestamp => new IonTimestampSerializer().SerializeText(timestamp, context),
                IonString @string => new IonStringSerializer().SerializeText(@string, context),
                IonQuotedSymbol quoted => new IonQuotedSymbolSerializer().SerializeText(quoted, context),
                IonIdentifier identifier => new IonIdentifierSerializer().SerializeText(identifier, context),
                IonOperator @operator => new IonOperatorSerializer().SerializeText(@operator, context),
                IonBlob blob => new IonBlobSterializer().SerializeText(blob, context),
                IonClob clob => new IonClobSerializer().SerializeText(clob, context),
                IonList list => new IonListSerializer().SerializeText(list, context),
                IonSexp sexp => new IonSexpSerializer().SerializeText(sexp, context),
                IonStruct @struct => new IonStructSerializer().SerializeText(@struct, context),
                _ => throw new ArgumentException($"Invalid ion value: {ionValue}")
            };
        }

        /// <summary>
        /// Parse the given string into an ion type
        /// </summary>
        /// <param name="ionText">the string</param>
        /// <param name="result">the result</param>
        /// <returns>true if successful, false otherwise</returns>
        internal static bool TryParse(string ionText, out IResult<IIonType> result)
        {
            var recognition = IonGrammar
                .GetRecognizer(IonValueSymbol)
                .Recognize(ionText);

            return recognition.TryParseRecognition(TryParseIonValueToken, out result);
        }

        /// <summary>
        /// Attempts the parse the given <see cref="CSTNode"/>, assumed to originate from the IonGrammar.xbnf spec
        /// </summary>
        /// <param name="ionToken">The cstnode with symbol-name: "ion-value"</param>
        /// <param name="result">The result of the parse operation</param>
        /// <returns>True if successful, false otherwise</returns>
        /// <exception cref="ArgumentException">If the <paramref name="ionToken"/> is problematic</exception>
        internal static bool TryParseIonValueToken(CSTNode ionValueToken, out IResult<IIonType> result)
        {
            var ionToken = ionValueToken.FirstNode();
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

        /// <summary>
        /// Parse the given string, using the IonGrammar.xbnf spec
        /// </summary>
        /// <param name="ionText">The text to parse</param>
        /// <returns>returns the parsed value</returns>
        internal static IIonType Parse(string ionText)
        {
            _ = TryParse(ionText, out var result);
            return result.Resolve();
        }

        /// <summary>
        /// Parse the given node, using the IonGrammar.xbnf spec
        /// </summary>
        /// <param name="ionValue">The cstnode with symbol-name: "ion-value"</param>
        /// <returns>returns the parsed value</returns>
        internal static IIonType ParseIonValueToken(CSTNode ionValue)
        {
            _ = TryParseIonValueToken(ionValue, out var result);
            return result.Resolve();
        }

        private static IResult<IIonType> Parse<TIonType>(CSTNode? node)
        {
            if (node is null)
                return Result.Of<IIonType>(new ArgumentNullException(nameof(node)));

            if (typeof(TIonType).Equals(typeof(IonNull)))
            {
                _ = new IonNullSerializer().TryParse(node, out var result);
                return result.Map<IIonType>(v => v);
            }

            if (typeof(TIonType).Equals(typeof(IonBool)))
            {
                _ = new IonBoolSerializer().TryParse(node, out var result);
                return result.Map<IIonType>(v => v);
            }

            if (typeof(TIonType).Equals(typeof(IonInt)))
            {
                _ = new IonIntSerializer().TryParse(node, out var result);
                return result.Map<IIonType>(v => v);
            }

            if (typeof(TIonType).Equals(typeof(IonFloat)))
            {
                _ = new IonFloatSerializer().TryParse(node, out var result);
                return result.Map<IIonType>(v => v);
            }

            if (typeof(TIonType).Equals(typeof(IonDecimal)))
            {
                _ = new IonDecimalSerializer().TryParse(node, out var result);
                return result.Map<IIonType>(v => v);
            }

            if (typeof(TIonType).Equals(typeof(IonTimestamp)))
            {
                _ = new IonTimestampSerializer().TryParse(node, out var result);
                return result.Map<IIonType>(v => v);
            }

            if (typeof(TIonType).Equals(typeof(IonString)))
            {
                _ = new IonStringSerializer().TryParse(node, out var result);
                return result.Map<IIonType>(v => v);
            }

            if (typeof(TIonType).Equals(typeof(IonOperator)))
            {
                _ = new IonOperatorSerializer().TryParse(node, out var result);
                return result.Map<IIonType>(v => v);
            }

            if (typeof(TIonType).Equals(typeof(IonIdentifier)))
            {
                _ = new IonIdentifierSerializer().TryParse(node, out var result);
                return result.Map<IIonType>(v => v);
            }

            if (typeof(TIonType).Equals(typeof(IonQuotedSymbol)))
            {
                _ = new IonQuotedSymbolSerializer().TryParse(node, out var result);
                return result.Map<IIonType>(v => v);
            }

            if (typeof(TIonType).Equals(typeof(IonBlob)))
            {
                _ = new IonBlobSterializer().TryParse(node, out var result);
                return result.Map<IIonType>(v => v);
            }

            if (typeof(TIonType).Equals(typeof(IonClob)))
            {
                _ = new IonClobSerializer().TryParse(node, out var result);
                return result.Map<IIonType>(v => v);
            }

            if (typeof(TIonType).Equals(typeof(IonList)))
            {
                _ = new IonListSerializer().TryParse(node, out var result);
                return result.Map<IIonType>(v => v);
            }

            if (typeof(TIonType).Equals(typeof(IonSexp)))
            {
                _ = new IonSexpSerializer().TryParse(node, out var result);
                return result.Map<IIonType>(v => v);
            }

            if (typeof(TIonType).Equals(typeof(IonStruct)))
            {
                _ = new IonStructSerializer().TryParse(node, out var result);
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

        private static IEnumerable<char> ToCharacterEnumerable(Stream ionStream)
        {
            var reader = new StreamReader(ionStream, Encoding.Unicode);
            var @char = -1;

            while ((@char = reader.Read()) != -1)
                yield return (char)@char;
        }

        private static bool TryParseIonPacket(CSTNode ionToken, out IResult<IonPacket> result)
        {
            result = ionToken
                .FindNodes(IonValueSymbol)
                .Select(node =>
                {
                    _ = TryParseIonValueToken(node, out var result);
                    return result;
                })
                .Fold()
                .Map(values => new IonPacket(values.ToArray()));

            return result is IResult<IonPacket>.DataResult;
        }
    }
}
