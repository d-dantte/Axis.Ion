using Axis.Luna.Common;
using Axis.Luna.Extensions;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using static Axis.Luna.Extensions.Common;

namespace Axis.Ion.Types
{
    public interface IIonSymbol: IIonType
    {
        #region Of

        public static IIonSymbol Of(string? symbol)
        {
            if ("null.symbol".Equals(symbol))
                return default(Identifier);

            if ("null".Equals(symbol))
                return default(Identifier);

            if (Operator.TryParse(symbol, out var @operator))
                return @operator;

            if (QuotedSymbol.TryParse(symbol, out var quotedSymbol))
                return quotedSymbol;

            if (Identifier.TryParse(symbol, out var identifier))
                return identifier;

            throw new FormatException($"Invalid format: {symbol}");
        }

        /// <summary>
        /// Creates a quoted symbol
        /// </summary>
        /// <param name="symbol">the symbol text</param>
        /// <returns>The <see cref="QuotedSymbol"/> instance</returns>
        public static IIonSymbol OfQuoted(string? symbol, params Annotation[] annotation) => new QuotedSymbol(symbol, annotation);

        /// <summary>
        /// Creates a identifier symbol
        /// </summary>
        /// <param name="symbol">the symbol text</param>
        /// <returns>The <see cref="Identifier"/> instance</returns>
        public static IIonSymbol OfIdentifier(string? symbol, params Annotation[] annotation) => new Identifier(symbol, annotation);

        /// <summary>
        /// Creates a operator symbol
        /// </summary>
        /// <param name="operator">the symbol text</param>
        /// <returns>The <see cref="Operator"/> instance</returns>
        public static IIonSymbol OfOperator(Operators[] @operator, params Annotation[] annotation) => new Operator(@operator, annotation);

        #endregion

        #region Members

        #endregion

        #region Nested types

