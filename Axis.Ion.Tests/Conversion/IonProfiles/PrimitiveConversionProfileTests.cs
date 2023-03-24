using Axis.Ion.Conversion;
using Axis.Ion.Conversion.Converters;
using Axis.Ion.Types;
using System.Numerics;

namespace Axis.Ion.Tests.Conversion.IonProfiles
{
    [TestClass]
    public class PrimitiveConversionProfileTests
    {
        private static readonly PrimitiveConverter profile = new PrimitiveConverter();

        [TestMethod]
        public void CanConvertToIon_Tests()
        {
            var types = new[]
            {
                (typeof(short), (object)(short)4),
                (typeof(ushort), (ushort)4),
                (typeof(int), 4),
                (typeof(uint), (uint)4),
                (typeof(long), 4l),
                (typeof(ulong), 4ul),
                (typeof(float), 4f),
                (typeof(double), 4.0),
                (typeof(decimal), 4m),
                (typeof(bool), false),
                (typeof(DateTime), DateTime.Now),
                (typeof(DateTimeOffset), DateTimeOffset.Now),
                (typeof(BigInteger), new BigInteger(4)),
                (typeof(string), "stuff")
            };

            var nullableBase = typeof(Nullable<>);

            foreach (var type in types)
            {
                var result = profile.CanConvert(type.Item1, type.Item2);
                Assert.IsTrue(result);

                if (type.Item1 != typeof(string))
                {
                    var nresult = profile.CanConvert(nullableBase.MakeGenericType(type.Item1), type.Item2);
                    Assert.IsTrue(nresult);
                }
            }

            Assert.ThrowsException<ArgumentNullException>(() => profile.CanConvert(null, "stuff"));
            Assert.IsFalse(profile.CanConvert(typeof(string), 5));
        }

        [TestMethod]
        public void CanConvertToClr_Tests()
        {
            var ionInt = new IonInt(4);
            var ionReal = new IonFloat(4);
            var ionDecimal = new IonDecimal(4);
            var ionBool = new IonBool(false);
            var ionTimestamp = new IonTimestamp(DateTimeOffset.Now);
            var ionString = new IonString("stuff");
            var types = new[]
            {
                (typeof(short), (IIonType)ionInt),
                (typeof(ushort), ionInt),
                (typeof(int), ionInt),
                (typeof(uint), ionInt),
                (typeof(long), ionInt),
                (typeof(ulong), ionInt),
                (typeof(float), ionReal),
                (typeof(double), ionReal),
                (typeof(decimal), ionDecimal),
                (typeof(bool), ionBool),
                (typeof(DateTime), ionTimestamp),
                (typeof(DateTimeOffset), ionTimestamp),
                (typeof(BigInteger), ionInt),
                (typeof(string), ionString)
            };

            var nullableBase = typeof(Nullable<>);

            foreach (var type in types)
            {
                var result = profile.CanConvert(type.Item1, type.Item2);
                Assert.IsTrue(result);

                if (type.Item1 != typeof(string))
                {
                    var nresult = profile.CanConvert(nullableBase.MakeGenericType(type.Item1), type.Item2);
                    Assert.IsTrue(nresult);
                }
            }

            Assert.ThrowsException<ArgumentNullException>(() => profile.CanConvert(null, IonString.Null()));
            Assert.ThrowsException<ArgumentNullException>(() => profile.CanConvert(typeof(string), (IIonType)null));
            Assert.IsFalse(profile.CanConvert(typeof(string), 5));
        }

