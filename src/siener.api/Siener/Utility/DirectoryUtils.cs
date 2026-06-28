using Siener.Lib.Models;

namespace Siener.Utility;

public class DirectoryUtils
{
    public static void GenerateCameraFolders(CameraConfig[] cameraConfigs)
    {
        string basePath = PathUtils.GetAppPath("Cameras");
        if (Directory.Exists(basePath))
            Directory.Delete(basePath, true); //Clean up old files.
        Directory.CreateDirectory(basePath);

        foreach (CameraConfig cam in cameraConfigs)
        {
            string camPath = Path.Combine(basePath, cam.Name);
            Directory.CreateDirectory(camPath);
            
            string framePath = Path.Combine(camPath, "Frames");
            Directory.CreateDirectory(framePath);

            string segmentPath = Path.Combine(camPath, "Recordings");
            Directory.CreateDirectory(segmentPath);
        }
    }
}