using Axis.Ion.Types;
using System;

namespace Axis.Ion.Conversion
{
    public static class Ionizer
    {
        public static IIonType ToIon<T>(T value)
        {
            throw new NotImplementedException();
        }

        public static IIonType ToIon(object value, Type sourceType)
        {
            throw new NotImplementedException();
        }

        public static T FromIon<T>(IIonType ion)
        {
            throw new NotImplementedException();
        }

        public static object FromIon(IIonType ion, Type destinationType)
        {
            throw new NotImplementedException();
        }
    }
}
