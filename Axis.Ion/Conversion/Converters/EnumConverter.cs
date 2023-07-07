using Axis.Ion.Types;
using System;

namespace Axis.Ion.Conversion.Converters
{
    /// <summary>
    /// 
    /// </summary>
    internal class EnumConverter : IConverter
    {
        #region IClrConverter
        public bool CanConvert(Type destinationType, IIonValue ion)
        {
            if (destinationType is null)
                throw new ArgumentNullException(nameof(destinationType));

            if (ion is null)
                throw new ArgumentNullException(nameof(ion));

            return destinationType.IsEnum
                && ion is IonTextSymbol symbol
                && symbol.IsIdentifier;
        }

        public object? ToClr(Type destinationType, IIonValue ion, ConversionContext context)
        {
            if (destinationType is null)
                throw new ArgumentNullException(nameof(destinationType));

            var value = ((IonTextSymbol)ion).Value;

            return value is null
                ? throw new ArgumentException("Invalid ion: cannot convert null to enum")
                : Enum.Parse(destinationType, value);
        }
        #endregion

        #region IIonConverter
        public bool CanConvert(Type sourceType, object? instance)
        {
            if (sourceType is null)
                throw new ArgumentNullException(nameof(sourceType));

            return (instance?.GetType() ?? sourceType).IsEnum;
        }

        public IIonValue ToIon(Type sourceType, object? instance, ConversionContext options)
        {
            if (instance is null)
                return IIonValue.NullOf(IonTypes.TextSymbol);

            if (!instance.GetType().IsEnum)
                throw new ArgumentException($"Supplied {nameof(instance)} is not an enum");

            return new IonTextSymbol(instance.ToString());
        }
        #endregion
    }
}
