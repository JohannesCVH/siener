using System;

namespace SpiEyes.Utility;

public class PathUtils
{
    public static string CreateAppPath(string? path = default) => 
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "SpiEyes", path ?? string.Empty);
    
    public static string CreateStreamsPath(string? path = default) =>
        Path.Combine(CreateAppPath(), "Streams", path ?? string.Empty);
}
