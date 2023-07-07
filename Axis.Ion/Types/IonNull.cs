using Axis.Luna.Extensions;
using System;
using System.Linq;
using static Axis.Luna.Extensions.Common;

namespace Axis.Ion.Types
{
    public readonly struct IonNull :
        IIonValue,
        IIonDeepCopyable<IonNull>,
        IIonNullable<IonNull>
    {
        private readonly IIonValue.Annotation[] _annotations;

        public IonTypes Type => IonTypes.Null;

        public IIonValue.Annotation[] Annotations => _annotations?.ToArray() ?? Array.Empty<IIonValue.Annotation>();

        public IonNull(params IIonValue.Annotation[] annotations)
        {
            _annotations = annotations
                .Validate()
                .ToArray();
        }

        /// <summary>
        /// Creates a null instance of the <see cref="IonNull"/>
        /// </summary>
        /// <param name="annotations">any available annotation</param>
        /// <returns>The newly created null instance</returns>
        public static IonNull Null(params IIonValue.Annotation[] annotations) => new IonNull(annotations);

        #region IIonType

        public bool IsNull => true;

        public string ToIonText() => "null.null";

        #endregion

        #region Record Implementation
        public override int GetHashCode()
            => HashCode.Combine(ValueHash(Annotations.HardCast<IIonValue.Annotation, object>()));

        public override bool Equals(object? obj)
        {
            return obj is IonNull other
                && other.Annotations.SequenceEqual(Annotations);
        }

        public override string ToString() => Annotations
            .Select(a => a.ToString())
            .Concat("null.null")
            .JoinUsing("");


        public static bool operator ==(IonNull first, IonNull second) => first.Equals(second);

        public static bool operator !=(IonNull first, IonNull second) => !first.Equals(second);

        #endregion

        #region IIonDeepCopy<>
        IIonValue IIonDeepCopyable<IIonValue>.DeepCopy() => DeepCopy();

        public IonNull DeepCopy() => new IonNull(Annotations);
        #endregion
    }
}
