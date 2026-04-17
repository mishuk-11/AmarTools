namespace AmarTools.BuildingBlocks.Security;

/// <summary>
/// Centralised, compile-time-safe permission constants used across the RBAC system.
///
/// Convention: <c>Module.Action</c> — e.g. <c>Events.Create</c>, <c>Voting.Manage</c>.
/// These string values are stored in the <c>EventCoordinator</c> permissions column
/// and validated by the <c>PermissionAuthorizationHandler</c> in the Web project.
/// </summary>
public static class Permissions
{
    public static class Events
    {
        public const string View    = "Events.View";
        public const string Create  = "Events.Create";
        public const string Edit    = "Events.Edit";
        public const string Archive = "Events.Archive";
        public const string Delete  = "Events.Delete";
    }

    public static class Contacts
    {
        public const string View   = "Contacts.View";
        public const string Manage = "Contacts.Manage";
    }

    public static class PhotoFrame
    {
        public const string View   = "PhotoFrame.View";
        public const string Manage = "PhotoFrame.Manage";
    }

    public static class Certificates
    {
        public const string View     = "Certificates.View";
        public const string Generate = "Certificates.Generate";
        public const string Manage   = "Certificates.Manage";
    }

    public static class Cheques
    {
        public const string View   = "Cheques.View";
        public const string Print  = "Cheques.Print";
        public const string Manage = "Cheques.Manage";
    }

    public static class Voting
    {
        public const string View   = "Voting.View";
        public const string Vote   = "Voting.Vote";
        public const string Manage = "Voting.Manage";
    }
}
