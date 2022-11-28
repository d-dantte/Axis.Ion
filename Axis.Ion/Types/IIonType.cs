using Axis.Luna.Extensions;
using System;
using System.Collections.Generic;

namespace Axis.Ion.Types
{
    public enum IonTypes
    {
        Null,
        Bool,
        Int,
        Float,
        Decimal,
        Timestamp,
        String,
        Symbol,
        Blob,
        Clob,
        Struct,
        List,
        Sexp
    }

    public interface IIonType
    {
        #region Of

        /// <summary>
        /// 
        /// </summary>
        /// <param name="initializer"></param>
        /// <returns></returns>
        public static IIonType OfNull(params IIonType.Annotation[] annotations) => new IonNull(annotations);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="initializer"></param>
        /// <returns></returns>
        public static IIonType Of(IonStruct.Initializer? initializer) => new IonStruct(initializer);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="initializer"></param>
        /// <returns></returns>
        public static IIonType Of(IonList.Initializer? initializer) => new IonList(initializer);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="initializer"></param>
        /// <returns></returns>
        public static IIonType Of(IonSexp.Initializer? initializer) => new IonSexp(initializer);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="initializer"></param>
        /// <returns></returns>
        public static IIonType Of(bool? value, params IIonType.Annotation[] annotations) => new IonBool(value, annotations);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="initializer"></param>
        /// <returns></returns>
        public static IIonType Of(long? value, params IIonType.Annotation[] annotations) => new IonInt(value, annotations);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="initializer"></param>
        /// <returns></returns>
        public static IIonType Of(double? value, params IIonType.Annotation[] annotations) => new IonFloat(value, annotations);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="initializer"></param>
        /// <returns></returns>
        public static IIonType Of(decimal? value, params IIonType.Annotation[] annotations) => new IonDecimal(value, annotations);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="initializer"></param>
        /// <returns></returns>
        public static IIonType Of(DateTimeOffset? value, params IIonType.Annotation[] annotations) => new IonTimestamp(value, annotations);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="initializer"></param>
        /// <returns></returns>
        public static IIonType Of(string? value, params IIonType.Annotation[] annotations) => new IonString(value, annotations);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="initializer"></param>
        /// <returns></returns>
        public static IIonType OfSymbol(string? value, params IIonType.Annotation[] annotations) => IIonSymbol.Of(value, annotations);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="initializer"></param>
        /// <returns></returns>
        public static IIonType OfSymbol(IIonSymbol.Operators[] value, params IIonType.Annotation[] annotations) => IIonSymbol.OfOperator(value, annotations);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="initializer"></param>
        /// <returns></returns>
        public static IIonType OfBlob(byte[]? value, params IIonType.Annotation[] annotations) => new IonBlob(value, annotations);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="initializer"></param>
        /// <returns></returns>
        public static IIonType OfClob(byte[]? value, params IIonType.Annotation[] annotations) => new IonClob(value, annotations);
        #endregion

        #region Members

        /// <summary>
        /// 
        /// </summary>
        IonTypes Type { get; }

        /// <summary>
        /// 
        /// </summary>
        Annotation[] Annotations { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        string ToIonText();

        #endregion


        #region Nested types

        /// <summary>
        /// 
        /// </summary>
        public readonly struct Annotation
        {
            public string Value { get; }

            internal Annotation(string value)
            {
                Value = value;
            }

            public override int GetHashCode() => HashCode.Combine(Value);

            public override bool Equals(object? obj)
            {
                return obj is Annotation other
                    && other.Value.NullOrEquals(Value);
            }

            public override string ToString() => $"{Value}::";


            public static bool operator ==(Annotation first, Annotation second) => first.Equals(second);

            public static bool operator !=(Annotation first, Annotation second) => !first.Equals(second);


            public static implicit operator Annotation(string annotation) => Parse(annotation);

            #region Parse
            public static bool TryParse(string @string, out Annotation annotation)
            {
                var normalized = @string?.TrimEnd("::") ?? "";
                if (IIonSymbol.QuotedSymbol.TryParse(normalized, out var quoted))
                {
                    annotation = new Annotation(quoted.ToIonText());
                    return true;
                }

                if(IIonSymbol.Identifier.TryParse(normalized, out var identifier))
                {
                    annotation = new Annotation(identifier.ToIonText());
                    return true;
                }

                annotation = default;
                return false;
            }

            public static Annotation Parse(string @string)
            {
                if (TryParse(@string, out Annotation annotation))
                    return annotation;

                throw new FormatException($"Invalid format: {@string}");
            }

            public static bool TryParseCollection(string @string, out Annotation[]? annotations)
            {
                annotations = null;
                if (string.IsNullOrWhiteSpace(@string))
                    return false;

                var annotationList = new List<Annotation>();
                foreach(var part in @string.Split("::", StringSplitOptions.RemoveEmptyEntries))
                {
                    if (TryParse(part, out Annotation annotation))
                        annotationList.Add(annotation);

                    else return false;
                }

                annotations = annotationList.ToArray();
                return true;
            }

            public static Annotation[] ParseCollection(string @string)
            {
                if (TryParseCollection(@string, out Annotation[]? annotations))
                    return annotations ?? throw new Exception("Unknown");

                throw new FormatException($"Invalid format: {@string}");
            }
            #endregion
        }

        #endregion
    }

    public interface IIonValueType<TValue>: IIonType
    {
        TValue Value { get; }

        bool ValueEquals(IIonValueType<TValue> other);
    }

    public interface IIonConainer<TValue>: IIonValueType<TValue[]?>
    {
    }
}
