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
    public class IonNullPayloadTests
    {

        [TestMethod]
        public void Write()
        {
            var memory = new MemoryStream();
            var payload = new IonNullPayload(default);

            payload.Write(memory);
            var memoryArray = memory.ToArray();
            Assert.AreEqual(1, memoryArray.Length);
            Assert.AreEqual(33, memoryArray[0]);


            payload = new IonNullPayload(new IonNull("fenrir", "jotun", "'bilerofon'"));
            memory = new MemoryStream();
            payload.Write(memory);
            memoryArray = memory.ToArray();
            Assert.AreEqual(52, memoryArray.Length);
            Assert.AreEqual(49, memoryArray[0]);
        }

        [TestMethod]
        public void Read()
        {
            var memory = new MemoryStream();
            var payload = new IonNullPayload(default);
            payload.Write(memory);
            memory.Position = 0;

            var payload2 = IonNullPayload.Read(payload.Metadata, memory);
            Assert.AreEqual(33, payload2.Metadata.Metadata);


            memory = new MemoryStream();
            payload = new IonNullPayload(new IonNull("fenrir", "jotun", "'bilerofon'"));
            payload.Write(memory);
            memory.Position = 1;

            payload2 = IonNullPayload.Read(payload.Metadata, memory);
            Assert.AreEqual(49, payload2.Metadata.Metadata);
            Assert.AreEqual(3, payload2.IonValue.Annotations.Length);
            Assert.IsTrue(
                new[] { "fenrir", "jotun", "'bilerofon'" }
                .SequenceEqual(payload2.IonValue.Annotations.Select(a => a.Value)));
        }
    }
}
