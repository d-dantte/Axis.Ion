using Axis.Luna.FInvoke;
using System;

namespace Axis.Ion.Conversion.ClrReflection
{
    internal record MapReflectionInfo
    {
        internal InstanceInvoker Keys { get; }
        internal InstanceInvoker GetValue { get; }
        internal InstanceInvoker SetValue { get; }

        public MapReflectionInfo(
            InstanceInvoker keys,
            InstanceInvoker getValues,
            InstanceInvoker setValues)
        {
            Keys = keys ?? throw new ArgumentNullException(nameof(keys));
            GetValue = getValues ?? throw new ArgumentNullException(nameof(getValues));
            SetValue = setValues ?? throw new ArgumentNullException(nameof(setValues));
        }
    }
}
