using Axis.Ion.Types;
using Axis.Luna.Common.Results;
using Axis.Pulsar.Grammar.CST;
using System;

namespace Axis.Ion.IO.Text.Serializers
{
    public class IonNullSerializer : IIonTextSerializer<IonNull>
    {
        public static string GrammarSymbol => IonNullSymbolName;

        #region Symbols
        public const string IonNullSymbolName = "ion-null";
        public const string NullSymbolName = "null";
        public const string NullNullSymbolName = "null-null";
        public const string AnnotationListSymbolName = "annotation-list";
        #endregion

        public static IResult<IonNull> Parse(CSTNode symbolNode)
        {
            ArgumentNullException.ThrowIfNull(symbolNode);

            try
            {
                if (!IonNullSymbolName.Equals(symbolNode.SymbolName))
                    return Result.Of<IonNull>(new ArgumentException(
                        $"Invalid symbol name: '{symbolNode.SymbolName}'. "
                        + $"Expected '{IonNullSymbolName}'"));

                (var annotations, var nullToken) = IonTextSerializer.DeconstructAnnotations(symbolNode);

                return nullToken.SymbolName switch
                {
                    NullSymbolName
                    or NullNullSymbolName => Result.Of(new IonNull(annotations)),

                    _ => Result.Of<IonNull>(new ArgumentException(
                        $"Invalid symbol encountered: '{nullToken.SymbolName}'. "
                        + $"Expected '{NullSymbolName}', or '{NullNullSymbolName}'"))
                };
            }
            catch (Exception e)
            {
                return Result.Of<IonNull>(e);
            }
        }

        public static string Serialize(IonNull value) => Serialize(value, new SerializingContext());

        public static string Serialize(IonNull value, SerializingContext context)
        {
            var text = context.Options.Nulls.UseLongFormNulls ? "null.null" : "null";

            var annotationText = IonAnnotationSerializer.Serialize(value.Annotations, context);
            return $"{annotationText}{text}";
        }
    }
}
