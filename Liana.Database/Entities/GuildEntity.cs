using System.ComponentModel.DataAnnotations;
using Liana.Models;

namespace Liana.Database.Entities;

public class GuildEntity
{
    [Key]
    public ulong Id { get; set; }
    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public required string Config { get; set; }
}