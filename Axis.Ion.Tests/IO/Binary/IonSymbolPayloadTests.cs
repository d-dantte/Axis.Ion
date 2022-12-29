using Axis.Ion.IO.Binary;
using Axis.Ion.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axis.Ion.Tests.IO.Binary
{
    [TestClass]
    public class IonSymbolPayloadTests
    {

        [TestMethod]
        public void Write()
        {
            // operator symbol
            var memory = new MemoryStream();
            var payload = new IonSymbolPayload(default(IIonSymbol.Operator));
            payload.Write(memory);
            var memoryArray = memory.ToArray();
            Assert.AreEqual(1, memoryArray.Length);
            Assert.AreEqual(104, memoryArray[0]);

            memory = new MemoryStream();
            payload = new IonSymbolPayload(IIonSymbol.Of("**"));
            payload.Write(memory);
            memoryArray = memory.ToArray();
            Assert.AreEqual(7, memoryArray.Length);
            Assert.AreEqual(72, memoryArray[0]);

            // quoted symbol
            memory = new MemoryStream();
            payload = new IonSymbolPayload(default(IIonSymbol.QuotedSymbol));
            payload.Write(memory);
            memoryArray = memory.ToArray();
            Assert.AreEqual(1, memoryArray.Length);
            Assert.AreEqual(168, memoryArray[0]);

            memory = new MemoryStream();
            payload = new IonSymbolPayload(IIonSymbol.Of("'stuffz again'"));
            payload.Write(memory);
            memoryArray = memory.ToArray();
            Assert.AreEqual(27, memoryArray.Length);
            Assert.AreEqual(136, memoryArray[0]);

            // identifier symbol
            memory = new MemoryStream();
            payload = new IonSymbolPayload(default(IIonSymbol.Identifier));
            payload.Write(memory);
            memoryArray = memory.ToArray();
            Assert.AreEqual(1, memoryArray.Length);
            Assert.AreEqual(232, memoryArray[0]);

            memory = new MemoryStream();
            payload = new IonSymbolPayload(IIonSymbol.Of("stuffz_again"));
            payload.Write(memory);
            memoryArray = memory.ToArray();
            Assert.AreEqual(27, memoryArray.Length);
            Assert.AreEqual(200, memoryArray[0]);
        }

        [TestMethod]
        public void Read()
        {
            var memory = new MemoryStream();
            var payload = new IonSymbolPayload(IIonSymbol.Of("**"));
            payload.Write(memory);
            memory.Position = 1;
            var payload2 = IonSymbolPayload.Read(payload.Metadata, memory);

            Assert.IsTrue(payload2.IonValue is IIonSymbol.Operator);
            Assert.AreEqual("**", payload2.IonValue.ToIonText());


            memory = new MemoryStream();
            payload = new IonSymbolPayload(IIonSymbol.Of("stuff"));
            payload.Write(memory);
            memory.Position = 1;
            payload2 = IonSymbolPayload.Read(payload.Metadata, memory);

            Assert.IsTrue(payload2.IonValue is IIonSymbol.Identifier);
            Assert.AreEqual("stuff", payload2.IonValue.ToIonText());


            memory = new MemoryStream();
            payload = new IonSymbolPayload(IIonSymbol.Of("'stuff'"));
            payload.Write(memory);
            memory.Position = 1;
            payload2 = IonSymbolPayload.Read(payload.Metadata, memory);

            Assert.IsTrue(payload2.IonValue is IIonSymbol.QuotedSymbol);
            Assert.AreEqual("'stuff'", payload2.IonValue.ToIonText());
        }
    }
}
