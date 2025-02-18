using Liana.Models.Enums;

namespace Liana.Models.Modules;

public class MessagingConfig
{
    public MessagingEnum Type {get; set;}
    public ulong? ChannelId { get; set; }
    public string? Content { get; set; }
    public List<string> Embeds { get; set; } = new();
}