using Axis.Ion.Types;
using Axis.Luna.Common;
using Axis.Luna.Common.Results;
using Axis.Pulsar.Grammar.CST;

namespace Axis.Ion.IO.Text
{
    /// <summary>
    /// Represents a specialized string serializer/deserializer for the <see cref="IIonType"/> instances
    /// </summary>
    /// <typeparam name="T">The actual ion type</typeparam>
    public interface IIonTypeTextSerializer<T>
    where T : IIonType
    {
        /// <summary>
        /// Serializes (streams) the ion value into text
        /// </summary>
        /// <param name="ionValue">the ion value</param>
        /// <param name="context">the context</param>
        /// <returns>the textual representation of the ion value</returns>
        string SerializeText(T ionValue, SerializingContext context);

        /// <summary>
        /// Parses the <see cref="CSTNode"/> representation of the ion text
        /// </summary>
        /// <param name="tokenNode">the token node</param>
        T ParseToken(CSTNode tokenNode);

        /// <summary>
        /// Attempts to parse the <see cref="CSTNode"/> into an <see cref="IIonType"/>
        /// </summary>
        /// <param name="tokenNode">the token node</param>
        /// <param name="result">the output result</param>
        /// <returns>true if parsing was successful, false otherwise</returns>
        bool TryParse(CSTNode tokenNode, out IResult<T> result);

        /// <summary>
        /// Parses the raw ion text back into it's ion value
        /// </summary>
        /// <param name="ionValueText">the textual representation of the ion value</param>
        T ParseString(string ionValueText);

        /// <summary>
        /// Attempts to parse the string into an <see cref="IIonType"/>
        /// </summary>
        /// <param name="ionValueText">the textual representation of the ion value</param>
        /// <param name="result">the output result</param>
        /// <returns>true if parsing was successful, false otherwise</returns>
        bool TryParse(string ionValueText, out IResult<T> result);
    }
}
