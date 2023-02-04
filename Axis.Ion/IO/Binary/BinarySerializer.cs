using Axis.Ion.IO.Binary.Payload;
using Axis.Ion.Types;
using Axis.Ion.Utils;
using Axis.Luna.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Axis.Ion.IO.Binary
{
    public class BinarySerializer : IIonSerializer
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
            IIonType ionValue,
            SerializerOptions options,
            SymbolHashList symbolTable)
        {
            ITypePayload payload = ionValue.Type switch
            {
                IonTypes.Null => new IonNullPayload((IonNull)ionValue),
                IonTypes.Bool => new IonBoolPayload((IonBool)ionValue),
                IonTypes.Int => new IonIntPayload((IonInt)ionValue),
                IonTypes.Decimal => new IonDecimalPayload((IonDecimal)ionValue),
                IonTypes.Float => new IonFloatPayload((IonFloat)ionValue),
                IonTypes.Timestamp => new IonTimestampPayload((IonTimestamp)ionValue),
                IonTypes.String => new IonStringPayload((IonString)ionValue),
                IonTypes.OperatorSymbol => new IonSymbolPayload((IonOperator)ionValue),
                IonTypes.Blob => new IonBlobPayload((IonBlob)ionValue),
                IonTypes.Clob => new IonClobPayload((IonClob)ionValue),
                IonTypes.List => new IonListPayload((IonList)ionValue),
                IonTypes.Sexp => new IonSexpPayload((IonSexp)ionValue),
                IonTypes.Struct => new IonStructPayload((IonStruct)ionValue),

                IonTypes.IdentifierSymbol =>
                    new IonSymbolPayload(symbolTable.AddOrGetID((IonIdentifier)ionValue)),

                IonTypes.QuotedSymbol =>
                    new IonSymbolPayload(symbolTable.AddOrGetID((IonQuotedSymbol)ionValue)),

                _ =>  throw new ArgumentException($"Invalid ion-type: {ionValue.Type}")
            };

            var memory = new MemoryStream();
            ITypePayload.Write(memory, payload, options, symbolTable);
            return memory.ToArray();
        }

        internal static IIonType? DeserializeIon(
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
                    .IonType,

                IonTypes.Bool => IonBoolPayload
                    .Read(ionStream, metadata, options, symbolTable)
                    .IonType,

                IonTypes.Int => IonIntPayload
                    .Read(ionStream, metadata, options, symbolTable)
                    .IonType,

                IonTypes.Decimal => IonDecimalPayload
                    .Read(ionStream, metadata, options, symbolTable)
                    .IonType,

                IonTypes.Float => IonFloatPayload
                    .Read(ionStream, metadata, options, symbolTable)
                    .IonType,

                IonTypes.Timestamp => IonTimestampPayload
                    .Read(ionStream, metadata, options, symbolTable)
                    .IonType,

                IonTypes.String => IonStringPayload
                    .Read(ionStream, metadata, options, symbolTable)
                    .IonType,

                IonTypes.Clob => IonClobPayload
                    .Read(ionStream, metadata, options, symbolTable)
                    .IonType,

                IonTypes.Blob => IonBlobPayload
                    .Read(ionStream, metadata, options, symbolTable)
                    .IonType,

                IonTypes.List => IonListPayload
                    .Read(ionStream, metadata, options, symbolTable)
                    .IonType,

                IonTypes.Sexp => IonSexpPayload
                    .Read(ionStream, metadata, options, symbolTable)
                    .IonType,

                IonTypes.Struct => IonStructPayload
                    .Read(ionStream, metadata, options, symbolTable)
                    .IonType,

                IonTypes.OperatorSymbol => IonSymbolPayload
                    .Read(ionStream, metadata, options, symbolTable)
                    .IonType,

                IonTypes.IdentifierSymbol => IonSymbolPayload
                    .Read(ionStream, metadata, options, symbolTable)
                    .IonType,

                IonTypes.QuotedSymbol => IonSymbolPayload
                    .Read(ionStream, metadata, options, symbolTable)
                    .IonType,

                _ => throw new InvalidOperationException($"Invalid metadata ion-type: {metadata.IonType}")
            };
        }

        private IEnumerable<IIonType> ReadIonValues(Stream stream)
        {
            var symbolTable = new SymbolHashList();
            IIonType? value;
            while ((value = DeserializeIon(stream, options, symbolTable)) != null)
            {
                yield return value;
            }
        }
    }
}
