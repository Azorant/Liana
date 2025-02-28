namespace Liana.Models.Enums;

[Flags]
public enum ReactionRoleEnum
{
    Add = 1 << 0,
    Remove = 1 << 1,
    Both = Add | Remove,
}