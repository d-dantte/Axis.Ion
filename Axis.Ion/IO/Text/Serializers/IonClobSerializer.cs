using Axis.Ion.Types;
using Axis.Luna.Common.Results;
using Axis.Luna.Extensions;
using Axis.Pulsar.Grammar.CST;
using System;
using System.Linq;
using System.Text;
using static Axis.Ion.IO.Text.SerializerOptions;

namespace Axis.Ion.IO.Text.Serializers
{
    public class IonClobSerializer : IIonTextSerializer<IonClob>
    {
        public static string GrammarSymbol => IonClobSymbol;

        #region Symbol
        public const string IonClobSymbol = "ion-clob";
        public const string NullClobSymbol = "null-clob";
        public const string ClobValueSymbol = "clob-value";
        public const string SingleLineStringSymbol = "clob-singleline-string";
        public const string MultilineStringSymbol = "clob-multiline-string";
        public const string MLStringSymbol = "clob-ml-string";
        #endregion

        public static IResult<IonClob> Parse(CSTNode symbolNode)
        {
            ArgumentNullException.ThrowIfNull(symbolNode);

            try
            {
                if (!IonClobSymbol.Equals(symbolNode.SymbolName))
                    return Result.Of<IonClob>(new ArgumentException(
                        $"Invalid symbol name: '{symbolNode.SymbolName}'. "
                        + $"Expected '{IonClobSymbol}'"));

                (var annotations, var clobToken) = IonTextSerializer.DeconstructAnnotations(symbolNode);

                return clobToken.SymbolName switch
                {
                    NullClobSymbol => Result.Of(new IonClob(null, annotations)),

                    ClobValueSymbol => clobToken.FindNode($"{SingleLineStringSymbol}|{MultilineStringSymbol}") switch
                    {
                        CSTNode token when SingleLineStringSymbol.Equals(token.SymbolName) => token
                            .TokenValue()
                            .Trim("\"")
                            .ApplyTo(ProcessEscapes)
                            .ApplyTo(@string => new IonClob(
                                Encoding.ASCII.GetBytes(@string),
                                annotations))
                            .ApplyTo(Result.Of),

                        CSTNode token when MultilineStringSymbol.Equals(token.SymbolName) => token
                            .FindAllNodes(MLStringSymbol)
                            .Select(node => node.TokenValue())
                            .Select(line => line.Trim("'''"))
                            .JoinUsing("")
                            .ApplyTo(ProcessEscapes)
                            .ApplyTo(@string => new IonClob(
                                Encoding.ASCII.GetBytes(@string),
                                annotations))
                            .ApplyTo(Result.Of),

                        _ => Result.Of<IonClob>(new ArgumentException(
                            $"Invalid symbol encountered: '{clobToken.SymbolName}'. "
                            + $"Expected '{SingleLineStringSymbol}', or '{MultilineStringSymbol}'"))
                    },

                    _ => Result.Of<IonClob>(new ArgumentException(
                        $"Invalid symbol encountered: '{clobToken.SymbolName}'. "
                        + $"Expected '{NullClobSymbol}', or '{ClobValueSymbol}'"))
                };
            }
            catch (Exception e)
            {
                return Result.Of<IonClob>(e);
            }
        }

        public static string Serialize(IonClob value) => Serialize(value, new SerializingContext());

        public static string Serialize(IonClob value, SerializingContext context)
        {
            var text = value.Value switch
            {
                null => "null.clob",
                _ => IonClobSerializer
                    .ConvertToString(value.Value, context.Options.Clobs)
                    .ApplyTo(strings => context.Options.Clobs.LineStyle switch
                    {
                        StringLineStyle.Singleline => strings[0].WrapIn("{{ \"", "\" }}"),
                        StringLineStyle.Multiline => strings
                            .Select(@string => @string.WrapIn("'''"))
                            .ApplyTo(lines => WrapLines(lines.ToArray(), context)),
                        _ => throw new ArgumentException($"Invalid line style: {context.Options.Clobs.LineStyle}")
                    })
            };

            var annotationText = IonAnnotationSerializer.Serialize(value.Annotations, context);
            return $"{annotationText}{text}";
        }

        internal static string[] ConvertToString(byte[] bytes, StringOptions clobOption)
        {
            return bytes
                .Select(@byte => (char)@byte)
                .Batch(clobOption.LineBreakPoint)
                .Select(tuple => new string(tuple.Batch.ToArray()))
                .ToArray();
        }

        internal static string WrapLines(string[] lines, SerializingContext context)
        {
            return lines.Length switch
            {
                0 => "{{ \"\" }}",
                1 => $"{{{{ {lines[0]} }}}}",
                _ => lines
                    .Prepend("")
                    .JoinUsing($"{Environment.NewLine}{context.Indentation(1)}")
                    .WrapIn("{{", $"{Environment.NewLine}}}}}")
            };
        }

        public static string ProcessEscapes(string escapeSequence)
        {
            throw new NotImplementedException();
        }
    }
}
