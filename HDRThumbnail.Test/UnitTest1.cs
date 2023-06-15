using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Security.Principal;
using OpenCvSharp;

namespace HDRThumbnail.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void LoadHDR()
        {
            string inputPath = MakeFilePath("hdr", "invalid.hdr");
            string outputPath = MakeFilePath("output", "test1.png");
            int width = 512;
            int height = 256;
            int hFOV = 100;

            Assert.ThrowsException<FileLoadException>(() => HDRParser.createThumbnail(inputPath, outputPath, width, height, hFOV));
        }

        [TestMethod]
        public void InvalidHFOV()
        {
            string inputPath = MakeFilePath("hdr", "little_paris_eiffel_tower_4k.hdr");
            string outputPath = MakeFilePath("output", "test2.png");
            int width = 512;
            int height = 256;
            int hFOV = 181;

            // Act and assert
            Assert.ThrowsException<System.ArgumentOutOfRangeException>(() => HDRParser.createThumbnail(inputPath, outputPath, width, height, hFOV));
        }

        [TestMethod]
        public void CheckOutputDimensions()
        {
            string inputPath = MakeFilePath("hdr", "little_paris_eiffel_tower_4k.hdr");
            string outputPath = MakeFilePath("output", "CheckOutputDimensions.png");
            int width = 512;
            int height = 256;
            int hFOV = 100;

            HDRParser.createThumbnail(inputPath, outputPath, width, height, hFOV);

            Mat image = Cv2.ImRead(outputPath, ImreadModes.AnyColor | ImreadModes.AnyDepth);

            Assert.AreEqual(image.Width, width);
            Assert.AreEqual(image.Height, height);
        }

        [TestMethod]
        public void DimensionsMirror()
        {
            string inputPath = MakeFilePath("hdr", "industrial_sunset_puresky_1k.hdr");
            string outputPath = MakeFilePath("output", "test3.png");
            int iwidth = 512;
            int iheight = 256;
            int hFOV = 100;

            HDRParser.createThumbnail(inputPath, outputPath, iwidth, horizontalFOV: hFOV);
            Mat image = Cv2.ImRead(outputPath, ImreadModes.AnyColor | ImreadModes.AnyDepth);
            Assert.AreEqual(image.Width, image.Height);

            HDRParser.createThumbnail(inputPath, outputPath, height: iheight, horizontalFOV: hFOV);
            image = Cv2.ImRead(outputPath, ImreadModes.AnyColor | ImreadModes.AnyDepth);
            Assert.AreEqual(image.Width, image.Height);
        }

        [TestMethod]
        public void VerticalImage()
        {
            string inputPath = MakeFilePath("hdr", "industrial_sunset_puresky_1k.hdr");
            string outputPath = MakeFilePath("output", "VerticalImage.png");
            int iwidth = 1024;
            int iheight = 512;
            int hFOV = 100;

            HDRParser.createThumbnail(inputPath, outputPath, iwidth, iheight, hFOV);
            Mat image = Cv2.ImRead(outputPath, ImreadModes.AnyColor | ImreadModes.AnyDepth);
        }

        private string MakeFilePath(string baseFolderName, string fileName)
        {
            string projectRoot = Directory.GetParent(Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory)).FullName;
            return Path.Combine(projectRoot, baseFolderName, fileName);
        }
    }
}
