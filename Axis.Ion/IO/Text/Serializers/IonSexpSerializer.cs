using Axis.Ion.Types;
using Axis.Luna.Common.Results;
using Axis.Luna.Extensions;
using Axis.Pulsar.Grammar.CST;
using System;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace Axis.Ion.IO.Text.Serializers
{
    public class IonSexpSerializer : IIonTextSerializer<IonSexp>
    {
        public static string GrammarSymbol => IonSexpSymbol;

        #region Symbols
        public const string IonSexpSymbol = "ion-sexp";
        public const string NullSexpSymbol = "null-sexp";
        public const string SexpValueSymbol = "sexp-value";
        public const string IonValueSymbol = "ion-value";
        #endregion

        public static IResult<IonSexp> Parse(CSTNode symbolNode)
        {
            ArgumentNullException.ThrowIfNull(symbolNode);

            try
            {
                if (!IonSexpSymbol.Equals(symbolNode.SymbolName))
                    return Result.Of<IonSexp>(new ArgumentException(
                        $"Invalid symbol name: '{symbolNode.SymbolName}'. "
                        + $"Expected '{IonSexpSymbol}'"));

                (var annotations, var sexpToken) = IonTextSerializer.DeconstructAnnotations(symbolNode);

                return sexpToken.SymbolName switch
                {
                    NullSexpSymbol => Result.Of(new IonSexp(annotations)),

                    SexpValueSymbol => sexpToken
                        .FindNodes(IonValueSymbol)
                        .Select(IonValueSerializer.Parse)
                        .Fold()
                        .Map(ionValues => new IonSexp(annotations, ionValues.ToArray())),

                    _ => Result.Of<IonSexp>(new ArgumentException(
                        $"Invalid symbol encountered: '{sexpToken.SymbolName}'. "
                        + $"Expected '{NullSexpSymbol}', or '{SexpValueSymbol}'"))
                };
            }
            catch (Exception e)
            {
                return Result.Of<IonSexp>(e);
            }
        }

        public static string Serialize(IonSexp value) => Serialize(value, new SerializingContext());

        public static string Serialize(IonSexp value, SerializingContext context)
        {
            (var ldelimiter, var valueSeparator, var rdelimiter) = context.Options.Sexps.UseMultipleLines switch
            {
                false => ("( ", " ", " )"),
                true => (
                    $"({Environment.NewLine}{context.Indentation(1)}",
                    $"{Environment.NewLine}{context.Indentation(1)}",
                    $"{Environment.NewLine}{context.Indentation()})")
            };

            var text = value.Value switch
            {
                null => "null.sexp",
                _ => value.Value
                    .Select(value => IonValueSerializer.Serialize(value, context.IndentContext()))
                    .JoinUsing(valueSeparator)
                    .WrapIn(ldelimiter, rdelimiter)
            };

            var annotationText = IonAnnotationSerializer.Serialize(value.Annotations, context);
            return $"{annotationText}{text}";
        }
    }
}
