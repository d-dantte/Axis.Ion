using Axis.Ion.IO.Text;
using Axis.Ion.IO.Text.Streamers;
using Axis.Ion.Types;

namespace Axis.Ion.Tests.IO.Text.Streamers
{
    [TestClass]
    public class IonBoolStreamerTests
    {
        [TestMethod]
        public void StreamText()
        {
            StreamingContext context = new StreamingContext(new SerializerOptions());
            #region Null
            var ionBool = new IonBool(null);
            var ionBoolAnnotated = new IonBool(null, IIonType.Annotation.ParseCollection("stuff::other::"));
            var ionBoolStreamer = new IonBoolStreamer();
            var text = ionBoolStreamer.StreamText(ionBool, context);
            var textAnnotated = ionBoolStreamer.StreamText(ionBoolAnnotated, context);

            Assert.AreEqual("null.bool", text);
            Assert.AreEqual("stuff::other::null.bool", textAnnotated);
            #endregion

            #region true
            ionBool = new IonBool(true);
            ionBoolAnnotated = new IonBool(true, IIonType.Annotation.ParseCollection("stuff::true::"));
            ionBoolStreamer = new IonBoolStreamer();

            text = ionBoolStreamer.StreamText(ionBool, context);
            textAnnotated = ionBoolStreamer.StreamText(ionBoolAnnotated, context);
            Assert.AreEqual("true", text);
            Assert.AreEqual("stuff::true::true", textAnnotated);

            context.Options.Bools.ValueCase = SerializerOptions.Case.Uppercase;
            text = ionBoolStreamer.StreamText(ionBool, context);
            textAnnotated = ionBoolStreamer.StreamText(ionBoolAnnotated, context);
            Assert.AreEqual("TRUE", text);
            Assert.AreEqual("stuff::true::TRUE", textAnnotated);

            context = new StreamingContext(new SerializerOptions());
            context.Options.Bools.ValueCase = SerializerOptions.Case.Titlecase;
            text = ionBoolStreamer.StreamText(ionBool, context);
            textAnnotated = ionBoolStreamer.StreamText(ionBoolAnnotated, context);
            Assert.AreEqual("True", text);
            Assert.AreEqual("stuff::true::True", textAnnotated);
            #endregion

            #region false
            ionBool = new IonBool(false);
            ionBoolAnnotated = new IonBool(false, IIonType.Annotation.ParseCollection("stuff::true::"));
            ionBoolStreamer = new IonBoolStreamer();

            text = ionBoolStreamer.StreamText(ionBool, new StreamingContext(new SerializerOptions()));
            textAnnotated = ionBoolStreamer.StreamText(ionBoolAnnotated, new StreamingContext(new SerializerOptions()));
            Assert.AreEqual("false", text);
            Assert.AreEqual("stuff::true::false", textAnnotated);

            context = new StreamingContext(new SerializerOptions());
            context.Options.Bools.ValueCase = SerializerOptions.Case.Uppercase;
            text = ionBoolStreamer.StreamText(ionBool, context);
            textAnnotated = ionBoolStreamer.StreamText(ionBoolAnnotated, context);
            Assert.AreEqual("FALSE", text);
            Assert.AreEqual("stuff::true::FALSE", textAnnotated);

            context = new StreamingContext(new SerializerOptions());
            context.Options.Bools.ValueCase = SerializerOptions.Case.Titlecase;
            text = ionBoolStreamer.StreamText(ionBool, context);
            textAnnotated = ionBoolStreamer.StreamText(ionBoolAnnotated, context);
            Assert.AreEqual("False", text);
            Assert.AreEqual("stuff::true::False", textAnnotated);
            #endregion
        }

        [TestMethod]
        public void TryParse()
        {
            var value1 = new IonBool(null);
            var value2 = new IonBool(true, "stuff", "$other_stuff");
            var context = new StreamingContext(new SerializerOptions());
            var streamer = new IonBoolStreamer();

            var text1 = streamer.StreamText(value1, context);
            var text2 = streamer.StreamText(value2, context);
            var result1 = streamer.ParseString(text1);
            var result2 = streamer.ParseString(text2);

            context.Options.Bools.ValueCase = SerializerOptions.Case.Titlecase;
            text1 = streamer.StreamText(value1, context);
            text2 = streamer.StreamText(value2, context);
            var result3 = streamer.ParseString(text1);
            var result4 = streamer.ParseString(text2);

            Assert.AreEqual(value1, result1);
            Assert.AreEqual(value2, result2);
            Assert.AreEqual(value1, result3);
            Assert.AreEqual(value2, result4);
        }
    }
}
