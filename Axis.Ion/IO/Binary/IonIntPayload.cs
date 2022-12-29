using Axis.Ion.Types;
using Axis.Luna.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;

namespace Axis.Ion.IO.Binary
{
    public readonly struct IonIntPayload : IPayloadWriter
    {
        public const byte Int8Mask = 63;
        public const byte Int16Mask = 64;
        public const byte Int32Mask = 128;
        public const byte IntUnlimitedMask = 192;

        public const byte Int8Value = 0;
        public const byte Int16Value = 1;
        public const byte Int32Value = 2;
        public const byte IntUnlimitedValue = 3;

        public TypeMetadata Metadata { get; }

        public IIonType IonValue { get; }

        public IonIntPayload(IonInt @int)
        {
            IonValue = @int;
            Metadata = ToIntTypeMetadata(@int);
        }

        #region Write

        public void Write(Stream outputStream)
        {
            var annotations = TypeMetadata.ToAnnotationData(IonValue);
            outputStream.WriteByte(Metadata.Metadata);
            outputStream.Write(annotations);
            outputStream.Write(SerializeData());
            outputStream.Flush();
        }

        public async Task WriteAsync(Stream outputStream)
        {
            var annotations = TypeMetadata.ToAnnotationData(IonValue);
            outputStream.WriteByte(Metadata.Metadata);
            await outputStream.WriteAsync(annotations);
            await outputStream.WriteAsync(SerializeData());
            await outputStream.FlushAsync();
        }
        #endregion

        private byte[] SerializeData()
        {
            if (IonValue.IsNull)
                return Array.Empty<byte>();

            var data = new List<byte>();
            var ionInt = (IonInt) IonValue;
            var intBytes = ionInt.Value?
                .ToByteArray()
                ?? throw new InvalidOperationException($"Invalid {typeof(IonInt)} value");

            if (intBytes.Length <= 0)
                throw new InvalidOperationException($"Invalid byte-count for {typeof(IonInt)} value: {intBytes.Length}");

            if (intBytes.Length > 4)
                data.AddRange(intBytes.Length.ToVarBytes());

            data.AddRange(intBytes.Length switch
            {
                3 => intBytes.Concat((byte)0),
                _ => intBytes
            });

            return data.ToArray();
        }

        private static byte ToIntTypeMetadata(IonInt @int)
        {
            var metadata = TypeMetadata.ToMetadataByte(@int);
            return @int.Value?.GetByteCount() switch
            {
                null => metadata,
                1 => (byte)(metadata & Int8Mask),
                2 => (byte)(metadata | Int16Mask),
                3 or 4 => (byte)(metadata | Int32Mask),
                > 4 => (byte)(metadata | IntUnlimitedMask),
                _ => throw new InvalidOperationException($"Invalid int byte-count: {@int.Value?.GetByteCount()}")
            };
        }
    }
}
