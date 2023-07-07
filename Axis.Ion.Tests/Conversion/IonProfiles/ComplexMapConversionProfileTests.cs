using Axis.Ion.Conversion;
using Axis.Ion.Conversion.Converters;
using Axis.Ion.Types;

namespace Axis.Ion.Tests.Conversion.IonProfiles
{
    [TestClass]
    public class ComplexMapConversionProfileTests
    {
        public static readonly ComplexMapConverter profile = new ComplexMapConverter();

        [TestMethod]
        public void CanConvertToIon_Tests()
        {
            var map = new ComplexMap<int>();
            var result = profile.CanConvert(typeof(ComplexMap<int>), map);
            Assert.IsTrue(result);

            result = profile.CanConvert(typeof(Dictionary<string, decimal>), map);
            Assert.IsTrue(result);

            result = profile.CanConvert(typeof(ComplexMap<decimal>), new Dictionary<string, decimal>());
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void CanConvertToClr_Tests()
        {
            IonStruct @struct = new IonStruct.Initializer
            {
                [ComplexMapConverter.ItemMapPropertyName] = IonStruct.Empty()
            };

            var map = new ComplexMap<int>();
            var result = profile.CanConvert(typeof(ComplexMap<int>), IonStruct.Null());
            Assert.IsTrue(result);

            result = profile.CanConvert(typeof(Dictionary<string, decimal>), IonStruct.Null());
            Assert.IsFalse(result);

            result = profile.CanConvert(typeof(ComplexMap<decimal>), @struct);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ToIon_Tests()
        {
            var options = new ConversionContext(ConversionOptionsBuilder.NewBuilder().Build());
            var map = new ComplexMap<string>
            {
                Name = "bleh",
                Price = 56.43m,
                ["strong-man"] = "hehehe"
            };

            var ion = profile.ToIon(map.GetType(), map, options);
            var @struct = ion as IonStruct;
            Assert.IsNotNull(ion);
            Assert.IsNotNull(@struct);
            Assert.AreEqual(@struct["Name"].Type, IonTypes.String);
            Assert.AreEqual(@struct["Price"].Type, IonTypes.Decimal);
            Assert.AreEqual(@struct["Description"].Type, IonTypes.String);
            Assert.IsTrue(@struct["Description"].IsNull);
            Assert.IsTrue(@struct.ContainsProperty(ComplexMapConverter.ItemMapPropertyName));
        }

        [TestMethod]
        public void ToClr_Tests()
        {
            var context = new ConversionContext(ConversionOptionsBuilder.NewBuilder().Build());
            IonStruct ionMap = new IonStruct.Initializer
            {
                ["Name"] = "some name",
                ["Price"] = 87.12m,
                [ComplexMapConverter.ItemMapPropertyName] = new IonStruct.Initializer
                {
                    ["bleh"] = "me"
                }
            };

            var obj = profile.ToClr(typeof(ComplexMap<string>), ionMap, context);
            Assert.IsNotNull(obj);

            Assert.IsTrue(obj is ComplexMap<string>);
            var map = obj as ComplexMap<string>;
            Assert.AreEqual("some name", map.Name);
            Assert.AreEqual("me", map["bleh"]);
        }

        public class ComplexMap<TValue>: Dictionary<string, TValue>
        {
            public string? Name { get; set; }
            public string? Description { get; set; }
            public decimal? Price { get; set; }
        }
    }

}
