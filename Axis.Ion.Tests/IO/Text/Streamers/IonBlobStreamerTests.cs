using Axis.Ion.IO.Text;
using Axis.Ion.IO.Text.Streamers;
using Axis.Ion.Types;

namespace Axis.Ion.Tests.IO.Text.Streamers
{
    [TestClass]
    public class IonBlobStreamerTests
    {
        [TestMethod]
        public void BlobStreamerTest()
        {
            SerializingContext context = new SerializingContext(new SerializerOptions());
            var streamer = new IonBlobSterializer();
            var bytes = new byte[50];
            new Random(DateTime.Now.Millisecond).NextBytes(bytes);
            var b64 = Convert.ToBase64String(bytes);

            var ion1 = new IonBlob();
            var ion2 = new IonBlob(null, IIonType.Annotation.ParseCollection("stuff::other::"));
            var ion3 = new IonBlob(bytes);
            var ion4 = new IonBlob(bytes, IIonType.Annotation.ParseCollection("stuff::true::"));

            var text1 = streamer.SerializeText(ion1, context);
            var text2 = streamer.SerializeText(ion2, context);
            var text3 = streamer.SerializeText(ion3, context);
            var text4 = streamer.SerializeText(ion4, context);

            Assert.AreEqual("null.blob", text1);
            Assert.AreEqual("stuff::other::null.blob", text2);
            Assert.AreEqual($"{{{{ {b64} }}}}", text3);
            Assert.AreEqual($"stuff::true::{{{{ {b64} }}}}", text4);

            var result1 = streamer.ParseString(text1);
            var result2 = streamer.ParseString(text2);
            var result3 = streamer.ParseString(text3);
            var result4 = streamer.ParseString(text4);

            Assert.AreEqual(ion1, result1);
            Assert.AreEqual(ion2, result2);
            Assert.AreEqual(ion3, result3);
            Assert.AreEqual(ion4, result4);
        }
    }
}
