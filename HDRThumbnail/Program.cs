using System;
using System.IO;
using HDRThumbnail;

class Program
{
    static void Main(string[] args)
    {
        string hdri = "little_paris_eiffel_tower_4k.hdr";
        //string hdri = "industrial_sunset_puresky_1k.hdr";
        //string currentDirectory = "D:\\Code workshop\\clo\\HDRThumbnail\\HDRThumbnail\v2_little_paris_eiffel_tower_1k.hdr";
        string currentDirectory = Directory.GetCurrentDirectory();
        string outputPath = "D:\\Code workshop\\clo\\HDRThumbnail\\HDRThumbnail\\outy.jpg";
        //currentDirectory = "C:\\Users\\rwong\\Downloads";
        //outputPath = "C:\\Users\\rwong\\Downloads\\thumbnail.png";
        string filepath = Path.Combine(currentDirectory, hdri);
        Console.WriteLine(filepath);


        HDRThumbnail.HDRParser.createThumbnail(filepath, outputPath, 1024, 512, 100);
        //HDRThumbnail.DevILTest.loadHDR(filepath);
        int width, height;
        //float[] pixelData = HDRThumbnail.HDRParser.ParseHDR(filepath, out width, out height);
        //Console.WriteLine(pixelData.ToString());
        ////Access individual pixel values from the pixel data
        //int pixelIndex = (1 * width + 10) * 3;  // Replace x and y with desired pixel coordinates
        //float red = pixelData[pixelIndex];
        //float green = pixelData[pixelIndex + 1];
        //float blue = pixelData[pixelIndex + 2];

        //HDRIFileParser.ParseHDRIFile(filePath);
        //HDRIImageData pixelData = HDRIFileParser.ParseHDRIFile(filepath);

        return;
    }
}