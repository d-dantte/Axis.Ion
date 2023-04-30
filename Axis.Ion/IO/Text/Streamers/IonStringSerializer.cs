using Axis.Ion.Types;
using Axis.Luna.Common;
using Axis.Luna.Common.Results;
using Axis.Luna.Extensions;
using Axis.Pulsar.Grammar.CST;
using System;
using System.Linq;
using static Axis.Ion.IO.Text.SerializerOptions;

namespace Axis.Ion.IO.Text.Streamers
{
    public class IonStringSerializer : AbstractIonTypeSerializer<IonString>
    {
        public const string IonStringSymbol = "ion-string";
        public const string NullStringSymbol = "null-string";
        public const string SinglelineStringSymbol = "singleline-string";
        public const string MultilineStringSymbol = "multiline-string";
        public const string MLStringSymbol = "ml-string";

        protected override string IonValueSymbolName => IonStringSymbol;

        public override string GetIonValueText(IonString ionValue, SerializingContext context)
        {
            return ionValue.Value switch
            {
                null => "null.string",
                _ => context.Options.Strings.LineStyle switch
                {
                    StringLineStyle.Singleline => ionValue.Value.WrapIn("\""),
                    StringLineStyle.Multiline => ionValue.Value
                        .Batch(context.Options.Strings.LineBreakPoint)
                        .Select(batch => batch.Batch.ToArray())
                        .Select(batch => new string(batch).WrapIn("'''"))
                        .JoinUsing($"{Environment.NewLine}{context.Indentation()}"),

                    _ => throw new ArgumentException($"Invalid {nameof(StringLineStyle)}: {context.Options.Strings.LineStyle}")
                }
            };
        }

        public override bool TryParse(CSTNode tokenNode, out IResult<IonString> result)
        {
            if (tokenNode is null)
                throw new ArgumentNullException(nameof(tokenNode));

            try
            {
                if (!IonStringSymbol.Equals(tokenNode.SymbolName))
                    return $"Invalid token node name: {tokenNode.SymbolName}".FailWithMessage(out result);

                (var annotations, var stringToken) = DeconstructAnnotations(tokenNode);

                return stringToken.SymbolName switch
                {
                    NullStringSymbol => new IonString(null, annotations).PassWithValue(out result),

                    SinglelineStringSymbol => stringToken
                        .TokenValue()
                        .Trim("\"")
                        .ApplyTo(@string => new IonString(@string, annotations))
                        .PassWithValue(out result),

                    MultilineStringSymbol => stringToken
                        .FindAllNodes(MLStringSymbol)
                        .Select(node => node.TokenValue())
                        .Select(line => line.Trim("'''"))
                        .JoinUsing("")
                        .ApplyTo(@string => new IonString(@string, annotations))
                        .PassWithValue(out result),

                    _ => $"Invalid ion bool token: {tokenNode.FirstNode().SymbolName}".FailWithMessage(out result)
                };
            }
            catch (Exception e)
            {
                result = Result.Of<IonString>(e);
                return false;
            }
        }
    }
}
