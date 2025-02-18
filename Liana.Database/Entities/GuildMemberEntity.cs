using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Liana.Database.Entities;

public class GuildMemberEntity
{
    [Key]
    public ulong Id { get; set; }
    [ForeignKey(nameof(Guild))]
    public ulong GuildId { get; set; }
    public virtual GuildEntity Guild { get; set; } = null!;
    public List<ulong> PersistentRoles { get; set; } = new();
}