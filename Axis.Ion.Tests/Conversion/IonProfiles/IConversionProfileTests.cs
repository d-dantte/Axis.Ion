using Axis.Ion.Conversion;
using Axis.Ion.Conversion.IonProfiles;
using Axis.Ion.Types;
using System.Numerics;

namespace Axis.Ion.Tests.Conversion.IonProfiles
{
    [TestClass]
    public class IConversionProfileTests
    {
        [TestMethod]
        public void CategoryOf_WithNullType_ThrowsException()
        {
            Assert.ThrowsException<ArgumentNullException>(() => IConversionProfile.CategoryOf(null));
        }

        [TestMethod]
        public void CategoryOf_WithPrimitiveType_ReturnsPrimitive()
        {
            var category = IConversionProfile.CategoryOf(typeof(int));
            Assert.AreEqual(TypeCategory.Primitive, category);

            category = IConversionProfile.CategoryOf(typeof(int?));
            Assert.AreEqual(TypeCategory.Primitive, category);
        }

        [TestMethod]
        public void CategoryOf_WithEnumType_ReturnsEnum()
        {
            var category = IConversionProfile.CategoryOf(typeof(TypeCode));
            Assert.AreEqual(TypeCategory.Enum, category);

            category = IConversionProfile.CategoryOf(typeof(TypeCode?));
            Assert.AreEqual(TypeCategory.Enum, category);
        }

        [TestMethod]
        public void CategoryOf_WithComplexMap_ReturnsComplexMap()
        {
            var category = IConversionProfile.CategoryOf(typeof(ComplexMap1));
            Assert.AreEqual(TypeCategory.ComplexMap, category);
        }

        [TestMethod]
        public void CategoryOf_WithMap_ReturnsMap()
        {
            var category = IConversionProfile.CategoryOf(typeof(Dictionary<string, string>));
            Assert.AreEqual(TypeCategory.Map, category);

            category = IConversionProfile.CategoryOf(typeof(Dictionary<int, string>));
            Assert.AreNotEqual(TypeCategory.Map, category);
        }

        [TestMethod]
        public void CategoryOf_WithComplexCollection_ReturnsComplexCollection()
        {
            var category = IConversionProfile.CategoryOf(typeof(ComplexCollection1));
            Assert.AreEqual(TypeCategory.ComplexCollection, category);
        }

        [TestMethod]
        public void CategoryOf_WithCollection_ReturnsCollection()
        {
            var category = IConversionProfile.CategoryOf(typeof(List<object>));
            Assert.AreEqual(TypeCategory.Collection, category);
        }

        [TestMethod]
        public void CategoryOf_WithArray_ReturnsArray()
        {
            var category = IConversionProfile.CategoryOf(typeof(int[]));
            Assert.AreEqual(TypeCategory.SingleDimensionArray, category);

            category = IConversionProfile.CategoryOf(typeof(int[,]));
            Assert.AreNotEqual(TypeCategory.SingleDimensionArray, category);
        }

