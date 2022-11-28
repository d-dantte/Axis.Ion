using Axis.Luna.Extensions;
using System;
using System.Linq;
using static Axis.Luna.Extensions.Common;

namespace Axis.Ion.Types
{
    public readonly struct IonTimestamp : IIonValueType<DateTimeOffset?>
    {
        internal static readonly string _Format = "yyyy-MM-ddTHH:mm:ss.ffffffzzz";

        private readonly IIonType.Annotation[] _annotations;

        public DateTimeOffset? Value { get; }

        public IonTypes Type => IonTypes.Timestamp;

        public IIonType.Annotation[] Annotations => _annotations?.ToArray() ?? Array.Empty<IIonType.Annotation>();

        internal IonTimestamp(DateTimeOffset? value, params IIonType.Annotation[] annotations)
        {
            Value = value;
            _annotations = annotations.Validate();
        }

        #region IIonType

        public bool ValueEquals(IIonValueType<DateTimeOffset?> other) => Value == other?.Value == true;

        public string ToIonText() => Value?.ToString(_Format) ?? "null.timestamp";

        #endregion

        #region Record Implementation
        public override int GetHashCode()
            => HashCode.Combine(Value, ValueHash(Annotations.HardCast<IIonType.Annotation, object>()));

        public override bool Equals(object? obj)
        {
            return obj is IonTimestamp other
                && other.ValueEquals(this)
                && other.Annotations.SequenceEqual(Annotations);
        }

        public override string ToString() => Annotations
            .Select(a => a.ToString())
            .Concat(ToIonText())
            .JoinUsing("");


        public static bool operator ==(IonTimestamp first, IonTimestamp second) => first.Equals(second);

        public static bool operator !=(IonTimestamp first, IonTimestamp second) => !first.Equals(second);

        #endregion

        public static implicit operator IonTimestamp(DateTimeOffset? value) => new IonTimestamp(value);
    }
}
