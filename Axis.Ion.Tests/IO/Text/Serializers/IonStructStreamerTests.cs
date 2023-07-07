using Axis.Ion.IO;
using Axis.Ion.IO.Text;
using Axis.Ion.Types;
using Axis.Luna.Common.Results;

namespace Axis.Ion.Tests.IO.Text.Serializers
{
    [TestClass]
    public class IonStructStreamerTests
    {
        [TestMethod]
        public void ListStreamerTest()
        {
            var context = new SerializingContext(new SerializerOptions());

            var ion1 = new IonStruct();
            var ion2 = new IonStruct(IIonValue.Annotation.ParseCollection("stuff::other::").Resolve());
            IonStruct ion3 = new IonStruct.Initializer
            {
                ["stuff"] = 54m,
                ["number of stuffs"] = 116,
                ["some_string"] = "some string",
                ["a_list"] = new IonList.Initializer
                {
                    true
                }
            };
            IonStruct ion4 = new IonStruct.Initializer(IIonValue.Annotation.ParseCollection("stuff::eurt::").Resolve())
            {
                ["stuff"] = 54m,
                ["'number of stuffs'"] = 116,
                ["some_string"] = "some string",
                ["a_list"] = new IonList.Initializer
                {
                    true
                }
            };

            #region single line
            context.Options.Structs.UseMultipleLines = false;
            var text1 = IonTextSerializer.Serialize(ion1, context);
            var text2 = IonTextSerializer.Serialize(ion2, context);
            var text3 = IonTextSerializer.Serialize(ion3, context);
            var text4 = IonTextSerializer.Serialize(ion4, context);

            Assert.AreEqual("null.struct", text1);
            Assert.AreEqual("stuff::other::null.struct", text2);
            Assert.AreEqual("{'number of stuffs':116, a_list:[true], some_string:\"some string\", stuff:54.0}", text3);
            Assert.AreEqual("stuff::eurt::{'number of stuffs':116, a_list:[true], some_string:\"some string\", stuff:54.0}", text4);

            var result1 = IonTextSerializer.Parse<IonStruct>(text1);
            var result2 = IonTextSerializer.Parse<IonStruct>(text2);
            var result3 = IonTextSerializer.Parse<IonStruct>(text3);
            var result4 = IonTextSerializer.Parse<IonStruct>(text4);

            Assert.AreEqual(ion1, result1.Resolve());
            Assert.AreEqual(ion2, result2.Resolve());
            Assert.AreEqual(ion3, result3.Resolve());
            Assert.AreEqual(ion4, result4.Resolve());
            #endregion

            #region single line with quoted identifier property names
            context.Options.Structs.UseMultipleLines = false;
            context.Options.Structs.UseQuotedIdentifierPropertyNames = true;
            text1 = IonTextSerializer.Serialize(ion1, context);
            text2 = IonTextSerializer.Serialize(ion2, context);
            text3 = IonTextSerializer.Serialize(ion3, context);
            text4 = IonTextSerializer.Serialize(ion4, context);

            Assert.AreEqual("null.struct", text1);
            Assert.AreEqual("stuff::other::null.struct", text2);
            Assert.AreEqual("{\"number of stuffs\":116, \"a_list\":[true], \"some_string\":\"some string\", \"stuff\":54.0}", text3);
            Assert.AreEqual("stuff::eurt::{\"number of stuffs\":116, \"a_list\":[true], \"some_string\":\"some string\", \"stuff\":54.0}", text4);

            result1 = IonTextSerializer.Parse<IonStruct>(text1);
            result2 = IonTextSerializer.Parse<IonStruct>(text2);
            result3 = IonTextSerializer.Parse<IonStruct>(text3);
            result4 = IonTextSerializer.Parse<IonStruct>(text4);

            Assert.AreEqual(ion1, result1.Resolve());
            Assert.AreEqual(ion2, result2.Resolve());
            Assert.AreEqual(ion3, result3.Resolve());
            Assert.AreEqual(ion4, result4.Resolve());
            #endregion

            #region multi line
            context.Options.Structs.UseMultipleLines = true;
            context.Options.Structs.UseQuotedIdentifierPropertyNames = false;
            context.Options.IndentationStyle = SerializerOptions.IndentationStyles.Spaces;

            text1 = IonTextSerializer.Serialize(ion1, context);
            text2 = IonTextSerializer.Serialize(ion2, context);
            text3 = IonTextSerializer.Serialize(ion3, context);
            text4 = IonTextSerializer.Serialize(ion4, context);

            Assert.AreEqual("null.struct", text1);
            Assert.AreEqual("stuff::other::null.struct", text2);
            Assert.AreEqual("{\r\n    'number of stuffs':116,\r\n    a_list:[true],\r\n    some_string:\"some string\",\r\n    stuff:54.0\r\n}", text3);
            Assert.AreEqual("stuff::eurt::{\r\n    'number of stuffs':116,\r\n    a_list:[true],\r\n    some_string:\"some string\",\r\n    stuff:54.0\r\n}", text4);

            result1 = IonTextSerializer.Parse<IonStruct>(text1);
            result2 = IonTextSerializer.Parse<IonStruct>(text2);
            result3 = IonTextSerializer.Parse<IonStruct>(text3);
            result4 = IonTextSerializer.Parse<IonStruct>(text4);

            Assert.AreEqual(ion1, result1.Resolve());
            Assert.AreEqual(ion2, result2.Resolve());
            Assert.AreEqual(ion3, result3.Resolve());
            Assert.AreEqual(ion4, result4.Resolve());
            #endregion
        }
    }
}
