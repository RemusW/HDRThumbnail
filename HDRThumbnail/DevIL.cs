using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevILSharp;

namespace HDRThumbnail
{
    public class DevILTest
    {
        public static void loadHDR(string filepath)
        {
            IL.Init();
            DevILSharp.ILU.Init();

            Image im = DevILSharp.Image.Load("");
            
            int imageId = IL.GenImage(); // Generate an image ID
            IL.BindImage(imageId); // Bind the image ID
            IL.LoadImage("path/to/your/hdr/file.hdr"); // Load the HDR file
            IL.DetermineType("path/to/your/hdr/file.hdr");
            
            if (IL.GetError() == ErrorCode.NoError)
            {
                // Access the pixel data



                // Process the HDR data as needed
                // ...
                IL.Save(ImageType.Png, "D:\\Code workshop\\clo\\HDRThumbnail\\HDRThumbnail\\devil.png");

                // Free the loaded image
                IL.DeleteImage(imageId);
            }


        }
    }
}
