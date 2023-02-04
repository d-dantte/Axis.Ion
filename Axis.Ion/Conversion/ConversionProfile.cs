using Axis.Ion.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Axis.Ion.Conversion
{
    internal class ConversionProfile<T> : IConverter
    {
        public bool CanConvert(Type type)
        {
            throw new NotImplementedException();
        }

        public object FromIon(Type type, IIonType ion, ConversionOptions options)
        {
            throw new NotImplementedException();
        }

        public IIonType ToIon(Type type, object instance, ConversionOptions optins)
        {
            throw new NotImplementedException();
        }
    }
}
