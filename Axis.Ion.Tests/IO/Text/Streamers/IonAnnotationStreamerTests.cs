using Axis.Ion.IO.Text.Streamers;
using Axis.Ion.Types;
using Axis.Luna.Extensions;

namespace Axis.Ion.Tests.IO.Text.Streamers
{
    [TestClass]
    public class IonAnnotationStreamerTest
    {
        [TestMethod]
        public void StreamText_ReturnsStringRepresentation()
        {
            var annotation = IIonType.Annotation.Parse("plain_annotation");
            var annotationText = IonAnnotationSerializer.StreamText(annotation);

            Assert.AreEqual($"{annotation.Value}::", annotationText);
        }

        [TestMethod]
        public void StreamText_WithAnnotationArray_ReturnsStringRepresentation()
        {
            var annotation = IIonType.Annotation.Parse("plain_annotation");
            var annotation2 = IIonType.Annotation.Parse("'another'");
            var annotationsText = IonAnnotationSerializer.StreamText(new[] { annotation, annotation2 });

            Assert.AreEqual($"{annotation.Value}::{annotation2.Value}::", annotationsText);
        }

        [TestMethod]
        public void ParseString_WithValidTokenString_ReturnsAnnotationArray()
        {
            var annotationText = "$the_identifier::";
            var annotation = IonAnnotationSerializer.ParseString(annotationText);

            Assert.IsNotNull(annotation);
            Assert.AreEqual(1, annotation.Length);
            Assert.IsTrue(annotationText.StartsWith(annotation[0].Value));

            annotationText = "$the_identifier::'and others'::";
            annotation = IonAnnotationSerializer.ParseString(annotationText);

            Assert.IsNotNull(annotation);
            Assert.AreEqual(2, annotation.Length);
            Assert.AreEqual(
                annotationText,
                annotation.Select(x => $"{x.Value}::").JoinUsing(""));
        }
    }
}
