using Axis.Luna.Extensions;
using System;
using System.Linq;
using static Axis.Luna.Extensions.Common;

namespace Axis.Ion.Types
{
    public readonly struct IonBool : IIonValueType<bool?>
    {
        private readonly IIonType.Annotation[] _annotations;

        public bool? Value { get; }

        public IonTypes Type => IonTypes.Bool;

        public IIonType.Annotation[] Annotations => _annotations?.ToArray() ?? Array.Empty<IIonType.Annotation>();

        internal IonBool(bool? value, params IIonType.Annotation[] annotations)
        {
            Value = value;
            _annotations = annotations.Validate();
        }


        #region IIonType

        public bool ValueEquals(IIonValueType<bool?> other) => Value.NullOrEquals(other.Value);

        public string ToIonText() => Value?.ToString() ?? "null.bool";

        #endregion

        #region Record Implementation
        public override int GetHashCode()
            => HashCode.Combine(Value, ValueHash(Annotations.HardCast<IIonType.Annotation, object>()));

        public override bool Equals(object? obj)
        {
            return obj is IonBool other
                && other.ValueEquals(this)
                && other.Annotations.SequenceEqual(Annotations);
        }

        public override string ToString() => Annotations
            .Select(a => a.ToString())
            .Concat(ToIonText())
            .JoinUsing("");


        public static bool operator ==(IonBool first, IonBool second) => first.Equals(second);

        public static bool operator !=(IonBool first, IonBool second) => !first.Equals(second);

        #endregion

        public static implicit operator IonBool(bool? value) => new IonBool(value);
    }
}
