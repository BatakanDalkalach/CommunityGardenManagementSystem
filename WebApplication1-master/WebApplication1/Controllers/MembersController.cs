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
    }
}
