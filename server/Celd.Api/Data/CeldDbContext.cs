using Celd.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace Celd.Api.Data;

public sealed class CeldDbContext(DbContextOptions<CeldDbContext> options) : DbContext(options)
{
    public DbSet<EquipmentItem> EquipmentItems => Set<EquipmentItem>();
    public DbSet<UserAccount> UserAccounts => Set<UserAccount>();
    public DbSet<LoginSession> LoginSessions => Set<LoginSession>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<HistoryEvent> HistoryEvents => Set<HistoryEvent>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<EquipmentItem>(b =>
        {
            b.HasKey(x => x.Id);
            b.HasIndex(x => x.AssetCode).IsUnique();
            b.Property(x => x.AssetCode).HasMaxLength(40).IsRequired();
            b.Property(x => x.Name).HasMaxLength(160).IsRequired();
            b.Property(x => x.Category).HasMaxLength(80).IsRequired();
            b.Property(x => x.Description).HasMaxLength(1000).IsRequired();
        });
        model.Entity<UserAccount>(b =>
        {
            b.HasKey(x => x.Id);
            b.HasIndex(x => x.EmailNormalized).IsUnique();
            b.Property(x => x.EmailNormalized).HasMaxLength(256).IsRequired();
            b.Property(x => x.DisplayName).HasMaxLength(120).IsRequired();
            b.Property(x => x.PasswordHash).IsRequired();
            b.Property(x => x.Role).HasMaxLength(12).IsRequired();
            b.ToTable(t => t.HasCheckConstraint("CK_User_Role", "\"Role\" IN ('Student', 'Staff')"));
        });
        model.Entity<LoginSession>(b =>
        {
            b.HasKey(x => x.Id);
            b.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserAccountId).OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(x => x.ExpiresUtc);
        });
        model.Entity<Reservation>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Status).HasMaxLength(20).IsRequired();
            b.ToTable(t => t.HasCheckConstraint("CK_Reservation_Status", "\"Status\" IN ('Reserved','CheckedOut','Cancelled','Returned')"));
            b.HasOne(x => x.Student).WithMany().HasForeignKey(x => x.StudentId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.EquipmentItem).WithMany(x => x.Reservations).HasForeignKey(x => x.EquipmentItemId).OnDelete(DeleteBehavior.Restrict);
            // BR-RES-002 / AC-04: REAL SQLite partial UNIQUE index; application checks alone are insufficient.
            b.HasIndex(x => x.EquipmentItemId).IsUnique()
                .HasFilter("\"Status\" IN ('Reserved', 'CheckedOut')")
                .HasDatabaseName("UX_OneActiveReservationPerItem");
            b.HasIndex(x => x.StudentId);
        });
        model.Entity<HistoryEvent>(b =>
        {
            b.HasKey(x => x.Id);
            b.HasOne(x => x.Reservation).WithMany(x => x.HistoryEvents).HasForeignKey(x => x.ReservationId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.Actor).WithMany().HasForeignKey(x => x.ActorUserId).OnDelete(DeleteBehavior.Restrict);
            b.Property(x => x.EventType).HasMaxLength(24).IsRequired();
            b.Property(x => x.ResultingStatus).HasMaxLength(20).IsRequired();
            b.HasIndex(x => new { x.ReservationId, x.OccurredUtc, x.Id });
        });
    }
}
