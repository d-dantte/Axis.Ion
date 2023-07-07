using Axis.Luna.Extensions;
using System;
using System.Linq;
using System.Text;
using static Axis.Luna.Extensions.Common;

namespace Axis.Ion.Types
{
    /// <summary>
    /// Represents a stream of ASCII encoded bytes of characters
    /// </summary>
    public readonly struct IonClob :
        IRefValue<byte[]>,
        IIonDeepCopyable<IonClob>,
        IIonNullable<IonClob>
    {
        private readonly IIonValue.Annotation[] _annotations;
        private readonly byte[]? _clob;

        public byte[]? Value => _clob?.ToArray();

        public IonTypes Type => IonTypes.Clob;

        public IIonValue.Annotation[] Annotations => _annotations?.ToArray() ?? Array.Empty<IIonValue.Annotation>();

        public IonClob(byte[]? value, params IIonValue.Annotation[] annotations)
        {
            _clob = value;
            _annotations = annotations
                .Validate()
                .ToArray();
        }

        /// <summary>
        /// Creates a null instance of the <see cref="IonClob"/>
        /// </summary>
        /// <param name="annotations">any available annotation</param>
        /// <returns>The newly created null instance</returns>
        public static IonClob Null(params IIonValue.Annotation[] annotations) => new IonClob(null, annotations);


        #region IIonType

        public bool IsNull => Value == null;

        public bool ValueEquals(IRefValue<byte[]> other)
            => other.Value.NullOrTrue(Value, Enumerable.SequenceEqual);

        public string ToIonText()
        {
            if (_clob is null)
                return "null.clob";

            return _clob
                .ApplyTo(Encoding.ASCII.GetString)
                .Replace(Environment.NewLine, "\n")
                .Split('\n')
                .Select(x => x.WrapIn("'''"))
                .JoinUsing(Environment.NewLine)
                .WrapIn("{{ ", " }}");
        }

        #endregion

        #region Record Implementation
        public override int GetHashCode()
            => HashCode.Combine(Value, ValueHash(Annotations.HardCast<IIonValue.Annotation, object>()));

        public override bool Equals(object? obj)
        {
            return obj is IonClob other
                && other.ValueEquals(this)
                && other.Annotations.SequenceEqual(Annotations);
        }

        public override string ToString() => Annotations
            .Select(a => a.ToString())
            .Concat(ToIonText())
            .JoinUsing("");


        public static bool operator ==(IonClob first, IonClob second) => first.Equals(second);

        public static bool operator !=(IonClob first, IonClob second) => !first.Equals(second);

        #endregion

        #region IIonDeepCopy<>
        IIonValue IIonDeepCopyable<IIonValue>.DeepCopy() => DeepCopy();

        public IonClob DeepCopy() => new IonClob(Value, Annotations);
        #endregion

        public static implicit operator IonClob(byte[]? value) => new IonClob(value);
    }
}
