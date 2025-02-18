using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Liana.Database.Entities;

public class MessageEntity
{
    [Key]
    public ulong Id { get; set; }
    [ForeignKey(nameof(Guild))]
    public ulong GuildId { get; set; }
    public virtual GuildEntity Guild { get; set; } = null!;
    public ulong ChannelId {get; set;}
    public ulong AuthorId {get; set;}
    [MaxLength(255)]
    public required string AuthorTag { get; set; }
    [MaxLength(4000)]
    public string? Content { get; set; }
    [MaxLength(4000)]
    public string? EditedContent { get; set; }
    public List<string> Attachments { get; set; } = new();
    public bool Deleted { get; set; }
}