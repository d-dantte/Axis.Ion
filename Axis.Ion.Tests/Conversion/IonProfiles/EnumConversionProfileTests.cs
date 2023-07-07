﻿using Axis.Ion.Conversion;
using Axis.Ion.Conversion.Converters;
using Axis.Ion.Types;

namespace Axis.Ion.Tests.Conversion.IonProfiles
{
    [TestClass]
    public class EnumConversionProfileTests
    {
        private static readonly EnumConverter profile = new EnumConverter();

        [TestMethod]
        public void CanConvertToIon_Tests()
        {
            var canConvert = profile.CanConvert(typeof(IonTypes), IonTypes.Sexp);
            Assert.IsTrue(canConvert);

            Assert.ThrowsException<ArgumentNullException>(() => profile.CanConvert(null, IonTypes.Sexp));
        }

        [TestMethod]
        public void CanConvertToClr_Tests()
        {
            var canConvert = profile.CanConvert(typeof(IonTypes), IonTextSymbol.Null());
            Assert.IsTrue(canConvert);

            Assert.ThrowsException<ArgumentNullException>(() => profile.CanConvert(null, IonTextSymbol.Null()));
            Assert.ThrowsException<ArgumentNullException>(() => profile.CanConvert(typeof(IonTypes), null));
        }

        [TestMethod]
        public void ToClr_Tests()
        {
            var options = new ConversionContext(ConversionOptionsBuilder.NewBuilder().Build());
            var enumType = typeof(IonTypes);
            var @string = new IonString("Sexp");
            var identifier = new IonTextSymbol("Sexp");

            var value = profile.ToClr(enumType, @string, options);
            Assert.AreEqual(IonTypes.Sexp, value);

            value = profile.ToClr(enumType, identifier, options);
            Assert.AreEqual(IonTypes.Sexp, value);

            Assert.ThrowsException<ArgumentNullException>(() => profile.ToClr(null, @string, options));

            var invalidEnum = new IonString("Bleh");
            Assert.ThrowsException<ArgumentException>(() => profile.ToClr(enumType, invalidEnum, options));

            var nullEnum = IIonValue.NullOf(IonTypes.TextSymbol);
            Assert.ThrowsException<ArgumentException>(() => profile.ToClr(enumType, nullEnum, options));
        }

        [TestMethod]
        public void ToIon_Tests()
        {
            var options = new ConversionContext(ConversionOptionsBuilder.NewBuilder().Build());
            var enumType = typeof(IonTypes);
            var result = profile.ToIon(enumType, IonTypes.Blob, options);
            var ion = (IonTextSymbol)result;

            Assert.IsNotNull(result);
            Assert.AreEqual(IonTypes.Blob.ToString(), ion.Value);

            result = profile.ToIon(enumType, null, options);
            ion = (IonTextSymbol)result;

            Assert.IsNotNull(result);
            Assert.IsTrue(ion.IsNull);
        }
    }
}
