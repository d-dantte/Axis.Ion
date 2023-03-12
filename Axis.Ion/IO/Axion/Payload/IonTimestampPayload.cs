using Axis.Ion.Types;
using Axis.Ion.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Axis.Ion.IO.Axion.Payload
{
    public readonly struct IonTimestampPayload : ITypePayload
    {
        public const byte DayMask = 1;
        public const byte HMSMask = 2;
        public const byte TicksMask = 4;

        public IonTimestampPayload(IonTimestamp timestamp)
        {
            IonType = timestamp;
            Metadata = TypeMetadata.SerializeMetadata(timestamp);
        }

        #region Read
        public static bool TryRead(
            TypeMetadata metadata,
            Stream stream,
            SerializerOptions options,
            SymbolHashList symbolTable,
            out IonTimestampPayload payload)
        {
            try
            {
                payload = Read(stream, metadata, options, symbolTable);
                return true;
            }
            catch
            {
                payload = default;
                return false;
            }
        }

        public static IonTimestampPayload Read(
            Stream stream,
            TypeMetadata metadata,
            SerializerOptions options,
            SymbolHashList symbolTable)
        {
            if (metadata.IonType != IonTypes.Timestamp)
                throw new ArgumentException($"Invalid type-metadata: {metadata.IonType}. Expected type is: {IonTypes.Timestamp}");

            // annotations
            var annotations = metadata.HasAnnotations
                ? TypeMetadata.ReadAnnotations(stream, options, symbolTable)
                : Array.Empty<IIonType.Annotation>();

            // null?
            if (metadata.IsNull)
                return new IonTimestampPayload((IonTimestamp)IIonType.NullOf(metadata.IonType, annotations));

            // non-null
            else
            {
                // component
                if (!stream.TryReadByte(out var componentByte))
                    throw new EndOfStreamException();

                // year
                if (!stream.TryReadVarByteInteger(out var year))
                    throw new EndOfStreamException();

                // month
                if (!stream.TryReadByte(out var month))
                    throw new EndOfStreamException();

                // day?
                byte dayByte = 1;
                if (componentByte.IsSet(0)
                    && !stream.TryReadByte(out dayByte))
                    throw new EndOfStreamException();

                // HMS?
                byte[] hmsBytes;
                if (componentByte.IsSet(1))
                {
                    if (!stream.TryReadExactBytes(2, out var bytes))
                        throw new EndOfStreamException();

                    hmsBytes = bytes
                        .Append((byte)(componentByte.IsSet(3) ? 1 : 0))
                        .Append((byte)0)
                        .ToArray();
                }

                else hmsBytes = new byte[4];

                // TickSeconds?
                byte[] tickSecondBytes;
                if (componentByte.IsSet(2))
                {
                    if (!stream.TryReadExactBytes(2, out var bytes))
                        throw new EndOfStreamException();

                    tickSecondBytes = bytes
                        .Append((byte)(componentByte >> 4))
                        .Append((byte)0)
                        .ToArray();
                }

                else tickSecondBytes = new byte[4];

                var hms = TimeSpan.FromSeconds(BitConverter.ToInt32(hmsBytes));
                var timestamp = new DateTimeOffset(
                    (int)year,
                    month,
                    dayByte,
                    hms.Hours,
                    hms.Minutes,
                    hms.Seconds,
                    TimeSpan.FromSeconds(0))
                    + TimeSpan.FromTicks(BitConverter.ToInt32(tickSecondBytes));

                return new IonTimestampPayload(new IonTimestamp(timestamp, annotations));
            }
        }
        #endregion

        #region ITypePayload

        public TypeMetadata Metadata { get; }

        public IIonType IonType { get; }

        public byte[] SerializeData(
            SerializerOptions options,
            SymbolHashList hashList)
        {
            var ionTimestamp = (IonTimestamp)IonType;

            if (ionTimestamp.IsNull)
                return Array.Empty<byte>();

            var timestamp = ionTimestamp.Value ?? throw new InvalidOperationException("ion timestamp should not be null");

            var bytes = new List<byte>
            {
                0 // place holder for component byte
            };

            var componentByte = 0
                | (timestamp.Day > 1 ? DayMask : 0)
                | (timestamp.HMS() > 0 ? HMSMask : 0)
                | (timestamp.TickSeconds() > 0 ? TicksMask : 0);

            bytes.AddRange(timestamp.Year.ToVarBytes());
            bytes.Add((byte)timestamp.Month);

            // day?
            if (componentByte.IsSet(0))
                bytes.Add((byte)timestamp.Day);

            // hms?
            if (componentByte.IsSet(1))
            {
                var hmsBytes = BitConverter.GetBytes(timestamp.HMS());

                // copy bit 17 unto the component byte
                componentByte |= hmsBytes[2] << 3;

                bytes.AddRange(hmsBytes[..2]);
            }

            // ticks?
            if (componentByte.IsSet(2))
            {
                var ticksecondsBytes = BitConverter.GetBytes(timestamp.TickSeconds());

                // copy bit 17 - 20 unto the componentByte
                componentByte |= ticksecondsBytes[2] << 4;

                bytes.AddRange(ticksecondsBytes[..2]);
            }

            // component byte
            bytes[0] = (byte)componentByte;

            return bytes.ToArray();
        }

        #endregion
    }
}
