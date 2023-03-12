using Axis.Ion.Types;
using Axis.Luna.Common;
using Axis.Luna.Extensions;
using Axis.Pulsar.Grammar.CST;
using System;
using System.Linq;

namespace Axis.Ion.IO.Text.Streamers
{
    public class IonStructSerializer : AbstractIonTypeSerializer<IonStruct>
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

        public override string GetIonValueText(IonStruct ionValue, SerializingContext context)
        {
            var indentedContext = context.IndentContext();
            (var ldelimiter, var valueSeparator, var rdelimiter) = context.Options.Structs.UseMultipleLines switch
            {
                false => ("{", ", ", "}"),
                true => (
                    $"{{{Environment.NewLine}{indentedContext.Indentation()}",
                    $",{Environment.NewLine}{indentedContext.Indentation()}",
                    $"{Environment.NewLine}{context.Indentation()}}}")
            };

            return ionValue.Value switch
            {
                null => "null.struct",
                _ => ionValue.Value
                    .Select(property => StreamProperty(property, indentedContext))
                    .JoinUsing(valueSeparator)
                    .WrapIn(ldelimiter, rdelimiter)
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

        private string StreamProperty(IonStruct.Property property, SerializingContext context)
        {
            var name = property.Name switch
            {
                IonQuotedSymbol qs => qs.ToIonText(),

                IonIdentifier id => id
                    .ToIonText()
                    .WrapIf(_ => context.Options.Structs.UseQuotedIdentifierPropertyNames, "\""),

                null => throw new InvalidOperationException("Property name cannot be null"),

                _ => throw new InvalidOperationException($"Invalid property name object: {property.Name.GetType()}")
            };

            if (name.Equals("null.symbol"))
                throw new InvalidOperationException("Property name cannot be null");

            return $"{name}:{TextSerializer.SerializeText(property.Value, context)}";
        }

        private IonStruct.Property ParseProperty(CSTNode structFieldNode)
        {
            var nameToken = structFieldNode.FirstNode();
            var ionValueToken = structFieldNode.FindNode(IonValueSymbol);

            var propertyName = nameToken.SymbolName switch
            {
                QuotedSymbol => IIonTextSymbol.Parse(nameToken.TokenValue()),
                IdentifierSymbol => IIonTextSymbol.Parse(nameToken.TokenValue()),
                SinglelineStringSymbol => IIonTextSymbol.Parse(nameToken.TokenValue().UnwrapFrom("\"")),
                _ => throw new InvalidOperationException($"Invalid property name symbol: {nameToken.SymbolName}")
            };

            return new IonStruct.Property(
                propertyName,
                TextSerializer.ParseIonValueToken(ionValueToken));
        }
    }
}
