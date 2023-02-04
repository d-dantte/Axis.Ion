using Axis.Luna.Extensions;
using System;
using System.Linq;
using static Axis.Luna.Extensions.Common;

namespace Axis.Ion.Types
{
    /// <summary>
    /// Represents a readonly array of bytes. The array returned by <see cref="IonBlob.Value"/> is always a copy of the internal
    /// array, that way, array elements cannot be reassigned.
    /// </summary>
    public readonly struct IonBlob : IIonValueType<byte[]?>
    {
        private readonly IIonType.Annotation[] _annotations;
        private readonly byte[]? _blob;

        public byte[]? Value => _blob?.ToArray();

        public IonTypes Type => IonTypes.Blob;

        public IIonType.Annotation[] Annotations => _annotations?.ToArray() ?? Array.Empty<IIonType.Annotation>();

        public IonBlob(byte[]? value, params IIonType.Annotation[] annotations)
        {
            _blob = value;
            _annotations = annotations
                .Validate()
                .ToArray();
        }

        #region IIonType

        public bool IsNull => Value == null;

        public bool ValueEquals(IIonValueType<byte[]?> other)
            => other.Value.NullOrTrue(Value, Enumerable.SequenceEqual);

        public string ToIonText()
        {
            if (_blob is null)
                return "null.blob";

            return Convert
                .ToBase64String(_blob)
                .WrapIn("{{ ", " }}");
        }

        #endregion


        #region Record Implementation
        public override int GetHashCode()
            => HashCode.Combine(Value, ValueHash(Annotations.HardCast<IIonType.Annotation, object>()));

        public override bool Equals(object? obj)
        {
            return obj is IonBlob other
                && other.ValueEquals(this)
                && other.Annotations.SequenceEqual(Annotations);
        }

        public override string ToString() => Annotations
            .Select(a => a.ToString())
            .Concat(ToIonText())
            .JoinUsing("");


        public static bool operator ==(IonBlob first, IonBlob second) => first.Equals(second);

        public static bool operator !=(IonBlob first, IonBlob second) => !first.Equals(second);

        #endregion

        public static implicit operator IonBlob(byte[]? value) => new IonBlob(value);
    }
}
