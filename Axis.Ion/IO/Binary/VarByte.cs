using Axis.Luna.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Axis.Ion.IO.Binary
{
    public static class VarBytesExtensions
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

        internal static byte[] ToVarBytes(this BigInteger bigInt)
        {
            return bigInt
                .GetBits()
                .OverflowEnumeration()
                .Skip(1)
                .ApplyTo(bits => new BitArray(bits.ToArray()))
                .ToBytes();
        }

        public static byte[] ToVarBytes(this int value) => ToVarBytes((BigInteger)value);

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

        internal static int GetBitLength(this BigInteger bigInt)
        {
            return bigInt.GetBits().Length;
        }

        internal static BitArray GetBits(this BigInteger bigInt)
        {
            var found = false;
            var bits = new List<bool>();
            var bytes = bigInt.ToByteArray();

            for (int cnt = bytes.Length - 1; cnt >= 0; cnt--)
            {
                var @byte = bytes[cnt];

                for (int i = 7; i >= 0; i--)
                {
                    if (@byte.IsSet(i) || found)
                    {
                        bits.Add(@byte.IsSet(i));
                        found = true;
                    }
                }
            }

            if (bits.Count > 0)
                bits.Reverse();

            else bits.Add(false);

            return new BitArray(bits.ToArray());
        }

        internal static string ToBinaryString(this BitArray bitArray)
        {
            if (bitArray is null)
                throw new ArgumentNullException(nameof(bitArray));

            return bitArray
                .Bits()
                .Select(bit => bit ? "1" : "0")
                .Reverse()
                .JoinUsing("");
        }

        internal static IEnumerable<bool> Bits(this BitArray bitArray)
        {
            for (int cnt = 0; cnt < bitArray.Length; cnt++)
                yield return bitArray[cnt];
        }

        internal static BitArray ToBitArray(this string binaryString)
        {
            if (binaryString is null)
                throw new ArgumentNullException(nameof(binaryString));

            if (binaryString.Length == 0)
                return new BitArray(new bool[0]);

            return binaryString
                .Reverse()
                .Select(ch => ch switch
                {
                    '1' => true,
                    '0' => false,
                    _ => throw new FormatException($"Invalid character found in binary string: {ch}")
                })
                .ToArray()
                .ApplyTo(barr => new BitArray(barr));
        }

        internal static bool IsEquivalentTo(this BitArray? first, BitArray? second)
        {
            // reference and null test
            if (first == second)
                return true;

            // null test
            else if (first is null || second is null)
                return false;

            return Enumerable.SequenceEqual(first.Bits(), second.Bits());
        }

        private static IEnumerable<bool> OverflowEnumeration(this BitArray bitarray)
        {
            var index = 0;
            for (int cnt = 0; cnt < bitarray.Length; cnt++)
            {
                if (index == 0)
                    yield return true;

                yield return bitarray[cnt];

                index = ++index % 7;
            }
        }
    }

    public static class StreamExtensions
    {
        public static bool TryReadByte(this Stream stream, out byte value)
        {
            int output = stream.ReadByte();
            if (output < 0)
            {
                value = default;
                return false;
            }

            value = (byte)output;
            return true;
        }

        public static bool TryReadExactBytes(this Stream stream, int byteCount, out byte[] bytes)
        {
            bytes = new byte[byteCount];
            var readCount = stream.Read(bytes);
            if (readCount != byteCount)
            {
                bytes = Array.Empty<byte>();
                return false;
            }

            return true;
        }

        public static bool TryReadInt(this Stream stream, out int value)
        {
            value = 0;
            byte[] intBytes = new byte[4];

            if (stream.Read(intBytes) != 4)
                return false;

            value = BitConverter.ToInt32(intBytes);
            return true;
        }

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

        public static IEnumerable<ArraySegment<byte>> ReadPages(this Stream stream)
        {
            while (stream.TryReadVarByteInteger(out var pageCount))
            {
                if (pageCount == 0)
                    yield break;

                var bytes = new byte[(int)pageCount];
                var bytesRead = stream.Read(bytes);

                if (bytesRead == -1)
                    yield break;

                yield return bytes.Slice(0, bytesRead);
            }
        }

        public static long Paginate(this Stream stream, Action<ArraySegment<byte>> consumer)
        {
            if (consumer == null)
                throw new ArgumentNullException(nameof(consumer));

            long pages = 0;
            foreach (var page in stream.ReadPages())
            {
                pages++;
                consumer.Invoke(page);
            }
            return pages;
        }
    }
}
