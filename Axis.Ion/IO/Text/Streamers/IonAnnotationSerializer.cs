using Axis.Ion.Types;
using Axis.Luna.Common;
using Axis.Luna.Common.Results;
using Axis.Luna.Extensions;
using Axis.Pulsar.Grammar.CST;
using System;
using System.Linq;

namespace Axis.Ion.IO.Text.Streamers
{
    public static class IonAnnotationSerializer
    {
        public const string AnnotationListSymbolName = "annotation-list";
        public const string AnnotationSymbolName = "annotation";
        public const string QuotedSymbolName = "quoted-symbol";
        public const string IdentifierSymbolName = "identifier";

        /// <summary>
        /// Serializes (streams) the annotation into text
        /// </summary>
        /// <param name="ionValue">the ion value</param>
        /// <returns>the textual representation of the ion value</returns>
        public static string StreamText(
            IIonType.Annotation annotation)
            => $"{annotation.Value}::";

        /// <summary>
        /// Serializes (streams) the annotation list into text
        /// </summary>
        /// <param name="ionValue">the ion value</param>
        /// <returns>the textual representation of the ion value</returns>
        public static string StreamText(
            IIonType.Annotation[] annotations)
            => annotations
                .ThrowIfNull(new ArgumentNullException(nameof(annotations)))
                .Select(StreamText)
                .JoinUsing("");

        /// <summary>
        /// Parses the <see cref="CSTNode"/> representation of the annotation text
        /// </summary>
        /// <param name="tokenNode">the token node</param>
        public static IIonType.Annotation[] ParseToken(CSTNode tokenNode)
        {
            _ = TryParse(tokenNode, out var result);
            return result.Resolve();
        }

        /// <summary>
        /// Attempts to parse the <see cref="CSTNode"/> into an <see cref="IIonType.Annotation"/>
        /// </summary>
        /// <param name="tokenNode">the token node</param>
        /// <param name="value">the output result</param>
        /// <returns>true if parsing was successful, false otherwise</returns>
        public static bool TryParse(CSTNode tokenNode, out IResult<IIonType.Annotation[]> result)
        {
            if (tokenNode is null)
                throw new ArgumentNullException(nameof(tokenNode));

            try
            {
                if (!AnnotationListSymbolName.Equals(tokenNode.SymbolName))
                    return "Invalid token node name: {tokenNode.SymbolName}".FailWithMessage(out result);

                var queryPath = $"{AnnotationSymbolName}.{IdentifierSymbolName}|{QuotedSymbolName}";
                return tokenNode
                    .FindNodes(queryPath)
                    .Select(cstnode => cstnode.TokenValue())
                    .Select(IIonType.Annotation.Parse)
                    .ToArray()
                    .PassWithValue(out result);
            }
            catch (Exception e)
            {
                result = Result.Of<IIonType.Annotation[]>(e);
                return false;
            }
        }

        /// <summary>
        /// Parses the raw ion text back into it's ion annotation list representation
        /// </summary>
        /// <param name="ionValueText">the textual representation of the annotation</param>
        public static IIonType.Annotation[] ParseString(string ionValueText)
        {
            _ = TryParse(ionValueText, out var result);
            return result.Resolve();
        }

        /// <summary>
        /// Attempts to parse the string into an <see cref="IIonType.Annotation"/>
        /// </summary>
        /// <param name="ionValueText">the textual representation of the ion value</param>
        /// <param name="value">the output result</param>
        /// <returns>true if parsing was successful, false otherwise</returns>
        public static bool TryParse(string ionValueText, out IResult<IIonType.Annotation[]> result)
        {
            var recognition = TextSerializer.IonGrammar
                .GetRecognizer(AnnotationListSymbolName)
                .Recognize(ionValueText);

            return recognition.TryParseRecognition(TryParse, out result);
        }
    }
}
