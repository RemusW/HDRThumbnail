using System.Drawing;
using System.Drawing.Imaging;
using System;
using System.Collections.Generic;
using OpenCvSharp;
using System.Runtime.InteropServices;

namespace HDRThumbnail
{
    public class HDRParser
    {
        public static void rwHDR(string filePath)
        {
            Mat image = Cv2.ImRead(filePath, ImreadModes.AnyColor | ImreadModes.AnyDepth);
            Cv2.ImShow("first read", image);
            // Check if the image was successfully loaded
            if (image.Empty())
            {
                Console.WriteLine("Failed to load the image.");
            }
            else
            {

                // Convert HDR image to LDR (8-bit) format
                Mat ldrImage = new Mat();
                image.ConvertTo(ldrImage, MatType.CV_8UC3, 255.0);

                // Adjust the brightness by multiplying pixel values by a factor
                //double brightnessFactor = 10.0; // Adjust this value as needed
                //ldrImage *= brightnessFactor;



                // Save the LDR image as JPG
                string outputFilePath = "D:\\Code workshop\\clo\\HDRThumbnail\\HDRThumbnail\\output.png";

                // Convert the Mat object to a regular byte array
                int rows = ldrImage.Rows;
                int cols = ldrImage.Cols;
                int channels = ldrImage.Channels();
                int dataSize = rows * cols * channels;
                Console.WriteLine("{0} {1} {2}", rows, cols, channels);

                byte[] byteImageData = new byte[dataSize];
                Marshal.Copy(ldrImage.Data, byteImageData, 0, dataSize);

                // Convert the byte array to a float array


                // Copy into a 3-dimensional array
                byte[,,] imageArray = new byte[rows, cols, channels];
                int index = 0;
                for (int r = 0; r < rows; r++)
                {
                    for (int c = 0; c < cols; c++)
                    {
                        for (int ch = 0; ch < channels; ch++)
                        {
                            imageArray[r, c, ch] = byteImageData[index++];
                        }
                    }
                }

                // Assign the multidimensional array to the Mat image
                Mat newim = new Mat(rows, cols, MatType.CV_8UC3, imageArray);
                Cv2.ImShow("byte transtered", newim);
                //Mat perspmat = createThumbnail(imageArray, newim);
                //newim.SetArray(imageArray.);
                //newim.SetTo(1000);
                thumbnail(ldrImage);

                //Cv2.ImShow("output", perspmat);
                //Cv2.ImWrite(outputFilePath, createThumbnail(imageArray, perspmat));
                Cv2.WaitKey(0);

                Console.WriteLine("Image saved successfully.");
            }

            Cv2.WaitKey(0);
        }


        public static void thumbnail (Mat equirectangularImage)
        {
            int perspectiveWidth = 1024;
            int perspectiveHeight = 512;
            float fieldOfView = 90f;
            float cameraYaw = 0;
            float cameraPitch = 0;
            float cameraRoll = 0;
            // Create an empty perspective image
            Mat perspectiveImage = new Mat(perspectiveHeight, perspectiveWidth, MatType.CV_8UC3);

            // Convert field of view to radians
            float fovRadians = fieldOfView * (float)Math.PI / 180.0f;

            // Iterate over each pixel in the perspective image
            for (int y = 0; y < perspectiveHeight; y++)
            {
                for (int x = 0; x < perspectiveWidth; x++)
                {
                    // Convert pixel coordinates to normalized device coordinates (NDC)
                    float ndcX = 2.0f * (x / (float)perspectiveWidth) - 1.0f;
                    float ndcY = 2.0f * (y / (float)perspectiveHeight) - 1.0f;

                    // Apply field of view angle to the NDC coordinates
                    float fovCorrectedX = ndcX * 1;
                    float fovCorrectedY = ndcY * 1;

                    // Convert corrected NDC to spherical coordinates
                    float elevation = (float)Math.Asin(fovCorrectedY);
                    float azimuth = (float)Math.Atan2(fovCorrectedX, Math.Sqrt(1.0f - fovCorrectedY * fovCorrectedY));

                    // Convert spherical coordinates to Cartesian coordinates
                    float xCartesian = (float)(Math.Cos(elevation) * Math.Sin(azimuth));
                    float yCartesian = (float)Math.Sin(elevation);
                    float zCartesian = (float)(Math.Cos(elevation) * Math.Cos(azimuth));

                    // Apply camera rotations
                    //RotatePoint(ref xCartesian, ref yCartesian, ref zCartesian, cameraYaw, cameraPitch, cameraRoll);

                    // Convert Cartesian coordinates to longitude and latitude angles
                    float longitude = (float)Math.Atan2(xCartesian, zCartesian);
                    float latitude = (float)Math.Asin(yCartesian);

                    // Map longitude and latitude to equirectangular image coordinates
                    int equirectangularX = (int)((longitude + Math.PI) / (2.0f * Math.PI) * equirectangularImage.Width);
                    int equirectangularY = (int)((latitude + Math.PI / 2.0f) / Math.PI * equirectangularImage.Height);

                    // Retrieve pixel value from equirectangular image
                    Vec3b pixel = equirectangularImage.Get<Vec3b>(equirectangularY, equirectangularX);

                    // Assign pixel value to perspective image
                    perspectiveImage.Set<Vec3b>(y, x, pixel);
                }
            }

            // Save perspective projection image
            //perspectiveImage.SaveImage("perspective.jpg");
            Cv2.ImShow("perspective", perspectiveImage);
        }

