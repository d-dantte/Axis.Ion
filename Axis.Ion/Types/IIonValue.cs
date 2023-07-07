using Axis.Luna.Extensions;
using Axis.Luna.Common;
using System;
using System.Linq;
using Axis.Luna.Common.Results;

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
        TextSymbol,
        Blob,
        Clob,
        List,
        Sexp,
        Struct
    }

    /// <summary>
    /// The base for all ion values
    /// </summary>
    public interface IIonValue: IIonDeepCopyable<IIonValue>
    {
        #region NullOf
        /// <summary>
        /// Creates null values for the given <see cref="IonTypes"/>, and annotations
        /// </summary>
        /// <param name="type">The ion type</param>
        /// <param name="annotations">The list of annotations</param>
        /// <returns>The null value</returns>
        /// <exception cref="ArgumentException">If the ion type is invalid</exception>
        public static IIonValue NullOf(IonTypes type, params Annotation[] annotations)
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
                IonTypes.TextSymbol => IonTextSymbol.Null(),
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
        /// The textual representation of this value alone, without the annotations
        /// </summary>
        string ToIonText();
        #endregion

        #region Nested types

        public readonly struct Annotation: IResultParsable<Annotation>
        {
            public string Value { get; }

            internal Annotation(string value)
            {
                Value = IonTextSymbol
                    .Parse(value)
                    .Resolve()
                    .Value!;
            }

            public Annotation(IonTextSymbol symbol)
            {
                if (symbol.IsNull)
                    throw new ArgumentException("Invalid symbol");

                Value = symbol.ToIonText();
            }

            public IonTextSymbol ToSymbol() => new IonTextSymbol(Value);

            public override int GetHashCode() => HashCode.Combine(Value);

            public override bool Equals(object? obj)
            {
                return obj is Annotation other
                    && other.Value.NullOrEquals(Value);
            }

            public override string ToString() => $"{Value}::";


            public static bool operator ==(Annotation first, Annotation second) => first.Equals(second);

            public static bool operator !=(Annotation first, Annotation second) => !first.Equals(second);


            public static implicit operator Annotation(string annotation) => new Annotation(annotation);

            public static implicit operator Annotation(IonTextSymbol symbol) => new Annotation(symbol);

            #region Parse

            public static bool TryParse(
                string @string,
                out IResult<Annotation> result)
                => (result = Parse(@string)) is IResult<Annotation>.DataResult;

            public static bool TryParseCollection(
                string @string,
                out IResult<Annotation[]> result)
                => (result = ParseCollection(@string)) is IResult<Annotation[]>.DataResult;

            public static IResult<Annotation[]> ParseCollection(string @string)
            {
                return @string?
                    .Split("::", StringSplitOptions.RemoveEmptyEntries)
                    .Select(Parse)
                    .Fold()
                    .Map(annotations => annotations.ToArray())
                    ?? Result.Of<Annotation[]>(new ArgumentNullException(nameof(@string)));
            }

            public static IResult<Annotation> Parse(string @string)
            {
                var normalized = @string?.TrimEnd("::") ?? "";
                var result = IonTextSymbol.Parse(normalized);
                return result.Map(symbol => new Annotation(symbol));
            }
            #endregion
        }

        #endregion
    }

    public interface IIonNullable<TSelf> where TSelf : IIonNullable<TSelf>
    {
        /// <summary>
        /// Creates a null instance of the type
        /// </summary>
        /// <param name="annotations">any available annotation</param>
        /// <returns>The newly created null instance</returns>
        static abstract TSelf Null(IIonValue.Annotation[] annotations);
    }


    /// <summary>
    /// Represents an <see cref="IIonValue"/> that encapsulates a clr value-type
    /// </summary>
    /// <typeparam name="TValue">the encapsulated value</typeparam>
    public interface IStructValue<TValue>: IIonValue
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
    /// Represents an <see cref="IIonValue"/> that encapsulates a clr ref-type
    /// </summary>
    /// <typeparam name="TValue">the encapsulated value</typeparam>
    public interface IRefValue<TValue>: IIonValue
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
    public interface IIonContainer<TValue>: IRefValue<TValue[]>
    {
        /// <summary>
        /// Creates a new, empty version of this container
        /// </summary>
        abstract static IIonContainer<TValue> Empty(params Annotation[] annotations);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TIon"></typeparam>
    public interface IIonDeepCopyable<TIon>
    where TIon : IIonDeepCopyable<TIon>
    {
        TIon DeepCopy();
    }
}
