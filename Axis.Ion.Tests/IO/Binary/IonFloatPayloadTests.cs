using Axis.Ion.IO.Axion;
using Axis.Ion.IO.Axion.Payload;
using Axis.Ion.Types;
using Axis.Ion.Utils;
using Axis.Luna.Common.Results;

namespace Axis.Ion.Tests.IO.Binary
{
    [TestClass]
    public class IonFloatPayloadTests
    {
        [TestMethod]
        public void Write()
        {
            var options = new SerializerOptions();

            #region default
            var memory = new MemoryStream();
            var payload = new IonFloatPayload(default);
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            var memoryArray = memory.ToArray();
            Assert.AreEqual(1, memoryArray.Length);
            Assert.AreEqual(37, memoryArray[0]);

            // annotations
            memory = new MemoryStream();
            payload = new IonFloatPayload(
                (IonFloat)IIonValue.NullOf(
                    IonTypes.Float,
                    IIonValue.Annotation.ParseCollection("abc::'xyz'").Resolve()));
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memoryArray = memory.ToArray();
            Assert.AreEqual(18, memoryArray.Length);
            Assert.AreEqual(53, memoryArray[0]);
            #endregion

            #region nan
            memory = new MemoryStream();
            payload = new IonFloatPayload(new IonFloat(double.NaN));
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memoryArray = memory.ToArray();
            Assert.AreEqual(1, memoryArray.Length);
            Assert.AreEqual(69, memoryArray[0]);

            // annotations
            memory = new MemoryStream();
            payload = new IonFloatPayload(
                new IonFloat(
                    double.NaN,
                    IIonValue.Annotation.ParseCollection("abc::'xyz'").Resolve()));
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memoryArray = memory.ToArray();
            Assert.AreEqual(18, memoryArray.Length);
            Assert.AreEqual(85, memoryArray[0]);
            #endregion

            #region +inf
            memory = new MemoryStream();
            payload = new IonFloatPayload(new IonFloat(double.PositiveInfinity));
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memoryArray = memory.ToArray();
            Assert.AreEqual(1, memoryArray.Length);
            Assert.AreEqual(133, memoryArray[0]);

            // annotations
            memory = new MemoryStream();
            payload = new IonFloatPayload(
                new IonFloat(
                    double.PositiveInfinity,
                    IIonValue.Annotation.ParseCollection("abc::'xyz'").Resolve()));
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memoryArray = memory.ToArray();
            Assert.AreEqual(18, memoryArray.Length);
            Assert.AreEqual(149, memoryArray[0]);
            #endregion

            #region -inf
            memory = new MemoryStream();
            payload = new IonFloatPayload(new IonFloat(double.NegativeInfinity));
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memoryArray = memory.ToArray();
            Assert.AreEqual(1, memoryArray.Length);
            Assert.AreEqual(197, memoryArray[0]);

            // annotations
            memory = new MemoryStream();
            payload = new IonFloatPayload(
                new IonFloat(
                    double.NegativeInfinity,
                    IIonValue.Annotation.ParseCollection("abc::'xyz'").Resolve()));
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memoryArray = memory.ToArray();
            Assert.AreEqual(18, memoryArray.Length);
            Assert.AreEqual(213, memoryArray[0]);
            #endregion

            #region value
            memory = new MemoryStream();
            payload = new IonFloatPayload(new IonFloat(09876434));
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memoryArray = memory.ToArray();
            Assert.AreEqual(9, memoryArray.Length);
            Assert.AreEqual(5, memoryArray[0]);

            // annotations
            memory = new MemoryStream();
            payload = new IonFloatPayload(
                new IonFloat(
                    0654.323456,
                    IIonValue.Annotation.ParseCollection("abc::'xyz'").Resolve()));
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memoryArray = memory.ToArray();
            Assert.AreEqual(26, memoryArray.Length);
            Assert.AreEqual(21, memoryArray[0]);
            #endregion
        }

