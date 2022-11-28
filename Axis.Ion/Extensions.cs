using Axis.Ion.Types;
using Axis.Ion.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using static Axis.Ion.Types.IIonType;

namespace Axis.Ion
{
    /// <summary>
    /// See https://github.com/amzn/ion-dotnet/blob/ff73b85e203712eaff7c7e51a6cb94e606b4a82e/Amazon.IonDotnet/Utils/BitConverterEx.cs#L20
    /// </summary>
    internal static class Extensions
    {
        #region remove these
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe int ToInt32Bits(this float value)
        {
            return *((int*)&value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe float ToSingle(this int value)
        {
            return *((float*)&value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe long ToInt64Bits(this double value)
        {
            return *((long*)&value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe double ToDouble(this long value)
        {
            return *((double*)&value);
        }
        #endregion

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
            if (IIonSymbol.Identifier.TryParse(propertyName, out _))
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
    }
}
