using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SpiEyes.Models;

public class PushSubscriptionKeys
{
    [Key]
    public Guid Id { get; set; }
    [ForeignKey("PushSubscription"), Required]
    public Guid PushSubscriptionId { get; set; }
    public string P256dh { get; set; }
    public string Auth { get; set; }
}