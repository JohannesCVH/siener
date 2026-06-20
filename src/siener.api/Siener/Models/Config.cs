namespace Siener.Models;

public class Config
{
    public string Email { get; set; }
    public VapidKeys VapidKeys { get; set; }
    public CameraConfig[] Cameras { get; set; }
    public string MediaMtxLocation { get; set; }
}