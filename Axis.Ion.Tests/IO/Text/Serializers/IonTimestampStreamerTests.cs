using Axis.Ion.IO;
using Axis.Ion.IO.Text;
using Axis.Ion.Types;
using Axis.Luna.Common.Results;

namespace Axis.Ion.Tests.IO.Text.Serializers
{
    [TestClass]
    public class IonTimestampStreamerTests
    {
        [TestMethod]
        public void Serialize()
        {
            SerializingContext context = new SerializingContext(new SerializerOptions());

            #region Null
            var ionTimestamp = new IonTimestamp(null);
            var ionTimestampAnnotated = new IonTimestamp(null, IIonValue.Annotation.ParseCollection("stuff::other::").Resolve());
            var text = IonTextSerializer.Serialize(ionTimestamp, context);
            var textAnnotated = IonTextSerializer.Serialize(ionTimestampAnnotated, context);

            Assert.AreEqual("null.timestamp", text);
            Assert.AreEqual("stuff::other::null.timestamp", textAnnotated);
            #endregion

            var timestamp = DateTimeOffset.Parse("2/2/2023 9:37:54 AM+05:00") + TimeSpan.FromMilliseconds(0.123456);
            ionTimestamp = new IonTimestamp(timestamp);
            ionTimestampAnnotated = new IonTimestamp(timestamp, IIonValue.Annotation.ParseCollection("stuff::eurt::").Resolve());

            #region Year precision
            context.Options.Timestamps.TimestampPrecision = SerializerOptions.TimestampPrecision.Year;
            text = IonTextSerializer.Serialize(ionTimestamp, context);
            textAnnotated = IonTextSerializer.Serialize(ionTimestampAnnotated, context);
            Assert.AreEqual("2023T", text);
            Assert.AreEqual("stuff::eurt::2023T", textAnnotated);
            #endregion

            #region Month precision
            context.Options.Timestamps.TimestampPrecision = SerializerOptions.TimestampPrecision.Month;
            text = IonTextSerializer.Serialize(ionTimestamp, context);
            textAnnotated = IonTextSerializer.Serialize(ionTimestampAnnotated, context);
            Assert.AreEqual("2023-02T", text);
            Assert.AreEqual("stuff::eurt::2023-02T", textAnnotated);
            #endregion

            #region Day precision
            context.Options.Timestamps.TimestampPrecision = SerializerOptions.TimestampPrecision.Day;
            text = IonTextSerializer.Serialize(ionTimestamp, context);
            textAnnotated = IonTextSerializer.Serialize(ionTimestampAnnotated, context);
            Assert.AreEqual("2023-02-02", text);
            Assert.AreEqual("stuff::eurt::2023-02-02", textAnnotated);
            #endregion

            #region Minute precision
            context.Options.Timestamps.TimestampPrecision = SerializerOptions.TimestampPrecision.Minute;
            text = IonTextSerializer.Serialize(ionTimestamp, context);
            textAnnotated = IonTextSerializer.Serialize(ionTimestampAnnotated, context);
            Assert.AreEqual("2023-02-02T09:37+05:00", text);
            Assert.AreEqual("stuff::eurt::2023-02-02T09:37+05:00", textAnnotated);
            #endregion

            #region Second precision
            context.Options.Timestamps.TimestampPrecision = SerializerOptions.TimestampPrecision.Second;
            text = IonTextSerializer.Serialize(ionTimestamp, context);
            textAnnotated = IonTextSerializer.Serialize(ionTimestampAnnotated, context);
            Assert.AreEqual("2023-02-02T09:37:54+05:00", text);
            Assert.AreEqual("stuff::eurt::2023-02-02T09:37:54+05:00", textAnnotated);
            #endregion

            #region Millisecond precision
            context.Options.Timestamps.TimestampPrecision = SerializerOptions.TimestampPrecision.MilliSecond;
            text = IonTextSerializer.Serialize(ionTimestamp, context);
            textAnnotated = IonTextSerializer.Serialize(ionTimestampAnnotated, context);
            Assert.AreEqual("2023-02-02T09:37:54.000123+05:00", text);
            Assert.AreEqual("stuff::eurt::2023-02-02T09:37:54.000123+05:00", textAnnotated);
            #endregion
        }

