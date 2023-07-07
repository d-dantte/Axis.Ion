using Axis.Ion.IO;
using Axis.Ion.IO.Text;
using Axis.Ion.Types;
using Axis.Luna.Common.Results;

namespace Axis.Ion.Tests.IO.Text.Serializers
{
    [TestClass]
    public class IonBoolStreamerTests
    {
        [TestMethod]
        public void Serialize()
        {
            SerializingContext context = new SerializingContext(new SerializerOptions());

            #region Null
            var ionBool = new IonBool(null);
            var ionBoolAnnotated = new IonBool(null, IIonValue.Annotation.ParseCollection("stuff::other::").Resolve());
            var text = IonTextSerializer.Serialize(ionBool, context);
            var textAnnotated = IonTextSerializer.Serialize(ionBoolAnnotated, context);

            Assert.AreEqual("null.bool", text);
            Assert.AreEqual("stuff::other::null.bool", textAnnotated);
            #endregion

            #region true
            ionBool = new IonBool(true);
            ionBoolAnnotated = new IonBool(true, IIonValue.Annotation.ParseCollection("stuff::true::").Resolve());

            text = IonTextSerializer.Serialize(ionBool, context);
            textAnnotated = IonTextSerializer.Serialize(ionBoolAnnotated, context);
            Assert.AreEqual("true", text);
            Assert.AreEqual("stuff::true::true", textAnnotated);

            context.Options.Bools.ValueCase = SerializerOptions.Case.Uppercase;
            text = IonTextSerializer.Serialize(ionBool, context);
            textAnnotated = IonTextSerializer.Serialize(ionBoolAnnotated, context);
            Assert.AreEqual("TRUE", text);
            Assert.AreEqual("stuff::true::TRUE", textAnnotated);

            context = new SerializingContext(new SerializerOptions());
            context.Options.Bools.ValueCase = SerializerOptions.Case.Titlecase;
            text = IonTextSerializer.Serialize(ionBool, context);
            textAnnotated = IonTextSerializer.Serialize(ionBoolAnnotated, context);
            Assert.AreEqual("True", text);
            Assert.AreEqual("stuff::true::True", textAnnotated);
            #endregion

            #region false
            ionBool = new IonBool(false);
            ionBoolAnnotated = new IonBool(false, IIonValue.Annotation.ParseCollection("stuff::true::").Resolve());

            text = IonTextSerializer.Serialize(ionBool, new SerializingContext(new SerializerOptions()));
            textAnnotated = IonTextSerializer.Serialize(ionBoolAnnotated, new SerializingContext(new SerializerOptions()));
            Assert.AreEqual("false", text);
            Assert.AreEqual("stuff::true::false", textAnnotated);

            context = new SerializingContext(new SerializerOptions());
            context.Options.Bools.ValueCase = SerializerOptions.Case.Uppercase;
            text = IonTextSerializer.Serialize(ionBool, context);
            textAnnotated = IonTextSerializer.Serialize(ionBoolAnnotated, context);
            Assert.AreEqual("FALSE", text);
            Assert.AreEqual("stuff::true::FALSE", textAnnotated);

            context = new SerializingContext(new SerializerOptions());
            context.Options.Bools.ValueCase = SerializerOptions.Case.Titlecase;
            text = IonTextSerializer.Serialize(ionBool, context);
            textAnnotated = IonTextSerializer.Serialize(ionBoolAnnotated, context);
            Assert.AreEqual("False", text);
            Assert.AreEqual("stuff::true::False", textAnnotated);
            #endregion
        }

        [TestMethod]
        public void TryParse()
        {
            var value1 = new IonBool(null);
            var value2 = new IonBool(true, "stuff", "$other_stuff");
            var context = new SerializingContext(new SerializerOptions());

            var text1 = IonTextSerializer.Serialize(value1, context);
            var text2 = IonTextSerializer.Serialize(value2, context);
            var result1 = IonTextSerializer.Parse<IonBool>(text1);
            var result2 = IonTextSerializer.Parse<IonBool>(text2);

            context.Options.Bools.ValueCase = SerializerOptions.Case.Titlecase;
            text1 = IonTextSerializer.Serialize(value1, context);
            text2 = IonTextSerializer.Serialize(value2, context);
            var result3 = IonTextSerializer.Parse<IonBool>(text1);
            var result4 = IonTextSerializer.Parse<IonBool>(text2);

            Assert.AreEqual(value1, result1.Resolve());
            Assert.AreEqual(value2, result2.Resolve());
            Assert.AreEqual(value1, result3.Resolve());
            Assert.AreEqual(value2, result4.Resolve());
        }
    }
}
