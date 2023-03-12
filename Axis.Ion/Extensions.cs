using Axis.Ion.Types;
using Axis.Ion.Utils;
using Axis.Luna.Extensions;
using Axis.Luna.FInvoke;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static Axis.Ion.Types.IIonType;

namespace Axis.Ion
{
    /// <summary>
    /// See https://github.com/amzn/ion-dotnet/blob/ff73b85e203712eaff7c7e51a6cb94e606b4a82e/Amazon.IonDotnet/Utils/BitConverterEx.cs#L20
    /// </summary>
    internal static class Extensions
    {
        internal static object? IonValue(this IIonType ion)
        {
            if (ion is null)
                throw new ArgumentNullException(nameof(ion));

            return ion switch
            {
                IonNull => null,
                IonBool @bool => @bool.Value,
                IonInt @int => @int.Value,
                IonFloat @float => @float.Value,
                IonDecimal @decimal => @decimal.Value,
                IonTimestamp timestamp => timestamp.Value,
                IonString @string => @string.Value,
                IonOperator @operator => @operator.Value,
                IonIdentifier identifier => identifier.Value,
                IonQuotedSymbol quoted => quoted.Value,
                IonClob clob => clob.Value,
                IonBlob blob => blob.Value,
                IonSexp sexp => sexp.Value,
                IonList list => list.Value,
                IonStruct @struct => @struct.Value,
                _ => throw new ArgumentException($"Invalid ion: {ion}")
            };
        }

        internal static T FirstOrThrow<T>(this IEnumerable<T> enumerable, Exception exception)
        {
            if (enumerable is null)
                throw new ArgumentNullException(nameof(enumerable));

            exception ??= new Exception();

            using var enumerator = enumerable.GetEnumerator();

            if (enumerator.MoveNext())
                return enumerator.Current;

            else return exception.Throw<T>();
        }

        internal static T? FirstOrNull<T>(this IEnumerable<T> enumerable)
        where T : struct
        {
            if (enumerable is null)
                throw new ArgumentNullException(nameof(enumerable));

            using var enumerator = enumerable.GetEnumerator();

            if (enumerator.MoveNext())
                return enumerator.Current;

            else return new T?();
        }

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

        internal static Annotation[] Validate(this Annotation[] annotations)
        {
            if (annotations == null)
                throw new ArgumentNullException(nameof(annotations));

            if(annotations.Any(a => a == default))
                throw new ArgumentException($"Invalid {nameof(annotations)}");

            return annotations;
        }

        internal static IonValueWrapper Wrap(this IIonType ionType)
            => new IonValueWrapper(ionType);

        internal static DecomposedDecimal Deconstruct(this decimal value) => new DecomposedDecimal(value);
        internal static DecomposedDecimal Deconstruct(this float value) => new DecomposedDecimal(value);
        internal static DecomposedDecimal Deconstruct(this double value) => new DecomposedDecimal(value);

        internal static bool IsEnumDefined<TEnum>(this TEnum enumValue)
        where TEnum : struct
        {
            return Enum.IsDefined(typeof(TEnum), enumValue);
        }

        internal static string AsString(this char[] charArray) => new string(charArray);

        internal static string AsString(this IEnumerable<char> charArray) => new string(charArray.ToArray());

        internal static void Repeat(this int repetitions, Action action)
        {
            repetitions = Math.Abs(repetitions);
            for(int cnt = 0; cnt < repetitions; cnt++)
            {
                action.Invoke();
            }
        }

        internal static void Repeat(this
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

        internal static IEnumerable<TOut> RepeatApply<TOut>(
            this BigInteger repetitions,
            Func<BigInteger, TOut> map)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            for (BigInteger cnt = 0; cnt < repetitions; cnt++)
                yield return map.Invoke(cnt);
        }

        internal static IEnumerable<TOut> RepeatApply<TOut>(
            this int repetitions,
            Func<int, TOut> map)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            for (int cnt = 0; cnt < repetitions; cnt++)
                yield return map.Invoke(cnt);
        }

        internal static byte[] GetBytes(this decimal @decimal)
        {
            return @decimal
                .ApplyTo(decimal.GetBits)
                .SelectMany(BitConverter.GetBytes)
                .ToArray();
        }

