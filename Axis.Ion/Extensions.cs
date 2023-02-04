using Axis.Ion.IO.Binary;
using Axis.Ion.Types;
using Axis.Ion.Utils;
using Axis.Luna.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using static Axis.Ion.Types.IIonType;

namespace Axis.Ion
{
    /// <summary>
    /// See https://github.com/amzn/ion-dotnet/blob/ff73b85e203712eaff7c7e51a6cb94e606b4a82e/Amazon.IonDotnet/Utils/BitConverterEx.cs#L20
    /// </summary>
    internal static class Extensions
    {
        internal static string Reverse(this string value)
        {
            if (string.IsNullOrWhiteSpace(value)
                || value.Length == 1)
                return value;

            else
                return value
                    .ToCharArray()
                    .With(Array.Reverse)
                    .ApplyTo(array => new string(array));
        }

        public static Annotation[] Validate(this Annotation[] annotations)
        {
            if (annotations == null)
                throw new ArgumentNullException(nameof(annotations));

            if(annotations.Any(a => a == default))
                throw new ArgumentException($"Invalid {nameof(annotations)}");

            return annotations;
        }

        public static string ValidatePropertyName(this string propertyName)
        {
            if (IonIdentifier.TryParse(propertyName, out _))
                return propertyName;

            else throw new FormatException($"Invlid property name: {propertyName}");
        }

        public static IonValueWrapper Wrap(this IIonType ionType)
            => new IonValueWrapper(ionType);

        public static DecomposedDecimal Deconstruct(this decimal value) => new DecomposedDecimal(value);
        public static DecomposedDecimal Deconstruct(this float value) => new DecomposedDecimal(value);
        public static DecomposedDecimal Deconstruct(this double value) => new DecomposedDecimal(value);

        public static bool IsEnumDefined<TEnum>(this TEnum enumValue)
        where TEnum : struct
        {
            return Enum.IsDefined(typeof(TEnum), enumValue);
        }

        public static string AsString(this char[] charArray) => new string(charArray);

        public static string AsString(this IEnumerable<char> charArray) => new string(charArray.ToArray());

        public static void Repeat(this int repetitions, Action action)
        {
            repetitions = Math.Abs(repetitions);
            for(int cnt = 0; cnt < repetitions; cnt++)
            {
                action.Invoke();
            }
        }

        public static void Repeat(this
            BigInteger repetitions,
            Action action)
        {
            if (repetitions <= int.MaxValue)
            {
                var intRepetitions = (int)repetitions;
                for (int cnt = 0; cnt < intRepetitions; cnt++)
                    action.Invoke();

                return;
            }

            if (repetitions <= long.MaxValue)
            {
                var longRepetitions = (long)repetitions;
                for (long cnt = 0; cnt < longRepetitions; cnt++)
                    action.Invoke();

                return;
            }

            for (BigInteger cnt = 0; cnt < repetitions; cnt++)
                action.Invoke();

            return;
        }

        public static IEnumerable<TOut> RepeatApply<TOut>(
            this BigInteger repetitions,
            Func<BigInteger, TOut> map)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            for (BigInteger cnt = 0; cnt < repetitions; cnt++)
                yield return map.Invoke(cnt);
        }

        public static IEnumerable<TOut> RepeatApply<TOut>(
            this int repetitions,
            Func<int, TOut> map)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            for (int cnt = 0; cnt < repetitions; cnt++)
                yield return map.Invoke(cnt);
        }

        public static byte[] GetBytes(this decimal @decimal)
        {
            return @decimal
                .ApplyTo(decimal.GetBits)
                .SelectMany(BitConverter.GetBytes)
                .ToArray();
        }

        public static decimal ToDecimal(this byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

            if (bytes.Length != 16)
                throw new ArgumentException("byte array length must be 16");

            return bytes
                .Batch(4)
                .Select(batch => batch.Batch.ToArray())
                .Select(intArray => BitConverter.ToInt32(intArray))
                .ToArray()
                .ApplyTo(intArray => new decimal(intArray));
        }

        public static IEnumerable<(long Index, IEnumerable<T> Batch)> Batch<T>(this
            IEnumerable<T> values,
            int batchCount,
            bool returnIncompleteTail = true)
        {
            long count = 0;
            long index = 0;
            var batch = new List<T>();

            foreach(var value in values)
            {
                batch.Add(value);

                if(++count % batchCount == 0)
                {
                    yield return (index++, batch);

                    batch = new List<T>();
                }
            }

            if (batch.Count > 0 && returnIncompleteTail)
                yield return (index++, batch);
        }

        public static int CastToInt(this BigInteger bigInteger) => (int)bigInteger;

        public static long CastToLong(this BigInteger bigInteger) => (long)bigInteger;

        public static int HMS(this DateTimeOffset timestamp)
        {
            return (timestamp.Hour * 3600)
                +  (timestamp.Minute * 60)
                +  timestamp.Second;
        }

        public static long TickSeconds(this DateTimeOffset timestamp) => timestamp.TimeOfDay.Ticks % 10_000_000L;

        public static string ToExponentNotation(this
            decimal value,
            string exponentDelimiter = "E",
            ushort maxSignificantDigits = 17)
        {
            var zeros = value.ToString().ExtractFormatZeros(maxSignificantDigits);
            return value
                .ToString($"0.{zeros}E0")
                .Replace("E", exponentDelimiter);
        }

        public static string ToExponentNotation(this
            double value,
            ushort maxSignificantDigits = 17)
            => new DecomposedDecimal(value).ToScientificNotation(maxSignificantDigits);

        private static string ExtractFormatZeros(this string value, ushort maxSignificantDigits)
        {
            return value
                .ExtractSignificantDigits(maxSignificantDigits)
                .ApplyTo(digits => Zeros(digits.Length - 1));
        }

        private static string ExtractSignificantDigits(this
            string value,
            ushort maxSignificantDigits = 17)
        {
            var digits = value
                .Replace("-", "")
                .Replace(".", "")
                .TrimStart('0')
                .TrimEnd('0');

            return digits.Length >= maxSignificantDigits
                ? digits[..maxSignificantDigits]
                : digits;
        }

        private static string Zeros(int count)
        {
            return count
                .RepeatApply(index => '0')
                .ApplyTo(chars => new string(chars.ToArray()));
        }
    }
}
