using Axis.Ion.Conversion;
using Axis.Ion.Conversion.IonProfiles;
using Axis.Ion.Types;

namespace Axis.Ion.Tests.Conversion.IonProfiles
{
    [TestClass]
    public class EnumConversionProfileTests
    {
        private static readonly EnumConversionProfile profile = new EnumConversionProfile();

        [TestMethod]
        public void CanConvert_Tests()
        {
            var canConvert = profile.CanConvert(typeof(IonTypes));
            Assert.IsTrue(canConvert);
        }

        [TestMethod]
        public void FromIon_Tests()
        {
            var options = new ConversionOptions();
            var enumType = typeof(IonTypes);
            var @string = new IonString("Sexp");
            var identifier = new IonIdentifier("Sexp");
            var quoted = new IonQuotedSymbol("Sexp");

            var value = profile.FromIon(enumType, @string, options);
            Assert.AreEqual(IonTypes.Sexp, value);

            value = profile.FromIon(enumType, identifier, options);
            Assert.AreEqual(IonTypes.Sexp, value);

            value = profile.FromIon(enumType, quoted, options);
            Assert.AreEqual(IonTypes.Sexp, value);

            Assert.ThrowsException<ArgumentNullException>(() => profile.FromIon(null, @string, options));

            var invalidEnum = new IonString("Bleh");
            Assert.ThrowsException<ArgumentException>(() => profile.FromIon(enumType, invalidEnum, options));

            var nullEnum = IIonType.NullOf(IonTypes.IdentifierSymbol);
            Assert.ThrowsException<ArgumentException>(() => profile.FromIon(enumType, nullEnum, options));
        }

        [TestMethod]
        public void ToIon_Tests()
        {
            var options = new ConversionOptions();
            var enumType = typeof(IonTypes);
            var result = profile.ToIon(enumType, IonTypes.Blob, options);
            var ion = (IonIdentifier)result;

            Assert.IsNotNull(result);
            Assert.AreEqual(IonTypes.Blob.ToString(), ion.Value);

            result = profile.ToIon(enumType, null, options);
            ion = (IonIdentifier)result;

            Assert.IsNotNull(result);
            Assert.IsTrue(ion.IsNull);
        }
    }
}
