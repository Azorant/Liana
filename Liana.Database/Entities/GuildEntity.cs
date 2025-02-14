using System.ComponentModel.DataAnnotations;
using Liana.Database.Models;

namespace Liana.Database.Entities;

public class GuildEntity
{
    [Key]
    public ulong Id { get; set; }
    public required GuildConfig Config { get; set; }
}