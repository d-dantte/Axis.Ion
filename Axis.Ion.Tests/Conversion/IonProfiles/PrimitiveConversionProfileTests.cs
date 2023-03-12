using Axis.Ion.Conversion;
using Axis.Ion.Conversion.IonProfiles;
using Axis.Ion.Types;
using System.Numerics;

namespace Axis.Ion.Tests.Conversion.IonProfiles
{
    [TestClass]
    public class PrimitiveConversionProfileTests
    {
        private static readonly PrimitiveConversionProfile profile = new PrimitiveConversionProfile();

        [TestMethod]
        public void CanConvert_Tests()
        {
            var types = new[]
            {
                typeof(short),
                typeof(ushort),
                typeof(int),
                typeof(uint),
                typeof(long),
                typeof(ulong),
                typeof(float),
                typeof(double),
                typeof(decimal),
                typeof(bool),
                typeof(DateTime),
                typeof(DateTimeOffset),
                typeof(BigInteger),
                typeof(string)
            };

            var nullableBase = typeof(Nullable<>);

            foreach (var type in types)
            {
                var result = profile.CanConvert(type);
                Assert.IsTrue(result);

                if (type != typeof(string))
                {
                    var nresult = profile.CanConvert(nullableBase.MakeGenericType(type));
                    Assert.IsTrue(nresult);
                }
            }

            Assert.ThrowsException<ArgumentNullException>(() => profile.CanConvert(null));
        }

        [TestMethod]
        public void FromIon_Tests()
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

            var options = new ConversionOptions();

            Assert.ThrowsException<ArgumentNullException>(() => profile.FromIon(null, intIon, options));
            Assert.ThrowsException<ArgumentNullException>(() => profile.FromIon(typeof(int), null, options));

            foreach (var info in types)
            {
                object? result;

                var ntype = nullableBase.MakeGenericType(info.type);

                // primitive/ion
                result = profile.FromIon(info.type, info.ion, options);
                Assert.IsNotNull(result);
                Assert.AreEqual(info.type, result.GetType());

                // primitive/null-ion-primitive
                result = profile.FromIon(info.type, info.nullIon, options);
                Assert.IsNull(result);

                // nullable-primitive/ion-primitive
                result = profile.FromIon(ntype, info.ion, options);
                Assert.IsNotNull(result);
                Assert.AreEqual(info.type, result.GetType());

                // nullable-primitive/null-ion-primitive
                result = profile.FromIon(ntype, info.nullIon, options);
                Assert.IsNull(result);

                // primitive/ion-non-primitive
                Assert.ThrowsException<ArgumentException>(() => profile.FromIon(info.type, nullIon, options));
                Assert.IsNull(result);

                // nullable-primitive/ion-non-primitive
                Assert.ThrowsException<ArgumentException>(() => profile.FromIon(ntype, nullIon, options));
            }

            // sstring
            var result2 = profile.FromIon(typeof(string), stringIon, options);
            Assert.AreEqual(stringIon.IonValue(), result2);

            result2 = profile.FromIon(typeof(string), nstringIon, options);
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

            var options = new ConversionOptions();

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
