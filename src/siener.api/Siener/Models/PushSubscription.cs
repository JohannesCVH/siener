using System.ComponentModel.DataAnnotations;

namespace Siener.Models;

public class PushSubscription
{
    
    [Key]
    public Guid Id { get; set; }
    public Guid UserID { get; set; }
    public string Endpoint { get; set; }
    public PushSubscriptionKeys Keys { get; set; }
}