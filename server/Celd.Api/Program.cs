using Celd.Api.Common;
using Celd.Api.Data;
using Celd.Api.Domain;
using Celd.Api.Features.Authentication;
using Celd.Api.Features.Catalog;
using Celd.Api.Features.Lending;
using Celd.Api.Features.Reservations;
using Celd.Api.Security;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder=WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<CeldDbContext>(o=>o.UseSqlite(
    builder.Configuration.GetConnectionString("Celd") ?? "Data Source=celd.db;Cache=Shared;Default Timeout=15"));
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o=>
    {
        o.Cookie.Name="CELD.Session";
        o.Cookie.HttpOnly=true;
        o.Cookie.SameSite=SameSiteMode.Lax;
        o.Cookie.SecurePolicy=builder.Environment.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
        o.ExpireTimeSpan=TimeSpan.FromHours(8);
        o.SlidingExpiration=false;
        o.Events.OnValidatePrincipal=SessionValidator.Validate;
        o.Events.OnRedirectToLogin=async ctx => { ctx.Response.StatusCode=401;
            await ApiErrors.Unauthenticated().ExecuteAsync(ctx.HttpContext); };
        o.Events.OnRedirectToAccessDenied=async ctx => { ctx.Response.StatusCode=403;
            await ApiErrors.Forbidden().ExecuteAsync(ctx.HttpContext); };
    });
builder.Services.AddAuthorization(options=>
{
    options.AddPolicy(Roles.Student,p=>p.RequireAuthenticatedUser().RequireRole(Roles.Student));
    options.AddPolicy(Roles.Staff,p=>p.RequireAuthenticatedUser().RequireRole(Roles.Staff));
});
builder.Services.AddAntiforgery(o=>{ o.HeaderName="X-CSRF-TOKEN"; o.Cookie.Name="CELD.Antiforgery";
    o.Cookie.HttpOnly=true; o.Cookie.SameSite=SameSiteMode.Lax;
    o.Cookie.SecurePolicy=builder.Environment.IsDevelopment()?CookieSecurePolicy.SameAsRequest:CookieSecurePolicy.Always;
});
builder.Services.AddProblemDetails();

var app=builder.Build();
app.UseExceptionHandler();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
// NFR-SEC-002: validate CSRF token on EVERY unsafe /api action, including login/logout.
app.Use(async (ctx,next)=>
{
    if (ctx.Request.Path.StartsWithSegments("/api") &&
        !HttpMethods.IsGet(ctx.Request.Method) && !HttpMethods.IsHead(ctx.Request.Method) &&
        !HttpMethods.IsOptions(ctx.Request.Method))
    {
        var anti=ctx.RequestServices.GetRequiredService<IAntiforgery>();
        try { await anti.ValidateRequestAsync(ctx); }
        catch (AntiforgeryValidationException)
        {
            await ApiErrors.Invalid("CSRF verification failed. Reload the page and try again.").ExecuteAsync(ctx);
            return;
        }
    }
    await next();
});
app.MapAuth();
app.MapCatalog();
app.MapReservations();
app.MapLending();
app.MapGet("/api/health",()=>Results.Ok(new { status="healthy" }));
if (app.Environment.WebRootFileProvider.GetFileInfo("index.html").Exists)
    app.MapFallbackToFile("index.html");
await DbInitializer.InitializeAsync(app.Services,app.Configuration);
app.Run();

public partial class Program { }
