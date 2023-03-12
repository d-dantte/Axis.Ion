using Axis.Ion.Utils;
using System.Text.RegularExpressions;

namespace Axis.Ion.Tests.Utils
{
    [TestClass]
    public class DecomposedDecimalTests
    {
        [TestMethod]
        public void Misc()
        {
            Console.WriteLine(DoubleConverter.ToExactString(54543.65465435654345654565456454));
            Console.WriteLine(DoubleConverter.ToExactString(0));
            Console.WriteLine(DoubleConverter.ToExactString(0.0));
            Console.WriteLine(DoubleConverter.ToExactString(-0));
            Console.WriteLine(DoubleConverter.ToExactString(-0.0));
        }

        [TestMethod]
        public void ConstructorShouldCreateValidInstance()
        {
            var @default = default(DecomposedDecimal);
            Assert.IsNotNull(@default);

            var value = new DecomposedDecimal(0d);
            Assert.IsNotNull(value);

            value = new DecomposedDecimal(0m);
            Assert.IsNotNull(value);

            value = new DecomposedDecimal(123456d);
            Assert.IsNotNull(value);

            value = new DecomposedDecimal(123456.0d);
            Assert.IsNotNull(value);

            value = new DecomposedDecimal(123456.789012345678901234567890d);
            Assert.IsNotNull(value);

            value = new DecomposedDecimal(0.00000000000000000000123456d);
            Assert.IsNotNull(value);

            value = new DecomposedDecimal(-123456d);
            Assert.IsNotNull(value);

            value = new DecomposedDecimal(-123456.0d);
            Assert.IsNotNull(value);

            value = new DecomposedDecimal(-123456.789012345678901234567890d);
            Assert.IsNotNull(value);

            value = new DecomposedDecimal(-0.00000000000000000000123456d);
            Assert.IsNotNull(value);
        }

        [TestMethod]
        public void Constructor2ShouldCreateValidInstance()
        {
            var @default = default(DecomposedDecimal);
            Assert.IsNotNull(@default);

            var value = new DecomposedDecimal(0d);
            Assert.IsNotNull(value);

            value = new DecomposedDecimal(0m);
            Assert.IsNotNull(value);

            Assert.ThrowsException<ArgumentException>(() => new DecomposedDecimal(double.PositiveInfinity));
            Assert.ThrowsException<ArgumentException>(() => new DecomposedDecimal(double.NegativeInfinity));
            Assert.ThrowsException<ArgumentException>(() => new DecomposedDecimal(double.NaN));

            value = new DecomposedDecimal(123456d);
            Assert.IsNotNull(value);

            value = new DecomposedDecimal(123456.0d);
            Assert.IsNotNull(value);

            value = new DecomposedDecimal(123456.789012345678901234567890d);
            Assert.IsNotNull(value);

            value = new DecomposedDecimal(0.00000000000000000000123456d);
            Assert.IsNotNull(value);

            value = new DecomposedDecimal(-123456d);
            Assert.IsNotNull(value);

            value = new DecomposedDecimal(-123456.0d);
            Assert.IsNotNull(value);

            value = new DecomposedDecimal(-123456.789012345678901234567890d);
            Assert.IsNotNull(value);

            value = new DecomposedDecimal(-0.00000000000000000000123456d);
            Assert.IsNotNull(value);
        }


        [TestMethod]
        public void ToSicientificNotation_ShouldReturnCorrectValue()
        {
            var value = default(DecomposedDecimal);
            var scientific = value.ToScientificNotation(5);
            Assert.AreEqual("0.0E0", scientific);

            value = new DecomposedDecimal(-0.0d);
            scientific = value.ToScientificNotation(5);
            Assert.AreEqual("0.0E0", scientific);

            value = new DecomposedDecimal(1.0d);
            scientific = value.ToScientificNotation(5);
            Assert.AreEqual("1.0E0", scientific);

            value = new DecomposedDecimal(10.0d);
            scientific = value.ToScientificNotation(5);
            Assert.AreEqual("1.0E1", scientific);

            value = new DecomposedDecimal(-3000.0d);
            scientific = value.ToScientificNotation(5);
            Assert.AreEqual("-3.0E3", scientific);

            value = new DecomposedDecimal(0.00234567d);
            scientific = value.ToScientificNotation(3);
            Assert.AreEqual("2.34E-3", scientific);

            value = new DecomposedDecimal(0.00234567d);
            scientific = value.ToScientificNotation(6);
            Assert.IsTrue(new Regex(@"^\d\.\d{5}E\-3$").IsMatch(scientific));

            value = new DecomposedDecimal(0.00234567d);
            scientific = value.ToScientificNotation(7);
            Assert.IsTrue(new Regex(@"^\d\.\d{6}E\-3$").IsMatch(scientific));

            value = new DecomposedDecimal(0.00234000d);
            scientific = value.ToScientificNotation(7);
            Assert.AreEqual("2.34E-3", scientific);
        }

        [TestMethod]
        public void ToSicientificNotation2_ShouldReturnCorrectValue()
        {
            var value = default(DecomposedDecimal2);
            var scientific = value.ToScientificNotation(5);
            Assert.AreEqual("0.0E0", scientific);

            value = new DecomposedDecimal2(-0.0d);
            scientific = value.ToScientificNotation(5);
            Assert.AreEqual("0.0E0", scientific);

            value = new DecomposedDecimal2(1.0d);
            scientific = value.ToScientificNotation(5);
            Assert.AreEqual("1.0E0", scientific);

            value = new DecomposedDecimal2(10.0d);
            scientific = value.ToScientificNotation(5);
            Assert.AreEqual("1.0E1", scientific);

            value = new DecomposedDecimal2(-3000.0d);
            scientific = value.ToScientificNotation(5);
            Assert.AreEqual("-3.0E3", scientific);

            value = new DecomposedDecimal2(0.00234567d);
            scientific = value.ToScientificNotation(3);
            Assert.AreEqual("2.34E-3", scientific);

            value = new DecomposedDecimal2(0.00234567d);
            scientific = value.ToScientificNotation(6);
            Assert.IsTrue(new Regex(@"^\d\.\d{5}E\-3$").IsMatch(scientific));

            value = new DecomposedDecimal2(0.00234567d);
            scientific = value.ToScientificNotation(7);
            Assert.IsTrue(new Regex(@"^\d\.\d{6}E\-3$").IsMatch(scientific));

            value = new DecomposedDecimal2(0.00234000d);
            scientific = value.ToScientificNotation(7);
            Assert.AreEqual("2.34E-3", scientific);
        }
    }
}