        [TestMethod]
        public void AreCompatible()
        {
            #region null clr
            Assert.ThrowsException<ArgumentNullException>(() => IConversionProfile.AreCompatible(
                Ion.Types.IonTypes.Null,
                null));
            #endregion

            #region Bool/Bool
            var clrType = typeof(bool);
            var ionType = Ion.Types.IonTypes.Bool;
            var areCompatible = IConversionProfile.AreCompatible(ionType, clrType);

            Assert.IsTrue(areCompatible);

            clrType = typeof(bool?);
            areCompatible = IConversionProfile.AreCompatible(ionType, clrType);

            Assert.IsTrue(areCompatible);

            clrType = typeof(int);
            areCompatible = IConversionProfile.AreCompatible(ionType, clrType);

            Assert.IsFalse(areCompatible);
            #endregion

            #region Int/Int
            var nt = typeof(Nullable<>);
            foreach (var t in new[] { typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(short), typeof(ushort), typeof(BigInteger) })
            {
                clrType = t;
                ionType = Ion.Types.IonTypes.Int;
                areCompatible = IConversionProfile.AreCompatible(ionType, clrType);

                Assert.IsTrue(areCompatible);

                clrType = nt.MakeGenericType(clrType);
                areCompatible = IConversionProfile.AreCompatible(ionType, clrType);

                Assert.IsTrue(areCompatible);

                clrType = typeof(Type);
                areCompatible = IConversionProfile.AreCompatible(ionType, clrType);

                Assert.IsFalse(areCompatible);
            }
            #endregion

            #region Float/Float
            foreach (var t in new[] { typeof(float), typeof(double) })
            {
                clrType = t;
                ionType = Ion.Types.IonTypes.Float;
                areCompatible = IConversionProfile.AreCompatible(ionType, clrType);

                Assert.IsTrue(areCompatible);

                clrType = nt.MakeGenericType(clrType);
                areCompatible = IConversionProfile.AreCompatible(ionType, clrType);

                Assert.IsTrue(areCompatible);

                clrType = typeof(Type);
                areCompatible = IConversionProfile.AreCompatible(ionType, clrType);

                Assert.IsFalse(areCompatible);
            }
            #endregion

            #region Decimal/Decimal
            clrType = typeof(decimal);
            ionType = Ion.Types.IonTypes.Decimal;
            areCompatible = IConversionProfile.AreCompatible(ionType, clrType);

            Assert.IsTrue(areCompatible);

            clrType = typeof(decimal?);
            areCompatible = IConversionProfile.AreCompatible(ionType, clrType);

            Assert.IsTrue(areCompatible);

            clrType = typeof(Type);
            areCompatible = IConversionProfile.AreCompatible(ionType, clrType);

            Assert.IsFalse(areCompatible);
            #endregion

            #region Datetime
            foreach (var t in new[] { typeof(DateTime), typeof(DateTimeOffset) })
            {
                clrType = t;
                ionType = Ion.Types.IonTypes.Timestamp;
                areCompatible = IConversionProfile.AreCompatible(ionType, clrType);

                Assert.IsTrue(areCompatible);

                clrType = nt.MakeGenericType(clrType);
                areCompatible = IConversionProfile.AreCompatible(ionType, clrType);

                Assert.IsTrue(areCompatible);

                clrType = typeof(Type);
                areCompatible = IConversionProfile.AreCompatible(ionType, clrType);

                Assert.IsFalse(areCompatible);
            }
            #endregion

            #region string
            clrType = typeof(string);
            ionType = Ion.Types.IonTypes.String;
            areCompatible = IConversionProfile.AreCompatible(ionType, clrType);

            Assert.IsTrue(areCompatible);

            clrType = typeof(Type);
            areCompatible = IConversionProfile.AreCompatible(ionType, clrType);

            Assert.IsFalse(areCompatible);
            #endregion

            #region Symbol/string
            foreach (var it in new[] { IonTypes.OperatorSymbol, IonTypes.QuotedSymbol, IonTypes.IdentifierSymbol })
            {
                ionType = it;
                clrType = typeof(string);
                areCompatible = IConversionProfile.AreCompatible(ionType, clrType);

                Assert.IsTrue(areCompatible);

                clrType = typeof(Type);
                areCompatible = IConversionProfile.AreCompatible(ionType, clrType);

                Assert.IsFalse(areCompatible);
            }
            #endregion

            #region blob
            var types = new[]
            {
                typeof(List<byte>),
                typeof(byte[]),
                typeof(HashSet<byte>)
            };
            foreach (var t in types)
            {
                clrType = t;
                ionType = Ion.Types.IonTypes.Blob;
                areCompatible = IConversionProfile.AreCompatible(ionType, clrType);

                Assert.IsTrue(areCompatible);

                clrType = typeof(Type);
                areCompatible = IConversionProfile.AreCompatible(ionType, clrType);

                Assert.IsFalse(areCompatible);
            }
            #endregion

            #region clob
            types = new[]
            {
                typeof(List<byte>),
                typeof(byte[]),
                typeof(HashSet<byte>)
            };
            foreach (var t in types)
            {
                clrType = t;
                ionType = Ion.Types.IonTypes.Clob;
                areCompatible = IConversionProfile.AreCompatible(ionType, clrType);

                Assert.IsTrue(areCompatible);

                clrType = typeof(Type);
                areCompatible = IConversionProfile.AreCompatible(ionType, clrType);

                Assert.IsFalse(areCompatible);
            }
            #endregion

            #region list
            types = new[]
            {
                typeof(List<object>),
                typeof(byte[]),
                typeof(HashSet<Guid>)
            };
            foreach (var t in types)
            {
                clrType = t;
                ionType = Ion.Types.IonTypes.List;
                areCompatible = IConversionProfile.AreCompatible(ionType, clrType);

                Assert.IsTrue(areCompatible);

                clrType = typeof(Type);
                areCompatible = IConversionProfile.AreCompatible(ionType, clrType);

                Assert.IsFalse(areCompatible);
            }
            #endregion

            #region sexp
            types = new[]
            {
                typeof(List<string>),
                typeof(string[]),
                typeof(HashSet<string>)
            };
            foreach (var t in types)
            {
                clrType = t;
                ionType = Ion.Types.IonTypes.Sexp;
                areCompatible = IConversionProfile.AreCompatible(ionType, clrType);

                Assert.IsTrue(areCompatible);

                clrType = typeof(Type);
                areCompatible = IConversionProfile.AreCompatible(ionType, clrType);

                Assert.IsFalse(areCompatible);
            }
            #endregion

            #region struct
            clrType = typeof(KeyValuePair<string, object>);
            ionType = Ion.Types.IonTypes.Struct;
            areCompatible = IConversionProfile.AreCompatible(ionType, clrType);

            Assert.IsTrue(areCompatible);
            #endregion
        }

        [TestMethod]
        public void ValidateSourceTypeCompatibilityTest()
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => IConversionProfile.ValidateSourceTypeCompatibility(null, typeof(int[])));

            Assert.ThrowsException<ArgumentException>(
                () => IConversionProfile.ValidateSourceTypeCompatibility(typeof(ISet<int>), typeof(int[])));

            Assert.ThrowsException<ArgumentException>(
                () => IConversionProfile.ValidateSourceTypeCompatibility(typeof(string[]), typeof(int[])));

            Assert.ThrowsException<ArgumentException>(
                () => IConversionProfile.ValidateSourceTypeCompatibility(typeof(IEnumerable<int>), typeof(IEnumerable<object>)));
        }
    }

    internal class ComplexMap1: Dictionary<string, Guid>
    {
        public string? Something { get; set; }
    }

    internal class ComplexCollection1: List<Uri>
    {
        public string? Bleh { get; set; }
    }
}
