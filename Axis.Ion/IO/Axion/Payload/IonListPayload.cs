using Axis.Ion.Types;
using Axis.Ion.Utils;
using Axis.Luna.Extensions;
using System;
using System.IO;
using System.Linq;

namespace Axis.Ion.IO.Axion.Payload
{
    public readonly struct IonListPayload : ITypePayload
    {
        public IonListPayload(IonList list)
        {
            IonValue = list;
            Metadata = TypeMetadata.SerializeMetadata(list);
        }

        #region Read
        public static bool TryRead(
            Stream stream,
            TypeMetadata metadata,
            SerializerOptions options,
            SymbolHashList symbolTable,
            out IonListPayload payload)
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

        public static IonListPayload Read(
            Stream stream,
            TypeMetadata metadata,
            SerializerOptions options,
            SymbolHashList symbolTable)
        {
            if (metadata.IonType != IonTypes.List)
                throw new ArgumentException($"Invalid symbol type-metadata: {metadata.IonType}.");

            // annotations
            var annotations = metadata.HasAnnotations
                ? TypeMetadata.ReadAnnotations(stream, options, symbolTable)
                : Array.Empty<IIonValue.Annotation>();

            // null?
            if (metadata.IsNull)
                return new IonListPayload((IonList)IIonValue.NullOf(IonTypes.List, annotations));

            // non-null?
            else
            {
                var itemCount = (int)stream.ReadVarByteInteger();

                return itemCount
                    .RepeatApply(index => BinarySerializer
                        .DeserializeIon(stream, options, symbolTable)
                        ?? throw new InvalidOperationException($""))
                    .ApplyTo(items => new IonList.Initializer(
                        annotations,
                        items.ToArray()))
                    .ApplyTo(initializer => new IonListPayload(new IonList(initializer)));
            }
        }
        #endregion

        #region ITypePayload

        public TypeMetadata Metadata { get; }

        public IIonValue IonValue { get; }

        public byte[] SerializeData(
            SerializerOptions options,
            SymbolHashList symbolTable)
        {
            if (IonValue.IsNull)
                return Array.Empty<byte>();

            var ionList = (IonList)IonValue;
            var items = ionList.Value ?? Array.Empty<IIonValue>();

            return items.Length
                .ToVarBytes()
                .Concat(items.SelectMany(item =>
                {
                    return BinarySerializer.SerializeIon(
                        item,
                        options,
                        symbolTable);
                }))
                .ToArray();
        }

        #endregion

    }
}
