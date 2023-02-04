using Axis.Luna.Extensions;
using System;
using System.Text.RegularExpressions;

namespace Axis.Ion.Utils
{
    [Obsolete]
    public struct DecomposedDecimal2
    {
        public enum NumericSign
        {
            Positive,
            Negative
        }

        private readonly string _value;
        private readonly int? _decimalndex;

        public string Integer => _value != null
            ? _value[..(_decimalndex ?? _value.Length)]
            : "0";

        public string Decimal => _decimalndex != null
            ? _value[_decimalndex.Value..]
            : "0";

        public NumericSign Sign { get; }


        public DecomposedDecimal2(float value)
            :this(DoubleConverter.ToExactString(value))
        {
        }

        public DecomposedDecimal2(double value)
            : this(DoubleConverter.ToExactString(value))
        {
        }

        public DecomposedDecimal2(decimal value)
            : this(value.ToString())
        {
        }

        private DecomposedDecimal2(string value)
        {
            var parts = value.Split('-', '.');

            Sign = value[0] == '-'
                ? NumericSign.Negative
                : NumericSign.Positive;

            // ion supports nan, +inf & -inf, so pass those values along,
            // and let the ion-type handle it.
            _value =
                value.EndsWith("NaN") ? "0" :
                value.EndsWith("Infinity") ? "0" :
                parts.Length == 1 ? parts[0] :
                parts.Length == 2 && Sign == NumericSign.Negative ? parts[1] :
                parts.Length == 2 && Sign == NumericSign.Positive ? $"{parts[0]}{parts[1]}" :
                $"{parts[1]}{parts[2]}";

            _decimalndex = value
                .IndexOf('.')
                .ApplyTo(index => index < 0 ? (int?)null : index);
        }


        public string ToScientificNotation(int maxPrecision = 17)
        {
            if (maxPrecision <= 0)
                throw new ArgumentException($"Invalid precision: {maxPrecision}");

            var sign = Sign switch
            {
                NumericSign.Positive => "",
                NumericSign.Negative => "-",
                _ => throw new InvalidOperationException($"Invalid sign: {Sign}")
            };

            if (_value == null
                || _value.Equals("0"))
                return $"{sign}0.0E0";

            var (firstSig, lastSig, diff) = SignificantDigits(maxPrecision);
            char integral = _value[firstSig];
            string mantissa;
            int exponent;
            string exponentSign;

            if (firstSig == lastSig)
            {
                mantissa = "0";

                exponent = firstSig == 0
                    ? diff
                    : firstSig;

                exponentSign = firstSig switch
                {
                    0 => "",
                    _ => "-"
                };
            }
            else if (firstSig > 0)
            {
                mantissa = _value[(firstSig + 1)..(lastSig + 1)];
                exponent = firstSig;
                exponentSign = "-";
            }
            else // significant.first == 0
            {
                mantissa = _value[(firstSig + 1)..(lastSig + 1)];
                exponent = (_decimalndex ?? _value.Length) - 1;
                exponentSign = "";
            }

            return $"{sign}{integral}.{mantissa}E{exponentSign}{exponent}";
        }

        private (int first, int last, int diff) SignificantDigits(int maxPrecision)
        {
            int first = -1;
            for (int cnt = 0; cnt < _value.Length; cnt++)
            {
                if (_value[cnt] != '0')
                {
                    first = cnt;
                    break;
                }
            }

            int last = -1;
            var lim = Math.Min(_value.Length - 1, first + maxPrecision - 1);
            var diff = 0;
            for(int cnt = lim; cnt >= first; cnt--)
            {
                if (_value[cnt] != '0')
                {
                    last = cnt;
                    break;
                }

                else diff++;
            }

            return (first, last, diff);
        }
    }

    public struct DecomposedDecimal
    {
        private static readonly Regex DecimalPattern = new Regex(@"\-?\d+(.\d+)?");

        public enum NumericSign
        {
            Positive,
            Negative
        }

        private readonly string? _significantDigits;
        private readonly int _exponent;
        private readonly NumericSign _sign;

        public DecomposedDecimal(double value)
            : this(DoubleConverter.ToExactString(value))
        {
        }

        public DecomposedDecimal(decimal value)
            :this(value.ToString())
        {
        }

        public DecomposedDecimal(string value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (!DecimalPattern.IsMatch(value))
                throw new ArgumentException($"invalid decimal value: {value}");

            _sign = value.StartsWith("-")
                ? NumericSign.Negative
                : NumericSign.Positive;

            var trimmed = _sign switch
            {
                NumericSign.Positive => TrimZeros(value),
                NumericSign.Negative => TrimZeros(value[1..]),
                _ => throw new InvalidOperationException($"Invalid sign: {_sign}")
            };

            var pointIndex = trimmed.IndexOf('.');

            (_significantDigits, var significantIndex) = ExtractSignificantDigits(trimmed.Replace(".", ""));

            _exponent = significantIndex >= pointIndex
                ? -significantIndex
                : pointIndex - 1; 
        }

        public string ToScientificNotation(int maxPrecision = 17)
        {
            if (_significantDigits is null)
                return $"{SignText()}0.0E0";

            var text = _significantDigits.Length > maxPrecision
                ? _significantDigits[..maxPrecision]
                : _significantDigits;

            text = text.TrimEnd('0');

            if (text.Length == 1)
                text = $"{text}0";

            return $"{SignText()}{text.Insert(1, ".")}E{_exponent}";
        }

        private string SignText() => _sign switch
        {
            NumericSign.Positive => "",
            NumericSign.Negative => "-",
            _ => throw new ArgumentException($"Invalid sign: {(char)_sign}")
        };

        private static string TrimStartZeros(string value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (string.IsNullOrWhiteSpace(value))
                return "0.0";

            if ("0".Equals(value) || "0.0".Equals(value))
                return "0.0";

            if (value.StartsWith("0."))
                return value;

            var trimmed = value.TrimStart("0");

            if (trimmed.Length == 0)
                return "0.0";

            if (trimmed.StartsWith("."))
                return $"0{trimmed}";

            return trimmed;
        }

        private static string TrimEndZeros(string value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (string.IsNullOrWhiteSpace(value))
                return "0.0";

            if ("0.0".Equals(value))
                return value;

            if (!value.Contains("."))
                return $"{value}.0";

            return value.TrimEnd('0');
        }

        private static string TrimZeros(string value)
        {
            return TrimEndZeros(TrimStartZeros(value));
        }

        private static (string? significantDigits, int significantIndex) ExtractSignificantDigits(string textValue)
        {
            if ("00".Equals(textValue))
                return (null, 0);

            var significantStartIndex = 0;
            for (; significantStartIndex < textValue.Length; significantStartIndex++)
            {
                if (textValue[significantStartIndex] != '0')
                    break;
            }

            var significantEndIndex = textValue.Length - 1;
            for (; significantEndIndex >= 0; significantEndIndex--)
            {
                if (textValue[significantEndIndex] != '0')
                    break;
            }

            var count = (significantEndIndex - significantStartIndex) + 1;
            return (textValue.Substring(significantStartIndex, count), significantStartIndex);
        }
    }
}
