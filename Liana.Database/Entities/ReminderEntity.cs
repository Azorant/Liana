using System.ComponentModel.DataAnnotations;

namespace Liana.Database.Entities;

public class ReminderEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public ulong UserId { get; set; }
    public ulong ChannelId { get; set; }
    [MaxLength(4096)]
    public required string Content { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime RemindAt { get; set; }
}