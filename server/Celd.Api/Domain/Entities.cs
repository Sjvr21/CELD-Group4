namespace Celd.Api.Domain;

public static class Roles
{
    public const string Student = "Student";
    public const string Staff = "Staff";
}

public static class Statuses
{
    public const string Reserved = "Reserved";
    public const string CheckedOut = "CheckedOut";
    public const string Cancelled = "Cancelled";
    public const string Returned = "Returned";
    public static readonly string[] All = [Reserved, CheckedOut, Cancelled, Returned];
    public static readonly string[] Active = [Reserved, CheckedOut];
}

public sealed class EquipmentItem
{
    public int Id { get; set; }
    public string AssetCode { get; set; } = "";
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public string Description { get; set; } = "";
    public List<Reservation> Reservations { get; set; } = [];
}

public sealed class UserAccount
{
    public int Id { get; set; }
    public string EmailNormalized { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string Role { get; set; } = "";
}

public sealed class LoginSession
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public int UserAccountId { get; set; }
    public UserAccount User { get; set; } = null!;
    public bool Revoked { get; set; }
    public DateTime ExpiresUtc { get; set; }
}

public sealed class Reservation
{
    public int Id { get; set; }
    public int EquipmentItemId { get; set; }
    public EquipmentItem EquipmentItem { get; set; } = null!;
    public int StudentId { get; set; }
    public UserAccount Student { get; set; } = null!;
    public string Status { get; set; } = Statuses.Reserved;
    public DateTime CreatedUtc { get; set; }
    public int Version { get; set; }
    public List<HistoryEvent> HistoryEvents { get; set; } = [];
}

public sealed class HistoryEvent
{
    public int Id { get; set; }
    public int ReservationId { get; set; }
    public Reservation Reservation { get; set; } = null!;
    public string EventType { get; set; } = "";
    public string ResultingStatus { get; set; } = "";
    public int ActorUserId { get; set; }
    public UserAccount Actor { get; set; } = null!;
    public DateTime OccurredUtc { get; set; }
}
