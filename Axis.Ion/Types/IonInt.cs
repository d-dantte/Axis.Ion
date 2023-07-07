using Axis.Ion.Numerics;
using Axis.Luna.Common.Numerics;
using Axis.Luna.Extensions;
using System;
using System.Linq;
using System.Numerics;
using static Axis.Luna.Extensions.Common;

namespace Axis.Ion.Types
{
    /// <summary>
    /// Ion Int.
    /// Note that all encapsulated values are assumed to be signed - including single byte values.
    /// </summary>
    public readonly struct IonInt :
        IStructValue<BigInteger>,
        IIonDeepCopyable<IonInt>,
        IIonNullable<IonInt>,
        INumericType
    {
        private readonly IIonValue.Annotation[] _annotations;

        public BigInteger? Value { get; }

        public IonTypes Type => IonTypes.Int;

        public IIonValue.Annotation[] Annotations => _annotations?.ToArray() ?? Array.Empty<IIonValue.Annotation>();

        public IonInt(BigInteger? value, params IIonValue.Annotation[] annotations)
        {
            Value = value;
            _annotations = annotations
                .Validate()
                .ToArray();
        }

        /// <summary>
        /// Creates a null instance of the <see cref="IonInt"/>
        /// </summary>
        /// <param name="annotations">any available annotation</param>
        /// <returns>The newly created null instance</returns>
        public static IonInt Null(params IIonValue.Annotation[] annotations) => new IonInt(null, annotations);

        #region IIonType

        public bool IsNull => Value == null;

        public bool ValueEquals(IStructValue<BigInteger> other) => Value == other?.Value == true;

        public string ToIonText() => Value?.ToString() ?? "null.int";

        #endregion

        #region INumericType
        public BigDecimal? ToBigDecimal() => Value == null ? null : new BigDecimal(Value.Value);
        #endregion

        #region Record Implementation
        public override int GetHashCode()
            => HashCode.Combine(Value, ValueHash(Annotations.HardCast<IIonValue.Annotation, object>()));

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

        #region IIonDeepCopy<>
        IIonValue IIonDeepCopyable<IIonValue>.DeepCopy() => DeepCopy();

        public IonInt DeepCopy() => new IonInt(Value, Annotations);
        #endregion

        public static implicit operator IonInt(long? value) => new IonInt(value);

        public static implicit operator IonInt(BigInteger? value) => new IonInt(value);
    }
}
