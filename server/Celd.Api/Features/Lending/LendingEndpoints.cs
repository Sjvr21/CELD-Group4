using Celd.Api.Common;
using Celd.Api.Data;
using Celd.Api.Domain;
using Celd.Api.Features.Reservations;
using Celd.Api.Security;
using Microsoft.EntityFrameworkCore;

namespace Celd.Api.Features.Lending;

public static class LendingEndpoints
{
    public static IEndpointRouteBuilder MapLending(this IEndpointRouteBuilder app)
    {
        var staff=app.MapGroup("/api/staff/reservations").RequireAuthorization(Roles.Staff);
        staff.MapGet("", async (string? status, CeldDbContext db) =>
        {
            if (!string.IsNullOrEmpty(status) && !Statuses.All.Contains(status))
                return ApiErrors.Invalid("Unknown reservation status filter.");
            var query=db.Reservations.AsNoTracking().AsSplitQuery()
                .Include(x=>x.Student).Include(x=>x.EquipmentItem)
                .Include(x=>x.HistoryEvents).ThenInclude(x=>x.Actor).AsQueryable();
            if (!string.IsNullOrEmpty(status)) query=query.Where(x=>x.Status==status);
            var records=await query.OrderByDescending(x=>x.CreatedUtc).ToListAsync();
            return Results.Ok(records.Select(ReservationQueries.Map).ToList());
        });
        staff.MapPost("/{id:int}/pickup", (int id, HttpContext ctx, CeldDbContext db) =>
            ReservationTransitions.Apply(db,id,CurrentActor.UserId(ctx.User),Statuses.Reserved,Statuses.CheckedOut,false));
        staff.MapPost("/{id:int}/return", (int id, HttpContext ctx, CeldDbContext db) =>
            ReservationTransitions.Apply(db,id,CurrentActor.UserId(ctx.User),Statuses.CheckedOut,Statuses.Returned,false));
        return app;
    }
}
