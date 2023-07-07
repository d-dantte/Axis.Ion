using Axis.Ion.IO.Axion;
using Axis.Ion.IO.Axion.Payload;
using Axis.Ion.Types;
using Axis.Ion.Utils;
using Axis.Luna.Common.Results;

namespace Axis.Ion.Tests.IO.Binary
{
    [TestClass]
    public class IonSymbolPayloadTests
    {
        [TestMethod]
        public void Write()
        {
            var options = new SerializerOptions();
            var table = new SymbolHashList();

            // operator symbol
            var memory = new MemoryStream();
            var payload = new IonSymbolPayload(default(IonOperator));
            ITypePayload.Write(memory, payload, options, table);
            var memoryArray = memory.ToArray();
            Assert.AreEqual(1, memoryArray.Length);
            Assert.AreEqual(40, memoryArray[0]);

            memory = new MemoryStream();
            payload = new IonSymbolPayload(IonOperator.Parse("**").Resolve());
            ITypePayload.Write(memory, payload, options, table);
            memoryArray = memory.ToArray();
            Assert.AreEqual(3, memoryArray.Length);
            Assert.AreEqual(72, memoryArray[0]);

            // symbol
            memory = new MemoryStream();
            payload = new IonSymbolPayload(default(IonTextSymbol));
            ITypePayload.Write(memory, payload, options, table);
            memoryArray = memory.ToArray();
            Assert.AreEqual(1, memoryArray.Length);
            Assert.AreEqual(41, memoryArray[0]);

            memory = new MemoryStream();
            payload = new IonSymbolPayload(IonTextSymbol.Parse("'stuffz again'").Resolve());
            ITypePayload.Write(memory, payload, options, table);
            memoryArray = memory.ToArray();
            Assert.AreEqual(26, memoryArray.Length);
            Assert.AreEqual(9, memoryArray[0]);

            memory = new MemoryStream();
            payload = new IonSymbolPayload(IonTextSymbol.Parse("stuffz_again").Resolve());
            ITypePayload.Write(memory, payload, options, table);
            memoryArray = memory.ToArray();
            Assert.AreEqual(26, memoryArray.Length);
            Assert.AreEqual(9, memoryArray[0]);
        }

        [TestMethod]
        public void Read()
        {
            var options = new SerializerOptions();
            var table = new SymbolHashList();

            var memory = new MemoryStream();
            var payload = new IonSymbolPayload(IonOperator.Parse("**").Resolve());
            ITypePayload.Write(memory, payload, options, table);
            memory.Position = 0;
            var payload2 = IonSymbolPayload.Read(
                memory,
                TypeMetadata.ReadMetadata(memory),
                options,
                table);

            Assert.IsTrue(payload2.IonValue is IonOperator);
            Assert.AreEqual("**", payload2.IonValue.ToIonText());


            memory = new MemoryStream();
            payload = new IonSymbolPayload(IonTextSymbol.Parse("stuff").Resolve());
            ITypePayload.Write(memory, payload, options, table);
            memory.Position = 0;
            payload2 = IonSymbolPayload.Read(
                memory,
                TypeMetadata.ReadMetadata(memory),
                options,
                table);

            Assert.IsTrue(payload2.IonValue is IonTextSymbol);
            Assert.AreEqual("stuff", payload2.IonValue.ToIonText());


            memory = new MemoryStream();
            payload = new IonSymbolPayload(IonTextSymbol.Parse("'stuff'").Resolve());
            ITypePayload.Write(memory, payload, options, table);
            memory.Position = 0;
            payload2 = IonSymbolPayload.Read(
                memory,
                TypeMetadata.ReadMetadata(memory),
                options,
                (table = new SymbolHashList()));

            Assert.IsTrue(payload2.IonValue is IonTextSymbol);
            Assert.AreEqual("stuff", payload2.IonValue.ToIonText());
        }
    }
}
