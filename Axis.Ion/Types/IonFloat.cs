using Axis.Ion.Numerics;
using Axis.Luna.Common.Numerics;
using Axis.Luna.Extensions;
using System;
using System.Linq;
using static Axis.Luna.Extensions.Common;

namespace Axis.Ion.Types
{
    public readonly struct IonFloat :
        IStructValue<double>,
        IIonDeepCopyable<IonFloat>,
        IIonNullable<IonFloat>,
        INumericType
    {
        private readonly IIonValue.Annotation[] _annotations;

        public double? Value { get; }

        public IonTypes Type => IonTypes.Float;

        public IIonValue.Annotation[] Annotations => _annotations?.ToArray() ?? Array.Empty<IIonValue.Annotation>();

        public IonFloat(double? value, params IIonValue.Annotation[] annotations)
        {
            Value = value;
            _annotations = annotations
                .Validate()
                .ToArray();
        }

        /// <summary>
        /// Creates a null instance of the <see cref="IonFloat"/>
        /// </summary>
        /// <param name="annotations">any available annotation</param>
        /// <returns>The newly created null instance</returns>
        public static IonFloat Null(params IIonValue.Annotation[] annotations) => new IonFloat(null, annotations);

        #region IIonType

        public bool IsNull => Value == null;

        public bool ValueEquals(IStructValue<double> other) => Value.NullOrEquals(other.Value);

        public string ToIonText() => Value switch
        {
            null => "null.float",
            double.NaN => "nan",
            double.PositiveInfinity => "+inf",
            double.NegativeInfinity => "-inf",
            _ => Value.Value.ToExponentNotation()
        };

        #endregion

        #region INumericType
        public BigDecimal? ToBigDecimal() => Value == null ? null : new BigDecimal(Value.Value);
        #endregion

        #region Record Implementation
        public override int GetHashCode()
            => HashCode.Combine(Value, ValueHash(Annotations.HardCast<IIonValue.Annotation, object>()));

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

        #region IIonDeepCopy<>
        IIonValue IIonDeepCopyable<IIonValue>.DeepCopy() => DeepCopy();

        public IonFloat DeepCopy() => new IonFloat(Value, Annotations);
        #endregion

        public static implicit operator IonFloat(double? value) => new IonFloat(value);
    }
}
