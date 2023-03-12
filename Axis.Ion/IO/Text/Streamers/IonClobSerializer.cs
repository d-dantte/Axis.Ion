using Axis.Ion.Types;
using Axis.Luna.Common;
using Axis.Luna.Extensions;
using Axis.Pulsar.Grammar.CST;
using System;
using System.Linq;
using System.Text;
using static Axis.Ion.IO.Text.SerializerOptions;

namespace Axis.Ion.IO.Text.Streamers
{
    public class IonClobSerializer : AbstractIonTypeSerializer<IonClob>
    {
        public const string IonClobSymbol = "ion-clob";
        public const string NullClobSymbol = "null-clob";
        public const string ClobValueSymbol = "clob-value";
        public const string SingleLineStringSymbol = "singleline-string";
        public const string MultilineStringSymbol = "clob-multiline-string";
        public const string MLStringSymbol = "ml-string";

        protected override string IonValueSymbolName => IonClobSymbol;

        override public string GetIonValueText(IonClob ionValue, SerializingContext context)
        {
            return ionValue.Value switch
            {
                null => "null.clob",
                _ => this
                    .ConvertToString(ionValue.Value, context.Options.Clobs)
                    .ApplyTo(strings => context.Options.Clobs.LineStyle switch
                    {
                        StringLineStyle.Singleline => strings[0].WrapIn("{{ \"", "\" }}"),
                        StringLineStyle.Multiline => strings
                            .Select(@string => @string.WrapIn("'''"))
                            .ApplyTo(lines => WrapLines(lines.ToArray(), context)),
                        _ => throw new ArgumentException($"Invalid line style: {context.Options.Clobs.LineStyle}")
                    })
            };
        }

        override public bool TryParse(CSTNode tokenNode, out IResult<IonClob> result)
        {
            if (tokenNode is null)
                throw new ArgumentNullException(nameof(tokenNode));

            try
            {
                if (!IonClobSymbol.Equals(tokenNode.SymbolName))
                    return $"Invalid token node name: {tokenNode.SymbolName}".FailWithMessage(out result);

                var annotationToken = tokenNode.FirstNode();
                (var annotations, var clobToken) = DeconstructAnnotations(tokenNode);

                return clobToken.SymbolName switch
                {
                    NullClobSymbol => new IonClob(null, annotations).PassWithValue(out result),

                    ClobValueSymbol => clobToken.FindNode($"{SingleLineStringSymbol}|{MultilineStringSymbol}") switch
                    {
                        CSTNode token when SingleLineStringSymbol.Equals(token.SymbolName) => token
                            .TokenValue()
                            .Trim("\"")
                            .ApplyTo(@string => new IonClob(
                                Encoding.ASCII.GetBytes(@string),
                                annotations))
                            .PassWithValue(out result),

                        CSTNode token when MultilineStringSymbol.Equals(token.SymbolName) => token
                            .FindAllNodes(MLStringSymbol)
                            .Select(node => node.TokenValue())
                            .Select(line => line.Trim("'''"))
                            .JoinUsing("")
                            .ApplyTo(@string => new IonClob(
                                Encoding.ASCII.GetBytes(@string),
                                annotations))
                            .PassWithValue(out result),

                        _ => TextExtensions.FailWithMessage(
                            $"Could not find the '{SingleLineStringSymbol}' or '{MultilineStringSymbol}' symbol in the token",
                            out result)
                    },

                    _ => TextExtensions.FailWithMessage(
                        $"Invalid ion bool token: {tokenNode.FirstNode().SymbolName}",
                        out result)
                };
            }
            catch (Exception e)
            {
                result = Result.Of<IonClob>(e);
                return false;
            }
        }

        private string[] ConvertToString(byte[] bytes, StringOptions clobOption)
        {
            return bytes
                .Select(@byte => (char)@byte)
                .Batch(clobOption.LineBreakPoint)
                .Select(tuple => new string(tuple.Batch.ToArray()))
                .ToArray();
        }

        private string WrapLines(string[] lines, SerializingContext context)
        {
            return lines.Length switch
            {
                0 => "{{ \"\" }}",
                1 => $"{{{{ {lines[0]} }}}}",
                _ => lines
                    .AppendAt(0, "")
                    .JoinUsing($"{Environment.NewLine}{context.Indentation(1)}")
                    .WrapIn("{{", $"{Environment.NewLine}}}}}")
            };
        }
    }
}
