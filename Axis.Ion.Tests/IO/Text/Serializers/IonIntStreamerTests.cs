using Axis.Ion.IO;
using Axis.Ion.IO.Text;
using Axis.Ion.Types;
using Axis.Luna.Common.Results;

namespace Axis.Ion.Tests.IO.Text.Serializers
{
    [TestClass]
    public class IonIntStreamerTests
    {
        [TestMethod]
        public void Serialize()
        {
            var context = new SerializingContext(new SerializerOptions());

            #region Null
            var ionInt = new IonInt(null);
            var ionIntAnnotated = new IonInt(null, IIonValue.Annotation.ParseCollection("stuff::other::").Resolve());
            var text = IonTextSerializer.Serialize(ionInt, context);
            var textAnnotated = IonTextSerializer.Serialize(ionIntAnnotated, context);

            Assert.AreEqual("null.int", text);
            Assert.AreEqual("stuff::other::null.int", textAnnotated);
            #endregion

            #region Decimal
            ionInt = new IonInt(1234567890);
            var nionInt = new IonInt(-1234567890);
            ionIntAnnotated = new IonInt(1234567890, IIonValue.Annotation.ParseCollection("stuff::true::").Resolve());
            var nionIntAnnotated = new IonInt(-1234567890, IIonValue.Annotation.ParseCollection("stuff::true::").Resolve());

            // no separator
            context.Options.Ints.NumberFormat = SerializerOptions.IntFormat.Decimal;
            text = IonTextSerializer.Serialize(ionInt, context);
            var ntext = IonTextSerializer.Serialize(nionInt, context);
            textAnnotated = IonTextSerializer.Serialize(ionIntAnnotated, context);
            var ntextAnnotated = IonTextSerializer.Serialize(nionIntAnnotated, context);

            Assert.AreEqual("1234567890", text);
            Assert.AreEqual("stuff::true::1234567890", textAnnotated);
            Assert.AreEqual("-1234567890", ntext);
            Assert.AreEqual("stuff::true::-1234567890", ntextAnnotated);

            // separator
            context.Options.Ints.UseDigitSeparator = true;
            text = IonTextSerializer.Serialize(ionInt, context);
            ntext = IonTextSerializer.Serialize(nionInt, context);
            textAnnotated = IonTextSerializer.Serialize(ionIntAnnotated, context);
            ntextAnnotated = IonTextSerializer.Serialize(nionIntAnnotated, context);

            Assert.AreEqual("1_234_567_890", text);
            Assert.AreEqual("stuff::true::1_234_567_890", textAnnotated);
            Assert.AreEqual("-1_234_567_890", ntext);
            Assert.AreEqual("stuff::true::-1_234_567_890", ntextAnnotated);
            #endregion

            #region BigHex
            ionInt = new IonInt(1234567890);
            nionInt = new IonInt(-1234567890);
            ionIntAnnotated = new IonInt(1234567890, IIonValue.Annotation.ParseCollection("stuff::true::").Resolve());
            nionIntAnnotated = new IonInt(-1234567890, IIonValue.Annotation.ParseCollection("stuff::true::").Resolve());

            // no separator
            context.Options.Ints.NumberFormat = SerializerOptions.IntFormat.BigHex;
            context.Options.Ints.UseDigitSeparator = false;
            text = IonTextSerializer.Serialize(ionInt, context);
            ntext = IonTextSerializer.Serialize(nionInt, context);
            textAnnotated = IonTextSerializer.Serialize(ionIntAnnotated, context);
            ntextAnnotated = IonTextSerializer.Serialize(nionIntAnnotated, context);

            Assert.AreEqual("0X499602D2", text);
            Assert.AreEqual("stuff::true::0X499602D2", textAnnotated);
            Assert.AreEqual("0XB669FD2E", ntext);
            Assert.AreEqual("stuff::true::0XB669FD2E", ntextAnnotated);

            // separator
            context.Options.Ints.UseDigitSeparator = true;
            text = IonTextSerializer.Serialize(ionInt, context);
            ntext = IonTextSerializer.Serialize(nionInt, context);
            textAnnotated = IonTextSerializer.Serialize(ionIntAnnotated, context);
            ntextAnnotated = IonTextSerializer.Serialize(nionIntAnnotated, context);

            Assert.AreEqual("0X4996_02D2", text);
            Assert.AreEqual("stuff::true::0X4996_02D2", textAnnotated);
            Assert.AreEqual("0XB669_FD2E", ntext);
            Assert.AreEqual("stuff::true::0XB669_FD2E", ntextAnnotated);
            #endregion

            #region SmallHex
            ionInt = new IonInt(1234567890);
            nionInt = new IonInt(-1234567890);
            ionIntAnnotated = new IonInt(1234567890, IIonValue.Annotation.ParseCollection("stuff::true::").Resolve());
            nionIntAnnotated = new IonInt(-1234567890, IIonValue.Annotation.ParseCollection("stuff::true::").Resolve());

            // no separator
            context.Options.Ints.NumberFormat = SerializerOptions.IntFormat.SmallHex;
            context.Options.Ints.UseDigitSeparator = false;
            text = IonTextSerializer.Serialize(ionInt, context);
            ntext = IonTextSerializer.Serialize(nionInt, context);
            textAnnotated = IonTextSerializer.Serialize(ionIntAnnotated, context);
            ntextAnnotated = IonTextSerializer.Serialize(nionIntAnnotated, context);

            Assert.AreEqual("0x499602d2", text);
            Assert.AreEqual("stuff::true::0x499602d2", textAnnotated);
            Assert.AreEqual("0xb669fd2e", ntext);
            Assert.AreEqual("stuff::true::0xb669fd2e", ntextAnnotated);

            // separator
            context.Options.Ints.UseDigitSeparator = true;
            text = IonTextSerializer.Serialize(ionInt, context);
            ntext = IonTextSerializer.Serialize(nionInt, context);
            textAnnotated = IonTextSerializer.Serialize(ionIntAnnotated, context);
            ntextAnnotated = IonTextSerializer.Serialize(nionIntAnnotated, context);

            Assert.AreEqual("0x4996_02d2", text);
            Assert.AreEqual("stuff::true::0x4996_02d2", textAnnotated);
            Assert.AreEqual("0xb669_fd2e", ntext);
            Assert.AreEqual("stuff::true::0xb669_fd2e", ntextAnnotated);
            #endregion

            #region BigBinary
            ionInt = new IonInt(100);
            nionInt = new IonInt(-100);
            ionIntAnnotated = new IonInt(100, IIonValue.Annotation.ParseCollection("stuff::true::").Resolve());
            nionIntAnnotated = new IonInt(-100, IIonValue.Annotation.ParseCollection("stuff::true::").Resolve());

            // no separator
            context.Options.Ints.NumberFormat = SerializerOptions.IntFormat.BigBinary;
            context.Options.Ints.UseDigitSeparator = false;
            text = IonTextSerializer.Serialize(ionInt, context);
            ntext = IonTextSerializer.Serialize(nionInt, context);
            textAnnotated = IonTextSerializer.Serialize(ionIntAnnotated, context);
            ntextAnnotated = IonTextSerializer.Serialize(nionIntAnnotated, context);

            Assert.AreEqual("0B1100100", text);
            Assert.AreEqual("stuff::true::0B1100100", textAnnotated);
            Assert.AreEqual("0B10011100", ntext);
            Assert.AreEqual("stuff::true::0B10011100", ntextAnnotated);

            // separator
            context.Options.Ints.UseDigitSeparator = true;
            text = IonTextSerializer.Serialize(ionInt, context);
            ntext = IonTextSerializer.Serialize(nionInt, context);
            textAnnotated = IonTextSerializer.Serialize(ionIntAnnotated, context);
            ntextAnnotated = IonTextSerializer.Serialize(nionIntAnnotated, context);

            Assert.AreEqual("0B110_0100", text);
            Assert.AreEqual("stuff::true::0B110_0100", textAnnotated);
            Assert.AreEqual("0B1001_1100", ntext);
            Assert.AreEqual("stuff::true::0B1001_1100", ntextAnnotated);
            #endregion

            #region SmallBinary
            ionInt = new IonInt(100);
            nionInt = new IonInt(-100);
            ionIntAnnotated = new IonInt(100, IIonValue.Annotation.ParseCollection("stuff::true::").Resolve());
            nionIntAnnotated = new IonInt(-100, IIonValue.Annotation.ParseCollection("stuff::true::").Resolve());

            // no separator
            context.Options.Ints.NumberFormat = SerializerOptions.IntFormat.SmallBinary;
            context.Options.Ints.UseDigitSeparator = false;
            text = IonTextSerializer.Serialize(ionInt, context);
            ntext = IonTextSerializer.Serialize(nionInt, context);
            textAnnotated = IonTextSerializer.Serialize(ionIntAnnotated, context);
            ntextAnnotated = IonTextSerializer.Serialize(nionIntAnnotated, context);

            Assert.AreEqual("0b1100100", text);
            Assert.AreEqual("stuff::true::0b1100100", textAnnotated);
            Assert.AreEqual("0b10011100", ntext);
            Assert.AreEqual("stuff::true::0b10011100", ntextAnnotated);

            // separator
            context.Options.Ints.UseDigitSeparator = true;
            text = IonTextSerializer.Serialize(ionInt, context);
            ntext = IonTextSerializer.Serialize(nionInt, context);
            textAnnotated = IonTextSerializer.Serialize(ionIntAnnotated, context);
            ntextAnnotated = IonTextSerializer.Serialize(nionIntAnnotated, context);

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
            var context = new SerializingContext(new SerializerOptions());

            #region decimal
            context.Options.Ints.UseDigitSeparator = false;
            context.Options.Ints.NumberFormat = SerializerOptions.IntFormat.Decimal;
            var ntext = IonTextSerializer.Serialize(nvalue, context);
            var text1 = IonTextSerializer.Serialize(value1, context);
            var text2 = IonTextSerializer.Serialize(value2, context);
            var nresult = IonTextSerializer.Parse<IonInt>(ntext);
            var result1 = IonTextSerializer.Parse<IonInt>(text1);
            var result2 = IonTextSerializer.Parse<IonInt>(text2);

            context.Options.Ints.UseDigitSeparator = true;
            context.Options.Ints.NumberFormat = SerializerOptions.IntFormat.Decimal;
            text1 = IonTextSerializer.Serialize(value1, context);
            text2 = IonTextSerializer.Serialize(value2, context);
            var result3 = IonTextSerializer.Parse<IonInt>(text1);
            var result4 = IonTextSerializer.Parse<IonInt>(text2);

            Assert.AreEqual(nvalue, nresult.Resolve());
            Assert.AreEqual(value1, result1.Resolve());
            Assert.AreEqual(value2, result2.Resolve());
            Assert.AreEqual(value1, result3.Resolve());
            Assert.AreEqual(value2, result4.Resolve());
            #endregion

            #region big hex
            context.Options.Ints.UseDigitSeparator = false;
            context.Options.Ints.NumberFormat = SerializerOptions.IntFormat.BigHex;
            text1 = IonTextSerializer.Serialize(value1, context);
            text2 = IonTextSerializer.Serialize(value2, context);
            result1 = IonTextSerializer.Parse<IonInt>(text1);
            result2 = IonTextSerializer.Parse<IonInt>(text2);

            context.Options.Ints.UseDigitSeparator = true;
            context.Options.Ints.NumberFormat = SerializerOptions.IntFormat.BigHex;
            text1 = IonTextSerializer.Serialize(value1, context);
            text2 = IonTextSerializer.Serialize(value2, context);
            result3 = IonTextSerializer.Parse<IonInt>(text1);
            result4 = IonTextSerializer.Parse<IonInt>(text2);

            Assert.AreEqual(value1, result1.Resolve());
            Assert.AreEqual(value2, result2.Resolve());
            Assert.AreEqual(value1, result3.Resolve());
            Assert.AreEqual(value2, result4.Resolve());
            #endregion

            #region small hex
            context.Options.Ints.UseDigitSeparator = false;
            context.Options.Ints.NumberFormat = SerializerOptions.IntFormat.SmallHex;
            text1 = IonTextSerializer.Serialize(value1, context);
            text2 = IonTextSerializer.Serialize(value2, context);
            result1 = IonTextSerializer.Parse<IonInt>(text1);
            result2 = IonTextSerializer.Parse<IonInt>(text2);

            context.Options.Ints.UseDigitSeparator = true;
            context.Options.Ints.NumberFormat = SerializerOptions.IntFormat.SmallHex;
            text1 = IonTextSerializer.Serialize(value1, context);
            text2 = IonTextSerializer.Serialize(value2, context);
            result3 = IonTextSerializer.Parse<IonInt>(text1);
            result4 = IonTextSerializer.Parse<IonInt>(text2);

            Assert.AreEqual(value1, result1.Resolve());
            Assert.AreEqual(value2, result2.Resolve());
            Assert.AreEqual(value1, result3.Resolve());
            Assert.AreEqual(value2, result4.Resolve());
            #endregion

            #region big binary
            context.Options.Ints.UseDigitSeparator = false;
            context.Options.Ints.NumberFormat = SerializerOptions.IntFormat.BigBinary;
            text1 = IonTextSerializer.Serialize(value1, context);
            text2 = IonTextSerializer.Serialize(value2, context);
            result1 = IonTextSerializer.Parse<IonInt>(text1);
            result2 = IonTextSerializer.Parse<IonInt>(text2);

            context.Options.Ints.UseDigitSeparator = true;
            context.Options.Ints.NumberFormat = SerializerOptions.IntFormat.BigBinary;
            text1 = IonTextSerializer.Serialize(value1, context);
            text2 = IonTextSerializer.Serialize(value2, context);
            result3 = IonTextSerializer.Parse<IonInt>(text1);
            result4 = IonTextSerializer.Parse<IonInt>(text2);

            Assert.AreEqual(value1, result1.Resolve());
            Assert.AreEqual(value2, result2.Resolve());
            Assert.AreEqual(value1, result3.Resolve());
            Assert.AreEqual(value2, result4.Resolve());
            #endregion

            #region small binary
            context.Options.Ints.UseDigitSeparator = false;
            context.Options.Ints.NumberFormat = SerializerOptions.IntFormat.SmallBinary;
            text1 = IonTextSerializer.Serialize(value1, context);
            text2 = IonTextSerializer.Serialize(value2, context);
            result1 = IonTextSerializer.Parse<IonInt>(text1);
            result2 = IonTextSerializer.Parse<IonInt>(text2);

            context.Options.Ints.UseDigitSeparator = true;
            context.Options.Ints.NumberFormat = SerializerOptions.IntFormat.SmallBinary;
            text1 = IonTextSerializer.Serialize(value1, context);
            text2 = IonTextSerializer.Serialize(value2, context);
            result3 = IonTextSerializer.Parse<IonInt>(text1);
            result4 = IonTextSerializer.Parse<IonInt>(text2);

            Assert.AreEqual(value1, result1.Resolve());
            Assert.AreEqual(value2, result2.Resolve());
            Assert.AreEqual(value1, result3.Resolve());
            Assert.AreEqual(value2, result4.Resolve());
            #endregion
        }
    }
}
