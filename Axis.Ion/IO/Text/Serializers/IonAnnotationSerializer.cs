using Axis.Ion.Types;
using Axis.Luna.Common.Results;
using Axis.Luna.Extensions;
using Axis.Pulsar.Grammar.CST;
using System;
using System.Linq;

namespace Axis.Ion.IO.Text.Serializers
{
    public class IonAnnotationSerializer : IIonTextSerializer<IIonValue.Annotation[]>
    {
        public static string GrammarSymbol => AnnotationListSymbol;

        #region Symbols
        public const string AnnotationListSymbol = "annotation-list";
        public const string AnnotationSymbol = "annotation";
        public const string QuotedSymbol = "quoted-symbol";
        public const string IdentifierSymbol = "identifier";
        #endregion

        public static IResult<IIonValue.Annotation[]> Parse(CSTNode symbolNode)
        {
            ArgumentNullException.ThrowIfNull(symbolNode);

            try
            {
                if (!AnnotationListSymbol.Equals(symbolNode.SymbolName))
                    return Result.Of<IIonValue.Annotation[]>(new ArgumentException(
                        $"Invalid symbol name: '{symbolNode.SymbolName}'. "
                        + $"Expected '{AnnotationListSymbol}'"));

                var queryPath = $"{AnnotationSymbol}.{IdentifierSymbol}|{QuotedSymbol}";
                return symbolNode
                    .FindNodes(queryPath)
                    .Select(cstnode => cstnode.TokenValue())
                    .Select(ProcessEscapes)
                    .Select(IIonValue.Annotation.Parse)
                    .Fold()
                    .Map(result => result.ToArray());
            }
            catch (Exception e)
            {
                return Result.Of<IIonValue.Annotation[]>(e);
            }
        }

        public static string Serialize(IIonValue.Annotation[] value) => Serialize(value, new SerializingContext());

        public static string Serialize(
            IIonValue.Annotation[] value,
            SerializingContext context)
            => value
                .ThrowIfNull(new ArgumentNullException(nameof(value)))
                .Select(annotation => $"{annotation.Value}::")
                .JoinUsing("");

        public static string ProcessEscapes(string escapeSequence)
        {
            throw new NotImplementedException();
        }
    }
}
