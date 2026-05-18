using System.Diagnostics;

namespace SpiEyes.Models;

public class Camera
{
    public string Name { get; set; }
    public Process StreamProc { get; set; }
    public Process FrameProc { get; set; }
    public Stream OutputStream { get; set; }
}
