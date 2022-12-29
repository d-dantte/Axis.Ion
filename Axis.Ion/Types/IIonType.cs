using Axis.Luna.Extensions;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Axis.Ion.Types
{
    public enum IonTypes
    {
        Null = 1,
        Bool,
        Int,
        Decimal,
        Float,
        Timestamp,
        String,
        OperatorSymbol,
        IdentifierSymbol,
        QuotedSymbol,
        Blob,
        Clob,
        List,
        Sexp,
        Struct
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
        public static IIonType Of(BigInteger? value, params IIonType.Annotation[] annotations) => new IonInt(value, annotations);

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
        public static IIonType OfString(string? value, params IIonType.Annotation[] annotations) => new IonString(value, annotations);

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

        #region NullOf

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="annotations"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static IIonType NullOf(IonTypes type, params Annotation[] annotations)
        {
            return type switch
            {
                IonTypes.Null => new IonNull(annotations),
                IonTypes.Bool => new IonBool(null, annotations),
                IonTypes.Int => new IonInt(null, annotations),
                IonTypes.Decimal => new IonDecimal(null, annotations),
                IonTypes.Float => new IonFloat(null, annotations),
                IonTypes.Timestamp => new IonTimestamp(null, annotations),
                IonTypes.String => new IonString(null, annotations),
                IonTypes.OperatorSymbol => new IIonSymbol.Operator(null, annotations),
                IonTypes.IdentifierSymbol => new IIonSymbol.Identifier(null, annotations),
                IonTypes.QuotedSymbol => new IIonSymbol.QuotedSymbol(null, annotations),
                IonTypes.Blob => new IonBlob(null, annotations),
                IonTypes.Clob => new IonClob(null, annotations),
                IonTypes.List => new IonList(annotations),
                IonTypes.Sexp => new IonSexp(annotations),
                IonTypes.Struct => new IonStruct(annotations),
                _ => throw new ArgumentException($"Invalid {typeof(IonTypes)} value: {type}")
            };
        }
        #endregion

        #region Members

        /// <summary>
        /// Indicating if the value is null (default).
        /// </summary>
        bool IsNull { get; }

        /// <summary>
        /// The <see cref="IonTypes"/>
        /// </summary>
        IonTypes Type { get; }

        /// <summary>
        /// The annotation list
        /// </summary>
        Annotation[] Annotations { get; }

        /// <summary>
        /// The textual representation of this value
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

            public Annotation(IIonSymbol? symbol)
            {
                if (symbol is IIonSymbol.Operator 
                    || symbol is null
                    || symbol.IsNull)
                    throw new ArgumentException("Invalid symbol");

                Value = symbol.ToIonText();
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

            public static implicit operator Annotation(IIonSymbol.Identifier symbol) => new Annotation(symbol);

            public static implicit operator Annotation(IIonSymbol.QuotedSymbol symbol) => new Annotation(symbol);

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
