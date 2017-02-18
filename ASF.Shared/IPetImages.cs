using ASF.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace ASF.Shared
{
    public interface IPetImages
    {
        IReadOnlyCollection<string> ImageUrls { get; }
    }

    public static class IPetImagesExtensions
    {
        private static async Task<byte[]> getImageData(string imageUrl)
        {
            WebRequest request = WebRequest.Create(imageUrl);
            using (var response = await request.GetResponseAsync())
            using (var stream = response.GetResponseStream())
            {
                return stream.ReadAll();
            }
        }

        public static async Task<Image> GenerateCompositeImage(this IPetImages instance,
            GenerateImageLayout layoutGenerator, float margin = 0f, Color? backgroundColor = null)
        {
            var disposables = new List<IDisposable>();

            try
            {
                var images = new List<Image>();

                foreach (string url in instance.ImageUrls)
                {
                    var data = await getImageData(url);
                    var stream = new MemoryStream(data);
                    disposables.Add(stream);

                    Image image = new Bitmap(stream);
                    disposables.Add(image);
                    images.Add(image);
                }

                return images.CombineImages(layoutGenerator, margin, backgroundColor);
            }
            finally
            {
                foreach (IDisposable disposable in disposables)
                {
                    disposable.Dispose();
                }
            }
        }
    }
}
