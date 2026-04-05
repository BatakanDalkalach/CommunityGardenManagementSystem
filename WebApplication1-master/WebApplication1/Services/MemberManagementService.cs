using Microsoft.EntityFrameworkCore;
using WebApplication1.DatabaseContext;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public class MemberManagementService
    {
        private readonly CommunityGardenDatabase _database;

        public MemberManagementService(CommunityGardenDatabase database)
        {
            _database = database;
        }

        /// <summary>
        /// Retrieves all garden members ordered alphabetically by full legal name,
        /// including their managed plots navigation property.
        /// </summary>
        /// <returns>A list of all <see cref="GardenMember"/> entities.</returns>
        public async Task<List<GardenMember>> RetrieveAllMembersAsync()
        {
            return await _database.GardenMembers
                .Include(member => member.ManagedPlots)
                .OrderBy(member => member.FullLegalName)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves a single garden member by their primary key, including their
        /// managed plots and recorded harvests.
        /// Returns <c>null</c> if no member with the given ID exists.
        /// </summary>
        /// <param name="memberId">The primary key of the garden member.</param>
        /// <returns>The matching <see cref="GardenMember"/> with related data, or <c>null</c> if not found.</returns>
        public async Task<GardenMember?> FindMemberByIdAsync(int memberId)
        {
            return await _database.GardenMembers
                .Include(member => member.ManagedPlots)
                .Include(member => member.RecordedHarvests)
                .FirstOrDefaultAsync(member => member.MemberId == memberId);
        }

        /// <summary>
        /// Persists a new garden member to the database.
        /// </summary>
        /// <param name="member">The <see cref="GardenMember"/> entity to insert.</param>
        /// <returns>The inserted member with its generated primary key populated.</returns>
        public async Task<GardenMember> EnrollNewMemberAsync(GardenMember member)
        {
            _database.GardenMembers.Add(member);
            await _database.SaveChangesAsync();
            return member;
        }

        /// <summary>
        /// Retrieves all garden members whose membership tier matches the specified value,
        /// ordered alphabetically by full legal name.
        /// </summary>
        /// <param name="tier">The membership tier to filter by (e.g., "Basic" or "Premium").</param>
        /// <returns>A filtered list of <see cref="GardenMember"/> entities.</returns>
        public async Task<List<GardenMember>> SearchByMembershipTypeAsync(string tier)
        {
            return await _database.GardenMembers
                .Where(m => m.MembershipTier == tier)
                .OrderBy(m => m.FullLegalName)
                .ToListAsync();
        }

        /// <summary>
        /// Updates selected profile fields of an existing garden member:
        /// <see cref="GardenMember.FullLegalName"/>, <see cref="GardenMember.MembershipTier"/>,
        /// <see cref="GardenMember.YearsOfExperience"/>, <see cref="GardenMember.PreferOrganicOnly"/>,
        /// and <see cref="GardenMember.GardeningInterests"/>.
        /// Does nothing if no member with the given ID exists.
        /// </summary>
        /// <param name="updated">
        /// A <see cref="GardenMember"/> containing the new field values.
        /// The <see cref="GardenMember.MemberId"/> is used to locate the record.
        /// </param>
        public async Task UpdateMemberAsync(GardenMember updated)
        {
            var existing = await _database.GardenMembers.FindAsync(updated.MemberId);
            if (existing == null) return;

            existing.FullLegalName = updated.FullLegalName;
            existing.MembershipTier = updated.MembershipTier;
            existing.YearsOfExperience = updated.YearsOfExperience;
            existing.PreferOrganicOnly = updated.PreferOrganicOnly;
            existing.GardeningInterests = updated.GardeningInterests;

            await _database.SaveChangesAsync();
        }
    }
}