        private static void RotatePoint(ref float x, ref float y, ref float z, float yaw, float pitch, float roll)
        {
            // Convert Euler angles to radians
            float yawRadians = yaw * (float)Math.PI / 180.0f;
            float pitchRadians = pitch * (float)Math.PI / 180.0f;
            float rollRadians = roll * (float)Math.PI / 180.0f;

            // Apply yaw rotation
            float cosYaw = (float)Math.Cos(yawRadians);
            float sinYaw = (float)Math.Sin(yawRadians);
            float tempX = x;
            x = cosYaw * tempX - sinYaw * z;
            z = sinYaw * tempX + cosYaw * z;

            // Apply pitch rotation
            float cosPitch = (float)Math.Cos(pitchRadians);
            float sinPitch = (float)Math.Sin(pitchRadians);
            float tempY = y;
            y = cosPitch * tempY - sinPitch * z;
            z = sinPitch * tempY + cosPitch * z;

            // Apply roll rotation
            float cosRoll = (float)Math.Cos(rollRadians);
            float sinRoll = (float)Math.Sin(rollRadians);
            tempX = x;
            x = cosRoll * tempX - sinRoll * y;
            y = sinRoll * tempX + cosRoll * y;
        }

        public static void perspMatThumbnail (Mat hdr)
        {
            Mat equirectangularImage = hdr;
            double fov = 60.0; // Field of view in degrees
            double aspectRatio = 1.0; // Width-to-height aspect ratio of the perspective image
            int perspectiveWidth = 1024; // Width of the perspective image
            int perspectiveHeight = 512; // Height of the perspective image

            double yaw = 0.0; // Yaw angle in degrees (left-right rotation)
            double pitch = 0.0; // Pitch angle in degrees (up-down rotation)
            double roll = 0.0; // Roll angle in degrees (tilt rotation)
            double distance = 1.0; // Distance from the center of the equirectangular image

            Mat perspectiveTransform = Cv2.GetPerspectiveTransform(
                new Point2f[] { new Point2f(0, 0), new Point2f(perspectiveWidth, 0),
                                new Point2f(perspectiveWidth, perspectiveHeight), new Point2f(0, perspectiveHeight) },
                new Point2f[] { new Point2f(perspectiveWidth / 2.0f, perspectiveHeight / 2.0f),
                                new Point2f(perspectiveWidth / 2.0f, perspectiveHeight / 2.0f),
                                new Point2f(perspectiveWidth / 2.0f, perspectiveHeight / 2.0f),
                                new Point2f(perspectiveWidth / 2.0f, perspectiveHeight / 2.0f) });

            Mat perspectiveImage = new Mat();
            Cv2.WarpPerspective(equirectangularImage, perspectiveImage, perspectiveTransform, new OpenCvSharp.Size(perspectiveWidth, perspectiveHeight));

            Cv2.ImShow("perspective", perspectiveImage);

        }

        public static Mat createThumbnail(byte[,,] pixelData, Mat hdrImage)
        {
            float[,,] thumbnail = new float[pixelData.GetLength(0), pixelData.GetLength(1), pixelData.GetLength(2)];
            int width = pixelData.GetLength(1);
            int height = pixelData.GetLength(0);


            Mat reverseProjection = new Mat(height, width, MatType.CV_8UC3);

            // Perform the reverse Equirectangular projection
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Calculate the latitude and longitude from the pixel coordinates
                    double latitude = ((double)y / height) * Math.PI - (Math.PI / 2);
                    double longitude = ((double)x / width) * 2 * Math.PI - Math.PI;

                    // Interpolate the HDR pixel value from the original HDR image
                    Vec3b hdrPixel = InterpolateHDRPixel(hdrImage, longitude, latitude);

                    // Assign the interpolated HDR pixel value to the reverse projection image
                    reverseProjection.Set(y, x, hdrPixel);
                }
            }

