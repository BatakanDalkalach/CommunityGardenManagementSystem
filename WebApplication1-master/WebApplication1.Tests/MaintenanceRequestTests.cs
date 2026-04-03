using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WebApplication1.DatabaseContext;
using WebApplication1.Models;
using Xunit;

namespace WebApplication1.Tests;

/// <summary>
/// xUnit tests for MaintenanceRequest CRUD operations using an in-memory SQLite database.
/// Each test runs against a fresh database. Seed data is added in the constructor:
///   - Plot A001 (PlotIdentifier = 1) — used as the FK target for all requests
///   - ID=1  "Broken fence post"        (Status = Pending,    RequestDate = 2024-03-10)
///   - ID=2  "Irrigation pipe leaking"  (Status = InProgress, RequestDate = 2024-04-05)
///   - ID=3  "Path resurfacing done"    (Status = Completed,  RequestDate = 2024-05-01)
/// </summary>
public class MaintenanceRequestTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly CommunityGardenDatabase _db;

    public MaintenanceRequestTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<CommunityGardenDatabase>()
            .UseSqlite(_connection)
            .Options;

        _db = new CommunityGardenDatabase(options);
        _db.Database.EnsureCreated();
        // EnsureCreated applies the HasData seed, so GardenPlot with PlotIdentifier=1 already exists.

        // Seed three maintenance requests — one per status value
        _db.MaintenanceRequests.AddRange(
            new MaintenanceRequest
            {
                Id = 1,
                PlotId = 1,
                Description = "Broken fence post near the north boundary needs replacement.",
                RequestDate = new DateTime(2024, 3, 10, 0, 0, 0, DateTimeKind.Utc),
                Status = MaintenanceStatus.Pending
            },
            new MaintenanceRequest
            {
                Id = 2,
                PlotId = 1,
                Description = "Irrigation pipe is leaking at the junction with plot B002.",
                RequestDate = new DateTime(2024, 4, 5, 0, 0, 0, DateTimeKind.Utc),
                Status = MaintenanceStatus.InProgress
            },
            new MaintenanceRequest
            {
                Id = 3,
                PlotId = 1,
                Description = "Gravel path between plots A001 and A002 has been resurfaced.",
                RequestDate = new DateTime(2024, 5, 1, 0, 0, 0, DateTimeKind.Utc),
                Status = MaintenanceStatus.Completed
            }
        );
        _db.SaveChanges();
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
    public async Task CreateMaintenanceRequest_AssignsId()
    {
        var request = new MaintenanceRequest
        {
            PlotId = 1,
            Description = "Shed door hinge is broken and needs to be replaced.",
            RequestDate = new DateTime(2024, 6, 15, 0, 0, 0, DateTimeKind.Utc),
            Status = MaintenanceStatus.Pending
        };

        _db.MaintenanceRequests.Add(request);
        await _db.SaveChangesAsync();

        Assert.True(request.Id > 0);
    }

    [Fact]
    public async Task CreateMaintenanceRequest_PersistsAllFields()
    {
        var date = new DateTime(2024, 7, 1, 0, 0, 0, DateTimeKind.Utc);

        var request = new MaintenanceRequest
        {
            PlotId = 1,
            Description = "Water tap near plot A001 is dripping and wasting water daily.",
            RequestDate = date,
            Status = MaintenanceStatus.InProgress
        };

        _db.MaintenanceRequests.Add(request);
        await _db.SaveChangesAsync();

        var saved = await _db.MaintenanceRequests.AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == request.Id);

        Assert.NotNull(saved);
        Assert.Equal(1, saved.PlotId);
        Assert.Equal("Water tap near plot A001 is dripping and wasting water daily.", saved.Description);
        Assert.Equal(date, saved.RequestDate);
        Assert.Equal(MaintenanceStatus.InProgress, saved.Status);
    }

    [Fact]
    public async Task CreateMaintenanceRequest_IncreasesTotalCount()
    {
        var before = await _db.MaintenanceRequests.CountAsync();

        _db.MaintenanceRequests.Add(new MaintenanceRequest
        {
            PlotId = 1,
            Description = "Compost bin lid is damaged and needs a replacement cover.",
            RequestDate = new DateTime(2024, 8, 1, 0, 0, 0, DateTimeKind.Utc),
            Status = MaintenanceStatus.Pending
        });
        await _db.SaveChangesAsync();

        var after = await _db.MaintenanceRequests.CountAsync();
        Assert.Equal(before + 1, after);
    }

    [Fact]
    public async Task CreateMaintenanceRequest_DefaultStatus_IsPending()
    {
        var request = new MaintenanceRequest
        {
            PlotId = 1,
            Description = "Signpost at the garden entrance is leaning and needs resetting."
        };

        _db.MaintenanceRequests.Add(request);
        await _db.SaveChangesAsync();

        var saved = await _db.MaintenanceRequests.AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == request.Id);

        Assert.NotNull(saved);
        Assert.Equal(MaintenanceStatus.Pending, saved.Status);
    }

    // -------------------------------------------------------------------------
    // Retrieve by status
    // -------------------------------------------------------------------------

    [Fact]
    public async Task RetrieveByStatus_Pending_ReturnsOnlyPendingRequests()
    {
        var pending = await _db.MaintenanceRequests
            .Where(r => r.Status == MaintenanceStatus.Pending)
            .ToListAsync();

        Assert.Single(pending);
        Assert.Equal("Broken fence post near the north boundary needs replacement.", pending[0].Description);
    }

    [Fact]
    public async Task RetrieveByStatus_InProgress_ReturnsOnlyInProgressRequests()
    {
        var inProgress = await _db.MaintenanceRequests
            .Where(r => r.Status == MaintenanceStatus.InProgress)
            .ToListAsync();

        Assert.Single(inProgress);
        Assert.Equal("Irrigation pipe is leaking at the junction with plot B002.", inProgress[0].Description);
    }

    [Fact]
    public async Task RetrieveByStatus_Completed_ReturnsOnlyCompletedRequests()
    {
        var completed = await _db.MaintenanceRequests
            .Where(r => r.Status == MaintenanceStatus.Completed)
            .ToListAsync();

        Assert.Single(completed);
        Assert.Equal("Gravel path between plots A001 and A002 has been resurfaced.", completed[0].Description);
    }

    [Fact]
    public async Task RetrieveByStatus_Pending_DoesNotIncludeInProgressOrCompleted()
    {
        var pending = await _db.MaintenanceRequests
            .Where(r => r.Status == MaintenanceStatus.Pending)
            .ToListAsync();

        Assert.All(pending, r => Assert.Equal(MaintenanceStatus.Pending, r.Status));
        Assert.DoesNotContain(pending, r => r.Status == MaintenanceStatus.InProgress);
        Assert.DoesNotContain(pending, r => r.Status == MaintenanceStatus.Completed);
    }

    [Fact]
    public async Task RetrieveAll_ReturnsAllThreeRequests()
    {
        var all = await _db.MaintenanceRequests.ToListAsync();

        Assert.Equal(3, all.Count);
        Assert.Contains(all, r => r.Status == MaintenanceStatus.Pending);
        Assert.Contains(all, r => r.Status == MaintenanceStatus.InProgress);
        Assert.Contains(all, r => r.Status == MaintenanceStatus.Completed);
    }

    [Fact]
    public async Task RetrieveById_WithValidId_ReturnsCorrectRequest()
    {
        var request = await _db.MaintenanceRequests.FindAsync(2);

        Assert.NotNull(request);
        Assert.Equal(MaintenanceStatus.InProgress, request.Status);
        Assert.Equal("Irrigation pipe is leaking at the junction with plot B002.", request.Description);
    }

    [Fact]
    public async Task RetrieveById_WithInvalidId_ReturnsNull()
    {
        var request = await _db.MaintenanceRequests.FindAsync(999);

        Assert.Null(request);
    }

    // -------------------------------------------------------------------------
    // Update status
    // -------------------------------------------------------------------------

    [Fact]
    public async Task UpdateStatus_PendingToInProgress_Persists()
    {
        var request = await _db.MaintenanceRequests.FindAsync(1);
        Assert.NotNull(request);
        Assert.Equal(MaintenanceStatus.Pending, request.Status);

        request.Status = MaintenanceStatus.InProgress;
        await _db.SaveChangesAsync();

        var updated = await _db.MaintenanceRequests.AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == 1);

        Assert.NotNull(updated);
        Assert.Equal(MaintenanceStatus.InProgress, updated.Status);
    }

    [Fact]
    public async Task UpdateStatus_InProgressToCompleted_Persists()
    {
        var request = await _db.MaintenanceRequests.FindAsync(2);
        Assert.NotNull(request);
        Assert.Equal(MaintenanceStatus.InProgress, request.Status);

        request.Status = MaintenanceStatus.Completed;
        await _db.SaveChangesAsync();

        var updated = await _db.MaintenanceRequests.AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == 2);

        Assert.NotNull(updated);
        Assert.Equal(MaintenanceStatus.Completed, updated.Status);
    }

    [Fact]
    public async Task UpdateStatus_CompletedToPending_Persists()
    {
        var request = await _db.MaintenanceRequests.FindAsync(3);
        Assert.NotNull(request);
        Assert.Equal(MaintenanceStatus.Completed, request.Status);

        request.Status = MaintenanceStatus.Pending;
        await _db.SaveChangesAsync();

        var updated = await _db.MaintenanceRequests.AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == 3);

        Assert.NotNull(updated);
        Assert.Equal(MaintenanceStatus.Pending, updated.Status);
    }

    [Fact]
    public async Task UpdateStatus_DoesNotAffectOtherFields()
    {
        var request = await _db.MaintenanceRequests.FindAsync(1);
        var originalDescription = request!.Description;
        var originalDate = request.RequestDate;
        var originalPlotId = request.PlotId;

        request.Status = MaintenanceStatus.Completed;
        await _db.SaveChangesAsync();

        var updated = await _db.MaintenanceRequests.AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == 1);

        Assert.NotNull(updated);
        Assert.Equal(originalDescription, updated.Description);
        Assert.Equal(originalDate, updated.RequestDate);
        Assert.Equal(originalPlotId, updated.PlotId);
    }

    [Fact]
    public async Task UpdateStatus_PendingToInProgress_AdjustsStatusCounts()
    {
        var pendingBefore = await _db.MaintenanceRequests.CountAsync(r => r.Status == MaintenanceStatus.Pending);
        var inProgressBefore = await _db.MaintenanceRequests.CountAsync(r => r.Status == MaintenanceStatus.InProgress);

        var request = await _db.MaintenanceRequests.FindAsync(1);
        request!.Status = MaintenanceStatus.InProgress;
        await _db.SaveChangesAsync();

        var pendingAfter = await _db.MaintenanceRequests.CountAsync(r => r.Status == MaintenanceStatus.Pending);
        var inProgressAfter = await _db.MaintenanceRequests.CountAsync(r => r.Status == MaintenanceStatus.InProgress);

        Assert.Equal(pendingBefore - 1, pendingAfter);
        Assert.Equal(inProgressBefore + 1, inProgressAfter);
    }

    // -------------------------------------------------------------------------
    // Delete
    // -------------------------------------------------------------------------

    [Fact]
    public async Task DeleteMaintenanceRequest_WithExistingId_RemovesFromDatabase()
    {
        var request = await _db.MaintenanceRequests.FindAsync(1);
        Assert.NotNull(request);

        _db.MaintenanceRequests.Remove(request);
        await _db.SaveChangesAsync();

        var deleted = await _db.MaintenanceRequests.FindAsync(1);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task DeleteMaintenanceRequest_DecreasesTotalCount()
    {
        var before = await _db.MaintenanceRequests.CountAsync();

        var request = await _db.MaintenanceRequests.FindAsync(2);
        _db.MaintenanceRequests.Remove(request!);
        await _db.SaveChangesAsync();

        var after = await _db.MaintenanceRequests.CountAsync();
        Assert.Equal(before - 1, after);
    }

    [Fact]
    public async Task DeleteMaintenanceRequest_OtherRequestsUnaffected()
    {
        var request = await _db.MaintenanceRequests.FindAsync(1);
        _db.MaintenanceRequests.Remove(request!);
        await _db.SaveChangesAsync();

        var remaining2 = await _db.MaintenanceRequests.FindAsync(2);
        var remaining3 = await _db.MaintenanceRequests.FindAsync(3);

        Assert.NotNull(remaining2);
        Assert.Equal(MaintenanceStatus.InProgress, remaining2.Status);
        Assert.NotNull(remaining3);
        Assert.Equal(MaintenanceStatus.Completed, remaining3.Status);
    }

    [Fact]
    public async Task DeleteMaintenanceRequest_PendingRequest_RemovesSuccessfully()
    {
        var request = await _db.MaintenanceRequests.FindAsync(1);
        Assert.NotNull(request);
        Assert.Equal(MaintenanceStatus.Pending, request.Status);

        _db.MaintenanceRequests.Remove(request);
        await _db.SaveChangesAsync();

        var deleted = await _db.MaintenanceRequests.FindAsync(1);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task DeleteMaintenanceRequest_CompletedRequest_RemovesSuccessfully()
    {
        var request = await _db.MaintenanceRequests.FindAsync(3);
        Assert.NotNull(request);
        Assert.Equal(MaintenanceStatus.Completed, request.Status);

        _db.MaintenanceRequests.Remove(request);
        await _db.SaveChangesAsync();

        var deleted = await _db.MaintenanceRequests.FindAsync(3);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task DeleteMaintenanceRequest_WithNonExistentId_DoesNotThrow()
    {
        var request = await _db.MaintenanceRequests.FindAsync(999);

        var exception = await Record.ExceptionAsync(async () =>
        {
            if (request != null)
            {
                _db.MaintenanceRequests.Remove(request);
                await _db.SaveChangesAsync();
            }
        });

        Assert.Null(exception);
    }
}
