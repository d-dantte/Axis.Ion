using Axis.Ion.Types;
using System;

namespace Axis.Ion.Conversion
{
    /// <summary>
    /// TODO
    /// </summary>
    public interface IConverter
    {
        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        bool CanConvert(Type type);

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="type"></param>
        /// <param name="instance"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        IIonType ToIon(Type type, object? instance, ConversionOptions options);

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="type"></param>
        /// <param name="ion"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        object? FromIon(Type type, IIonType ion, ConversionOptions options);
    }
}
