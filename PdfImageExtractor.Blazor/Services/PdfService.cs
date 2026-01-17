using PDFtoImage;
using SkiaSharp;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace PdfImageExtractor.Blazor.Services
{
    public class ExtractedPage
    {
        public int PageNumber { get; set; }
        public byte[] Data { get; set; } = Array.Empty<byte>();
        public string PreviewBase64 { get; set; } = "";
    }

    public class PdfService
    {
        public async Task<List<ExtractedPage>> ExtractPagesAsync(Stream pdfStream, string format, int dpi = 300)
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
                var results = new List<ExtractedPage>();
                int pageIndex = 0;
                
                foreach (var bitmap in Conversion.ToImages(memoryStream, options: options))
                {
                    using (bitmap)
                    {
                        // 1. Generate High-Res Output
                        using var msData = new MemoryStream();
                        bitmap.Encode(msData, skFormat, 100);
                        var data = msData.ToArray();

                        // 2. Generate Low-Res Preview (JPEG - smaller base64)
                        string previewBase64 = "";
                        try 
                        {
                            int previewWidth = 400;
                            int previewHeight = (int)((float)bitmap.Height / bitmap.Width * previewWidth);
                            
                            // Log for debugging
                            Console.WriteLine($"[PdfService] Generating thumbnail. Original: {bitmap.Width}x{bitmap.Height}, Target: {previewWidth}x{previewHeight}");

                            var info = new SKImageInfo(previewWidth, previewHeight);
                            using var scaledBitmap = new SKBitmap(info);
                            using var canvas = new SKCanvas(scaledBitmap);
                            
                            // High quality scaling
                            using var paint = new SKPaint { FilterQuality = SKFilterQuality.High };
                            canvas.Clear(SKColors.White); // Ensure background is white
                            canvas.DrawBitmap(bitmap, new SKRect(0, 0, previewWidth, previewHeight), paint);
                            
                            using var msPreview = new MemoryStream();
                            scaledBitmap.Encode(msPreview, SKEncodedImageFormat.Jpeg, 70);
                            var oldPos = msPreview.Position;
                            previewBase64 = Convert.ToBase64String(msPreview.ToArray());
                            Console.WriteLine($"[PdfService] Thumbnail generated. Size: {previewBase64.Length} chars.");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[PdfService] Thumbnail generation FAILED: {ex.Message}");
                            previewBase64 = ""; 
                        }

                        results.Add(new ExtractedPage 
                        { 
                            PageNumber = pageIndex + 1,
                            Data = data,
                            PreviewBase64 = previewBase64
                        });
                        
                        pageIndex++;
                    }
                }
                
                return results;
            });
        }

        public async Task<byte[]> CreateZipAsync(List<ExtractedPage> pages, string extension)
        {
            using var ms = new MemoryStream();
            using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, true))
            {
                foreach (var page in pages)
                {
                    var entry = archive.CreateEntry($"page_{page.PageNumber}.{extension}");
                    using var entryStream = entry.Open();
                    await entryStream.WriteAsync(page.Data, 0, page.Data.Length);
                }
            }
            return ms.ToArray();
        }
    }
}
