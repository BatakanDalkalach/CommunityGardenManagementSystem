using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.DatabaseContext;
using WebApplication1.Models;

namespace WebApplication1.Areas.Admin.Controllers
{
    // Admin area: accessible only to users with the "Admin" role.
    // Административна област: достъпна само за потребители с роля "Admin".
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly CommunityGardenDatabase _ctx;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(CommunityGardenDatabase ctx, UserManager<ApplicationUser> userManager)
        {
            _ctx = ctx;
            _userManager = userManager;
        }

        // GET: /Admin/Dashboard/Index
        public async Task<IActionResult> Index()
        {
            // Gather garden statistics for the admin overview
            // Събиране на статистики за административния преглед
            ViewBag.TotalPlots = await _ctx.GardenPlots.CountAsync();
            ViewBag.OccupiedPlots = await _ctx.GardenPlots.CountAsync(p => p.IsOccupied);
            ViewBag.TotalMembers = await _ctx.GardenMembers.CountAsync();
            ViewBag.TotalHarvests = await _ctx.HarvestRecords.CountAsync();
            ViewBag.TotalUsers = _userManager.Users.Count();

            var recentMembers = await _ctx.GardenMembers
                .OrderByDescending(m => m.RegistrationDate)
                .Take(5)
                .ToListAsync();

            var recentHarvests = await _ctx.HarvestRecords
                .Include(h => h.Harvester)
                .Include(h => h.SourcePlot)
                .OrderByDescending(h => h.CollectionDate)
                .Take(5)
                .ToListAsync();

            ViewBag.RecentMembers = recentMembers;
            ViewBag.RecentHarvests = recentHarvests;

            return View();
        }

        // GET: /Admin/Dashboard/Users
        public async Task<IActionResult> Users()
        {
            // List all Identity users with their roles
            // Изброяване на всички потребители с техните роли
            var users = _userManager.Users.ToList();
            var userRoles = new Dictionary<string, IList<string>>();

            foreach (var user in users)
                userRoles[user.Id] = await _userManager.GetRolesAsync(user);

            ViewBag.UserRoles = userRoles;
            return View(users);
        }
    }
}
