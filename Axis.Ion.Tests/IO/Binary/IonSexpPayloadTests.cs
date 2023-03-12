using Axis.Ion.IO.Axion;
using Axis.Ion.IO.Axion.Payload;
using Axis.Ion.Types;
using Axis.Ion.Utils;

namespace Axis.Ion.Tests.IO.Binary
{
    [TestClass]
    public class IonSexpPayloadTests
    {
        [TestMethod]
        public void Write()
        {
            var options = new SerializerOptions();

            #region default
            var memory = new MemoryStream();
            var payload = new IonSexpPayload(default);
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            var memoryArray = memory.ToArray();
            Assert.AreEqual(1, memoryArray.Length);
            Assert.AreEqual(46, memoryArray[0]);

            // annotations
            memory = new MemoryStream();
            payload = new IonSexpPayload(
                (IonSexp)IIonType.NullOf(
                    IonTypes.Sexp,
                    IIonType.Annotation.ParseCollection("abc::'xyz'")));
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memoryArray = memory.ToArray();
            Assert.AreEqual(18, memoryArray.Length);
            Assert.AreEqual(62, memoryArray[0]);
            #endregion

            #region value
            memory = new MemoryStream();
            payload = new IonSexpPayload(new IonSexp.Initializer
            {
                455,
                new IonOperator(Operators.Plus),
                "other stuff"
            });
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memoryArray = memory.ToArray();
            Assert.AreEqual(31, memoryArray.Length);
            Assert.AreEqual(14, memoryArray[0]);

            // annotations
            memory = new MemoryStream();
            payload = new IonSexpPayload(
                new IonSexp.Initializer(IIonType.Annotation.ParseCollection("abc::'xyz'"))
                {
                    455,
                    new IonOperator(Operators.Plus),
                    "other stuff"
                });
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memoryArray = memory.ToArray();
            Assert.AreEqual(48, memoryArray.Length);
            Assert.AreEqual(30, memoryArray[0]);
            #endregion
        }

        [TestMethod]
        public void Read()
        {
            var options = new SerializerOptions();

            #region default
            var memory = new MemoryStream();
            var payload = new IonSexpPayload(default);
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memory.Position = 0;
            var payload2 = IonSexpPayload.Read(
                memory,
                TypeMetadata.ReadMetadata(memory),
                options,
                 new SymbolHashList());
            Assert.AreEqual(default(IonSexp), payload2.IonType);

            // annotations
            var ionValue = (IonSexp)IIonType.NullOf(
                IonTypes.Sexp,
                IIonType.Annotation.ParseCollection("abc::'xyz'"));
            memory = new MemoryStream();
            payload = new IonSexpPayload(ionValue);
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memory.Position = 0;
            payload2 = IonSexpPayload.Read(
                memory,
                TypeMetadata.ReadMetadata(memory),
                options,
                 new SymbolHashList());
            Assert.AreEqual(ionValue, payload2.IonType);
            #endregion

            #region value
            ionValue = new IonSexp.Initializer
            {
                455,
                new IonOperator(Operators.Plus),
                "other stuff"
            };
            memory = new MemoryStream();
            payload = new IonSexpPayload(ionValue);
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memory.Position = 0;
            payload2 = IonSexpPayload.Read(
                memory,
                TypeMetadata.ReadMetadata(memory),
                options,
                 new SymbolHashList());
            Assert.AreEqual(ionValue, payload2.IonType);

            // annotations
            ionValue = new IonSexp.Initializer(IIonType.Annotation.ParseCollection("abc::'xyz'"))
            {
                455,
                new IonOperator(Operators.Plus),
                "other stuff"
            };
            memory = new MemoryStream();
            payload = new IonSexpPayload(ionValue);
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memory.Position = 0;
            payload2 = IonSexpPayload.Read(
                memory,
                TypeMetadata.ReadMetadata(memory),
                options,
                 new SymbolHashList());
            Assert.AreEqual(ionValue, payload2.IonType);
            #endregion
        }
    }
}
