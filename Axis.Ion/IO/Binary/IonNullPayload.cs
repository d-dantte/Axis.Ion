using Axis.Ion.Types;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Axis.Ion.IO.Binary
{
    public readonly struct IonNullPayload : IPayloadWriter
    {
        public TypeMetadata Metadata { get; }
        
        public IIonType IonValue { get; }


        public IonNullPayload(IonNull @null)
        {
            IonValue = @null;
            Metadata = TypeMetadata.ToMetadataByte(@null);
        }

        #region Write

        public void Write(Stream outputStream)
        {
            var (metadata, annotations) = TypeMetadata.ToTypeComponents(IonValue);
            outputStream.WriteByte(metadata);
            outputStream.Write(annotations);
            outputStream.Flush();
        }

        public async Task WriteAsync(Stream outputStream)
        {
            var (metadata, annotations) = TypeMetadata.ToTypeComponents(IonValue);
            await outputStream.WriteAsync(new[] { metadata });
            await outputStream.WriteAsync(annotations);
            await outputStream.FlushAsync();
        }
        #endregion

        #region Read
        public static bool TryRead(
            TypeMetadata metadata,
            Stream stream,
            out IonNullPayload payload)
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

        public static IonNullPayload Read(
            TypeMetadata metadata,
            Stream stream)
        {
            if (metadata.IonType != IonTypes.Null)
                throw new ArgumentException($"Invalid type-metadata: {metadata.IonType}. Expected type is: {IonTypes.Null}");

            var annotations = metadata.HasAnnotations
                ? TypeMetadata.ReadAnnotations(stream)
                : Array.Empty<IIonType.Annotation>();

            return new IonNullPayload(new IonNull(annotations));
        }

        public static async Task<IonNullPayload> ReadAsync(
            TypeMetadata metadata,
            Stream stream)
        {
            if (metadata.IonType != IonTypes.Null)
                throw new ArgumentException($"Invalid type-metadata: {metadata.IonType}. Expected type is: {IonTypes.Null}");

            var annotations = metadata.HasAnnotations
                ? await TypeMetadata.ReadAnnotationsAsync(stream)
                : Array.Empty<IIonType.Annotation>();

            return new IonNullPayload(new IonNull(annotations));
        }
        #endregion
    }
}
