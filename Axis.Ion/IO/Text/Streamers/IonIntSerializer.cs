using Axis.Ion.IO.Axion;
using Axis.Ion.Types;
using Axis.Luna.Common;
using Axis.Luna.Extensions;
using Axis.Pulsar.Grammar.CST;
using System;
using System.Globalization;
using System.Linq;
using System.Numerics;

namespace Axis.Ion.IO.Text.Streamers
{
    public class IonIntSerializer : AbstractIonTypeSerializer<IonInt>
    {
        public const string IonIntSymbol = "ion-int";
        public const string NullIntSymbol = "null-int";
        public const string BinaryIntSymbol = "binary-int";
        public const string HexIntSymbol = "hex-int";
        public const string DecimalIntSymbol = "regular-int";

        override protected string IonValueSymbolName => IonIntSymbol;

        public override string GetIonValueText(IonInt ionValue, SerializingContext context)
        {
            return ionValue.IsNull switch
            {
                true => "null.int",
                false => ConvertToText(ionValue, context.Options.Ints)
            };
        }

        public override bool TryParse(CSTNode tokenNode, out IResult<IonInt> result)
        {
            if (tokenNode is null)
                throw new ArgumentNullException(nameof(tokenNode));

            try
            {
                if (!IonIntSymbol.Equals(tokenNode.SymbolName))
                    return $"Invalid token node name: {tokenNode.SymbolName}".FailWithMessage(out result);

                (var annotations, var intToken) = DeconstructAnnotations(tokenNode);

                return intToken.SymbolName switch
                {
                    NullIntSymbol => new IonInt(null, annotations).PassWithValue(out result),

                    BinaryIntSymbol => intToken
                        .TokenValue()[2..] // remove the '0b' prefix
                        .Replace("_", "")
                        .ToBitArray()
                        .ToBytes()
                        .ApplyTo(barr => new IonInt(new BigInteger(barr), annotations))
                        .PassWithValue(out result),

                    HexIntSymbol => intToken
                        .TokenValue()[2..] // remove the '0x' prefix
                        .Replace("_", "")
                        .ApplyTo(hex => BigInteger.Parse(hex, NumberStyles.HexNumber))
                        .ApplyTo(bigInt => new IonInt(bigInt, annotations))
                        .PassWithValue(out result),

                    DecimalIntSymbol => intToken
                        .TokenValue()
                        .Replace("_", "")
                        .ApplyTo(dec => BigInteger.Parse(dec, NumberStyles.Integer))
                        .ApplyTo(bigInt => new IonInt(bigInt, annotations))
                        .PassWithValue(out result),

                    _ => $"Invalid ion bool token: {tokenNode.FirstNode().SymbolName}".FailWithMessage(out result)
                };

                throw new NotImplementedException();
            }
            catch (Exception e)
            {
                result = Result.Of<IonInt>(e);
                return false;
            }
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
                    .ApplyTo(tuple => $"{(tuple.sign ? "":"-")}{tuple.unsigned}"),

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
