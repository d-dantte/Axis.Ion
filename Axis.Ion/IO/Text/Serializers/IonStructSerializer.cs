using Axis.Ion.Types;
using Axis.Luna.Common.Results;
using Axis.Luna.Extensions;
using Axis.Pulsar.Grammar.CST;
using System;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace Axis.Ion.IO.Text.Serializers
{
    public class IonStructSerializer : IIonTextSerializer<IonStruct>
    {
        public static string GrammarSymbol => IonStructSymbol;

        #region Symbol
        public const string IonStructSymbol = "ion-struct";
        public const string NullStructSymbol = "null-struct";
        public const string StructValueSymbol = "struct-value";
        public const string StructFieldSymbol = "struct-field";
        public const string IonValueSymbol = "ion-value";
        public const string SinglelineStringSymbol = "singleline-string";
        public const string MultilineStringSymbol = "multiline-string";
        public const string QuotedSymbol = "quoted-symbol";
        public const string IdentifierSymbol = "identifier";
        #endregion

        public static IResult<IonStruct> Parse(CSTNode symbolNode)
        {
            ArgumentNullException.ThrowIfNull(symbolNode);

            try
            {
                if (!IonStructSymbol.Equals(symbolNode.SymbolName))
                    return Result.Of<IonStruct>(new ArgumentException(
                        $"Invalid symbol name: '{symbolNode.SymbolName}'. "
                        + $"Expected '{IonStructSymbol}'"));

                (var annotations, var structToken) = IonTextSerializer.DeconstructAnnotations(symbolNode);

                return structToken.SymbolName switch
                {
                    NullStructSymbol => Result.Of(new IonStruct(annotations)),

                    StructValueSymbol => structToken
                        .FindNodes(StructFieldSymbol)
                        .Select(ParseProperty)
                        .Fold()
                        .Map(properties => new IonStruct(annotations, properties.ToArray())),

                    _ => Result.Of<IonStruct>(new ArgumentException(
                        $"Invalid symbol encountered: '{structToken.SymbolName}'. "
                        + $"Expected '{NullStructSymbol}', or '{StructValueSymbol}'"))
                };
            }
            catch (Exception e)
            {
                return Result.Of<IonStruct>(e);
            }
        }

        public static string Serialize(IonStruct value) => Serialize(value, new SerializingContext());

        public static string Serialize(IonStruct value, SerializingContext context)
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

            var text = value.Value switch
            {
                null => "null.struct",
                _ => value.Value
                    .Select(property => SerializeProperty(property, indentedContext))
                    .JoinUsing(valueSeparator)
                    .WrapIn(ldelimiter, rdelimiter)
            };

            var annotationText = IonAnnotationSerializer.Serialize(value.Annotations, context);
            return $"{annotationText}{text}";
        }

        internal static string SerializeProperty(IonStruct.Property property, SerializingContext context)
        {
            var name = context.Options.Structs.UseQuotedIdentifierPropertyNames
                ? property.Name.Value.WrapIn("\"")
                : property.Name.ToIonText();

            if (name.Equals("null.symbol"))
                throw new InvalidOperationException("Property name cannot be a null symbol");

            return $"{name}:{IonValueSerializer.Serialize(property.Value, context)}";
        }

        internal static IResult<IonStruct.Property> ParseProperty(CSTNode structFieldNode)
        {
            var nameToken = structFieldNode
                .FirstNode()?
                .FirstNode();
            var ionValueToken = structFieldNode.FindNode(IonValueSymbol);

            var propertyName = nameToken?.SymbolName switch
            {
                QuotedSymbol
                or IdentifierSymbol => IonTextSymbolSerializer
                    .Parse(CSTNode.Of(IonTextSymbolSerializer.IonSymbol, nameToken))
                    .Map(symbol => symbol.Value!),

                SinglelineStringSymbol
                or MultilineStringSymbol => IonStringSerializer
                    .Parse(CSTNode.Of(IonStringSerializer.IonStringSymbol, nameToken))
                    .Map(@string => @string.Value!),

                _ => Result.Of<string>(new InvalidOperationException(
                    $"Invalid property name symbol: {nameToken.SymbolName}. "
                    + $"Expected '{QuotedSymbol}', or '{SinglelineStringSymbol}', etc"))
            };

            return Result.Of(() =>
                new IonStruct.Property(
                    propertyName.Resolve(),
                    IonValueSerializer.Parse(ionValueToken).Resolve()));
        }
    }
}
