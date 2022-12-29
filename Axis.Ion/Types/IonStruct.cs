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
        private readonly Dictionary<string, IIonType>? _properties;

        /// <summary>
        /// A list of the properties contained by this struct, in ascending order of name
        /// </summary>
        public Property[]? Value => _properties?
            .Select(kvp => new Property(kvp.Key, kvp.Value))
            .OrderBy(prop => prop.Name)
            .ToArray();

        public PropertyMap Properties => new PropertyMap(_properties);

        public IonTypes Type => IonTypes.Struct;

        public IIonType.Annotation[] Annotations => _annotations?.ToArray() ?? Array.Empty<IIonType.Annotation>();


        internal IonStruct(Initializer? initializer)
        {
            _annotations = initializer?.Annotations.ToArray();
            _properties = initializer != null
                ? new Dictionary<string, IIonType>(initializer.PropertyMap)
                : null;
        }

        internal IonStruct(params IIonType.Annotation[] annotations)
        {
            _annotations = annotations?.ToArray();
            _properties = null;
        }

        public IonStruct Default(params IIonType.Annotation[] annotations) => new IonStruct(annotations);

        #region IIonValueType

        public bool IsNull => Value == null;

        public bool ValueEquals(IIonValueType<IonStruct.Property[]?> other)
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
                    .OrderBy(p => p.Name)
                    .All(p =>
                        thisProperties?.TryGetValue(p.Name, out var value) == true
                        && value.NullOrEquals(p.Value) == true);
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
            private readonly IIonSymbol? _propertyName;

            /// <summary>
            /// The property's name. Note that this can either be a proper string, or similar to <see cref="IIonSymbol.Identifier"/>
            /// </summary>
            public string Name => _propertyName?.ToString() ?? "";

            /// <summary>
            /// The property's value. This is never null, unless when the <see cref="Property"/> is a default values
            /// </summary>
            public IIonType Value { get; }

            internal Property(string name, IIonType value)
            {
                Value = value ?? throw new ArgumentNullException(nameof(value));
                _propertyName = IIonSymbol
                    .Of(name)
                    .ThrowIf(
                        s => s is IIonSymbol.Operator,
                        new ArgumentException($"Invalid property name: {name}"));
            }

            internal Property(IIonSymbol.Identifier name, IIonType value)
            {
                _propertyName = name.ThrowIfDefault(new ArgumentException(nameof(name)));
                Value = value ?? throw new ArgumentNullException(nameof(value));
            }

            internal Property(IIonSymbol.QuotedSymbol name, IIonType value)
            {
                _propertyName = name.ThrowIfDefault(new ArgumentException(nameof(name)));
                Value = value ?? throw new ArgumentNullException(nameof(value));
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
        /// 
        /// </summary>
        public struct PropertyMap : IIndexer<string, IIonType>
        {
            private readonly Dictionary<string, IIonType> _properties;

            /// <summary>
            /// 
            /// </summary>
            public IEnumerable<string> Names => _properties.Keys;

            /// <summary>
            /// 
            /// </summary>
            public IEnumerable<IIonType> Values => _properties.Values;

            /// <summary>
            /// 
            /// </summary>
            public int Count => _properties.Count;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="propertyName"></param>
            /// <returns></returns>
            public IIonType this[string propertyName]
            {
                get => _properties[propertyName];
                set => _properties[propertyName.ValidatePropertyName()] = value.ThrowIfNull(new ArgumentNullException(nameof(value)));
            }


            internal PropertyMap(Dictionary<string, IIonType>? properties)
            {
                _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="propertyName"></param>
            /// <returns></returns>
            public bool Contains(string propertyName) => _properties.ContainsKey(propertyName);

            /// <summary>
            /// 
            /// </summary>
            /// <param name="propertyName"></param>
            /// <param name="value"></param>
            /// <returns></returns>
            public bool Add(string propertyName, IIonType value)
                => _properties.TryAdd(
                    propertyName.ValidatePropertyName(),
                    value.ThrowIfNull(new ArgumentNullException(nameof(value))));

            /// <summary>
            /// 
            /// </summary>
            /// <param name="propertyName"></param>
            /// <param name="value"></param>
            /// <returns></returns>
            public bool Remove(string propertyName, out IIonType? value)
                => _properties.Remove(propertyName, out value);
        }

        /// <summary>
        /// 
        /// </summary>
        public class Initializer : IWriteonlyIndexer<string, IonValueWrapper>
        {
            internal IIonType.Annotation[] Annotations;
            internal Dictionary<string, IIonType> PropertyMap;

            public int Count => PropertyMap.Count;

            public Property[] Properties => PropertyMap
                .Select(kvp => new Property(kvp.Key, kvp.Value))
                .OrderBy(prop => prop.Name)
                .ToArray();

            public Initializer(IIonType.Annotation[] annotations, params Property[] values)
            {
                Annotations = annotations ?? throw new ArgumentNullException(nameof(annotations));
                PropertyMap = values?
                    .ToDictionary(p => p.Name, p => p.Value)
                    ?? new Dictionary<string, IIonType>();
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
            /// 
            /// </summary>
            /// <param name="key"></param>
            /// <returns></returns>
            public IonValueWrapper this[string key] 
            {
                set => PropertyMap[key.ValidatePropertyName()] = value.Value.ThrowIfNull(
                    new ArgumentException($"Invalid {nameof(IonValueWrapper)} value"));
            }
        }
        #endregion
    }
}
