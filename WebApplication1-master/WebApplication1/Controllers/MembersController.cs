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

        public async Task<IActionResult> Index(string? tier = null)
        {
            var members = string.IsNullOrEmpty(tier) 
                ? await _svc.RetrieveAllMembersAsync()
                : await _svc.SearchByMembershipTypeAsync(tier);
            
            ViewBag.SelectedTier = tier;
            return View(members);
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
