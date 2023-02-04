using Axis.Ion.Utils;
using Axis.Luna.Extensions;
using System;
using System.Linq;
using static Axis.Luna.Extensions.Common;

namespace Axis.Ion.Types
{
    public readonly struct IonDecimal : IIonValueType<decimal?>
    {
        private readonly IIonType.Annotation[] _annotations;

        public decimal? Value { get; }

        public IonTypes Type => IonTypes.Decimal;

        public IIonType.Annotation[] Annotations => _annotations?.ToArray() ?? Array.Empty<IIonType.Annotation>();

        public IonDecimal(decimal? value, params IIonType.Annotation[] annotations)
        {
            Value = value;
            _annotations = annotations
                .Validate()
                .ToArray();
        }


        #region IIonType

        public bool IsNull => Value == null;

        public bool ValueEquals(IIonValueType<decimal?> other) => Value == other?.Value == true;

        public string ToIonText() => Value != null
            ? Value.Value.ToExponentNotation("D")
            : "null.decimal";

        #endregion

        #region Record Implementation
        public override int GetHashCode()
            => HashCode.Combine(Value, ValueHash(Annotations.HardCast<IIonType.Annotation, object>()));

        public override bool Equals(object? obj)
        {
            return obj is IonDecimal other
                && other.ValueEquals(this)
                && other.Annotations.SequenceEqual(Annotations);
        }

        public override string ToString() => Annotations
            .Select(a => a.ToString())
            .Concat(ToIonText())
            .JoinUsing("");


        public static bool operator ==(IonDecimal first, IonDecimal second) => first.Equals(second);

        public static bool operator !=(IonDecimal first, IonDecimal second) => !first.Equals(second);

        #endregion

        public static implicit operator IonDecimal(decimal? value) => new IonDecimal(value);
    }
}
