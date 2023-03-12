using System.Reflection;

namespace Axis.Ion.Conversion.ClrReflection
{
    /// <summary>
    /// TODO
    /// </summary>
    /// <typeparam name="TMember"></typeparam>
    internal interface IMemberReflectionInfo<TMember>
    where TMember : MemberInfo
    {
        /// <summary>
        /// The member
        /// </summary>
        TMember Member { get; }
    }
}
