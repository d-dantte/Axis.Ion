using Axis.Ion.Conversion;
using Axis.Ion.Conversion.Converters;
using Axis.Ion.Types;

namespace Axis.Ion.Tests.Conversion.IonProfiles
{
    [TestClass]
    public class ComplexCollectionProfileTests
    {
        public static readonly ComplexCollectionConverter profile = new ComplexCollectionConverter();

        [TestMethod]
        public void CanConvertToIon_Tests()
        {
            var list = new ComplexList<int>();
            var result = profile.CanConvert(typeof(ComplexList<int>), list);
            Assert.IsTrue(result);

            result = profile.CanConvert(typeof(List<decimal>), list);
            Assert.IsTrue(result);

            result = profile.CanConvert(typeof(ComplexList<decimal>), new List<decimal>());
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void CanConvertToClr_Tests()
        {
            IonStruct @struct = new IonStruct.Initializer
            {
                [ComplexCollectionConverter.ItemListPropertyName] = IonList.Empty()
            };

            var map = new ComplexList<int>();
            var result = profile.CanConvert(typeof(ComplexList<int>), IonStruct.Null());
            Assert.IsTrue(result);

            result = profile.CanConvert(typeof(Dictionary<string, decimal>), IonStruct.Null());
            Assert.IsFalse(result);

            result = profile.CanConvert(typeof(ComplexList<decimal>), @struct);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ToIon_Tests()
        {
            var options = new ConversionContext(ConversionOptionsBuilder.NewBuilder().Build());
            var list = new ComplexList<string>
            {
                Name = "bleh",
                Price = 56.43m,
            };

            list.Add("hehehe");

            var ion = profile.ToIon(list.GetType(), list, options);
            Assert.IsNotNull(ion);
            Assert.IsTrue(ion is IonStruct @struct);
            Assert.AreEqual(@struct.Properties["Name"].Type, IonTypes.String);
            Assert.AreEqual(@struct.Properties["Price"].Type, IonTypes.Decimal);
            Assert.AreEqual(@struct.Properties["Description"].Type, IonTypes.String);
            Assert.IsTrue(@struct.Properties["Description"].IsNull);
            Assert.IsTrue(@struct.Properties.Contains(ComplexCollectionConverter.ItemListPropertyName));
        }

        [TestMethod]
        public void ToClr_Tests()
        {
            var context = new ConversionContext(ConversionOptionsBuilder.NewBuilder().Build());
            IonStruct ionMap = new IonStruct.Initializer
            {
                ["Name"] = "some name",
                ["Price"] = 87.12m,
                [ComplexCollectionConverter.ItemListPropertyName] = new IonList.Initializer
                {
                    "me"
                }
            };

            var obj = profile.ToClr(typeof(ComplexList<string>), ionMap, context);
            Assert.IsNotNull(obj);

            Assert.IsTrue(obj is ComplexList<string>);
            var list = obj as ComplexList<string>;
            Assert.AreEqual("some name", list.Name);
            Assert.AreEqual("me", list[0]);
        }

        public class ComplexList<TValue> : List<TValue>
        {
            public string? Name { get; set; }
            public string? Description { get; set; }
            public decimal? Price { get; set; }
        }
    }
}
