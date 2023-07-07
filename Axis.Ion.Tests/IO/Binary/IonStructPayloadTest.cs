using Axis.Ion.IO.Axion;
using Axis.Ion.IO.Axion.Payload;
using Axis.Ion.Types;
using Axis.Ion.Utils;
using Axis.Luna.Common.Results;

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
            var payload = new IonStructPayload(IonStruct.Null());
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            var memoryArray = memory.ToArray();
            Assert.AreEqual(1, memoryArray.Length);
            Assert.AreEqual(46, memoryArray[0]);

            // annotations
            memory = new MemoryStream();
            payload = new IonStructPayload(
                (IonStruct)IIonValue.NullOf(
                    IonTypes.Struct,
                    IIonValue.Annotation.ParseCollection("abc::'xyz'").Resolve()));
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memoryArray = memory.ToArray();
            Assert.AreEqual(18, memoryArray.Length);
            Assert.AreEqual(62, memoryArray[0]);
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
            Assert.AreEqual(14, memoryArray[0]);

            // annotations
            memory = new MemoryStream();
            payload = new IonStructPayload(
                new IonStruct.Initializer(IIonValue.Annotation.ParseCollection("abc::'xyz'").Resolve())
                {
                    ["stuff"] = 455,
                    ["otherStuff"] = new IonOperator(Operators.Plus),
                    ["super_bleh"] = "other stuff"
                });
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memoryArray = memory.ToArray();
            Assert.AreEqual(104, memoryArray.Length);
            Assert.AreEqual(30, memoryArray[0]);
            #endregion
        }

        [TestMethod]
        public void Read()
        {
            var options = new SerializerOptions();

            #region default
            var memory = new MemoryStream();
            var payload = new IonStructPayload(IonStruct.Null());
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memory.Position = 0;
            var payload2 = IonStructPayload.Read(
                memory,
                TypeMetadata.ReadMetadata(memory),
                options,
                 new SymbolHashList());
            Assert.AreEqual(IonStruct.Null(), payload2.IonValue);

            // annotations
            var ionValue = (IonStruct)IIonValue.NullOf(
                IonTypes.Struct,
                IIonValue.Annotation.ParseCollection("abc::'xyz'").Resolve());
            memory = new MemoryStream();
            payload = new IonStructPayload(ionValue);
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memory.Position = 0;
            payload2 = IonStructPayload.Read(
                memory,
                TypeMetadata.ReadMetadata(memory),
                options,
                 new SymbolHashList());
            Assert.AreEqual(ionValue, payload2.IonValue);
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
            Assert.AreEqual(ionValue, payload2.IonValue);

            // annotations
            ionValue = new IonStruct.Initializer(IIonValue.Annotation.ParseCollection("abc::'xyz'").Resolve())
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
            Assert.AreEqual(ionValue, payload2.IonValue);
            #endregion
        }
    }
}
