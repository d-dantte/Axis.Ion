namespace Axis.Ion.Conversion
{
    /// <summary>
    /// 
    /// </summary>
    public interface IConversionNotificationListener
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="event"></param>
        /// <param name="message"></param>
        void Notify(ConversionEvent @event, string message);
    }

    /// <summary>
    /// 
    /// </summary>
    public enum ConversionEvent
    {

    }
}
