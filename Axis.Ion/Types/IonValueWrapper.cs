using Axis.Luna.Extensions;
using System;

namespace Axis.Ion.Types
{
    /// <summary>
    /// PS: this struct should NEVER accept null IIonTypes. If the intent is to pass an IonNull, it should be deliberate
    /// <para>
    /// Note that <see cref="IIonType"/>s that have no direct conversion to c# primitive types will not have implicit conversions
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
        public IIonType Value { get; }

        public IonValueWrapper(IIonType value)
        {
            Value = value.ThrowIfNull(new ArgumentNullException(nameof(value)));
        }


        #region Implicit operators

        public static implicit operator IonValueWrapper(bool? value)
            => new IonValueWrapper(new IonBool(value));

        public static implicit operator IonValueWrapper(long? value)
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
        #endregion
    }
}
