using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.DatabaseContext;
using WebApplication1.Models;

namespace WebApplication1.Areas.Admin.Controllers
{
    // Admin area: full CRUD for Announcements, restricted to Admin role.
    // Административна област: пълен CRUD за съобщения, ограничен до роля "Admin".
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AnnouncementsController : Controller
    {
        private readonly CommunityGardenDatabase _ctx;

        public AnnouncementsController(CommunityGardenDatabase ctx)
        {
            _ctx = ctx;
        }

        // GET: /Admin/Announcements
        public async Task<IActionResult> Index()
        {
            var announcements = await _ctx.Announcements
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
            return View(announcements);
        }

        // GET: /Admin/Announcements/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var announcement = await _ctx.Announcements.FindAsync(id);
            return announcement == null ? NotFound() : View(announcement);
        }

        // GET: /Admin/Announcements/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Admin/Announcements/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Announcement announcement)
        {
            if (!ModelState.IsValid) return View(announcement);

            try
            {
                announcement.CreatedAt = DateTime.UtcNow;
                _ctx.Announcements.Add(announcement);
                await _ctx.SaveChangesAsync();
                TempData["SuccessMsg"] = "Announcement created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(string.Empty, "Unable to create the announcement. Please try again.");
                return View(announcement);
            }
        }

        // GET: /Admin/Announcements/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var announcement = await _ctx.Announcements.FindAsync(id);
            return announcement == null ? NotFound() : View(announcement);
        }

        // POST: /Admin/Announcements/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Announcement announcement)
        {
            if (id != announcement.Id) return NotFound();

            if (!ModelState.IsValid) return View(announcement);

            try
            {
                _ctx.Announcements.Update(announcement);
                await _ctx.SaveChangesAsync();
                TempData["SuccessMsg"] = "Announcement updated successfully!";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _ctx.Announcements.AnyAsync(a => a.Id == id))
                    return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /Admin/Announcements/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var announcement = await _ctx.Announcements.FindAsync(id);
            return announcement == null ? NotFound() : View(announcement);
        }

        // POST: /Admin/Announcements/Delete/5
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var announcement = await _ctx.Announcements.FindAsync(id);
                if (announcement != null)
                {
                    _ctx.Announcements.Remove(announcement);
                    await _ctx.SaveChangesAsync();
                }
                TempData["SuccessMsg"] = "Announcement deleted successfully!";
            }
            catch (DbUpdateException)
            {
                TempData["ErrorMsg"] = "Unable to delete this announcement.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
