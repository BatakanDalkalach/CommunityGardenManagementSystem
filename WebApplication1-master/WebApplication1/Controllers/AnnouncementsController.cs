using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.DatabaseContext;

namespace WebApplication1.Controllers
{
    public class AnnouncementsController : Controller
    {
        private readonly CommunityGardenDatabase _ctx;

        public AnnouncementsController(CommunityGardenDatabase ctx)
        {
            _ctx = ctx;
        }

        // GET: /Announcements
        public async Task<IActionResult> Index()
        {
            var announcements = await _ctx.Announcements
                .Where(a => a.IsPublished)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
            return View(announcements);
        }
    }
}
