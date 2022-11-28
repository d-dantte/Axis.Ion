using Axis.Luna.Extensions;
using System;
using System.Linq;
using static Axis.Luna.Extensions.Common;

namespace Axis.Ion.Types
{
    public readonly struct IonString : IIonValueType<string?>
    {
        private readonly IIonType.Annotation[] _annotations;

        public string? Value { get; }

        public IonTypes Type => IonTypes.String;

        public IIonType.Annotation[] Annotations => _annotations?.ToArray() ?? Array.Empty<IIonType.Annotation>();

        internal IonString(string? value, params IIonType.Annotation[] annotations)
        {
            Value = value;
            _annotations = annotations.Validate();
        }

        #region IIonType

        public bool ValueEquals(IIonValueType<string?> other) => Value.NullOrEquals(other?.Value);

        public string ToIonText()
        {
            if (Value is null)
                return "null.string";

            return Value;
        }

        #endregion

        #region Record Implementation
        public override int GetHashCode()
            => HashCode.Combine(Value, ValueHash(Annotations.HardCast<IIonType.Annotation, object>()));

        public override bool Equals(object? obj)
        {
            return obj is IonString other
                && other.ValueEquals(this)
                && other.Annotations.SequenceEqual(Annotations);
        }

        public override string ToString() => Annotations
            .Select(a => a.ToString())
            .Concat(Value?.WrapIn("\""))
            .JoinUsing("");


        public static bool operator ==(IonString first, IonString second) => first.Equals(second);

        public static bool operator !=(IonString first, IonString second) => !first.Equals(second);

        #endregion

        public static implicit operator IonString(string? value) => new IonString(value);
    }
}
