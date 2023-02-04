using Axis.Ion.IO.Binary;
using Axis.Ion.IO.Binary.Payload;
using Axis.Ion.Types;
using Axis.Ion.Utils;

namespace Axis.Ion.Tests.IO.Binary
{
    [TestClass]
    public class IonNullPayloadTests
    {

        [TestMethod]
        public void Write()
        {
            var options = new SerializerOptions();
            var table = new SymbolHashList();

            var memory = new MemoryStream();
            var payload = new IonNullPayload(default);

            ITypePayload.Write(memory, payload, options, table);
            var memoryArray = memory.ToArray();
            Assert.AreEqual(1, memoryArray.Length);
            Assert.AreEqual(33, memoryArray[0]);


            payload = new IonNullPayload(new IonNull("fenrir", "jotun", "'bilerofon'"));
            memory = new MemoryStream();
            ITypePayload.Write(memory, payload, options, table);
            memoryArray = memory.ToArray();
            Assert.AreEqual(48, memoryArray.Length);
            Assert.AreEqual(49, memoryArray[0]);
        }

        [TestMethod]
        public void Read()
        {
            var options = new SerializerOptions();
            var table = new SymbolHashList();

            var memory = new MemoryStream();
            var payload = new IonNullPayload(default);
            ITypePayload.Write(memory, payload, options, table);
            memory.Position = 0;

            var payload2 = IonNullPayload.Read(
                memory,
                TypeMetadata.ReadMetadata(memory),
                options,
                table);
            Assert.AreEqual(33, payload2.Metadata.Metadata);


            memory = new MemoryStream();
            table = new SymbolHashList();
            payload = new IonNullPayload(new IonNull("fenrir", "jotun", "'bilerofon'"));
            ITypePayload.Write(memory, payload, options, table);

            memory.Position = 0;
            table = new SymbolHashList();
            payload2 = IonNullPayload.Read(
                memory,
                TypeMetadata.ReadMetadata(memory),
                options,
                table);
            Assert.AreEqual(49, payload2.Metadata.Metadata);
            Assert.AreEqual(3, payload2.IonType.Annotations.Length);
            Assert.IsTrue(
                new[] { "fenrir", "jotun", "'bilerofon'" }
                .SequenceEqual(payload2.IonType.Annotations.Select(a => a.Value)));
        }
    }
}
