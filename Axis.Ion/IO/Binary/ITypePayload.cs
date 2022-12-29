using Axis.Ion.Types;

namespace Axis.Ion.IO.Binary
{
    /// <summary>
    /// A structured representation of the binary data that is written/read
    /// </summary>
    public interface ITypePayload
    {
        /// <summary>
        /// The metadata component
        /// </summary>
        TypeMetadata Metadata { get; }

        /// <summary>
        /// The ion value
        /// </summary>
        IIonType IonValue { get; }
    }
}
