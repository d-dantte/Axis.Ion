using Axis.Ion.Types;
using System;

namespace Axis.Ion.Conversion.IonProfiles
{
    /// <summary>
    /// 
    /// </summary>
    internal class EnumConversionProfile : IConversionProfile
    {
        public EnumConversionProfile()
        { }

        public bool CanConvert(Type type) => type.IsEnum;

        public object? FromIon(Type type, IIonType ion, ConversionOptions options)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            var value = ion switch
            {
                IonString @string => @string.Value,
                IonIdentifier identifier => identifier.Value,
                IonQuotedSymbol quoted => quoted.Value,
                _ => throw new ArgumentException($"Invalid ion type: {ion?.Type}")
            };

            if (value is null)
                throw new ArgumentException("Invalid ion: cannot convert null to enum");

            return Enum.Parse(type, value);
        }

        public IIonType ToIon(Type type, object? instance, ConversionOptions options)
        {
            IConversionProfile.ValidateSourceTypeCompatibility(type, instance?.GetType());

            if (instance is null)
                return IIonType.NullOf(IonTypes.IdentifierSymbol);

            return new IonIdentifier(instance.ToString());
        }
    }
}
