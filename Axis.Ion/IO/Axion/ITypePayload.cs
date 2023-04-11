using Axis.Ion.IO.Axion.Payload;
using Axis.Ion.Types;
using Axis.Ion.Utils;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Axis.Ion.IO.Axion
{
    public interface ITypePayloadReader<TTypePayload> where TTypePayload : ITypePayload
    {
        static abstract bool TryRead(
            Stream stream,
            TypeMetadata metadata,
            SerializerOptions options,
            SymbolHashList symbolTable,
            out TTypePayload payload);

        static abstract TTypePayload Read(
            Stream stream,
            TypeMetadata metadata,
            SerializerOptions options,
            SymbolHashList symbolTable);
    }

    /// <summary>
    /// A structured representation of the binary data that is written/read
    /// </summary>
    public interface ITypePayload
    {
        /// <summary>
        /// The metadata component
        /// </summary>
        TypeMetadata Metadata { get; }

        /// <summary>
        /// The ion value
        /// </summary>
        IIonType IonType { get; }

        /// <summary>
        /// Serializes the data-component of the payload.
        /// </summary>
        /// <param name="options">The serializer options instance</param>
        /// <param name="symbolTable">The hashlist to which symbols will be added for encapsulated symbols</param>
        /// <returns>A byte array containing only the data-component</returns>
        byte[] SerializeData(SerializerOptions options, SymbolHashList symbolTable);

        /// <summary>
        /// Writes the payload to the stream
        /// </summary>
        /// <param name="outputStream">The output stream</param>
        /// <param name="payload">The payload to be written</param>
        /// <param name="options">The serializer options</param>
        /// <param name="hashList">The symbol table</param>
        public static void Write(
            Stream outputStream,
            ITypePayload payload,
            SerializerOptions options,
            SymbolHashList symbolTable)
        {
            var annotations = TypeMetadata.SerializeAnnotationData(payload.IonType, options, symbolTable);
            outputStream.WriteByte(payload.Metadata.Metadata);
            outputStream.Write(annotations);
            outputStream.Write(payload.SerializeData(options, symbolTable));
            outputStream.Flush();
        }

        /// <summary>
        /// <summary>
        /// Writes the payload to the stream
        /// </summary>
        /// <param name="outputStream">The output stream</param>
        /// <param name="payload">The payload to be written</param>
        /// <param name="options">The serializer options</param>
        /// <param name="symbolTable">The symbol table</param>
        public static async Task WriteAsync(
            Stream outputStream,
            ITypePayload payload,
            SerializerOptions options,
            SymbolHashList symbolTable)
        {
            var annotations = TypeMetadata.SerializeAnnotationData(payload.IonType, options, symbolTable);
            outputStream.WriteByte(payload.Metadata.Metadata);
            await outputStream.WriteAsync(annotations);
            await outputStream.WriteAsync(payload.SerializeData(options, symbolTable));
            await outputStream.FlushAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static ITypePayload Of(IIonType type)
        {
            return type.Type switch
            {
                IonTypes.Null => new IonNullPayload((IonNull)type),

                IonTypes.Bool => new IonBoolPayload((IonBool)type),

                IonTypes.Int => new IonIntPayload((IonInt)type),

                IonTypes.Decimal => new IonDecimalPayload((IonDecimal)type),

                IonTypes.Float => new IonFloatPayload((IonFloat)type),

                IonTypes.Timestamp => new IonTimestampPayload((IonTimestamp)type),

                IonTypes.String => new IonStringPayload((IonString)type),

                IonTypes.Clob => new IonClobPayload((IonClob)type),

                IonTypes.Blob => new IonBlobPayload((IonBlob)type),

                IonTypes.List => new IonListPayload((IonList)type),

                IonTypes.Sexp => new IonSexpPayload((IonSexp)type),

                IonTypes.Struct => new IonStructPayload((IonStruct)type),

                IonTypes.OperatorSymbol => new IonSymbolPayload((IonOperator)type),

                IonTypes.IdentifierSymbol => new IonSymbolPayload((IonIdentifier)type),

                IonTypes.QuotedSymbol => new IonSymbolPayload((IonQuotedSymbol)type),

                _ => throw new InvalidOperationException($"Invalid ion-type: {type.Type}")
            };
        }
    }
}
