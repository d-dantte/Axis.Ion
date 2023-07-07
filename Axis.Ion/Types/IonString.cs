using Axis.Luna.Extensions;
using System;
using System.Linq;
using static Axis.Luna.Extensions.Common;

namespace Axis.Ion.Types
{
    public readonly struct IonString :
        IRefValue<string>,
        IIonDeepCopyable<IonString>,
        IIonNullable<IonString>
    {
        private readonly IIonValue.Annotation[] _annotations;

        public string? Value { get; }

        public IonTypes Type => IonTypes.String;

        public IIonValue.Annotation[] Annotations => _annotations?.ToArray() ?? Array.Empty<IIonValue.Annotation>();

        public IonString(string? value, params IIonValue.Annotation[] annotations)
        {
            Value = value;
            _annotations = annotations
                .Validate()
                .ToArray();
        }

        /// <summary>
        /// Creates a null instance of the <see cref="IonString"/>
        /// </summary>
        /// <param name="annotations">any available annotation</param>
        /// <returns>The newly created null instance</returns>
        public static IonString Null(params IIonValue.Annotation[] annotations) => new IonString(null, annotations);

        #region IIonType

        public bool IsNull => Value == null;

        public bool ValueEquals(IRefValue<string> other) => Value.NullOrEquals(other?.Value);

        public string ToIonText()
        {
            if (Value is null)
                return "null.string";

            return Value;
        }

        #endregion

        #region Record Implementation
        public override int GetHashCode()
            => HashCode.Combine(Value, ValueHash(Annotations.HardCast<IIonValue.Annotation, object>()));

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

        #region IIonDeepCopy<>
        IIonValue IIonDeepCopyable<IIonValue>.DeepCopy() => DeepCopy();

        public IonString DeepCopy() => new IonString(Value, Annotations);
        #endregion

        public static implicit operator IonString(string? value) => new IonString(value);
    }
}
