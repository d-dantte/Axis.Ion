using Axis.Ion.Types;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Axis.Ion.IO.Binary
{
    public readonly struct IonBoolPayload : IPayloadWriter
    {
        public const byte TrueMask = 64;
        public const byte FalseMask = 191;

        public const int FalseValue = 0;
        public const int TrueValue = 1;


        public TypeMetadata Metadata { get; }

        public IIonType IonValue { get; }

        public IonBoolPayload(IonBool @bool)
        {
            IonValue = @bool;
            Metadata = ToBoolTypeMetadata(@bool);
        }

        #region Write

        public void Write(Stream outputStream)
        {
            var annotations = TypeMetadata.ToAnnotationData(IonValue);
            outputStream.WriteByte(Metadata.Metadata);
            outputStream.Write(annotations);
            outputStream.Flush();
        }

        public async Task WriteAsync(Stream outputStream)
        {
            var annotations = TypeMetadata.ToAnnotationData(IonValue);
            outputStream.WriteByte(Metadata.Metadata);
            await outputStream.WriteAsync(annotations);
            await outputStream.FlushAsync();
        }
        #endregion

        #region Read
        public static bool TryRead(
            TypeMetadata metadata,
            Stream stream,
            out IonSymbolPayload payload)
        {
            try
            {
                payload = Read(metadata, stream);
                return true;
            }
            catch
            {
                payload = default;
                return false;
            }
        }

        public static IonBoolPayload Read(
            TypeMetadata metadata,
            Stream stream)
        {
            if (metadata.IonType != IonTypes.Bool)
                throw new ArgumentException($"Invalid type-metadata: {metadata.IonType}. Expected type is: {IonTypes.Bool}");

            if (metadata.CustomMetadata != TrueValue
                && metadata.CustomMetadata != FalseValue)
                throw new ArgumentException($"Invalid bool value: {metadata.CustomMetadata}");

            // annotations
            var annotations = metadata.HasAnnotations
                ? TypeMetadata.ReadAnnotations(stream)
                : Array.Empty<IIonType.Annotation>();

            return new IonBoolPayload(metadata.CustomMetadata switch
            {
                0 => new IonBool(false, annotations),
                1 => new IonBool(true, annotations),
                _ => throw new ArgumentException($"Invalid bool value: {metadata.CustomMetadata}") // should get here though
            });
        }

        public static Task<IonBoolPayload> ReadAsync(
            TypeMetadata metadata,
            Stream stream)
        {
            if (metadata.IonType != IonTypes.Bool)
                return Task.FromException<IonBoolPayload>(
                    new ArgumentException($"Invalid type-metadata: {metadata.IonType}. Expected type is: {IonTypes.Bool}"));

            if (metadata.CustomMetadata != TrueValue
                && metadata.CustomMetadata != FalseValue)
                return Task.FromException<IonBoolPayload>(
                    new ArgumentException($"Invalid bool value: {metadata.CustomMetadata}"));

            // annotations
            var annotations = metadata.HasAnnotations
                ? TypeMetadata.ReadAnnotations(stream)
                : Array.Empty<IIonType.Annotation>();

            var payload = new IonBoolPayload(metadata.CustomMetadata switch
            {
                0 => new IonBool(false, annotations),
                1 => new IonBool(true, annotations),
                _ => throw new ArgumentException($"Invalid bool value: {metadata.CustomMetadata}") // should get here though
            });

            return Task.FromResult(payload);
        }
        #endregion

        private static byte ToBoolTypeMetadata(IonBool @bool)
        {
            var metadata = TypeMetadata.ToMetadataByte(@bool);
            return @bool.Value switch
            {
                true => (byte)(metadata | TrueMask),
                false => (byte)(metadata & FalseMask),
                _ => metadata
            };
        }
    }
}
