using Axis.Ion.Types;
using Axis.Luna.Common.Results;
using Axis.Luna.Extensions;
using Axis.Pulsar.Grammar.CST;
using System;
using System.Linq;
using static Axis.Ion.IO.Text.SerializerOptions;
using static System.Net.Mime.MediaTypeNames;

namespace Axis.Ion.IO.Text.Serializers
{
    public class IonStringSerializer : IIonTextSerializer<IonString>
    {
        public static string GrammarSymbol => IonStringSymbol;

        #region Symbols
        public const string IonStringSymbol = "ion-string";
        public const string NullStringSymbol = "null-string";
        public const string SinglelineStringSymbol = "singleline-string";
        public const string MultilineStringSymbol = "multiline-string";
        public const string MLStringSymbol = "ml-string";
        #endregion

        public static IResult<IonString> Parse(CSTNode symbolNode)
        {
            ArgumentNullException.ThrowIfNull(symbolNode);

            try
            {
                if (!IonStringSymbol.Equals(symbolNode.SymbolName))
                    return Result.Of<IonString>(new ArgumentException(
                        $"Invalid symbol name: '{symbolNode.SymbolName}'. "
                        + $"Expected '{IonStringSymbol}'"));

                (var annotations, var stringToken) = IonTextSerializer.DeconstructAnnotations(symbolNode);

                return stringToken.SymbolName switch
                {
                    NullStringSymbol => Result.Of(new IonString(null, annotations)),

                    SinglelineStringSymbol => stringToken
                        .TokenValue()
                        .Trim("\"")
                        .ApplyTo(ProcessEscapes)
                        .ApplyTo(@string => Result.Of(new IonString(@string, annotations))),

                    MultilineStringSymbol => stringToken
                        .FindAllNodes(MLStringSymbol)
                        .Select(node => node.TokenValue())
                        .Select(line => line.Trim("'''"))
                        .JoinUsing("")
                        .ApplyTo(ProcessEscapes)
                        .ApplyTo(@string => Result.Of(new IonString(@string, annotations))),

                    _ => Result.Of<IonString>(new ArgumentException(
                        $"Invalid symbol encountered: '{stringToken.SymbolName}'. "
                        + $"Expected '{NullStringSymbol}', '{SinglelineStringSymbol}', etc"))
                };
            }
            catch (Exception e)
            {
                return Result.Of<IonString>(e);
            }
        }

        public static string Serialize(IonString value) => Serialize(value, new SerializingContext());

        public static string Serialize(IonString value, SerializingContext context)
        {
            var text = value.Value switch
            {
                null => "null.string",
                _ => context.Options.Strings.LineStyle switch
                {
                    StringLineStyle.Singleline => value.Value.WrapIn("\""),
                    StringLineStyle.Multiline => value.Value
                        .Batch(context.Options.Strings.LineBreakPoint)
                        .Select(batch => batch.Batch.ToArray())
                        .Select(batch => new string(batch).WrapIn("'''"))
                        .JoinUsing($"{Environment.NewLine}{context.Indentation()}"),

                    _ => throw new ArgumentException($"Invalid {nameof(StringLineStyle)}: {context.Options.Strings.LineStyle}")
                }
            };

            var annotationText = IonAnnotationSerializer.Serialize(value.Annotations, context);
            return $"{annotationText}{text}";
        }

        public static string ProcessEscapes(string escapeSequence)
        {
            throw new NotImplementedException();
        }
    }
}
