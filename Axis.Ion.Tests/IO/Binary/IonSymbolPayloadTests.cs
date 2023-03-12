using Axis.Ion.IO.Axion;
using Axis.Ion.IO.Axion.Payload;
using Axis.Ion.Types;
using Axis.Ion.Utils;

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
            payload = new IonSymbolPayload(IonOperator.Parse("**"));
            ITypePayload.Write(memory, payload, options, table);
            memoryArray = memory.ToArray();
            Assert.AreEqual(3, memoryArray.Length);
            Assert.AreEqual(72, memoryArray[0]);

            // quoted symbol
            memory = new MemoryStream();
            payload = new IonSymbolPayload(default(IonQuotedSymbol));
            ITypePayload.Write(memory, payload, options, table);
            memoryArray = memory.ToArray();
            Assert.AreEqual(1, memoryArray.Length);
            Assert.AreEqual(42, memoryArray[0]);

            memory = new MemoryStream();
            payload = new IonSymbolPayload(IonQuotedSymbol.Parse("'stuffz again'"));
            ITypePayload.Write(memory, payload, options, table);
            memoryArray = memory.ToArray();
            Assert.AreEqual(26, memoryArray.Length);
            Assert.AreEqual(10, memoryArray[0]);

            // identifier symbol
            memory = new MemoryStream();
            payload = new IonSymbolPayload(default(IonIdentifier));
            ITypePayload.Write(memory, payload, options, table);
            memoryArray = memory.ToArray();
            Assert.AreEqual(1, memoryArray.Length);
            Assert.AreEqual(41, memoryArray[0]);

            memory = new MemoryStream();
            payload = new IonSymbolPayload(IonIdentifier.Parse("stuffz_again"));
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
            var payload = new IonSymbolPayload(IonOperator.Parse("**"));
            ITypePayload.Write(memory, payload, options, table);
            memory.Position = 0;
            var payload2 = IonSymbolPayload.Read(
                memory,
                TypeMetadata.ReadMetadata(memory),
                options,
                table);

            Assert.IsTrue(payload2.IonType is IonOperator);
            Assert.AreEqual("**", payload2.IonType.ToIonText());


            memory = new MemoryStream();
            payload = new IonSymbolPayload(IonIdentifier.Parse("stuff"));
            ITypePayload.Write(memory, payload, options, table);
            memory.Position = 0;
            payload2 = IonSymbolPayload.Read(
                memory,
                TypeMetadata.ReadMetadata(memory),
                options,
                table);

            Assert.IsTrue(payload2.IonType is IonIdentifier);
            Assert.AreEqual("stuff", payload2.IonType.ToIonText());


            memory = new MemoryStream();
            payload = new IonSymbolPayload(IonQuotedSymbol.Parse("'stuff'"));
            ITypePayload.Write(memory, payload, options, table);
            memory.Position = 0;
            payload2 = IonSymbolPayload.Read(
                memory,
                TypeMetadata.ReadMetadata(memory),
                options,
                table);

            Assert.IsTrue(payload2.IonType is IonQuotedSymbol);
            Assert.AreEqual("'stuff'", payload2.IonType.ToIonText());
        }
    }
}
