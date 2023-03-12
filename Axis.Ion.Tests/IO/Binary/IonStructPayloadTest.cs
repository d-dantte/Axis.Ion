using Axis.Ion.IO.Axion;
using Axis.Ion.IO.Axion.Payload;
using Axis.Ion.Types;
using Axis.Ion.Utils;

namespace Axis.Ion.Tests.IO.Binary
{
    [TestClass]
    public class IonStructPayloadTest
    {
        [TestMethod]
        public void Write()
        {
            var options = new SerializerOptions();

            #region default
            var memory = new MemoryStream();
            var payload = new IonStructPayload(default);
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            var memoryArray = memory.ToArray();
            Assert.AreEqual(1, memoryArray.Length);
            Assert.AreEqual(47, memoryArray[0]);

            // annotations
            memory = new MemoryStream();
            payload = new IonStructPayload(
                (IonStruct)IIonType.NullOf(
                    IonTypes.Struct,
                    IIonType.Annotation.ParseCollection("abc::'xyz'")));
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memoryArray = memory.ToArray();
            Assert.AreEqual(18, memoryArray.Length);
            Assert.AreEqual(63, memoryArray[0]);
            #endregion

            #region value
            memory = new MemoryStream();
            payload = new IonStructPayload(new IonStruct.Initializer
            {
                ["stuff"] = 455,
                ["otherStuff"] = new IonOperator(Operators.Plus),
                ["super_bleh"] = "other stuff"
            });
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memoryArray = memory.ToArray();
            Assert.AreEqual(87, memoryArray.Length);
            Assert.AreEqual(15, memoryArray[0]);

            // annotations
            memory = new MemoryStream();
            payload = new IonStructPayload(
                new IonStruct.Initializer(IIonType.Annotation.ParseCollection("abc::'xyz'"))
                {
                    ["stuff"] = 455,
                    ["otherStuff"] = new IonOperator(Operators.Plus),
                    ["super_bleh"] = "other stuff"
                });
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memoryArray = memory.ToArray();
            Assert.AreEqual(104, memoryArray.Length);
            Assert.AreEqual(31, memoryArray[0]);
            #endregion
        }

        [TestMethod]
        public void Read()
        {
            var options = new SerializerOptions();

            #region default
            var memory = new MemoryStream();
            var payload = new IonStructPayload(default);
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memory.Position = 0;
            var payload2 = IonStructPayload.Read(
                memory,
                TypeMetadata.ReadMetadata(memory),
                options,
                 new SymbolHashList());
            Assert.AreEqual(default(IonStruct), payload2.IonType);

            // annotations
            var ionValue = (IonStruct)IIonType.NullOf(
                IonTypes.Struct,
                IIonType.Annotation.ParseCollection("abc::'xyz'"));
            memory = new MemoryStream();
            payload = new IonStructPayload(ionValue);
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memory.Position = 0;
            payload2 = IonStructPayload.Read(
                memory,
                TypeMetadata.ReadMetadata(memory),
                options,
                 new SymbolHashList());
            Assert.AreEqual(ionValue, payload2.IonType);
            #endregion

            #region value
            ionValue = new IonStruct.Initializer
            {
                ["stuff"] = 455,
                ["otherStuff"] = new IonOperator(Operators.Plus),
                ["super_bleh"] = "other stuff"
            };
            memory = new MemoryStream();
            payload = new IonStructPayload(ionValue);
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memory.Position = 0;
            payload2 = IonStructPayload.Read(
                memory,
                TypeMetadata.ReadMetadata(memory),
                options,
                 new SymbolHashList());
            Assert.AreEqual(ionValue, payload2.IonType);

            // annotations
            ionValue = new IonStruct.Initializer(IIonType.Annotation.ParseCollection("abc::'xyz'"))
            {
                ["stuff"] = 455,
                ["otherStuff"] = new IonOperator(Operators.Plus),
                ["super_bleh"] = "other stuff"
            };
            memory = new MemoryStream();
            payload = new IonStructPayload(ionValue);
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memory.Position = 0;
            payload2 = IonStructPayload.Read(
                memory,
                TypeMetadata.ReadMetadata(memory),
                options,
                 new SymbolHashList());
            Assert.AreEqual(ionValue, payload2.IonType);
            #endregion
        }
    }
}
