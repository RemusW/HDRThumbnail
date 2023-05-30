using System.Drawing;
using System.Drawing.Imaging;
using static System.Formats.Asn1.AsnWriter;

namespace HDRThumbnail
{
    public class HDRIFileParser
    {
        public static HDRIImageData ParseHDRIFile(string filepath)
        {
            // Read the file as bytes
            byte[] fileBytes = File.ReadAllBytes(filepath);

            // Parse the header and extract necessary information
            int width = BitConverter.ToInt32(fileBytes, 0);
            int height = BitConverter.ToInt32(fileBytes, 4);

            // Calculate the data start offset
            int dataStartOffset = 8 + 4; // Header size + width and height size

            // Extract the pixel data
            float[] pixelData = new float[width * height * 3]; // Assuming RGB format

            for (int i = 0; i < pixelData.Length; i++)
            {
                pixelData[i] = BitConverter.ToSingle(fileBytes, dataStartOffset + i * 4);
            }

            return new HDRIImageData(width, height, pixelData);
        }
    }

    public class HDRIImageData
    {
        public int Width { get; }
        public int Height { get; }
        public float[] PixelData { get; }

        public HDRIImageData(int width, int height, float[] pixelData)
        {
            Width = width;
            Height = height;
            PixelData = pixelData;
        }
    }
}

