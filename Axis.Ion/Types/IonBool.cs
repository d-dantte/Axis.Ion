﻿using Axis.Luna.Extensions;
using System;
using System.Linq;
using static Axis.Luna.Extensions.Common;

namespace Axis.Ion.Types
{
    public readonly struct IonBool :
        IStructValue<bool>,
        IIonDeepCopyable<IonBool>,
        IIonNullable<IonBool>
    {
        private readonly IIonValue.Annotation[] _annotations;

        public bool? Value { get; }

        public IonTypes Type => IonTypes.Bool;

        public IIonValue.Annotation[] Annotations => _annotations?.ToArray() ?? Array.Empty<IIonValue.Annotation>();

        public IonBool(bool? value, params IIonValue.Annotation[] annotations)
        {
            Value = value;
            _annotations = annotations
                .Validate()
                .ToArray();
        }

        /// <summary>
        /// Creates a null instance of the <see cref="IonBool"/>
        /// </summary>
        /// <param name="annotations">any available annotation</param>
        /// <returns>The newly created null instance</returns>
        public static IonBool Null(params IIonValue.Annotation[] annotations) => new IonBool(null, annotations);


        #region IIonType

        public bool IsNull => Value == null;

        public bool ValueEquals(IStructValue<bool> other) => Value.NullOrEquals(other.Value);

        public string ToIonText() => Value?.ToString() ?? "null.bool";

        #endregion

        #region Record Implementation
        public override int GetHashCode()
            => HashCode.Combine(Value, ValueHash(Annotations.HardCast<IIonValue.Annotation, object>()));

        public override bool Equals(object? obj)
        {
            return obj is IonBool other
                && other.ValueEquals(this)
                && other.Annotations.SequenceEqual(Annotations);
        }

        public override string ToString() => Annotations
            .Select(a => a.ToString())
            .Concat(ToIonText())
            .JoinUsing("");


        public static bool operator ==(IonBool first, IonBool second) => first.Equals(second);

        public static bool operator !=(IonBool first, IonBool second) => !first.Equals(second);

        #endregion

        #region IIonDeepCopy<>
        IIonValue IIonDeepCopyable<IIonValue>.DeepCopy() => DeepCopy();

        public IonBool DeepCopy() => new IonBool(Value, Annotations);
        #endregion

        public static implicit operator IonBool(bool? value) => new IonBool(value);
    }
}
