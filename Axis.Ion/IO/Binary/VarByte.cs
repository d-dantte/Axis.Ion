using Axis.Luna.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace Axis.Ion.IO.Binary
{
    public static class VarBytes
    {
        private static readonly byte[] ByteMasks = new byte[]
        {
            1,
            2,
            4,
            8,
            16,
            32,
            64,
            128
        };


        public static BigInteger ToBigInt(this byte[] varbytes)
        {
            if (varbytes == null)
                throw new ArgumentNullException(nameof(varbytes));

            if (varbytes.Length == 0)
                return BigInteger.Zero;

            var bitOffset = 0;
            return varbytes
                .Aggregate(
                    new BitArray(8 * varbytes.Length),
                    (array, varbyte) => array.AppendWithoutOverflow(varbyte, bitOffset++))
                .ToBytes()
                .NewBigInt();
        }

        public static byte[] ToVarBytes(this BigInteger bigInt)
        {
            var bitCount = 0;

            return bigInt
                .ToByteArray()
                .SelectMany((@byte, byteIndex) =>
                {
                    var bits = new List<bool>();

                    for (int cnt = 0; cnt < 8; cnt++)
                        bits.AppendWithOverflow(@byte.IsSet(cnt), ref bitCount);

                    return bits;
                })
                .ApplyTo(bools => new BitArray(bools.ToArray()))
                .ToBytes();
        }

        public static byte[] ToVarBytes(this int value) => ToVarBytes((BigInteger)value);

        public static byte[] ToVarBytes(this long value) => ToVarBytes((BigInteger)value);

        public static byte[] ToBytes(this BitArray bitArray)
        {
            var quotient = Math.DivRem(bitArray.Length, 8, out var rem);
            var count =  rem > 0 ? quotient + 1 : quotient;

            return Enumerable
                .Range(0, count)
                .Select(index => bitArray.ToByte(index))
                .ToArray();
        }

        public static bool IsSet(this byte @byte, int bitIndex) => (@byte & ByteMasks[bitIndex]) == ByteMasks[bitIndex];

        public static bool IsSet(this int @byte, int bitIndex) => (@byte & ByteMasks[bitIndex]) == ByteMasks[bitIndex];

        public static BigInteger NewBigInt(this byte[] bytes) => new BigInteger(bytes);

        public static BigInteger ReadVarByteInteger(this Stream stream)
        {
            var bytes = new List<byte>();
            int @byte = 128; // sets the overflow bit
            while (@byte.IsSet(7)
                && (@byte = stream.ReadByte()) != -1)
            {
                bytes.Add((byte)@byte);
            }

            return bytes
                .ToArray()
                .ToBigInt();
        }

        public static bool TryReadVarByteInteger(this Stream stream, out BigInteger value)
        {
            try
            {
                value = ReadVarByteInteger(stream);
                return true;
            }
            catch
            {
                value = default;
                return false;
            }
        }

        internal static BitArray AppendWithoutOverflow(this BitArray array, byte @byte, int offset)
        {
            var actualOffset = offset * 7;
            array[actualOffset] = @byte.IsSet(0);
            array[actualOffset + 1] = @byte.IsSet(1);
            array[actualOffset + 2] = @byte.IsSet(2);
            array[actualOffset + 3] = @byte.IsSet(3);
            array[actualOffset + 4] = @byte.IsSet(4);
            array[actualOffset + 5] = @byte.IsSet(5);
            array[actualOffset + 6] = @byte.IsSet(6);

            return array;
        }

        internal static byte ToByte(this BitArray bitArray, int byteIndex)
        {
            var bitOffset = byteIndex * 8;
            byte @byte = 0;

            for (int index = 0; index < 8; index++)
            {
                var currentOffset = bitOffset + index;
                if (currentOffset < bitArray.Length)
                {
                    if (bitArray[currentOffset])
                        @byte |= ByteMasks[index];
                }
                else break;
            }
            return @byte;
        }

        internal static List<bool> AppendWithOverflow(this List<bool> bools, bool value, ref int bitCount)
        {
            bools.Add(value);
            bitCount++;

            if (bitCount % 7 == 0)
                bools.Add(true);

            return bools;
        }
    }
}
