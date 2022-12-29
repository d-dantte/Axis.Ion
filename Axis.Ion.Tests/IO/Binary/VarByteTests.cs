using Axis.Ion.IO.Binary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Axis.Ion.Tests.IO.Binary
{
    [TestClass]
    public class VarByteTests
    {
        [TestMethod]
        public void AppendWithOverflow_WithValidInput_AddsCorrectValues()
        {
            var bits = new List<bool>();
            var bitCount = 0;

            for (int cnt = 0; cnt < 20; cnt++)
            {
                _ = bits.AppendWithOverflow(false, ref bitCount);
            }

            Assert.AreEqual(22, bits.Count);
            Assert.AreEqual(20, bitCount);

            var expected = new[] 
            {
                false, false, false, false, false, false, false, true, false, false, false, false,
                false, false, false, true, false, false, false, false, false, false
            };
            Assert.IsTrue(expected.SequenceEqual(bits));
        }

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
        public void ToVarBytes()
        {
            var count = 10;
            var bytes = count.ToVarBytes();

            Assert.AreEqual(2, bytes.Length);
            Assert.AreEqual(138, bytes[0]); //0b_1000_1010
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
    }
}
