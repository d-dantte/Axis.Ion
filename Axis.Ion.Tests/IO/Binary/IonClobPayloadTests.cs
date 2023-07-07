using Axis.Ion.IO.Axion;
using Axis.Ion.IO.Axion.Payload;
using Axis.Ion.Types;
using Axis.Ion.Utils;
using System.Text;
using Axis.Luna.Common.Results;

namespace Axis.Ion.Tests.IO.Binary
{
    [TestClass]
    public class IonClobPayloadTests
    {
        [TestMethod]
        public void Write()
        {
            var options = new SerializerOptions();

            #region default
            var memory = new MemoryStream();
            var payload = new IonClobPayload(default);
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            var memoryArray = memory.ToArray();
            Assert.AreEqual(1, memoryArray.Length);
            Assert.AreEqual(43, memoryArray[0]);

            // annotations
            memory = new MemoryStream();
            payload = new IonClobPayload(
                (IonClob)IIonValue.NullOf(
                    IonTypes.Clob,
                    IIonValue.Annotation.ParseCollection("abc::'xyz'").Resolve()));
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memoryArray = memory.ToArray();
            Assert.AreEqual(18, memoryArray.Length);
            Assert.AreEqual(59, memoryArray[0]);
            #endregion

            #region value
            memory = new MemoryStream();
            payload = new IonClobPayload(new IonClob(Encoding.ASCII.GetBytes("lorem ipsum")));
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memoryArray = memory.ToArray();
            Assert.AreEqual(13, memoryArray.Length);
            Assert.AreEqual(11, memoryArray[0]);

            // annotations
            memory = new MemoryStream();
            payload = new IonClobPayload(
                new IonClob(
                    Encoding.ASCII.GetBytes("lorem ipsum"),
                    IIonValue.Annotation.ParseCollection("abc::'xyz'").Resolve()));
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memoryArray = memory.ToArray();
            Assert.AreEqual(30, memoryArray.Length);
            Assert.AreEqual(27, memoryArray[0]);
            #endregion
        }

        [TestMethod]
        public void Read()
        {
            var options = new SerializerOptions();

            #region default
            var memory = new MemoryStream();
            var payload = new IonClobPayload(default);
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memory.Position = 0;
            var payload2 = IonClobPayload.Read(
                memory,
                TypeMetadata.ReadMetadata(memory),
                options,
                 new SymbolHashList());
            Assert.AreEqual(default(IonClob), payload2.IonValue);

            // annotations
            var ionValue = (IonClob)IIonValue.NullOf(
                IonTypes.Clob,
                IIonValue.Annotation.ParseCollection("abc::'xyz'").Resolve());
            memory = new MemoryStream();
            payload = new IonClobPayload(ionValue);
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memory.Position = 0;
            payload2 = IonClobPayload.Read(
                memory,
                TypeMetadata.ReadMetadata(memory),
                options,
                 new SymbolHashList());
            Assert.AreEqual(ionValue, payload2.IonValue);
            #endregion

            #region value
            ionValue = new IonClob(Encoding.ASCII.GetBytes("lorem ipsum"));
            memory = new MemoryStream();
            payload = new IonClobPayload(ionValue);
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memory.Position = 0;
            payload2 = IonClobPayload.Read(
                memory,
                TypeMetadata.ReadMetadata(memory),
                options,
                 new SymbolHashList());
            Assert.AreEqual(ionValue, payload2.IonValue);

            // annotations
            ionValue = new IonClob(
                Encoding.ASCII.GetBytes("lorem ipsum"),
                IIonValue.Annotation.ParseCollection("abc::'xyz'").Resolve());
            memory = new MemoryStream();
            payload = new IonClobPayload(ionValue);
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memory.Position = 0;
            payload2 = IonClobPayload.Read(
                memory,
                TypeMetadata.ReadMetadata(memory),
                options,
                 new SymbolHashList());
            Assert.AreEqual(ionValue, payload2.IonValue);
            #endregion
        }
    }
}
