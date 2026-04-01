using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WebApplication1.DatabaseContext;
using WebApplication1.Models;
using WebApplication1.Services;
using Xunit;

namespace WebApplication1.Tests;

/// <summary>
/// xUnit tests for PlotManagementService using an in-memory SQLite database.
/// Each test runs against a fresh database seeded with 2 members, 3 plots, and 2 harvest records.
/// Seeded plots: A001 (occupied, ID=1), A002 (vacant, ID=2), B001 (occupied, ID=3).
/// </summary>
public class PlotManagementServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly CommunityGardenDatabase _db;
    private readonly PlotManagementService _service;

    public PlotManagementServiceTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<CommunityGardenDatabase>()
            .UseSqlite(_connection)
            .Options;

        _db = new CommunityGardenDatabase(options);
        _db.Database.EnsureCreated();
        _service = new PlotManagementService(_db);
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    // -------------------------------------------------------------------------
    // RetrieveAllPlotsAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task RetrieveAllPlotsAsync_ReturnsAllSeededPlots()
    {
        var plots = await _service.RetrieveAllPlotsAsync();

        Assert.Equal(3, plots.Count);
    }

    [Fact]
    public async Task RetrieveAllPlotsAsync_ReturnsOrderedByDesignation()
    {
        var plots = await _service.RetrieveAllPlotsAsync();
        var designations = plots.Select(p => p.PlotDesignation).ToList();

        Assert.Equal(designations.OrderBy(d => d).ToList(), designations);
    }

    // -------------------------------------------------------------------------
    // FindPlotByIdentifierAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task FindPlotByIdentifierAsync_WithValidId_ReturnsCorrectPlot()
    {
        var plot = await _service.FindPlotByIdentifierAsync(1);

        Assert.NotNull(plot);
        Assert.Equal(1, plot.PlotIdentifier);
        Assert.Equal("A001", plot.PlotDesignation);
        Assert.Equal(25.5, plot.SquareMeters);
    }

    [Fact]
    public async Task FindPlotByIdentifierAsync_WithValidId_IncludesCurrentTenant()
    {
        var plot = await _service.FindPlotByIdentifierAsync(1);

        Assert.NotNull(plot);
        Assert.NotNull(plot.CurrentTenant);
        Assert.Equal("Sarah Johnson", plot.CurrentTenant.FullLegalName);
    }

    [Fact]
    public async Task FindPlotByIdentifierAsync_WithVacantPlot_HasNullTenant()
    {
        var plot = await _service.FindPlotByIdentifierAsync(2);

        Assert.NotNull(plot);
        Assert.Null(plot.CurrentTenant);
    }

    [Fact]
    public async Task FindPlotByIdentifierAsync_WithInvalidId_ReturnsNull()
    {
        var plot = await _service.FindPlotByIdentifierAsync(999);

        Assert.Null(plot);
    }

    // -------------------------------------------------------------------------
    // RegisterNewPlotAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task RegisterNewPlotAsync_ReturnsPlotWithAssignedId()
    {
        var newPlot = new GardenPlot
        {
            PlotDesignation = "C001",
            SquareMeters = 20.0,
            SoilType = "Loamy",
            WaterAccessAvailable = true,
            IsOccupied = false,
            YearlyRentalFee = 100m,
            LastMaintenanceDate = DateTime.Today
        };

        var result = await _service.RegisterNewPlotAsync(newPlot);

        Assert.True(result.PlotIdentifier > 0);
    }

    [Fact]
    public async Task RegisterNewPlotAsync_PersistsPlotToDatabase()
    {
        var newPlot = new GardenPlot
        {
            PlotDesignation = "D001",
            SquareMeters = 15.0,
            SoilType = "Sandy",
            WaterAccessAvailable = false,
            IsOccupied = false,
            YearlyRentalFee = 80m,
            LastMaintenanceDate = DateTime.Today
        };

        var created = await _service.RegisterNewPlotAsync(newPlot);
        var retrieved = await _service.FindPlotByIdentifierAsync(created.PlotIdentifier);

        Assert.NotNull(retrieved);
        Assert.Equal("D001", retrieved.PlotDesignation);
        Assert.Equal(15.0, retrieved.SquareMeters);
        Assert.Equal(80m, retrieved.YearlyRentalFee);
    }

    [Fact]
    public async Task RegisterNewPlotAsync_IncreasesPlotsCount()
    {
        var newPlot = new GardenPlot
        {
            PlotDesignation = "E001",
            SquareMeters = 25.0,
            SoilType = "Clay-Loam",
            WaterAccessAvailable = true,
            IsOccupied = false,
            YearlyRentalFee = 120m,
            LastMaintenanceDate = DateTime.Today
        };

        await _service.RegisterNewPlotAsync(newPlot);
        var all = await _service.RetrieveAllPlotsAsync();

        Assert.Equal(4, all.Count);
    }

    // -------------------------------------------------------------------------
    // ModifyPlotDetailsAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task ModifyPlotDetailsAsync_UpdatesExistingPlotInDatabase()
    {
        var modifiedPlot = new GardenPlot
        {
            PlotIdentifier = 2,
            PlotDesignation = "A002",   // keep same designation (unique constraint)
            SquareMeters = 40.0,
            SoilType = "Clay",
            WaterAccessAvailable = false,
            IsOccupied = true,
            YearlyRentalFee = 200m,
            LastMaintenanceDate = DateTime.Today
        };

        await _service.ModifyPlotDetailsAsync(modifiedPlot);

        // Use AsNoTracking to bypass the change tracker and read from DB
        var updated = await _db.GardenPlots.AsNoTracking()
            .FirstOrDefaultAsync(p => p.PlotIdentifier == 2);
        Assert.NotNull(updated);
        Assert.Equal(40.0, updated.SquareMeters);
        Assert.Equal("Clay", updated.SoilType);
        Assert.Equal(200m, updated.YearlyRentalFee);
        Assert.True(updated.IsOccupied);
    }

    // -------------------------------------------------------------------------
    // RemovePlotAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task RemovePlotAsync_WithExistingId_RemovesPlotFromDatabase()
    {
        // Plot 2 (A002) has no harvest records, safe to delete without cascade concerns
        await _service.RemovePlotAsync(2);

        var deleted = await _service.FindPlotByIdentifierAsync(2);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task RemovePlotAsync_WithExistingId_DecreasesTotalCount()
    {
        await _service.RemovePlotAsync(2);

        var remaining = await _service.RetrieveAllPlotsAsync();
        Assert.Equal(2, remaining.Count);
    }

    [Fact]
    public async Task RemovePlotAsync_WithNonExistentId_DoesNotThrowException()
    {
        var exception = await Record.ExceptionAsync(() => _service.RemovePlotAsync(999));

        Assert.Null(exception);
    }

    // -------------------------------------------------------------------------
    // CheckPlotExistsAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task CheckPlotExistsAsync_WithExistingId_ReturnsTrue()
    {
        var exists = await _service.CheckPlotExistsAsync(1);

        Assert.True(exists);
    }

    [Fact]
    public async Task CheckPlotExistsAsync_WithNonExistentId_ReturnsFalse()
    {
        var exists = await _service.CheckPlotExistsAsync(999);

        Assert.False(exists);
    }

    [Fact]
    public async Task CheckPlotExistsAsync_AfterDeletion_ReturnsFalse()
    {
        await _service.RemovePlotAsync(2);

        var exists = await _service.CheckPlotExistsAsync(2);
        Assert.False(exists);
    }

    // -------------------------------------------------------------------------
    // GetVacantPlotsAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetVacantPlotsAsync_ReturnsOnlyUnoccupiedPlots()
    {
        var vacantPlots = await _service.GetVacantPlotsAsync();

        Assert.All(vacantPlots, p => Assert.False(p.IsOccupied));
    }

    [Fact]
    public async Task GetVacantPlotsAsync_ReturnsCorrectCount()
    {
        // Seed data: A001 (occupied), A002 (vacant), B001 (occupied) => 1 vacant
        var vacantPlots = await _service.GetVacantPlotsAsync();

        Assert.Single(vacantPlots);
        Assert.Equal("A002", vacantPlots[0].PlotDesignation);
    }

    [Fact]
    public async Task GetVacantPlotsAsync_ExcludesOccupiedPlots()
    {
        var vacantPlots = await _service.GetVacantPlotsAsync();
        var designations = vacantPlots.Select(p => p.PlotDesignation).ToList();

        Assert.DoesNotContain("A001", designations);
        Assert.DoesNotContain("B001", designations);
    }

    [Fact]
    public async Task GetVacantPlotsAsync_AfterMarkingPlotOccupied_UpdatesResults()
    {
        // Mark A002 as occupied via an update, then verify it no longer appears in vacant list
        var modifiedPlot = new GardenPlot
        {
            PlotIdentifier = 2,
            PlotDesignation = "A002",
            SquareMeters = 30.0,
            SoilType = "Sandy-Loam",
            WaterAccessAvailable = true,
            IsOccupied = true,          // was false in seed
            YearlyRentalFee = 175m,
            LastMaintenanceDate = new DateTime(2024, 2, 15)
        };
        await _service.ModifyPlotDetailsAsync(modifiedPlot);

        var vacantPlots = await _service.GetVacantPlotsAsync();
        Assert.Empty(vacantPlots);
    }
}
