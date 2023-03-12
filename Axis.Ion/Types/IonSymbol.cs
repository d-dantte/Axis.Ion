using Axis.Luna.Common;
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
    public readonly struct IonOperator : IRefValue<Operators[]>
    {
        private readonly IIonType.Annotation[] _annotations;

        /// <summary>
        /// An empty operator array is not allowed
        /// </summary>
        /// <param name="operators"></param>
        /// <param name="annotations"></param>
        public IonOperator(Operators[]? operators, params IIonType.Annotation[] annotations)
        {
            Value = operators.ThrowIf(
                ops => ops?.Length == 0,
                new ArgumentException("Empty operator array not allowed"));
            _annotations = annotations
                .Validate()
                .ToArray();
        }

        public IonOperator(params Operators[] operators)
            : this(operators, Array.Empty<IIonType.Annotation>())
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

        public IIonType.Annotation[] Annotations => _annotations?.ToArray() ?? Array.Empty<IIonType.Annotation>();

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
                ValueHash(Annotations.HardCast<IIonType.Annotation, object>()));

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

        public static implicit operator IonOperator(Operators[]? @operator) => new IonOperator(@operator);

        #endregion

        #region Parsing
        public static IonOperator Parse(string? @string, params IIonType.Annotation[] annotations)
        {
            _ = TryParse(@string, annotations, out var result);
            return result.Resolve();
        }

        public static bool TryParse(
            string? @string,
            out IResult<IonOperator> symbolResult)
            => TryParse(@string, Array.Empty<IIonType.Annotation>(), out symbolResult);

        public static bool TryParse(
            string? @string,
            IIonType.Annotation[] annotations,
            out IResult<IonOperator> result)
        {
            if (@string is null)
            {
                result = Result.Of<IonOperator>(new ArgumentNullException(nameof(@string)));
                return false;
            }

            if (string.IsNullOrWhiteSpace(@string))
            {
                result = Result.Of<IonOperator>(new FormatException("Invalid input format"));
                return false;
            }

            var results = @string
                .Trim()
                .Select(@char => (Operators)@char)
                .GroupBy(op => op.IsEnumDefined())
                .Select(group => group.Key switch
                {
                    true => Result.Of<IonOperator>(new IonOperator(group.ToArray(), annotations)),
                    false => Result.Of<IonOperator>(
                        new FormatException(
                            $"Invalid operator symbols found: {group.Select(op => (char)op).JoinUsing(", ")}"))
                })
                .ToArray();

            result = results.Length == 2
                ? results.FirstOrDefault(r => r is IResult<IonOperator>.ErrorResult)
                : results[0];

            return result is IResult<IonOperator>.DataResult;
        }
        #endregion
    }

    public interface IIonTextSymbol: IRefValue<string>
    {
        public static IIonTextSymbol Parse(string value, params Annotation[] annotations)
        {
            _ = TryParse(value, annotations, out var result);
            return result.Resolve();
        }

        public static bool TryParse(
            string symbol,
            Annotation[] annotations,
            out IResult<IIonTextSymbol> symbolResult)
        {
            try
            {
                if (IonIdentifier.TryParse(symbol, annotations, out var identifierResult))
                    symbolResult = identifierResult.Map<IIonTextSymbol>(identifier => identifier);

                else if (IonQuotedSymbol.TryParse(symbol, annotations, out var quotedSymbolResult))
                    symbolResult = quotedSymbolResult.Map<IIonTextSymbol>(identifier => identifier);

                else
                    symbolResult = Result.Of<IIonTextSymbol>(new FormatException($"Invalid symbol format: {symbol}"));

                return symbolResult is IResult<IIonTextSymbol>.DataResult;
            }
            catch (Exception ex)
            {
                symbolResult = Result.Of<IIonTextSymbol>(ex);
                return false;
            }
        }

        public static bool TryParse(string value, out IResult<IIonTextSymbol> symbolResult)
            => TryParse(value, Array.Empty<Annotation>(), out symbolResult);
    }

    /// <summary>
    /// Ion Identifier symbol - unquoted word characters
    /// </summary>
    public readonly struct IonIdentifier : IIonTextSymbol
    {
        private static readonly Regex IdentifierPattern = new Regex(
            "^[a-zA-Z_\\$][a-zA-Z0-9_\\$]*$",
            RegexOptions.Compiled);

        private readonly IIonType.Annotation[] _annotations;

        public IonIdentifier(string? symbolText, params IIonType.Annotation[] annotations)
        {
            Value = 
                symbolText == null ? null :
                IdentifierPattern.IsMatch(symbolText) ? symbolText :
                throw new FormatException($"Invalid identifier format: {symbolText}");

            _annotations = annotations.Validate();
        }

        /// <summary>
        /// Creates a null instance of the <see cref="IonIdentifier"/>
        /// </summary>
        /// <returns>The newly created null instance</returns>
        public static IonIdentifier Null() => new IonIdentifier();

        #region IIonValueType

        public string? Value { get; }

        public bool ValueEquals(IRefValue<string> other) => Value.NullOrEquals(other.Value);
        #endregion

        #region IIonType
        public IonTypes Type => IonTypes.IdentifierSymbol;

        public bool IsNull => Value == null;

        public IIonType.Annotation[] Annotations => _annotations?.ToArray() ?? Array.Empty<IIonType.Annotation>();

        public string ToIonText() => Value ?? "null.symbol";
        #endregion

        #region Record Implementation
        public override int GetHashCode()
            => HashCode.Combine(
                Value,
                ValueHash(Annotations.HardCast<IIonType.Annotation, object>()));

        public override bool Equals(object? obj)
        {
            return obj is IonIdentifier other
                && other.Value.NullOrEquals(Value)
                && other.Annotations.SequenceEqual(Annotations);
        }

        public override string ToString() => Annotations
            .Select(a => a.ToString())
            .Concat(ToIonText())
            .JoinUsing("");


        public static bool operator ==(IonIdentifier first, IonIdentifier second) => first.Equals(second);

        public static bool operator !=(IonIdentifier first, IonIdentifier second) => !first.Equals(second);

        public static implicit operator IonIdentifier(string text) => Parse(text);

        #endregion

        #region Parsing
        public static IonIdentifier Parse(string? @string, params IIonType.Annotation[] annotations)
        {
            _ = TryParse(@string, annotations, out var result);
            return result.Resolve();
        }

        public static bool TryParse(
            string? @string,
            out IResult<IonIdentifier> symbolResult)
            => TryParse(@string, Array.Empty<IIonType.Annotation>(), out symbolResult);

        public static bool TryParse(
            string? @string,
            IIonType.Annotation[] annotations,
            out IResult<IonIdentifier> result)
        {
            if (@string is null)
            {
                result = Result.Of<IonIdentifier>(new ArgumentNullException(nameof(@string)));
                return false;
            }

            if (string.IsNullOrWhiteSpace(@string))
            {
                result = Result.Of<IonIdentifier>(new FormatException("Invalid identifier format"));
                return false;
            }

            var trimmed = @string.Trim();
            if (!IdentifierPattern.IsMatch(trimmed))
            {
                result = Result.Of<IonIdentifier>(new FormatException($"Invalid identifier format: {@string}"));
                return false;
            }

            result = Result.Of(new IonIdentifier(trimmed, annotations));
            return true;
        }
        #endregion
    }

    /// <summary>
    /// Quoted symbol.
    /// </summary>
    public readonly struct IonQuotedSymbol : IIonTextSymbol
    {
        private readonly IIonType.Annotation[] _annotations;

        public IonQuotedSymbol(string? symbolText, params IIonType.Annotation[] annotations)
        {
            Value = Normalize(symbolText);
            _annotations = annotations.Validate();
        }

        private static string? Normalize(string? symbolText)
        {
            if (string.IsNullOrWhiteSpace(symbolText))
                return symbolText;

            var normalized = symbolText.StartsWith('\'') && symbolText.EndsWith('\'')
                ? symbolText[1..^1]
                : symbolText;

            if (normalized.Contains('\''))
                throw new FormatException($"Invalid quoted-symbol format: {symbolText}");

            return normalized;
        }

        /// <summary>
        /// Creates a null instance of the <see cref="IonQuotedSymbol"/>
        /// </summary>
        /// <returns>The newly created null instance</returns>
        public static IonQuotedSymbol Null() => new IonQuotedSymbol();

        #region IIonValueType

        public string? Value { get; }

        public bool ValueEquals(IRefValue<string> other) => Value.NullOrEquals(other.Value);
        #endregion

        #region IIonType

        public IonTypes Type => IonTypes.QuotedSymbol;

        public bool IsNull => Value == null;

        public IIonType.Annotation[] Annotations => _annotations?.ToArray() ?? Array.Empty<IIonType.Annotation>();

        public string ToIonText()
        {
            if (Value is null)
                return "null.symbol";

            return $"'{Value}'";
        }
        #endregion

        #region Record Implementation
        public override int GetHashCode()
            => HashCode.Combine(
                Value,
                ValueHash(Annotations.HardCast<IIonType.Annotation, object>()));

        public override bool Equals(object? obj)
        {
            return obj is IonQuotedSymbol other
                && other.Value.NullOrEquals(Value)
                && other.Annotations.SequenceEqual(Annotations);
        }

        public override string ToString() => Annotations
            .Select(a => a.ToString())
            .Concat(ToIonText())
            .JoinUsing("");


        public static bool operator ==(IonQuotedSymbol first, IonQuotedSymbol second) => first.Equals(second);

        public static bool operator !=(IonQuotedSymbol first, IonQuotedSymbol second) => !first.Equals(second);

        public static implicit operator IonQuotedSymbol(string text) => Parse(text);

        #endregion

        #region Parsing
        public static IonQuotedSymbol Parse(string? @string, params IIonType.Annotation[] annotations)
        {
            _ = TryParse(@string, annotations, out var result);
            return result.Resolve();
        }

        public static bool TryParse(
            string? @string,
            out IResult<IonQuotedSymbol> symbolResult)
            => TryParse(@string, Array.Empty<IIonType.Annotation>(), out symbolResult);

        public static bool TryParse(
            string? @string,
            IIonType.Annotation[] annotations,
            out IResult<IonQuotedSymbol> result)
        {
            if (@string is null)
            {
                result = Result.Of<IonQuotedSymbol>(new ArgumentNullException(nameof(@string)));
                return false;
            }

            if (string.IsNullOrWhiteSpace(@string))
            {
                result = Result.Of<IonQuotedSymbol>(new FormatException("Invalid input format"));
                return false;
            }

            result = Result.Of(() => new IonQuotedSymbol(@string, annotations));
            return result is IResult<IonQuotedSymbol>.DataResult;
        }
        #endregion
    }

}
