using Celd.Api.Common;
using Celd.Api.Data;
using Celd.Api.Domain;
using Celd.Api.Security;
using Microsoft.EntityFrameworkCore;

namespace Celd.Api.Features.Reservations;

public static class CreateReservation
{
    public sealed record Request(int EquipmentId);

    // FR-RES-001, BR-RES-001/002/004: scoped actor, real DB unique index and atomic SaveChanges.
    public static async Task<IResult> Handle(Request? request, HttpContext ctx, CeldDbContext db)
    {
        if (request is null || request.EquipmentId<=0) return ApiErrors.Invalid("A valid equipmentId is required.");
        var studentId=CurrentActor.UserId(ctx.User); // derived from server-validated session, NOT request body
        var item=await db.EquipmentItems.AsNoTracking().AnyAsync(x=>x.Id==request.EquipmentId);
        if (!item) return ApiErrors.Missing();
        if (await db.Reservations.AnyAsync(x=>x.EquipmentItemId==request.EquipmentId &&
            (x.Status==Statuses.Reserved || x.Status==Statuses.CheckedOut)))
            return ApiErrors.Conflict("This equipment item is no longer available.");
        var now=DateTime.UtcNow;
        var reservation=new Reservation { EquipmentItemId=request.EquipmentId, StudentId=studentId,
            Status=Statuses.Reserved, CreatedUtc=now, Version=1 };
        // New reservation + creation event in the SAME EF Core SaveChanges transaction.
        reservation.HistoryEvents.Add(new HistoryEvent { ActorUserId=studentId, EventType="Created",
            ResultingStatus=Statuses.Reserved, OccurredUtc=now });
        db.Reservations.Add(reservation);
        try
        {
            await db.SaveChangesAsync();
            var detail=await ReservationQueries.Detail(db,reservation.Id);
            return Results.Created($"/api/reservations/{reservation.Id}",detail);
        }
        catch (Exception ex) when (ApiErrors.IsConcurrencyFailure(ex))
        {
            db.ChangeTracker.Clear();
            return ApiErrors.Conflict("Competing request or database constraint: the item is not available.");
        }
    }
}
