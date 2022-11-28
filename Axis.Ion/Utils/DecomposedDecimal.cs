using Axis.Luna.Extensions;
using System;

namespace Axis.Ion.Utils
{
    public struct DecomposedDecimal
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


        public DecomposedDecimal(float value)
            :this(DoubleConverter.ToExactString(value))
        {
        }

        public DecomposedDecimal(double value)
            : this(DoubleConverter.ToExactString(value))
        {
        }

        public DecomposedDecimal(decimal value)
            : this(value.ToString())
        {
        }

        private DecomposedDecimal(string value)
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

            if(firstSig == lastSig)
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
}
