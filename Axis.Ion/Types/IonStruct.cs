using Axis.Luna.Common.Results;
using Axis.Luna.Common.Utils;
using Axis.Luna.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using static Axis.Ion.Types.IIonValue;
using static Axis.Luna.Extensions.Common;

namespace Axis.Ion.Types
{
    public class IonStruct :
        IIonContainer<IonStruct.Property>,
        IIonDeepCopyable<IonStruct>,
        IIonNullable<IonStruct>
    {
        private readonly Annotation[]? _annotations;
        private readonly Dictionary<IonTextSymbol, IIonValue>? _properties;

        #region Construction
        /// <summary>
        /// Creates a default (null) struct
        /// </summary>
        public IonStruct()
        : this((Annotation[]?)null, null)
        {
        }

        /// <summary>
        /// Creates a default (null) instance with the given annotations
        /// </summary>
        /// <param name="annotations"></param>
        public IonStruct(params Annotation[] annotations)
        : this(annotations, null)
        {
        }

        /// <summary>
        /// Creates an instance instantiated with the given initial properties
        /// </summary>
        /// <param name="properties"></param>
        public IonStruct(params Property[] properties)
        : this(null, properties)
        {
        }

        /// <summary>
        /// Creates an instance with the given annotations and properties. If <paramref name="properties"/> is null,
        /// this becomes a default (null) instance.
        /// </summary>
        /// <param name="annotations"></param>
        /// <param name="properties"></param>
        public IonStruct(
            Annotation[]? annotations,
            params Property[]? properties)
        {
            _annotations = annotations?.ToArray();
            _properties = properties?.ToDictionary(prop => prop.Name, prop => prop.Value);
        }

        /// <summary>
        /// Creates an instance from the given initializer
        /// </summary>
        /// <param name="initializer"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public IonStruct(Initializer initializer)
        : this(initializer?.Annotations ?? throw new ArgumentNullException(nameof(initializer)),
              initializer.Properties)
        {

        }
        #endregion

        #region IIonValue

        public IonTypes Type => IonTypes.Struct;

        public bool IsNull => _properties is null;

        public Annotation[] Annotations => _annotations?.ToArray() ?? Array.Empty<Annotation>();

        public string ToIonText()
        {
            if (IsNull)
                return "null.struct";

            return Value!
                .Select(v => v.ToString())
                .JoinUsing(", ")
                .WrapIn("{", "}");
        }
        #endregion

        #region IIonContainer

        public Property[]? Value => _properties?
            .Select(kvp => new Property(kvp.Key, kvp.Value))
            .ToArray();

        public bool ValueEquals(IRefValue<Property[]> other)
        {
            if (other == null)
                return false;

            var otherProperties = other.Value;

            if (otherProperties == null && IsNull)
                return true;

            if (otherProperties == null ^ IsNull)
                return false;

            if (_properties!.Count == otherProperties!.Length)
            {
                return otherProperties.All(prop =>
                {
                    return _properties!.TryGetValue(prop.Name, out var value)
                        && value.NullOrEquals(prop.Value) == true;
                });
            }

            return false;
        }

        public static IonStruct Empty(params Annotation[] annotations) => new IonStruct(annotations);

        static IIonContainer<Property> IIonContainer<Property>.Empty(params Annotation[] annotations) => Empty(annotations);
        #endregion

        #region DeepCopyable
        public IonStruct DeepCopy()
        {
            return IsNull
                ? new IonStruct(Annotations)
                : Value!
                    .Select(property => new Property(property.Name!, property.Value.DeepCopy()))
                    .ToArray()
                    .ApplyTo(properties => new IonStruct(Annotations, properties));
        }

        IIonValue IIonDeepCopyable<IIonValue>.DeepCopy() => DeepCopy();
        #endregion

        #region IIonNullable
        /// <summary>
        /// Creates a null instance of the <see cref="IonList"/>
        /// </summary>
        /// <param name="annotations">any available annotation</param>
        /// <returns>The newly created null instance</returns>
        public static IonStruct Null(params IIonValue.Annotation[] annotations) => new IonStruct(annotations);
        #endregion

        #region Members
        public IIonValue this[string propertyName]
        {
            get => _properties is not null
                ? _properties[propertyName]
                : throw new InvalidOperationException("Attempting to retrieve property of 'null.struct'");

            set
            {
                if (_properties is not null)
                    _properties[propertyName] = value ?? throw new ArgumentNullException(nameof(value));

                else throw new InvalidOperationException("Attempting to set property of 'null.struct'");
            }
        }

        public bool TryGetValue(string propertyName, out IIonValue? value)
        {
            return _properties is not null
                ? _properties.TryGetValue(propertyName, out value)
                : throw new InvalidOperationException("Attempting to retrieve property of 'null.struct'");
        }

        public bool ContainsProperty(string propertyName)
        {
            return _properties is not null
                ? _properties.ContainsKey(propertyName)
                : throw new InvalidOperationException("Attempting to query property of 'null.struct'");
        }

        public IIonValue GetOrAdd(string propertyName, Func<IonTextSymbol, IIonValue> valueProducer)
        {
            if (valueProducer is null)
                throw new ArgumentNullException(nameof(valueProducer));

            return _properties is not null
                ? _properties.GetOrAdd((IonTextSymbol)propertyName, valueProducer)
                : throw new InvalidOperationException("Attempting to set property of 'null.struct'");
        }

        public IonStruct Add(string propertyName, IIonValue value)
        {
            if (_properties is null)
                throw new InvalidOperationException("Attempting to set property of 'null.struct'");

            this[propertyName] = value;
            return this;
        }

        public bool TryRemove(string propertyName, out IIonValue? value)
        {
            if (_properties is null)
                throw new InvalidOperationException("");

            return _properties.Remove(propertyName, out value);
        }

        public int Count => _properties?.Count
            ?? throw new InvalidOperationException("Attempting to query property count of 'null.struct'");
        #endregion

        #region Record
        public override string ToString()
            => Annotations
                .Select(a => a.ToString())
                .Concat(ToIonText())
                .JoinUsing("");

        public override int GetHashCode()
            => HashCode.Combine(
                ValueHash(Value?.HardCast<Property, object>() ?? Enumerable.Empty<object>()),
                ValueHash(Annotations.HardCast<Annotation, object>()));

        public override bool Equals(object? obj)
        {
            return obj is IonStruct other
                && other.ValueEquals(this)
                && other.Annotations.SequenceEqual(Annotations);
        }

        public static bool operator ==(IonStruct left, IonStruct right) => left.NullOrEquals(right);
        public static bool operator !=(IonStruct left, IonStruct right) => !(left == right);
        #endregion

        public static implicit operator IonStruct(Initializer initializer) => new IonStruct(initializer);
        public static implicit operator IonStruct(Property[] properties) => new IonStruct(properties);

        #region Nested Types

        /// <summary>
        /// Name-Value pair for the struct. The name is a <see cref="IonTextSymbol"/> devoid of any annotations.
        /// </summary>
        public readonly record struct Property
        {
            public IonTextSymbol Name { get; }

            public IIonValue Value { get; }

            public Property(IonTextSymbol name, IIonValue value)
            {
                Value = value ?? throw new ArgumentNullException(nameof(value));
                Name = name
                    .ThrowIf(
                        _name => _name.IsNull(),
                        new ArgumentException($"{nameof(name)} cannot be '{name.ToIonText()}'"))
                    .ApplyTo(_name => new IonTextSymbol(_name.Value));
            }

            public override string ToString() => $"{Name}: {Value}";
        }

        /// <summary>
        /// The Initializer for structs
        /// </summary>
        public record Initializer : IWriteonlyIndexer<string, IonValueWrapper>
        {
            internal Annotation[] Annotations;
            internal Dictionary<IonTextSymbol, IIonValue> PropertyMap;

            public int Count => PropertyMap.Count;

            public Property[] Properties => PropertyMap
                .Select(kvp => new Property(kvp.Key, kvp.Value))
                .OrderBy(prop => prop.Name.ToIonText())
                .ToArray();

            public Initializer()
            : this((Annotation[]?)null, null)
            {
            }

            public Initializer(string annotations)
            : this(Annotation.ParseCollection(annotations).Resolve(), null)
            {
            }

            public Initializer(params Annotation[] annotations)
            : this(annotations, null)
            {
            }

            public Initializer(params Property[] properties)
            : this(null, properties)
            {
            }

            public Initializer(
                Annotation[]? annotations,
                params Property[]? properties)
            {
                Annotations = annotations?.ToArray() ?? Array.Empty<Annotation>();
                PropertyMap = properties?
                    .ToDictionary(prop => prop.Name, prop => prop.Value)
                    ?? new Dictionary<IonTextSymbol, IIonValue>();
            }

            /// <summary>
            /// Gets the <see cref="IonValueWrapper"/> for the given key symbol
            /// </summary>
            /// <param name="key">the key</param>
            /// <returns>the value mapped to the key</returns>
            public IonValueWrapper this[string key]
            {
                set => PropertyMap[key] = value.Value.ThrowIfNull(
                    new ArgumentException($"Invalid {nameof(IonValueWrapper)} value"));
            }
        }
        #endregion
    }
}
