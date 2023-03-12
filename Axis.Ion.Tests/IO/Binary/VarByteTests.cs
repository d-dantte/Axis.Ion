using Axis.Ion.IO.Axion;
using Axis.Luna.Extensions;
using System.Collections;
using System.Numerics;

namespace Axis.Ion.Tests.IO.Binary
{
    [TestClass]
    public class VarByteTests
    {
        [TestMethod]
        public void ToByte_WithValidInput_ShouldConvertToByte()
        {
            // 0b_0000_0011
            var bitArray = new BitArray(new[]
            {
                true,
                true,
                false,
                false,
                false,
                false
            });

            var @byte = bitArray.ToByte(0);

            Assert.AreEqual((byte)3, @byte);
        }

        [TestMethod]
        public void AppendWithoutOverflow_WithValidArgs_ShouldAppend()
        {
            var @bytes = new[]
            {
                (byte)0b_1000_1001,
                (byte)0b_1000_0010,
                (byte)0b_0000_0000
            };
            var bitArray = new BitArray(24);
            bitArray.AppendWithoutOverflow(bytes[0], 0);
            bitArray.AppendWithoutOverflow(bytes[1], 1);
            bitArray.AppendWithoutOverflow(bytes[2], 2);

            Assert.AreEqual(9, bitArray.ToByte(0));
            Assert.AreEqual(1, bitArray.ToByte(1));
            Assert.AreEqual(0, bitArray.ToByte(2));
        }

        [TestMethod]
        public void ToBytes()
        {
            var bitArray = new BitArray(new[]
            {
                true, false, false, false,  false, false, false, false, // 1

                true, false, false, false,  false, false, false, true,  // 129

                true, true                                              // 3
            });

            var bytes = bitArray.ToBytes();

            Assert.AreEqual(1, bytes[0]);
            Assert.AreEqual(129, bytes[1]);
            Assert.AreEqual(3, bytes[2]);
        }

        [TestMethod]
        public void ToBigInt()
        {
            var count = 10;
            var varBytes = count.ToVarBytes();
            var bigInt = varBytes.ToBigInt();

            Assert.AreEqual(new BigInteger(10), bigInt);
        }

        [TestMethod]
        public void ReadVarByteIntegerTests()
        {
            var @int = 23315467;
            var varbytes = @int.ToVarBytes();
            var memory = new MemoryStream(varbytes);
            memory.Position = 0;

            var varbyteInt = memory.ReadVarByteInteger();

            Assert.AreEqual(@int, (int)varbyteInt);


            @int = 12;
            varbytes = @int.ToVarBytes();
            memory = new MemoryStream(varbytes);
            memory.Position = 0;

            varbyteInt = memory.ReadVarByteInteger();

            Assert.AreEqual(@int, (int)varbyteInt);
        }

        [TestMethod]
        public void GetBitsTest()
        {
            for (int cnt = 0; cnt < short.MaxValue; cnt++)
            {
                var bar = new BigInteger(cnt).GetBits();

                var expected = Convert.ToString(cnt, 2);
                var value = bar.ToBinaryString();

                if (!expected.Equals(value))
                    Console.WriteLine(value);

                Assert.AreEqual(expected, value);
            }
        }

        [TestMethod]
        public void ToVarBytes()
        {
            var value = new BigInteger(10);
            var bytes = value.ToVarBytes();

            Assert.AreEqual(1, bytes.Length);
            Assert.AreEqual(10, bytes[0]); //0b_1000_1010


            value = new BigInteger(127);
            bytes = value.ToVarBytes();

            Assert.AreEqual(1, bytes.Length);
            Assert.AreEqual(127, bytes[0]);

            value = new BigInteger(128);
            bytes = value.ToVarBytes();

            Assert.AreEqual(2, bytes.Length);
            Assert.AreEqual(128, bytes[0]);
            Assert.AreEqual(1, bytes[1]);

            value = new BigInteger(16376);
            bytes = value.ToVarBytes();

            Assert.AreEqual(2, bytes.Length);
            Assert.AreEqual(248, bytes[0]);
            Assert.AreEqual(127, bytes[1]);
        }

        [TestMethod]
        public void IsEquivalentTo()
        {
            var bitArray = new BitArray(new byte[] { 65 });
            var bitArray2 = new BitArray(new byte[] { 65 });
            var bitArray3 = new BitArray(new byte[] { 64 });
            BitArray? null1 = null;
            BitArray? null2 = null;

            Assert.IsTrue(bitArray.IsEquivalentTo(bitArray2));
            Assert.IsFalse(bitArray.IsEquivalentTo(bitArray3));
            Assert.IsTrue(null1.IsEquivalentTo(null2));
            Assert.IsFalse(bitArray.IsEquivalentTo(null1));
            Assert.IsFalse(null2.IsEquivalentTo(bitArray3));
        }

        [TestMethod]
        public void ToBitArray()
        {
            var bitArray = new BitArray(new byte[] { 76 });
            var binaryString = bitArray.ToBinaryString();
            var bitArray2 = binaryString.ToBitArray();

            Assert.IsTrue(bitArray.IsEquivalentTo(bitArray2));
            var exception = Assert.ThrowsException<FormatException>(() => "454fr".ToBitArray());
            Console.WriteLine(exception.Message);
        }
    }
}
