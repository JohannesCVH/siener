namespace Siener.Data.Entities;

public class Event
{
    public int Id { get; set; }
    public required string Camera { get; set; }
    public short DetectionTypes { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public bool Notified { get; set; }
}