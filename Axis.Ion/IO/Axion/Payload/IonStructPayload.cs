using Axis.Ion.Types;
using Axis.Ion.Utils;
using Axis.Luna.Extensions;
using System;
using System.IO;
using System.Linq;

namespace Axis.Ion.IO.Axion.Payload
{
    public readonly struct IonStructPayload : ITypePayload
    {
        public IonStructPayload(IonStruct list)
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
            out IonStructPayload payload)
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

        public static IonStructPayload Read(
            Stream stream,
            TypeMetadata metadata,
            SerializerOptions options,
            SymbolHashList symbolTable)
        {
            if (metadata.IonType != IonTypes.Struct)
                throw new ArgumentException($"Invalid symbol type-metadata: {metadata.IonType}.");

            // annotations
            var annotations = metadata.HasAnnotations
                ? TypeMetadata.ReadAnnotations(stream, options, symbolTable)
                : Array.Empty<IIonValue.Annotation>();

            // null?
            if (metadata.IsNull)
                return new IonStructPayload((IonStruct)IIonValue.NullOf(IonTypes.Struct, annotations));

            // non-null?
            else
            {
                var propertyCount = (int)stream.ReadVarByteInteger();

                return propertyCount
                    .RepeatApply(index =>
                    {
                        // name
                        var nameSymbol = BinarySerializer
                            .DeserializeIon(stream, options, symbolTable)
                            .ApplyTo(ion => (IonTextSymbol)ion!);

                        // value
                        var value = BinarySerializer
                            .DeserializeIon(stream, options, symbolTable)
                            ?? throw new InvalidOperationException($"");

                        return new IonStruct.Property(
                            nameSymbol.Value ?? throw new InvalidOperationException(""),
                            value);
                    })
                    .ApplyTo(properties => new IonStruct.Initializer(
                        annotations,
                        properties.ToArray()))
                    .ApplyTo(initializer => new IonStructPayload(new IonStruct(initializer)));
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

            var ionStruct = (IonStruct)IonValue;
            var properties = ionStruct.Value ?? Array.Empty<IonStruct.Property>();

            return properties.Length
                .ToVarBytes()
                .Concat(properties.SelectMany(property =>
                {
                    return BinarySerializer
                        .SerializeIon(property.Name, options, symbolTable)
                        .Concat(
                            BinarySerializer.SerializeIon(
                                property.Value,
                                options,
                                symbolTable));
                }))
                .ToArray();
        }

        #endregion

    }
}
