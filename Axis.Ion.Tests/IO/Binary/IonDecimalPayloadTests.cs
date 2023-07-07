using Axis.Ion.IO.Axion;
using Axis.Ion.IO.Axion.Payload;
using Axis.Ion.Types;
using Axis.Ion.Utils;
using Axis.Luna.Common.Results;

namespace Axis.Ion.Tests.IO.Binary
{
    [TestClass]
    public class IonDecimalPayloadTests
    {
        [TestMethod]
        public void Write()
        {
            var options = new SerializerOptions();

            #region default
            var memory = new MemoryStream();
            var payload = new IonDecimalPayload(default);
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            var memoryArray = memory.ToArray();
            Assert.AreEqual(1, memoryArray.Length);
            Assert.AreEqual(36, memoryArray[0]);

            // annotations
            memory = new MemoryStream();
            payload = new IonDecimalPayload(
                (IonDecimal)IIonValue.NullOf(
                    IonTypes.Decimal,
                    IIonValue.Annotation.ParseCollection("abc::'xyz'").Resolve()));
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memoryArray = memory.ToArray();
            Assert.AreEqual(18, memoryArray.Length);
            Assert.AreEqual(52, memoryArray[0]);
            #endregion

            #region value
            memory = new MemoryStream();
            payload = new IonDecimalPayload(new IonDecimal(09876434));
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memoryArray = memory.ToArray();
            Assert.AreEqual(17, memoryArray.Length);
            Assert.AreEqual(4, memoryArray[0]);

            // annotations
            memory = new MemoryStream();
            payload = new IonDecimalPayload(
                new IonDecimal(
                    0654.323456m,
                    IIonValue.Annotation.ParseCollection("abc::'xyz'").Resolve()));
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memoryArray = memory.ToArray();
            Assert.AreEqual(34, memoryArray.Length);
            Assert.AreEqual(20, memoryArray[0]);
            #endregion
        }

        [TestMethod]
        public void Read()
        {
            var options = new SerializerOptions();

            #region default
            var memory = new MemoryStream();
            var payload = new IonDecimalPayload(default);
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            var memoryArray = memory.ToArray();
            memory.Position = 0;
            var payload2 = IonDecimalPayload.Read(
                memory,
                TypeMetadata.ReadMetadata(memory),
                options,
                new SymbolHashList());
            Assert.AreEqual(default(IonDecimal), payload2.IonValue);

            // annotations
            var ionValue = (IonDecimal)IIonValue.NullOf(
                IonTypes.Decimal,
                IIonValue.Annotation.ParseCollection("abc::'xyz'").Resolve());
            memory = new MemoryStream();
            payload = new IonDecimalPayload(ionValue);
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memoryArray = memory.ToArray();
            memory.Position = 0;
            payload2 = IonDecimalPayload.Read(
                memory,
                TypeMetadata.ReadMetadata(memory),
                options,
                new SymbolHashList());
            Assert.AreEqual(ionValue, payload2.IonValue);
            #endregion

            #region value
            ionValue = new IonDecimal(987656.01m);
            memory = new MemoryStream();
            payload = new IonDecimalPayload(ionValue);
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memoryArray = memory.ToArray();
            memory.Position = 0;
            payload2 = IonDecimalPayload.Read(
                memory,
                TypeMetadata.ReadMetadata(memory),
                options,
                new SymbolHashList());
            Assert.AreEqual(ionValue, payload2.IonValue);

            // annotations
            ionValue = new IonDecimal(
                98643.688743m,
                IIonValue.Annotation.ParseCollection("abc::'xyz'").Resolve());
            memory = new MemoryStream();
            payload = new IonDecimalPayload(ionValue);
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memoryArray = memory.ToArray();
            memory.Position = 0;
            payload2 = IonDecimalPayload.Read(
                memory,
                TypeMetadata.ReadMetadata(memory),
                options,
                new SymbolHashList());
            Assert.AreEqual(ionValue, payload2.IonValue);
            #endregion
        }
    }
}
