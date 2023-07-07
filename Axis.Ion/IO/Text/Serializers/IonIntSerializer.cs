using Axis.Ion.IO.Axion;
using Axis.Ion.Types;
using Axis.Luna.Common;
using Axis.Luna.Common.Results;
using Axis.Luna.Extensions;
using Axis.Pulsar.Grammar.CST;
using System;
using System.Globalization;
using System.Linq;
using System.Numerics;
using static Axis.Ion.Types.IIonValue;
using static System.Net.Mime.MediaTypeNames;

namespace Axis.Ion.IO.Text.Serializers
{
    public class IonIntSerializer : IIonTextSerializer<IonInt>
    {
        public static string GrammarSymbol => IntSymbol;

        #region Symbols
        internal const string IntSymbol = "ion-int";
        internal const string NullIntSymbol = "null-int";
        internal const string IntNumberSymbol = "int-number";
        internal const string NegativeSignSymbol = "negative-sign";
        internal const string IntNotationSymbol = "int-notation";
        internal const string BinaryIntSymbol = "binary-int";
        internal const string HexIntSymbol = "hex-int";
        internal const string RegularIntSymbol = "regular-int";
        #endregion

        public static IResult<IonInt> Parse(CSTNode symbolNode)
        {
            ArgumentNullException.ThrowIfNull(symbolNode);

            try
            {
                if (!IntSymbol.Equals(symbolNode.SymbolName))
                    return Result.Of<IonInt>(new ArgumentException(
                        $"Invalid symbol name: '{symbolNode.SymbolName}'. "
                        + $"Expected '{IntSymbol}'"));

                (var annotations, var intToken) = IonTextSerializer.DeconstructAnnotations(symbolNode);

                return intToken.SymbolName switch
                {
                    NullIntSymbol => Result.Of(new IonInt(null, annotations)),
                    IntNumberSymbol => ParseIntNumber(intToken, annotations),

                    _ => Result.Of<IonInt>(new ArgumentException(
                        $"Invalid symbol encountered: '{intToken.SymbolName}'. "
                        + $"Expected '{NullIntSymbol}', or '{IntNumberSymbol}'"))
                };

                throw new NotImplementedException();
            }
            catch (Exception e)
            {
                return Result.Of<IonInt>(e);
            }
        }

        internal static IResult<IonInt> ParseIntNumber(CSTNode intNumberSymbol, Annotation[] annotations)
        {
            try
            {
                if (!IntNumberSymbol.Equals(intNumberSymbol.SymbolName))
                    return Result.Of<IonInt>(new ArgumentException(
                        $"Invalid symbol name: '{intNumberSymbol.SymbolName}'. "
                        + $"Expected '{IntNumberSymbol}'"));

                var isNegative = intNumberSymbol
                    .FindNode(NegativeSignSymbol)
                    .AsOptional()
                    .Map(value => value.TokenValue())
                    .Map(value => value.Equals("-"))
                    .ValueOr(false);

                return intNumberSymbol
                    .FindNode($"{IntNotationSymbol}.{BinaryIntSymbol}|{HexIntSymbol}|{RegularIntSymbol}")?
                    .SymbolName switch
                {
                    BinaryIntSymbol => intNumberSymbol
                        .TokenValue()
                        .TrimStart("-")[2..] // remove the '0b' prefix, and any negative sign
                        .Replace("_", "")
                        .ToBitArray()
                        .ToBytes()
                        .ApplyTo(barr => new BigInteger(barr) * (isNegative ? -1:1))
                        .ApplyTo(bigInt => Result.Of(new IonInt(bigInt, annotations))),

                    HexIntSymbol => intNumberSymbol
                        .TokenValue()
                        .TrimStart("-")[2..] // remove the '0x' prefix, and any negative sign
                        .Replace("_", "")
                        .ApplyTo(hex => BigInteger.Parse(hex, NumberStyles.HexNumber) * (isNegative ? -1 : 1))
                        .ApplyTo(bigInt => Result.Of(new IonInt(bigInt, annotations))),

                    RegularIntSymbol => intNumberSymbol
                        .TokenValue()
                        .Replace("_", "")
                        .ApplyTo(dec => BigInteger.Parse(dec, NumberStyles.Integer))
                        .ApplyTo(bigInt => Result.Of(new IonInt(bigInt, annotations))),

                    _ => Result.Of<IonInt>(new ArgumentException(
                        $"Invalid symbol encountered. "
                        + $"Expected '{BinaryIntSymbol}', '{HexIntSymbol}', etc"))
                };
            }
            catch(Exception e)
            {
                return Result.Of<IonInt>(e);
            }
        }

        public static string Serialize(IonInt value) => Serialize(value, new SerializingContext());

        public static string Serialize(IonInt value, SerializingContext context)
        {
            var text = value.IsNull switch
            {
                true => "null.int",
                false => ConvertToText(value, context.Options.Ints)
            };

            var annotationText = IonAnnotationSerializer.Serialize(value.Annotations, context);
            return $"{annotationText}{text}";
        }

        internal static string ConvertToText(IonInt ionValue, SerializerOptions.IntOptions options)
        {
            var bigInt = ionValue.Value ?? throw new ArgumentException("Invalid ion int");
            return options.NumberFormat switch
            {
                SerializerOptions.IntFormat.Decimal => bigInt
                    .ApplyTo(@int => (sign: @int >= 0, unsigned: @int.ToString().TrimStart("-")))
                    .ApplyTo(tuple => options.UseDigitSeparator
                        ? (tuple.sign, unsigned: ApplySeparator(tuple.unsigned, 3))
                        : tuple)
                    .ApplyTo(tuple => $"{(tuple.sign ? "" : "-")}{tuple.unsigned}"),

                SerializerOptions.IntFormat.BigHex => bigInt
                    .ToString("X")
                    .ApplyTo(hex => options.UseDigitSeparator
                        ? ApplySeparator(hex, 4)
                        : hex)
                    .ApplyTo(hex => $"0X{hex}"),

                SerializerOptions.IntFormat.SmallHex => bigInt
                    .ToString("x")
                    .ApplyTo(hex => options.UseDigitSeparator
                        ? ApplySeparator(hex, 4)
                        : hex)
                    .ApplyTo(hex => $"0x{hex}"),

                SerializerOptions.IntFormat.BigBinary => bigInt
                    .GetBits()
                    .ToBinaryString()
                    .ApplyTo(bin => options.UseDigitSeparator
                        ? ApplySeparator(bin, 4)
                        : bin)
                    .ApplyTo(bin => $"0B{bin}"),

                SerializerOptions.IntFormat.SmallBinary => bigInt
                    .GetBits()
                    .ToBinaryString()
                    .ApplyTo(bin => options.UseDigitSeparator
                        ? ApplySeparator(bin, 4)
                        : bin)
                    .ApplyTo(bin => $"0b{bin}"),

                _ => throw new ArgumentException($"Invalid number format specified: {options.NumberFormat}")
            };
        }

        internal static string ApplySeparator(string value, int interval)
        {
            return value
                .Reverse()
                .Select((@char, index) => (@char, index))
                .GroupBy(tuple => tuple.index / interval, tuple => tuple.@char)
                .Select(group => new string(group.ToArray()))
                .JoinUsing("_")
                .Reverse();
        }
    }
}
