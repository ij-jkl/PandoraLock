namespace Domain.Constants;

public static class Permissions
{
    public static class Users
    {
        public const string Read = "Permissions.Users.Read";
        public const string Create = "Permissions.Users.Create";
        public const string Update = "Permissions.Users.Update";
        public const string Delete = "Permissions.Users.Delete";
    }

    public static class Files
    {
        public const string Read = "Permissions.Files.Read";
        public const string Create = "Permissions.Files.Create";
        public const string Update = "Permissions.Files.Update";
        public const string Delete = "Permissions.Files.Delete";
    }

    public static class AuditLogs
    {
        public const string Read = "Permissions.AuditLogs.Read";
    }
}
