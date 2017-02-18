using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace ASF.Shared.Helpers
{
    public static class ImageHelper
    {
        public static byte[] SaveToBytes(this Image image, ImageFormat format)
        {
            using (Stream stream = new MemoryStream())
            {
                image.Save(stream, format);
                stream.Position = 0;
                return stream.ReadAll();
            }
        }

        public static string GetMimeType(this ImageFormat format)
            => ImageCodecInfo.GetImageEncoders()
            .First(e => e.FormatID == format.Guid)
            .MimeType;
    }
}
