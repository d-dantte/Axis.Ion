using Axis.Luna.Extensions;
using System;
using System.Linq;
using System.Numerics;
using static Axis.Luna.Extensions.Common;

namespace Axis.Ion.Types
{
    /// <summary>
    /// TODO: replace long? with BigInteger?
    /// </summary>
    public readonly struct IonInt : IIonValueType<BigInteger?>
    {
        private readonly IIonType.Annotation[] _annotations;

        public BigInteger? Value { get; }

        public IonTypes Type => IonTypes.Int;

        public IIonType.Annotation[] Annotations => _annotations?.ToArray() ?? Array.Empty<IIonType.Annotation>();

        internal IonInt(
            long? value,
            params IIonType.Annotation[] annotations)
            : this((BigInteger?)value, annotations)
        {
        }

        internal IonInt(BigInteger? value, params IIonType.Annotation[] annotations)
        {
            Value = value;
            _annotations = annotations.Validate();
        }

        #region IIonType

        public bool IsNull => Value == null;

        public bool ValueEquals(IIonValueType<BigInteger?> other) => Value == other?.Value == true;

        public string ToIonText() => Value?.ToString() ?? "null.int";

        #endregion

        #region Record Implementation
        public override int GetHashCode()
            => HashCode.Combine(Value, ValueHash(Annotations.HardCast<IIonType.Annotation, object>()));

        public override bool Equals(object? obj)
        {
            return obj is IonInt other
                && other.ValueEquals(this)
                && other.Annotations.SequenceEqual(Annotations);
        }

        public override string ToString() => Annotations
            .Select(a => a.ToString())
            .Concat(ToIonText())
            .JoinUsing("");


        public static bool operator ==(IonInt first, IonInt second) => first.Equals(second);

        public static bool operator !=(IonInt first, IonInt second) => !first.Equals(second);

        #endregion

        public static implicit operator IonInt(long? value) => new IonInt(value);

        public static implicit operator IonInt(BigInteger? value) => new IonInt(value);
    }
}
