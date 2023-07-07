using Axis.Luna.Common;
using Axis.Luna.Common.Results;
using Axis.Luna.Extensions;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using static Axis.Luna.Extensions.Common;

namespace Axis.Ion.Types
{
    /// <summary>
    /// List of supported operators
    /// </summary>
    public enum Operators
    {
        Exclamation = '!',
        Hash = '#',
        Percent = '%',
        Ampersand = '&',
        Star = '*',
        Plus = '+',
        Minus = '-',
        Dot = '.',
        FSlash = '/',
        SColon = ';',
        Less = '<',
        Equals = '=',
        Greater = '>',
        QMark = '?',
        At = '@',
        Caret = '^',
        BTick = '`',
        Pipe = '|',
        Tilde = '~'
    }

    /// <summary>
    /// NOTE: an operator is a SEQUENCE of ONE OR MORE operator characters.
    /// </summary>
    public readonly struct IonOperator : IRefValue<Operators[]>, IIonDeepCopyable<IonOperator>
    {
        private readonly IIonValue.Annotation[] _annotations;

        /// <summary>
        /// An empty operator array is not allowed
        /// </summary>
        /// <param name="operators"></param>
        /// <param name="annotations"></param>
        public IonOperator(Operators[]? operators, params IIonValue.Annotation[] annotations)
        {
            Value = operators.ThrowIf(
                ops => ops?.Length == 0,
                new ArgumentException("Empty operator array not allowed"));
            _annotations = annotations
                .Validate()
                .ToArray();
        }

        public IonOperator(params Operators[] operators)
            : this(operators, Array.Empty<IIonValue.Annotation>())
        {
        }

        /// <summary>
        /// Creates a null instance of the <see cref="IonOperator"/>
        /// </summary>
        /// <returns>The newly created null instance</returns>
        public static IonOperator Null() => new IonOperator();

        public string? AsString()
        {
            return Value?
                .Select(op => (char)op)
                .ToArray()
                .ApplyTo(chars => new string(chars));
        }

        #region IIonValueType
        public Operators[]? Value { get; }

        public bool ValueEquals(IRefValue<Operators[]> other) => Value.NullOrEquals(other.Value);
        #endregion

        #region IIonType

        public IonTypes Type => IonTypes.OperatorSymbol;

        public bool IsNull => Value == null;

        public IIonValue.Annotation[] Annotations => _annotations?.ToArray() ?? Array.Empty<IIonValue.Annotation>();

        public string ToIonText()
        {
            if (Value is null)
                return "null.symbol";

            return Value
                .Select(op => (char)op)
                .ToArray()
                .ApplyTo(charArray => new string(charArray));
        }
        #endregion

        #region Record Implementation
        public override int GetHashCode()
            => HashCode.Combine(
                ValueHash(Value.HardCast<Operators, object>()),
                ValueHash(Annotations.HardCast<IIonValue.Annotation, object>()));

        public override bool Equals(object? obj)
        {
            return obj is IonOperator other
                && other.Value.NullOrTrue(Value, Enumerable.SequenceEqual)
                && other.Annotations.NullOrTrue(Annotations, Enumerable.SequenceEqual);
        }

        public override string ToString() => Annotations
            .Select(a => a.ToString())
            .Concat(ToIonText())
            .JoinUsing("");


        public static bool operator ==(IonOperator first, IonOperator second) => first.Equals(second);

        public static bool operator !=(IonOperator first, IonOperator second) => !first.Equals(second);

        public static implicit operator IonOperator(Operators[] @operator) => new IonOperator(@operator);

        #endregion

        #region IIonDeepCopy<>
        IIonValue IIonDeepCopyable<IIonValue>.DeepCopy() => DeepCopy();

        public IonOperator DeepCopy() => new IonOperator(Value, Annotations);
        #endregion

        #region Parsing
        public static IResult<IonOperator> Parse(string? @string, params IIonValue.Annotation[] annotations)
        {
            if (@string is null)
                return Result.Of<IonOperator>(new ArgumentNullException(nameof(@string)));

            if (string.IsNullOrWhiteSpace(@string))
                return Result.Of<IonOperator>(new FormatException("Invalid input format"));

            IResult<IonOperator>[] results = @string
                .Trim()
                .Select(@char => (Operators)@char)
                .GroupBy(op => op.IsEnumDefined())
                .Select(group => group.Key switch
                {
                    true => Result.Of(new IonOperator(group.ToArray(), annotations)),
                    false => Result.Of<IonOperator>(
                        new FormatException(
                            $"Invalid operator symbols found: {group.Select(op => (char)op).JoinUsing(", ")}"))
                })
                .ToArray();

            return results.Length == 2
                ? results!.First(r => r is IResult<IonOperator>.ErrorResult)
                : results![0];
        }

        public static bool TryParse(
            string? @string,
            out IResult<IonOperator> symbolResult)
            => TryParse(@string, Array.Empty<IIonValue.Annotation>(), out symbolResult);

        public static bool TryParse(
            string? @string,
            IIonValue.Annotation[] annotations,
            out IResult<IonOperator> result)
            => (result = Parse(@string, annotations)) is IResult<IonOperator>.DataResult;
        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public readonly struct IonTextSymbol :
        IRefValue<string>,
        IIonDeepCopyable<IonTextSymbol>,
        IResultParsable<IonTextSymbol>,
        IIonNullable<IonTextSymbol>
    {
        private static readonly Regex IdentifierPattern = new Regex(
            "^[a-zA-Z_\\$][a-zA-Z0-9_\\$]*$",
            RegexOptions.Compiled);

        private readonly IIonValue.Annotation[] _annotations;

        public bool IsIdentifier => !IsNull && IdentifierPattern.IsMatch(Value!);

        public IonTextSymbol(IIonValue.Annotation[] annotations)
        : this(null, annotations)
        {
        }

        public IonTextSymbol(string? symbolText, params IIonValue.Annotation[] annotations)
        {
            Value =
                symbolText is null ? null :
                IdentifierPattern.IsMatch(symbolText) ? symbolText :
                Normalize(symbolText);

            _annotations = annotations.Validate().ToArray();
        }

        /// <summary>
        /// Creates a null instance of the <see cref="IonIdentifier"/>
        /// </summary>
        /// <returns>The newly created null instance</returns>
        public static IonTextSymbol Null(params IIonValue.Annotation[] annotations) => new IonTextSymbol(annotations);

        private static string? Normalize(string? symbolText)
        {
            if (string.IsNullOrWhiteSpace(symbolText))
                return symbolText;

            var normalized = symbolText.IsWrappedIn("'")
                ? symbolText[1..^1]
                : symbolText;

            if (normalized.Contains('\''))
                throw new FormatException($"Invalid quoted-symbol format: {symbolText}");

            return normalized;
        }

        #region IIonValueType

        public string? Value { get; }

        public bool ValueEquals(IRefValue<string> other) => Value.NullOrEquals(other.Value);
        #endregion

        #region IIonType
        public IonTypes Type => IonTypes.TextSymbol;

        public bool IsNull => Value == null;

        public IIonValue.Annotation[] Annotations => _annotations?.ToArray() ?? Array.Empty<IIonValue.Annotation>();

        public string ToIonText()
        {
            return
                Value is null ? "null.symbol" :
                IdentifierPattern.IsMatch(Value) ? Value :
                $"'{Value}'";
        }
        #endregion

        #region Record Implementation
        public override int GetHashCode()
            => HashCode.Combine(
                Value,
                ValueHash(Annotations.HardCast<IIonValue.Annotation, object>()));

        public override bool Equals(object? obj)
        {
            return obj is IonTextSymbol other
                && other.Value.NullOrEquals(Value)
                && other.Annotations.SequenceEqual(Annotations);
        }

        public override string ToString() => Annotations
            .Select(a => a.ToString())
            .Concat(ToIonText())
            .JoinUsing("");


        public static bool operator ==(IonTextSymbol first, IonTextSymbol second) => first.Equals(second);

        public static bool operator !=(IonTextSymbol first, IonTextSymbol second) => !first.Equals(second);

        public static implicit operator IonTextSymbol(string text) => Parse(text).Resolve();

        #endregion

        #region IIonDeepCopy<>
        IIonValue IIonDeepCopyable<IIonValue>.DeepCopy() => DeepCopy();

        public IonTextSymbol DeepCopy() => new IonTextSymbol(Value, Annotations);
        #endregion

        #region Parsing

        public static bool TryParse(
            string? @string,
            out IResult<IonTextSymbol> symbolResult)
            => TryParse(@string, Array.Empty<IIonValue.Annotation>(), out symbolResult);

        public static bool TryParse(
            string? @string,
            IIonValue.Annotation[] annotations,
            out IResult<IonTextSymbol> result)
            => (result = Parse(@string, annotations)) is IResult<IonTextSymbol>.DataResult;

        public static IResult<IonTextSymbol> Parse(
            string? @string)
            => Parse(@string, Array.Empty<IIonValue.Annotation>());

        public static IResult<IonTextSymbol> Parse(
            string? @string,
            params IIonValue.Annotation[] annotations)
        {
            if (@string is null)
                return Result.Of<IonTextSymbol>(new ArgumentNullException(nameof(@string)));

            if (string.IsNullOrWhiteSpace(@string))
                return Result.Of<IonTextSymbol>(new FormatException("Invalid symbol format"));

            return Result.Of(() => new IonTextSymbol(@string.Trim(), annotations));
        }
        #endregion
    }
}
