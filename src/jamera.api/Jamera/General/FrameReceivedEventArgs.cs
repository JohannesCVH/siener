using SixLabors.ImageSharp;

namespace SpiEyes.General;

public class FrameReceivedEventArgs : EventArgs
{
    public Image Frame { get; }
    public long FrameNumber { get; }

    public FrameReceivedEventArgs(Image frame, long frameNumber)
    {
        Frame = frame;
        FrameNumber = frameNumber;
    }
}