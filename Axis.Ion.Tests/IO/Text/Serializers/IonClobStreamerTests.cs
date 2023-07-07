using Axis.Ion.IO;
using Axis.Ion.IO.Text;
using Axis.Ion.Types;
using Axis.Luna.Common.Results;
using System.Text;

namespace Axis.Ion.Tests.IO.Text.Serializers
{
    [TestClass]
    public class IonClobStreamerTests
    {
        [TestMethod]
        public void ClobStreamerTest()
        {
            SerializingContext context = new SerializingContext(new SerializerOptions());
            var @string = "the quick brown fox jumps over the lazy dog";
            var bytes = Encoding.ASCII.GetBytes(@string);

            var ion1 = new IonClob(null);
            var ion2 = new IonClob(null, IIonValue.Annotation.ParseCollection("stuff::other::").Resolve());
            var ion3 = new IonClob(bytes);
            var ion4 = new IonClob(bytes, IIonValue.Annotation.ParseCollection("stuff::other::").Resolve());

            #region Single line
            context.Options.Clobs.LineStyle = SerializerOptions.StringLineStyle.Singleline;
            var text1 = IonTextSerializer.Serialize(ion1, context);
            var text2 = IonTextSerializer.Serialize(ion2, context);
            var text3 = IonTextSerializer.Serialize(ion3, context);
            var text4 = IonTextSerializer.Serialize(ion4, context);

            Assert.AreEqual("null.clob", text1);
            Assert.AreEqual("stuff::other::null.clob", text2);
            Assert.AreEqual("{{ \"the quick brown fox jumps over the lazy dog\" }}", text3);
            Assert.AreEqual("stuff::other::{{ \"the quick brown fox jumps over the lazy dog\" }}", text4);

            var result1 = IonTextSerializer.Parse<IonClob>(text1);
            var result2 = IonTextSerializer.Parse<IonClob>(text2);
            var result3 = IonTextSerializer.Parse<IonClob>(text3);
            var result4 = IonTextSerializer.Parse<IonClob>(text4);

            Assert.AreEqual(ion1, result1.Resolve());
            Assert.AreEqual(ion2, result2.Resolve());
            Assert.AreEqual(ion3, result3.Resolve());
            Assert.AreEqual(ion4, result4.Resolve());
            #endregion

            #region Multi line
            context.Options.Clobs.LineStyle = SerializerOptions.StringLineStyle.Multiline;
            context.Options.Clobs.LineBreakPoint = 10;
            context.Options.IndentationStyle = SerializerOptions.IndentationStyles.Tabs;
            text1 = IonTextSerializer.Serialize(ion1, context);
            text2 = IonTextSerializer.Serialize(ion2, context);
            text3 = IonTextSerializer.Serialize(ion3, context);
            text4 = IonTextSerializer.Serialize(ion4, context);

            result1 = IonTextSerializer.Parse<IonClob>(text1);
            result2 = IonTextSerializer.Parse<IonClob>(text2);
            result3 = IonTextSerializer.Parse<IonClob>(text3);
            result4 = IonTextSerializer.Parse<IonClob>(text4);

            Assert.AreEqual(ion1, result1.Resolve());
            Assert.AreEqual(ion2, result2.Resolve());
            Assert.AreEqual(ion3, result3.Resolve());
            Assert.AreEqual(ion4, result4.Resolve());
            #endregion
        }
    }
}
