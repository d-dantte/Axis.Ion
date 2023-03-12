﻿using Axis.Ion.IO.Text;
using Axis.Ion.IO.Text.Streamers;
using Axis.Ion.Types;
using System.Text.RegularExpressions;

namespace Axis.Ion.Tests.IO.Text.Streamers
{
    [TestClass] 
    public class IonFloatStreamerTests
    {
        private static readonly Regex ExponentPattern = new Regex(@"\-?\d+\.\d+E\-?\d+");

        [TestMethod]
        public void StreamText()
        {
            var context = new SerializingContext(new SerializerOptions());
            #region Null
            var ionFloat = new IonFloat(null);
            var ionFloatAnnotated = new IonFloat(null, IIonType.Annotation.ParseCollection("stuff::other::"));
            var ionFloatStreamer = new IonFloatSerializer();
            var text = ionFloatStreamer.SerializeText(ionFloat, context);
            var textAnnotated = ionFloatStreamer.SerializeText(ionFloatAnnotated, context);

            Assert.AreEqual("null.float", text);
            Assert.AreEqual("stuff::other::null.float", textAnnotated);
            #endregion

            #region value
            ionFloat = new IonFloat(123456789.0009d);
            var lionFloat = new IonFloat(0.000000098765d);
            var nionFloat = new IonFloat(-123456789.0009d);
            ionFloatAnnotated = new IonFloat(123456789.0009d, IIonType.Annotation.ParseCollection("stuff::true::"));
            var nionFloatAnnotated = new IonFloat(-123456789.0009d, IIonType.Annotation.ParseCollection("stuff::true::"));
            ionFloatStreamer = new IonFloatSerializer();

            // no exponent
            text = ionFloatStreamer.SerializeText(ionFloat, context);
            var ltext = ionFloatStreamer.SerializeText(lionFloat, context);
            var ntext = ionFloatStreamer.SerializeText(nionFloat, context);
            textAnnotated = ionFloatStreamer.SerializeText(ionFloatAnnotated, context);
            var ntextAnnotated = ionFloatStreamer.SerializeText(nionFloatAnnotated, context);

            Assert.IsTrue(ExponentPattern.IsMatch(text));
            Assert.IsTrue(ExponentPattern.IsMatch(ltext));
            Assert.IsTrue(ExponentPattern.IsMatch(textAnnotated));
            Assert.IsTrue(ExponentPattern.IsMatch(ntext));
            Assert.IsTrue(ExponentPattern.IsMatch(ntextAnnotated));
            #endregion
        }

        [TestMethod]
        public void TryParse()
        {
            var nvalue = new IonFloat(null);
            var value1 = new IonFloat(1000);
            var value2 = new IonFloat(-1000, "stuff", "$other_stuff");
            var value3 = new IonFloat(0.000006557, "stuff", "$other_stuff");
            var context = new SerializingContext(new SerializerOptions());
            var streamer = new IonFloatSerializer();

            // no exponent
            var ntext = streamer.SerializeText(nvalue, context);
            var text1 = streamer.SerializeText(value1, context);
            var text2 = streamer.SerializeText(value2, context);
            var text3 = streamer.SerializeText(value3, context);

            var nresult = streamer.ParseString(ntext);
            var result1 = streamer.ParseString(text1);
            result1.ToIonText();
            var result2 = streamer.ParseString(text2);
            var result3 = streamer.ParseString(text3);

            Assert.AreEqual(nvalue, nresult);
            Assert.AreEqual(value1, result1);
            Assert.AreEqual(value2, result2);
            Assert.AreEqual(value3, result3);
        }
    }
}
