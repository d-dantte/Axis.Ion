using Axis.Luna.Extensions;
using Axis.Luna.Common;
using System;
using System.Linq;

namespace Axis.Ion.Types
{
    /// <summary>
    /// The finite set of ion types
    /// </summary>
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

    /// <summary>
    /// The base for all ion values
    /// </summary>
    public interface IIonType: IIonDeepCopyable<IIonType>
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
                IonTypes.Null => IonNull.Null(annotations),
                IonTypes.Bool => IonBool.Null(annotations),
                IonTypes.Int => IonInt.Null(annotations),
                IonTypes.Decimal => IonDecimal.Null(annotations),
                IonTypes.Float => IonFloat.Null(annotations),
                IonTypes.Timestamp => IonTimestamp.Null(annotations),
                IonTypes.String => IonString.Null(annotations),
                IonTypes.OperatorSymbol => IonOperator.Null(),
                IonTypes.IdentifierSymbol => IonIdentifier.Null(),
                IonTypes.QuotedSymbol => IonQuotedSymbol.Null(),
                IonTypes.Blob => IonBlob.Null(annotations),
                IonTypes.Clob => IonClob.Null(annotations),
                IonTypes.List => IonList.Null(annotations),
                IonTypes.Sexp => IonSexp.Null(annotations),
                IonTypes.Struct => IonStruct.Null(annotations),
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

            public static Annotation Parse(string @string)
            {
                _ = TryParse(@string, out var annotation);
                return annotation.Resolve();
            }

            public static Annotation[] ParseCollection(string @string)
            {
                _ = TryParseCollection(@string, out var result);
                return result.Resolve();
            }

            public static bool TryParse(string @string, out IResult<Annotation> annotationResult)
            {
                var normalized = @string?.TrimEnd("::") ?? "";
                _ = IIonTextSymbol.TryParse(normalized, out var result);
                annotationResult = result.Map(symbol => new Annotation(symbol.ToIonText()));

                return annotationResult is IResult<Annotation>.DataResult;
            }

            public static bool TryParseCollection(string @string, out IResult<Annotation[]> annotationsResult)
            {
                annotationsResult = @string?
                    .Split("::", StringSplitOptions.RemoveEmptyEntries)
                    .Select(part =>
                    {
                        _ = TryParse(part, out var result);
                        return result;
                    })
                    .Fold()
                    .Map(annotations => annotations.ToArray())
                    ?? Result.Of<Annotation[]>(new ArgumentNullException(nameof(@string)));

                return annotationsResult is IResult<Annotation[]>.DataResult;
            }
            #endregion
        }

        #endregion
    }

    /// <summary>
    /// Represents an <see cref="IIonType"/> that encapsulates a clr value-type
    /// </summary>
    /// <typeparam name="TValue">the encapsulated value</typeparam>
    public interface IStructValue<TValue>: IIonType
    where TValue: struct
    {
        /// <summary>
        /// The encapsulated value
        /// </summary>
        TValue? Value { get; }

        /// <summary>
        /// Value equality test.
        /// </summary>
        /// <param name="other">The other <see cref="IStructValue{TValue}"/> to test for equality</param>
        /// <returns>True if the given <see cref="IStructValue{TValue}"/> encapsulates a value equal to the current instance's value, false otherwise</returns>
        bool ValueEquals(IStructValue<TValue> other);
    }

    /// <summary>
    /// Represents an <see cref="IIonType"/> that encapsulates a clr ref-type
    /// </summary>
    /// <typeparam name="TValue">the encapsulated value</typeparam>
    public interface IRefValue<TValue>: IIonType
    where TValue: class
    {
        /// <summary>
        /// The encapsulated value
        /// </summary>
        TValue? Value { get; }

        /// <summary>
        /// Value equality test.
        /// </summary>
        /// <param name="other">The other <see cref="IRefValue{TValue}{TValue}"/> to test for equality</param>
        /// <returns>True if the given <see cref="IRefValue{TValue}"/> encapsulates a value equal to the current instance's value, false otherwise</returns>
        bool ValueEquals(IRefValue<TValue> other);
    }

    /// <summary>
    /// Represents an <see cref="IStructValue{TValue}"/> whose value is a collection (array)
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public interface IIonConainer<TValue>: IRefValue<TValue[]>
    {
    }

    public interface IIonDeepCopyable<TIon>
    where TIon : IIonDeepCopyable<TIon>
    {
        TIon DeepCopy();
    }
}
