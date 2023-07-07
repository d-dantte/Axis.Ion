using Axis.Ion.IO.Axion;
using Axis.Ion.IO.Axion.Payload;
using Axis.Ion.Types;
using Axis.Ion.Utils;
using Axis.Luna.Common.Results;

namespace Axis.Ion.Tests.IO.Binary
{
    [TestClass]
    public class IonBoolPayloadTests
    {
        [TestMethod]
        public void Write()
        {
            var options = new SerializerOptions();

            #region default
            var memory = new MemoryStream();
            var payload = new IonBoolPayload(default);
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            var memoryArray = memory.ToArray();
            Assert.AreEqual(1, memoryArray.Length);
            Assert.AreEqual(34, memoryArray[0]);

            // annotations
            memory = new MemoryStream();
            payload = new IonBoolPayload(
                (IonBool)IIonValue.NullOf(
                    IonTypes.Bool,
                    IIonValue.Annotation.ParseCollection("abc::'xyz'").Resolve()));
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memoryArray = memory.ToArray();
            Assert.AreEqual(18, memoryArray.Length);
            Assert.AreEqual(50, memoryArray[0]);
            #endregion

            #region true
            memory = new MemoryStream();
            payload = new IonBoolPayload(new IonBool(true));
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memoryArray = memory.ToArray();
            Assert.AreEqual(1, memoryArray.Length);
            Assert.AreEqual(66, memoryArray[0]);

            // annotations
            memory = new MemoryStream();
            payload = new IonBoolPayload(
                new IonBool(
                    true,
                    IIonValue.Annotation.ParseCollection("abc::'xyz'").Resolve()));
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memoryArray = memory.ToArray();
            Assert.AreEqual(18, memoryArray.Length);
            Assert.AreEqual(82, memoryArray[0]);
            #endregion

            #region false
            memory = new MemoryStream();
            payload = new IonBoolPayload(new IonBool(false));
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memoryArray = memory.ToArray();
            Assert.AreEqual(1, memoryArray.Length);
            Assert.AreEqual(2, memoryArray[0]);

            // annotations
            memory = new MemoryStream();
            payload = new IonBoolPayload(
                new IonBool(
                    false,
                    IIonValue.Annotation.ParseCollection("abc::'xyz'").Resolve()));
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memoryArray = memory.ToArray();
            Assert.AreEqual(18, memoryArray.Length);
            Assert.AreEqual(18, memoryArray[0]);
            #endregion
        }

        [TestMethod]
        public void Read()
        {
            var options = new SerializerOptions();

            #region default
            var memory = new MemoryStream();
            var payload = new IonBoolPayload(default);
            ITypePayload.Write(memory, payload, options,  new SymbolHashList());
            memory.Position = 0;
            var payload2 = IonBoolPayload.Read(
                memory,
                TypeMetadata.ReadMetadata(memory),
                options,
                 new SymbolHashList());
            Assert.AreEqual(default(IonBool), payload2.IonValue);

            // annotations
            var ionValue = (IonBool)IIonValue.NullOf(
                IonTypes.Bool,
                IIonValue.Annotation.ParseCollection("abc::'xyz'").Resolve());
            memory = new MemoryStream();
            payload = new IonBoolPayload(ionValue);
            ITypePayload.Write(memory, payload, options,  new SymbolHashList());
            memory.Position = 0;
            payload2 = IonBoolPayload.Read(
                memory,
                TypeMetadata.ReadMetadata(memory),
                options,
                 new SymbolHashList());
            Assert.AreEqual(ionValue, payload2.IonValue);
            #endregion

            #region true
            ionValue = new IonBool(true);
            memory = new MemoryStream();
            payload = new IonBoolPayload(ionValue);
            ITypePayload.Write(memory, payload, options,  new SymbolHashList());
            memory.Position = 0;
            payload2 = IonBoolPayload.Read(
                memory,
                TypeMetadata.ReadMetadata(memory),
                options,
                 new SymbolHashList());
            Assert.AreEqual(ionValue, payload2.IonValue);

            // annotations
            ionValue = new IonBool(
                true,
                IIonValue.Annotation.ParseCollection("abc::'xyz'").Resolve());
            memory = new MemoryStream();
            payload = new IonBoolPayload(ionValue);
            ITypePayload.Write(memory, payload, options,  new SymbolHashList());
            memory.Position = 0;
            payload2 = IonBoolPayload.Read(
                memory,
                TypeMetadata.ReadMetadata(memory),
                options,
                 new SymbolHashList());
            Assert.AreEqual(ionValue, payload2.IonValue);
            #endregion

            #region false
            ionValue = new IonBool(true);
            memory = new MemoryStream();
            payload = new IonBoolPayload(ionValue);
            ITypePayload.Write(memory, payload, options,  new SymbolHashList());
            memory.Position = 0;
            payload2 = IonBoolPayload.Read(
                memory,
                TypeMetadata.ReadMetadata(memory),
                options,
                 new SymbolHashList());
            Assert.AreEqual(ionValue, payload2.IonValue);

            // annotations
            ionValue = new IonBool(
                false,
                IIonValue.Annotation.ParseCollection("abc::'xyz'").Resolve());
            memory = new MemoryStream();
            payload = new IonBoolPayload(ionValue);
            ITypePayload.Write(memory, payload, options,  new SymbolHashList());
            memory.Position = 0;
            payload2 = IonBoolPayload.Read(
                memory,
                TypeMetadata.ReadMetadata(memory),
                options,
                 new SymbolHashList());
            Assert.AreEqual(ionValue, payload2.IonValue);
            #endregion
        }
    }
}
