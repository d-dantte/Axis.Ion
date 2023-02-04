using Axis.Ion.IO.Text;
using Axis.Ion.IO.Text.Streamers;
using Axis.Ion.Types;

namespace Axis.Ion.Tests.IO.Text.Streamers
{
    [TestClass]
    public class IonTimestampStreamerTest
    {
        [TestMethod]
        public void StreamText()
        {
            StreamingContext context = new StreamingContext(new SerializerOptions());
            #region Null
            var ionTimestamp = new IonTimestamp(null);
            var ionTimestampAnnotated = new IonTimestamp(null, IIonType.Annotation.ParseCollection("stuff::other::"));
            var ionTimestampStreamer = new IonTimestampStreamer();
            var text = ionTimestampStreamer.StreamText(ionTimestamp, context);
            var textAnnotated = ionTimestampStreamer.StreamText(ionTimestampAnnotated, context);

            Assert.AreEqual("null.timestamp", text);
            Assert.AreEqual("stuff::other::null.timestamp", textAnnotated);
            #endregion

            var timestamp = DateTimeOffset.Parse("2/2/2023 9:37:54 AM+05:00") + TimeSpan.FromMilliseconds(0.123456);
            ionTimestamp = new IonTimestamp(timestamp);
            ionTimestampAnnotated = new IonTimestamp(timestamp, IIonType.Annotation.ParseCollection("stuff::true::"));
            ionTimestampStreamer = new IonTimestampStreamer();

            #region Year precision
            context.Options.Timestamps.TimestampPrecision = SerializerOptions.TimestampPrecision.Year;
            text = ionTimestampStreamer.StreamText(ionTimestamp, context);
            textAnnotated = ionTimestampStreamer.StreamText(ionTimestampAnnotated, context);
            Assert.AreEqual("2023T", text);
            Assert.AreEqual("stuff::true::2023T", textAnnotated);
            #endregion

            #region Month precision
            context.Options.Timestamps.TimestampPrecision = SerializerOptions.TimestampPrecision.Month;
            text = ionTimestampStreamer.StreamText(ionTimestamp, context);
            textAnnotated = ionTimestampStreamer.StreamText(ionTimestampAnnotated, context);
            Assert.AreEqual("2023-02T", text);
            Assert.AreEqual("stuff::true::2023-02T", textAnnotated);
            #endregion

            #region Day precision
            context.Options.Timestamps.TimestampPrecision = SerializerOptions.TimestampPrecision.Day;
            text = ionTimestampStreamer.StreamText(ionTimestamp, context);
            textAnnotated = ionTimestampStreamer.StreamText(ionTimestampAnnotated, context);
            Assert.AreEqual("2023-02-02", text);
            Assert.AreEqual("stuff::true::2023-02-02", textAnnotated);
            #endregion

            #region Minute precision
            context.Options.Timestamps.TimestampPrecision = SerializerOptions.TimestampPrecision.Minute;
            text = ionTimestampStreamer.StreamText(ionTimestamp, context);
            textAnnotated = ionTimestampStreamer.StreamText(ionTimestampAnnotated, context);
            Assert.AreEqual("2023-02-02T09:37+05:00", text);
            Assert.AreEqual("stuff::true::2023-02-02T09:37+05:00", textAnnotated);
            #endregion

            #region Second precision
            context.Options.Timestamps.TimestampPrecision = SerializerOptions.TimestampPrecision.Second;
            text = ionTimestampStreamer.StreamText(ionTimestamp, context);
            textAnnotated = ionTimestampStreamer.StreamText(ionTimestampAnnotated, context);
            Assert.AreEqual("2023-02-02T09:37:54+05:00", text);
            Assert.AreEqual("stuff::true::2023-02-02T09:37:54+05:00", textAnnotated);
            #endregion

            #region Millisecond precision
            context.Options.Timestamps.TimestampPrecision = SerializerOptions.TimestampPrecision.MilliSecond;
            text = ionTimestampStreamer.StreamText(ionTimestamp, context);
            textAnnotated = ionTimestampStreamer.StreamText(ionTimestampAnnotated, context);
            Assert.AreEqual("2023-02-02T09:37:54.000123+05:00", text);
            Assert.AreEqual("stuff::true::2023-02-02T09:37:54.000123+05:00", textAnnotated);
            #endregion
        }

