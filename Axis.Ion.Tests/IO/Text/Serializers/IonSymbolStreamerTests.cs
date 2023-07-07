using Axis.Ion.IO;
using Axis.Ion.IO.Text;
using Axis.Ion.Types;
using Axis.Luna.Common.Results;
using System.IO;

namespace Axis.Ion.Tests.IO.Text.Serializers
{
    [TestClass]
    public class IonSymbolStreamerTests
    {
        [TestMethod]
        public void OperatorStreamerTest()
        {
            SerializingContext context = new SerializingContext(new SerializerOptions());

            var ion1 = new IonOperator();
            var ion2 = new IonOperator(null, IIonValue.Annotation.ParseCollection("stuff::other::").Resolve());
            var ion3 = new IonOperator(Operators.Minus, Operators.Minus);
            var ion4 = new IonOperator(new[] { Operators.Minus }, IIonValue.Annotation.ParseCollection("stuff::eurt::").Resolve());

            var text1 = IonTextSerializer.Serialize(ion1, context);
            var text2 = IonTextSerializer.Serialize(ion2, context);
            var text3 = IonTextSerializer.Serialize(ion3, context);
            var text4 = IonTextSerializer.Serialize(ion4, context);

            Assert.AreEqual("null.symbol", text1);
            Assert.AreEqual("stuff::other::null.symbol", text2);
            Assert.AreEqual("--", text3);
            Assert.AreEqual("stuff::eurt::-", text4);

            var result1 = IonTextSerializer.Parse<IonOperator>(text1);
            var result2 = IonTextSerializer.Parse<IonOperator>(text2);
            var result3 = IonTextSerializer.Parse<IonOperator>(text3);
            var result4 = IonTextSerializer.Parse<IonOperator>(text4);

            Assert.AreEqual(ion1, result1.Resolve());
            Assert.AreEqual(ion2, result2.Resolve());
            Assert.AreEqual(ion3, result3.Resolve());
            Assert.AreEqual(ion4, result4.Resolve());
        }

        [TestMethod]
        public void IdentifierStreamerTest()
        {
            SerializingContext context = new SerializingContext(new SerializerOptions());

            var ion1 = new IonTextSymbol();
            var ion2 = new IonTextSymbol(null, IIonValue.Annotation.ParseCollection("stuff::other::").Resolve());
            var ion3 = new IonTextSymbol("stuff");
            var ion4 = new IonTextSymbol("$OtherStuff_plusMore", IIonValue.Annotation.ParseCollection("stuff::eurt::").Resolve());

            var text1 = IonTextSerializer.Serialize(ion1, context);
            var text2 = IonTextSerializer.Serialize(ion2, context);
            var text3 = IonTextSerializer.Serialize(ion3, context);
            var text4 = IonTextSerializer.Serialize(ion4, context);

            Assert.AreEqual("null.symbol", text1);
            Assert.AreEqual("stuff::other::null.symbol", text2);
            Assert.AreEqual("stuff", text3);
            Assert.AreEqual("stuff::eurt::$OtherStuff_plusMore", text4);

            var result1 = IonTextSerializer.Parse<IonTextSymbol>(text1);
            var result2 = IonTextSerializer.Parse<IonTextSymbol>(text2);
            var result3 = IonTextSerializer.Parse<IonTextSymbol>(text3);
            var result4 = IonTextSerializer.Parse<IonTextSymbol>(text4);

            Assert.AreEqual(ion1, result1.Resolve());
            Assert.AreEqual(ion2, result2.Resolve());
            Assert.AreEqual(ion3, result3.Resolve());
            Assert.AreEqual(ion4, result4.Resolve());
        }

        [TestMethod]
        public void QuotedSymbolStreamerTest()
        {
            SerializingContext context = new SerializingContext(new SerializerOptions());

            var ion1 = new IonTextSymbol();
            var ion2 = new IonTextSymbol(null, IIonValue.Annotation.ParseCollection("stuff::other::").Resolve());
            var ion3 = new IonTextSymbol("the symbol");
            var ion4 = new IonTextSymbol("$OtherStuff_plusMore xyz", IIonValue.Annotation.ParseCollection("stuff::eurt::").Resolve());

            var text1 = IonTextSerializer.Serialize(ion1, context);
            var text2 = IonTextSerializer.Serialize(ion2, context);
            var text3 = IonTextSerializer.Serialize(ion3, context);
            var text4 = IonTextSerializer.Serialize(ion4, context);

            Assert.AreEqual("null.symbol", text1);
            Assert.AreEqual("stuff::other::null.symbol", text2);
            Assert.AreEqual("'the symbol'", text3);
            Assert.AreEqual("stuff::eurt::'$OtherStuff_plusMore xyz'", text4);

            var result1 = IonTextSerializer.Parse<IonTextSymbol>(text1);
            var result2 = IonTextSerializer.Parse<IonTextSymbol>(text2);
            var result3 = IonTextSerializer.Parse<IonTextSymbol>(text3);
            var result4 = IonTextSerializer.Parse<IonTextSymbol>(text4);

            Assert.AreEqual(ion1, result1.Resolve());
            Assert.AreEqual(ion2, result2.Resolve());
            Assert.AreEqual(ion3, result3.Resolve());
            Assert.AreEqual(ion4, result4.Resolve());
        }
    }
}
