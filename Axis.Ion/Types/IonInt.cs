using Axis.Luna.Extensions;
using System;
using System.Linq;
using static Axis.Luna.Extensions.Common;

namespace Axis.Ion.Types
{
    public readonly struct IonInt : IIonValueType<long?>
    {
        private readonly IIonType.Annotation[] _annotations;

        public long? Value { get; }

        public IonTypes Type => IonTypes.Int;

        public IIonType.Annotation[] Annotations => _annotations?.ToArray() ?? Array.Empty<IIonType.Annotation>();

        internal IonInt(long? value, params IIonType.Annotation[] annotations)
        {
            Value = value;
            _annotations = annotations.Validate();
        }

        #region IIonType

        public bool ValueEquals(IIonValueType<long?> other) => Value == other?.Value == true;

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
    }
}