            // Save the reverse projection image to a file
            //reverseProjection.SaveImage("output.hdr");

            return reverseProjection;
        }

        static Vec3b InterpolateHDRPixel(Mat hdrImage, double longitude, double latitude)
        {
            // Convert the latitude and longitude to pixel coordinates in the HDR image
            int x = (int)((longitude + Math.PI) / (2 * Math.PI) * hdrImage.Width);
            int y = (int)((latitude + (Math.PI / 2)) / Math.PI * hdrImage.Height);

            // Clamp the pixel coordinates within the image boundaries
            x = Math.Min(Math.Max(x, 0), hdrImage.Width - 1);
            y = Math.Min(Math.Max(y, 0), hdrImage.Height - 1);

            // Retrieve the HDR pixel value from the image
            Vec3b hdrPixel = hdrImage.Get<Vec3b>(y, x);

            return hdrPixel;
        }

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
                        string[] resolution = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (resolution.Length != 4 || !int.TryParse(resolution[3], out width) || !int.TryParse(resolution[1], out height))
                        {
                            Console.WriteLine("Invalid resolution line." + width + " " + height);
                            return null;
                        }
                        Console.WriteLine(width + " " + height);

                        // Allocate pixel data array
                        pixelData = new float[width * height * 3];  // Assuming RGB format

                        // Read pixel values
                        // Read the binary data portion
                        //line = streamReader.ReadLine();
                        Console.WriteLine("File position: " + streamReader.BaseStream.Position);
                        byte[] binaryData = new byte[fileStream.Length - fileStream.Position];
                        //string rgbeData = streamReader.ReadToEnd();
                        fileStream.Read(binaryData, 0, binaryData.Length);
                        //byte[] rgbeByte = ConvertStringToBytes(rgbeData);

                        //pixelData = HDRParser.decodeRGBE2(binaryData, width, height);
                        pixelData = HDRParser.Decode32BitRleRGBE(binaryData);
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

        private static float[] DecodeRgbE(byte[] rgbeData, int width, int height)
        {
            int numPixels = width * height;
            float[] imageData = new float[numPixels * 3]; // RGB format

            int dataIndex = 0;
            int pixelIndex = 0;

            for (int i = 0; i < numPixels; i++)
            {
                byte rgbeIndicator = rgbeData[dataIndex++];
                byte rgbeValue = rgbeData[dataIndex++];

                if (rgbeIndicator == 0x02 && rgbeValue == 0x02)
                {
                    // New RLE scanline encoding
                    int runLength = BitConverter.ToUInt16(rgbeData, dataIndex);
                    dataIndex += 2;

                    if (runLength == 0)
                    {
                        throw new InvalidDataException("Invalid RLE scanline encoding.");
                    }

                    byte red = rgbeData[dataIndex++];
                    byte green = rgbeData[dataIndex++];
                    byte blue = rgbeData[dataIndex++];

                    for (int j = 0; j < runLength; j++)
                    {
                        imageData[pixelIndex++] = red / 255.0f;
                        imageData[pixelIndex++] = green / 255.0f;
                        imageData[pixelIndex++] = blue / 255.0f;
                    }
                }
                else if (rgbeIndicator == 0x01 && rgbeValue == 0x02)
                {
                    // Old RLE scanline encoding
                    int runLength = rgbeData[dataIndex++];

                    if (runLength == 0)
                    {
                        throw new InvalidDataException("Invalid RLE scanline encoding.");
                    }

                    byte red = rgbeData[dataIndex++];
                    byte green = rgbeData[dataIndex++];
                    byte blue = rgbeData[dataIndex++];

                    for (int j = 0; j < runLength; j++)
                    {
                        imageData[pixelIndex++] = red / 255.0f;
                        imageData[pixelIndex++] = green / 255.0f;
                        imageData[pixelIndex++] = blue / 255.0f;
                    }
                }
                else
                {
                    // Individual pixel encoding
                    float exponent = (float)Math.Pow(2, rgbeValue - 128);
                    imageData[pixelIndex++] = rgbeData[dataIndex++] / 255.0f * exponent;
                    imageData[pixelIndex++] = rgbeData[dataIndex++] / 255.0f * exponent;
                    imageData[pixelIndex++] = rgbeData[dataIndex++] / 255.0f * exponent;
                }
            }

            return imageData;
        }

        private static float[] decodeRGBE2(byte[] pixelData, int width, int height)
        {
            // Convert the pixel data to float RGB format
            Console.WriteLine("Bits of 32-bit_rle_rgbe read: " + pixelData.Length * 8);
            Console.WriteLine("Size of RGB output: " + (width * height * 3 * 32));


            int numPixels = width * height;
            float[] floatRGB = new float[3 * numPixels];

            int dataIndex = 0;
            int pixelIndex = 0;

            while (pixelIndex < numPixels)
            {
                Console.WriteLine(dataIndex + " " + pixelIndex + "<" + numPixels);
                byte controlByte = pixelData[dataIndex++];
                bool isRLE = (controlByte & 0x80) != 0;
                int count = controlByte & 0x7F;
                Console.WriteLine(isRLE + " " + count);
                if (isRLE)
                {
                    byte r = pixelData[dataIndex++];
                    byte g = pixelData[dataIndex++];
                    byte b = pixelData[dataIndex++];
                    byte e = pixelData[dataIndex++];

                    float maxComponent = Math.Max(Math.Max(r, g), b);
                    float scale = (float)Math.Pow(2, e - 128) / maxComponent;

                    for (int i = 0; i < count; i++)
                    {
                        floatRGB[pixelIndex * 3] = r * scale;
                        floatRGB[pixelIndex * 3 + 1] = g * scale;
                        floatRGB[pixelIndex * 3 + 2] = b * scale;
                        pixelIndex++;
                    }
                }
                else
                {
                    for (int i = 0; i < count; i++)
                    {
                        byte r = pixelData[dataIndex++];
                        byte g = pixelData[dataIndex++];
                        byte b = pixelData[dataIndex++];
                        byte e = pixelData[dataIndex++];

                        float maxComponent = Math.Max(Math.Max(r, g), b);
                        float scale = (float)Math.Pow(2, e - 128) / maxComponent;

                        floatRGB[pixelIndex * 3] = r * scale;
                        floatRGB[pixelIndex * 3 + 1] = g * scale;
                        floatRGB[pixelIndex * 3 + 2] = b * scale;
                        pixelIndex++;
                    }
                }
            }
            return floatRGB;
        }

        private static void printrgb(byte r, byte g, byte b, byte e, bool isRLE)
        {
            Console.WriteLine(r + " " + g + " " + b + " " + e + " " + isRLE);
        }

        public static float[] Decode32BitRleRGBE(byte[] encodedData)
        {
            List<float> decodedData = new List<float>();

            int index = 0;
            while (index < encodedData.Length)
            {
                if (encodedData[index++] == 0x02 && encodedData[index++] == 0x02 && encodedData[index++] >= 0x80)
                {
                    // Run length encoded scanline
                    int scanlineWidth = encodedData[index - 1] - 0x80;
                    float[] scanlineData = DecodeScanline(encodedData, index, scanlineWidth);
                    decodedData.AddRange(scanlineData);
                    index += 4 * scanlineWidth;
                }
                else
                {
                    // Individual scanline
                    float[] scanlineData = DecodeScanline(encodedData, index, 1);
                    decodedData.AddRange(scanlineData);
                    index += 4;
                }
            }

            return decodedData.ToArray();
        }

        private static float[] DecodeScanline(byte[] encodedData, int startIndex, int scanlineWidth)
        {
            float[] scanlineData = new float[3 * scanlineWidth];
            int scanlineIndex = 0;

            for (int i = 0; i < scanlineWidth; i++)
            {
                byte red = encodedData[startIndex + 0];
                byte green = encodedData[startIndex + 1];
                byte blue = encodedData[startIndex + 2];
                byte exponent = encodedData[startIndex + 3];

                Color rgbColor = ConvertRGBEToRGB(red, green, blue, exponent);
                scanlineData[scanlineIndex++] = rgbColor.R / 255f;
                scanlineData[scanlineIndex++] = rgbColor.G / 255f;
                scanlineData[scanlineIndex++] = rgbColor.B / 255f;

                startIndex += 4;
            }

            return scanlineData;
        }

        private static Color ConvertRGBEToRGB(byte red, byte green, byte blue, byte exponent)
        {
            float factor = (float)Math.Pow(2, exponent - 128) / 255f;
            float r = red * factor;
            float g = green * factor;
            float b = blue * factor;

            Console.WriteLine((int)r + " " + (int)g + " " + (int)b);
            return Color.FromArgb((int)r, (int)g, (int)b);
        }
    }

}

