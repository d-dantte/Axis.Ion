using Axis.Ion.Conversion;
using Axis.Ion.Conversion.IonProfiles;
using Axis.Ion.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axis.Ion.Tests.Conversion.IonProfiles
{
    [TestClass]
    public class ObjectConversionProfileTests
    {
        [TestMethod]
        public void Creation_Tests()
        {
            var profile = new ObjectConversionProfile(typeof(Poco1));
            Assert.IsNotNull(profile);
        }

        [TestMethod]
        public void CanConvert_Tests()
        {
            var profile = new ObjectConversionProfile(typeof(Poco1));
            var result = profile.CanConvert(typeof(Poco1));
            Assert.IsTrue(result);

            result = profile.CanConvert(typeof(object));
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ToIon_Tests()
        {
            var options = new ConversionOptions();
            var profile = new ObjectConversionProfile(typeof(Poco1));
            var instance = new Poco1(5, "stuff", true, DateTimeOffset.Now);

            var ion = profile.ToIon(typeof(Poco1), instance, options);
            Assert.IsNotNull(ion);
            Assert.AreEqual(IonTypes.Struct, ion.Type);
        }


        [TestMethod]
        public void FromIon_Tests()
        {
            var options = new ConversionOptions();
            var initializer = new IonStruct.Initializer
            {
                ["Prop1"] = 4,
                ["Prop2"] = "stuff",
                ["Prop3"] = false,
                ["Prop4"] = DateTimeOffset.Now
            };
            var ionStruct = new IonStruct(initializer);

            // poco 1
            var profile = new ObjectConversionProfile(typeof(Poco1));
            var instance = profile.FromIon(typeof(Poco1), ionStruct, options);
            Assert.IsNotNull(instance);

            // poco 2
            profile = new ObjectConversionProfile(typeof(Poco2));
            instance = profile.FromIon(typeof(Poco2), ionStruct, options);
            Assert.IsNotNull(instance);

            // poco 3
            profile = new ObjectConversionProfile(typeof(Poco3));
            instance = profile.FromIon(typeof(Poco3), ionStruct, options);
            Assert.IsNotNull(instance);

            // poco 4
            profile = new ObjectConversionProfile(typeof(Poco4));
            Assert.ThrowsException<InvalidOperationException>(() => profile.FromIon(typeof(Poco4), ionStruct, options));
        }

        public class Poco1
        {
            public int Prop1 { get; set; }
            public string? Prop2 { get; set; }
            public bool Prop3 { get; }
            public DateTimeOffset Prop4 { get; set; }

            public Poco1(int prop1, string? prop2, bool prop3, DateTimeOffset prop4)
            {
                Prop1 = prop1;
                Prop2 = prop2;
                Prop3 = prop3;
                Prop4 = prop4;
            }

            public Poco1(bool prop3)
            {
                Prop3 = prop3;
            }

            public Poco1(bool notProfilable, int notProfilableInt)
            {
                Prop1 = notProfilableInt;
                Prop3 = notProfilable;
            }
        }

        public class Poco2
        {
            public int Prop1 { get; set; }
            public string? Prop2 { get; set; }
            public bool Prop3 { get; }
            public DateTimeOffset Prop4 { get; set; }


            public Poco2(bool prop3)
            {
                Prop3 = prop3;
            }

            public Poco2(bool notProfilable, int notProfilableInt)
            {
                Prop1 = notProfilableInt;
                Prop3 = notProfilable;
            }
        }

        public class Poco3
        {
            public int Prop1 { get; set; }
            public string? Prop2 { get; set; }
            public bool Prop3 { get; }
            public DateTimeOffset Prop4 { get; set; }


            public Poco3()
            {
            }
        }
        public class Poco4
        {
            public int Prop1_ { get; set; }
            public string? Prop2_ { get; set; }
            public bool Prop3_ { get; }
            public DateTimeOffset Prop4_ { get; set; }


            public Poco4(Uri bleh)
            {
            }
        }
    }
}