        [TestMethod]
        public void TryParse()
        {
            var timestamp = DateTimeOffset.Parse("2/2/2023 9:37:54 AM+05:00") + TimeSpan.FromMilliseconds(0.123456);
            var value1 = new IonTimestamp(null);
            var value2 = new IonTimestamp(timestamp);
            var value3 = new IonTimestamp(timestamp, "stuff", "$other_stuff");
            var context = new SerializingContext(new SerializerOptions());
            var localOffset = TimeZoneInfo.Local.BaseUtcOffset;

            #region Year precision
            context.Options.Timestamps.TimestampPrecision = SerializerOptions.TimestampPrecision.Year;
            var text1 = IonTextSerializer.Serialize(value1, context);
            var text2 = IonTextSerializer.Serialize(value2, context);
            var text3 = IonTextSerializer.Serialize(value3, context);

            var result1 = IonTextSerializer.Parse<IonTimestamp>(text1);
            var result2 = IonTextSerializer.Parse<IonTimestamp>(text2);
            var result3 = IonTextSerializer.Parse<IonTimestamp>(text3);

            Assert.AreEqual(value1, result1.Resolve());
            Assert.AreEqual(
                value2
                    .Copy(IonTimestamp.Precision.Year)
                    .SwitchOffset(localOffset),
                result2.Resolve());
            Assert.AreEqual(
                value3
                .Copy(IonTimestamp.Precision.Year)
                .SwitchOffset(localOffset),
                result3.Resolve());
            #endregion

            #region Month precision
            context.Options.Timestamps.TimestampPrecision = SerializerOptions.TimestampPrecision.Month;
            text1 = IonTextSerializer.Serialize(value1, context);
            text2 = IonTextSerializer.Serialize(value2, context);
            text3 = IonTextSerializer.Serialize(value3, context);

            result1 = IonTextSerializer.Parse<IonTimestamp>(text1);
            result2 = IonTextSerializer.Parse<IonTimestamp>(text2);
            result3 = IonTextSerializer.Parse<IonTimestamp>(text3);

            Assert.AreEqual(value1, result1.Resolve());
            Assert.AreEqual(
                value2
                    .Copy(IonTimestamp.Precision.Month)
                    .SwitchOffset(localOffset),
                result2.Resolve());
            Assert.AreEqual(
                value3
                .Copy(IonTimestamp.Precision.Month)
                .SwitchOffset(localOffset),
                result3.Resolve());
            #endregion

            #region Day precision
            context.Options.Timestamps.TimestampPrecision = SerializerOptions.TimestampPrecision.Day;
            text1 = IonTextSerializer.Serialize(value1, context);
            text2 = IonTextSerializer.Serialize(value2, context);
            text3 = IonTextSerializer.Serialize(value3, context);

            result1 = IonTextSerializer.Parse<IonTimestamp>(text1);
            result2 = IonTextSerializer.Parse<IonTimestamp>(text2);
            result3 = IonTextSerializer.Parse<IonTimestamp>(text3);

            Assert.AreEqual(value1, result1.Resolve());
            Assert.AreEqual(
                value2
                    .Copy(IonTimestamp.Precision.Day)
                    .SwitchOffset(localOffset),
                result2.Resolve());
            Assert.AreEqual(
                value3
                .Copy(IonTimestamp.Precision.Day)
                .SwitchOffset(localOffset),
                result3.Resolve());
            #endregion

            #region Minute precision
            context.Options.Timestamps.TimestampPrecision = SerializerOptions.TimestampPrecision.Minute;
            text1 = IonTextSerializer.Serialize(value1, context);
            text2 = IonTextSerializer.Serialize(value2, context);
            text3 = IonTextSerializer.Serialize(value3, context);

            result1 = IonTextSerializer.Parse<IonTimestamp>(text1);
            result2 = IonTextSerializer.Parse<IonTimestamp>(text2);
            result3 = IonTextSerializer.Parse<IonTimestamp>(text3);

            Assert.AreEqual(value1, result1.Resolve());
            Assert.AreEqual(
                value2
                    .Copy(IonTimestamp.Precision.Minute),
                result2.Resolve());
            Assert.AreEqual(
                value3
                .Copy(IonTimestamp.Precision.Minute),
                result3.Resolve());
            #endregion

            #region Second precision
            context.Options.Timestamps.TimestampPrecision = SerializerOptions.TimestampPrecision.Second;
            text1 = IonTextSerializer.Serialize(value1, context);
            text2 = IonTextSerializer.Serialize(value2, context);
            text3 = IonTextSerializer.Serialize(value3, context);

            result1 = IonTextSerializer.Parse<IonTimestamp>(text1);
            result2 = IonTextSerializer.Parse<IonTimestamp>(text2);
            result3 = IonTextSerializer.Parse<IonTimestamp>(text3);

            Assert.AreEqual(value1, result1.Resolve());
            Assert.AreEqual(
                value2
                    .Copy(IonTimestamp.Precision.Second),
                result2.Resolve());
            Assert.AreEqual(
                value3
                .Copy(IonTimestamp.Precision.Second),
                result3.Resolve());
            #endregion

            #region Millisecond precision
            context.Options.Timestamps.TimestampPrecision = SerializerOptions.TimestampPrecision.MilliSecond;
            text1 = IonTextSerializer.Serialize(value1, context);
            text2 = IonTextSerializer.Serialize(value2, context);
            text3 = IonTextSerializer.Serialize(value3, context);

            result1 = IonTextSerializer.Parse<IonTimestamp>(text1);
            result2 = IonTextSerializer.Parse<IonTimestamp>(text2);
            result3 = IonTextSerializer.Parse<IonTimestamp>(text3);

            Assert.AreEqual(value1, result1.Resolve());
            Assert.AreEqual(
                value2
                    .Copy(IonTimestamp.Precision.Millisecond),
                result2.Resolve());
            Assert.AreEqual(
                value3
                .Copy(IonTimestamp.Precision.Millisecond),
                result3.Resolve());
            #endregion
        }
    }
}
