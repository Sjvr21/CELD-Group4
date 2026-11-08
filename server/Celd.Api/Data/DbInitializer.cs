using Celd.Api.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Celd.Api.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider services, IConfiguration config)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<CeldDbContext>();
        // SQLite file persistence. The instructor must approve this provider/version.
        // For a new production schema version, replace EnsureCreated with reviewed migrations.
        await db.Database.EnsureCreatedAsync();
        await db.Database.ExecuteSqlRawAsync("PRAGMA journal_mode=WAL;");
        if (!await db.EquipmentItems.AnyAsync())
        {
            db.EquipmentItems.AddRange(
                new EquipmentItem { AssetCode="LAP-101", Name="Laptop Dell Latitude", Category="Computers", Description="Portable computer with charging adapter." },
                new EquipmentItem { AssetCode="LAP-102", Name="Laptop Lenovo ThinkPad", Category="Computers", Description="Portable computer for academic use." },
                new EquipmentItem { AssetCode="CAM-201", Name="Canon Camera", Category="Cameras", Description="Digital camera with carrying case." },
                new EquipmentItem { AssetCode="CAM-202", Name="Sony Camera", Category="Cameras", Description="Digital camera with included battery." },
                new EquipmentItem { AssetCode="AUD-301", Name="USB Microphone", Category="Audio", Description="Microphone with desk stand." },
                new EquipmentItem { AssetCode="PRJ-401", Name="Portable Projector", Category="Presentation", Description="Projector with power and HDMI cables." });
        }
        var hasher = new PasswordHasher<UserAccount>();
        async Task AddUser(string key, string role, string email, string label)
        {
            var pass = config[$"Seed:{key}Password"];
            if (string.IsNullOrWhiteSpace(pass)) return;
            if (pass.Length < 12) throw new InvalidOperationException($"Seed password for {key} must have at least 12 characters.");
            if (await db.UserAccounts.AnyAsync(x => x.EmailNormalized == email.ToUpperInvariant())) return;
            var user = new UserAccount { EmailNormalized=email.ToUpperInvariant(), DisplayName=label, Role=role };
            user.PasswordHash = hasher.HashPassword(user, pass);
            db.UserAccounts.Add(user);
        }
        await AddUser("Student1", Roles.Student, "student1@celd.local", "Student One");
        await AddUser("Student2", Roles.Student, "student2@celd.local", "Student Two");
        await AddUser("Staff", Roles.Staff, "staff@celd.local", "Lending Desk Staff");
        await db.SaveChangesAsync();
    }
}
