using Axis.Ion.Conversion.ClrReflection;
using Axis.Ion.Types;
using Axis.Luna.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Axis.Ion.Conversion.Converters
{
    /// <summary>
    /// Convertion profiles are specialized <see cref="IConverter"/> instances that can convert complex (non-primitive) types to and from ion
    /// </summary>
    internal class ObjectConverter : IConverter
    {
        public static readonly string ObjectRefPrefix = "@Ref-";

        private readonly Type targetType;
        private readonly ConstructorReflectionInfo[] constructorProfiles;
        private readonly PropertyReflectionInfo[] propertyProfiles;

        public ObjectConverter(Type targetType)
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

        #region IClrConverter
        public bool CanConvert(Type destinationType, IIonValue ion)
        {
            if (destinationType is null)
                throw new ArgumentNullException(nameof(destinationType));

            if (ion is null)
                throw new ArgumentNullException(nameof(ion));

            return
                IonTypes.Struct == ion.Type
                && targetType.Equals(destinationType);
        }

        public object? ToClr(Type type, IIonValue ion, ConversionContext context)
        {
            if (ion.Type != IonTypes.Struct)
                throw new ArgumentException($"Invalid ion type: {ion.Type}, expected: {IonTypes.Struct}");

            var @struct = (IonStruct)ion;

            // Construct
            (var value, var uninitializedProperties) = ConstructType(@struct, context);

            // Initialize
            return InitializeProperties(value, uninitializedProperties, context);
        }
        #endregion

        #region IIonConverter
        public bool CanConvert(Type sourceType, object? instance)
        {
            if (sourceType is null)
                throw new ArgumentNullException(nameof(sourceType));

            return targetType.Equals(instance?.GetType() ?? sourceType);
        }

        public IIonValue ToIon(Type sourceType, object? instance, ConversionContext context)
        {
            sourceType.ValidateCongruenceWith(instance?.GetType());

            if (instance is null)
                return IIonValue.NullOf(IonTypes.Struct);

            var ionStruct = propertyProfiles
                .Where(profile => profile.Getter is not null)
                .Select(profile =>
                {
                    if (profile.Member.DeclaringType is null)
                        throw new ArgumentException($"Invalid member DeclaringType for: {profile.Member}");

                    if (context.Options.IgnoredProperties.TryGetValue(profile.Member.DeclaringType, out var ignoredProps)
                        && ignoredProps.Contains(profile.Member))
                        return null;

                    var clrValue = profile.Getter!
                        .Invoke(instance, Array.Empty<object>());

                    if (NullValueBehavior.Ignore == context.Options.NullValueBehavior
                        && clrValue is null)
                        return null;

                    if (DefaultValueBehavior.Ignore == context.Options.DefaultValueBehavior
                        && profile.Member.PropertyType.DefaultValue().NullOrEquals(clrValue))
                        return null;

                    var ion = clrValue.ToIonValue(profile.Member.PropertyType, context);

                    if (ion is null)
                        return null;

                    return new IonStruct.Property(profile.Member.Name, ion).AsNullable();
                })
                .Where(structProperty => structProperty is not null)
                .Select(structProperty => structProperty ?? throw new ArgumentNullException(nameof(structProperty)))
                .ApplyTo(properties => new IonStruct.Initializer(
                    Array.Empty<IIonValue.Annotation>(),
                    properties.ToArray()))
                .ApplyTo(initializer => new IonStruct(initializer));

            return ionStruct;
        }
        #endregion

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

        private (object value, IonStruct.Property[] uninitializedProperties) ConstructType(
            IonStruct ion,
            ConversionContext context)
        {
            (var constructor, var ionArguments) = SelectConstructor(ion);

            var argumentTypes = constructor.Member
                .GetParameters()
                .Select(p => p.ParameterType)
                .ToArray();

            var arguments = ionArguments
                .Select((prop, index) => prop.Value.ToClrObject(argumentTypes[index], context))
                .ToArray();

            var value = constructor.Invoker.New(arguments);

            var initializedProperties = ionArguments
                .Select(prop => prop.Name)
                .ApplyTo(props => new HashSet<IonTextSymbol>(props));

            var uninitializedProperties = ion.Value!
                .Where(prop => !initializedProperties.Contains(prop.Name))
                .ToArray();

            return (value, uninitializedProperties);
        }

        private object InitializeProperties(
            object instance,
            IonStruct.Property[] uninitializedProperties,
            ConversionContext context)
        {
            var propertyMap = uninitializedProperties.ToDictionary(props => props.Name, props => props.Value);

            propertyProfiles
                .Where(profile => profile.Setter is not null)
                .Where(profile => propertyMap.ContainsKey(profile.Member.Name))
                .ForAll(profile =>
                {
                    var ignoredProperties = context.Options.IgnoredProperties;

                    if (profile.Member.DeclaringType is null)
                        throw new ArgumentException($"Invalid member DeclaringType for: {profile.Member}");

                    if (profile.Setter is null)
                        throw new ArgumentException($"Property '{profile.Member.Name}' is not writable");

                    if (ignoredProperties.TryGetValue(profile.Member.DeclaringType, out var ignoredProps)
                        && ignoredProps.Contains(profile.Member))
                        return;

                    var ion = propertyMap[profile.Member.Name];

                    if (NullValueBehavior.Ignore.Equals(context.Options.NullValueBehavior)
                        && ion.IsNull)
                        return;

                    var propertyValue = ion.ToClrObject(
                        profile.Member.PropertyType,
                        context);

                    if (SkipNull(propertyValue, context)
                        || SkipDefault(propertyValue, profile.Member.PropertyType, context))
                        return;

                    profile.Setter.Invoke(instance, new[] { propertyValue });
                });

            return instance;
        }

        private static string InvalidTypeError(string param) => $"Supplied type must not be {param}";

        private static bool SkipDefault(object? value, Type targetType, ConversionContext context)
        {
            return 
                EqualityComparer<object>.Default.Equals(value, targetType.DefaultValue())
                && DefaultValueBehavior.Ignore.Equals(context.Options.DefaultValueBehavior);
        }

        private static bool SkipNull(object? value, ConversionContext context)
        {
            return
                value is null
                && NullValueBehavior.Ignore.Equals(context.Options.NullValueBehavior);
        }
    }
}
