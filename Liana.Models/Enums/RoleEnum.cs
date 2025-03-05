namespace Liana.Models.Enums;

[Flags]
public enum RoleEnum
{
    Staff = 1 << 0,
    Moderator = 1 << 1,
    Admin = Staff | Moderator
}