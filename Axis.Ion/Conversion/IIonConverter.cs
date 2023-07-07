using Axis.Ion.Types;
using System;

namespace Axis.Ion.Conversion
{
    /// <summary>
    /// 
    /// </summary>
    public interface IIonConverter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceType"></param>
        /// <param name="instance"></param>
        /// <returns></returns>
        bool CanConvert(Type sourceType, object? instance);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceType"></param>
        /// <param name="instance"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        IIonValue ToIon(Type sourceType, object? instance, ConversionContext context);
    }
}
