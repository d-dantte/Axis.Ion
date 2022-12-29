using System.IO;
using System.Threading.Tasks;

namespace Axis.Ion.IO.Binary
{
    public interface IPayloadWriter: ITypePayload
    {
        void Write(Stream outputStream);

        Task WriteAsync(Stream outputStream);
    }
}
