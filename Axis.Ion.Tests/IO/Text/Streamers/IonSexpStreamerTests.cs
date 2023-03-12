﻿using Axis.Ion.IO.Text;
using Axis.Ion.IO.Text.Streamers;
using Axis.Ion.Types;

namespace Axis.Ion.Tests.IO.Text.Streamers
{
    [TestClass]
    public class IonSexpStreamerTests
    {
        [TestMethod]
        public void SexpStreamerTest()
        {
            var context = new SerializingContext(new SerializerOptions());
            var streamer = new IonSexpSerializer();

            var ion1 = new IonSexp();
            var ion2 = new IonSexp(IIonType.Annotation.ParseCollection("stuff::other::"));
            IonSexp ion3 = new IonSexp.Initializer
            {
                54m,
                116,
                "some string",
                new IonSexp.Initializer
                {
                    true,
                    new IonList.Initializer
                    {

                    }
                }
            };
            IonSexp ion4 = new IonSexp.Initializer(IIonType.Annotation.ParseCollection("stuff::true::"))
            {
                54m,
                116,
                "some string",
                new IonSexp.Initializer
                {
                    true,
                    new IonList.Initializer
                    {

                    }
                }
            };

            #region Dont use multiline
            context.Options.Sexps.UseMultipleLines = false;
            var text1 = streamer.SerializeText(ion1, context);
            var text2 = streamer.SerializeText(ion2, context);
            var text3 = streamer.SerializeText(ion3, context);
            var text4 = streamer.SerializeText(ion4, context);

            Assert.AreEqual("null.sexp", text1);
            Assert.AreEqual("stuff::other::null.sexp", text2);
            Assert.AreEqual("( 54.0 116 \"some string\" ( true [] ) )", text3);
            Assert.AreEqual("stuff::true::( 54.0 116 \"some string\" ( true [] ) )", text4);

            var result1 = streamer.ParseString(text1);
            var result2 = streamer.ParseString(text2);
            var result3 = streamer.ParseString(text3);
            var result4 = streamer.ParseString(text4);

            Assert.AreEqual(ion1, result1);
            Assert.AreEqual(ion2, result2);
            Assert.AreEqual(ion3, result3);
            Assert.AreEqual(ion4, result4);
            #endregion

            #region use multiline + indentation
            context.Options.Sexps.UseMultipleLines = true;
            context.Options.IndentationStyle = SerializerOptions.IndentationStyles.Tabs;
            text1 = streamer.SerializeText(ion1, context);
            text2 = streamer.SerializeText(ion2, context);
            text3 = streamer.SerializeText(ion3, context);
            text4 = streamer.SerializeText(ion4, context);

            Assert.AreEqual("null.sexp", text1);
            Assert.AreEqual("stuff::other::null.sexp", text2);
            Assert.AreEqual("(\r\n\t54.0\r\n\t116\r\n\t\"some string\"\r\n\t(\r\n\t\ttrue\r\n\t\t[]\r\n\t)\r\n)", text3);
            Assert.AreEqual("stuff::true::(\r\n\t54.0\r\n\t116\r\n\t\"some string\"\r\n\t(\r\n\t\ttrue\r\n\t\t[]\r\n\t)\r\n)", text4);

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
