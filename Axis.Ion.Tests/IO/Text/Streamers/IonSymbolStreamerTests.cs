using Axis.Ion.IO.Text;
using Axis.Ion.IO.Text.Streamers;
using Axis.Ion.Types;

namespace Axis.Ion.Tests.IO.Text.Streamers
{
    [TestClass]
    public class IonSymbolStreamerTests
    {
        [TestMethod]
        public void OperatorStreamerTest()
        {
            SerializingContext context = new SerializingContext(new SerializerOptions());
            var streamer = new IonOperatorSerializer();

            var ion1 = new IonOperator();
            var ion2 = new IonOperator(null, IIonType.Annotation.ParseCollection("stuff::other::"));
            var ion3 = new IonOperator(Operators.Minus, Operators.Minus);
            var ion4 = new IonOperator(new[] { Operators.Minus }, IIonType.Annotation.ParseCollection("stuff::true::"));

            var text1 = streamer.SerializeText(ion1, context);
            var text2 = streamer.SerializeText(ion2, context);
            var text3 = streamer.SerializeText(ion3, context);
            var text4 = streamer.SerializeText(ion4, context);

            Assert.AreEqual("null.symbol", text1);
            Assert.AreEqual("stuff::other::null.symbol", text2);
            Assert.AreEqual("--", text3);
            Assert.AreEqual("stuff::true::-", text4);

            var result1 = streamer.ParseString(text1);
            var result2 = streamer.ParseString(text2);
            var result3 = streamer.ParseString(text3);
            var result4 = streamer.ParseString(text4);

            Assert.AreEqual(ion1, result1);
            Assert.AreEqual(ion2, result2);
            Assert.AreEqual(ion3, result3);
            Assert.AreEqual(ion4, result4);
        }

        [TestMethod]
        public void IdentifierStreamerTest()
        {
            SerializingContext context = new SerializingContext(new SerializerOptions());
            var streamer = new IonIdentifierSerializer();

            var ion1 = new IonIdentifier();
            var ion2 = new IonIdentifier(null, IIonType.Annotation.ParseCollection("stuff::other::"));
            var ion3 = new IonIdentifier("stuff");
            var ion4 = new IonIdentifier("$OtherStuff_plusMore", IIonType.Annotation.ParseCollection("stuff::true::"));

            var text1 = streamer.SerializeText(ion1, context);
            var text2 = streamer.SerializeText(ion2, context);
            var text3 = streamer.SerializeText(ion3, context);
            var text4 = streamer.SerializeText(ion4, context);

            Assert.AreEqual("null.symbol", text1);
            Assert.AreEqual("stuff::other::null.symbol", text2);
            Assert.AreEqual("stuff", text3);
            Assert.AreEqual("stuff::true::$OtherStuff_plusMore", text4);

            var result1 = streamer.ParseString(text1);
            var result2 = streamer.ParseString(text2);
            var result3 = streamer.ParseString(text3);
            var result4 = streamer.ParseString(text4);

            Assert.AreEqual(ion1, result1);
            Assert.AreEqual(ion2, result2);
            Assert.AreEqual(ion3, result3);
            Assert.AreEqual(ion4, result4);
        }

        [TestMethod]
        public void QuotedSymbolStreamerTest()
        {
            SerializingContext context = new SerializingContext(new SerializerOptions());
            var streamer = new IonQuotedSymbolSerializer();

            var ion1 = new IonQuotedSymbol();
            var ion2 = new IonQuotedSymbol(null, IIonType.Annotation.ParseCollection("stuff::other::"));
            var ion3 = new IonQuotedSymbol("the symbol");
            var ion4 = new IonQuotedSymbol("'the symbol'", IIonType.Annotation.ParseCollection("stuff::true::"));

            var text1 = streamer.SerializeText(ion1, context);
            var text2 = streamer.SerializeText(ion2, context);
            var text3 = streamer.SerializeText(ion3, context);
            var text4 = streamer.SerializeText(ion4, context);

            Assert.AreEqual("null.symbol", text1);
            Assert.AreEqual("stuff::other::null.symbol", text2);
            Assert.AreEqual("'the symbol'", text3);
            Assert.AreEqual("stuff::true::'the symbol'", text4);

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
