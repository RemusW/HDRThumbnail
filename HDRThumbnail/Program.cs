using System;
using System.IO;


using HDRThumbnail;

class Program
{
    static void Main(string[] args)
    {
        string hdri = "little_paris_eiffel_tower_1k.hdr";
        string hdripath = "/cloremus/Documents/clo/HDRThumbnail/HDRThumbnail/little_paris_eiffel_tower_1k.hdr";
        string currentDirectory = Directory.GetCurrentDirectory();
        string filePath = Path.Combine(currentDirectory, hdri);

        int width, height;
        float[] pixelData = HDRDecoder.DecodeHDR(filePath, out width, out height);

        // Access individual pixel values from the pixel data
        int pixelIndex = (10 * width + 10) * 3;  // Replace x and y with desired pixel coordinates
        float red = pixelData[pixelIndex];
        float green = pixelData[pixelIndex + 1];
        float blue = pixelData[pixelIndex + 2];

        //HDRIFileParser.ParseHDRIFile(filePath);

        return;
    }

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

    public class HDRDecoder
    {
        public static float[] DecodeHDR(string filePath, out int width, out int height)
        {
            float[] pixelData = null;
            width = 0;
            height = 0;

            try
            {
                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    using (var streamReader = new StreamReader(fileStream))
                    {
                        string line;

                        // Skip comments
                        do
                        {
                            line = streamReader.ReadLine();
                        } while (line.StartsWith("#"));

                        // Read the resolution line
                        string[] resolution = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        width = int.Parse(resolution[0]);
                        height = int.Parse(resolution[1]);

                        // Allocate pixel data array
                        pixelData = new float[width * height * 3];  // Assuming RGB format

                        // Read pixel values
                        int pixelIndex = 0;
                        while ((line = streamReader.ReadLine()) != null && pixelIndex < pixelData.Length)
                        {
                            string[] pixelValues = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            pixelData[pixelIndex++] = float.Parse(pixelValues[0]);
                            pixelData[pixelIndex++] = float.Parse(pixelValues[1]);
                            pixelData[pixelIndex++] = float.Parse(pixelValues[2]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error decoding HDR file: " + ex.Message);
            }

            return pixelData;
        }
    }
}