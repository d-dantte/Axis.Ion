﻿using Axis.Ion.IO.Axion;
using Axis.Ion.IO.Axion.Payload;
using Axis.Ion.Types;
using Axis.Ion.Utils;
using Axis.Luna.Common.Results;

namespace Axis.Ion.Tests.IO.Binary
{
    [TestClass]
    public class IonListPayloadTests
    {
        [TestMethod]
        public void Write()
        {
            var options = new SerializerOptions();

            #region default
            var memory = new MemoryStream();
            var payload = new IonListPayload(IonList.Null());
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            var memoryArray = memory.ToArray();
            Assert.AreEqual(1, memoryArray.Length);
            Assert.AreEqual(44, memoryArray[0]);

            // annotations
            memory = new MemoryStream();
            payload = new IonListPayload(
                (IonList)IIonValue.NullOf(
                    IonTypes.List,
                    IIonValue.Annotation.ParseCollection("abc::'xyz'").Resolve()));
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memoryArray = memory.ToArray();
            Assert.AreEqual(18, memoryArray.Length);
            Assert.AreEqual(60, memoryArray[0]);
            #endregion

            #region value
            memory = new MemoryStream();
            payload = new IonListPayload(new IonList.Initializer
            {
                455,
                2.5m,
                new IonBool(true),
                "other stuff"
            });
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memoryArray = memory.ToArray();
            Assert.AreEqual(47, memoryArray.Length);
            Assert.AreEqual(12, memoryArray[0]);

            // annotations
            memory = new MemoryStream();
            payload = new IonListPayload(
                new IonList.Initializer(IIonValue.Annotation.ParseCollection("abc::'xyz'").Resolve())
                {
                    455,
                    2.5m,
                    "other stuff"
                });
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memoryArray = memory.ToArray();
            Assert.AreEqual(63, memoryArray.Length);
            Assert.AreEqual(28, memoryArray[0]);
            #endregion
        }

        [TestMethod]
        public void Read()
        {
            var options = new SerializerOptions();

            #region default
            var memory = new MemoryStream();
            var payload = new IonListPayload(IonList.Null());
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memory.Position = 0;
            var payload2 = IonListPayload.Read(
                memory,
                TypeMetadata.ReadMetadata(memory),
                options,
                 new SymbolHashList());
            Assert.AreEqual(IonList.Null(), payload2.IonValue);

            // annotations
            var ionValue = (IonList)IIonValue.NullOf(
                IonTypes.List,
                IIonValue.Annotation.ParseCollection("abc::'xyz'").Resolve());
            memory = new MemoryStream();
            payload = new IonListPayload(ionValue);
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memory.Position = 0;
            payload2 = IonListPayload.Read(
                memory,
                TypeMetadata.ReadMetadata(memory),
                options,
                 new SymbolHashList());
            Assert.AreEqual(ionValue, payload2.IonValue);
            #endregion

            #region value
            ionValue = new IonList.Initializer
            {
                455,
                2.5m,
                "other stuff"
            };
            memory = new MemoryStream();
            payload = new IonListPayload(ionValue);
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memory.Position = 0;
            payload2 = IonListPayload.Read(
                memory,
                TypeMetadata.ReadMetadata(memory),
                options,
                 new SymbolHashList());
            Assert.AreEqual(ionValue, payload2.IonValue);

            // annotations
            ionValue = new IonList.Initializer(IIonValue.Annotation.ParseCollection("abc::'xyz'").Resolve())
            {
                455,
                2.5m,
                "other stuff"
            };
            memory = new MemoryStream();
            payload = new IonListPayload(ionValue);
            ITypePayload.Write(memory, payload, options, new SymbolHashList());
            memory.Position = 0;
            payload2 = IonListPayload.Read(
                memory,
                TypeMetadata.ReadMetadata(memory),
                options,
                 new SymbolHashList());
            Assert.AreEqual(ionValue, payload2.IonValue);
            #endregion
        }
    }
}
