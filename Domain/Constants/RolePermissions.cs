namespace Domain.Constants;

public static class RolePermissions
{
    public static IReadOnlyList<string> GetPermissionsForRole(string role)
    {
        return role switch
        {
            AppRoles.Admin => new List<string>
            {
                Permissions.Users.Read,
                Permissions.Users.Create,
                Permissions.Users.Update,
                Permissions.Users.Delete,
                Permissions.Files.Read,
                Permissions.Files.Create,
                Permissions.Files.Update,
                Permissions.Files.Delete,
                Permissions.AuditLogs.Read
            },
            AppRoles.Manager => new List<string>
            {
                Permissions.Users.Read,
                Permissions.Users.Create,
                Permissions.Users.Update,
                Permissions.Files.Read,
                Permissions.Files.Create,
                Permissions.Files.Update,
                Permissions.Files.Delete,
                Permissions.AuditLogs.Read
            },
            AppRoles.User => new List<string>
            {
                Permissions.Files.Read,
                Permissions.Files.Create,
                Permissions.Files.Update
            },
            _ => new List<string>()
        };
    }
}