        [TestMethod]
        public void ToClr_Tests()
        {
            var nullableBase = typeof(Nullable<>);

            var nullIon = IIonType.NullOf(IonTypes.Null);

            #region Types
            // bool
            IIonType boolIon = new IonBool(true);
            IIonType nboolIon = IIonType.NullOf(IonTypes.Bool);

            // int
            IIonType intIon = new IonInt(4);
            IIonType nintIon = IIonType.NullOf(IonTypes.Int);

            // real
            IIonType realIon = new IonFloat(4);
            IIonType nrealIon = IIonType.NullOf(IonTypes.Float);

            // decimal
            IIonType decimalIon = new IonDecimal(4);
            IIonType ndecimalIon = IIonType.NullOf(IonTypes.Decimal);

            // timestamp
            IIonType timestampIon = new IonTimestamp(DateTime.Now);
            IIonType ntimestampIon = IIonType.NullOf(IonTypes.Timestamp);

            // string
            IIonType stringIon = new IonString("stuff");
            IIonType nstringIon = IIonType.NullOf(IonTypes.String);
            #endregion

            (Type type, IIonType ion, IIonType nullIon)[] types = new[]
            {
                (typeof(bool), boolIon, nboolIon),
                (typeof(short), intIon, nintIon),
                (typeof(ushort), intIon, nintIon),
                (typeof(int), intIon, nintIon),
                (typeof(uint), intIon, nintIon),
                (typeof(long), intIon, nintIon),
                (typeof(ulong), intIon, nintIon),
                (typeof(BigInteger), intIon, nintIon),
                (typeof(float), realIon, nrealIon),
                (typeof(double), realIon, nrealIon),
                (typeof(decimal), decimalIon, ndecimalIon),
                (typeof(DateTime), timestampIon, ntimestampIon),
                (typeof(DateTimeOffset), timestampIon, ntimestampIon)
            };

            var context = new ConversionContext(ConversionOptionsBuilder.NewBuilder().Build());

            Assert.ThrowsException<ArgumentNullException>(() => profile.ToClr(null, intIon, context));
            Assert.ThrowsException<ArgumentNullException>(() => profile.ToClr(typeof(int), null, context));

            foreach (var info in types)
            {
                object? result;

                var ntype = nullableBase.MakeGenericType(info.type);

                // primitive/ion
                result = profile.ToClr(info.type, info.ion, context);
                Assert.IsNotNull(result);
                Assert.AreEqual(info.type, result.GetType());

                // primitive/null-ion-primitive
                result = profile.ToClr(info.type, info.nullIon, context);
                Assert.IsNull(result);

                // nullable-primitive/ion-primitive
                result = profile.ToClr(ntype, info.ion, context);
                Assert.IsNotNull(result);
                Assert.AreEqual(info.type, result.GetType());

                // nullable-primitive/null-ion-primitive
                result = profile.ToClr(ntype, info.nullIon, context);
                Assert.IsNull(result);

                // primitive/ion-non-primitive
                Assert.ThrowsException<ArgumentException>(() => profile.ToClr(info.type, nullIon, context));
                Assert.IsNull(result);

                // nullable-primitive/ion-non-primitive
                Assert.ThrowsException<ArgumentException>(() => profile.ToClr(ntype, nullIon, context));
            }

            // sstring
            var result2 = profile.ToClr(typeof(string), stringIon, context);
            Assert.AreEqual(stringIon.IonValue(), result2);

            result2 = profile.ToClr(typeof(string), nstringIon, context);
            Assert.IsNull(result2);
        }

        [TestMethod]
        public void ToIon_Tests()
        {
            (Type type, Type ntype, IonTypes ionType, object value)[] args = new[]
            {
                (typeof(short), typeof(short?), IonTypes.Int, (object)5),
                (typeof(ushort), typeof(ushort?), IonTypes.Int, (object)5),
                (typeof(int), typeof(int?), IonTypes.Int, (object)5),
                (typeof(uint), typeof(uint?), IonTypes.Int, (object)5u),
                (typeof(long), typeof(long?), IonTypes.Int, (object)5L),
                (typeof(ulong), typeof(ulong?), IonTypes.Int, (object)5ul),
                (typeof(BigInteger), typeof(BigInteger?), IonTypes.Int, (object)new BigInteger(5)),
                (typeof(float), typeof(float?), IonTypes.Float, (object)5.0),
                (typeof(double), typeof(double?), IonTypes.Float, (object)5.0),
                (typeof(decimal), typeof(decimal?), IonTypes.Decimal, (object)5.0m),
                (typeof(bool), typeof(bool?), IonTypes.Bool, (object)false),
                (typeof(DateTime), typeof(DateTime?), IonTypes.Timestamp, (object)DateTime.Now),
                (typeof(DateTimeOffset), typeof(DateTimeOffset?), IonTypes.Timestamp, (object)DateTimeOffset.Now)
            };

            var options = new ConversionContext(ConversionOptionsBuilder.NewBuilder().Build());

            Assert.ThrowsException<ArgumentNullException>(() => profile.ToIon(null, null, options));

            foreach (var info in args)
            {
                // clr-type/value
                var result = profile.ToIon(info.type, info.value, options);
                Assert.IsNotNull(result);
                Assert.AreEqual(result.Type, info.ionType);
                if (info.type == typeof(DateTime))
                {
                    var ts = ((IonTimestamp)result).Value ?? throw new ArgumentNullException();
                    Assert.AreEqual(info.value, ts.DateTime);
                }
                else
                {
                    Assert.AreEqual(result.IonValue()?.ToString(), info.value.ToString());
                }

                // nullable-clr-type/value
                result = profile.ToIon(info.ntype, info.value, options);
                Assert.IsNotNull(result);
                Assert.AreEqual(result.Type, info.ionType);
                if (info.type == typeof(DateTime))
                {
                    var ts = ((IonTimestamp)result).Value ?? throw new ArgumentNullException();
                    Assert.AreEqual(info.value, ts.DateTime);
                }
                else
                {
                    Assert.AreEqual(result.IonValue()?.ToString(), info.value.ToString());
                }

                // clr-type/null
                result = profile.ToIon(info.type, null, options);
                Assert.IsNotNull(result);
                Assert.AreEqual(result.Type, info.ionType);
                Assert.IsNull(result.IonValue());

                // nullable-clr-type/null
                result = profile.ToIon(info.ntype, null, options);
                Assert.IsNotNull(result);
                Assert.AreEqual(result.Type, info.ionType);
                Assert.IsNull(result.IonValue());

                // clr-type/invalid-value
                Assert.ThrowsException<ArgumentException>(() => profile.ToIon(info.type, new object(), options));

                // nullable-clr-type/invalid-value
                Assert.ThrowsException<ArgumentException>(() => profile.ToIon(info.ntype, new object(), options));
            }
        }
    }
}
