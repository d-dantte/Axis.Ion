using Axis.Ion.Types;
using Axis.Luna.Common.Results;
using Axis.Pulsar.Grammar.CST;
using System;
using System.Globalization;
using static System.Net.Mime.MediaTypeNames;

namespace Axis.Ion.IO.Text.Serializers
{
    public class IonBoolSerializer : IIonTextSerializer<IonBool>
    {
        public static string GrammarSymbol => IonBoolSymbol;

        #region Symbols
        public const string IonBoolSymbol = "ion-bool";
        public const string NullBoolSymbol = "null-bool";
        public const string TrueBoolSymbol = "true-bool";
        public const string FalseBoolSymbol = "false-bool";
        #endregion

        public static IResult<IonBool> Parse(CSTNode symbolNode)
        {
            ArgumentNullException.ThrowIfNull(symbolNode);

            try
            {
                if (!IonBoolSymbol.Equals(symbolNode.SymbolName))
                    return Result.Of<IonBool>(new ArgumentException(
                        $"Invalid symbol name: '{symbolNode.SymbolName}'. "
                        + $"Expected '{IonBoolSymbol}'"));

                (var annotations, var boolToken) = IonTextSerializer.DeconstructAnnotations(symbolNode);

                return boolToken.SymbolName switch
                {
                    NullBoolSymbol => Result.Of(new IonBool(null, annotations)),
                    TrueBoolSymbol => Result.Of(new IonBool(true, annotations)),
                    FalseBoolSymbol => Result.Of(new IonBool(false, annotations)),
                    _ => Result.Of<IonBool>(new ArgumentException(
                        $"Invalid symbol encountered: '{boolToken.SymbolName}'. "
                        + $"Expected '{NullBoolSymbol}', '{TrueBoolSymbol}', etc"))
                };
            }
            catch (Exception e)
            {
                return Result.Of<IonBool>(e);
            }
        }

        public static string Serialize(IonBool value) => Serialize(value, new SerializingContext());

        public static string Serialize(IonBool value, SerializingContext context)
        {
            var options = context.Options;
            var text = value.Value switch
            {
                null => "null.bool",
                true => ApplyCase("true", options.Bools.ValueCase),
                false => ApplyCase("false", options.Bools.ValueCase)
            };

            var annotationText = IonAnnotationSerializer.Serialize(value.Annotations, context);
            return $"{annotationText}{text}";
        }

        private static string ApplyCase(string value, SerializerOptions.Case @case)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException($"Invalid value");

            var textInfo = CultureInfo.InvariantCulture.TextInfo;
            return @case switch
            {
                SerializerOptions.Case.Uppercase => textInfo.ToUpper(value),
                SerializerOptions.Case.Lowercase => textInfo.ToLower(value),
                SerializerOptions.Case.Titlecase => textInfo.ToTitleCase(value),
                _ => throw new ArgumentException($"Invalid case: {@case}")
            };
        }
    }
}
