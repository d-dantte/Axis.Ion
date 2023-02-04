using Axis.Ion.Types;
using Axis.Luna.Common;
using Axis.Pulsar.Grammar.CST;
using System;
using System.Globalization;

namespace Axis.Ion.IO.Text.Streamers
{
    public class IonBoolStreamer : AbstractIonTypeStreamer<IonBool>
    {
        public const string IonBoolSymbol = "ion-bool";
        public const string NullBoolSymbol = "null-bool";
        public const string TrueBoolSymbol = "true-bool";
        public const string FalseBoolSymbol = "false-bool";

        override protected string IonValueSymbolName => IonBoolSymbol;

        override public bool TryParse(CSTNode tokenNode, out IResult<IonBool> result)
        {
            if (tokenNode is null)
                throw new ArgumentNullException(nameof(tokenNode));

            try
            {
                if (!IonBoolSymbol.Equals(tokenNode.SymbolName))
                    return $"Invalid token node name: {tokenNode.SymbolName}".FailWithMessage(out result);

                (var annotations, var boolToken) = DeconstructAnnotations(tokenNode);

                return boolToken.SymbolName switch
                {
                    NullBoolSymbol => new IonBool(null, annotations).PassWithValue(out result),
                    TrueBoolSymbol => new IonBool(true, annotations).PassWithValue(out result),
                    FalseBoolSymbol => new IonBool(false, annotations).PassWithValue(out result),
                    _ => $"Invalid ion bool token: {tokenNode.FirstNode().SymbolName}".FailWithMessage(out result)
                };
            }
            catch (Exception e)
            {
                result = Result.Of<IonBool>(e);
                return false;
            }
        }

        override public string GetIonValueText(IonBool ionValue, StreamingContext context)
        {
            var options = context.Options;
            return ionValue.Value switch
            {
                null => "null.bool",
                true => ApplyCase("true", options.Bools.ValueCase),
                false => ApplyCase("false", options.Bools.ValueCase)
            };
        }

        private string ApplyCase(string value, SerializerOptions.Case @case)
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