        [TestMethod]
        public void TryParse()
        {
            var timestamp = DateTimeOffset.Parse("2/2/2023 9:37:54 AM+05:00") + TimeSpan.FromMilliseconds(0.123456);
            var value1 = new IonTimestamp(null);
            var value2 = new IonTimestamp(timestamp);
            var value3 = new IonTimestamp(timestamp, "stuff", "$other_stuff");
            var context = new StreamingContext(new SerializerOptions());
            var streamer = new IonTimestampStreamer();
            var localOffset = TimeZoneInfo.Local.BaseUtcOffset;

            #region Year precision
            context.Options.Timestamps.TimestampPrecision = SerializerOptions.TimestampPrecision.Year;
            var text1 = streamer.StreamText(value1, context);
            var text2 = streamer.StreamText(value2, context);
            var text3 = streamer.StreamText(value3, context);

            var result1 = streamer.ParseString(text1);
            var result2 = streamer.ParseString(text2);
            var result3 = streamer.ParseString(text3);

            Assert.AreEqual(value1, result1);
            Assert.AreEqual(
                value2
                    .Copy(IonTimestamp.Precision.Year)
                    .SwitchOffset(localOffset),
                result2);
            Assert.AreEqual(
                value3
                .Copy(IonTimestamp.Precision.Year)
                .SwitchOffset(localOffset),
                result3);
            #endregion

            #region Month precision
            context.Options.Timestamps.TimestampPrecision = SerializerOptions.TimestampPrecision.Month;
            text1 = streamer.StreamText(value1, context);
            text2 = streamer.StreamText(value2, context);
            text3 = streamer.StreamText(value3, context);

            result1 = streamer.ParseString(text1);
            result2 = streamer.ParseString(text2);
            result3 = streamer.ParseString(text3);

            Assert.AreEqual(value1, result1);
            Assert.AreEqual(
                value2
                    .Copy(IonTimestamp.Precision.Month)
                    .SwitchOffset(localOffset),
                result2);
            Assert.AreEqual(
                value3
                .Copy(IonTimestamp.Precision.Month)
                .SwitchOffset(localOffset),
                result3);
            #endregion

            #region Day precision
            context.Options.Timestamps.TimestampPrecision = SerializerOptions.TimestampPrecision.Day;
            text1 = streamer.StreamText(value1, context);
            text2 = streamer.StreamText(value2, context);
            text3 = streamer.StreamText(value3, context);

            result1 = streamer.ParseString(text1);
            result2 = streamer.ParseString(text2);
            result3 = streamer.ParseString(text3);

            Assert.AreEqual(value1, result1);
            Assert.AreEqual(
                value2
                    .Copy(IonTimestamp.Precision.Day)
                    .SwitchOffset(localOffset),
                result2);
            Assert.AreEqual(
                value3
                .Copy(IonTimestamp.Precision.Day)
                .SwitchOffset(localOffset),
                result3);
            #endregion

            #region Minute precision
            context.Options.Timestamps.TimestampPrecision = SerializerOptions.TimestampPrecision.Minute;
            text1 = streamer.StreamText(value1, context);
            text2 = streamer.StreamText(value2, context);
            text3 = streamer.StreamText(value3, context);

            result1 = streamer.ParseString(text1);
            result2 = streamer.ParseString(text2);
            result3 = streamer.ParseString(text3);

            Assert.AreEqual(value1, result1);
            Assert.AreEqual(
                value2
                    .Copy(IonTimestamp.Precision.Minute),
                result2);
            Assert.AreEqual(
                value3
                .Copy(IonTimestamp.Precision.Minute),
                result3);
            #endregion

            #region Second precision
            context.Options.Timestamps.TimestampPrecision = SerializerOptions.TimestampPrecision.Second;
            text1 = streamer.StreamText(value1, context);
            text2 = streamer.StreamText(value2, context);
            text3 = streamer.StreamText(value3, context);

            result1 = streamer.ParseString(text1);
            result2 = streamer.ParseString(text2);
            result3 = streamer.ParseString(text3);

            Assert.AreEqual(value1, result1);
            Assert.AreEqual(
                value2
                    .Copy(IonTimestamp.Precision.Second),
                result2);
            Assert.AreEqual(
                value3
                .Copy(IonTimestamp.Precision.Second),
                result3);
            #endregion

            #region Millisecond precision
            context.Options.Timestamps.TimestampPrecision = SerializerOptions.TimestampPrecision.MilliSecond;
            text1 = streamer.StreamText(value1, context);
            text2 = streamer.StreamText(value2, context);
            text3 = streamer.StreamText(value3, context);

            result1 = streamer.ParseString(text1);
            result2 = streamer.ParseString(text2);
            result3 = streamer.ParseString(text3);

            Assert.AreEqual(value1, result1);
            Assert.AreEqual(
                value2
                    .Copy(IonTimestamp.Precision.Millisecond),
                result2);
            Assert.AreEqual(
                value3
                .Copy(IonTimestamp.Precision.Millisecond),
                result3);
            #endregion
        }
    }
}
