using System.Security.Claims;
using Celd.Api.Common;
using Celd.Api.Data;
using Celd.Api.Domain;
using Celd.Api.Security;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Celd.Api.Features.Authentication;

public static class AuthEndpoints
{
    public sealed record LoginRequest(string Email, string Password);
    public sealed record UserResponse(int Id, string DisplayName, string Role);

    public static IEndpointRouteBuilder MapAuth(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth");
        group.MapGet("/csrf", (HttpContext ctx, IAntiforgery anti) =>
        {
            ctx.Response.Headers.CacheControl="no-store";
            var tokens = anti.GetAndStoreTokens(ctx);
            return Results.Ok(new { requestToken = tokens.RequestToken });
        });
        group.MapGet("/me", async (HttpContext ctx, CeldDbContext db) =>
        {
            if (ctx.User.Identity?.IsAuthenticated != true) return ApiErrors.Unauthenticated();
            var id=CurrentActor.UserId(ctx.User);
            var u=await db.UserAccounts.AsNoTracking().Where(x=>x.Id==id)
                .Select(x=>new UserResponse(x.Id,x.DisplayName,x.Role)).SingleOrDefaultAsync();
            return u is null ? ApiErrors.Unauthenticated() : Results.Ok(u);
        });
        group.MapPost("/login", async (LoginRequest? request, HttpContext ctx, CeldDbContext db) =>
        {
            if (request is null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return ApiErrors.Invalid("Email and password are required.");
            var key=request.Email.Trim().ToUpperInvariant();
            var user=await db.UserAccounts.SingleOrDefaultAsync(x=>x.EmailNormalized==key);
            // Do not disclose which credential was wrong; never log entered passwords.
            var hasher=new PasswordHasher<UserAccount>();
            if (user is null || hasher.VerifyHashedPassword(user,user.PasswordHash,request.Password)==PasswordVerificationResult.Failed)
                return Results.Problem(statusCode:401,title:"Invalid credentials",detail:"Email or password is incorrect.");
            var session=new LoginSession { UserAccountId=user.Id, ExpiresUtc=DateTime.UtcNow.AddHours(8) };
            db.LoginSessions.Add(session);
            await db.SaveChangesAsync();
            var claims=new List<Claim>
            {
                new(ClaimTypes.NameIdentifier,user.Id.ToString()), new(ClaimTypes.Name,user.DisplayName),
                new(ClaimTypes.Role,user.Role), new("sid",session.Id)
            };
            var principal=new ClaimsPrincipal(new ClaimsIdentity(claims,CookieAuthenticationDefaults.AuthenticationScheme));
            await ctx.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,principal,
                new AuthenticationProperties { IsPersistent=false, ExpiresUtc=session.ExpiresUtc });
            return Results.Ok(new UserResponse(user.Id,user.DisplayName,user.Role));
        });
        group.MapPost("/logout", async (HttpContext ctx, CeldDbContext db) =>
        {
            if (ctx.User.Identity?.IsAuthenticated != true) return ApiErrors.Unauthenticated();
            var sid=ctx.User.FindFirstValue("sid");
            if (!string.IsNullOrEmpty(sid))
                await db.LoginSessions.Where(x=>x.Id==sid).ExecuteUpdateAsync(s=>s.SetProperty(x=>x.Revoked,true));
            await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Results.Ok(new { message="Signed out" });
        }).RequireAuthorization();
        return app;
    }
}
