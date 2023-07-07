using Axis.Ion.IO;
using Axis.Ion.IO.Text;
using Axis.Ion.Types;
using Axis.Luna.Common.Results;

namespace Axis.Ion.Tests.IO.Text.Serializers
{
    [TestClass]
    public class IonNullStreamerTests
    {
        [TestMethod]
        public void Serialize()
        {
            var ionNull = new IonNull();
            var ionNullAnnotated = new IonNull("stuff", "other");

            var text = IonTextSerializer.Serialize(ionNull, new SerializingContext(new SerializerOptions()));
            var textAnnotated = IonTextSerializer.Serialize(ionNullAnnotated, new SerializingContext(new SerializerOptions()));
            Assert.AreEqual("null", text);
            Assert.AreEqual("stuff::other::null", textAnnotated);


            var context = new SerializingContext(new SerializerOptions());
            context.Options.Nulls.UseLongFormNulls = true;

            text = IonTextSerializer.Serialize(ionNull, context);
            textAnnotated = IonTextSerializer.Serialize(ionNullAnnotated, context);
            Assert.AreEqual("null.null", text);
            Assert.AreEqual("stuff::other::null.null", textAnnotated);
        }

        [TestMethod]
        public void TryParse()
        {
            var ionNull = new IonNull();
            var ionNullAnnotated = new IonNull("stuff", "other");
            var context = new SerializingContext(new SerializerOptions());
            context.Options.Nulls.UseLongFormNulls = true;

            var text = IonTextSerializer.Serialize(ionNull, new SerializingContext(new SerializerOptions()));
            var textAnnotated = IonTextSerializer.Serialize(ionNullAnnotated, new SerializingContext(new SerializerOptions()));
            var longtext = IonTextSerializer.Serialize(ionNull, context);
            var longtextAnnotated = IonTextSerializer.Serialize(ionNullAnnotated, context);

            var textResult = IonTextSerializer.Parse<IonNull>(text);
            var textAnnotatedResult = IonTextSerializer.Parse<IonNull>(textAnnotated);
            var longtextResult = IonTextSerializer.Parse<IonNull>(longtext);
            var longtextAnnotatedResult = IonTextSerializer.Parse<IonNull>(longtextAnnotated);

            Assert.AreEqual(ionNull, textResult.Resolve());
            Assert.AreEqual(ionNullAnnotated, textAnnotatedResult.Resolve());
            Assert.AreEqual(ionNull, longtextResult.Resolve());
            Assert.AreEqual(ionNullAnnotated, longtextAnnotatedResult.Resolve());
        }
    }
}
