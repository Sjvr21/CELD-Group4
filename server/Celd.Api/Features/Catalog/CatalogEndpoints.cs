using Celd.Api.Common;
using Celd.Api.Data;
using Celd.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace Celd.Api.Features.Catalog;

public static class CatalogEndpoints
{
    public sealed record EquipmentResponse(int Id, string AssetCode, string Name, string Category, string Description, bool Available);
    public static IEndpointRouteBuilder MapCatalog(this IEndpointRouteBuilder app)
    {
        var group=app.MapGroup("/api/equipment");
        group.MapGet("", async (string? search, string? category, CeldDbContext db) =>
        {
            var query=db.EquipmentItems.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term=search.Trim().ToLower();
                query=query.Where(x=>x.Name.ToLower().Contains(term));
            }
            if (!string.IsNullOrWhiteSpace(category))
            {
                var cat=category.Trim().ToLower();
                query=query.Where(x=>x.Category.ToLower()==cat);
            }
            var items=await query.OrderBy(x=>x.Name).Select(x=>new EquipmentResponse(
                x.Id,x.AssetCode,x.Name,x.Category,x.Description,
                !x.Reservations.Any(r=>r.Status==Statuses.Reserved || r.Status==Statuses.CheckedOut))).ToListAsync();
            return Results.Ok(items);
        });
        group.MapGet("/categories", async (CeldDbContext db) =>
            Results.Ok(await db.EquipmentItems.AsNoTracking().Select(x=>x.Category).Distinct().OrderBy(x=>x).ToListAsync()));
        group.MapGet("/{id:int}", async (int id, CeldDbContext db) =>
        {
            var x=await db.EquipmentItems.AsNoTracking().Where(e=>e.Id==id)
                .Select(e=>new EquipmentResponse(e.Id,e.AssetCode,e.Name,e.Category,e.Description,
                    !e.Reservations.Any(r=>r.Status==Statuses.Reserved || r.Status==Statuses.CheckedOut)))
                .SingleOrDefaultAsync();
            return x is null ? ApiErrors.Missing() : Results.Ok(x);
        });
        return app;
    }
}
