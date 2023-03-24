using Axis.Ion.Conversion;
using Axis.Ion.Conversion.Converters;
using Axis.Ion.Types;
using System.Collections.Concurrent;

namespace Axis.Ion.Tests.Conversion.IonProfiles
{
    [TestClass]
    public class MapConversionProfileTests
    {
        private static readonly MapConverter profile = new MapConverter();

        [TestMethod]
        public void CanConvertToIon_Tests()
        {
            var mapType = typeof(Dictionary<string, object>);
            object map = new Dictionary<string, object>();
            var canConvert = profile.CanConvert(mapType, map);
            Assert.IsTrue(canConvert);

            mapType = typeof(Dictionary<Guid, object>);
            canConvert = profile.CanConvert(mapType, map);
            Assert.IsFalse(canConvert);

            mapType = typeof(ConcurrentDictionary<string, Uri>);
            map = new ConcurrentDictionary<string, Uri>();
            canConvert = profile.CanConvert(mapType, map);
            Assert.IsTrue(canConvert);

            mapType = typeof(IDictionary<string, string>);
            map = new Dictionary<string, string>();
            canConvert = profile.CanConvert(mapType, map);
            Assert.IsTrue(canConvert);

            mapType = typeof(ConcurrentDictionary<Guid, object>);
            map = new ConcurrentDictionary<Guid, object>();
            canConvert = profile.CanConvert(mapType, map);
            Assert.IsFalse(canConvert);

            Assert.ThrowsException<ArgumentNullException>(() => profile.CanConvert(null, new ConcurrentDictionary<string, Uri>()));
        }

        [TestMethod]
        public void CanConvertToClr_Tests()
        {
            var mapType = typeof(Dictionary<string, object>);
            var map = IonStruct.Null();
            var canConvert = profile.CanConvert(mapType, map);
            Assert.IsTrue(canConvert);

            mapType = typeof(Dictionary<Guid, object>);
            canConvert = profile.CanConvert(mapType, map);
            Assert.IsFalse(canConvert);

            mapType = typeof(ConcurrentDictionary<string, Uri>);
            canConvert = profile.CanConvert(mapType, map);
            Assert.IsTrue(canConvert);

            mapType = typeof(IDictionary<string, string>);
            canConvert = profile.CanConvert(mapType, map);
            Assert.IsTrue(canConvert);

            mapType = typeof(ConcurrentDictionary<Guid, object>);
            canConvert = profile.CanConvert(mapType, map);
            Assert.IsFalse(canConvert);

            Assert.ThrowsException<ArgumentNullException>(() => profile.CanConvert(null, map));
            Assert.ThrowsException<ArgumentNullException>(() => profile.CanConvert(typeof(IDictionary<string, string>), null));
        }

        [TestMethod]
        public void ToIon_Tests()
        {
            var context = new ConversionContext(ConversionOptionsBuilder.NewBuilder().Build());
            var dictionary = new Dictionary<string, object>
            {
                ["stuff"] = 4,
                ["another stuff"] = "the answer",
                ["decimal-prop"] = 5.4m,
                ["bleh"] = IonTypes.Timestamp,
                ["the time stamp"] = DateTimeOffset.Now
            };

            var ion = profile.ToIon(dictionary.GetType(), dictionary, context);
            Assert.IsNotNull(ion);
            Assert.IsTrue(ion is IonStruct @struct);

            Assert.IsTrue(@struct.Properties.Contains("stuff"));
            Assert.AreEqual(IonTypes.Int, @struct.Properties["stuff"].Type);

            Assert.IsTrue(@struct.Properties.Contains("another stuff"));
            Assert.AreEqual(IonTypes.String, @struct.Properties["another stuff"].Type);

            Assert.IsTrue(@struct.Properties.Contains("decimal-prop"));
            Assert.AreEqual(IonTypes.Decimal, @struct.Properties["decimal-prop"].Type);

            Assert.IsTrue(@struct.Properties.Contains("bleh"));
            Assert.AreEqual(IonTypes.IdentifierSymbol, @struct.Properties["bleh"].Type);

            Assert.IsTrue(@struct.Properties.Contains("the time stamp"));
            Assert.AreEqual(IonTypes.Timestamp, @struct.Properties["the time stamp"].Type);

            Assert.IsTrue(profile.ToIon(dictionary.GetType(), null, context).IsNull);
            Assert.ThrowsException<ArgumentNullException>(() => profile.ToIon(null, dictionary, context));
        }

        [TestMethod]
        public void ToClr_Tests()
        {
            var options = new ConversionContext(ConversionOptionsBuilder.NewBuilder().Build());
            var dt = DateTimeOffset.Now;
            var init = new IonStruct.Initializer
            {
                ["stuff"] = 4,
                ["another stuff"] = "the answer",
                ["decimal-prop"] = 5.4m,
                ["bleh"] = new IonIdentifier(IonTypes.Timestamp.ToString()),
                ["the time stamp"] = dt
            };
            var ionStruct = new IonStruct(init);

            var result = profile.ToClr(typeof(Dictionary<string, object>), ionStruct, options);
            var map = result as Dictionary<string, object>;

            Assert.IsNotNull(result);
            Assert.IsNotNull(map);

            Assert.IsTrue(map.ContainsKey("stuff"));
            Assert.AreEqual(4, map["stuff"]);

            Assert.IsTrue(map.ContainsKey("'another stuff'"));
            Assert.AreEqual("the answer", map["'another stuff'"]);

            Assert.IsTrue(map.ContainsKey("'decimal-prop'"));
            Assert.AreEqual(5.4m, map["'decimal-prop'"]);

            Assert.IsTrue(map.ContainsKey("bleh"));
            Assert.AreEqual(IonTypes.Timestamp.ToString(), map["bleh"]);

            Assert.IsTrue(map.ContainsKey("'the time stamp'"));
            Assert.AreEqual(dt, map["'the time stamp'"]);
        }
    }
}
