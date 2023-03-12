using System;
using System.Collections.Generic;
using System.Text;

namespace Axis.Ion.Conversion
{
    public class ConversionOptions
    {
        /// <summary>
        /// Gets the collection of registered <see cref="IConverter"/> instances. This property is never null.
        /// </summary>
        public IConverter[]? Converters { get; set; } = Array.Empty<IConverter>();
    }
}
