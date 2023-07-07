using Axis.Ion.IO;
using Axis.Ion.IO.Text;
using Axis.Ion.Types;
using Axis.Luna.Common.Results;

namespace Axis.Ion.Tests.IO.Text.Serializers
{
    [TestClass]
    public class IonBlobStreamerTests
    {
        [TestMethod]
        public void BlobStreamerTest()
        {
            SerializingContext context = new SerializingContext(new SerializerOptions());
            var bytes = new byte[50];
            new Random(DateTime.Now.Millisecond).NextBytes(bytes);
            var b64 = Convert.ToBase64String(bytes);

            var ion1 = new IonBlob();
            var ion2 = new IonBlob(null, IIonValue.Annotation.ParseCollection("stuff::other::").Resolve());
            var ion3 = new IonBlob(bytes);
            var ion4 = new IonBlob(bytes, IIonValue.Annotation.ParseCollection("stuff::yea::").Resolve());

            var text1 = IonTextSerializer.Serialize(ion1, context);
            var text2 = IonTextSerializer.Serialize(ion2, context);
            var text3 = IonTextSerializer.Serialize(ion3, context);
            var text4 = IonTextSerializer.Serialize(ion4, context);

            Assert.AreEqual("null.blob", text1);
            Assert.AreEqual("stuff::other::null.blob", text2);
            Assert.AreEqual($"{{{{ {b64} }}}}", text3);
            Assert.AreEqual($"stuff::yea::{{{{ {b64} }}}}", text4);

            var result1 = IonTextSerializer.Parse<IonBlob>(text1);
            var result2 = IonTextSerializer.Parse<IonBlob>(text2);
            var result3 = IonTextSerializer.Parse<IonBlob>(text3);
            var result4 = IonTextSerializer.Parse<IonBlob>(text4);

            Assert.AreEqual(ion1, result1.Resolve());
            Assert.AreEqual(ion2, result2.Resolve());
            Assert.AreEqual(ion3, result3.Resolve());
            Assert.AreEqual(ion4, result4.Resolve());
        }
    }
}
