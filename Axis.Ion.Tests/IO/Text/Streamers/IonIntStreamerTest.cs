using Axis.Ion.IO.Text;
using Axis.Ion.IO.Text.Streamers;
using Axis.Ion.Types;

namespace Axis.Ion.Tests.IO.Text.Streamers
{
    [TestClass]
    public class IonIntStreamerTest
    {
        [TestMethod]
        public void StreamText()
        {
            var context = new StreamingContext(new SerializerOptions());
            #region Null
            var ionInt = new IonInt(null);
            var ionIntAnnotated = new IonInt(null, IIonType.Annotation.ParseCollection("stuff::other::"));
            var ionIntStreamer = new IonIntStreamer();
            var text = ionIntStreamer.StreamText(ionInt, context);
            var textAnnotated = ionIntStreamer.StreamText(ionIntAnnotated, context);

            Assert.AreEqual("null.int", text);
            Assert.AreEqual("stuff::other::null.int", textAnnotated);
            #endregion

            #region Decimal
            ionInt = new IonInt(1234567890);
            var nionInt = new IonInt(-1234567890);
            ionIntAnnotated = new IonInt(1234567890, IIonType.Annotation.ParseCollection("stuff::true::"));
            var nionIntAnnotated = new IonInt(-1234567890, IIonType.Annotation.ParseCollection("stuff::true::"));
            ionIntStreamer = new IonIntStreamer();

            // no separator
            context.Options.Ints.NumberFormat = SerializerOptions.IntFormat.Decimal;
            text = ionIntStreamer.StreamText(ionInt, context);
            var ntext = ionIntStreamer.StreamText(nionInt, context);
            textAnnotated = ionIntStreamer.StreamText(ionIntAnnotated, context);
            var ntextAnnotated = ionIntStreamer.StreamText(nionIntAnnotated, context);

            Assert.AreEqual("1234567890", text);
            Assert.AreEqual("stuff::true::1234567890", textAnnotated);
            Assert.AreEqual("-1234567890", ntext);
            Assert.AreEqual("stuff::true::-1234567890", ntextAnnotated);

            // separator
            context.Options.Ints.UseDigitSeparator = true;
            text = ionIntStreamer.StreamText(ionInt, context);
            ntext = ionIntStreamer.StreamText(nionInt, context);
            textAnnotated = ionIntStreamer.StreamText(ionIntAnnotated, context);
            ntextAnnotated = ionIntStreamer.StreamText(nionIntAnnotated, context);

            Assert.AreEqual("1_234_567_890", text);
            Assert.AreEqual("stuff::true::1_234_567_890", textAnnotated);
            Assert.AreEqual("-1_234_567_890", ntext);
            Assert.AreEqual("stuff::true::-1_234_567_890", ntextAnnotated);
            #endregion

            #region BigHex
            ionInt = new IonInt(1234567890);
            nionInt = new IonInt(-1234567890);
            ionIntAnnotated = new IonInt(1234567890, IIonType.Annotation.ParseCollection("stuff::true::"));
            nionIntAnnotated = new IonInt(-1234567890, IIonType.Annotation.ParseCollection("stuff::true::"));
            ionIntStreamer = new IonIntStreamer();

            // no separator
            context.Options.Ints.NumberFormat = SerializerOptions.IntFormat.BigHex;
            context.Options.Ints.UseDigitSeparator = false;
            text = ionIntStreamer.StreamText(ionInt, context);
            ntext = ionIntStreamer.StreamText(nionInt, context);
            textAnnotated = ionIntStreamer.StreamText(ionIntAnnotated, context);
            ntextAnnotated = ionIntStreamer.StreamText(nionIntAnnotated, context);

            Assert.AreEqual("0X499602D2", text);
            Assert.AreEqual("stuff::true::0X499602D2", textAnnotated);
            Assert.AreEqual("0XB669FD2E", ntext);
            Assert.AreEqual("stuff::true::0XB669FD2E", ntextAnnotated);

            // separator
            context.Options.Ints.UseDigitSeparator = true;
            text = ionIntStreamer.StreamText(ionInt, context);
            ntext = ionIntStreamer.StreamText(nionInt, context);
            textAnnotated = ionIntStreamer.StreamText(ionIntAnnotated, context);
            ntextAnnotated = ionIntStreamer.StreamText(nionIntAnnotated, context);

            Assert.AreEqual("0X4996_02D2", text);
            Assert.AreEqual("stuff::true::0X4996_02D2", textAnnotated);
            Assert.AreEqual("0XB669_FD2E", ntext);
            Assert.AreEqual("stuff::true::0XB669_FD2E", ntextAnnotated);
            #endregion

            #region SmallHex
            ionInt = new IonInt(1234567890);
            nionInt = new IonInt(-1234567890);
            ionIntAnnotated = new IonInt(1234567890, IIonType.Annotation.ParseCollection("stuff::true::"));
            nionIntAnnotated = new IonInt(-1234567890, IIonType.Annotation.ParseCollection("stuff::true::"));
            ionIntStreamer = new IonIntStreamer();

            // no separator
            context.Options.Ints.NumberFormat = SerializerOptions.IntFormat.SmallHex;
            context.Options.Ints.UseDigitSeparator = false;
            text = ionIntStreamer.StreamText(ionInt, context);
            ntext = ionIntStreamer.StreamText(nionInt, context);
            textAnnotated = ionIntStreamer.StreamText(ionIntAnnotated, context);
            ntextAnnotated = ionIntStreamer.StreamText(nionIntAnnotated, context);

            Assert.AreEqual("0x499602d2", text);
            Assert.AreEqual("stuff::true::0x499602d2", textAnnotated);
            Assert.AreEqual("0xb669fd2e", ntext);
            Assert.AreEqual("stuff::true::0xb669fd2e", ntextAnnotated);

            // separator
            context.Options.Ints.UseDigitSeparator = true;
            text = ionIntStreamer.StreamText(ionInt, context);
            ntext = ionIntStreamer.StreamText(nionInt, context);
            textAnnotated = ionIntStreamer.StreamText(ionIntAnnotated, context);
            ntextAnnotated = ionIntStreamer.StreamText(nionIntAnnotated, context);

            Assert.AreEqual("0x4996_02d2", text);
            Assert.AreEqual("stuff::true::0x4996_02d2", textAnnotated);
            Assert.AreEqual("0xb669_fd2e", ntext);
            Assert.AreEqual("stuff::true::0xb669_fd2e", ntextAnnotated);
            #endregion

            #region BigBinary
            ionInt = new IonInt(100);
            nionInt = new IonInt(-100);
            ionIntAnnotated = new IonInt(100, IIonType.Annotation.ParseCollection("stuff::true::"));
            nionIntAnnotated = new IonInt(-100, IIonType.Annotation.ParseCollection("stuff::true::"));
            ionIntStreamer = new IonIntStreamer();

            // no separator
            context.Options.Ints.NumberFormat = SerializerOptions.IntFormat.BigBinary;
            context.Options.Ints.UseDigitSeparator = false;
            text = ionIntStreamer.StreamText(ionInt, context);
            ntext = ionIntStreamer.StreamText(nionInt, context);
            textAnnotated = ionIntStreamer.StreamText(ionIntAnnotated, context);
            ntextAnnotated = ionIntStreamer.StreamText(nionIntAnnotated, context);

            Assert.AreEqual("0B1100100", text);
            Assert.AreEqual("stuff::true::0B1100100", textAnnotated);
            Assert.AreEqual("0B10011100", ntext);
            Assert.AreEqual("stuff::true::0B10011100", ntextAnnotated);

            // separator
            context.Options.Ints.UseDigitSeparator = true;
            text = ionIntStreamer.StreamText(ionInt, context);
            ntext = ionIntStreamer.StreamText(nionInt, context);
            textAnnotated = ionIntStreamer.StreamText(ionIntAnnotated, context);
            ntextAnnotated = ionIntStreamer.StreamText(nionIntAnnotated, context);

            Assert.AreEqual("0B110_0100", text);
            Assert.AreEqual("stuff::true::0B110_0100", textAnnotated);
            Assert.AreEqual("0B1001_1100", ntext);
            Assert.AreEqual("stuff::true::0B1001_1100", ntextAnnotated);
            #endregion

            #region SmallBinary
            ionInt = new IonInt(100);
            nionInt = new IonInt(-100);
            ionIntAnnotated = new IonInt(100, IIonType.Annotation.ParseCollection("stuff::true::"));
            nionIntAnnotated = new IonInt(-100, IIonType.Annotation.ParseCollection("stuff::true::"));
            ionIntStreamer = new IonIntStreamer();

            // no separator
            context.Options.Ints.NumberFormat = SerializerOptions.IntFormat.SmallBinary;
            context.Options.Ints.UseDigitSeparator = false;
            text = ionIntStreamer.StreamText(ionInt, context);
            ntext = ionIntStreamer.StreamText(nionInt, context);
            textAnnotated = ionIntStreamer.StreamText(ionIntAnnotated, context);
            ntextAnnotated = ionIntStreamer.StreamText(nionIntAnnotated, context);

            Assert.AreEqual("0b1100100", text);
            Assert.AreEqual("stuff::true::0b1100100", textAnnotated);
            Assert.AreEqual("0b10011100", ntext);
            Assert.AreEqual("stuff::true::0b10011100", ntextAnnotated);

            // separator
            context.Options.Ints.UseDigitSeparator = true;
            text = ionIntStreamer.StreamText(ionInt, context);
            ntext = ionIntStreamer.StreamText(nionInt, context);
            textAnnotated = ionIntStreamer.StreamText(ionIntAnnotated, context);
            ntextAnnotated = ionIntStreamer.StreamText(nionIntAnnotated, context);

            Assert.AreEqual("0b110_0100", text);
            Assert.AreEqual("stuff::true::0b110_0100", textAnnotated);
            Assert.AreEqual("0b1001_1100", ntext);
            Assert.AreEqual("stuff::true::0b1001_1100", ntextAnnotated);
            #endregion
        }


