using System.IO;

namespace Axis.Ion.IO
{
    /// <summary>
    /// The base contract for ion serialization.
    /// <para>
    /// Implementations can choose how they encode/decode the data
    /// </para>
    /// </summary>
    public interface IIonBinarySerializer
    {
        /// <summary>
        /// Serialize a list of ion types into an implementation-specific encoding
        /// </summary>
        /// <param name="packet">The ion packet</param>
        /// <returns>the serialized bytes</returns>
        byte[] Serialize(IonPacket packet);

        /// <summary>
        /// Deserialize implementation encoded bytes read from a stream into an array of ion types
        /// </summary>
        /// <param name="ionStream">The stream of bytes</param>
        /// <returns>the deserialized packet</returns>
        IonPacket Deserialize(Stream ionStream);
    }
}
