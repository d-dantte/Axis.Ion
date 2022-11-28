using Axis.Ion.Utils;
using Axis.Luna.Extensions;
using System;
using System.Linq;
using static Axis.Luna.Extensions.Common;

namespace Axis.Ion.Types
{
    public readonly struct IonFloat : IIonValueType<double?>
    {
        private readonly IIonType.Annotation[] _annotations;

        public double? Value { get; }

        public IonTypes Type => IonTypes.Float;

        public IIonType.Annotation[] Annotations => _annotations?.ToArray() ?? Array.Empty<IIonType.Annotation>();

        internal IonFloat(double? value, params IIonType.Annotation[] annotations)
        {
            Value = value;
            _annotations = annotations.Validate();
        }

        #region IIonType

        public bool ValueEquals(IIonValueType<double?> other) => Value == other?.Value == true;

        public string ToIonText() => Value != null 
            ? new DecomposedDecimal(Value.Value).ToScientificNotation()
            : "null.float";

        #endregion

        #region Record Implementation
        public override int GetHashCode()
            => HashCode.Combine(Value, ValueHash(Annotations.HardCast<IIonType.Annotation, object>()));

        public override bool Equals(object? obj)
        {
            return obj is IonFloat other
                && other.ValueEquals(this)
                && other.Annotations.SequenceEqual(Annotations);
        }

        public override string ToString() => Annotations
            .Select(a => a.ToString())
            .Concat(ToIonText())
            .JoinUsing("");


        public static bool operator ==(IonFloat first, IonFloat second) => first.Equals(second);

        public static bool operator !=(IonFloat first, IonFloat second) => !first.Equals(second);

        #endregion

        public static implicit operator IonFloat(double? value) => new IonFloat(value);
    }
}
