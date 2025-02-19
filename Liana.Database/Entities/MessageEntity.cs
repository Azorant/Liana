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
    public List<ContentEdit>? ContentEdits { get; set; }
    public List<string> Attachments { get; set; } = new();
    public List<AttachmentsEdit>? AttachmentsEdits { get; set; }
    public bool Deleted { get; set; }
}

public class ContentEdit
{
    [MaxLength(4000)]
    public string Content { get; set; }
    public DateTime Date { get; set; }
}

public class AttachmentsEdit
{
    public List<string> Attachments { get; set; } = new();
    public DateTime Date { get; set; }
}