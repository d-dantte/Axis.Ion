using Axis.Ion.Types;
using Axis.Ion.Utils;
using System;
using System.IO;

namespace Axis.Ion.IO.Axion.Payload
{
    public readonly struct IonBoolPayload : ITypePayload
    {
        public const byte TrueMask = 64;
        public const byte FalseMask = 191;

        public const int FalseValue = 0;
        public const int TrueValue = 1;

        public IonBoolPayload(IonBool @bool)
        {
            IonType = @bool;
            Metadata = ToBoolTypeMetadata(@bool);
        }

        private static byte ToBoolTypeMetadata(IonBool @bool)
        {
            var metadata = TypeMetadata.SerializeMetadata(@bool);
            return @bool.Value switch
            {
                true => (byte)(metadata | TrueMask),
                false => (byte)(metadata & FalseMask),
                _ => metadata
            };
        }

        #region Read
        public static bool TryRead(
            Stream stream,
            TypeMetadata metadata,
            SerializerOptions options,
            SymbolHashList symbolTable,
            out IonBoolPayload payload)
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

        public static IonBoolPayload Read(
            Stream stream,
            TypeMetadata metadata,
            SerializerOptions options,
            SymbolHashList symbolTable)
        {
            if (metadata.IonType != IonTypes.Bool)
                throw new ArgumentException($"Invalid type-metadata: {metadata.IonType}. Expected type is: {IonTypes.Bool}");

            if (metadata.CustomMetadata != TrueValue
                && metadata.CustomMetadata != FalseValue)
                throw new ArgumentException($"Invalid bool value: {metadata.CustomMetadata}");

            // annotations
            var annotations = metadata.HasAnnotations
                ? TypeMetadata.ReadAnnotations(stream, options, symbolTable)
                : Array.Empty<IIonType.Annotation>();

            if (metadata.IsNull)
                return new IonBoolPayload(
                    (IonBool)IIonType.NullOf(IonTypes.Bool, annotations));

            return new IonBoolPayload(metadata.CustomMetadata switch
            {
                0 => new IonBool(false, annotations),
                1 => new IonBool(true, annotations),
                _ => throw new ArgumentException($"Invalid bool value: {metadata.CustomMetadata}") // should get here though
            });
        }
        #endregion

        #region ITypePayload

        public TypeMetadata Metadata { get; }

        public IIonType IonType { get; }

        public byte[] SerializeData(
            SerializerOptions options,
            SymbolHashList hashList)
            => Array.Empty<byte>();

        #endregion
    }
}
