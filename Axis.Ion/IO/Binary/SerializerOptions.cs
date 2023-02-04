namespace Axis.Ion.IO.Binary
{
    /// <summary>
    /// make this a record, or a readonly struct, or something
    /// </summary>
    public class SerializerOptions
    {
        /// <summary>
        /// specifies the page size that is read/writen when an extremely lengthy stream
        /// of bytes needs to be written.
        /// Pagination delimits the start of each chunk/page of the lengthy stream with an
        /// integer representing the number of bytes written into that chunk/page.
        /// </summary>
        public int DatapageSize { get; set; } = int.MaxValue;
    }
}
