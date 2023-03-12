using Axis.Ion.IO.Text;
using Axis.Ion.IO.Text.Streamers;
using Axis.Ion.Types;

namespace Axis.Ion.Tests.IO.Text.Streamers
{
    [TestClass]
    public class IonStructStreamerTests
    {
        [TestMethod]
        public void ListStreamerTest()
        {
            var context = new SerializingContext(new SerializerOptions());
            var streamer = new IonStructSerializer();

            var ion1 = new IonStruct();
            var ion2 = new IonStruct(IIonType.Annotation.ParseCollection("stuff::other::"));
            IonStruct ion3 = new IonStruct.Initializer
            {
                ["stuff"] = 54m,
                ["'number of stuffs'"] = 116,
                ["some_string"] = "some string",
                ["a_list"] = new IonList.Initializer
                {
                    true
                }
            };
            IonStruct ion4 = new IonStruct.Initializer(IIonType.Annotation.ParseCollection("stuff::true::"))
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
            var text1 = streamer.SerializeText(ion1, context);
            var text2 = streamer.SerializeText(ion2, context);
            var text3 = streamer.SerializeText(ion3, context);
            var text4 = streamer.SerializeText(ion4, context);

            Assert.AreEqual("null.struct", text1);
            Assert.AreEqual("stuff::other::null.struct", text2);
            Assert.AreEqual("{'number of stuffs':116, a_list:[true], some_string:\"some string\", stuff:54.0}", text3);
            Assert.AreEqual("stuff::true::{'number of stuffs':116, a_list:[true], some_string:\"some string\", stuff:54.0}", text4);

            var result1 = streamer.ParseString(text1);
            var result2 = streamer.ParseString(text2);
            var result3 = streamer.ParseString(text3);
            var result4 = streamer.ParseString(text4);

            Assert.AreEqual(ion1, result1);
            Assert.AreEqual(ion2, result2);
            Assert.AreEqual(ion3, result3);
            Assert.AreEqual(ion4, result4);
            #endregion

            #region single line with quoted identifier property names
            context.Options.Structs.UseMultipleLines = false;
            context.Options.Structs.UseQuotedIdentifierPropertyNames = true;
            text1 = streamer.SerializeText(ion1, context);
            text2 = streamer.SerializeText(ion2, context);
            text3 = streamer.SerializeText(ion3, context);
            text4 = streamer.SerializeText(ion4, context);

            Assert.AreEqual("null.struct", text1);
            Assert.AreEqual("stuff::other::null.struct", text2);
            Assert.AreEqual("{'number of stuffs':116, \"a_list\":[true], \"some_string\":\"some string\", \"stuff\":54.0}", text3);
            Assert.AreEqual("stuff::true::{'number of stuffs':116, \"a_list\":[true], \"some_string\":\"some string\", \"stuff\":54.0}", text4);

            result1 = streamer.ParseString(text1);
            result2 = streamer.ParseString(text2);
            result3 = streamer.ParseString(text3);
            result4 = streamer.ParseString(text4);

            Assert.AreEqual(ion1, result1);
            Assert.AreEqual(ion2, result2);
            Assert.AreEqual(ion3, result3);
            Assert.AreEqual(ion4, result4);
            #endregion

            #region multi line
            context.Options.Structs.UseMultipleLines = true;
            context.Options.Structs.UseQuotedIdentifierPropertyNames = false;
            context.Options.IndentationStyle = SerializerOptions.IndentationStyles.Spaces;

            text1 = streamer.SerializeText(ion1, context);
            text2 = streamer.SerializeText(ion2, context);
            text3 = streamer.SerializeText(ion3, context);
            text4 = streamer.SerializeText(ion4, context);

            Assert.AreEqual("null.struct", text1);
            Assert.AreEqual("stuff::other::null.struct", text2);
            Assert.AreEqual("{\r\n    'number of stuffs':116,\r\n    a_list:[true],\r\n    some_string:\"some string\",\r\n    stuff:54.0\r\n}", text3);
            Assert.AreEqual("stuff::true::{\r\n    'number of stuffs':116,\r\n    a_list:[true],\r\n    some_string:\"some string\",\r\n    stuff:54.0\r\n}", text4);

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
