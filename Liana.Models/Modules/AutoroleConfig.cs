namespace Liana.Models.Modules;

public class AutoroleConfig
{
    public List<ulong>? UserRoles { get; set; }
    public List<ulong>? BotRoles { get; set; }
    public List<ulong>? PersistentRoles { get; set; }
}