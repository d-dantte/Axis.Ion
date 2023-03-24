using Axis.Ion.Conversion;
using Axis.Ion.Conversion.Converters;
using Axis.Ion.Types;

namespace Axis.Ion.Tests.Conversion.IonProfiles
{
    [TestClass]
    public class CollectionConversionProfileTests
    {
        private static readonly CollectionConverter profile = new CollectionConverter();

        [TestMethod]
        public void CanConvertToIon_Tests()
        {
            var canConvert = profile.CanConvert(typeof(IEnumerable<int>), new int[0]);
            Assert.IsTrue(canConvert);

            canConvert = profile.CanConvert(typeof(ICollection<int>), new int[0]);
            Assert.IsTrue(canConvert);

            canConvert = profile.CanConvert(typeof(IList<int>), new List<int>());
            Assert.IsTrue(canConvert);

            canConvert = profile.CanConvert(typeof(ISet<int>), new HashSet<int>());
            Assert.IsTrue(canConvert);

            canConvert = profile.CanConvert(typeof(List<int>), new List<int>());
            Assert.IsTrue(canConvert);

            canConvert = profile.CanConvert(typeof(HashSet<int>), new HashSet<int>());
            Assert.IsTrue(canConvert);

            canConvert = profile.CanConvert(typeof(Queue<int>), new Queue<int>());
            Assert.IsTrue(canConvert);

            canConvert = profile.CanConvert(typeof(Stack<int>), new Stack<int>());
            Assert.IsTrue(canConvert);

            canConvert = profile.CanConvert(typeof(int[]), new int[0]);
            Assert.IsTrue(canConvert);

            canConvert = profile.CanConvert(typeof(SampleIntList), new SampleIntList());
            Assert.IsTrue(canConvert);

            canConvert = profile.CanConvert(typeof(SampleStringList), new SampleStringList());
            Assert.IsTrue(canConvert);

            Assert.IsFalse(profile.CanConvert(typeof(List<int>), new int[0]));
        }

        [TestMethod]
        public void CanConvertToClr_Tests()
        {
            var ionList = IonList.Null();
            var canConvert = profile.CanConvert(typeof(IEnumerable<int>), ionList);
            Assert.IsTrue(canConvert);

            canConvert = profile.CanConvert(typeof(ICollection<int>), ionList);
            Assert.IsTrue(canConvert);

            canConvert = profile.CanConvert(typeof(IList<int>), ionList);
            Assert.IsTrue(canConvert);

            canConvert = profile.CanConvert(typeof(ISet<int>), ionList);
            Assert.IsTrue(canConvert);

            canConvert = profile.CanConvert(typeof(List<int>), ionList);
            Assert.IsTrue(canConvert);

            canConvert = profile.CanConvert(typeof(HashSet<int>), ionList);
            Assert.IsTrue(canConvert);

            canConvert = profile.CanConvert(typeof(Queue<int>), ionList);
            Assert.IsTrue(canConvert);

            canConvert = profile.CanConvert(typeof(Stack<int>), ionList);
            Assert.IsTrue(canConvert);

            canConvert = profile.CanConvert(typeof(int[]), ionList);
            Assert.IsTrue(canConvert);

            canConvert = profile.CanConvert(typeof(SampleIntList), ionList);
            Assert.IsTrue(canConvert);

            canConvert = profile.CanConvert(typeof(SampleStringList), ionList);
            Assert.IsTrue(canConvert);

            Assert.ThrowsException<ArgumentNullException>(() => profile.CanConvert(null, ionList));
            Assert.ThrowsException<ArgumentNullException>(() => profile.CanConvert(typeof(int[]), (IIonType)null));
        }

        [TestMethod]
        public void ToIon_Tests()
        {
            var options = new ConversionContext(ConversionOptionsBuilder.NewBuilder().Build());
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
        public void ToClr_Tests()
        {
            var options = new ConversionContext(ConversionOptionsBuilder.NewBuilder().Build());
            IonList ionList = new IonList.Initializer
            {
                5, 6, 7, 8
            };

            Assert.ThrowsException<ArgumentNullException>(
                () => profile.ToClr(null, IonList.Null(), options));

            Assert.ThrowsException<ArgumentNullException>(
                () => profile.ToClr(typeof(IEnumerable<int>), null, options));

            Assert.ThrowsException<ArgumentException>(
                () => profile.ToClr(typeof(object), IonList.Null(), options));

            Assert.ThrowsException<InvalidCastException>(
                () => profile.ToClr(typeof(IEnumerable<int>), IonInt.Null(), options));

            Assert.ThrowsException<InvalidCastException>(
                () => profile.ToClr(typeof(List<int>), IonInt.Null(), options));

            Assert.ThrowsException<InvalidCastException>(
                () => profile.ToClr(typeof(SampleIntList), IonInt.Null(), options));

            var result = profile.ToClr(typeof(SampleIntList), IonList.Null(), options);
            Assert.IsNull(result);

            result = profile.ToClr(typeof(SampleIntList), ionList, options);
            Assert.IsNotNull(result);
            Assert.AreEqual(typeof(SampleIntList), result.GetType());
            Assert.IsTrue(new[] { 5, 6, 7, 8 }.SequenceEqual(result as IEnumerable<int>));

            result = profile.ToClr(typeof(ICollection<int>), ionList, options);
            Assert.IsNotNull(result);
            Assert.AreEqual(typeof(List<int>), result.GetType());
            Assert.IsTrue(new[] { 5, 6, 7, 8 }.SequenceEqual(result as IEnumerable<int>));

            result = profile.ToClr(typeof(ISet<int>), ionList, options);
            Assert.IsNotNull(result);
            Assert.AreEqual(typeof(HashSet<int>), result.GetType());
            Assert.IsTrue(new[] { 5, 6, 7, 8 }.SequenceEqual(result as IEnumerable<int>));

            result = profile.ToClr(typeof(int[]), ionList, options);
            Assert.IsNotNull(result);
            Assert.AreEqual(typeof(int[]), result.GetType());
            Assert.IsTrue(new[] { 5, 6, 7, 8 }.SequenceEqual(result as IEnumerable<int>));

            result = profile.ToClr(typeof(Stack<int>), ionList, options);
            Assert.IsNotNull(result);
            Assert.AreEqual(typeof(Stack<int>), result.GetType());
            Assert.IsTrue(new[] { 5, 6, 7, 8 }.SequenceEqual(result as IEnumerable<int>));

            result = profile.ToClr(typeof(Queue<int>), ionList, options);
            Assert.IsNotNull(result);
            Assert.AreEqual(typeof(Queue<int>), result.GetType());
            Assert.IsTrue(new[] { 5, 6, 7, 8 }.SequenceEqual(result as IEnumerable<int>));

            Assert.ThrowsException<ArgumentException>(() => profile.ToClr(typeof(ICustomList<int>), ionList, options));
        }


        public class SampleIntList : List<int> { }
        public class SampleStringList : List<string> { }
        public interface ICustomList<T> : IList<T> { }

    }
}
