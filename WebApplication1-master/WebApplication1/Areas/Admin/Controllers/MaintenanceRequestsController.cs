using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.DatabaseContext;
using WebApplication1.Models;

namespace WebApplication1.Areas.Admin.Controllers
{
    // Admin area: view and update status of maintenance requests, restricted to Admin role.
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class MaintenanceRequestsController : Controller
    {
        private readonly CommunityGardenDatabase _ctx;

        public MaintenanceRequestsController(CommunityGardenDatabase ctx)
        {
            _ctx = ctx;
        }

        // GET: /Admin/MaintenanceRequests
        public async Task<IActionResult> Index(string? statusFilter)
        {
            var query = _ctx.MaintenanceRequests
                .Include(r => r.Plot)
                .AsQueryable();

            if (!string.IsNullOrEmpty(statusFilter) &&
                Enum.TryParse<MaintenanceStatus>(statusFilter, out var parsed))
            {
                query = query.Where(r => r.Status == parsed);
            }

            ViewBag.StatusFilter = statusFilter;
            ViewBag.PendingCount = await _ctx.MaintenanceRequests.CountAsync(r => r.Status == MaintenanceStatus.Pending);
            ViewBag.InProgressCount = await _ctx.MaintenanceRequests.CountAsync(r => r.Status == MaintenanceStatus.InProgress);
            ViewBag.CompletedCount = await _ctx.MaintenanceRequests.CountAsync(r => r.Status == MaintenanceStatus.Completed);

            var requests = await query
                .OrderByDescending(r => r.RequestDate)
                .ToListAsync();

            return View(requests);
        }

        // GET: /Admin/MaintenanceRequests/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var request = await _ctx.MaintenanceRequests
                .Include(r => r.Plot)
                .FirstOrDefaultAsync(r => r.Id == id);

            return request == null ? NotFound() : View(request);
        }

        // GET: /Admin/MaintenanceRequests/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var request = await _ctx.MaintenanceRequests
                .Include(r => r.Plot)
                .FirstOrDefaultAsync(r => r.Id == id);

            return request == null ? NotFound() : View(request);
        }

        // POST: /Admin/MaintenanceRequests/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MaintenanceStatus status)
        {
            var request = await _ctx.MaintenanceRequests.FindAsync(id);
            if (request == null) return NotFound();

            request.Status = status;

            try
            {
                await _ctx.SaveChangesAsync();
                TempData["SuccessMsg"] = "Status updated successfully!";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _ctx.MaintenanceRequests.AnyAsync(r => r.Id == id))
                    return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
