using Axis.Luna.Common.Utils;
using Axis.Luna.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static Axis.Luna.Extensions.Common;

namespace Axis.Ion.Types
{
    public readonly struct IonSexp : IIonConainer<IIonType>, IReadonlyIndexer<int, IIonType>
    {
        private readonly IIonType.Annotation[]? _annotations;
        private readonly List<IIonType>? _elements;

        public IIonType[]? Value => _elements?.ToArray();

        public List<IIonType>? Items => _elements;

        public IonTypes Type => IonTypes.Sexp;

        public IIonType.Annotation[] Annotations => _annotations?.ToArray() ?? Array.Empty<IIonType.Annotation>();

        public int Count => _elements?.Count ?? -1;

        public IIonType this[int key]
        {
            get => _elements?[key] ?? throw new InvalidOperationException($"Cannot read from the default {nameof(IonList)}");
        }

        public IonSexp(Initializer? initializer)
        {
            _annotations = initializer?.Annotations
                .Validate()
                .ToArray();
            _elements = initializer?.Elements.ToList();
        }

        public IonSexp(params IIonType.Annotation[] annotations)
        {
            _annotations = annotations.Validate().ToArray();
            _elements = null;
        }

        /// <summary>
        /// Creates a null instance of the <see cref="IonSexp"/>
        /// </summary>
        /// <param name="annotations">any available annotation</param>
        /// <returns>The newly created null instance</returns>
        public static IonSexp Null(params IIonType.Annotation[] annotations) => new IonSexp(annotations);

        #region IIonValueType

        public bool IsNull => Value == null;

        public bool ValueEquals(IRefValue<IIonType[]> other)
            => Value.NullOrTrue(other?.Value, Enumerable.SequenceEqual);

        public string ToIonText()
        {
            return Value?
                .Select(x => x.ToString())
                .JoinUsing(", ")
                .WrapIn("(", ")")
                ?? "null.sexp";
        }

        #endregion

        #region Record Implementation
        public override int GetHashCode()
            => HashCode.Combine(
                ValueHash(Value?.HardCast<IIonType, object>() ?? Enumerable.Empty<object>()),
                ValueHash(Annotations.HardCast<IIonType.Annotation, object>()));

        public override bool Equals(object? obj)
        {
            return obj is IonSexp other
                && other.ValueEquals(this)
                && other.Annotations.SequenceEqual(Annotations);
        }

        public override string ToString()
            => Annotations
                .Select(a => a.ToString())
                .Concat(ToIonText())
                .JoinUsing("");


        public static bool operator ==(IonSexp first, IonSexp second) => first.Equals(second);

        public static bool operator !=(IonSexp first, IonSexp second) => !first.Equals(second);

        #endregion

        public static implicit operator IonSexp(Initializer? initializer) => new IonSexp(initializer);

        public static implicit operator IonSexp(IIonType[]? elements)
            => new IonSexp(
                elements != null
                    ? new Initializer(Array.Empty<IIonType.Annotation>(), elements)
                    : null);

        #region Nested types

        /// <summary>
        /// Declarative initializer for the <see cref="IonList"/>
        /// </summary>
        public class Initializer : IEnumerable<IonValueWrapper>
        {
            private readonly List<IIonType> _elements = new List<IIonType>();
            private readonly IIonType.Annotation[] _annotations;

            internal IIonType.Annotation[] Annotations => _annotations;

            internal IIonType[] Elements => _elements.ToArray();

            public int Count => _elements.Count;


            public Initializer(IIonType.Annotation[] annotations, params IIonType[] elements)
            {
                _annotations = annotations.Validate();
                _elements.AddRange(
                    elements
                        .ThrowIfNull(new ArgumentNullException(nameof(elements)))
                        .Select(e => e.ThrowIfNull(new InvalidOperationException($"{nameof(elements)} cannot contain null"))));
            }

            public Initializer(string annotations, params IIonType[] elements)
                : this(IIonType.Annotation.ParseCollection(annotations), elements)
            { }

            public Initializer()
                : this(Array.Empty<IIonType.Annotation>())
            { }

            #region initializer implementation
            public IEnumerator<IonValueWrapper> GetEnumerator() => _elements.Select(v => v.Wrap()).GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public void Add(IonValueWrapper valueWrapper)
                => _elements.Add(valueWrapper.Value.ThrowIfNull(new ArgumentException($"Invalid {nameof(IonValueWrapper)} value")));
            #endregion
        }

        #endregion
    }
}
