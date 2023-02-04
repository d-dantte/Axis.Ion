using Axis.Ion.Types;
using Axis.Luna.Common;
using Axis.Luna.Extensions;
using Axis.Pulsar.Grammar.CST;
using System;
using System.Linq;

namespace Axis.Ion.IO.Text.Streamers
{
    public class IonStructStreamer : AbstractIonTypeStreamer<IonStruct>
    {
        public const string IonStructSymbol = "ion-struct";
        public const string NullStructSymbol = "null-struct";
        public const string StructValueSymbol = "struct-value";
        public const string StructFieldSymbol = "struct-field";
        public const string IonValueSymbol = "ion-value";
        public const string SinglelineStringSymbol = "singleline-string";
        public const string QuotedSymbol = "quoted-symbol";
        public const string IdentifierSymbol = "identifier";

        protected override string IonValueSymbolName => IonStructSymbol;

        public override string GetIonValueText(IonStruct ionValue, StreamingContext context)
        {
            var valueIndentation = context.Indentation(1);
            var indentedContext = context.IndentContext();
            return ionValue.Value switch
            {
                null => "null.struct",
                _ => ionValue.Value
                    .Select(property => StreamProperty(property, indentedContext))
                    .Select(text => $"{valueIndentation}{text}")
                    .Append(context.Indentation())
                    .JoinUsing($",{Environment.NewLine}")
                    .WrapIn("{", "}")
            };
        }

        public override bool TryParse(CSTNode tokenNode, out IResult<IonStruct> result)
        {
            if (tokenNode is null)
                throw new ArgumentNullException(nameof(tokenNode));

            try
            {
                if (!IonStructSymbol.Equals(tokenNode.SymbolName))
                    return $"Invalid token node name: {tokenNode.SymbolName}".FailWithMessage(out result);

                var annotationToken = tokenNode.FirstNode();
                (var annotations, var structToken) = DeconstructAnnotations(tokenNode);

                return structToken.SymbolName switch
                {
                    NullStructSymbol => new IonStruct(annotations).PassWithValue(out result),

                    StructValueSymbol => structToken
                        .FindNodes(StructFieldSymbol)
                        .Select(ParseProperty)
                        .ApplyTo(ionProperties => new IonStruct(
                            new IonStruct.Initializer(annotations, ionProperties.ToArray())))
                        .PassWithValue(out result),

                    _ => $"Invalid ion bool token: {tokenNode.FirstNode().SymbolName}".FailWithMessage(out result)
                };
            }
            catch (Exception e)
            {
                result = Result.Of<IonStruct>(e);
                return false;
            }
        }

        private string StreamProperty(IonStruct.Property property, StreamingContext context)
        {
            return $"{property.Name}:{TextSerializer.StreamText(property.Value, context)}";
        }

        private IonStruct.Property ParseProperty(CSTNode structFieldNode)
        {
            var nameToken = structFieldNode.FirstNode();
            var ionValueToken = structFieldNode
                .FindNode(IonValueSymbol)
                .FirstNode();

            return new IonStruct.Property(
                value: TextSerializer.Parse(ionValueToken),
                name: nameToken.SymbolName switch
                {
                    SinglelineStringSymbol => nameToken.TokenValue().Trim("\""),
                    QuotedSymbol => nameToken.TokenValue(),
                    IdentifierSymbol => nameToken.TokenValue(),
                    _ => throw new ArgumentException($"Invalid property-name token found: {nameToken.SymbolName}")
                });
        }
    }
}
