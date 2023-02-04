using Axis.Ion.IO.Binary.Payload;
using Axis.Ion.Types;
using Axis.Ion.Utils;
using Axis.Luna.Extensions;
using System;
using System.IO;
using System.Linq;

namespace Axis.Ion.IO.Binary
{
    public readonly struct TypeMetadata
    {
        #region Masks
        internal const byte IonTypeMask = 15;
        internal const byte AnnotationMask = 16;
        internal const byte NullMask = 32;
        #endregion


        private readonly byte _metadata;

        /// <summary>
        /// The raw metadata byte
        /// </summary>
        public byte Metadata => _metadata;

        /// <summary>
        /// The <see cref="IonTypes"/> represented by this metadata.
        /// </summary>
        public IonTypes IonType { get; }

        /// <summary>
        /// Indicates if the ion value has annotations
        /// </summary>
        public bool HasAnnotations => (_metadata & AnnotationMask) == AnnotationMask;

        /// <summary>
        /// Indicates if the ion value is null
        /// </summary>
        public bool IsNull => (_metadata & NullMask) == NullMask;

        /// <summary>
        /// Returns the free bits as an integer value, usable by the individual ion-types as they choose.
        /// </summary>
        public int CustomMetadata => _metadata >> 6;


        public TypeMetadata(byte metadata)
        {
            _metadata = metadata;
            IonType = (IonTypes)(metadata & IonTypeMask);

            Validate();
        }

        private void Validate()
        {
            if (!Enum.IsDefined(typeof(IonTypes), IonType))
                throw new ArgumentException($"Invalid metadata ion type: {IonType}");
        }


        public override bool Equals(object? obj)
        {
            return obj is TypeMetadata other
                && other._metadata == _metadata;
        }

        public override int GetHashCode() => _metadata.GetHashCode();

        #region metadata
        /// <summary>
        /// Serializes the metadata of the <see cref="IIonType"/>
        /// </summary>
        /// <param name="ionType">The ion type</param>
        /// <returns>The metadata byte</returns>
        public static byte SerializeMetadata(IIonType ionType)
        {
            byte metadatabyte = (byte)ionType.Type;

            // annotated?
            byte mask = ionType.Annotations.Length > 0
                ? AnnotationMask
                : (byte)0;
            metadatabyte |= mask;

            // null?
            mask = ionType.IsNull
                ? NullMask
                : (byte)0;
            metadatabyte |= mask;

            return metadatabyte;
        }

        public static TypeMetadata ReadMetadata(Stream inputStream)
        {
            var metadatabyte = inputStream.ReadByte();

            if (metadatabyte < 0)
                throw new EndOfStreamException();

            return (byte)metadatabyte;
        }
        #endregion

        #region annotations
        /// <summary>
        /// Serializes the annotations, if present. If absent, returns an empty byte array
        /// </summary>
        /// <param name="ionType">The ion type</param>
        /// <returns>The serialized annotations</returns>
        public static byte[] SerializeAnnotationData(
            IIonType ionType,
            SerializerOptions options,
            SymbolHashList symbolTable)
        {
            var memory = new MemoryStream();

            // annotations?
            if (ionType.Annotations.Length > 0)
            {
                // annotation count
                memory.Write(ionType.Annotations.Length.ToVarBytes());

                ionType.Annotations
                    .Select(annotation =>
                    {
                        var symbol = annotation.ToSymbol();
                        return new IonSymbolPayload(symbolTable.AddOrGetID(symbol));
                    })
                    .ForAll(payload => ITypePayload.Write(memory, payload, options, symbolTable));

                memory.Flush();
            }

            return memory.ToArray();
        }

        public static IIonType.Annotation[] ReadAnnotations(
            Stream stream,
            SerializerOptions options,
            SymbolHashList symbolTable)
        {
            var annotationCount = stream.ReadVarByteInteger();
            return annotationCount
                .RepeatApply(count =>
                {
                    var metadata = ReadMetadata(stream);

                    // verify that this annotation-symbol doesn't have it's own annotation
                    if (metadata.HasAnnotations)
                        throw new InvalidDataException("Invalid annotation metadata");

                    return IonSymbolPayload
                        .Read(stream, metadata, options, symbolTable)
                        .ApplyTo(payload => payload.IonType.ToIonText())
                        .ApplyTo(IIonType.Annotation.Parse);
                })
                .ToArray();
        }
        #endregion

        #region operator overloads
        public static implicit operator TypeMetadata(byte value) => new TypeMetadata(value);

        public static bool operator ==(TypeMetadata first, TypeMetadata second) => first.Equals(second);

        public static bool operator !=(TypeMetadata first, TypeMetadata second) => !first.Equals(second);
        #endregion
    }
}
