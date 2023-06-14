# HDRThumbnail

HDRThumbnailLib is a package for creating jpg/png thumbnails from hdr files

```
using HDRThumbnail;
// createThumbnail(string filePath, string outputPath, [Optional] int width, [Optional] int height, int horizontalFOV = 100)
// FOV must be set between (0,180) degrees otherwise it will default to 100
// If only one width or one height parameter is passed, the unassigned dimension will duplicate the passed-in value.
HDRThumbnail.HDRParser.createThumbnail("../input.hdr, "../output.hdr, 1024, 512, 100);
```

# Nuget
https://www.nuget.org/packages/HDRThumbnailLib/1.0.0
