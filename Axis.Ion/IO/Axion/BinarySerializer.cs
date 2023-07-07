using Axis.Ion.IO.Axion.Payload;
using Axis.Ion.Types;
using Axis.Ion.Utils;
using Axis.Luna.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static Axis.Ion.IO.Axion.Payload.IonSymbolPayload;

namespace Axis.Ion.IO.Axion
{
    public class BinarySerializer : IIonBinarySerializer
    {
        private readonly SerializerOptions options;

        public BinarySerializer(SerializerOptions? options = null)
        {
            this.options = options ?? new SerializerOptions();
        }

        public IonPacket Deserialize(Stream ionStream)
        {
            var symbolTable = new SymbolHashList();
            return ReadIonValues(ionStream)
                .ToArray()
                .ApplyTo(values => new IonPacket(values));
        }

        public byte[] Serialize(IonPacket ionPacket)
        {
            var symbolTable = new SymbolHashList();
            return ionPacket.IonValues
                .SelectMany(value => SerializeIon(value, options, symbolTable))
                .ToArray();
        }

        internal static byte[] SerializeIon(
            IIonValue ionValue,
            SerializerOptions options,
            SymbolHashList symbolTable)
        {
            ITypePayload payload = ionValue switch
            {
                IonNull @null => new IonNullPayload(@null),
                IonBool @bool => new IonBoolPayload(@bool),
                IonInt @int => new IonIntPayload(@int),
                IonDecimal @decimal => new IonDecimalPayload(@decimal),
                IonFloat @float => new IonFloatPayload(@float),
                IonTimestamp timestamp => new IonTimestampPayload(timestamp),
                IonString @string => new IonStringPayload(@string),
                IonOperator @operator => new IonSymbolPayload(@operator),
                IonBlob blob => new IonBlobPayload(blob),
                IonClob clob => new IonClobPayload(clob),
                IonList list => new IonListPayload(list),
                IonSexp sexp => new IonSexpPayload(sexp),
                IonStruct @struct => new IonStructPayload(@struct),

                IonTextSymbol symbol => symbolTable.TryAdd(symbol, out var index)
                    ? new IonSymbolPayload(symbol)
                    : new IonSymbolPayload(new IonSymbolID(index)),

                _ =>  throw new ArgumentException($"Invalid ion-type: {ionValue.Type}")
            };

            var memory = new MemoryStream();
            ITypePayload.Write(memory, payload, options, symbolTable);
            return memory.ToArray();
        }

        internal static IIonValue? DeserializeIon(
            Stream ionStream,
            SerializerOptions options,
            SymbolHashList symbolTable)
        {
            var metadataByte = ionStream.ReadByte();

            if (metadataByte < 0)
                return null; // EOF

            var metadata = new TypeMetadata((byte)metadataByte);

            return metadata.IonType switch
            {
                IonTypes.Null => IonNullPayload
                    .Read(ionStream, metadata, options, symbolTable)
                    .IonValue,

                IonTypes.Bool => IonBoolPayload
                    .Read(ionStream, metadata, options, symbolTable)
                    .IonValue,

                IonTypes.Int => IonIntPayload
                    .Read(ionStream, metadata, options, symbolTable)
                    .IonValue,

                IonTypes.Decimal => IonDecimalPayload
                    .Read(ionStream, metadata, options, symbolTable)
                    .IonValue,

                IonTypes.Float => IonFloatPayload
                    .Read(ionStream, metadata, options, symbolTable)
                    .IonValue,

                IonTypes.Timestamp => IonTimestampPayload
                    .Read(ionStream, metadata, options, symbolTable)
                    .IonValue,

                IonTypes.String => IonStringPayload
                    .Read(ionStream, metadata, options, symbolTable)
                    .IonValue,

                IonTypes.Clob => IonClobPayload
                    .Read(ionStream, metadata, options, symbolTable)
                    .IonValue,

                IonTypes.Blob => IonBlobPayload
                    .Read(ionStream, metadata, options, symbolTable)
                    .IonValue,

                IonTypes.List => IonListPayload
                    .Read(ionStream, metadata, options, symbolTable)
                    .IonValue,

                IonTypes.Sexp => IonSexpPayload
                    .Read(ionStream, metadata, options, symbolTable)
                    .IonValue,

                IonTypes.Struct => IonStructPayload
                    .Read(ionStream, metadata, options, symbolTable)
                    .IonValue,

                IonTypes.OperatorSymbol => IonSymbolPayload
                    .Read(ionStream, metadata, options, symbolTable)
                    .IonValue,

                IonTypes.TextSymbol => IonSymbolPayload
                    .Read(ionStream, metadata, options, symbolTable)
                    .IonValue,

                _ => throw new InvalidOperationException($"Invalid metadata ion-type: {metadata.IonType}")
            };
        }

        private IEnumerable<IIonValue> ReadIonValues(Stream stream)
        {
            var symbolTable = new SymbolHashList();
            IIonValue? value;
            while ((value = DeserializeIon(stream, options, symbolTable)) != null)
            {
                yield return value;
            }
        }
    }
}
