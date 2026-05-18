using System.ComponentModel.DataAnnotations;

namespace Jamera.Models;

public class User
{
    [Key]
    public Guid Id { get; set; }
    public string Username { get; set; }
}