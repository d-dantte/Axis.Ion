using Axis.Ion.Conversion;
using Axis.Ion.Conversion.Converters;
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
            var profile = new ObjectConverter(typeof(Poco1));
            Assert.IsNotNull(profile);
        }

        [TestMethod]
        public void CanConvertToIon_Tests()
        {
            var poco = new Poco1(false);
            var profile = new ObjectConverter(typeof(Poco1));
            var result = profile.CanConvert(typeof(Poco1), poco);
            Assert.IsTrue(result);

            result = profile.CanConvert(typeof(object), poco);
            Assert.IsTrue(result);

            result = profile.CanConvert(typeof(Poco1), new object());
            Assert.IsFalse(result);

            Assert.ThrowsException<ArgumentNullException>(() => profile.CanConvert(null, poco));
        }

        [TestMethod]
        public void CanConvertToClr_Tests()
        {
            var ion = IonStruct.Null();
            var profile = new ObjectConverter(typeof(Poco1));
            var result = profile.CanConvert(typeof(Poco1), ion);
            Assert.IsTrue(result);

            result = profile.CanConvert(typeof(object), ion);
            Assert.IsFalse(result);

            result = profile.CanConvert(typeof(Poco1), IonBool.Null());
            Assert.IsFalse(result);

            Assert.ThrowsException<ArgumentNullException>(() => profile.CanConvert(null, ion));
            Assert.ThrowsException<ArgumentNullException>(() => profile.CanConvert(typeof(Poco1), null));
        }

        [TestMethod]
        public void ToIon_Tests()
        {
            var context = new ConversionContext(ConversionOptionsBuilder.NewBuilder().Build());
            var profile = new ObjectConverter(typeof(Poco1));
            var instance = new Poco1(5, "stuff", true, DateTimeOffset.Now);

            var ion = profile.ToIon(typeof(Poco1), instance, context);
            Assert.IsNotNull(ion);
            Assert.AreEqual(IonTypes.Struct, ion.Type);

            ion = profile.ToIon(typeof(Poco1), null, context);
            Assert.IsTrue(ion.IsNull);

            // with ignored properties
            context = new ConversionContext(ConversionOptionsBuilder
                .NewBuilder()
                .WithIgnoredProperties(typeof(Poco1), nameof(Poco1.Prop1))
                .Build());

            ion = profile.ToIon(typeof(Poco1), instance, context);
            var @struct = (IonStruct)ion;
            Assert.IsFalse(@struct.ContainsProperty(nameof(Poco1.Prop1)));

            // with ignored nulls
            instance = new Poco1(5, null, true, DateTimeOffset.Now);
            context = new ConversionContext(ConversionOptionsBuilder
                .NewBuilder()
                .WithNullValueBehavior(NullValueBehavior.Ignore)
                .Build());
            ion = profile.ToIon(typeof(Poco1), instance, context);
            @struct = (IonStruct)ion;
            Assert.IsFalse(@struct.ContainsProperty(nameof(Poco1.Prop2)));

            // with ignored defaults
            instance = new Poco1(5, "stuff", false, DateTimeOffset.Now);
            context = new ConversionContext(ConversionOptionsBuilder
                .NewBuilder()
                .WithDefaultValueBehavior(DefaultValueBehavior.Ignore)
                .Build());
            ion = profile.ToIon(typeof(Poco1), instance, context);
            @struct = (IonStruct)ion;
            Assert.IsFalse(@struct.ContainsProperty(nameof(Poco1.Prop3)));
        }

        [TestMethod]
        public void ToClr_Tests()
        {
            var options = new ConversionContext(ConversionOptionsBuilder.NewBuilder().Build());
            var initializer = new IonStruct.Initializer
            {
                ["Prop1"] = 4,
                ["Prop2"] = "stuff",
                ["Prop3"] = false,
                ["Prop4"] = DateTimeOffset.Now
            };
            var ionStruct = new IonStruct(initializer);

            // poco 1
            var profile = new ObjectConverter(typeof(Poco1));
            var instance = profile.ToClr(typeof(Poco1), ionStruct, options);
            Assert.IsNotNull(instance);

            // poco 2
            profile = new ObjectConverter(typeof(Poco2));
            instance = profile.ToClr(typeof(Poco2), ionStruct, options);
            Assert.IsNotNull(instance);

            // poco 3
            profile = new ObjectConverter(typeof(Poco3));
            instance = profile.ToClr(typeof(Poco3), ionStruct, options);
            Assert.IsNotNull(instance);

            // poco 4
            profile = new ObjectConverter(typeof(Poco4));
            Assert.ThrowsException<InvalidOperationException>(() => profile.ToClr(typeof(Poco4), ionStruct, options));
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
