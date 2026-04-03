using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WebApplication1.DatabaseContext;
using WebApplication1.Models;
using Xunit;

namespace WebApplication1.Tests;

/// <summary>
/// xUnit tests for Announcement CRUD operations using an in-memory SQLite database.
/// Each test runs against a fresh database. Seed data is added per-test as needed:
///   - ID=1 "Spring Planting Workshop"  (IsPublished = true,  CreatedAt = 2024-03-01)
///   - ID=2 "Water Schedule Update"     (IsPublished = true,  CreatedAt = 2024-04-10)
///   - ID=3 "Draft: Summer BBQ Plans"   (IsPublished = false, CreatedAt = 2024-05-20)
/// </summary>
public class AnnouncementTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly CommunityGardenDatabase _db;

    public AnnouncementTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<CommunityGardenDatabase>()
            .UseSqlite(_connection)
            .Options;

        _db = new CommunityGardenDatabase(options);
        _db.Database.EnsureCreated();

        // Seed three announcements: two published, one unpublished
        _db.Announcements.AddRange(
            new Announcement
            {
                Id = 1,
                Title = "Spring Planting Workshop",
                Content = "Join us on April 5th for a hands-on planting workshop in the main garden area.",
                CreatedAt = new DateTime(2024, 3, 1, 0, 0, 0, DateTimeKind.Utc),
                IsPublished = true
            },
            new Announcement
            {
                Id = 2,
                Title = "Water Schedule Update",
                Content = "Due to maintenance, watering will be unavailable on weekends starting May 1st.",
                CreatedAt = new DateTime(2024, 4, 10, 0, 0, 0, DateTimeKind.Utc),
                IsPublished = true
            },
            new Announcement
            {
                Id = 3,
                Title = "Draft: Summer BBQ Plans",
                Content = "Planning notes for the end-of-season BBQ — not ready for members yet.",
                CreatedAt = new DateTime(2024, 5, 20, 0, 0, 0, DateTimeKind.Utc),
                IsPublished = false
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
    public async Task CreateAnnouncement_AssignsId()
    {
        var announcement = new Announcement
        {
            Title = "New Composting Station",
            Content = "A composting station has been installed near Plot B. All members are welcome to contribute.",
            IsPublished = true
        };

        _db.Announcements.Add(announcement);
        await _db.SaveChangesAsync();

        Assert.True(announcement.Id > 0);
    }

    [Fact]
    public async Task CreateAnnouncement_PersistsAllFields()
    {
        var created = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc);

        var announcement = new Announcement
        {
            Title = "Tool Library Opening",
            Content = "The shared tool library is now open every Saturday from 9am to noon.",
            CreatedAt = created,
            IsPublished = false
        };

        _db.Announcements.Add(announcement);
        await _db.SaveChangesAsync();

        var saved = await _db.Announcements.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == announcement.Id);

        Assert.NotNull(saved);
        Assert.Equal("Tool Library Opening", saved.Title);
        Assert.Equal("The shared tool library is now open every Saturday from 9am to noon.", saved.Content);
        Assert.Equal(created, saved.CreatedAt);
        Assert.False(saved.IsPublished);
    }

    [Fact]
    public async Task CreateAnnouncement_IncreasesTotalCount()
    {
        var before = await _db.Announcements.CountAsync();

        _db.Announcements.Add(new Announcement
        {
            Title = "Plot Inspection Notice",
            Content = "Annual plot inspections will take place during the last week of September.",
            IsPublished = true
        });
        await _db.SaveChangesAsync();

        var after = await _db.Announcements.CountAsync();
        Assert.Equal(before + 1, after);
    }

    // -------------------------------------------------------------------------
    // Retrieve published vs unpublished
    // -------------------------------------------------------------------------

    [Fact]
    public async Task RetrievePublished_ReturnsOnlyPublishedAnnouncements()
    {
        var published = await _db.Announcements
            .Where(a => a.IsPublished)
            .ToListAsync();

        Assert.Equal(2, published.Count);
        Assert.All(published, a => Assert.True(a.IsPublished));
    }

    [Fact]
    public async Task RetrieveUnpublished_ReturnsOnlyUnpublishedAnnouncements()
    {
        var unpublished = await _db.Announcements
            .Where(a => !a.IsPublished)
            .ToListAsync();

        Assert.Single(unpublished);
        Assert.Equal("Draft: Summer BBQ Plans", unpublished[0].Title);
    }

    [Fact]
    public async Task RetrievePublished_DoesNotIncludeUnpublishedAnnouncements()
    {
        var published = await _db.Announcements
            .Where(a => a.IsPublished)
            .ToListAsync();

        Assert.DoesNotContain(published, a => a.Title == "Draft: Summer BBQ Plans");
    }

    [Fact]
    public async Task RetrieveAll_ReturnsBothPublishedAndUnpublished()
    {
        var all = await _db.Announcements.ToListAsync();

        Assert.Equal(3, all.Count);
        Assert.Contains(all, a => a.IsPublished);
        Assert.Contains(all, a => !a.IsPublished);
    }

    [Fact]
    public async Task RetrieveById_WithValidId_ReturnsCorrectAnnouncement()
    {
        var announcement = await _db.Announcements.FindAsync(1);

        Assert.NotNull(announcement);
        Assert.Equal("Spring Planting Workshop", announcement.Title);
        Assert.True(announcement.IsPublished);
    }

    [Fact]
    public async Task RetrieveById_WithInvalidId_ReturnsNull()
    {
        var announcement = await _db.Announcements.FindAsync(999);

        Assert.Null(announcement);
    }

    [Fact]
    public async Task RetrievePublished_OrderedByDateDescending_MostRecentFirst()
    {
        var published = await _db.Announcements
            .Where(a => a.IsPublished)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        Assert.Equal("Water Schedule Update", published[0].Title);
        Assert.Equal("Spring Planting Workshop", published[1].Title);
    }

    // -------------------------------------------------------------------------
    // Update IsPublished
    // -------------------------------------------------------------------------

    [Fact]
    public async Task UpdateIsPublished_UnpublishedToPublished_Persists()
    {
        var draft = await _db.Announcements.FindAsync(3);
        Assert.NotNull(draft);
        Assert.False(draft.IsPublished);

        draft.IsPublished = true;
        await _db.SaveChangesAsync();

        var updated = await _db.Announcements.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == 3);

        Assert.NotNull(updated);
        Assert.True(updated.IsPublished);
    }

    [Fact]
    public async Task UpdateIsPublished_PublishedToUnpublished_Persists()
    {
        var live = await _db.Announcements.FindAsync(1);
        Assert.NotNull(live);
        Assert.True(live.IsPublished);

        live.IsPublished = false;
        await _db.SaveChangesAsync();

        var updated = await _db.Announcements.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == 1);

        Assert.NotNull(updated);
        Assert.False(updated.IsPublished);
    }

    [Fact]
    public async Task UpdateIsPublished_PublishingDraft_IncreasesPublishedCount()
    {
        var beforeCount = await _db.Announcements.CountAsync(a => a.IsPublished);

        var draft = await _db.Announcements.FindAsync(3);
        draft!.IsPublished = true;
        await _db.SaveChangesAsync();

        var afterCount = await _db.Announcements.CountAsync(a => a.IsPublished);
        Assert.Equal(beforeCount + 1, afterCount);
    }

    [Fact]
    public async Task UpdateIsPublished_DoesNotAffectOtherFields()
    {
        var draft = await _db.Announcements.FindAsync(3);
        var originalTitle = draft!.Title;
        var originalContent = draft.Content;
        var originalCreatedAt = draft.CreatedAt;

        draft.IsPublished = true;
        await _db.SaveChangesAsync();

        var updated = await _db.Announcements.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == 3);

        Assert.NotNull(updated);
        Assert.Equal(originalTitle, updated.Title);
        Assert.Equal(originalContent, updated.Content);
        Assert.Equal(originalCreatedAt, updated.CreatedAt);
    }

    // -------------------------------------------------------------------------
    // Delete
    // -------------------------------------------------------------------------

    [Fact]
    public async Task DeleteAnnouncement_WithExistingId_RemovesFromDatabase()
    {
        var announcement = await _db.Announcements.FindAsync(1);
        Assert.NotNull(announcement);

        _db.Announcements.Remove(announcement);
        await _db.SaveChangesAsync();

        var deleted = await _db.Announcements.FindAsync(1);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task DeleteAnnouncement_DecreasesTotalCount()
    {
        var before = await _db.Announcements.CountAsync();

        var announcement = await _db.Announcements.FindAsync(2);
        _db.Announcements.Remove(announcement!);
        await _db.SaveChangesAsync();

        var after = await _db.Announcements.CountAsync();
        Assert.Equal(before - 1, after);
    }

    [Fact]
    public async Task DeleteAnnouncement_OtherAnnouncementsUnaffected()
    {
        var announcement = await _db.Announcements.FindAsync(1);
        _db.Announcements.Remove(announcement!);
        await _db.SaveChangesAsync();

        var remaining2 = await _db.Announcements.FindAsync(2);
        var remaining3 = await _db.Announcements.FindAsync(3);

        Assert.NotNull(remaining2);
        Assert.Equal("Water Schedule Update", remaining2.Title);
        Assert.NotNull(remaining3);
        Assert.Equal("Draft: Summer BBQ Plans", remaining3.Title);
    }

    [Fact]
    public async Task DeleteAnnouncement_UnpublishedDraft_RemovesSuccessfully()
    {
        var draft = await _db.Announcements.FindAsync(3);
        Assert.NotNull(draft);
        Assert.False(draft.IsPublished);

        _db.Announcements.Remove(draft);
        await _db.SaveChangesAsync();

        var deleted = await _db.Announcements.FindAsync(3);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task DeleteAnnouncement_WithNonExistentId_DoesNotThrow()
    {
        var announcement = await _db.Announcements.FindAsync(999);

        var exception = await Record.ExceptionAsync(async () =>
        {
            if (announcement != null)
            {
                _db.Announcements.Remove(announcement);
                await _db.SaveChangesAsync();
            }
        });

        Assert.Null(exception);
    }
}
