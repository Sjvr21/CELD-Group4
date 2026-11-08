using Celd.Api.Common;
using Celd.Api.Data;
using Celd.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace Celd.Api.Features.Reservations;

public static class ReservationTransitions
{
    // BR-RES-003: legal transition policy defined in Domain/ReservationLifecycle.cs.

    public static async Task<IResult> Apply(CeldDbContext db, int reservationId, int actorId,
        string from, string to, bool mustOwn)
    {
        if (!ReservationLifecycle.CanTransition(from,to)) return ApiErrors.Invalid("Unsupported lifecycle transition.");
        var current=await db.Reservations.AsNoTracking().Where(x=>x.Id==reservationId)
            .Select(x=>new { x.StudentId,x.Status,x.Version }).SingleOrDefaultAsync();
        if (current is null) return ApiErrors.Missing();
        // BR-AUT-002: prevent existence disclosure through other students' reservation IDs.
        if (mustOwn && current.StudentId!=actorId) return ApiErrors.Missing();
        if (current.Status!=from) return ApiErrors.Conflict("This action is not valid for the current reservation status.");
        try
        {
            // BR-RES-004 / AC-10: transaction contains BOTH conditional status update and History insert.
            await using var tx=await db.Database.BeginTransactionAsync();
            // BR-RES-003/006: compare-and-swap prevents stale pickup/cancel/return overwrites.
            var updated=await db.Reservations.Where(x=>x.Id==reservationId && x.Status==from && x.Version==current.Version)
                .ExecuteUpdateAsync(s=>s.SetProperty(x=>x.Status,to).SetProperty(x=>x.Version,x=>x.Version+1));
            if (updated!=1)
            {
                await tx.RollbackAsync();
                return ApiErrors.Conflict("A concurrent action changed this reservation.");
            }
            db.HistoryEvents.Add(new HistoryEvent { ReservationId=reservationId,ActorUserId=actorId,
                EventType=to,ResultingStatus=to,OccurredUtc=DateTime.UtcNow });
            await db.SaveChangesAsync();
            await tx.CommitAsync();
        }
        catch (Exception ex) when (ApiErrors.IsConcurrencyFailure(ex))
        {
            db.ChangeTracker.Clear();
            return ApiErrors.Conflict("A concurrent action changed this reservation.");
        }
        // Unexpected DB exceptions propagate to 500; transaction disposal rolls back.
        return Results.Ok(await ReservationQueries.Detail(db,reservationId));
    }
}
