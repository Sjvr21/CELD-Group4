using System.Net;
using System.Net.Http.Json;
using Celd.Api.Domain;
using Celd.Api.Features.Catalog;
using Celd.Api.Features.Reservations;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Celd.Api.Tests;

public sealed class FeatureIntegrationTests
{
    private static async Task<int> FirstItem(HttpClient c)
    {
        var items=await c.GetFromJsonAsync<List<CatalogEndpoints.EquipmentResponse>>("/api/equipment");
        Assert.NotNull(items);
        Assert.True(items.Count>=6);
        return items[0].Id;
    }
    private static async Task<ReservationQueries.ReservationDto> Create(HttpClient c,int id)
    {
        var response=await TestApp.Post(c,"/api/reservations",new { equipmentId=id });
        Assert.Equal(HttpStatusCode.Created,response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<ReservationQueries.ReservationDto>())!;
    }

    [Fact]
    public async Task AC01_Public_catalog_search_category_empty_and_not_found()
    {
        using var app=new TestApp();using var visitor=app.Client();
        var id=await FirstItem(visitor);
        Assert.Equal(HttpStatusCode.OK,(await visitor.GetAsync($"/api/equipment/{id}")).StatusCode);
        Assert.Empty((await visitor.GetFromJsonAsync<List<CatalogEndpoints.EquipmentResponse>>(
            "/api/equipment?search=zzzz%20not%20a%20device"))!);
        Assert.Single((await visitor.GetFromJsonAsync<List<CatalogEndpoints.EquipmentResponse>>(
            "/api/equipment?search=deLL%20LatiTUde&category=Computers"))!);
        Assert.Equal(HttpStatusCode.NotFound,(await visitor.GetAsync("/api/equipment/999999")).StatusCode);
    }

    [Fact]
    public async Task AC02_Invalid_credentials_logout_and_ended_session_refused()
    {
        using var app=new TestApp();using var visitor=app.Client();
        var bad=await TestApp.Post(visitor,"/api/auth/login",new {email="student1@celd.local",password="wrong"});
        Assert.Equal(HttpStatusCode.Unauthorized,bad.StatusCode);
        using var student=await app.Student1();
        Assert.Equal(HttpStatusCode.OK,(await student.GetAsync("/api/auth/me")).StatusCode);
        var exit=await TestApp.Post(student,"/api/auth/logout");
        Assert.Equal(HttpStatusCode.OK,exit.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized,(await student.GetAsync("/api/reservations/mine")).StatusCode);
        using var staff=await app.Staff();
        Assert.Equal(HttpStatusCode.OK,(await staff.GetAsync("/api/staff/reservations")).StatusCode);
    }

    [Fact]
    public async Task AC03_Create_sets_reserved_and_availability_false_with_history()
    {
        using var app=new TestApp();using var student=await app.Student1();
        var item=await FirstItem(student);var record=await Create(student,item);
        Assert.Equal(Statuses.Reserved,record.Status);
        Assert.Single(record.History);
        Assert.Equal("Created",record.History[0].EventType);
        var detail=await student.GetFromJsonAsync<CatalogEndpoints.EquipmentResponse>($"/api/equipment/{item}");
        Assert.False(detail!.Available);
        var again=await TestApp.Post(student,"/api/reservations",new { equipmentId=item });
        Assert.Equal(HttpStatusCode.Conflict,again.StatusCode);
    }

    [Fact]
    public async Task AC04_Concurrent_http_requests_real_sqlite_at_most_one_active()
    {
        using var app=new TestApp();using var first=await app.Student1();using var second=await app.Student2();
        var item=await FirstItem(first);
        // Both clients are authenticated and carry independent CSRF/cookie sessions.
        var gate=new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var bothReady=new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var ready=0;
        async Task<HttpResponseMessage> Fire(HttpClient c)
        {
            var csrf=await c.GetFromJsonAsync<TestApp.TokenResponse>("/api/auth/csrf");
            using var request=new HttpRequestMessage(HttpMethod.Post,"/api/reservations")
                {Content=JsonContent.Create(new { equipmentId=item })};
            request.Headers.Add("X-CSRF-TOKEN",csrf!.RequestToken);
            if (Interlocked.Increment(ref ready)==2) bothReady.SetResult(true);
            await gate.Task;
            return await c.SendAsync(request);
        }
        var t1=Fire(first);var t2=Fire(second);await bothReady.Task;gate.SetResult(true);
        var responses=await Task.WhenAll(t1,t2);
        Assert.Single(responses.Where(r=>r.StatusCode==HttpStatusCode.Created));
        Assert.Single(responses.Where(r=>r.StatusCode==HttpStatusCode.Conflict));
        await using var db=app.Database();
        Assert.Equal(1,await db.Reservations.CountAsync(r=>r.EquipmentItemId==item &&
            (r.Status==Statuses.Reserved || r.Status==Statuses.CheckedOut)));
        Assert.Single(await db.HistoryEvents.ToListAsync());
    }

