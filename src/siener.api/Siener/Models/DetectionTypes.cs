namespace Siener.Models;

[Flags]
public enum DetectionTypes
{
    None    = 0,
    Person  = 1 << 0,
    Dog     = 1 << 1,
    Car     = 1 << 2
}