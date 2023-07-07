using Axis.Luna.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static Axis.Ion.Types.IIonValue;
using static Axis.Ion.Types.IonStruct;
using static Axis.Luna.Extensions.Common;

namespace Axis.Ion.Types
{
    public class IonSexp :
        IIonContainer<IIonValue>,
        IIonDeepCopyable<IonSexp>,
        IIonNullable<IonSexp>
    {
        private readonly Annotation[]? _annotations;
        private readonly List<IIonValue>? _items;

        #region Construction
        /// <summary>
        /// Creates a default (null) list
        /// </summary>
        public IonSexp()
        : this((Annotation[]?)null, null)
        {
        }

        /// <summary>
        /// Creates a default (null) instance with the given annotations
        /// </summary>
        /// <param name="annotations"></param>
        public IonSexp(params Annotation[] annotations)
        : this(annotations, null)
        {
        }

        /// <summary>
        /// Creates an instance instantiated with the given initial properties
        /// </summary>
        /// <param name="items"></param>
        public IonSexp(params IIonValue[] items)
        : this(null, items)
        {
        }

        /// <summary>
        /// Creates an instance with the given annotations and items. If <paramref name="items"/> is null,
        /// this becomes a default (null) instance.
        /// </summary>
        /// <param name="annotations"></param>
        /// <param name="items"></param>
        public IonSexp(
            Annotation[]? annotations,
            params IIonValue[]? items)
        {
            _annotations = annotations?.ToArray();
            _items = items?
                .ThrowIfAny(
                    _item => _item is null,
                    _ => new ArgumentException($"'{nameof(items)} must not contain null values'"))
                .ToList();
        }

        /// <summary>
        /// Creates an instance from the given initializer
        /// </summary>
        /// <param name="initializer"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public IonSexp(Initializer initializer)
        : this(initializer?.Annotations ?? throw new ArgumentNullException(nameof(initializer)),
              initializer.Items.ToArray())
        {
        }
        #endregion

        #region IIonValue

        public IonTypes Type => IonTypes.Sexp;

        public bool IsNull => _items is null;

        public Annotation[] Annotations => _annotations?.ToArray() ?? Array.Empty<Annotation>();

        public string ToIonText()
        {
            if (IsNull)
                return "null.list";

            return Value!
                .Select(v => v.ToString())
                .JoinUsing(", ")
                .WrapIn("(", ")");
        }
        #endregion

        #region IIonContainer

        public IIonValue[]? Value => _items?.ToArray();

        public bool ValueEquals(IRefValue<IIonValue[]> other)
        {
            return Value.NullOrTrue(other?.Value, Enumerable.SequenceEqual);
        }

        public static IonSexp Empty(params Annotation[] annotations) => new IonSexp(annotations);

        static IIonContainer<IIonValue> IIonContainer<IIonValue>.Empty(params Annotation[] annotations) => Empty(annotations);
        #endregion

        #region DeepCopyable
        public IonSexp DeepCopy()
        {
            return IsNull
                ? new IonSexp(Annotations)
                : Value!
                    .Select(item => item.DeepCopy())
                    .ToArray()
                    .ApplyTo(items => new IonSexp(Annotations, items));
        }

        IIonValue IIonDeepCopyable<IIonValue>.DeepCopy() => DeepCopy();
        #endregion

        #region IIonNullable
        /// <summary>
        /// Creates a null instance of the <see cref="IonSexp"/>
        /// </summary>
        /// <param name="annotations">any available annotation</param>
        /// <returns>The newly created null instance</returns>
        public static IonSexp Null(params IIonValue.Annotation[] annotations) => new IonSexp(annotations);
        #endregion

        #region Members
        public IIonValue this[int index]
        {
            get => _items is not null
                ? _items[index]
                : throw new InvalidOperationException("Attempting to retrieve item of 'null.list'");

            set
            {
                if (_items is not null)
                    _items[index] = value;

                else throw new InvalidOperationException("Attempting to set item of 'null.list'");
            }
        }

        public IonSexp Add(IIonValue value)
        {
            if (_items is null)
                throw new InvalidOperationException("Attempting to add item of 'null.list'");

            _items.Add(value);
            return this;
        }

        public int Count => _items?.Count
            ?? throw new InvalidOperationException("Attempting to query item count of 'null.list'");
        #endregion

        #region Record
        public override string ToString()
            => Annotations
                .Select(a => a.ToString())
                .Concat(ToIonText())
                .JoinUsing("");

        public override int GetHashCode()
            => HashCode.Combine(
                ValueHash(Value?.HardCast<IIonValue, object>() ?? Enumerable.Empty<object>()),
                ValueHash(Annotations.HardCast<Annotation, object>()));

        public override bool Equals(object? obj)
        {
            return obj is IonSexp other
                && other.ValueEquals(this)
                && other.Annotations.SequenceEqual(Annotations);
        }

        public static bool operator ==(IonSexp left, IonSexp right) => left.NullOrEquals(right);
        public static bool operator !=(IonSexp left, IonSexp right) => !(left == right);
        #endregion

        public static implicit operator IonSexp(Initializer initializer) => new IonSexp(initializer);
        public static implicit operator IonSexp(IIonValue[] items) => new IonSexp(items);

        #region Nested Types

        /// <summary>
        /// The Initializer for structs
        /// </summary>
        public record Initializer : IEnumerable<IonValueWrapper>
        {
            internal Annotation[] Annotations;
            internal List<IIonValue> Items;

            public int Count => Items.Count;

            public IIonValue[] Values => Items.ToArray();

            public Initializer()
            : this((Annotation[]?)null, null)
            {
            }

            public Initializer(params Annotation[] annotations)
            : this(annotations, null)
            {
            }

            public Initializer(params IIonValue[] items)
            : this(null, items)
            {
            }

            public Initializer(
                Annotation[]? annotations,
                params IIonValue[]? items)
            {
                Annotations = annotations?.ToArray() ?? Array.Empty<Annotation>();
                Items = items?
                    .ToList()
                    ?? new List<IIonValue>();
            }

            #region Initializer
            public IEnumerator<IonValueWrapper> GetEnumerator() => Items.Select(v => v.Wrap()).GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public void Add(IonValueWrapper valueWrapper)
                => Items.Add(
                    valueWrapper.Value.ThrowIfNull(
                        new ArgumentException($"Invalid {nameof(IonValueWrapper)} value")));
            #endregion
        }
        #endregion
    }
}
