namespace Celd.Api.Domain;

// BR-RES-003: single shared policy; handlers remain focused on individual use cases.
public static class ReservationLifecycle
{
    public static bool CanTransition(string current, string target) =>
        (current,target) switch
        {
            (Statuses.Reserved,Statuses.Cancelled) => true,
            (Statuses.Reserved,Statuses.CheckedOut) => true,
            (Statuses.CheckedOut,Statuses.Returned) => true,
            _ => false
        };
}
