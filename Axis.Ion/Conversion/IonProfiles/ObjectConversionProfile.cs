using Axis.Ion.Conversion.ClrReflection;
using Axis.Ion.Types;
using Axis.Luna.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Axis.Ion.Conversion.IonProfiles
{
    /// <summary>
    /// Convertion profiles are specialized <see cref="IConverter"/> instances that can convert complex (non-primitive) types to and from ion
    /// </summary>
    internal class ObjectConversionProfile : IConversionProfile
    {
        private readonly Type targetType;
        private readonly ConstructorReflectionInfo[] constructorProfiles;
        private readonly PropertyReflectionInfo[] propertyProfiles;

        public ObjectConversionProfile(Type targetType)
        {
            this.targetType = ValidateType(targetType);

            // constructor profiles
            constructorProfiles = targetType
                .GetConstructors()
                .Where(ctor => ctor.IsPublic)
                .Where(ctor => ConstructorReflectionInfo.IsNormalizable(ctor))
                .Select(ctor => new ConstructorReflectionInfo(ctor))
                .ToArray();

            // property profiles
            propertyProfiles = targetType
                .GetProperties()
                .Where(prop => PropertyReflectionInfo.IsProfileable(prop))
                .Select(property => new PropertyReflectionInfo(property))
                .ToArray();
        }

        public bool CanConvert(Type type) => targetType.Equals(type);

        public IIonType ToIon(Type type, object? instance, ConversionOptions options)
        {
            IConversionProfile.ValidateSourceTypeCompatibility(type, instance?.GetType());

            if (instance is null)
                return IIonType.NullOf(IonTypes.Struct);

            var ionStruct = propertyProfiles
                .Where(profile => profile.Getter is not null)
                .Select(profile => new IonStruct.Property(
                    profile.Member.Name,
                    Ionizer.ToIon(
                        profile.Member.PropertyType,
                        profile.Getter.Invoke(instance, Array.Empty<object>()),
                        options)))
                .ApplyTo(properties => new IonStruct.Initializer(
                    Array.Empty<IIonType.Annotation>(),
                    properties.ToArray()))
                .ApplyTo(initializer => new IonStruct(initializer));

            return ionStruct;
        }

        public object? FromIon(Type type, IIonType ion, ConversionOptions options)
        {
            if (ion.Type != IonTypes.Struct)
                throw new ArgumentException($"Invalid ion type: {ion.Type}, expected: {IonTypes.Struct}");

            var @struct = (IonStruct)ion;

            // Construct
            (var value, var uninitializedProperties) = ConstructType(@struct, options);

            // Initialize
            return InitializeProperties(value, uninitializedProperties, options);
        }

        private Type ValidateType(Type type)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(targetType));

            if (!type.IsClass && !type.IsValueType)
                throw new ArgumentException("Supplied type must be a Class or Struct");

            if (type.IsAbstract)
                throw new ArgumentException(InvalidTypeError("an abstract type"));

            if (type.IsEnum)
                throw new ArgumentException(InvalidTypeError("an enum"));

            if (type.IsGenericTypeDefinition)
                throw new ArgumentException(InvalidTypeError("a generic type definition"));

            if (type.IsArray)
                throw new ArgumentException(InvalidTypeError("an array"));

            if (type.Extends(typeof(Delegate)))
                throw new ArgumentException(InvalidTypeError("a delegate"));

            return type;
        }

        private (object value, IonStruct.Property[] uninitializedProperties) ConstructType(
            IonStruct ion,
            ConversionOptions options)
        {
            (var constructor, var ionArguments) = SelectConstructor(ion);

            var argumentTypes = constructor.Member
                .GetParameters()
                .Select(p => p.ParameterType)
                .ToArray();

            var arguments = ionArguments
                .Select((prop, index) => Ionizer.FromIon(argumentTypes[index], prop.Value, options))
                .ToArray();

            var value = constructor.Invoker.New(arguments);

            var initializedProperties = ionArguments
                .Select(prop => prop.Name ?? throw new ArgumentNullException("Invalid property name: null"))
                .ApplyTo(props => new HashSet<IIonTextSymbol>(props));

            var uninitializedProperties = ion.Value
                .Where(prop => !initializedProperties.Contains(
                    prop.Name ?? throw new ArgumentNullException("Invalid property name: null")))
                .ToArray();

            return (value, uninitializedProperties);
        }

        private object InitializeProperties(
            object value,
            IonStruct.Property[] uninitializedProperties,
            ConversionOptions options)
        {
            var propertyMap = uninitializedProperties.ToDictionary(
                props => props.NameText ?? throw new ArgumentNullException($"Invalid property name: null"),
                props => props.Value);

            propertyProfiles
                .Where(profile => profile.Setter is not null)
                .Where(profile => propertyMap.ContainsKey(profile.Member.Name))
                .ForAll(profile =>
                {
                    var propertyValue = Ionizer.FromIon(
                        profile.Member.PropertyType,
                        propertyMap[profile.Member.Name],
                        options);

                    profile.Setter.Invoke(value, new[] { propertyValue });
                });

            return value;
        }

        private (ConstructorReflectionInfo constructor, IonStruct.Property[] arguments) SelectConstructor(IonStruct ion)
        {
            var profileInfo = constructorProfiles
                .OrderByDescending(profile => profile.Member.GetParameters().Length)
                .Select(profile => new
                {
                    CanConstruct = profile.CanConstruct(ion, out var args),
                    Profile = profile,
                    Arguments = args
                })
                .Where(info => info.CanConstruct)
                .FirstOrThrow(new InvalidOperationException($"No suitable constructor could be found"));

            return (profileInfo.Profile, profileInfo.Arguments);
        }

        private string InvalidTypeError(string param) => $"Supplied type must not be {param}";
    }
}
