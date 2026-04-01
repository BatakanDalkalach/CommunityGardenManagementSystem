using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WebApplication1.DatabaseContext;
using WebApplication1.Models;
using WebApplication1.Services;
using Xunit;

namespace WebApplication1.Tests;

/// <summary>
/// xUnit tests for MemberManagementService using an in-memory SQLite database.
/// Each test runs against a fresh database seeded with:
///   - Member ID=1: Sarah Johnson, tier=Premium
///   - Member ID=2: Michael Chen,  tier=Basic
/// </summary>
public class MemberManagementServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly CommunityGardenDatabase _db;
    private readonly MemberManagementService _service;

    public MemberManagementServiceTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<CommunityGardenDatabase>()
            .UseSqlite(_connection)
            .Options;

        _db = new CommunityGardenDatabase(options);
        _db.Database.EnsureCreated();
        _service = new MemberManagementService(_db);
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    // -------------------------------------------------------------------------
    // RetrieveAllMembersAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task RetrieveAllMembersAsync_ReturnsAllSeededMembers()
    {
        var members = await _service.RetrieveAllMembersAsync();

        Assert.Equal(2, members.Count);
    }

    [Fact]
    public async Task RetrieveAllMembersAsync_ReturnsOrderedByFullLegalName()
    {
        var members = await _service.RetrieveAllMembersAsync();
        var names = members.Select(m => m.FullLegalName).ToList();

        Assert.Equal(names.OrderBy(n => n).ToList(), names);
    }

    [Fact]
    public async Task RetrieveAllMembersAsync_IncludesManagedPlots()
    {
        var members = await _service.RetrieveAllMembersAsync();
        var sarah = members.First(m => m.MemberId == 1);

        Assert.NotNull(sarah.ManagedPlots);
        Assert.NotEmpty(sarah.ManagedPlots);
    }

    [Fact]
    public async Task RetrieveAllMembersAsync_MemberWithNoPlots_HasEmptyManagedPlots()
    {
        // Enroll a new member who has no plots assigned
        var newMember = await _service.EnrollNewMemberAsync(new GardenMember
        {
            FullLegalName = "Zara Novak",
            EmailContact = "zara.novak@example.com",
            MembershipTier = "Basic",
            RegistrationDate = DateTime.Today,
            YearsOfExperience = 1,
            PreferOrganicOnly = false
        });

        var all = await _service.RetrieveAllMembersAsync();
        var zara = all.First(m => m.MemberId == newMember.MemberId);

        Assert.NotNull(zara.ManagedPlots);
        Assert.Empty(zara.ManagedPlots);
    }

    // -------------------------------------------------------------------------
    // FindMemberByIdAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task FindMemberByIdAsync_WithValidId_ReturnsCorrectMember()
    {
        var member = await _service.FindMemberByIdAsync(1);

        Assert.NotNull(member);
        Assert.Equal(1, member.MemberId);
        Assert.Equal("Sarah Johnson", member.FullLegalName);
        Assert.Equal("sarah.j@email.com", member.EmailContact);
    }

    [Fact]
    public async Task FindMemberByIdAsync_WithValidId_IncludesManagedPlots()
    {
        var member = await _service.FindMemberByIdAsync(1);

        Assert.NotNull(member);
        Assert.NotNull(member.ManagedPlots);
        Assert.NotEmpty(member.ManagedPlots);
    }

    [Fact]
    public async Task FindMemberByIdAsync_WithValidId_IncludesRecordedHarvests()
    {
        var member = await _service.FindMemberByIdAsync(1);

        Assert.NotNull(member);
        Assert.NotNull(member.RecordedHarvests);
        Assert.NotEmpty(member.RecordedHarvests);
    }

    [Fact]
    public async Task FindMemberByIdAsync_WithInvalidId_ReturnsNull()
    {
        var member = await _service.FindMemberByIdAsync(999);

        Assert.Null(member);
    }

    [Fact]
    public async Task FindMemberByIdAsync_ReturnsCorrectMembershipTier()
    {
        var michael = await _service.FindMemberByIdAsync(2);

        Assert.NotNull(michael);
        Assert.Equal("Basic", michael.MembershipTier);
        Assert.Equal("Michael Chen", michael.FullLegalName);
    }

    // -------------------------------------------------------------------------
    // EnrollNewMemberAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task EnrollNewMemberAsync_ReturnsEnrolledMemberWithAssignedId()
    {
        var newMember = new GardenMember
        {
            FullLegalName = "Alice Green",
            EmailContact = "alice.green@example.com",
            MembershipTier = "Basic",
            RegistrationDate = DateTime.Today,
            YearsOfExperience = 3,
            PreferOrganicOnly = true
        };

        var result = await _service.EnrollNewMemberAsync(newMember);

        Assert.True(result.MemberId > 0);
        Assert.Equal("Alice Green", result.FullLegalName);
    }

    [Fact]
    public async Task EnrollNewMemberAsync_PersistsMemberToDatabase()
    {
        var newMember = new GardenMember
        {
            FullLegalName = "Bob Smith",
            EmailContact = "bob.smith@example.com",
            MembershipTier = "Premium",
            RegistrationDate = DateTime.Today,
            YearsOfExperience = 10,
            PreferOrganicOnly = false,
            GardeningInterests = "Berries, Herbs"
        };

        var enrolled = await _service.EnrollNewMemberAsync(newMember);
        var retrieved = await _service.FindMemberByIdAsync(enrolled.MemberId);

        Assert.NotNull(retrieved);
        Assert.Equal("Bob Smith", retrieved.FullLegalName);
        Assert.Equal("bob.smith@example.com", retrieved.EmailContact);
        Assert.Equal("Premium", retrieved.MembershipTier);
        Assert.Equal(10, retrieved.YearsOfExperience);
    }

    [Fact]
    public async Task EnrollNewMemberAsync_IncreasesTotalMemberCount()
    {
        var newMember = new GardenMember
        {
            FullLegalName = "Carol White",
            EmailContact = "carol.white@example.com",
            MembershipTier = "Basic",
            RegistrationDate = DateTime.Today,
            YearsOfExperience = 1,
            PreferOrganicOnly = true
        };

        await _service.EnrollNewMemberAsync(newMember);
        var all = await _service.RetrieveAllMembersAsync();

        Assert.Equal(3, all.Count);
    }

    [Fact]
    public async Task EnrollNewMemberAsync_MultipleMembersGetDistinctIds()
    {
        var memberA = await _service.EnrollNewMemberAsync(new GardenMember
        {
            FullLegalName = "Dave Brown",
            EmailContact = "dave.brown@example.com",
            MembershipTier = "Basic",
            RegistrationDate = DateTime.Today
        });

        var memberB = await _service.EnrollNewMemberAsync(new GardenMember
        {
            FullLegalName = "Eve Black",
            EmailContact = "eve.black@example.com",
            MembershipTier = "Premium",
            RegistrationDate = DateTime.Today
        });

        Assert.NotEqual(memberA.MemberId, memberB.MemberId);
    }

    // -------------------------------------------------------------------------
    // SearchByMembershipTypeAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task SearchByMembershipTypeAsync_WithPremiumTier_ReturnsPremiumMembers()
    {
        var premiumMembers = await _service.SearchByMembershipTypeAsync("Premium");

        Assert.Single(premiumMembers);
        Assert.Equal("Sarah Johnson", premiumMembers[0].FullLegalName);
    }

    [Fact]
    public async Task SearchByMembershipTypeAsync_WithBasicTier_ReturnsBasicMembers()
    {
        var basicMembers = await _service.SearchByMembershipTypeAsync("Basic");

        Assert.Single(basicMembers);
        Assert.Equal("Michael Chen", basicMembers[0].FullLegalName);
    }

    [Fact]
    public async Task SearchByMembershipTypeAsync_WithNonExistentTier_ReturnsEmptyList()
    {
        var goldMembers = await _service.SearchByMembershipTypeAsync("Gold");

        Assert.Empty(goldMembers);
    }

    [Fact]
    public async Task SearchByMembershipTypeAsync_ReturnsOnlyMatchingTier()
    {
        var premiumMembers = await _service.SearchByMembershipTypeAsync("Premium");

        Assert.All(premiumMembers, m => Assert.Equal("Premium", m.MembershipTier));
    }

    [Fact]
    public async Task SearchByMembershipTypeAsync_ReturnsOrderedByFullLegalName()
    {
        // Add a second Premium member so we can verify ordering
        await _service.EnrollNewMemberAsync(new GardenMember
        {
            FullLegalName = "Aaron Davis",
            EmailContact = "aaron.davis@example.com",
            MembershipTier = "Premium",
            RegistrationDate = DateTime.Today,
            YearsOfExperience = 5,
            PreferOrganicOnly = false
        });

        var premiumMembers = await _service.SearchByMembershipTypeAsync("Premium");

        Assert.Equal(2, premiumMembers.Count);
        Assert.Equal("Aaron Davis", premiumMembers[0].FullLegalName);
        Assert.Equal("Sarah Johnson", premiumMembers[1].FullLegalName);
    }

    [Fact]
    public async Task SearchByMembershipTypeAsync_AfterEnrollingNewMember_IncludesNewMember()
    {
        await _service.EnrollNewMemberAsync(new GardenMember
        {
            FullLegalName = "Frank Lee",
            EmailContact = "frank.lee@example.com",
            MembershipTier = "Basic",
            RegistrationDate = DateTime.Today,
            YearsOfExperience = 2,
            PreferOrganicOnly = true
        });

        var basicMembers = await _service.SearchByMembershipTypeAsync("Basic");

        Assert.Equal(2, basicMembers.Count);
        Assert.Contains(basicMembers, m => m.FullLegalName == "Frank Lee");
        Assert.Contains(basicMembers, m => m.FullLegalName == "Michael Chen");
    }
}
