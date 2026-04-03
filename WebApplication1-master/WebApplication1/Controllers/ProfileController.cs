using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.DatabaseContext;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly CommunityGardenDatabase _db;

        public ProfileController(UserManager<ApplicationUser> userManager, CommunityGardenDatabase db)
        {
            _userManager = userManager;
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var linkedMember = await _db.GardenMembers
                .Include(m => m.ManagedPlots)
                .Include(m => m.RecordedHarvests)
                    .ThenInclude(h => h.SourcePlot)
                .FirstOrDefaultAsync(m => m.EmailContact == user.Email);

            var vm = new ProfileViewModel
            {
                User = user,
                LinkedMember = linkedMember
            };

            return View(vm);
        }
    }
}
