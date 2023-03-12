using Axis.Ion.IO.Axion;
using Axis.Ion.IO.Axion.Payload;
using Axis.Ion.Types;
using Axis.Ion.Utils;

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
                (IonDecimal)IIonType.NullOf(
                    IonTypes.Decimal,
                    IIonType.Annotation.ParseCollection("abc::'xyz'")));
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
                    IIonType.Annotation.ParseCollection("abc::'xyz'")));
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
            Assert.AreEqual(default(IonDecimal), payload2.IonType);

            // annotations
            var ionValue = (IonDecimal)IIonType.NullOf(
                IonTypes.Decimal,
                IIonType.Annotation.ParseCollection("abc::'xyz'"));
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
            Assert.AreEqual(ionValue, payload2.IonType);
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
            Assert.AreEqual(ionValue, payload2.IonType);

            // annotations
            ionValue = new IonDecimal(
                98643.688743m,
                IIonType.Annotation.ParseCollection("abc::'xyz'"));
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
            Assert.AreEqual(ionValue, payload2.IonType);
            #endregion
        }
    }
}
