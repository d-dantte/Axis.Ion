using Axis.Ion.Conversion.ClrReflection;
using Axis.Ion.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axis.Ion.Tests.Conversion.ClrReflection
{
    [TestClass]
    public class ConstructorReflectionInfoTests
    {
        [TestMethod]
        public void CanConstruct_WithValidArgs()
        {
            var ion = new IonStruct(new IonStruct.Initializer
            {
                ["Id"] = 5,
                ["Name"] = "Isobel",
                ["Dance"] = new IonStruct.Initializer
                {
                    ["Key"] = 12,
                    ["Value"] = "steps"
                }
            });

            #region single-arg ctor
            var ctor = typeof(SampleClass)
                .GetConstructor(new[]
                {
                    typeof(int)
                })
                ?? throw new Exception("Constructor not found");

            var ctorInfo = new ConstructorReflectionInfo(ctor);

            var result = ctorInfo.CanConstruct(ion, out var source);

            Assert.IsTrue(result);
            Assert.IsNotNull(source);
            Assert.AreEqual(1, source.Length);
            #endregion

            #region 2-arg ctor
            ctor = typeof(SampleClass)
                .GetConstructor(new[]
                {
                    typeof(string),
                    typeof(int)
                })
                ?? throw new Exception("Constructor not found");

            ctorInfo = new ConstructorReflectionInfo(ctor);

            result = ctorInfo.CanConstruct(ion, out source);

            Assert.IsTrue(result);
            Assert.IsNotNull(source);
            Assert.AreEqual(2, source.Length);
            #endregion

            #region 3-arg ctor
            ctor = typeof(SampleClass)
                .GetConstructor(new[]
                {
                    typeof(string),
                    typeof(int),
                    typeof(KeyValuePair<int, string>)
                })
                ?? throw new Exception("Constructor not found");

            ctorInfo = new ConstructorReflectionInfo(ctor);

            result = ctorInfo.CanConstruct(ion, out source);

            Assert.IsTrue(result);
            Assert.IsNotNull(source);
            Assert.AreEqual(3, source.Length);
            #endregion
        }

        [TestMethod]
        public void CanConstruct_WithInvalidArgs()
        {
            var ion = new IonStruct(new IonStruct.Initializer
            {
                ["Ids"] = 5,
                ["FirstName"] = "Isobel",
                ["Dances"] = new IonStruct.Initializer
                {
                    ["Key"] = 12,
                    ["Value"] = "steps"
                }
            });

            #region single-arg ctor
            var ctor = typeof(SampleClass)
                .GetConstructor(new[]
                {
                    typeof(int)
                })
                ?? throw new Exception("Constructor not found");

            var ctorInfo = new ConstructorReflectionInfo(ctor);

            var result = ctorInfo.CanConstruct(ion, out var source);

            Assert.IsFalse(result);
            #endregion

            #region 2-arg ctor
            ctor = typeof(SampleClass)
                .GetConstructor(new[]
                {
                    typeof(string),
                    typeof(int)
                })
                ?? throw new Exception("Constructor not found");

            ctorInfo = new ConstructorReflectionInfo(ctor);

            result = ctorInfo.CanConstruct(ion, out source);

            Assert.IsFalse(result);
            #endregion

            #region 3-arg ctor
            ctor = typeof(SampleClass)
                .GetConstructor(new[]
                {
                    typeof(string),
                    typeof(int),
                    typeof(KeyValuePair<int, string>)
                })
                ?? throw new Exception("Constructor not found");

            ctorInfo = new ConstructorReflectionInfo(ctor);

            result = ctorInfo.CanConstruct(ion, out source);

            Assert.IsFalse(result);
            #endregion
        }


        internal class SampleClass
        {
            public SampleClass() { }

            public SampleClass(int id) { }
            public SampleClass(string name) { }
            public SampleClass(string name, int id) { }
            public SampleClass(string name, int id, KeyValuePair<int, string> dance) { }
        }
    }
}
