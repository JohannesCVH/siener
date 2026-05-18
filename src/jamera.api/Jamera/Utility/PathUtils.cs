using System;

namespace Jamera.Utility;

public class PathUtils
{
    public static string CreateAppPath(string? path = default) => 
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Jamera", path ?? string.Empty);
    
    public static string CreateStreamsPath(string? path = default) =>
        Path.Combine(CreateAppPath(), "Streams", path ?? string.Empty);
}
