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
            var ionNullStreamer = new IonNullSerializer();

            var text = ionNullStreamer.SerializeText(ionNull, new SerializingContext(new SerializerOptions()));
            var textAnnotated = ionNullStreamer.SerializeText(ionNullAnnotated, new SerializingContext(new SerializerOptions()));
            Assert.AreEqual("null", text);
            Assert.AreEqual("stuff::other::null", textAnnotated);


            var context = new SerializingContext(new SerializerOptions());
            context.Options.Nulls.UseLongFormNulls = true;

            text = ionNullStreamer.SerializeText(ionNull, context);
            textAnnotated = ionNullStreamer.SerializeText(ionNullAnnotated, context);
            Assert.AreEqual("null.null", text);
            Assert.AreEqual("stuff::other::null.null", textAnnotated);
        }

        [TestMethod]
        public void TryParse()
        {
            var ionNull = new IonNull();
            var ionNullAnnotated = new IonNull("stuff", "other");
            var ionNullStreamer = new IonNullSerializer();
            var context = new SerializingContext(new SerializerOptions());
            context.Options.Nulls.UseLongFormNulls = true;

            var text = ionNullStreamer.SerializeText(ionNull, new SerializingContext(new SerializerOptions()));
            var textAnnotated = ionNullStreamer.SerializeText(ionNullAnnotated, new SerializingContext(new SerializerOptions()));
            var longtext = ionNullStreamer.SerializeText(ionNull, context);
            var longtextAnnotated = ionNullStreamer.SerializeText(ionNullAnnotated, context);

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
