using Axis.Luna.Common.Utils;
using Axis.Luna.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using static Axis.Luna.Extensions.Common;
using static Axis.Luna.Extensions.ExceptionExtension;

namespace Axis.Ion.Types
{
    public readonly struct IonStruct: IIonConainer<IonStruct.Property>
    {
        private readonly IIonType.Annotation[]? _annotations;
        private readonly Dictionary<IIonTextSymbol, IIonType>? _properties;

        /// <summary>
        /// A list of the properties contained by this struct, in ascending order of name
        /// </summary>
        public Property[]? Value => _properties?
            .Select(kvp => new Property(kvp.Key, kvp.Value))
            .OrderBy(prop => prop.Name?.ToIonText() ?? throw new ArgumentException($"Invalid property: {prop}"))
            .ToArray();

        /// <summary>
        /// The property map
        /// </summary>
        public PropertyMap Properties => new PropertyMap(_properties);

        public IonTypes Type => IonTypes.Struct;

        public IIonType.Annotation[] Annotations => _annotations?.ToArray() ?? Array.Empty<IIonType.Annotation>();

        public IonStruct(Initializer? initializer)
        {
            _annotations = initializer?.Annotations.Validate().ToArray();
            _properties = initializer != null
                ? new Dictionary<IIonTextSymbol, IIonType>(initializer.PropertyMap)
                : null;
        }

        public IonStruct(params IIonType.Annotation[] annotations)
        {
            _annotations = annotations
                .Validate()
                .ToArray();
            _properties = null;
        }

        /// <summary>
        /// Creates a null instance of the <see cref="IonList"/>
        /// </summary>
        /// <param name="annotations">any available annotation</param>
        /// <returns>The newly created null instance</returns>
        public static IonStruct Null(params IIonType.Annotation[] annotations) => new IonStruct(annotations);

        /// <summary>
        /// Creates and returns an empty <see cref="IonList"/> instance.
        /// </summary>
        /// <param name="annotations">any available annotation</param>
        /// <returns>The newly created empty instance</returns>
        public static IonStruct Empty(params IIonType.Annotation[] annotations) => new Initializer(annotations);

        #region IIonValueType

        public bool IsNull => Value == null;

        public bool ValueEquals(IRefValue<Property[]> other)
        {
            if (other == null)
                return false;

            var otherProperties = other.Value;
            var thisProperties = _properties;

            if (otherProperties == null && thisProperties == null)
                return true;

            if (thisProperties?.Count == otherProperties?.Length)
            {
                return otherProperties
                    .Select(prop => (
                        Value: prop.Value,
                        Name: prop.Name ?? throw new InvalidOperationException("Property name cannot be null")))
                    .OrderBy(ptuple => ptuple.Name.ToIonText())
                    .All(ptuple =>
                    {
                        return thisProperties?.TryGetValue(ptuple.Name, out var value) == true
                        && value.NullOrEquals(ptuple.Value) == true;
                    });
            }

            return false;
        }

        public string ToIonText()
        {
            if (_properties == null)
                return "null.struct";

            return Value
                .Select(v => v.ToString())
                .JoinUsing(", ")
                .WrapIn("{", "}");
        }

        #endregion

        #region Record Implementation
        public override int GetHashCode()
            => HashCode.Combine(
                ValueHash(Value?.HardCast<Property, object>() ?? Enumerable.Empty<object>()),
                ValueHash(Annotations.HardCast<IIonType.Annotation, object>()));

        public override bool Equals(object? obj)
        {
            return obj is IonStruct other
                && other.ValueEquals(this)
                && other.Annotations.SequenceEqual(Annotations);
        }

        public override string ToString()
            => Annotations
                .Select(a => a.ToString())
                .Concat(ToIonText())
                .JoinUsing("");


        public static bool operator ==(IonStruct first, IonStruct second) => first.Equals(second);

        public static bool operator !=(IonStruct first, IonStruct second) => !first.Equals(second);

        #endregion

        public static implicit operator IonStruct(Initializer? initializer) => new IonStruct(initializer);


        #region Nested types

        /// <summary>
        /// 
        /// </summary>
        public readonly struct Property
        {
            private readonly IIonTextSymbol _propertyName;

            /// <summary>
            /// The property's name. Note that this can either be a proper string, or similar to <see cref="IIonSymbol.Identifier"/>
            /// <para>
            /// NOTE: consider making this non-nullable
            /// </para>
            /// </summary>
            public IIonTextSymbol? Name => _propertyName;

            /// <summary>
            /// The name of the property in string-text form
            /// <para>
            /// NOTE: consider making this non-nullable
            /// </para>
            /// </summary>
            public string? NameText => Name?.ToIonText();

            /// <summary>
            /// The property's value. This is never null, unless when the <see cref="Property"/> is a default values
            /// </summary>
            public IIonType Value { get; }

            public Property(string name, IIonType value)
            : this(IIonTextSymbol.Parse(name), value)
            {
            }

            public Property(IIonTextSymbol name, IIonType value)
            {
                Value = value ?? throw new ArgumentNullException(nameof(value));
                _propertyName = name
                    .ThrowIfNull(new ArgumentNullException(nameof(name)))
                    .ThrowIf(n => n.IsNull, new ArgumentException("Null-symbol is forbidden"));
            }

            public override int GetHashCode() => HashCode.Combine(_propertyName, Value);

            public override bool Equals(object? obj)
            {
                return obj is Property other
                    && other._propertyName.NullOrEquals(_propertyName)
                    && other.Value.Equals(Value);
            }

            public override string ToString()
            {
                if (Name is null)
                    return "<:>";

                return $"{Name}: {Value}";
            }

            public static bool operator ==(Property first, Property second) => first.Equals(second);
            public static bool operator !=(Property first, Property second) => !first.Equals(second);
        }

        /// <summary>
        /// Instances of this class are wrapped around the values of a struct and enable convenient key-value indexing
        /// of the structs properties
        /// </summary>
        public readonly struct PropertyMap : IIndexer<IIonTextSymbol, IIonType>
        {
            private readonly Dictionary<IIonTextSymbol, IIonType> _properties;

            /// <summary>
            /// Names of all properties in no particular order
            /// </summary>
            public IEnumerable<IIonTextSymbol> Names => _properties.Keys;

            /// <summary>
            /// Values of all properties in no particular order
            /// </summary>
            public IEnumerable<IIonType> Values => _properties.Values;

            /// <summary>
            /// Number of properties in the map
            /// </summary>
            public int Count => _properties.Count;

            /// <summary>
            /// Gets the value mapped to the given property name
            /// </summary>
            /// <param name="propertyName">The property name</param>
            /// <returns>The mapped value</returns>
            public IIonType this[IIonTextSymbol propertyName]
            {
                get => _properties[propertyName ?? throw new ArgumentNullException(nameof(propertyName))];
                set => _properties[propertyName ?? throw new ArgumentNullException(nameof(propertyName))]
                    = value.ThrowIfNull(new ArgumentNullException(nameof(value)));
            }

            /// <summary>
            /// Gets the value mapped to the given property name
            /// </summary>
            /// <param name="propertyName">The property name</param>
            /// <returns>The mapped value</returns>
            public IIonType this[string propertyName]
            {
                get => this[IIonTextSymbol.Parse(propertyName)];
                set => this[IIonTextSymbol.Parse(propertyName)] = value;
            }


            internal PropertyMap(Dictionary<IIonTextSymbol, IIonType>? properties)
            {
                _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            }

            /// <summary>
            /// Indicates if this map contains a property with the given name
            /// </summary>
            /// <param name="propertyName">The property name to check for</param>
            /// <returns>True if the property exists, false otherwise</returns>
            public bool Contains(IIonTextSymbol propertyName) => _properties.ContainsKey(propertyName);

            /// <summary>
            /// Indicates if this map contains a property with the given name
            /// </summary>
            /// <param name="propertyName">The property name to check for</param>
            /// <returns>True if the property exists, false otherwise</returns>
            public bool Contains(string propertyName) => _properties.ContainsKey(IIonTextSymbol.Parse(propertyName));

            /// <summary>
            /// Adds a new property to the map if one doesn't exist
            /// </summary>
            /// <param name="propertyName">The property name</param>
            /// <param name="value">The property value</param>
            /// <returns></returns>
            public bool Add(IIonTextSymbol propertyName, IIonType value)
                => _properties.TryAdd(
                    propertyName,
                    value.ThrowIfNull(new ArgumentNullException(nameof(value))));

            public bool Add(string propertyName, IIonType value) => Add(IIonTextSymbol.Parse(propertyName), value);

            /// <summary>
            /// Removes the property with the given name if it exists, and returns it's value
            /// </summary>
            /// <param name="propertyName">The name of the property</param>
            /// <param name="value">the returned value if the property exists</param>
            /// <returns>True if the property existed and was removed, false otherwise</returns>
            public bool Remove(IIonTextSymbol propertyName, out IIonType? value)
                => _properties.Remove(propertyName, out value);

            public bool Remove(string propertyName, out IIonType? value) => Remove(IIonTextSymbol.Parse(propertyName), out value);

            public bool TryGetvalue(IIonTextSymbol propertyName, out IIonType? value)
                => _properties.TryGetValue(propertyName, out value);

            public bool TryGetvalue(string propertyName, out IIonType? value)
                => TryGetvalue(IIonTextSymbol.Parse(propertyName), out value);
        }

        /// <summary>
        /// The Initializer for structs
        /// </summary>
        public class Initializer : IWriteonlyIndexer<string, IonValueWrapper>
        {
            internal IIonType.Annotation[] Annotations;
            internal Dictionary<IIonTextSymbol, IIonType> PropertyMap;

            public int Count => PropertyMap.Count;

            public Property[] Properties => PropertyMap
                .Select(kvp => new Property(kvp.Key, kvp.Value))
                .OrderBy(prop => prop.Name?.ToIonText() ?? throw new ArgumentNullException("Invalid property name: null"))
                .ToArray();

            public Initializer(IIonType.Annotation[] annotations, params Property[] values)
            {
                Annotations = annotations ?? throw new ArgumentNullException(nameof(annotations));
                PropertyMap = values?
                    .ToDictionary(
                        p => p.Name ?? throw new ArgumentNullException("Invalid property name: null"),
                        p => p.Value)
                    ?? new Dictionary<IIonTextSymbol, IIonType>();
            }

            public Initializer(string annotations, params Property[] values)
                : this(IIonType.Annotation.ParseCollection(annotations), values)
            { }

            public Initializer(params IIonType.Annotation[] annotations)
                : this(annotations, Array.Empty<Property>())
            { }

            public Initializer()
                : this(Array.Empty<IIonType.Annotation>(), Array.Empty<Property>())
            { }

            /// <summary>
            /// Gets the <see cref="IonValueWrapper"/> for the given key symbol
            /// </summary>
            /// <param name="key">the key</param>
            /// <returns>the value mapped to the key</returns>
            public IonValueWrapper this[IIonTextSymbol key]
            {
                set => PropertyMap[key] = value.Value.ThrowIfNull(
                    new ArgumentException($"Invalid {nameof(IonValueWrapper)} value"));
            }


            /// <summary>
            /// Gets the <see cref="IonValueWrapper"/> for the given key string
            /// </summary>
            /// <param name="key">the key</param>
            /// <returns>the value mapped to the key</returns>
            public IonValueWrapper this[string key]
            {
                set => this[IIonTextSymbol.Parse(key)] = value;
            }
        }
        #endregion
    }
}