    [Fact]
    public async Task AC04_Physical_unique_index_prevents_bypassing_handler()
    {
        using var app=new TestApp();using var student=await app.Student1();var item=await FirstItem(student);
        await Create(student,item);
        await using var db=app.Database();
        var studentId=await db.UserAccounts.Where(x=>x.Role==Roles.Student).Select(x=>x.Id).FirstAsync();
        db.Reservations.Add(new Reservation {EquipmentItemId=item,StudentId=studentId,
            Status=Statuses.CheckedOut,CreatedUtc=DateTime.UtcNow,Version=1});
        await Assert.ThrowsAsync<DbUpdateException>(()=>db.SaveChangesAsync());
    }

    [Fact]
    public async Task AC05_Ownership_isolated_student_cannot_read_or_cancel_other()
    {
        using var app=new TestApp();using var s1=await app.Student1();using var s2=await app.Student2();
        var r=await Create(s1,await FirstItem(s1));
        Assert.Equal(HttpStatusCode.NotFound,(await s2.GetAsync($"/api/reservations/{r.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,(await TestApp.Post(s2,$"/api/reservations/{r.Id}/cancel")).StatusCode);
        var mine=await s2.GetFromJsonAsync<List<ReservationQueries.ReservationDto>>("/api/reservations/mine");
        Assert.Empty(mine!);
        await using var db=app.Database();Assert.Equal(Statuses.Reserved,(await db.Reservations.FindAsync(r.Id))!.Status);
    }

    [Fact]
    public async Task AC06_Cancel_releases_item_and_preserves_past_history()
    {
        using var app=new TestApp();using var s1=await app.Student1();using var s2=await app.Student2();
        var item=await FirstItem(s1);var r=await Create(s1,item);
        Assert.Equal(HttpStatusCode.OK,(await TestApp.Post(s1,$"/api/reservations/{r.Id}/cancel")).StatusCode);
        var after=await s1.GetFromJsonAsync<ReservationQueries.ReservationDto>($"/api/reservations/{r.Id}");
        Assert.Equal(Statuses.Cancelled,after!.Status);Assert.Equal(2,after.History.Count);
        Assert.True((await s1.GetFromJsonAsync<CatalogEndpoints.EquipmentResponse>($"/api/equipment/{item}"))!.Available);
        await Create(s2,item);
    }

    [Fact]
    public async Task AC07_Only_staff_can_pick_up_and_queue_filters_by_status()
    {
        using var app=new TestApp();using var student=await app.Student1();using var staff=await app.Staff();
        using var visitor=app.Client();var r=await Create(student,await FirstItem(student));
        Assert.Equal(HttpStatusCode.Forbidden,(await TestApp.Post(student,$"/api/staff/reservations/{r.Id}/pickup")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized,(await TestApp.Post(visitor,$"/api/staff/reservations/{r.Id}/pickup")).StatusCode);
        Assert.Equal(HttpStatusCode.OK,(await TestApp.Post(staff,$"/api/staff/reservations/{r.Id}/pickup")).StatusCode);
        var found=await staff.GetFromJsonAsync<List<ReservationQueries.ReservationDto>>("/api/staff/reservations?status=CheckedOut");
        Assert.Single(found!);Assert.Equal(Statuses.CheckedOut,found[0].Status);
    }

    [Fact]
    public async Task AC08_Return_releases_and_new_reservation_succeeds()
    {
        using var app=new TestApp();using var s1=await app.Student1();using var s2=await app.Student2();using var staff=await app.Staff();
        var item=await FirstItem(s1);var r=await Create(s1,item);
        Assert.Equal(HttpStatusCode.OK,(await TestApp.Post(staff,$"/api/staff/reservations/{r.Id}/pickup")).StatusCode);
        Assert.Equal(HttpStatusCode.OK,(await TestApp.Post(staff,$"/api/staff/reservations/{r.Id}/return")).StatusCode);
        var returned=await staff.GetFromJsonAsync<ReservationQueries.ReservationDto>($"/api/reservations/{r.Id}");
        Assert.Equal(Statuses.Returned,returned!.Status);Assert.Equal(3,returned.History.Count);
        Assert.True((await s1.GetFromJsonAsync<CatalogEndpoints.EquipmentResponse>($"/api/equipment/{item}"))!.Available);
        await Create(s2,item);
    }

    [Fact]
    public async Task AC09_Invalid_repeated_and_stale_transitions_do_not_append_history()
    {
        using var app=new TestApp();using var student=await app.Student1();using var staff=await app.Staff();
        var r=await Create(student,await FirstItem(student));
        Assert.Equal(HttpStatusCode.Conflict,(await TestApp.Post(staff,$"/api/staff/reservations/{r.Id}/return")).StatusCode);
        Assert.Equal(HttpStatusCode.OK,(await TestApp.Post(student,$"/api/reservations/{r.Id}/cancel")).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict,(await TestApp.Post(student,$"/api/reservations/{r.Id}/cancel")).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict,(await TestApp.Post(staff,$"/api/staff/reservations/{r.Id}/pickup")).StatusCode);
        await using var db=app.Database();
        Assert.Equal(2,await db.HistoryEvents.CountAsync(h=>h.ReservationId==r.Id));
        Assert.Equal(Statuses.Cancelled,(await db.Reservations.FindAsync(r.Id))!.Status);
    }

    [Fact]
    public async Task AC09_Concurrent_cancel_and_pickup_only_one_transition_commits()
    {
        using var app=new TestApp();using var student=await app.Student1();using var staff=await app.Staff();
        var r=await Create(student,await FirstItem(student));
        var tasks=new[]{TestApp.Post(student,$"/api/reservations/{r.Id}/cancel"),
            TestApp.Post(staff,$"/api/staff/reservations/{r.Id}/pickup")};
        var responses=await Task.WhenAll(tasks);
        Assert.Single(responses.Where(x=>x.StatusCode==HttpStatusCode.OK));
        Assert.Single(responses.Where(x=>x.StatusCode==HttpStatusCode.Conflict));
        await using var db=app.Database();
        Assert.Equal(2,await db.HistoryEvents.CountAsync(h=>h.ReservationId==r.Id));
        var final=(await db.Reservations.FindAsync(r.Id))!.Status;
        Assert.Contains(final,new[]{Statuses.Cancelled,Statuses.CheckedOut});
    }

    [Fact]
    public async Task AC10_Forced_history_insert_failure_rolls_back_status_change_real_sqlite()
    {
        using var app=new TestApp();using var student=await app.Student1();using var staff=await app.Staff();
        var r=await Create(student,await FirstItem(student));
        await using(var db=app.Database())
        {
            // Forced REAL SQLite storage failure; no mock or EF InMemory provider.
            await db.Database.ExecuteSqlRawAsync("""
                CREATE TRIGGER fail_history BEFORE INSERT ON HistoryEvents
                BEGIN SELECT RAISE(ABORT, 'injected history write failure'); END;
                """);
        }
        var response=await TestApp.Post(staff,$"/api/staff/reservations/{r.Id}/pickup");
        Assert.False(response.IsSuccessStatusCode);
        await using var verify=app.Database();
        Assert.Equal(Statuses.Reserved,(await verify.Reservations.FindAsync(r.Id))!.Status);
        Assert.Equal(1,await verify.HistoryEvents.CountAsync(h=>h.ReservationId==r.Id));
    }

    [Fact]
    public async Task AC10_Forced_creation_history_failure_leaves_no_partial_reservation()
    {
        using var app=new TestApp();using var student=await app.Student1();var item=await FirstItem(student);
        await using(var db=app.Database())
        {
            await db.Database.ExecuteSqlRawAsync("""
                CREATE TRIGGER fail_creation_history BEFORE INSERT ON HistoryEvents
                BEGIN SELECT RAISE(ABORT, 'injected creation history failure'); END;
                """);
        }
        var response=await TestApp.Post(student,"/api/reservations",new{equipmentId=item});
        Assert.False(response.IsSuccessStatusCode);
        await using var verify=app.Database();
        Assert.Equal(0,await verify.Reservations.CountAsync(r=>r.EquipmentItemId==item));
        Assert.Equal(0,await verify.HistoryEvents.CountAsync());
    }

    [Fact]
    public async Task API_Requires_csrf_on_all_mutating_requests_even_login()
    {
        using var app=new TestApp();using var anonymous=app.Client();
        var response=await anonymous.PostAsJsonAsync("/api/auth/login",new{email="a",password="b"});
        Assert.Equal(HttpStatusCode.BadRequest,response.StatusCode);
        using var student=await app.Student1();
        var another=await student.PostAsJsonAsync("/api/reservations",new{equipmentId=1});
        Assert.Equal(HttpStatusCode.BadRequest,another.StatusCode);
    }

    [Fact]
    public async Task API_Validation_missing_id_and_staff_cannot_create_reservations()
    {
        using var app=new TestApp();using var student=await app.Student1();using var staff=await app.Staff();
        Assert.Equal(HttpStatusCode.BadRequest,(await TestApp.Post(student,"/api/reservations",new{equipmentId=-4})).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,(await TestApp.Post(student,"/api/reservations",new{equipmentId=123456})).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden,(await TestApp.Post(staff,"/api/reservations",new{equipmentId=1})).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest,(await staff.GetAsync("/api/staff/reservations?status=unknown")).StatusCode);
    }

    [Fact]
    public async Task DR05_Records_persist_across_dbcontext_reopen()
    {
        using var app=new TestApp();using var student=await app.Student1();using var staff=await app.Staff();
        var r=await Create(student,await FirstItem(student));
        await TestApp.Post(staff,$"/api/staff/reservations/{r.Id}/pickup");
        await TestApp.Post(staff,$"/api/staff/reservations/{r.Id}/return");
        await using var reopened=app.Database();
        Assert.Equal(Statuses.Returned,(await reopened.Reservations.FindAsync(r.Id))!.Status);
        Assert.Equal(3,await reopened.HistoryEvents.CountAsync(h=>h.ReservationId==r.Id));
    }
}
