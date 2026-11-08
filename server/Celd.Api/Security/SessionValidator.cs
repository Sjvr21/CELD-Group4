using System.Security.Claims;
using Celd.Api.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

namespace Celd.Api.Security;

public static class SessionValidator
{
    // BR-AUT-001/FR-AUT-002: signed cookie + SERVER-side session revocation.
    public static async Task Validate(CookieValidatePrincipalContext context)
    {
        var sid = context.Principal?.FindFirstValue("sid");
        var userText = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
        var role = context.Principal?.FindFirstValue(ClaimTypes.Role);
        if (string.IsNullOrEmpty(sid) || !int.TryParse(userText, out var uid))
        {
            context.RejectPrincipal(); return;
        }
        var db = context.HttpContext.RequestServices.GetRequiredService<CeldDbContext>();
        var valid = await db.LoginSessions.AsNoTracking().AnyAsync(s => s.Id == sid && !s.Revoked &&
            s.ExpiresUtc > DateTime.UtcNow && s.UserAccountId == uid && s.User.Role == role);
        if (!valid) context.RejectPrincipal();
    }
}
