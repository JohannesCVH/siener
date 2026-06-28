namespace Siener.Utility;

public class PathUtils
{
    public static string GetAppPath(string? path = default) => 
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".siener", path ?? string.Empty);
    
    public static string GetCamerasPath() =>
        Path.Combine(GetAppPath(), "Cameras");

    public static string GetCameraPath(string camera) =>
        Path.Combine(GetCamerasPath(), camera);
}