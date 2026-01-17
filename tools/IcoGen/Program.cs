using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace IcoGen {
    class Program {
        static void Main(string[] args) {
            if (args.Length != 2) {
                Console.WriteLine("Usage: IcoGen <input.png> <output.ico>");
                return;
            }

            string inputPath = args[0];
            string outputPath = args[1];

            try {
                using (var stream = File.OpenWrite(outputPath)) 
                using (var original = (Bitmap)Image.FromFile(inputPath)) 
                using (var bitmap = new Bitmap(original, 256, 256)) 
                {
                    // Header
                    stream.WriteByte(0); stream.WriteByte(0); // Reserved
                    stream.WriteByte(1); stream.WriteByte(0); // Type (1=ICO)
                    stream.WriteByte(1); stream.WriteByte(0); // Count (1)

                    int width = bitmap.Width >= 256 ? 0 : bitmap.Width;
                    int height = bitmap.Height >= 256 ? 0 : bitmap.Height;
                    
                    stream.WriteByte((byte)width);
                    stream.WriteByte((byte)height);
                    stream.WriteByte(0); // Palette
                    stream.WriteByte(0); // Reserved
                    stream.WriteByte(0); stream.WriteByte(0); // Color planes
                    stream.WriteByte(32); stream.WriteByte(0); // Bits per pixel

                    using (var memoryStream = new MemoryStream()) {
                        bitmap.Save(memoryStream, ImageFormat.Png);
                        byte[] buffer = memoryStream.ToArray();
                        int size = buffer.Length;
                        
                        stream.WriteByte((byte)size); 
                        stream.WriteByte((byte)(size >> 8));
                        stream.WriteByte((byte)(size >> 16));
                        stream.WriteByte((byte)(size >> 24));

                        stream.WriteByte(22); stream.WriteByte(0); stream.WriteByte(0); stream.WriteByte(0); // Offset
                        
                        stream.Write(buffer, 0, size);
                    }
                }
                Console.WriteLine("Created icon: " + outputPath);
            } catch (Exception ex) {
                Console.WriteLine("Error: " + ex.Message);
                Environment.Exit(1);
            }
        }
    }
}
