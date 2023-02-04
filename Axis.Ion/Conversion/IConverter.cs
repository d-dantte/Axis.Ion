using Axis.Ion.Types;
using System;

namespace Axis.Ion.Conversion
{
    public interface IConverter
    {
        bool CanConvert(Type type);

        IIonType ToIon(Type type, object instance, ConversionOptions optins);

        object FromIon(Type type, IIonType ion, ConversionOptions options);
    }
}
