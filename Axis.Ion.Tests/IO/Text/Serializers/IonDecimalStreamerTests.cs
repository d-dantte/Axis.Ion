using Axis.Ion.IO;
using Axis.Ion.IO.Text;
using Axis.Ion.Types;
using Axis.Luna.Common.Results;

namespace Axis.Ion.Tests.IO.Text.Serializers
{
    [TestClass]
    public class IonDecimalStreamerTests
    {
        [TestMethod]
        public void Serialize()
        {
            var context = new SerializingContext(new SerializerOptions());

            #region Null
            var ionDecimal = new IonDecimal(null);
            var ionDecimalAnnotated = new IonDecimal(null, IIonValue.Annotation.ParseCollection("stuff::other::").Resolve());
            var text = IonTextSerializer.Serialize(ionDecimal, context);
            var textAnnotated = IonTextSerializer.Serialize(ionDecimalAnnotated, context);

            Assert.AreEqual("null.decimal", text);
            Assert.AreEqual("stuff::other::null.decimal", textAnnotated);
            #endregion

            #region value
            ionDecimal = new IonDecimal(123456789.0009m);
            var lionDecimal = new IonDecimal(0.000000098765m);
            var nionDecimal = new IonDecimal(-123456789.0009m);
            ionDecimalAnnotated = new IonDecimal(123456789.0009m, IIonValue.Annotation.ParseCollection("stuff::true::").Resolve());
            var nionDecimalAnnotated = new IonDecimal(-123456789.0009m, IIonValue.Annotation.ParseCollection("stuff::true::").Resolve());

            // no exponent
            context.Options.Decimals.UseExponentNotation = false;
            text = IonTextSerializer.Serialize(ionDecimal, context);
            var ltext = IonTextSerializer.Serialize(lionDecimal, context);
            var ntext = IonTextSerializer.Serialize(nionDecimal, context);
            textAnnotated = IonTextSerializer.Serialize(ionDecimalAnnotated, context);
            var ntextAnnotated = IonTextSerializer.Serialize(nionDecimalAnnotated, context);

            Assert.AreEqual("123456789.0009", text);
            Assert.AreEqual("0.000000098765", ltext);
            Assert.AreEqual("stuff::true::123456789.0009", textAnnotated);
            Assert.AreEqual("-123456789.0009", ntext);
            Assert.AreEqual("stuff::true::-123456789.0009", ntextAnnotated);

            // exponent
            context.Options.Decimals.UseExponentNotation = true;
            ltext = IonTextSerializer.Serialize(lionDecimal, context);
            text = IonTextSerializer.Serialize(ionDecimal, context);
            ntext = IonTextSerializer.Serialize(nionDecimal, context);
            textAnnotated = IonTextSerializer.Serialize(ionDecimalAnnotated, context);
            ntextAnnotated = IonTextSerializer.Serialize(nionDecimalAnnotated, context);

            Assert.AreEqual("1.234567890009D8", text);
            Assert.AreEqual("9.8765D-8", ltext);
            Assert.AreEqual("stuff::true::1.234567890009D8", textAnnotated);
            Assert.AreEqual("-1.234567890009D8", ntext);
            Assert.AreEqual("stuff::true::-1.234567890009D8", ntextAnnotated);
            #endregion
        }

        [TestMethod]
        public void TryParse()
        {
            var nvalue = new IonDecimal(null);
            var value1 = new IonDecimal(1000);
            var value2 = new IonDecimal(-1000, "stuff", "$other_stuff");
            var value3 = new IonDecimal(0.000006557m, "stuff", "$other_stuff");
            var context = new SerializingContext(new SerializerOptions());

            // no exponent
            context.Options.Decimals.UseExponentNotation = false;
            var ntext = IonTextSerializer.Serialize(nvalue, context);
            var text1 = IonTextSerializer.Serialize(value1, context);
            var text2 = IonTextSerializer.Serialize(value2, context);
            var text3 = IonTextSerializer.Serialize(value3, context);

            var nresult = IonTextSerializer.Parse<IonDecimal>(ntext);
            var result1 = IonTextSerializer.Parse<IonDecimal>(text1);
            var result2 = IonTextSerializer.Parse<IonDecimal>(text2);
            var result3 = IonTextSerializer.Parse<IonDecimal>(text3);

            Assert.AreEqual(nvalue, nresult.Resolve());
            Assert.AreEqual(value1, result1.Resolve());
            Assert.AreEqual(value2, result2.Resolve());
            Assert.AreEqual(value3, result3.Resolve());

            // exponent
            context.Options.Decimals.UseExponentNotation = true;
            ntext = IonTextSerializer.Serialize(nvalue, context);
            text1 = IonTextSerializer.Serialize(value1, context);
            text2 = IonTextSerializer.Serialize(value2, context);
            text3 = IonTextSerializer.Serialize(value3, context);

            nresult = IonTextSerializer.Parse<IonDecimal>(ntext);
            result1 = IonTextSerializer.Parse<IonDecimal>(text1);
            result2 = IonTextSerializer.Parse<IonDecimal>(text2);
            result3 = IonTextSerializer.Parse<IonDecimal>(text3);

            Assert.AreEqual(nvalue, nresult.Resolve());
            Assert.AreEqual(value1, result1.Resolve());
            Assert.AreEqual(value2, result2.Resolve());
            Assert.AreEqual(value3, result3.Resolve());
        }
    }
}
