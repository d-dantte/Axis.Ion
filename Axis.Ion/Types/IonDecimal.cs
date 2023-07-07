using Axis.Ion.Numerics;
using Axis.Luna.Common.Numerics;
using Axis.Luna.Extensions;
using System;
using System.Linq;
using static Axis.Luna.Extensions.Common;

namespace Axis.Ion.Types
{
    public readonly struct IonPrimitiveDecimal :
        IStructValue<decimal>,
        IIonDeepCopyable<IonPrimitiveDecimal>,
        IIonNullable<IonPrimitiveDecimal>,
        INumericType
    {
        private readonly IIonValue.Annotation[] _annotations;

        public decimal? Value { get; }

        public IonTypes Type => IonTypes.Decimal;

        public IIonValue.Annotation[] Annotations => _annotations?.ToArray() ?? Array.Empty<IIonValue.Annotation>();

        public IonPrimitiveDecimal(decimal? value, params IIonValue.Annotation[] annotations)
        {
            Value = value;
            _annotations = annotations
                .Validate()
                .ToArray();
        }

        /// <summary>
        /// Creates a null instance of the <see cref="IonDecimal"/>
        /// </summary>
        /// <param name="annotations">any available annotation</param>
        /// <returns>The newly created null instance</returns>
        public static IonPrimitiveDecimal Null(params IIonValue.Annotation[] annotations) => new IonPrimitiveDecimal(null, annotations);

        #region IIonType

        public bool IsNull => Value == null;

        public bool ValueEquals(IStructValue<decimal> other) => Value == other?.Value == true;

        public string ToIonText() => Value != null
            ? Value.Value.ToExponentNotation("D")
            : "null.decimal";

        #endregion

        #region INumericType
        public BigDecimal? ToBigDecimal() => Value == null ? null : new BigDecimal(Value.Value);
        #endregion

        #region Record Implementation
        public override int GetHashCode()
            => HashCode.Combine(Value, ValueHash(Annotations.HardCast<IIonValue.Annotation, object>()));

        public override bool Equals(object? obj)
        {
            return obj is IonPrimitiveDecimal other
                && other.ValueEquals(this)
                && other.Annotations.SequenceEqual(Annotations);
        }

        public override string ToString() => Annotations
            .Select(a => a.ToString())
            .Concat(ToIonText())
            .JoinUsing("");


        public static bool operator ==(IonPrimitiveDecimal first, IonPrimitiveDecimal second) => first.Equals(second);

        public static bool operator !=(IonPrimitiveDecimal first, IonPrimitiveDecimal second) => !first.Equals(second);

        #endregion

        #region IIonDeepCopy<>
        IIonValue IIonDeepCopyable<IIonValue>.DeepCopy() => DeepCopy();

        public IonPrimitiveDecimal DeepCopy() => new IonPrimitiveDecimal(Value, Annotations);
        #endregion

        public static implicit operator IonPrimitiveDecimal(decimal? value) => new IonPrimitiveDecimal(value);
    }

    public readonly struct IonDecimal :
        IStructValue<BigDecimal>,
        IIonDeepCopyable<IonDecimal>,
        IIonNullable<IonDecimal>,
        INumericType
    {
        private readonly IIonValue.Annotation[] _annotations;

        public BigDecimal? Value { get; }

        public IonTypes Type => IonTypes.Decimal;

        public IIonValue.Annotation[] Annotations => _annotations?.ToArray() ?? Array.Empty<IIonValue.Annotation>();

        public IonDecimal(BigDecimal? value, params IIonValue.Annotation[] annotations)
        {
            Value = value;
            _annotations = annotations
                .Validate()
                .ToArray();
        }

        /// <summary>
        /// Creates a null instance of the <see cref="IonDecimal"/>
        /// </summary>
        /// <param name="annotations">any available annotation</param>
        /// <returns>The newly created null instance</returns>
        public static IonDecimal Null(params IIonValue.Annotation[] annotations) => new IonDecimal(null, annotations);

        #region IIonType

        public bool IsNull => Value == null;

        public bool ValueEquals(IStructValue<BigDecimal> other) => Value == other?.Value == true;

        public string ToIonText() => Value is not null
            ? Value!.Value.ToString().Replace("E","D")
            : "null.decimal";

        #endregion

        #region INumericType
        public BigDecimal? ToBigDecimal() => Value;
        #endregion

        #region Record Implementation
        public override int GetHashCode()
            => HashCode.Combine(Value, ValueHash(Annotations.HardCast<IIonValue.Annotation, object>()));

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

        #region IIonDeepCopy<>
        IIonValue IIonDeepCopyable<IIonValue>.DeepCopy() => DeepCopy();

        public IonDecimal DeepCopy() => new IonDecimal(Value, Annotations);
        #endregion

        public static implicit operator IonDecimal(BigDecimal? value) => new IonDecimal(value);

        public static implicit operator IonDecimal(decimal? value) => new IonDecimal(value);
    }
}
