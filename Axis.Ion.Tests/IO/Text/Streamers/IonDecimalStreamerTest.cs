using Axis.Ion.IO.Text;
using Axis.Ion.IO.Text.Streamers;
using Axis.Ion.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axis.Ion.Tests.IO.Text.Streamers
{
    [TestClass]
    public class IonDecimalStreamerTest
    {
        [TestMethod]
        public void StreamText()
        {
            var context = new StreamingContext(new SerializerOptions());
            #region Null
            var ionDecimal = new IonDecimal(null);
            var ionDecimalAnnotated = new IonDecimal(null, IIonType.Annotation.ParseCollection("stuff::other::"));
            var ionDecimalStreamer = new IonDecimalStreamer();
            var text = ionDecimalStreamer.StreamText(ionDecimal, context);
            var textAnnotated = ionDecimalStreamer.StreamText(ionDecimalAnnotated, context);

            Assert.AreEqual("null.decimal", text);
            Assert.AreEqual("stuff::other::null.decimal", textAnnotated);
            #endregion

            #region value
            ionDecimal = new IonDecimal(123456789.0009m);
            var lionDecimal = new IonDecimal(0.000000098765m);
            var nionDecimal = new IonDecimal(-123456789.0009m);
            ionDecimalAnnotated = new IonDecimal(123456789.0009m, IIonType.Annotation.ParseCollection("stuff::true::"));
            var nionDecimalAnnotated = new IonDecimal(-123456789.0009m, IIonType.Annotation.ParseCollection("stuff::true::"));
            ionDecimalStreamer = new IonDecimalStreamer();

            // no exponent
            context.Options.Decimals.UseExponentNotation = false;
            text = ionDecimalStreamer.StreamText(ionDecimal, context);
            var ltext = ionDecimalStreamer.StreamText(lionDecimal, context);
            var ntext = ionDecimalStreamer.StreamText(nionDecimal, context);
            textAnnotated = ionDecimalStreamer.StreamText(ionDecimalAnnotated, context);
            var ntextAnnotated = ionDecimalStreamer.StreamText(nionDecimalAnnotated, context);

            Assert.AreEqual("123456789.0009", text);
            Assert.AreEqual("0.000000098765", ltext);
            Assert.AreEqual("stuff::true::123456789.0009", textAnnotated);
            Assert.AreEqual("-123456789.0009", ntext);
            Assert.AreEqual("stuff::true::-123456789.0009", ntextAnnotated);

            // exponent
            context.Options.Decimals.UseExponentNotation = true;
            ltext = ionDecimalStreamer.StreamText(lionDecimal, context);
            text = ionDecimalStreamer.StreamText(ionDecimal, context);
            ntext = ionDecimalStreamer.StreamText(nionDecimal, context);
            textAnnotated = ionDecimalStreamer.StreamText(ionDecimalAnnotated, context);
            ntextAnnotated = ionDecimalStreamer.StreamText(nionDecimalAnnotated, context);

            Assert.AreEqual("1.234567890009D8", text);
            Assert.AreEqual("9.8765D-8", ltext);
            Assert.AreEqual("stuff::true::1.234567890009D8", textAnnotated);
            Assert.AreEqual("-1.234567890009D8", ntext);
            Assert.AreEqual("stuff::true::-1.234567890009D8", ntextAnnotated);
            #endregion
        }

        [TestMethod]
        public void TryParse()
        {
            var nvalue = new IonDecimal(null);
            var value1 = new IonDecimal(1000);
            var value2 = new IonDecimal(-1000, "stuff", "$other_stuff");
            var value3 = new IonDecimal(0.000006557m, "stuff", "$other_stuff");
            var context = new StreamingContext(new SerializerOptions());
            var streamer = new IonDecimalStreamer();

            // no exponent
            context.Options.Decimals.UseExponentNotation = false;
            var ntext = streamer.StreamText(nvalue, context);
            var text1 = streamer.StreamText(value1, context);
            var text2 = streamer.StreamText(value2, context);
            var text3 = streamer.StreamText(value3, context);

            var nresult = streamer.ParseString(ntext);
            var result1 = streamer.ParseString(text1);
            result1.ToIonText();
            var result2 = streamer.ParseString(text2);
            var result3 = streamer.ParseString(text3);

            Assert.AreEqual(nvalue, nresult);
            Assert.AreEqual(value1, result1);
            Assert.AreEqual(value2, result2);
            Assert.AreEqual(value3, result3);

            // exponent
            context.Options.Decimals.UseExponentNotation = true;
            ntext = streamer.StreamText(nvalue, context);
            text1 = streamer.StreamText(value1, context);
            text2 = streamer.StreamText(value2, context);
            text3 = streamer.StreamText(value3, context);

            nresult = streamer.ParseString(ntext);
            result1 = streamer.ParseString(text1);
            result1.ToIonText();
            result2 = streamer.ParseString(text2);
            result3 = streamer.ParseString(text3);

            Assert.AreEqual(nvalue, nresult);
            Assert.AreEqual(value1, result1);
            Assert.AreEqual(value2, result2);
            Assert.AreEqual(value3, result3);
        }
    }
}
