using Axis.Ion.Types;
using System;

namespace Axis.Ion.Conversion
{
    /// <summary>
    /// 
    /// </summary>
    public interface IClrConverter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="destinationType"></param>
        /// <param name="ion"></param>
        /// <returns></returns>
        bool CanConvert(Type destinationType, IIonValue ion);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="destinationType"></param>
        /// <param name="ion"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        object? ToClr(Type destinationType, IIonValue ion, ConversionContext context);
    }
}
