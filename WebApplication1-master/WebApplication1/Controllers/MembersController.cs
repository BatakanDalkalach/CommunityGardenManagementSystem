using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    public class MembersController : Controller
    {
        private readonly MemberManagementService _svc;

        public MembersController(MemberManagementService svc)
        {
            _svc = svc;
        }

        public async Task<IActionResult> Index(string? tier = null, int page = 1)
        {
            const int pageSize = 6;
            var allMembers = string.IsNullOrEmpty(tier)
                ? await _svc.RetrieveAllMembersAsync()
                : await _svc.SearchByMembershipTypeAsync(tier);

            var totalPages = (int)Math.Ceiling(allMembers.Count / (double)pageSize);
            page = Math.Max(1, Math.Min(page, Math.Max(1, totalPages)));

            ViewBag.SelectedTier = tier;
            ViewBag.TotalCount = allMembers.Count;
            ViewBag.OrganicCount = allMembers.Count(m => m.PreferOrganicOnly);
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(allMembers.Skip((page - 1) * pageSize).Take(pageSize).ToList());
        }

        public async Task<IActionResult> ViewProfile(int? id)
        {
            if (!id.HasValue) return NotFound();
            
            var member = await _svc.FindMemberByIdAsync(id.Value);
            return member == null ? NotFound() : View(member);
        }

        // Require login to register a new garden member
        // Изисква вход за регистриране на нов член на градината
        [Authorize]
        public IActionResult Register()
        {
            ViewBag.TierOptions = new[] { "Basic", "Standard", "Premium" };
            return View();
        }

        [Authorize]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(GardenMember member)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.TierOptions = new[] { "Basic", "Standard", "Premium" };
                return View(member);
            }

            await _svc.EnrollNewMemberAsync(member);
            TempData["WelcomeMsg"] = $"Welcome {member.FullLegalName}! Registration successful.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Statistics()
        {
            var all = await _svc.RetrieveAllMembersAsync();

            var byTier = all
                .GroupBy(m => m.MembershipTier)
                .ToDictionary(g => g.Key, g => g.Count());

            var vm = new MemberStatisticsViewModel
            {
                TotalMembers = all.Count,
                AverageYearsOfExperience = all.Count > 0
                    ? Math.Round(all.Average(m => m.YearsOfExperience), 1)
                    : 0,
                OrganicCount = all.Count(m => m.PreferOrganicOnly),
                NonOrganicCount = all.Count(m => !m.PreferOrganicOnly),
                MembersWithPlots = all.Count(m => m.ManagedPlots != null && m.ManagedPlots.Any()),
                MembersWithoutPlots = all.Count(m => m.ManagedPlots == null || !m.ManagedPlots.Any()),
                MembersByTier = byTier,
                MostCommonTier = byTier.Count > 0
                    ? byTier.OrderByDescending(kv => kv.Value).First().Key
                    : "N/A",
                NewestMemberYear = all.Count > 0
                    ? all.Max(m => m.RegistrationDate.Year)
                    : DateTime.Today.Year
            };

            return View(vm);
        }

        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (!id.HasValue) return NotFound();

            var member = await _svc.FindMemberByIdAsync(id.Value);
            if (member == null) return NotFound();

            ViewBag.TierOptions = new[] { "Basic", "Standard", "Premium" };
            return View(member);
        }

        [Authorize]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(GardenMember member)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.TierOptions = new[] { "Basic", "Standard", "Premium" };
                return View(member);
            }

            await _svc.UpdateMemberAsync(member);
            TempData["WelcomeMsg"] = $"{member.FullLegalName}'s profile updated successfully.";
            return RedirectToAction(nameof(ViewProfile), new { id = member.MemberId });
        }
    }
}
