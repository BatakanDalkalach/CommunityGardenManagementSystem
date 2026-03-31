using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.DatabaseContext
{
    // This class represents the database context and manages entity-to-database mapping.
    // Extends IdentityDbContext to include ASP.NET Core Identity tables.
    // Този клас представлява контекста на базата данни и управлява връзката между обектите и таблиците.
    // Разширява IdentityDbContext, за да включи таблиците на ASP.NET Core Identity.
    public class CommunityGardenDatabase : IdentityDbContext<ApplicationUser>
    {
        public CommunityGardenDatabase(DbContextOptions<CommunityGardenDatabase> configuration)
            : base(configuration)
        {
        }
        // DbSet properties represent database tables and allow querying and saving data.
        // DbSet свойствата представляват таблиците в базата данни и позволяват заявки и запис на данни.
        public DbSet<GardenPlot> GardenPlots { get; set; } = null!;
        public DbSet<GardenMember> GardenMembers { get; set; } = null!;
        public DbSet<HarvestRecord> HarvestRecords { get; set; } = null!;
        public DbSet<Announcement> Announcements { get; set; } = null!;
        public DbSet<MaintenanceRequest> MaintenanceRequests { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Defines primary key and unique index to ensure each plot has a unique identifier and designation.
            // Дефинира първичен ключ и уникален индекс, за да гарантира уникален идентификатор и обозначение на всеки парцел.
            builder.Entity<GardenPlot>(plot =>
            {
                plot.HasKey(p => p.PlotIdentifier);
                plot.Property(p => p.PlotDesignation).HasMaxLength(4);
                plot.HasIndex(p => p.PlotDesignation).IsUnique();
                
                plot.HasOne(p => p.CurrentTenant)
                    .WithMany(m => m.ManagedPlots)
                    .HasForeignKey(p => p.AssignedGardenerId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
            // Ensures each member has a unique email and limits email length.
            // Гарантира, че всеки член има уникален имейл и ограничава дължината му.
            builder.Entity<GardenMember>(member =>
            {
                member.HasKey(m => m.MemberId);
                member.Property(m => m.EmailContact).HasMaxLength(120);
                member.HasIndex(m => m.EmailContact).IsUnique();
            });

            builder.Entity<HarvestRecord>(harvest =>
            {
                harvest.HasKey(h => h.RecordId);

                harvest.HasOne(h => h.SourcePlot)
                    .WithMany(p => p.HarvestHistory)
                    .HasForeignKey(h => h.PlotIdentifier)
                    .OnDelete(DeleteBehavior.Cascade);

                harvest.HasOne(h => h.Harvester)
                    .WithMany(m => m.RecordedHarvests)
                    .HasForeignKey(h => h.MemberId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Announcement: simple entity with no relationships
            // Announcement: прост обект без релации
            builder.Entity<Announcement>(announcement =>
            {
                announcement.HasKey(a => a.Id);
                announcement.Property(a => a.Title).HasMaxLength(200);
                announcement.Property(a => a.Content).HasMaxLength(5000);
            });

            // MaintenanceRequest: linked to GardenPlot via PlotId
            // MaintenanceRequest: свързан с GardenPlot чрез PlotId
            builder.Entity<MaintenanceRequest>(req =>
            {
                req.HasKey(r => r.Id);
                req.Property(r => r.Description).HasMaxLength(1000);
                req.Property(r => r.Status).HasConversion<string>();

                req.HasOne(r => r.Plot)
                    .WithMany()
                    .HasForeignKey(r => r.PlotId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Seed starter data
            // Начални данни (Seed данни) за базата
            builder.Entity<GardenMember>().HasData(
                new GardenMember
                {
                    MemberId = 1,
                    FullLegalName = "Sarah Johnson",
                    EmailContact = "sarah.j@email.com",
                    MembershipTier = "Premium",
                    RegistrationDate = new DateTime(2022, 4, 15),
                    YearsOfExperience = 8,
                    PreferOrganicOnly = true,
                    GardeningInterests = "Tomatoes, Herbs, Peppers"
                },
                new GardenMember
                {
                    MemberId = 2,
                    FullLegalName = "Michael Chen",
                    EmailContact = "m.chen@email.com",
                    MembershipTier = "Basic",
                    RegistrationDate = new DateTime(2023, 1, 20),
                    YearsOfExperience = 2,
                    PreferOrganicOnly = false,
                    GardeningInterests = "Root vegetables, Squash"
                }
            );

            builder.Entity<GardenPlot>().HasData(
                new GardenPlot
                {
                    PlotIdentifier = 1,
                    PlotDesignation = "A001",
                    SquareMeters = 25.5,
                    SoilType = "Clay-Loam",
                    WaterAccessAvailable = true,
                    IsOccupied = true,
                    YearlyRentalFee = 150m,
                    LastMaintenanceDate = new DateTime(2024, 3, 1),
                    AssignedGardenerId = 1
                },
                new GardenPlot
                {
                    PlotIdentifier = 2,
                    PlotDesignation = "A002",
                    SquareMeters = 30.0,
                    SoilType = "Sandy-Loam",
                    WaterAccessAvailable = true,
                    IsOccupied = false,
                    YearlyRentalFee = 175m,
                    LastMaintenanceDate = new DateTime(2024, 2, 15),
                    AssignedGardenerId = null
                },
                new GardenPlot
                {
                    PlotIdentifier = 3,
                    PlotDesignation = "B001",
                    SquareMeters = 20.0,
                    SoilType = "Loamy",
                    WaterAccessAvailable = true,
                    IsOccupied = true,
                    YearlyRentalFee = 125m,
                    LastMaintenanceDate = new DateTime(2024, 3, 10),
                    AssignedGardenerId = 2
                }
            );

            builder.Entity<HarvestRecord>().HasData(
                new HarvestRecord
                {
                    RecordId = 1,
                    PlotIdentifier = 1,
                    MemberId = 1,
                    CropName = "Cherry Tomatoes",
                    QuantityKilograms = 12.5,
                    CollectionDate = new DateTime(2024, 7, 15),
                    QualityScore = 5,
                    HarvestNotes = "Excellent yield, very sweet",
                    IsOrganicCertified = true
                },
                new HarvestRecord
                {
                    RecordId = 2,
                    PlotIdentifier = 3,
                    MemberId = 2,
                    CropName = "Carrots",
                    QuantityKilograms = 8.3,
                    CollectionDate = new DateTime(2024, 6, 20),
                    QualityScore = 4,
                    HarvestNotes = "Good size, some slight pest damage",
                    IsOrganicCertified = false
                }
            );
        }
    }
}
