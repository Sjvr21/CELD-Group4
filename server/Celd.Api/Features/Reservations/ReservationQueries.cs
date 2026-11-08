using Celd.Api.Data;
using Celd.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace Celd.Api.Features.Reservations;

public static class ReservationQueries
{
    public sealed record HistoryDto(int Id, string EventType, string ResultingStatus, int ActorUserId, string ActorName, DateTime OccurredUtc);
    public sealed record ReservationDto(int Id, int EquipmentId, string AssetCode, string EquipmentName,
        int StudentId, string StudentName, string Status, DateTime CreatedUtc, IReadOnlyList<HistoryDto> History);

    public static async Task<ReservationDto?> Detail(CeldDbContext db, int id)
    {
        var r=await db.Reservations.AsNoTracking().AsSplitQuery()
            .Include(x=>x.Student).Include(x=>x.EquipmentItem)
            .Include(x=>x.HistoryEvents).ThenInclude(h=>h.Actor)
            .SingleOrDefaultAsync(x=>x.Id==id);
        return r is null ? null : Map(r);
    }

    public static ReservationDto Map(Reservation r) => new(r.Id,r.EquipmentItemId,r.EquipmentItem.AssetCode,
        r.EquipmentItem.Name,r.StudentId,r.Student.DisplayName,r.Status,r.CreatedUtc,
        r.HistoryEvents.OrderBy(h=>h.OccurredUtc).ThenBy(h=>h.Id)
            .Select(h=>new HistoryDto(h.Id,h.EventType,h.ResultingStatus,h.ActorUserId,h.Actor.DisplayName,h.OccurredUtc)).ToList());
}
