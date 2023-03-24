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
        public bool CanConvert(Type destinationType, IIonType ion)
        {
            if (destinationType is null)
                throw new ArgumentNullException(nameof(destinationType));

            if (ion is null)
                throw new ArgumentNullException(nameof(ion));

            return destinationType.IsEnum
                && IonTypes.IdentifierSymbol == ion.Type;
        }

        public object? ToClr(Type destinationType, IIonType ion, ConversionContext context)
        {
            if (destinationType is null)
                throw new ArgumentNullException(nameof(destinationType));

            var value = ion switch
            {
                IonString @string => @string.Value,
                IonIdentifier identifier => identifier.Value,
                IonQuotedSymbol quoted => quoted.Value,
                _ => throw new ArgumentException($"Invalid ion type: {ion?.Type}")
            };

            if (value is null)
                throw new ArgumentException("Invalid ion: cannot convert null to enum");

            return Enum.Parse(destinationType, value);
        }
        #endregion

        #region IIonConverter
        public bool CanConvert(Type sourceType, object? instance)
        {
            if (sourceType is null)
                throw new ArgumentNullException(nameof(sourceType));

            return (instance?.GetType() ?? sourceType).IsEnum;
        }

        public IIonType ToIon(Type sourceType, object? instance, ConversionContext options)
        {
            if (instance is null)
                return IIonType.NullOf(IonTypes.IdentifierSymbol);

            if (!instance.GetType().IsEnum)
                throw new ArgumentException($"Supplied {nameof(instance)} is not an enum");

            return new IonIdentifier(instance.ToString());
        }
        #endregion
    }
}
