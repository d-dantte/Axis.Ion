using Axis.Ion.IO.Binary;
using Axis.Ion.IO.Binary.Payload;
using Axis.Ion.Types;
using Axis.Ion.Utils;

namespace Axis.Ion.Tests.IO.Binary
{
    [TestClass]
    public class IonTimestampPayloadTests
    {
        [TestMethod]
        public void Write()
        {
            var options = new SerializerOptions();

            #region default
            var memory = new MemoryStream();
            var payload = new IonTimestampPayload(default(IonTimestamp));
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            var memoryArray = memory.ToArray();
            Assert.AreEqual(1, memoryArray.Length);
            Assert.AreEqual(38, memoryArray[0]);
            #endregion

            #region year + month
            memory = new MemoryStream();
            payload = new IonTimestampPayload(
                new IonTimestamp(
                    new DateTimeOffset(
                        2005,
                        9,
                        1,
                        0,
                        0,
                        0,
                        TimeSpan.FromSeconds(0))));
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memoryArray = memory.ToArray();
            Assert.AreEqual(5, memoryArray.Length);
            Assert.AreEqual(6, memoryArray[0]);
            Assert.AreEqual(0, memoryArray[1]);
            #endregion

            #region year + month + day
            memory = new MemoryStream();
            payload = new IonTimestampPayload(
                new IonTimestamp(
                    new DateTimeOffset(
                        2005,
                        9,
                        14,
                        0,
                        0,
                        0,
                        TimeSpan.FromSeconds(0))));
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memoryArray = memory.ToArray();
            Assert.AreEqual(6, memoryArray.Length);
            Assert.AreEqual(6, memoryArray[0]);
            Assert.AreEqual(1, memoryArray[1]);
            #endregion

            #region year + month + day + hms
            memory = new MemoryStream();
            payload = new IonTimestampPayload(
                new IonTimestamp(
                    new DateTimeOffset(
                        2005,
                        9,
                        14,
                        22,
                        36,
                        21,
                        TimeSpan.FromSeconds(0))));
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memoryArray = memory.ToArray();
            Assert.AreEqual(8, memoryArray.Length);
            Assert.AreEqual(6, memoryArray[0]);
            Assert.AreEqual(11, memoryArray[1]);
            #endregion

            #region year + month + day + hms + ticks
            memory = new MemoryStream();
            payload = new IonTimestampPayload(
                new IonTimestamp(
                    new DateTimeOffset(
                        2005,
                        9,
                        14,
                        22,
                        36,
                        21,
                        TimeSpan.FromSeconds(0))
                    + TimeSpan.FromTicks(242_311)));
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memoryArray = memory.ToArray();
            Assert.AreEqual(10, memoryArray.Length);
            Assert.AreEqual(6, memoryArray[0]);
            Assert.AreEqual(63, memoryArray[1]);
            #endregion
        }

        [TestMethod]
        public void Read()
        {
            var options = new SerializerOptions();

            #region default
            var memory = new MemoryStream();
            var payload = new IonTimestampPayload(default);
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            var memoryArray = memory.ToArray();
            memory.Position = 0;
            var payload2 = IonTimestampPayload.Read(
                memory,
                TypeMetadata.ReadMetadata(memory),
                options,
                new SymbolHashList());

            Assert.AreEqual(default(IonTimestamp), payload2.IonType);
            #endregion

            #region year + month
            var ionValue = new IonTimestamp(
                new DateTimeOffset(
                    2005,
                    9,
                    1,
                    0,
                    0,
                    0,
                    TimeSpan.FromSeconds(0)));
            memory = new MemoryStream();
            payload = new IonTimestampPayload(ionValue);
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memoryArray = memory.ToArray();
            memory.Position = 0;
            payload2 = IonTimestampPayload.Read(
                memory,
                TypeMetadata.ReadMetadata(memory),
                options,
                new SymbolHashList());
            Assert.AreEqual(ionValue, payload2.IonType);
            #endregion

            #region year + month + day
            ionValue = new IonTimestamp(
                new DateTimeOffset(
                    2005,
                    9,
                    14,
                    0,
                    0,
                    0,
                    TimeSpan.FromSeconds(0)));
            memory = new MemoryStream();
            payload = new IonTimestampPayload(ionValue);
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memoryArray = memory.ToArray();
            memory.Position = 0;
            payload2 = IonTimestampPayload.Read(
                memory,
                TypeMetadata.ReadMetadata(memory),
                options,
                new SymbolHashList());
            Assert.AreEqual(ionValue, payload2.IonType);
            #endregion

            #region year + month + day + hms
            ionValue = new IonTimestamp(
                new DateTimeOffset(
                    2005,
                    9,
                    14,
                    22,
                    36,
                    21,
                    TimeSpan.FromSeconds(0)));
            memory = new MemoryStream();
            payload = new IonTimestampPayload(ionValue);
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memoryArray = memory.ToArray();
            memory.Position = 0;
            payload2 = IonTimestampPayload.Read(
                memory,
                TypeMetadata.ReadMetadata(memory),
                options,
                new SymbolHashList());
            Assert.AreEqual(ionValue, payload2.IonType);
            #endregion

            #region year + month + day + hms + ticks
            ionValue = new IonTimestamp(
                new DateTimeOffset(
                    2005,
                    9,
                    14,
                    22,
                    36,
                    21,
                    TimeSpan.FromSeconds(0))
                    + TimeSpan.FromTicks(242_311));
            memory = new MemoryStream();
            payload = new IonTimestampPayload(ionValue);
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memoryArray = memory.ToArray();
            memory.Position = 0;
            payload2 = IonTimestampPayload.Read(
                memory,
                TypeMetadata.ReadMetadata(memory),
                options,
                new SymbolHashList());
            Assert.AreEqual(ionValue, payload2.IonType);
            #endregion
        }
    }
}
