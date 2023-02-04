using Axis.Ion.IO.Text;
using Axis.Ion.IO.Text.Streamers;
using Axis.Ion.Types;

namespace Axis.Ion.Tests.IO.Text.Streamers
{
    [TestClass]
    public class IonNullStreamerTests
    {
        [TestMethod]
        public void StreamText()
        {
            var ionNull = new IonNull();
            var ionNullAnnotated = new IonNull("stuff", "other");
            var ionNullStreamer = new IonNullStreamer();

            var text = ionNullStreamer.StreamText(ionNull, new StreamingContext(new SerializerOptions()));
            var textAnnotated = ionNullStreamer.StreamText(ionNullAnnotated, new StreamingContext(new SerializerOptions()));
            Assert.AreEqual("null", text);
            Assert.AreEqual("stuff::other::null", textAnnotated);


            var context = new StreamingContext(new SerializerOptions());
            context.Options.Nulls.UseLongFormNulls = true;

            text = ionNullStreamer.StreamText(ionNull, context);
            textAnnotated = ionNullStreamer.StreamText(ionNullAnnotated, context);
            Assert.AreEqual("null.null", text);
            Assert.AreEqual("stuff::other::null.null", textAnnotated);
        }

        [TestMethod]
        public void TryParse()
        {
            var ionNull = new IonNull();
            var ionNullAnnotated = new IonNull("stuff", "other");
            var ionNullStreamer = new IonNullStreamer();
            var context = new StreamingContext(new SerializerOptions());
            context.Options.Nulls.UseLongFormNulls = true;

            var text = ionNullStreamer.StreamText(ionNull, new StreamingContext(new SerializerOptions()));
            var textAnnotated = ionNullStreamer.StreamText(ionNullAnnotated, new StreamingContext(new SerializerOptions()));
            var longtext = ionNullStreamer.StreamText(ionNull, context);
            var longtextAnnotated = ionNullStreamer.StreamText(ionNullAnnotated, context);

            var textResult = ionNullStreamer.ParseString(text);
            var textAnnotatedResult = ionNullStreamer.ParseString(textAnnotated);
            var longtextResult = ionNullStreamer.ParseString(longtext);
            var longtextAnnotatedResult = ionNullStreamer.ParseString(longtextAnnotated);

            Assert.AreEqual(ionNull, textResult);
            Assert.AreEqual(ionNullAnnotated, textAnnotatedResult);
            Assert.AreEqual(ionNull, longtextResult);
            Assert.AreEqual(ionNullAnnotated, longtextAnnotatedResult);
        }
    }
}