        internal static decimal ToDecimal(this byte[] bytes)
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

        internal static IEnumerable<(long Index, IEnumerable<T> Batch)> Batch<T>(this
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

        internal static int CastToInt(this BigInteger bigInteger) => (int)bigInteger;

        internal static long CastToLong(this BigInteger bigInteger) => (long)bigInteger;

        internal static int HMS(this DateTimeOffset timestamp)
        {
            return (timestamp.Hour * 3600)
                +  (timestamp.Minute * 60)
                +  timestamp.Second;
        }

        internal static long TickSeconds(this DateTimeOffset timestamp) => timestamp.TimeOfDay.Ticks % 10_000_000L;

        internal static string ToExponentNotation(this
            decimal value,
            string exponentDelimiter = "E",
            ushort maxSignificantDigits = 17)
        {
            var zeros = value.ToString().ExtractFormatZeros(maxSignificantDigits);
            return value
                .ToString($"0.{zeros}E0")
                .Replace("E", exponentDelimiter);
        }

        internal static string ToExponentNotation(this
            double value,
            ushort maxSignificantDigits = 17)
            => new DecomposedDecimal(value).ToScientificNotation(maxSignificantDigits);

        internal static bool IsIntegral(this Type clrType, out Type actualType)
        {
            actualType = clrType.IsGenericType && clrType.GetGenericTypeDefinition() == typeof(Nullable<>)
                ? clrType.GetGenericArguments()[0]
                : clrType;

            return Type.GetTypeCode(actualType) switch
            {
                TypeCode.Int16 => true,
                TypeCode.UInt16 => true,
                TypeCode.Int32 => true,
                TypeCode.UInt32 => true,
                TypeCode.Int64 => true,
                TypeCode.UInt64 => true,
                _ => typeof(BigInteger).Equals(actualType)
            };
        }

        internal static bool IsReal(this Type clrType, out Type actualType)
        {
            actualType = clrType.IsGenericType && clrType.GetGenericTypeDefinition() == typeof(Nullable<>)
                ? clrType.GetGenericArguments()[0]
                : clrType;

            return Type.GetTypeCode(actualType) switch
            {
                TypeCode.Single => true,
                TypeCode.Double => true,
                _ => false
            };
        }

        internal static bool IsDecimal(this Type clrType, out Type actualType)
        {
            actualType = clrType.IsGenericType && clrType.GetGenericTypeDefinition() == typeof(Nullable<>)
                ? clrType.GetGenericArguments()[0]
                : clrType;

            return Type.GetTypeCode(actualType) switch
            {
                TypeCode.Decimal => true,
                _ => false
            };
        }

        internal static bool IsBoolean(this Type clrType, out Type actualType)
        {
            actualType = clrType.IsGenericType && clrType.GetGenericTypeDefinition() == typeof(Nullable<>)
                ? clrType.GetGenericArguments()[0]
                : clrType;

            return Type.GetTypeCode(actualType) switch
            {
                TypeCode.Boolean => true,
                _ => false
            };
        }

        internal static bool IsDateTime(this Type clrType, out Type actualType)
        {
            actualType = clrType.IsGenericType && clrType.GetGenericTypeDefinition() == typeof(Nullable<>)
                ? clrType.GetGenericArguments()[0]
                : clrType;

            return actualType == typeof(DateTime)
                || actualType == typeof(DateTimeOffset);
        }

        internal static bool IsString(this Type clrType) => typeof(string).Equals(clrType);

        internal static object Invoke(this
            InstanceInvoker invoker,
            object @this)
            => invoker.Invoke(@this, Array.Empty<object>());

        internal static T ValueOrThrow<T>(this IStructValue<T> ionType) where T : struct
        {
            if (ionType is null)
                throw new ArgumentNullException(nameof(ionType));

            return ionType.Value ?? throw new ArgumentNullException("ion type is null");
        }

        internal static T ValueOrThrow<T>(this IRefValue<T> ionType) where T : class
        {
            if (ionType is null)
                throw new ArgumentNullException(nameof(ionType));

            return ionType.Value ?? throw new ArgumentNullException("ion type is null");
        }

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
