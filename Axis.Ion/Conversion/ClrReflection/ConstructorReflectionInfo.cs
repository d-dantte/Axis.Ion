using Axis.Ion.Types;
using Axis.Luna.Extensions;
using Axis.Luna.FInvoke;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Axis.Ion.Conversion.ClrReflection
{
    internal record ConstructorReflectionInfo : IMemberReflectionInfo<ConstructorInfo>
    {
        public ConstructorInfo Member { get; private set; }

        public ConstructorInvoker Invoker { get; private set; }

        public ConstructorReflectionInfo(ConstructorInfo ctor)
        {
            Member = ctor ?? throw new ArgumentNullException(nameof(ctor));

            if (!IsNormalizable(Member))
                throw new ArgumentException("The supplied constructor cannot be profiles");

            Invoker = ConstructorInvoker.InvokerFor(ctor);
        }

        public bool CanConstruct(IonStruct ionStruct, out IonStruct.Property[] argumentSource)
        {
            if (ionStruct is null)
                throw new ArgumentNullException(nameof(ionStruct));

            if (ionStruct.IsNull)
            {
                argumentSource = Array.Empty<IonStruct.Property>();
                return false;
            }

            var constructorParameterLength = Member.GetParameters().Length;
            if (constructorParameterLength == 0)
            {
                argumentSource = Array.Empty<IonStruct.Property>();
                return true;
            }

            var normalizedStructMap = ionStruct.Value!
                .GroupBy(property => NormalizeParameterName(property.Name.Value!))
                .ToDictionary(
                    group => group.Key,
                    group => group.ToArray());

            argumentSource = Member
                .GetParameters()
                .Select(param =>
                {
                    var normalizedParam = NormalizeParameterName(param.Name);
                    if (!normalizedStructMap.TryGetValue(normalizedParam, out var ionProperties))
                        return null;

                    return ionProperties
                        .Where(ionProp => ConversionUtils.AreCompatible(ionProp.Value.Type, param.ParameterType))
                        .FirstOrNull();
                })
                .Where(prop => prop is not null)
                .Select(prop => prop!.Value)
                .ToArray();

            if (constructorParameterLength == argumentSource.Length)
                return true;

            argumentSource = Array.Empty<IonStruct.Property>();
            return false;
        }

        public static bool IsNormalizable(ConstructorInfo ctor)
            => ctor.GetParameters().Length == NormalizedParameterNames(ctor).Count();

        private static HashSet<string> NormalizedParameterNames(ConstructorInfo ctor) 
            => ctor
                .GetParameters()
                .Select(param => param.Name)
                .Select(NormalizeParameterName)
                .ApplyTo(name => new HashSet<string>(name));

        private static string NormalizeParameterName(string? parameterName)
            => parameterName?.ToLower() ?? throw new ArgumentNullException(nameof(parameterName));
    }
}
