using Celd.Api.Common;
using Celd.Api.Data;
using Celd.Api.Security;
using Celd.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace Celd.Api.Features.Reservations;

public static class ReservationEndpoints
{
    public static IEndpointRouteBuilder MapReservations(this IEndpointRouteBuilder app)
    {
        var group=app.MapGroup("/api/reservations").RequireAuthorization();
        group.MapPost("", CreateReservation.Handle).RequireAuthorization(Roles.Student);
        group.MapGet("/mine", async (HttpContext ctx, CeldDbContext db) =>
        {
            var userId=CurrentActor.UserId(ctx.User);
            var reservations=await db.Reservations.AsNoTracking().AsSplitQuery()
                .Include(x=>x.Student).Include(x=>x.EquipmentItem)
                .Include(x=>x.HistoryEvents).ThenInclude(x=>x.Actor)
                .Where(x=>x.StudentId==userId).OrderByDescending(x=>x.CreatedUtc).ToListAsync();
            return Results.Ok(reservations.Select(ReservationQueries.Map).ToList());
        }).RequireAuthorization(Roles.Student);
        group.MapGet("/{id:int}", async (int id, HttpContext ctx, CeldDbContext db) =>
        {
            var record=await ReservationQueries.Detail(db,id);
            if (record is null) return ApiErrors.Missing();
            if (CurrentActor.Role(ctx.User)==Roles.Student && record.StudentId!=CurrentActor.UserId(ctx.User))
                return ApiErrors.Missing();
            return Results.Ok(record);
        });
        group.MapPost("/{id:int}/cancel", (int id, HttpContext ctx, CeldDbContext db) =>
            ReservationTransitions.Apply(db,id,CurrentActor.UserId(ctx.User),Statuses.Reserved,Statuses.Cancelled,true))
            .RequireAuthorization(Roles.Student);
        return app;
    }
}
