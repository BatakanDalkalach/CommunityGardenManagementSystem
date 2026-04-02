using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WebApplication1.DatabaseContext;
using WebApplication1.Models;
using Xunit;

namespace WebApplication1.Tests;

/// <summary>
/// xUnit tests for HarvestRecord CRUD operations using an in-memory SQLite database.
/// Each test runs against a fresh database seeded via HasData with:
///   - Members: ID=1 (Sarah Johnson, Premium), ID=2 (Michael Chen, Basic)
///   - Plots:   ID=1 (A001, occupied by member 1), ID=2 (A002, vacant), ID=3 (B001, occupied by member 2)
///   - Records: ID=1 (Plot 1, Cherry Tomatoes, 2024-07-15), ID=2 (Plot 3, Carrots, 2024-06-20)
/// </summary>
public class HarvestRecordTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly CommunityGardenDatabase _db;

    public HarvestRecordTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<CommunityGardenDatabase>()
            .UseSqlite(_connection)
            .Options;

        _db = new CommunityGardenDatabase(options);
        _db.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    // -------------------------------------------------------------------------
    // Create
    // -------------------------------------------------------------------------

    [Fact]
    public async Task CreateHarvestRecord_AssignsId()
    {
        var record = new HarvestRecord
        {
            PlotIdentifier = 1,
            MemberId = 1,
            CropName = "Basil",
            QuantityKilograms = 2.0,
            CollectionDate = new DateTime(2024, 8, 1),
            QualityScore = 4,
            IsOrganicCertified = true
        };

        _db.HarvestRecords.Add(record);
        await _db.SaveChangesAsync();

        Assert.True(record.RecordId > 0);
    }

    [Fact]
    public async Task CreateHarvestRecord_PersistsAllFields()
    {
        var record = new HarvestRecord
        {
            PlotIdentifier = 2,
            MemberId = 2,
            CropName = "Zucchini",
            QuantityKilograms = 5.5,
            CollectionDate = new DateTime(2024, 9, 10),
            QualityScore = 3,
            HarvestNotes = "Good growth despite dry spell",
            IsOrganicCertified = false
        };

        _db.HarvestRecords.Add(record);
        await _db.SaveChangesAsync();

        var saved = await _db.HarvestRecords.AsNoTracking()
            .FirstOrDefaultAsync(h => h.RecordId == record.RecordId);

        Assert.NotNull(saved);
        Assert.Equal("Zucchini", saved.CropName);
        Assert.Equal(5.5, saved.QuantityKilograms);
        Assert.Equal(new DateTime(2024, 9, 10), saved.CollectionDate);
        Assert.Equal(3, saved.QualityScore);
        Assert.Equal("Good growth despite dry spell", saved.HarvestNotes);
        Assert.False(saved.IsOrganicCertified);
    }

    [Fact]
    public async Task CreateHarvestRecord_IncreasesTotalCount()
    {
        var before = await _db.HarvestRecords.CountAsync();

        _db.HarvestRecords.Add(new HarvestRecord
        {
            PlotIdentifier = 1,
            MemberId = 1,
            CropName = "Mint",
            QuantityKilograms = 1.0,
            CollectionDate = new DateTime(2024, 8, 5),
            QualityScore = 5
        });
        await _db.SaveChangesAsync();

        var after = await _db.HarvestRecords.CountAsync();
        Assert.Equal(before + 1, after);
    }

    // -------------------------------------------------------------------------
    // Retrieve by ID
    // -------------------------------------------------------------------------

    [Fact]
    public async Task RetrieveById_WithValidId_ReturnsCorrectRecord()
    {
        var record = await _db.HarvestRecords.FindAsync(1);

        Assert.NotNull(record);
        Assert.Equal(1, record.RecordId);
        Assert.Equal("Cherry Tomatoes", record.CropName);
        Assert.Equal(12.5, record.QuantityKilograms);
        Assert.Equal(5, record.QualityScore);
        Assert.True(record.IsOrganicCertified);
    }

    [Fact]
    public async Task RetrieveById_WithValidId_IncludesNavigationProperties()
    {
        var record = await _db.HarvestRecords
            .Include(h => h.SourcePlot)
            .Include(h => h.Harvester)
            .FirstOrDefaultAsync(h => h.RecordId == 1);

        Assert.NotNull(record);
        Assert.NotNull(record.SourcePlot);
        Assert.Equal("A001", record.SourcePlot.PlotDesignation);
        Assert.NotNull(record.Harvester);
        Assert.Equal("Sarah Johnson", record.Harvester.FullLegalName);
    }

    [Fact]
    public async Task RetrieveById_WithInvalidId_ReturnsNull()
    {
        var record = await _db.HarvestRecords.FindAsync(999);

        Assert.Null(record);
    }

    // -------------------------------------------------------------------------
    // List all ordered by date
    // -------------------------------------------------------------------------

    [Fact]
    public async Task ListAllRecords_ReturnsSeededCount()
    {
        var records = await _db.HarvestRecords.ToListAsync();

        Assert.Equal(2, records.Count);
    }

    [Fact]
    public async Task ListAllRecords_OrderedByDateDescending_MostRecentFirst()
    {
        var records = await _db.HarvestRecords
            .OrderByDescending(h => h.CollectionDate)
            .ToListAsync();

        // Record 1: 2024-07-15 (Cherry Tomatoes), Record 2: 2024-06-20 (Carrots)
        Assert.Equal("Cherry Tomatoes", records[0].CropName);
        Assert.Equal("Carrots", records[1].CropName);
    }

    [Fact]
    public async Task ListAllRecords_OrderedByDateAscending_OldestFirst()
    {
        var records = await _db.HarvestRecords
            .OrderBy(h => h.CollectionDate)
            .ToListAsync();

        Assert.True(records[0].CollectionDate <= records[1].CollectionDate);
    }

    [Fact]
    public async Task ListAllRecords_AfterAddingRecord_IncludesNewRecord()
    {
        _db.HarvestRecords.Add(new HarvestRecord
        {
            PlotIdentifier = 1,
            MemberId = 1,
            CropName = "Sage",
            QuantityKilograms = 0.8,
            CollectionDate = new DateTime(2024, 10, 1),
            QualityScore = 4
        });
        await _db.SaveChangesAsync();

        var records = await _db.HarvestRecords
            .OrderByDescending(h => h.CollectionDate)
            .ToListAsync();

        Assert.Equal(3, records.Count);
        Assert.Equal("Sage", records[0].CropName);
    }

    // -------------------------------------------------------------------------
    // Filter by plot
    // -------------------------------------------------------------------------

    [Fact]
    public async Task FilterByPlot_WithPlot1_ReturnsOnlyCherryTomatoes()
    {
        var records = await _db.HarvestRecords
            .Where(h => h.PlotIdentifier == 1)
            .ToListAsync();

        Assert.Single(records);
        Assert.Equal("Cherry Tomatoes", records[0].CropName);
    }

    [Fact]
    public async Task FilterByPlot_WithPlot3_ReturnsOnlyCarrots()
    {
        var records = await _db.HarvestRecords
            .Where(h => h.PlotIdentifier == 3)
            .ToListAsync();

        Assert.Single(records);
        Assert.Equal("Carrots", records[0].CropName);
    }

    [Fact]
    public async Task FilterByPlot_WithVacantPlot_ReturnsEmptyList()
    {
        // Plot 2 (A002) has no harvest records in seed data
        var records = await _db.HarvestRecords
            .Where(h => h.PlotIdentifier == 2)
            .ToListAsync();

        Assert.Empty(records);
    }

    [Fact]
    public async Task FilterByPlot_AfterAddingRecordToPlot_ReturnsUpdatedResults()
    {
        _db.HarvestRecords.Add(new HarvestRecord
        {
            PlotIdentifier = 1,
            MemberId = 1,
            CropName = "Peppers",
            QuantityKilograms = 3.2,
            CollectionDate = new DateTime(2024, 8, 20),
            QualityScore = 5
        });
        await _db.SaveChangesAsync();

        var plot1Records = await _db.HarvestRecords
            .Where(h => h.PlotIdentifier == 1)
            .ToListAsync();

        Assert.Equal(2, plot1Records.Count);
        Assert.All(plot1Records, r => Assert.Equal(1, r.PlotIdentifier));
    }

    [Fact]
    public async Task FilterByPlot_ResultsAreCorrectlyIsolated()
    {
        var plot1Records = await _db.HarvestRecords
            .Where(h => h.PlotIdentifier == 1)
            .ToListAsync();

        var plot3Records = await _db.HarvestRecords
            .Where(h => h.PlotIdentifier == 3)
            .ToListAsync();

        Assert.DoesNotContain(plot1Records, r => r.PlotIdentifier == 3);
        Assert.DoesNotContain(plot3Records, r => r.PlotIdentifier == 1);
    }

    // -------------------------------------------------------------------------
    // Delete
    // -------------------------------------------------------------------------

    [Fact]
    public async Task DeleteRecord_WithExistingId_RemovesFromDatabase()
    {
        var record = await _db.HarvestRecords.FindAsync(1);
        Assert.NotNull(record);

        _db.HarvestRecords.Remove(record);
        await _db.SaveChangesAsync();

        var deleted = await _db.HarvestRecords.FindAsync(1);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task DeleteRecord_DecreasesTotalCount()
    {
        var before = await _db.HarvestRecords.CountAsync();

        var record = await _db.HarvestRecords.FindAsync(2);
        _db.HarvestRecords.Remove(record!);
        await _db.SaveChangesAsync();

        var after = await _db.HarvestRecords.CountAsync();
        Assert.Equal(before - 1, after);
    }

    [Fact]
    public async Task DeleteRecord_OtherRecordsUnaffected()
    {
        var record = await _db.HarvestRecords.FindAsync(1);
        _db.HarvestRecords.Remove(record!);
        await _db.SaveChangesAsync();

        var remaining = await _db.HarvestRecords.FindAsync(2);
        Assert.NotNull(remaining);
        Assert.Equal("Carrots", remaining.CropName);
    }

    [Fact]
    public async Task DeleteRecord_WithNonExistentId_DoesNotThrow()
    {
        var record = await _db.HarvestRecords.FindAsync(999);

        var exception = await Record.ExceptionAsync(async () =>
        {
            if (record != null)
            {
                _db.HarvestRecords.Remove(record);
                await _db.SaveChangesAsync();
            }
        });

        Assert.Null(exception);
    }
}