        [TestMethod]
        public void Read()
        {
            var options = new SerializerOptions();

            #region default
            var memory = new MemoryStream();
            var payload = new IonFloatPayload(default);
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            var memoryArray = memory.ToArray();
            memory.Position = 0;
            var payload2 = IonFloatPayload.Read(
                memory,
                TypeMetadata.ReadMetadata(memory),
                options,
                new SymbolHashList());
            Assert.AreEqual(default(IonFloat), payload2.IonValue);

            // annotations
            var ionValue = (IonFloat)IIonValue.NullOf(
                IonTypes.Float,
                IIonValue.Annotation.ParseCollection("abc::'xyz'").Resolve());
            memory = new MemoryStream();
            payload = new IonFloatPayload(ionValue);
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memoryArray = memory.ToArray();
            memory.Position = 0;
            payload2 = IonFloatPayload.Read(
                memory,
                TypeMetadata.ReadMetadata(memory),
                options,
                new SymbolHashList());
            Assert.AreEqual(ionValue, payload2.IonValue);
            #endregion

            #region nan
            ionValue = new IonFloat(double.NaN);
            memory = new MemoryStream();
            payload = new IonFloatPayload(ionValue);
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memoryArray = memory.ToArray();
            memory.Position = 0;
            payload2 = IonFloatPayload.Read(
                memory,
                TypeMetadata.ReadMetadata(memory),
                options,
                new SymbolHashList());
            Assert.AreEqual(ionValue, payload2.IonValue);

            // annotations
            ionValue = new IonFloat(
                double.NaN,
                IIonValue.Annotation.ParseCollection("abc::'xyz'").Resolve());
            memory = new MemoryStream();
            payload = new IonFloatPayload(ionValue);
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memoryArray = memory.ToArray();
            memory.Position = 0;
            payload2 = IonFloatPayload.Read(
                memory,
                TypeMetadata.ReadMetadata(memory),
                options,
                new SymbolHashList());
            Assert.AreEqual(ionValue, payload2.IonValue);
            #endregion

            #region +inf
            ionValue = new IonFloat(double.PositiveInfinity);
            memory = new MemoryStream();
            payload = new IonFloatPayload(ionValue);
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memoryArray = memory.ToArray();
            memory.Position = 0;
            payload2 = IonFloatPayload.Read(
                memory,
                TypeMetadata.ReadMetadata(memory),
                options,
                new SymbolHashList());
            Assert.AreEqual(ionValue, payload2.IonValue);

            // annotations
            ionValue = new IonFloat(
                double.PositiveInfinity,
                IIonValue.Annotation.ParseCollection("abc::'xyz'").Resolve());
            memory = new MemoryStream();
            payload = new IonFloatPayload(ionValue);
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memoryArray = memory.ToArray();
            memory.Position = 0;
            payload2 = IonFloatPayload.Read(
                memory,
                TypeMetadata.ReadMetadata(memory),
                options,
                new SymbolHashList());
            Assert.AreEqual(ionValue, payload2.IonValue);
            #endregion

            #region -inf
            ionValue = new IonFloat(double.NegativeInfinity);
            memory = new MemoryStream();
            payload = new IonFloatPayload(ionValue);
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memoryArray = memory.ToArray();
            memory.Position = 0;
            payload2 = IonFloatPayload.Read(
                memory,
                TypeMetadata.ReadMetadata(memory),
                options,
                new SymbolHashList());
            Assert.AreEqual(ionValue, payload2.IonValue);

            // annotations
            ionValue = new IonFloat(
                double.NegativeInfinity,
                IIonValue.Annotation.ParseCollection("abc::'xyz'").Resolve());
            memory = new MemoryStream();
            payload = new IonFloatPayload(ionValue);
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memoryArray = memory.ToArray();
            memory.Position = 0;
            payload2 = IonFloatPayload.Read(
                memory,
                TypeMetadata.ReadMetadata(memory),
                options,
                new SymbolHashList());
            Assert.AreEqual(ionValue, payload2.IonValue);
            #endregion

            #region value
            ionValue = new IonFloat(987656.01);
            memory = new MemoryStream();
            payload = new IonFloatPayload(ionValue);
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memoryArray = memory.ToArray();
            memory.Position = 0;
            payload2 = IonFloatPayload.Read(
                memory,
                TypeMetadata.ReadMetadata(memory),
                options,
                new SymbolHashList());
            Assert.AreEqual(ionValue, payload2.IonValue);

            // annotations
            ionValue = new IonFloat(
                98643.688743,
                IIonValue.Annotation.ParseCollection("abc::'xyz'").Resolve());
            memory = new MemoryStream();
            payload = new IonFloatPayload(ionValue);
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memoryArray = memory.ToArray();
            memory.Position = 0;
            payload2 = IonFloatPayload.Read(
                memory,
                TypeMetadata.ReadMetadata(memory),
                options,
                new SymbolHashList());
            Assert.AreEqual(ionValue, payload2.IonValue);
            #endregion
        }
    }
}
