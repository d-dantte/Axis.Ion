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
    public readonly struct IonOperator : IIonValueType<Operators[]?>
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

        #region IIonValueType
        public Operators[]? Value { get; }

        public bool ValueEquals(IIonValueType<Operators[]?> other) => Value.NullOrEquals(other.Value);
        #endregion

        #region IIonType

        public IonTypes Type => IonTypes.OperatorSymbol;

        public bool IsNull => Value == null;

        public IIonType.Annotation[] Annotations => _annotations?.ToArray() ?? Array.Empty<IIonType.Annotation>();

        public string ToIonText()
        {
            if (Value is null)
                return "null.symbol"; // why was this an empty string?

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
            if (TryParse(@string, annotations, out IResult<IonOperator> result))
                return result.As<IResult<IonOperator>.DataResult>().Data;

            else throw result.As<IResult<IonOperator>.ErrorResult>().Cause();
        }

        public static bool TryParse(
            string? @string,
            IIonType.Annotation[] annotatinos,
            out IonOperator symbol)
        {
            if (TryParse(@string, annotatinos, out IResult<IonOperator> result))
            {
                symbol = result.As<IResult<IonOperator>.DataResult>().Data;
                return true;
            }

            symbol = default;
            return false;
        }

        public static bool TryParse(string? @string, out IonOperator symbol)
            => TryParse(@string, Array.Empty<IIonType.Annotation>(), out symbol);

        private static bool TryParse(
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

    public interface IIonTextSymbol: IIonValueType<string?>
    {
        public static IIonTextSymbol Parse(string value, params Annotation[] annotations)
        {
            if (IonQuotedSymbol.TryParse(value, annotations, out var quoted))
                return quoted;

            if (IonIdentifier.TryParse(value, annotations, out var identifier))
                return identifier;

            else throw new FormatException("Invalid symbol format");
        }

        public static bool TryParse(
            string value,
            Annotation[] annotations,
            out IIonTextSymbol? symbol)
        {
            try
            {
                symbol =
                    IonQuotedSymbol.TryParse(value, annotations, out var quoted) ? quoted :
                    IonIdentifier.TryParse(value, annotations, out var identifier) ? identifier :
                    (IIonTextSymbol?)null;

                return symbol is not null;
            }
            catch
            {
                symbol = default;
                return false;
            }
        }

        public static bool TryParse(string value, out IIonTextSymbol? symbol)
            => TryParse(value, Array.Empty<Annotation>(), out symbol);
    }

    /// <summary>
    /// Ion Identifier symbol - unquoted word characters
    /// </summary>
    public readonly struct IonIdentifier : IIonTextSymbol
    {
        private static readonly Regex _IdentifierPattern = new Regex(
            "^[a-zA-Z_\\$][a-zA-Z0-9_\\$]*$",
            RegexOptions.Compiled);

        private readonly IIonType.Annotation[] _annotations;


        public IonIdentifier(string? symbolText, params IIonType.Annotation[] annotations)
        {
            Value = symbolText;
            _annotations = annotations.Validate();
        }

        #region IIonValueType

        public string? Value { get; }

        public bool ValueEquals(IIonValueType<string?> other) => Value.NullOrEquals(other.Value);
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
            if (TryParse(@string, annotations, out IResult<IonIdentifier> result))
                return result.As<IResult<IonIdentifier>.DataResult>().Data;

            else throw result.As<IResult<IonIdentifier>.ErrorResult>().Cause();
        }

        public static bool TryParse(
            string? @string,
            IIonType.Annotation[] annotations,
            out IonIdentifier symbol)
        {
            if (TryParse(@string, annotations, out IResult<IonIdentifier> result))
            {
                symbol = result.As<IResult<IonIdentifier>.DataResult>().Data;
                return true;
            }

            symbol = default;
            return false;
        }

        public static bool TryParse(string? @string, out IonIdentifier symbol)
            => TryParse(@string, Array.Empty<IIonType.Annotation>(), out symbol);

        private static bool TryParse(
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
                result = Result.Of<IonIdentifier>(new FormatException("Invalid input format"));
                return false;
            }

            var trimmed = @string.Trim();
            if (!_IdentifierPattern.IsMatch(trimmed))
            {
                result = Result.Of<IonIdentifier>(new FormatException($"Invalid input format: {@string}"));
                return false;
            }

            result = Result.Of<IonIdentifier>(new IonIdentifier(trimmed, annotations));
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
            Value = symbolText;
            _annotations = annotations.Validate();
        }

        #region IIonValueType

        public string? Value { get; }

        public bool ValueEquals(IIonValueType<string?> other) => Value.NullOrEquals(other.Value);
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
            if (TryParse(@string, annotations, out IResult<IonQuotedSymbol> result))
                return result.As<IResult<IonQuotedSymbol>.DataResult>().Data;

            else throw result.As<IResult<IonQuotedSymbol>.ErrorResult>().Cause();
        }

        public static bool TryParse(
            string? @string,
            IIonType.Annotation[] annotations,
            out IonQuotedSymbol symbol)
        {
            if (TryParse(@string, annotations, out IResult<IonQuotedSymbol> result))
            {
                symbol = result.As<IResult<IonQuotedSymbol>.DataResult>().Data;
                return true;
            }

            symbol = default;
            return false;
        }

        public static bool TryParse(string? @string, out IonQuotedSymbol symbol)
            => TryParse(@string, Array.Empty<IIonType.Annotation>(), out symbol);

        private static bool TryParse(
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

            var trimmed = @string.Trim();
            if (!@string.StartsWith('\'') || !@string.EndsWith('\''))
            {
                result = Result.Of<IonQuotedSymbol>(new FormatException($"Invalid input format: {@string}"));
                return false;
            }

            trimmed = trimmed.UnwrapFrom("'");
            for (int i = 0; i < trimmed.Length; i++)
            {
                if (trimmed[i] == '\'')
                {
                    if (i == 0 || trimmed[i - 1] != '\\')
                    {
                        result = Result.Of<IonQuotedSymbol>(new FormatException($"Invalid input format: {@string}"));
                        return false;
                    }
                }
            }

            result = Result.Of<IonQuotedSymbol>(new IonQuotedSymbol(trimmed, annotations));
            return true;
        }
        #endregion
    }

}
