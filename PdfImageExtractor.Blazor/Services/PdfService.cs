using PDFtoImage;
using SkiaSharp;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace PdfImageExtractor.Blazor.Services
{
    public class PdfService
    {
        public async Task<List<byte[]>> ExtractPagesAsync(Stream pdfStream, string format, int dpi = 300)
        {
            var skFormat = format.ToLower() switch
            {
                "png" => SKEncodedImageFormat.Png,
                "jpg" or "jpeg" => SKEncodedImageFormat.Jpeg,
                "webp" => SKEncodedImageFormat.Webp,
                "bmp" => SKEncodedImageFormat.Bmp,
                _ => SKEncodedImageFormat.Png
            };

            using var memoryStream = new MemoryStream();
            await pdfStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            return await Task.Run(() => 
            {
                var options = new RenderOptions { Dpi = dpi };
                var images = new List<byte[]>();
                
                foreach (var bitmap in Conversion.ToImages(memoryStream, options: options))
                {
                    using (bitmap)
                    {
                        using var ms = new MemoryStream();
                        bitmap.Encode(ms, skFormat, 100);
                        images.Add(ms.ToArray());
                    }
                }
                
                return images;
            });
        }

        public async Task<byte[]> CreateZipAsync(List<byte[]> images, string extension)
        {
            using var ms = new MemoryStream();
            using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, true))
            {
                for (int i = 0; i < images.Count; i++)
                {
                    var entry = archive.CreateEntry($"page_{i + 1}.{extension}");
                    using var entryStream = entry.Open();
                    await entryStream.WriteAsync(images[i], 0, images[i].Length);
                }
            }
            return ms.ToArray();
        }
    }
}
