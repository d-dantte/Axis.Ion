using Axis.Luna.FInvoke;
using System;
using System.Reflection;

namespace Axis.Ion.Conversion.ClrReflection
{
    internal record PropertyReflectionInfo : IMemberReflectionInfo<PropertyInfo>
    {
        public PropertyInfo Member { get; private set; }

        public InstanceInvoker? Getter { get; private set; }

        public InstanceInvoker? Setter { get; private set; }

        public PropertyReflectionInfo(PropertyInfo property)
        {
            Member = property ?? throw new ArgumentNullException(nameof(property));

            if (!IsProfileable(property))
                throw new ArgumentException("The supplied property cannot be profiles");

            Getter = Member.GetGetMethod()?.InstanceInvoker();
            Setter = Member.GetSetMethod()?.InstanceInvoker();
        }

        public static bool IsProfileable(PropertyInfo property)
        {
            if (property is null)
                throw new ArgumentNullException(nameof(property));

            // Indexers are not profilable
            if (property.GetIndexParameters().Length > 0)
                return false;

            return !(property.GetGetMethod() == null && property.GetSetMethod() == null);
        }
    }
}
