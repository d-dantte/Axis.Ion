using Axis.Ion.IO;
using Axis.Ion.IO.Text;
using Axis.Ion.Types;
using Axis.Luna.Common.Results;

namespace Axis.Ion.Tests.IO.Text.Serializers
{
    [TestClass]
    public class IonListStreamerTests
    {
        [TestMethod]
        public void ListStreamerTest()
        {
            var context = new SerializingContext(new SerializerOptions());

            var ion1 = new IonList();
            var ion2 = new IonList(IIonValue.Annotation.ParseCollection("stuff::other::").Resolve());
            IonList ion3 = new IonList.Initializer
            {
                54m,
                116,
                "some string",
                new IonList.Initializer
                {
                    true
                }
            };
            IonList ion4 = new IonList.Initializer(IIonValue.Annotation.ParseCollection("stuff::eurt::").Resolve())
            {
                54m,
                116,
                "some string",
                new IonList.Initializer
                {
                    true
                }
            };

            #region Dont use multiline
            context.Options.Lists.UseMultipleLines = false;
            var text1 = IonTextSerializer.Serialize(ion1, context);
            var text2 = IonTextSerializer.Serialize(ion2, context);
            var text3 = IonTextSerializer.Serialize(ion3, context);
            var text4 = IonTextSerializer.Serialize(ion4, context);

            Assert.AreEqual("null.list", text1);
            Assert.AreEqual("stuff::other::null.list", text2);
            Assert.AreEqual("[54.0, 116, \"some string\", [true]]", text3);
            Assert.AreEqual("stuff::eurt::[54.0, 116, \"some string\", [true]]", text4);

            var result1 = IonTextSerializer.Parse<IonList>(text1);
            var result2 = IonTextSerializer.Parse<IonList>(text2);
            var result3 = IonTextSerializer.Parse<IonList>(text3);
            var result4 = IonTextSerializer.Parse<IonList>(text4);

            Assert.AreEqual(ion1, result1.Resolve());
            Assert.AreEqual(ion2, result2.Resolve());
            Assert.AreEqual(ion3, result3.Resolve());
            Assert.AreEqual(ion4, result4.Resolve());
            #endregion

            #region use multiline + indentation
            context.Options.Lists.UseMultipleLines = true;
            context.Options.IndentationStyle = SerializerOptions.IndentationStyles.Tabs;
            text1 = IonTextSerializer.Serialize(ion1, context);
            text2 = IonTextSerializer.Serialize(ion2, context);
            text3 = IonTextSerializer.Serialize(ion3, context);
            text4 = IonTextSerializer.Serialize(ion4, context);

            Assert.AreEqual("null.list", text1);
            Assert.AreEqual("stuff::other::null.list", text2);
            Assert.AreEqual("[\r\n\t54.0,\r\n\t116,\r\n\t\"some string\",\r\n\t[\r\n\t\ttrue\r\n\t]\r\n]", text3);
            Assert.AreEqual("stuff::eurt::[\r\n\t54.0,\r\n\t116,\r\n\t\"some string\",\r\n\t[\r\n\t\ttrue\r\n\t]\r\n]", text4);

            result1 = IonTextSerializer.Parse<IonList>(text1);
            result2 = IonTextSerializer.Parse<IonList>(text2);
            result3 = IonTextSerializer.Parse<IonList>(text3);
            result4 = IonTextSerializer.Parse<IonList>(text4);

            Assert.AreEqual(ion1, result1.Resolve());
            Assert.AreEqual(ion2, result2.Resolve());
            Assert.AreEqual(ion3, result3.Resolve());
            Assert.AreEqual(ion4, result4.Resolve());
            #endregion
        }
    }
}
