using System.ComponentModel.DataAnnotations;

namespace SpiEyes.Models;

public class User
{
    [Key]
    public Guid Id { get; set; }
    public string Username { get; set; }
}