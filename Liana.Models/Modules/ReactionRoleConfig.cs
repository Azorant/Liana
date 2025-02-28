using Liana.Models.Enums;

namespace Liana.Models.Modules;

public class ReactionRoleConfig
{
    public ulong Id { get; set; }
    // Should this role be actioned on reaction add, remove, or both
    public ReactionRoleEnum Reaction { get; set; }
    // Should this role be added or removed when actioned
    public ReactionRoleEnum Action { get; set; }
    /// <summary>
    /// Roles required for this role to be given
    /// </summary>
    public List<ulong>? Prerequisite { get; set; } = null;
    /// <summary>
    /// Roles that prevent this role from being given
    /// </summary>
    public List<ulong>? Exclusive { get; set; } = null;
}