using Axis.Ion.IO.Binary;
using Axis.Ion.IO.Binary.Payload;
using Axis.Ion.Types;
using Axis.Ion.Utils;
using System.Text;

namespace Axis.Ion.Tests.IO.Binary
{
    [TestClass]
    public class IonBlobPayloadTests
    {
        [TestMethod]
        public void Write()
        {
            var options = new SerializerOptions();

            #region default
            var memory = new MemoryStream();
            var payload = new IonBlobPayload(default);
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            var memoryArray = memory.ToArray();
            Assert.AreEqual(1, memoryArray.Length);
            Assert.AreEqual(43, memoryArray[0]);

            // annotations
            memory = new MemoryStream();
            payload = new IonBlobPayload(
                (IonBlob)IIonType.NullOf(
                    IonTypes.Blob,
                    IIonType.Annotation.ParseCollection("abc::'xyz'")));
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memoryArray = memory.ToArray();
            Assert.AreEqual(18, memoryArray.Length);
            Assert.AreEqual(59, memoryArray[0]);
            #endregion

            #region value
            memory = new MemoryStream();
            payload = new IonBlobPayload(new IonBlob(Encoding.ASCII.GetBytes("lorem ipsum")));
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memoryArray = memory.ToArray();
            Assert.AreEqual(13, memoryArray.Length);
            Assert.AreEqual(11, memoryArray[0]);

            // annotations
            memory = new MemoryStream();
            payload = new IonBlobPayload(
                new IonBlob(
                    Encoding.ASCII.GetBytes("lorem ipsum"),
                    IIonType.Annotation.ParseCollection("abc::'xyz'")));
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
            var payload = new IonBlobPayload(default);
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memory.Position = 0;
            var payload2 = IonBlobPayload.Read(
                memory,
                TypeMetadata.ReadMetadata(memory),
                options,
                 new SymbolHashList());
            Assert.AreEqual(default(IonBlob), payload2.IonType);

            // annotations
            var ionValue = (IonBlob)IIonType.NullOf(
                IonTypes.Blob,
                IIonType.Annotation.ParseCollection("abc::'xyz'"));
            memory = new MemoryStream();
            payload = new IonBlobPayload(ionValue);
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memory.Position = 0;
            payload2 = IonBlobPayload.Read(
                memory,
                TypeMetadata.ReadMetadata(memory),
                options,
                 new SymbolHashList());
            Assert.AreEqual(ionValue, payload2.IonType);
            #endregion

            #region value
            ionValue = new IonBlob(Encoding.ASCII.GetBytes("lorem ipsum"));
            memory = new MemoryStream();
            payload = new IonBlobPayload(ionValue);
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memory.Position = 0;
            payload2 = IonBlobPayload.Read(
                memory,
                TypeMetadata.ReadMetadata(memory),
                options,
                 new SymbolHashList());
            Assert.AreEqual(ionValue, payload2.IonType);

            // annotations
            ionValue = new IonBlob(
                Encoding.ASCII.GetBytes("lorem ipsum"),
                IIonType.Annotation.ParseCollection("abc::'xyz'"));
            memory = new MemoryStream();
            payload = new IonBlobPayload(ionValue);
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memory.Position = 0;
            payload2 = IonBlobPayload.Read(
                memory,
                TypeMetadata.ReadMetadata(memory),
                options,
                 new SymbolHashList());
            Assert.AreEqual(ionValue, payload2.IonType);
            #endregion
        }
    }
}
