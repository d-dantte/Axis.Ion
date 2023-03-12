using Axis.Luna.Extensions;
using System;
using System.Linq;
using static Axis.Luna.Extensions.Common;

namespace Axis.Ion.Types
{
    public readonly struct IonTimestamp : IStructValue<DateTimeOffset>
    {
        internal static readonly string Format = "yyyy-MM-ddTHH:mm:ss.ffffffzzz";

        private readonly IIonType.Annotation[] _annotations;

        public DateTimeOffset? Value { get; }

        public IonTypes Type => IonTypes.Timestamp;

        public IIonType.Annotation[] Annotations => _annotations?.ToArray() ?? Array.Empty<IIonType.Annotation>();

        public IonTimestamp(DateTimeOffset? value, params IIonType.Annotation[] annotations)
        {
            Value = value;
            _annotations = annotations
                .Validate()
                .ToArray();
        }

        /// <summary>
        /// Creates a null instance of the <see cref="IonTimestamp"/>
        /// </summary>
        /// <returns>The newly created null instance</returns>
        public static IonTimestamp Null(params IIonType.Annotation[] annotations) => new IonTimestamp(null, annotations);

        #region IIonType

        public bool IsNull => Value == null;

        public bool ValueEquals(IStructValue<DateTimeOffset> other) => Value == other?.Value == true;

        public string ToIonText() => Value?.ToString(Format) ?? "null.timestamp";

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

        public IonTimestamp Copy(Precision precision = Precision.Millisecond)
        {
            if (Value is null)
                return new IonTimestamp(null, _annotations);

            var ts = Value.Value;
            var milliseconds = int.Parse(ts.ToString("ffffff")) / 1000d;
            var dateTime = precision switch
            {
                Precision.Year => new DateTimeOffset(ts.Year, 1, 1, 0, 0, 0, Value.Value.Offset),
                Precision.Month => new DateTimeOffset(ts.Year, ts.Month, 1, 0, 0, 0, Value.Value.Offset),
                Precision.Day => new DateTimeOffset(ts.Year, ts.Month, ts.Day, 0, 0, 0, Value.Value.Offset),
                Precision.Minute => new DateTimeOffset(ts.Year, ts.Month, ts.Day, ts.Hour, ts.Minute, 0, Value.Value.Offset),
                Precision.Second => new DateTimeOffset(ts.Year, ts.Month, ts.Day, ts.Hour, ts.Minute, ts.Second, Value.Value.Offset),
                Precision.Millisecond => new DateTimeOffset(ts.Year, ts.Month, ts.Day, ts.Hour, ts.Minute, ts.Second, Value.Value.Offset) + TimeSpan.FromMilliseconds(milliseconds),
                _ => throw new ArgumentException($"Invalid precision: {precision}")
            };

            return new IonTimestamp(dateTime, _annotations);
        }

        public IonTimestamp SwitchOffset(TimeSpan newOffset)
        {
            if (Value is null)
                return new IonTimestamp(null, _annotations);

            return new IonTimestamp(
                new DateTimeOffset(Value.Value.DateTime, newOffset),
                _annotations);
        }

        public static implicit operator IonTimestamp(DateTimeOffset? value) => new IonTimestamp(value);

        public static implicit operator IonTimestamp(DateTime? value) => value.Map(v => new DateTimeOffset(v));

        #region Nested types
        public enum Precision
        {
            Year,
            Month,
            Day,
            Minute,
            Second,
            Millisecond
        }
        #endregion
    }
}
