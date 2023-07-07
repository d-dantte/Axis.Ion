using Axis.Ion.IO.Axion;
using Axis.Ion.IO.Axion.Payload;
using Axis.Ion.Types;
using Axis.Ion.Utils;
using Axis.Luna.Common.Results;

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
            var payload = new IonSexpPayload(IonSexp.Null());
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            var memoryArray = memory.ToArray();
            Assert.AreEqual(1, memoryArray.Length);
            Assert.AreEqual(45, memoryArray[0]);

            // annotations
            memory = new MemoryStream();
            payload = new IonSexpPayload(
                (IonSexp)IIonValue.NullOf(
                    IonTypes.Sexp,
                    IIonValue.Annotation.ParseCollection("abc::'xyz'").Resolve()));
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memoryArray = memory.ToArray();
            Assert.AreEqual(18, memoryArray.Length);
            Assert.AreEqual(61, memoryArray[0]);
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
            Assert.AreEqual(13, memoryArray[0]);

            // annotations
            memory = new MemoryStream();
            payload = new IonSexpPayload(
                new IonSexp.Initializer(IIonValue.Annotation.ParseCollection("abc::'xyz'").Resolve())
                {
                    455,
                    new IonOperator(Operators.Plus),
                    "other stuff"
                });
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memoryArray = memory.ToArray();
            Assert.AreEqual(48, memoryArray.Length);
            Assert.AreEqual(29, memoryArray[0]);
            #endregion
        }

        [TestMethod]
        public void Read()
        {
            var options = new SerializerOptions();

            #region default
            var memory = new MemoryStream();
            var payload = new IonSexpPayload(IonSexp.Null());
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memory.Position = 0;
            var payload2 = IonSexpPayload.Read(
                memory,
                TypeMetadata.ReadMetadata(memory),
                options,
                 new SymbolHashList());
            Assert.AreEqual(IonSexp.Null(), payload2.IonValue);

            // annotations
            var ionValue = (IonSexp)IIonValue.NullOf(
                IonTypes.Sexp,
                IIonValue.Annotation.ParseCollection("abc::'xyz'").Resolve());
            memory = new MemoryStream();
            payload = new IonSexpPayload(ionValue);
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memory.Position = 0;
            payload2 = IonSexpPayload.Read(
                memory,
                TypeMetadata.ReadMetadata(memory),
                options,
                 new SymbolHashList());
            Assert.AreEqual(ionValue, payload2.IonValue);
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
            Assert.AreEqual(ionValue, payload2.IonValue);

            // annotations
            ionValue = new IonSexp.Initializer(IIonValue.Annotation.ParseCollection("abc::'xyz'").Resolve())
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
            Assert.AreEqual(ionValue, payload2.IonValue);
            #endregion
        }
    }
}
