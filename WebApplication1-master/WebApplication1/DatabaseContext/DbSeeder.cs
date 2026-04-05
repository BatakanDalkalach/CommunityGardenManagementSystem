using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.DatabaseContext
{
    // Runtime seeder that supplements the migration HasData seed with additional records.
    // Добавя допълнителни начални записи към вече съществуващите от миграциите.
    public static class DbSeeder
    {
        public static async Task SeedAsync(CommunityGardenDatabase db)
        {
            await db.Database.EnsureCreatedAsync();

            await SeedMembersAsync(db);
            await SeedPlotsAsync(db);
            await SeedHarvestRecordsAsync(db);
            await SeedAnnouncementsAsync(db);
            await SeedMaintenanceRequestsAsync(db);

            await db.SaveChangesAsync();
        }

        // Adds members 3-6 if they are not already present (members 1-2 come from HasData migration).
        // Добавя членове 3-6, ако вече не съществуват (членове 1-2 идват от HasData миграция).
        private static async Task SeedMembersAsync(CommunityGardenDatabase db)
        {
            var existingIds = await db.GardenMembers.Select(m => m.MemberId).ToListAsync();

            var newMembers = new List<GardenMember>
            {
                new GardenMember
                {
                    MemberId = 3,
                    FullLegalName = "Elena Petrova",
                    EmailContact = "elena.petrova@email.com",
                    MembershipTier = "Premium",
                    RegistrationDate = new DateTime(2021, 6, 10),
                    YearsOfExperience = 12,
                    PreferOrganicOnly = true,
                    GardeningInterests = "Roses, Lavender, Medicinal herbs"
                },
                new GardenMember
                {
                    MemberId = 4,
                    FullLegalName = "David Okonkwo",
                    EmailContact = "d.okonkwo@email.com",
                    MembershipTier = "Basic",
                    RegistrationDate = new DateTime(2024, 2, 28),
                    YearsOfExperience = 1,
                    PreferOrganicOnly = false,
                    GardeningInterests = "Lettuce, Beans, Cucumbers"
                },
                new GardenMember
                {
                    MemberId = 5,
                    FullLegalName = "Sofia Andersen",
                    EmailContact = "sofia.andersen@email.com",
                    MembershipTier = "Premium",
                    RegistrationDate = new DateTime(2019, 3, 15),
                    YearsOfExperience = 9,
                    PreferOrganicOnly = true,
                    GardeningInterests = "Strawberries, Blueberries, Companion planting"
                },
                new GardenMember
                {
                    MemberId = 6,
                    FullLegalName = "Marcus Webb",
                    EmailContact = "m.webb@email.com",
                    MembershipTier = "Standard",
                    RegistrationDate = new DateTime(2023, 9, 4),
                    YearsOfExperience = 3,
                    PreferOrganicOnly = false,
                    GardeningInterests = "Pumpkins, Squash, Root vegetables"
                }
            };

            foreach (var member in newMembers.Where(m => !existingIds.Contains(m.MemberId)))
                db.GardenMembers.Add(member);
        }

        // Adds plots 4-8 if not already present (plots 1-3 come from HasData migration).
        // Добавя парцели 4-8, ако вече не съществуват (парцели 1-3 идват от HasData миграция).
        private static async Task SeedPlotsAsync(CommunityGardenDatabase db)
        {
            var existingIds = await db.GardenPlots.Select(p => p.PlotIdentifier).ToListAsync();

            var newPlots = new List<GardenPlot>
            {
                new GardenPlot
                {
                    PlotIdentifier = 4,
                    PlotDesignation = "B002",
                    SquareMeters = 18.0,
                    SoilType = "Loamy",
                    WaterAccessAvailable = true,
                    IsOccupied = true,
                    YearlyRentalFee = 110m,
                    LastMaintenanceDate = new DateTime(2025, 1, 20),
                    AssignedGardenerId = 3
                },
                new GardenPlot
                {
                    PlotIdentifier = 5,
                    PlotDesignation = "C001",
                    SquareMeters = 40.0,
                    SoilType = "Sandy",
                    WaterAccessAvailable = false,
                    IsOccupied = true,
                    YearlyRentalFee = 200m,
                    LastMaintenanceDate = new DateTime(2025, 3, 5),
                    AssignedGardenerId = 4
                },
                new GardenPlot
                {
                    PlotIdentifier = 6,
                    PlotDesignation = "C002",
                    SquareMeters = 22.5,
                    SoilType = "Clay",
                    WaterAccessAvailable = true,
                    IsOccupied = false,
                    YearlyRentalFee = 130m,
                    LastMaintenanceDate = new DateTime(2025, 2, 10),
                    AssignedGardenerId = null
                },
                new GardenPlot
                {
                    PlotIdentifier = 7,
                    PlotDesignation = "D001",
                    SquareMeters = 30.0,
                    SoilType = "Loamy",
                    WaterAccessAvailable = true,
                    IsOccupied = true,
                    YearlyRentalFee = 160m,
                    LastMaintenanceDate = new DateTime(2025, 4, 12),
                    AssignedGardenerId = 5
                },
                new GardenPlot
                {
                    PlotIdentifier = 8,
                    PlotDesignation = "D002",
                    SquareMeters = 35.5,
                    SoilType = "Sandy loam",
                    WaterAccessAvailable = true,
                    IsOccupied = true,
                    YearlyRentalFee = 185m,
                    LastMaintenanceDate = new DateTime(2025, 3, 22),
                    AssignedGardenerId = 6
                }
            };

            foreach (var plot in newPlots.Where(p => !existingIds.Contains(p.PlotIdentifier)))
                db.GardenPlots.Add(plot);
        }

        // Adds harvest records 3-7 if not already present (records 1-2 come from HasData migration).
        // Добавя записи за реколта 3-7, ако вече не съществуват (записи 1-2 идват от HasData миграция).
        private static async Task SeedHarvestRecordsAsync(CommunityGardenDatabase db)
        {
            var existingIds = await db.HarvestRecords.Select(h => h.RecordId).ToListAsync();

            var newRecords = new List<HarvestRecord>
            {
                new HarvestRecord
                {
                    RecordId = 3,
                    PlotIdentifier = 4,
                    MemberId = 3,
                    CropName = "Lavender",
                    QuantityKilograms = 3.2,
                    CollectionDate = new DateTime(2025, 7, 8),
                    QualityScore = 5,
                    HarvestNotes = "Fragrant and full-bloom; dried successfully",
                    IsOrganicCertified = true
                },
                new HarvestRecord
                {
                    RecordId = 4,
                    PlotIdentifier = 5,
                    MemberId = 4,
                    CropName = "Green Beans",
                    QuantityKilograms = 6.7,
                    CollectionDate = new DateTime(2025, 8, 14),
                    QualityScore = 3,
                    HarvestNotes = "Decent yield; soil dryness affected growth",
                    IsOrganicCertified = false
                },
                new HarvestRecord
                {
                    RecordId = 5,
                    PlotIdentifier = 7,
                    MemberId = 5,
                    CropName = "Strawberries",
                    QuantityKilograms = 8.4,
                    CollectionDate = new DateTime(2025, 6, 22),
                    QualityScore = 5,
                    HarvestNotes = "Exceptional season; added straw mulch which dramatically improved yield",
                    IsOrganicCertified = true
                },
                new HarvestRecord
                {
                    RecordId = 6,
                    PlotIdentifier = 8,
                    MemberId = 6,
                    CropName = "Butternut Squash",
                    QuantityKilograms = 14.1,
                    CollectionDate = new DateTime(2025, 9, 30),
                    QualityScore = 4,
                    HarvestNotes = "Good size and colour; two fruits showed minor blossom-end rot",
                    IsOrganicCertified = false
                },
                new HarvestRecord
                {
                    RecordId = 7,
                    PlotIdentifier = 4,
                    MemberId = 3,
                    CropName = "Medicinal Chamomile",
                    QuantityKilograms = 1.8,
                    CollectionDate = new DateTime(2025, 8, 5),
                    QualityScore = 5,
                    HarvestNotes = "Hand-picked at peak bloom; dried at low temperature to preserve oils",
                    IsOrganicCertified = true
                }
            };

            foreach (var record in newRecords.Where(r => !existingIds.Contains(r.RecordId)))
                db.HarvestRecords.Add(record);
        }

        // Seeds 5 announcements if none exist yet.
        // Добавя 5 обявления, ако все още няма нито едно.
        private static async Task SeedAnnouncementsAsync(CommunityGardenDatabase db)
        {
            if (await db.Announcements.AnyAsync())
                return;

            db.Announcements.AddRange(
                new Announcement
                {
                    Title = "Spring Planting Season Opens April 15",
                    Content = "We are excited to announce that the spring planting season officially begins on April 15th. " +
                              "All plot holders should clear and prepare their plots by April 10th. " +
                              "Shared tools are available from the equipment shed during opening hours. " +
                              "New members are encouraged to attend the orientation session on April 12th at 10:00 AM.",
                    CreatedAt = new DateTime(2025, 3, 28, 9, 0, 0, DateTimeKind.Utc),
                    IsPublished = true
                },
                new Announcement
                {
                    Title = "Compost Bins Now Available at Section B",
                    Content = "The garden committee has installed three new compost bins near Section B. " +
                              "Members are encouraged to deposit vegetable scraps and garden trimmings (no meat or dairy). " +
                              "Finished compost will be distributed to all active plot holders at the end of each season. " +
                              "Please follow the posted composting guidelines to keep the bins in good condition.",
                    CreatedAt = new DateTime(2025, 4, 5, 14, 30, 0, DateTimeKind.Utc),
                    IsPublished = true
                },
                new Announcement
                {
                    Title = "Water Conservation Notice – Summer Restrictions",
                    Content = "Due to the recent drought advisory, the garden board has introduced summer water-use guidelines. " +
                              "Watering is permitted only between 6:00–8:00 AM and 7:00–9:00 PM on weekdays. " +
                              "Weekend watering is unrestricted. Members with drip irrigation systems are exempt. " +
                              "Violations may result in temporary suspension of water access. Thank you for your cooperation.",
                    CreatedAt = new DateTime(2025, 6, 1, 8, 0, 0, DateTimeKind.Utc),
                    IsPublished = false
                },
                new Announcement
                {
                    Title = "Annual Harvest Festival – Save the Date",
                    Content = "Mark your calendars! The Community Garden Harvest Festival will be held on Saturday, October 4th from 10:00 AM to 4:00 PM. " +
                              "Members are invited to bring produce for display and the community tasting table. " +
                              "There will be a best-harvest competition with prizes for largest yield, most unusual crop, and best organic display. " +
                              "Children's activities and a seed-swap station will also be available. All are welcome!",
                    CreatedAt = new DateTime(2025, 9, 10, 10, 0, 0, DateTimeKind.Utc),
                    IsPublished = true
                },
                new Announcement
                {
                    Title = "New Tool Lending Library Launching Next Month",
                    Content = "The garden board is pleased to announce the launch of our tool lending library starting November 1st. " +
                              "Members will be able to borrow tools such as tillers, wheelbarrows, and aerators for up to 3 days at a time. " +
                              "A refundable deposit is required for power tools. Full terms and the catalogue will be posted to the notice board. " +
                              "If you would like to donate tools in good condition, please contact the committee by October 20th.",
                    CreatedAt = new DateTime(2025, 10, 8, 11, 0, 0, DateTimeKind.Utc),
                    IsPublished = false
                }
            );
        }

        // Seeds 3 maintenance requests if none exist yet.
        // Добавя 3 заявки за поддръжка, ако все още няма нито една.
        private static async Task SeedMaintenanceRequestsAsync(CommunityGardenDatabase db)
        {
            if (await db.MaintenanceRequests.AnyAsync())
                return;

            db.MaintenanceRequests.AddRange(
                new MaintenanceRequest
                {
                    PlotId = 2,
                    Description = "The irrigation tap on the eastern border of the plot is leaking steadily and has been creating a muddy patch " +
                                  "that is difficult to work around. The tap handle is also stiff and hard to turn off fully. " +
                                  "Please repair or replace the fitting at the earliest opportunity.",
                    RequestDate = new DateTime(2025, 5, 14),
                    Status = MaintenanceStatus.Completed
                },
                new MaintenanceRequest
                {
                    PlotId = 5,
                    Description = "Several fence panels along the northern edge of plot C001 have rotted at the base and are leaning inward. " +
                                  "The damage is allowing rabbits to enter the plot and damage crops. " +
                                  "Replacement of at least three panels is needed before the next growing season.",
                    RequestDate = new DateTime(2025, 8, 3),
                    Status = MaintenanceStatus.InProgress
                },
                new MaintenanceRequest
                {
                    PlotId = 7,
                    Description = "A large tree root from the adjacent pathway has broken through the soil surface in the north-west corner of D001, " +
                                  "making it impossible to dig or plant in that section. The root appears to originate from the oak tree on the boundary. " +
                                  "Requesting assessment and, if possible, removal or rerouting of the root.",
                    RequestDate = new DateTime(2025, 10, 19),
                    Status = MaintenanceStatus.Pending
                }
            );
        }
    }
}
