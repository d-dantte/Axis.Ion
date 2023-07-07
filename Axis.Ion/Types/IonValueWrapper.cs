using Axis.Luna.Extensions;
using System;
using System.Numerics;

namespace Axis.Ion.Types
{
    /// <summary>
    /// PS: this struct should NEVER accept null IIonTypes. If the intent is to pass an IonNull, it should be deliberate
    /// <para>
    /// Note that <see cref="IIonValue"/>s that have no direct conversion to c# primitive types will not have implicit conversions
    /// to this wrapper. To use them with the wrapper, do the following
    /// <code>
    /// IonValueWrapper w = IIonType.Of(xxx).Wrap();
    /// </code>
    /// These types include:
    /// <list type="number">
    ///     <item><see cref="IonNull"/></item>
    ///     <item><see cref="IIonSymbol"/></item>
    ///     <item><see cref="IonClob"/></item>
    /// </list>
    /// </para>
    /// </summary>
    public readonly struct IonValueWrapper
    {
        public IIonValue Value { get; }

        public IonValueWrapper(IIonValue value)
        {
            Value = value.ThrowIfNull(new ArgumentNullException(nameof(value)));
        }


        #region Implicit operators

        public static implicit operator IonValueWrapper(bool? value)
            => new IonValueWrapper(new IonBool(value));

        public static implicit operator IonValueWrapper(long? value)
            => new IonValueWrapper(new IonInt(value));

        public static implicit operator IonValueWrapper(BigInteger? value)
            => new IonValueWrapper(new IonInt(value));

        public static implicit operator IonValueWrapper(double? value)
            => new IonValueWrapper(new IonFloat(value));

        public static implicit operator IonValueWrapper(decimal? value)
            => new IonValueWrapper(new IonDecimal(value));

        public static implicit operator IonValueWrapper(DateTimeOffset? value)
            => new IonValueWrapper(new IonTimestamp(value));

        public static implicit operator IonValueWrapper(string value)
            => new IonValueWrapper(new IonString(value));

        public static implicit operator IonValueWrapper(byte[] value)
            => new IonValueWrapper(new IonBlob(value));

        public static implicit operator IonValueWrapper(IonStruct.Initializer value)
            => new IonValueWrapper(new IonStruct(value));

        public static implicit operator IonValueWrapper(IonList.Initializer value)
            => new IonValueWrapper(new IonList(value));

        public static implicit operator IonValueWrapper(IonSexp.Initializer value)
            => new IonValueWrapper(new IonSexp(value));


        public static implicit operator IonValueWrapper(IonBool value)
            => new IonValueWrapper(value);

        public static implicit operator IonValueWrapper(IonInt value)
            => new IonValueWrapper(value);

        public static implicit operator IonValueWrapper(IonFloat value)
            => new IonValueWrapper(value);

        public static implicit operator IonValueWrapper(IonDecimal value)
            => new IonValueWrapper(value);

        public static implicit operator IonValueWrapper(IonTimestamp value)
            => new IonValueWrapper(value);

        public static implicit operator IonValueWrapper(IonString value)
            => new IonValueWrapper(value);

        public static implicit operator IonValueWrapper(IonTextSymbol value)
            => new IonValueWrapper(value);

        public static implicit operator IonValueWrapper(IonOperator value)
            => new IonValueWrapper(value);

        public static implicit operator IonValueWrapper(IonClob value)
            => new IonValueWrapper(value);

        public static implicit operator IonValueWrapper(IonBlob value)
            => new IonValueWrapper(value);

        public static implicit operator IonValueWrapper(IonSexp value)
            => new IonValueWrapper(value);

        public static implicit operator IonValueWrapper(IonList value)
            => new IonValueWrapper(value);

        public static implicit operator IonValueWrapper(IonStruct value)
            => new IonValueWrapper(value);
        #endregion
    }
}
