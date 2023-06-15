using System;
using OpenCvSharp;
using System.Runtime.InteropServices;
using System.IO;

namespace HDRThumbnail
{
    public class HDRParser
    {
        public static void createThumbnail(string filePath, string outputPath, [Optional] int width, [Optional] int height, int horizontalFOV = 100)
        {
            if (width == 0 && height == 0)
            {
                width = 1024;
                height = 512;
            }
            else if (width == 0 || height == 0)
            {
                width = width | height;
                height = width | height;
            }
            if (horizontalFOV > 180 || horizontalFOV < 0)
            {
                throw new ArgumentOutOfRangeException("horizontalFOV");
            }

            Mat image = Cv2.ImRead(filePath, ImreadModes.AnyColor | ImreadModes.AnyDepth);

            // Check if the image was successfully loaded
            if (image.Empty())
            {
                Console.WriteLine("Failed to load the image.");
                throw new FileLoadException("Error loading input file");
            }
            else
            {
                // Gamma correction to fit 8-bit rgb
                Mat ldrImage = new Mat(image.Height, image.Width, MatType.CV_8UC3);

                for (int y = 0; y < image.Height; y++)
                {
                    for (int x = 0; x < image.Width; x++)
                    {
                        Vec3b pixel = new Vec3b();
                        for (int i = 0; i < 3; i++)
                        {
                            float color = (float)Math.Pow(image.Get<Vec3f>(y, x)[i], 1.0f / 2.2f);
                            pixel[i] = (byte)Math.Max(0, Math.Min(255, (color * 255.0)));
                        }
                        ldrImage.Set<Vec3b>(y, x, pixel);
                    }
                }

                // Create and save perspective projection image
                Mat thumbnail = hdrToperspective(ldrImage, width, height, horizontalFOV);
                bool writeImage = thumbnail.SaveImage(outputPath);
                if (writeImage)
                    Console.WriteLine("Image saved successfully to " + outputPath);
                else
                    throw new Exception("Failed to write output image");
            }
        }


        private static Mat hdrToperspective(Mat equirectangularImage, int width, int height, int horizontalFOV)
        {
            int perspectiveWidth = width;
            int perspectiveHeight = height;
            float fieldOfView = horizontalFOV;
            float vFieldOfView = fieldOfView / (perspectiveWidth / perspectiveHeight);
            float cameraYaw = 0;
            float cameraPitch = 0;
            float cameraRoll = 0;

            // Create an empty perspective image
            Mat perspectiveImage = new Mat(perspectiveHeight, perspectiveWidth, MatType.CV_8UC3);

            // Convert field of view to radians
            float hFovRadians = fieldOfView * (float)Math.PI / 180.0f;
            float vFovRadians = vFieldOfView * (float)Math.PI / 180.0f;

            // Iterate over each pixel in the perspective image
            for (int y = 0; y < perspectiveHeight; y++)
            {
                for (int x = 0; x < perspectiveWidth; x++)
                {
                    // Convert pixel coordinates to normalized device coordinates (NDC)
                    float ndcX = 2.0f * (x / (float)perspectiveWidth) - 1.0f;
                    float ndcY = 2.0f * (y / (float)perspectiveHeight) - 1.0f;

                    // Apply field of view angle to the NDC coordinates
                    float fovCorrectedX = ndcX * hFovRadians / 2;
                    float fovCorrectedY = ndcY * vFovRadians / 2;

                    // Convert corrected NDC to spherical coordinates
                    float elevation = (float)Math.Asin(fovCorrectedY);
                    float azimuth = (float)Math.Atan2(fovCorrectedX, 1);

                    // Convert spherical coordinates to Cartesian coordinates
                    float xCartesian = (float)(Math.Cos(elevation) * Math.Sin(azimuth));
                    float yCartesian = (float)Math.Sin(elevation);
                    float zCartesian = (float)(Math.Cos(elevation) * Math.Cos(azimuth));

                    // Apply camera rotations
                    RotatePoint(ref xCartesian, ref yCartesian, ref zCartesian, cameraYaw, cameraPitch, cameraRoll);

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


            return perspectiveImage;
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
    }
}

