using Axis.Ion.IO.Binary;
using Axis.Ion.IO.Binary.Payload;
using Axis.Ion.Types;
using Axis.Ion.Utils;
using System.Numerics;

namespace Axis.Ion.Tests.IO.Binary
{
    [TestClass]
    public class IonIntPayloadTests
    {
        [TestMethod]
        public void Write()
        {
            var options = new SerializerOptions();

            #region default
            var memory = new MemoryStream();
            var payload = new IonIntPayload(default);
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            var memoryArray = memory.ToArray();
            Assert.AreEqual(1, memoryArray.Length);
            Assert.AreEqual(35, memoryArray[0]);

            // annotations
            memory = new MemoryStream();
            payload = new IonIntPayload(
                (IonInt)IIonType.NullOf(
                    IonTypes.Int,
                    IIonType.Annotation.ParseCollection("abc::'xyz'")));
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memoryArray = memory.ToArray();
            Assert.AreEqual(18, memoryArray.Length);
            Assert.AreEqual(51, memoryArray[0]);
            #endregion

            #region int8
            memory = new MemoryStream();
            payload = new IonIntPayload(new IonInt(sbyte.MaxValue));
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memoryArray = memory.ToArray();
            Assert.AreEqual(2, memoryArray.Length);
            Assert.AreEqual(3, memoryArray[0]);

            // annotations
            memory = new MemoryStream();
            payload = new IonIntPayload(
                new IonInt(
                    sbyte.MaxValue,
                    IIonType.Annotation.ParseCollection("abc::'xyz'")));
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memoryArray = memory.ToArray();
            Assert.AreEqual(19, memoryArray.Length);
            Assert.AreEqual(19, memoryArray[0]);
            #endregion

            #region int16
            memory = new MemoryStream();
            payload = new IonIntPayload(new IonInt(short.MaxValue));
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memoryArray = memory.ToArray();
            Assert.AreEqual(3, memoryArray.Length);
            Assert.AreEqual(67, memoryArray[0]);

            // annotations
            memory = new MemoryStream();
            payload = new IonIntPayload(
                new IonInt(
                    short.MaxValue,
                    IIonType.Annotation.ParseCollection("abc::'xyz'")));
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memoryArray = memory.ToArray();
            Assert.AreEqual(20, memoryArray.Length);
            Assert.AreEqual(83, memoryArray[0]);
            #endregion

            #region int32
            memory = new MemoryStream();
            payload = new IonIntPayload(new IonInt(16777215));
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memoryArray = memory.ToArray();
            Assert.AreEqual(5, memoryArray.Length);
            Assert.AreEqual(131, memoryArray[0]);

            // annotations
            memory = new MemoryStream();
            payload = new IonIntPayload(
                new IonInt(
                    int.MaxValue,
                    IIonType.Annotation.ParseCollection("abc::'xyz'")));
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memoryArray = memory.ToArray();
            Assert.AreEqual(22, memoryArray.Length);
            Assert.AreEqual(147, memoryArray[0]);
            #endregion

            #region intUnlimited
            memory = new MemoryStream();
            payload = new IonIntPayload(new IonInt(BigInteger.Parse("1461501637330902918203684832716283019655932542975"))); //20 byte integer
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memoryArray = memory.ToArray();
            Assert.AreEqual(24, memoryArray.Length);
            Assert.AreEqual(195, memoryArray[0]);

            // annotations
            memory = new MemoryStream();
            payload = new IonIntPayload(
                new IonInt(
                    BigInteger.Parse("1461501637330902918203684832716283019655932542975"),
                    IIonType.Annotation.ParseCollection("abc::'xyz'")));
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memoryArray = memory.ToArray();
            Assert.AreEqual(41, memoryArray.Length);
            Assert.AreEqual(211, memoryArray[0]);
            #endregion
        }

