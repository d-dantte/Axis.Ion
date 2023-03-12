using Axis.Ion.IO.Text;
using Axis.Ion.IO.Text.Streamers;
using Axis.Ion.Types;
using System.Text;

namespace Axis.Ion.Tests.IO.Text.Streamers
{
    [TestClass]
    public class IonClobStreamerTests
    {
        [TestMethod]
        public void ClobStreamerTest()
        {
            SerializingContext context = new SerializingContext(new SerializerOptions());
            var streamer = new IonClobSerializer();
            var @string = "the quick brown fox jumps over the lazy dog";
            var bytes = Encoding.ASCII.GetBytes(@string);

            var ion1 = new IonClob(null);
            var ion2 = new IonClob(null, IIonType.Annotation.ParseCollection("stuff::other::"));
            var ion3 = new IonClob(bytes);
            var ion4 = new IonClob(bytes, IIonType.Annotation.ParseCollection("stuff::other::"));

            #region Single line
            context.Options.Clobs.LineStyle = SerializerOptions.StringLineStyle.Singleline;
            var text1 = streamer.SerializeText(ion1, context);
            var text2 = streamer.SerializeText(ion2, context);
            var text3 = streamer.SerializeText(ion3, context);
            var text4 = streamer.SerializeText(ion4, context);

            Assert.AreEqual("null.clob", text1);
            Assert.AreEqual("stuff::other::null.clob", text2);
            Assert.AreEqual("{{ \"the quick brown fox jumps over the lazy dog\" }}", text3);
            Assert.AreEqual("stuff::other::{{ \"the quick brown fox jumps over the lazy dog\" }}", text4);

            var result1 = streamer.ParseString(text1);
            var result2 = streamer.ParseString(text2);
            var result3 = streamer.ParseString(text3);
            var result4 = streamer.ParseString(text4);

            Assert.AreEqual(ion1, result1);
            Assert.AreEqual(ion2, result2);
            Assert.AreEqual(ion3, result3);
            Assert.AreEqual(ion4, result4);
            #endregion

            #region Multi line
            context.Options.Clobs.LineStyle = SerializerOptions.StringLineStyle.Multiline;
            context.Options.Clobs.LineBreakPoint = 10;
            context.Options.IndentationStyle = SerializerOptions.IndentationStyles.Tabs;
            text1 = streamer.SerializeText(ion1, context);
            text2 = streamer.SerializeText(ion2, context);
            text3 = streamer.SerializeText(ion3, context);
            text4 = streamer.SerializeText(ion4, context);

            result1 = streamer.ParseString(text1);
            result2 = streamer.ParseString(text2);
            result3 = streamer.ParseString(text3);
            result4 = streamer.ParseString(text4);

            Assert.AreEqual(ion1, result1);
            Assert.AreEqual(ion2, result2);
            Assert.AreEqual(ion3, result3);
            Assert.AreEqual(ion4, result4);
            #endregion
        }
    }
}
