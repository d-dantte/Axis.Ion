using Axis.Ion.Types;
using Axis.Luna.Common.Results;
using Axis.Luna.Extensions;
using Axis.Pulsar.Grammar.CST;
using System;
using System.Linq;

namespace Axis.Ion.IO.Text.Serializers
{
    public class IonListSerializer : IIonTextSerializer<IonList>
    {
        public static string GrammarSymbol => IonListSymbol;

        #region Symbols
        public const string IonListSymbol = "ion-list";
        public const string NullListSymbol = "null-list";
        public const string ListValueSymbol = "list-value";
        public const string IonValueSymbol = "ion-value";
        #endregion

        public static IResult<IonList> Parse(CSTNode symbolNode)
        {
            ArgumentNullException.ThrowIfNull(symbolNode);

            try
            {
                if (!IonListSymbol.Equals(symbolNode.SymbolName))
                    return Result.Of<IonList>(new ArgumentException(
                        $"Invalid symbol name: '{symbolNode.SymbolName}'. "
                        + $"Expected '{IonListSymbol}'"));

                (var annotations, var listToken) = IonTextSerializer.DeconstructAnnotations(symbolNode);

                return listToken.SymbolName switch
                {
                    NullListSymbol => Result.Of(new IonList(annotations)),

                    ListValueSymbol => listToken
                        .FindNodes(IonValueSymbol)
                        .Select(IonValueSerializer.Parse)
                        .Fold()
                        .Map(ionValues => new IonList(annotations, ionValues.ToArray())),

                    _ => Result.Of<IonList>(new ArgumentException(
                        $"Invalid symbol encountered: '{listToken.SymbolName}'. "
                        + $"Expected '{NullListSymbol}', or '{ListValueSymbol}'"))
                };
            }
            catch (Exception e)
            {
                return Result.Of<IonList>(e);
            }
        }

        public static string Serialize(IonList value) => Serialize(value, new SerializingContext());

        public static string Serialize(IonList value, SerializingContext context)
        {
            var indentedContext = context.IndentContext();
            (var ldelimiter, var valueSeparator, var rdelimiter) = context.Options.Lists.UseMultipleLines switch
            {
                false => ("[", ", ", "]"),
                true => (
                    $"[{Environment.NewLine}{indentedContext.Indentation()}",
                    $",{Environment.NewLine}{indentedContext.Indentation()}",
                    $"{Environment.NewLine}{context.Indentation()}]")
            };

            var text = value.Value switch
            {
                null => "null.list",
                _ => value.Value
                    .Select(value => IonValueSerializer.Serialize(value, indentedContext))
                    .JoinUsing(valueSeparator)
                    .WrapIn(ldelimiter, rdelimiter)
            };

            var annotationText = IonAnnotationSerializer.Serialize(value.Annotations, context);
            return $"{annotationText}{text}";
        }
    }
}