        [TestMethod]
        public void Read()
        {
            var options = new SerializerOptions();

            #region default
            var memory = new MemoryStream();
            var payload = new IonIntPayload(default);
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            var memoryArray = memory.ToArray();
            memory.Position = 0;
            var payload2 = IonIntPayload.Read(
                memory,
                TypeMetadata.ReadMetadata(memory),
                options,
                new SymbolHashList());
            Assert.AreEqual(default(IonInt), payload2.IonType);

            // annotations
            var ionValue = (IonInt)IIonType.NullOf(
                IonTypes.Int,
                IIonType.Annotation.ParseCollection("abc::'xyz'"));
            memory = new MemoryStream();
            payload = new IonIntPayload(ionValue);
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memoryArray = memory.ToArray();
            memory.Position = 0;
            payload2 = IonIntPayload.Read(
                memory,
                TypeMetadata.ReadMetadata(memory),
                options,
                new SymbolHashList());
            Assert.AreEqual(ionValue, payload2.IonType);
            #endregion

            #region int8
            ionValue = new IonInt(sbyte.MaxValue);
            memory = new MemoryStream();
            payload = new IonIntPayload(ionValue);
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memoryArray = memory.ToArray();
            memory.Position = 0;
            payload2 = IonIntPayload.Read(
                memory,
                TypeMetadata.ReadMetadata(memory),
                options,
                new SymbolHashList());
            Assert.AreEqual(ionValue, payload2.IonType);

            // annotations
            ionValue = new IonInt(
                sbyte.MaxValue,
                IIonType.Annotation.ParseCollection("abc::'xyz'"));
            memory = new MemoryStream();
            payload = new IonIntPayload(ionValue);
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memoryArray = memory.ToArray();
            memory.Position = 0;
            payload2 = IonIntPayload.Read(
                memory,
                TypeMetadata.ReadMetadata(memory),
                options,
                new SymbolHashList());
            Assert.AreEqual(ionValue, payload2.IonType);
            #endregion

            #region int16
            ionValue = new IonInt(short.MaxValue);
            memory = new MemoryStream();
            payload = new IonIntPayload(ionValue);
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memoryArray = memory.ToArray();
            memory.Position = 0;
            payload2 = IonIntPayload.Read(
                memory,
                TypeMetadata.ReadMetadata(memory),
                options,
                new SymbolHashList());
            Assert.AreEqual(ionValue, payload2.IonType);

            // annotations
            ionValue = new IonInt(
                short.MaxValue,
                IIonType.Annotation.ParseCollection("abc::'xyz'"));
            memory = new MemoryStream();
            payload = new IonIntPayload(ionValue);
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memoryArray = memory.ToArray();
            memory.Position = 0;
            payload2 = IonIntPayload.Read(
                memory,
                TypeMetadata.ReadMetadata(memory),
                options,
                new SymbolHashList());
            Assert.AreEqual(ionValue, payload2.IonType);
            #endregion

            #region int32
            ionValue = new IonInt(int.MaxValue);
            memory = new MemoryStream();
            payload = new IonIntPayload(ionValue);
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memoryArray = memory.ToArray();
            memory.Position = 0;
            payload2 = IonIntPayload.Read(
                memory,
                TypeMetadata.ReadMetadata(memory),
                options,
                new SymbolHashList());
            Assert.AreEqual(ionValue, payload2.IonType);

            // annotations
            ionValue = new IonInt(
                int.MaxValue,
                IIonType.Annotation.ParseCollection("abc::'xyz'"));
            memory = new MemoryStream();
            payload = new IonIntPayload(ionValue);
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memoryArray = memory.ToArray();
            memory.Position = 0;
            payload2 = IonIntPayload.Read(
                memory,
                TypeMetadata.ReadMetadata(memory),
                options,
                new SymbolHashList());
            Assert.AreEqual(ionValue, payload2.IonType);
            #endregion

            #region intUnlimited
            ionValue = new IonInt(BigInteger.Parse("1461501637330902918203684832716283019655932542975"));
            memory = new MemoryStream();
            payload = new IonIntPayload(ionValue);
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memoryArray = memory.ToArray();
            memory.Position = 0;
            payload2 = IonIntPayload.Read(
                memory,
                TypeMetadata.ReadMetadata(memory),
                options,
                new SymbolHashList());
            Assert.AreEqual(ionValue, payload2.IonType);

            // annotations
            ionValue = new IonInt(
                BigInteger.Parse("1461501637330902918203684832716283019655932542975"),
                IIonType.Annotation.ParseCollection("abc::'xyz'"));
            memory = new MemoryStream();
            payload = new IonIntPayload(ionValue);
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memoryArray = memory.ToArray();
            memory.Position = 0;
            payload2 = IonIntPayload.Read(
                memory,
                TypeMetadata.ReadMetadata(memory),
                options,
                new SymbolHashList());
            Assert.AreEqual(ionValue, payload2.IonType);
            #endregion
        }
    }
}
