using System.Security.Claims;

namespace Celd.Api.Security;

public static class CurrentActor
{
    public static int UserId(ClaimsPrincipal principal) =>
        int.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new InvalidOperationException("Missing user id in session."));
    public static string Role(ClaimsPrincipal principal) => principal.FindFirstValue(ClaimTypes.Role) ?? "";
}