        [TestMethod]
        public void TryParse()
        {
            var nvalue = new IonInt(null);
            var value1 = new IonInt(1000);
            var value2 = new IonInt(-1000, "stuff", "$other_stuff");
            var context = new StreamingContext(new SerializerOptions());
            var streamer = new IonIntStreamer();

            #region decimal
            context.Options.Ints.UseDigitSeparator = false;
            context.Options.Ints.NumberFormat = SerializerOptions.IntFormat.Decimal;
            var ntext = streamer.StreamText(nvalue, context);
            var text1 = streamer.StreamText(value1, context);
            var text2 = streamer.StreamText(value2, context);
            var nresult = streamer.ParseString(ntext);
            var result1 = streamer.ParseString(text1);
            var result2 = streamer.ParseString(text2);

            context.Options.Ints.UseDigitSeparator = true;
            context.Options.Ints.NumberFormat = SerializerOptions.IntFormat.Decimal;
            text1 = streamer.StreamText(value1, context);
            text2 = streamer.StreamText(value2, context);
            var result3 = streamer.ParseString(text1);
            var result4 = streamer.ParseString(text2);

            Assert.AreEqual(nvalue, nresult);
            Assert.AreEqual(value1, result1);
            Assert.AreEqual(value2, result2);
            Assert.AreEqual(value1, result3);
            Assert.AreEqual(value2, result4);
            #endregion

            #region big hex
            context.Options.Ints.UseDigitSeparator = false;
            context.Options.Ints.NumberFormat = SerializerOptions.IntFormat.BigHex;
            text1 = streamer.StreamText(value1, context);
            text2 = streamer.StreamText(value2, context);
            result1 = streamer.ParseString(text1);
            result2 = streamer.ParseString(text2);

            context.Options.Ints.UseDigitSeparator = true;
            context.Options.Ints.NumberFormat = SerializerOptions.IntFormat.BigHex;
            text1 = streamer.StreamText(value1, context);
            text2 = streamer.StreamText(value2, context);
            result3 = streamer.ParseString(text1);
            result4 = streamer.ParseString(text2);

            Assert.AreEqual(value1, result1);
            Assert.AreEqual(value2, result2);
            Assert.AreEqual(value1, result3);
            Assert.AreEqual(value2, result4);
            #endregion

            #region small hex
            context.Options.Ints.UseDigitSeparator = false;
            context.Options.Ints.NumberFormat = SerializerOptions.IntFormat.SmallHex;
            text1 = streamer.StreamText(value1, context);
            text2 = streamer.StreamText(value2, context);
            result1 = streamer.ParseString(text1);
            result2 = streamer.ParseString(text2);

            context.Options.Ints.UseDigitSeparator = true;
            context.Options.Ints.NumberFormat = SerializerOptions.IntFormat.SmallHex;
            text1 = streamer.StreamText(value1, context);
            text2 = streamer.StreamText(value2, context);
            result3 = streamer.ParseString(text1);
            result4 = streamer.ParseString(text2);

            Assert.AreEqual(value1, result1);
            Assert.AreEqual(value2, result2);
            Assert.AreEqual(value1, result3);
            Assert.AreEqual(value2, result4);
            #endregion

            #region big binary
            context.Options.Ints.UseDigitSeparator = false;
            context.Options.Ints.NumberFormat = SerializerOptions.IntFormat.BigBinary;
            text1 = streamer.StreamText(value1, context);
            text2 = streamer.StreamText(value2, context);
            result1 = streamer.ParseString(text1);
            result2 = streamer.ParseString(text2);

            context.Options.Ints.UseDigitSeparator = true;
            context.Options.Ints.NumberFormat = SerializerOptions.IntFormat.BigBinary;
            text1 = streamer.StreamText(value1, context);
            text2 = streamer.StreamText(value2, context);
            result3 = streamer.ParseString(text1);
            result4 = streamer.ParseString(text2);

            Assert.AreEqual(value1, result1);
            Assert.AreEqual(value2, result2);
            Assert.AreEqual(value1, result3);
            Assert.AreEqual(value2, result4);
            #endregion

            #region small binary
            context.Options.Ints.UseDigitSeparator = false;
            context.Options.Ints.NumberFormat = SerializerOptions.IntFormat.SmallBinary;
            text1 = streamer.StreamText(value1, context);
            text2 = streamer.StreamText(value2, context);
            result1 = streamer.ParseString(text1);
            result2 = streamer.ParseString(text2);

            context.Options.Ints.UseDigitSeparator = true;
            context.Options.Ints.NumberFormat = SerializerOptions.IntFormat.SmallBinary;
            text1 = streamer.StreamText(value1, context);
            text2 = streamer.StreamText(value2, context);
            result3 = streamer.ParseString(text1);
            result4 = streamer.ParseString(text2);

            Assert.AreEqual(value1, result1);
            Assert.AreEqual(value2, result2);
            Assert.AreEqual(value1, result3);
            Assert.AreEqual(value2, result4);
            #endregion
        }
    }
}
