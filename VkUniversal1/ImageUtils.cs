using System;
using System.IO;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace VkUniversal1
{
    public static class ImageUtils
    {
        public static async Task<byte[]> UriToByte(Uri imageSource)
        {
            if (imageSource == null) return null;
            using (var wc = new WebClient())
                return await wc.DownloadDataTaskAsync(imageSource);
        }

        public static async Task<ImageSource> ConvertToImageSource( byte[] imageBuffer)
        {
            if (imageBuffer == null) return null;
            ImageSource imageSource;
            using (var stream = new MemoryStream(imageBuffer))
            {
                var ras = stream.AsRandomAccessStream();
                var decoder = await BitmapDecoder.CreateAsync(BitmapDecoder.JpegDecoderId, ras);
                var provider = await decoder.GetPixelDataAsync();
                var buffer = provider.DetachPixelData();
                var bitmap = new WriteableBitmap((int)decoder.PixelWidth, (int)decoder.PixelHeight);
                await bitmap.PixelBuffer.AsStream().WriteAsync(buffer, 0, buffer.Length);
                imageSource = bitmap;
            }
            return imageSource;
        }
    }
}