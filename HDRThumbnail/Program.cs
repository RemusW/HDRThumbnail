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
        string filepath = Path.Combine(currentDirectory, hdri);

        int width, height;
        float[] pixelData = HDRParser.ParseHDR(filepath, out width, out height);
        //Access individual pixel values from the pixel data
        int pixelIndex = (10 * width + 10) * 3;  // Replace x and y with desired pixel coordinates
        float red = pixelData[pixelIndex];
        float green = pixelData[pixelIndex + 1];
        float blue = pixelData[pixelIndex + 2];

        //HDRIFileParser.ParseHDRIFile(filePath);
        //HDRIImageData pixelData = HDRIFileParser.ParseHDRIFile(filepath);

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
                    Console.WriteLine("Opening File");
                    using (var streamReader = new StreamReader(fileStream))
                    {
					    Console.WriteLine("Streaming file");
                        string line;

                        // Skip comments
                        //do
                        //{
                        //    line = streamReader.ReadLine();
                        //} while (line.StartsWith("#"));

                        line = streamReader.ReadLine();
                        line = streamReader.ReadLine();
                        line = streamReader.ReadLine();
                        // Read the resolution line
                        string[] resolution = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        Console.WriteLine("Invalid resolution line." + resolution[0] + " " + resolution[1]);
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

public class HDRParser
{
    public static float[] ParseHDR(string filePath, out int width, out int height)
    {
        float[] pixelData = null;
        byte[] rawPixelData = null;
        width = 0;
        height = 0;

        Console.WriteLine("HDR Parser");
        try
        {
            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                using (var streamReader = new StreamReader(fileStream))
                {
                    Console.WriteLine("Starting to read lines");
                    // Read and validate the file format identifier
                    string format = streamReader.ReadLine();
                    if (format != "#?RADIANCE" && format != "#?RGBE")
                    {
                        Console.WriteLine("Invalid file format.");
                        return null;
                    }

                    // Skip comments and empty lines
                    string line;
                    do
                    {
                        line = streamReader.ReadLine()?.Trim();
                    } while (!string.IsNullOrEmpty(line) && line.StartsWith("#"));

                    line = streamReader.ReadLine();
                    line = streamReader.ReadLine();
                    //Read the resolution line
                    //string[] resolution = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    //if (resolution.Length != 4 || !int.TryParse(resolution[3], out width) || !int.TryParse(resolution[1], out height))
                    //{
                    //    Console.WriteLine("Invalid resolution line." + width + " " + height);
                    //    return null;
                    //}

                    string[] resolution = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    Console.WriteLine("Invalid resolution line." + resolution[1] + " " + resolution[3]);
                    width = int.Parse(resolution[1]);
                    height = int.Parse(resolution[3]);

                    // Allocate pixel data array
                    pixelData = new float[width * height * 3];  // Assuming RGB format
                    string rgbeData = streamReader.ReadToEnd();
                    // Read pixel values
                    //int pixelIndex = 0;
                    //while ((line = streamReader.ReadLine()) != null && pixelIndex < pixelData.Length)
                    //{
                    //    string[] pixelValues = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    //    for (int i = 0; i < pixelValues.Length; i++)
                    //    {
                    //        if (float.TryParse(pixelValues[i], out float pixel))
                    //        {
                    //            pixelData[pixelIndex++] = pixel;
                    //        }
                    //        else
                    //        {
                    //            Console.WriteLine("Invalid pixel value.");
                    //            return null;
                    //        }
                    //    }
                    //}
                    byte[] pixelD = RgbeDecoder.DecodeRgbe(rgbeData, width, height);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error parsing HDR file: " + ex.Message);
            return null;
        }

        return pixelData;
    }
}


public class RgbeDecoder
{
    public static byte[] DecodeRgbe(string rgbeString, int width, int height)
    {
        byte[] rgbeData  = System.Text.Encoding.ASCII.GetBytes(rgbeString);
        Console.WriteLine(rgbeString);

        byte[] imageData = new byte[width * height * 3]; // Assuming RGB format

        int dataIndex = 0;
        int scanlineWidth = width * 4; // Each scanline has 4 bytes per pixel (RGBE format)
        int numPixels = width * height;

        for (int y = 0; y < height; y++)
        {
            int scanlineStart = y * scanlineWidth;
            int scanlineEnd = scanlineStart + scanlineWidth;

            int x = 0;
            while (x < width)
            {
                byte rgbeIndicator = rgbeData[dataIndex++];
                byte rgbeValue = rgbeData[dataIndex++];

                if (rgbeIndicator == 0x02 && rgbeValue == 0x02)
                {
                    // New RLE scanline encoding
                    int runLength = BitConverter.ToUInt16(rgbeData, dataIndex);
                    dataIndex += 2;

                    if (runLength == 0 || x + runLength > width)
                    {
                        throw new InvalidDataException("Invalid RLE scanline encoding.");
                    }

                    byte red = rgbeData[dataIndex++];
                    byte green = rgbeData[dataIndex++];
                    byte blue = rgbeData[dataIndex++];

                    for (int i = 0; i < runLength; i++)
                    {
                        int pixelIndex = scanlineStart + (x + i) * 3;
                        imageData[pixelIndex] = red;
                        imageData[pixelIndex + 1] = green;
                        imageData[pixelIndex + 2] = blue;
                    }

                    x += runLength;
                }
                else if (rgbeIndicator == 0x01 && rgbeValue == 0x02)
                {
                    // Old RLE scanline encoding
                    int runLength = rgbeData[dataIndex++];

                    if (runLength == 0 || x + runLength > width)
                    {
                        throw new InvalidDataException("Invalid RLE scanline encoding.");
                    }

                    byte red = rgbeData[dataIndex++];
                    byte green = rgbeData[dataIndex++];
                    byte blue = rgbeData[dataIndex++];

                    for (int i = 0; i < runLength; i++)
                    {
                        int pixelIndex = scanlineStart + (x + i) * 3;
                        imageData[pixelIndex] = red;
                        imageData[pixelIndex + 1] = green;
                        imageData[pixelIndex + 2] = blue;
                    }

                    x += runLength;
                }
                else
                {
                    // Individual pixel encoding
                    imageData[scanlineStart + x * 3] = rgbeIndicator;
                    imageData[scanlineStart + x * 3 + 1] = rgbeValue;
                    imageData[scanlineStart + x * 3 + 2] = rgbeData[dataIndex++];
                    x++;
                }
            }

            if (dataIndex > rgbeData.Length)
            {
                throw new InvalidDataException("Incomplete or corrupt RGEB data.");
            }
        }

        return imageData;
    }
}
