using SpiEyes.Models;

namespace SpiEyes.Utility;

public class DirectoryUtils
{
    public static void GenerateStreamFolders(CameraConfig[] cameras)
    {
        string basePath = PathUtils.CreateAppPath("Streams");
        Directory.Delete(basePath, true); //Clean up old files.
        Directory.CreateDirectory(basePath);

        foreach (CameraConfig cam in cameras)
        {
            string camPath = Path.Combine(basePath, cam.Name);
            Directory.CreateDirectory(camPath);
            string framesPath = Path.Combine(camPath, "Frames");
            Directory.CreateDirectory(framesPath);
            string segmentsPath = Path.Combine(camPath, "Recordings");
            Directory.CreateDirectory(segmentsPath);
        }
    }
}