        /// <summary>
        /// 
        /// </summary>
        public readonly struct QuotedSymbol: IIonSymbol
        {
            private readonly Annotation[] _annotations;

            public string? Symbol { get; }

            public IonTypes Type => IonTypes.Symbol;

            public Annotation[] Annotations => _annotations?.ToArray() ?? Array.Empty<Annotation>();

            internal QuotedSymbol(string? symbolText, params Annotation[] annotations)
            {
                Symbol = symbolText;
                _annotations = annotations.Validate();
            }

            #region IIonSymbol
            public string ToIonText()
            {
                if (Symbol is null)
                    return "null.symbol";

                return $"'{Symbol}'";
            }
            #endregion

            #region Record Implementation
            public override int GetHashCode()
                => HashCode.Combine(
                    Symbol,
                    ValueHash(Annotations.HardCast<Annotation, object>()));

            public override bool Equals(object? obj)
            {
                return obj is QuotedSymbol other
                    && other.Symbol.NullOrEquals(Symbol)
                    && other.Annotations.SequenceEqual(Annotations);
            }

            public override string ToString() => Annotations
                .Select(a => a.ToString())
                .Concat(ToIonText())
                .JoinUsing("");


            public static bool operator ==(QuotedSymbol first, QuotedSymbol second) => first.Equals(second);

            public static bool operator !=(QuotedSymbol first, QuotedSymbol second) => !first.Equals(second);

            public static implicit operator QuotedSymbol(string text) => Parse(text);

            #endregion

            #region Parsing
            public static QuotedSymbol Parse(string? @string)
            {
                if (TryParse(@string, out IResult<QuotedSymbol> result))
                    return result.As<IResult<QuotedSymbol>.DataResult>().Data;

                else throw result.As<IResult<QuotedSymbol>.ErrorResult>().Cause();
            }

            public static bool TryParse(string? @string, out QuotedSymbol symbol)
            {
                if (TryParse(@string, out IResult<QuotedSymbol> result))
                {
                    symbol = result.As<IResult<QuotedSymbol>.DataResult>().Data;
                    return true;
                }

                symbol = default;
                return false;
            }

            private static bool TryParse(string? @string, out IResult<QuotedSymbol> result)
            {
                if (@string is null)
                {
                    result = IResult<QuotedSymbol>.Of(new ArgumentNullException(nameof(@string)));
                    return false;
                }

                if (string.IsNullOrWhiteSpace(@string))
                {
                    result = IResult<QuotedSymbol>.Of(new FormatException("Invalid input format"));
                    return false;
                }

                var trimmed = @string.Trim();
                if(!@string.StartsWith('\'') || !@string.EndsWith('\''))
                {
                    result = IResult<QuotedSymbol>.Of(new FormatException($"Invalid input format: {@string}"));
                    return false;
                }

                trimmed = trimmed.UnwrapFrom("'");
                for(int i = 0; i < trimmed.Length; i++)
                {
                    if(trimmed[i] == '\'')
                    {
                        if(i == 0 || trimmed[i-1] != '\\')
                        {
                            result = IResult<QuotedSymbol>.Of(new FormatException($"Invalid input format: {@string}"));
                            return false;
                        }
                    }
                }

                result = IResult<QuotedSymbol>.Of(new QuotedSymbol(trimmed));
                return true;
            }
            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        public readonly struct Identifier : IIonSymbol
        {
            private readonly Annotation[] _annotations;
            private static readonly Regex _IdentifierPattern = new Regex("^[a-zA-Z_\\$][a-zA-Z0-9_\\$]*$", RegexOptions.Compiled);

            public string? Symbol { get; }

            public IonTypes Type => IonTypes.Symbol;

            public Annotation[] Annotations => _annotations?.ToArray() ?? Array.Empty<Annotation>();

            internal Identifier(string? symbolText, params Annotation[] annotations)
            {
                Symbol = symbolText;
                _annotations = annotations.Validate();
            }

            #region IIonSymbol
            public string ToIonText() => Symbol ?? "null.symbol";
            #endregion

            #region Record Implementation
            public override int GetHashCode()
                => HashCode.Combine(
                    Symbol,
                    ValueHash(Annotations.HardCast<Annotation, object>()));

            public override bool Equals(object? obj)
            {
                return obj is Identifier other
                    && other.Symbol.NullOrEquals(Symbol)
                    && other.Annotations.SequenceEqual(Annotations);
            }

            public override string ToString() => Annotations
                .Select(a => a.ToString())
                .Concat(ToIonText())
                .JoinUsing("");


            public static bool operator ==(Identifier first, Identifier second) => first.Equals(second);

            public static bool operator !=(Identifier first, Identifier second) => !first.Equals(second);

            public static implicit operator Identifier(string text) => Parse(text);

            #endregion

            #region Parsing
            public static Identifier Parse(string? @string)
            {
                if (TryParse(@string, out IResult<Identifier> result))
                    return result.As<IResult<Identifier>.DataResult>().Data;

                else throw result.As<IResult<Identifier>.ErrorResult>().Cause();
            }

            public static bool TryParse(string? @string, out Identifier symbol)
            {
                if (TryParse(@string, out IResult<Identifier> result))
                {
                    symbol = result.As<IResult<Identifier>.DataResult>().Data;
                    return true;
                }

                symbol = default;
                return false;
            }

            private static bool TryParse(string? @string, out IResult<Identifier> result)
            {
                if(@string is null)
                {
                    result = IResult<Identifier>.Of(new ArgumentNullException(nameof(@string)));
                    return false;
                }

                if(string.IsNullOrWhiteSpace(@string))
                {
                    result = IResult<Identifier>.Of(new FormatException("Invalid input format"));
                    return false;
                }

                var trimmed = @string.Trim();
                if (!_IdentifierPattern.IsMatch(trimmed))
                {
                    result = IResult<Identifier>.Of(new FormatException($"Invalid input format: {@string}"));
                    return false;
                }

                result = IResult<Identifier>.Of(new Identifier(trimmed));
                return true;
            }
            #endregion
        }

        /// <summary>
        /// NOTE: an operator is a SEQUENCE of ONE OR MORE operator characters.
        /// The current implementation is WRONG!!!!!!
        /// </summary>
        public readonly struct Operator : IIonSymbol
        {
            private readonly Annotation[] _annotations;

            public Operators[] Symbol { get; }

            public IonTypes Type => IonTypes.Symbol;

            public Annotation[] Annotations => _annotations?.ToArray() ?? Array.Empty<Annotation>();

            internal Operator(Operators[] @operator, params Annotation[] annotations)
            {
                Symbol = @operator;
                _annotations = annotations.Validate();
            }

            #region IIonSymbol
            public string ToIonText()
            {
                if (Symbol is null)
                    return "";

                return Symbol
                    .Select(op => (char)op)
                    .ToArray()
                    .ApplyTo(charArray => new string(charArray));
            }
            #endregion

            #region Record Implementation
            public override int GetHashCode()
                => HashCode.Combine(
                    ValueHash(Symbol.HardCast<Operators, object>()),
                    ValueHash(Annotations.HardCast<Annotation, object>()));

            public override bool Equals(object? obj)
            {
                return obj is Operator other
                    && other.Symbol.NullOrTrue(Symbol, Enumerable.SequenceEqual)
                    && other.Annotations.NullOrTrue(Annotations, Enumerable.SequenceEqual);
            }

            public override string ToString() => Annotations
                .Select(a => a.ToString())
                .Concat(ToIonText())
                .JoinUsing("");


            public static bool operator ==(Operator first, Operator second) => first.Equals(second);

            public static bool operator !=(Operator first, Operator second) => !first.Equals(second);

            public static implicit operator Operator(Operators[] @operator) => new Operator(@operator);

            #endregion

            #region Parsing
            public static Operator Parse(string? @string)
            {
                if (TryParse(@string, out IResult<Operator> result))
                    return result.As<IResult<Operator>.DataResult>().Data;

                else throw result.As<IResult<Operator>.ErrorResult>().Cause();
            }

            public static bool TryParse(string? @string, out Operator symbol)
            {
                if(TryParse(@string, out IResult<Operator> result))
                {
                    symbol = result.As<IResult<Operator>.DataResult>().Data;
                    return true;
                }

                symbol = default;
                return false;
            }

            private static bool TryParse(string? @string, out IResult<Operator> result)
            {
                if(@string is null)
                {
                    result = IResult<Operator>.Of(new ArgumentNullException(nameof(@string)));
                    return false;
                }

                if(string.IsNullOrWhiteSpace(@string))
                {
                    result = IResult<Operator>.Of(new FormatException("Invalid input format"));
                    return false;
                }

                var results = @string
                    .Trim()
                    .Select(@char => (Operators)@char)
                    .GroupBy(op => op.IsEnumDefined())
                    .Select(group => group.Key switch
                    {
                        true => IResult<Operator>.Of(group.ToArray()),
                        false => IResult<Operator>.Of(
                            new FormatException(
                                $"Invalid operator symbols found: {group.Select(op => (char)op).JoinUsing(", ")}"))
                    })
                    .ToArray();

                result = results.Length == 2
                    ? results.FirstOrDefault(r => r is IResult<Operator>.ErrorResult)
                    : results[0];

                return result is IResult<Operator>.DataResult;
            }
            #endregion
        }

        public enum Operators
        {
            Exclamation  = '!',
            Hash         = '#',
            Percent      = '%',
            Ampersand    = '&',
            Star         = '*',
            Plus         = '+',
            Minus        = '-',
            Dot          = '.',
            FSlash       = '/',
            SColon       = ';',
            Less         = '<',
            Equals       = '=',
            Greater      = '>',
            QMark        = '?',
            At           = '@',
            Caret        = '^',
            BTick        = '`',
            Pipe         = '|',
            Tilde        = '~'
        }

        #endregion
    }
}
