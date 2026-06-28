namespace Siener.Lib.Models;

public class Config
{
    public string Email { get; set; }
    public VapidKeys VapidKeys { get; set; }
    public CameraConfig[] Cameras { get; set; }
    public string MediaMtxLocation { get; set; }
    public string OnnxLocation { get; set; }
}