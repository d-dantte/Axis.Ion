using Axis.Ion.IO;
using Axis.Ion.IO.Text;
using Axis.Ion.Types;
using Axis.Luna.Common.Results;
using System.Text.RegularExpressions;

namespace Axis.Ion.Tests.IO.Text.Serializers
{
    [TestClass] 
    public class IonFloatStreamerTests
    {
        private static readonly Regex ExponentPattern = new Regex(@"\-?\d+\.\d+E\-?\d+");

        [TestMethod]
        public void Serialize()
        {
            var context = new SerializingContext(new SerializerOptions());

            #region Null
            var ionFloat = new IonFloat(null);
            var ionFloatAnnotated = new IonFloat(null, IIonValue.Annotation.ParseCollection("stuff::other::").Resolve());
            var text = IonTextSerializer.Serialize(ionFloat, context);
            var textAnnotated = IonTextSerializer.Serialize(ionFloatAnnotated, context);

            Assert.AreEqual("null.float", text);
            Assert.AreEqual("stuff::other::null.float", textAnnotated);
            #endregion

            #region value
            ionFloat = new IonFloat(123456789.0009d);
            var lionFloat = new IonFloat(0.000000098765d);
            var nionFloat = new IonFloat(-123456789.0009d);
            ionFloatAnnotated = new IonFloat(123456789.0009d, IIonValue.Annotation.ParseCollection("stuff::true::").Resolve());
            var nionFloatAnnotated = new IonFloat(-123456789.0009d, IIonValue.Annotation.ParseCollection("stuff::true::").Resolve());

            // no exponent
            text = IonTextSerializer.Serialize(ionFloat, context);
            var ltext = IonTextSerializer.Serialize(lionFloat, context);
            var ntext = IonTextSerializer.Serialize(nionFloat, context);
            textAnnotated = IonTextSerializer.Serialize(ionFloatAnnotated, context);
            var ntextAnnotated = IonTextSerializer.Serialize(nionFloatAnnotated, context);

            Assert.IsTrue(ExponentPattern.IsMatch(text));
            Assert.IsTrue(ExponentPattern.IsMatch(ltext));
            Assert.IsTrue(ExponentPattern.IsMatch(textAnnotated));
            Assert.IsTrue(ExponentPattern.IsMatch(ntext));
            Assert.IsTrue(ExponentPattern.IsMatch(ntextAnnotated));
            #endregion
        }

        [TestMethod]
        public void TryParse()
        {
            var nvalue = new IonFloat(null);
            var value1 = new IonFloat(1000);
            var value2 = new IonFloat(-1000, "stuff", "$other_stuff");
            var value3 = new IonFloat(0.000006557, "stuff", "$other_stuff");
            var context = new SerializingContext(new SerializerOptions());

            // no exponent
            var ntext = IonTextSerializer.Serialize(nvalue, context);
            var text1 = IonTextSerializer.Serialize(value1, context);
            var text2 = IonTextSerializer.Serialize(value2, context);
            var text3 = IonTextSerializer.Serialize(value3, context);

            var nresult = IonTextSerializer.Parse<IonFloat>(ntext);
            var result1 = IonTextSerializer.Parse<IonFloat>(text1);

            var result2 = IonTextSerializer.Parse<IonFloat>(text2);
            var result3 = IonTextSerializer.Parse<IonFloat>(text3);

            Assert.AreEqual(nvalue, nresult.Resolve());
            Assert.AreEqual(value1, result1.Resolve());
            Assert.AreEqual(value2, result2.Resolve());
            Assert.AreEqual(value3, result3.Resolve());
        }
    }
}
