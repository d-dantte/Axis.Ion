using Axis.Luna.Extensions;
using System;
using System.Collections.Generic;

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
        #region NullOf

        /// <summary>
        /// Creates null values for the given <see cref="IonTypes"/>, and annotations
        /// </summary>
        /// <param name="type">The ion type</param>
        /// <param name="annotations">The list of annotations</param>
        /// <returns>The null value</returns>
        /// <exception cref="ArgumentException">If the ion type is invalid</exception>
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
                IonTypes.OperatorSymbol => new IonOperator(null, annotations),
                IonTypes.IdentifierSymbol => new IonIdentifier(null, annotations),
                IonTypes.QuotedSymbol => new IonQuotedSymbol(null, annotations),
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
        /// The <see cref="IonTypes"/>
        /// </summary>
        IonTypes Type { get; }

        /// <summary>
        /// Indicating if the value is null (default).
        /// </summary>
        bool IsNull { get; }

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

        public readonly struct Annotation
        {
            public string Value { get; }

            internal Annotation(string value)
            {
                Value = value.ThrowIf(
                    string.IsNullOrWhiteSpace,
                    new ArgumentException(nameof(value)));
            }

            public Annotation(IonIdentifier symbol)
            {
                if (symbol.IsNull)
                    throw new ArgumentException("Invalid symbol");

                Value = symbol.ToIonText();
            }

            public Annotation(IonQuotedSymbol symbol)
            {
                if (symbol.IsNull)
                    throw new ArgumentException("Invalid symbol");

                Value = symbol.ToIonText();
            }

            public IIonTextSymbol ToSymbol()
            {
                if (Value.StartsWith("'"))
                    return IonQuotedSymbol.Parse(Value);

                else return IonIdentifier.Parse(Value);
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

            public static implicit operator Annotation(IonIdentifier symbol) => new Annotation(symbol);

            public static implicit operator Annotation(IonQuotedSymbol symbol) => new Annotation(symbol);

            #region Parse
            public static bool TryParse(string @string, out Annotation annotation)
            {
                annotation = default;
                var normalized = @string?.TrimEnd("::") ?? "";
                if (IIonTextSymbol.TryParse(normalized, out var symbol))
                {
                    if (symbol == null)
                        return false;

                    annotation = new Annotation(symbol.ToIonText());
                    return true;
                }
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
