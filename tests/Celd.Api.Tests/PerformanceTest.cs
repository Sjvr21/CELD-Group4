using System.Diagnostics;
using System.Net;
using Celd.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;

namespace Celd.Api.Tests;

public sealed class PerformanceTest(ITestOutputHelper output)
{
    [Fact]
    public async Task AC16_Fifty_items_and_hundred_historical_reservations_meet_two_second_bound()
    {
        using var app=new TestApp(); using var staff=await app.Staff();
        await using (var db=app.Database())
        {
            var existing=await db.EquipmentItems.CountAsync();
            var extra=Enumerable.Range(existing+1,Math.Max(0,50-existing))
                .Select(n=>new EquipmentItem { AssetCode=$"PERF-{n:000}",Name=$"Equipment {n}",Category="Test",Description="Local test fixture" }).ToList();
            db.EquipmentItems.AddRange(extra);
            await db.SaveChangesAsync();
            var student=await db.UserAccounts.FirstAsync(x=>x.Role==Roles.Student);
            var staffUser=await db.UserAccounts.FirstAsync(x=>x.Role==Roles.Staff);
            var equipmentId=await db.EquipmentItems.Select(x=>x.Id).FirstAsync();
            for(var i=0;i<100;i++)
            {
                var now=DateTime.UtcNow.AddDays(-100+i);
                var record=new Reservation {EquipmentItemId=equipmentId,StudentId=student.Id,
                    Status=Statuses.Returned,Version=3,CreatedUtc=now};
                record.HistoryEvents.Add(new HistoryEvent {ActorUserId=student.Id,EventType="Created",ResultingStatus=Statuses.Reserved,OccurredUtc=now});
                record.HistoryEvents.Add(new HistoryEvent {ActorUserId=staffUser.Id,EventType=Statuses.CheckedOut,ResultingStatus=Statuses.CheckedOut,OccurredUtc=now.AddHours(1)});
                record.HistoryEvents.Add(new HistoryEvent {ActorUserId=staffUser.Id,EventType=Statuses.Returned,ResultingStatus=Statuses.Returned,OccurredUtc=now.AddHours(2)});
                db.Reservations.Add(record);
            }
            await db.SaveChangesAsync();
        }
        // warm-up: do not include process startup/first EF query initialization.
        Assert.Equal(HttpStatusCode.OK,(await staff.GetAsync("/api/equipment")).StatusCode);
        Assert.Equal(HttpStatusCode.OK,(await staff.GetAsync("/api/staff/reservations")).StatusCode);
        var timer=Stopwatch.StartNew();var catalog=await staff.GetAsync("/api/equipment");timer.Stop();
        var catalogMs=timer.Elapsed.TotalMilliseconds;
        timer.Restart();var reservations=await staff.GetAsync("/api/staff/reservations");timer.Stop();
        var listMs=timer.Elapsed.TotalMilliseconds;
        output.WriteLine($"Local provider=SQLite; items>=50; historical reservations=100; after warm-up; catalog_ms={catalogMs:F1}; staff_list_ms={listMs:F1}; environment={Environment.OSVersion}");
        Assert.Equal(HttpStatusCode.OK,catalog.StatusCode);
        Assert.Equal(HttpStatusCode.OK,reservations.StatusCode);
        Assert.True(catalogMs<=2000,$"Catalog took {catalogMs:F0}ms (limit 2000)");
        Assert.True(listMs<=2000,$"Reservation list took {listMs:F0}ms (limit 2000)");
    }
}
