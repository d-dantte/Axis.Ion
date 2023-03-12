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
    public class CollectionConversionProfileTests
    {
        private static readonly CollectionConversionProfile profile = new CollectionConversionProfile();

        [TestMethod]
        public void CanConvert_Tests()
        {
            var canConvert = profile.CanConvert(typeof(IEnumerable<int>));
            Assert.IsTrue(canConvert);

            canConvert = profile.CanConvert(typeof(ICollection<int>));
            Assert.IsTrue(canConvert);

            canConvert = profile.CanConvert(typeof(IList<int>));
            Assert.IsTrue(canConvert);

            canConvert = profile.CanConvert(typeof(ISet<int>));
            Assert.IsTrue(canConvert);

            canConvert = profile.CanConvert(typeof(List<int>));
            Assert.IsTrue(canConvert);

            canConvert = profile.CanConvert(typeof(HashSet<int>));
            Assert.IsTrue(canConvert);

            canConvert = profile.CanConvert(typeof(Queue<int>));
            Assert.IsTrue(canConvert);

            canConvert = profile.CanConvert(typeof(Stack<int>));
            Assert.IsTrue(canConvert);

            canConvert = profile.CanConvert(typeof(int[]));
            Assert.IsTrue(canConvert);

            canConvert = profile.CanConvert(typeof(SampleIntList));
            Assert.IsTrue(canConvert);

            canConvert = profile.CanConvert(typeof(SampleStringList));
            Assert.IsTrue(canConvert);
        }

        [TestMethod]
        public void ToIon_Tests()
        {
            var options = new ConversionOptions();
            (Type type, IEnumerable<int> instance)[] args = new[]
            {
                (typeof(IList<int>), (IEnumerable<int>) new List<int>{ 3, 4, 5 }),
                (typeof(IEnumerable<int>),  new int[]{ 3, 4, 5 }),
                (typeof(HashSet<int>),  new HashSet<int>{ 3, 4, 5 }),
                (typeof(int[]),  new int[]{ 3, 4, 5 })
            };

            // null
            var result = profile.ToIon(typeof(int[]), null, options);
            Assert.IsNotNull(result);
            Assert.AreEqual(IonTypes.List, result.Type);

            var ionList = (IonList)result;
            Assert.IsTrue(ionList.IsNull);

            foreach (var arg in args)
            {
                result = profile.ToIon(arg.type, arg.instance, options);
                Assert.IsNotNull(result);
                Assert.AreEqual(IonTypes.List, result.Type);

                ionList = (IonList)result;
                Assert.IsTrue(arg.instance.SequenceEqual(ionList.Value.Select(v => (int)((IonInt)v).Value)));
            }
        }

        [TestMethod]
        public void FromIon_Tests()
        {
            var options = new ConversionOptions();
            IonList ionList = new IonList.Initializer
            {
                5, 6, 7, 8
            };

            Assert.ThrowsException<ArgumentNullException>(
                () => profile.FromIon(null, IonList.Null(), options));

            Assert.ThrowsException<ArgumentNullException>(
                () => profile.FromIon(typeof(IEnumerable<int>), null, options));

            Assert.ThrowsException<ArgumentException>(
                () => profile.FromIon(typeof(object), IonList.Null(), options));

            Assert.ThrowsException<InvalidCastException>(
                () => profile.FromIon(typeof(IEnumerable<int>), IonInt.Null(), options));

            Assert.ThrowsException<InvalidCastException>(
                () => profile.FromIon(typeof(List<int>), IonInt.Null(), options));

            Assert.ThrowsException<InvalidCastException>(
                () => profile.FromIon(typeof(SampleIntList), IonInt.Null(), options));

            var result = profile.FromIon(typeof(SampleIntList), IonList.Null(), options);
            Assert.IsNull(result);

            result = profile.FromIon(typeof(SampleIntList), ionList, options);
            Assert.IsNotNull(result);
            Assert.AreEqual(typeof(SampleIntList), result.GetType());
            Assert.IsTrue(new[] { 5, 6, 7, 8 }.SequenceEqual(result as IEnumerable<int>));

            result = profile.FromIon(typeof(ICollection<int>), ionList, options);
            Assert.IsNotNull(result);
            Assert.AreEqual(typeof(List<int>), result.GetType());
            Assert.IsTrue(new[] { 5, 6, 7, 8 }.SequenceEqual(result as IEnumerable<int>));

            result = profile.FromIon(typeof(ISet<int>), ionList, options);
            Assert.IsNotNull(result);
            Assert.AreEqual(typeof(HashSet<int>), result.GetType());
            Assert.IsTrue(new[] { 5, 6, 7, 8 }.SequenceEqual(result as IEnumerable<int>));

            result = profile.FromIon(typeof(int[]), ionList, options);
            Assert.IsNotNull(result);
            Assert.AreEqual(typeof(int[]), result.GetType());
            Assert.IsTrue(new[] { 5, 6, 7, 8 }.SequenceEqual(result as IEnumerable<int>));

            Assert.ThrowsException<ArgumentException>(() => profile.FromIon(typeof(ICustomList<int>), ionList, options));
        }


        public class SampleIntList : List<int> { }
        public class SampleStringList : List<string> { }
        public interface ICustomList<T> : IList<T> { }

    }
}
