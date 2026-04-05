using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApplication1.DatabaseContext;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class MaintenanceRequestsController : Controller
    {
        private readonly CommunityGardenDatabase _ctx;

        public MaintenanceRequestsController(CommunityGardenDatabase ctx)
        {
            _ctx = ctx;
        }

        // GET: /MaintenanceRequests
        public async Task<IActionResult> Index()
        {
            var requests = await _ctx.MaintenanceRequests
                .Include(r => r.Plot)
                .OrderByDescending(r => r.RequestDate)
                .ToListAsync();
            return View(requests);
        }

        // GET: /MaintenanceRequests/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var request = await _ctx.MaintenanceRequests
                .Include(r => r.Plot)
                .FirstOrDefaultAsync(r => r.Id == id);

            return request == null ? NotFound() : View(request);
        }

        // Require login to create, edit, or delete maintenance requests
        // Изисква вход за създаване, редактиране или изтриване на заявки за поддръжка
        [Authorize]
        public IActionResult Create()
        {
            LoadPlotOptions();
            return View();
        }

        [Authorize]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MaintenanceRequest request)
        {
            if (!ModelState.IsValid)
            {
                LoadPlotOptions(request.PlotId);
                return View(request);
            }

            try
            {
                _ctx.MaintenanceRequests.Add(request);
                await _ctx.SaveChangesAsync();
                TempData["SuccessMsg"] = "Maintenance request submitted successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(string.Empty, "Unable to submit the request. Please try again.");
                LoadPlotOptions(request.PlotId);
                return View(request);
            }
        }

        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var request = await _ctx.MaintenanceRequests.FindAsync(id);
            if (request == null) return NotFound();

            LoadPlotOptions(request.PlotId);
            return View(request);
        }

        [Authorize]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MaintenanceRequest request)
        {
            if (id != request.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                LoadPlotOptions(request.PlotId);
                return View(request);
            }

            try
            {
                _ctx.MaintenanceRequests.Update(request);
                await _ctx.SaveChangesAsync();
                TempData["SuccessMsg"] = "Maintenance request updated successfully!";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _ctx.MaintenanceRequests.AnyAsync(r => r.Id == id))
                    return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var request = await _ctx.MaintenanceRequests
                .Include(r => r.Plot)
                .FirstOrDefaultAsync(r => r.Id == id);

            return request == null ? NotFound() : View(request);
        }

        [Authorize]
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var request = await _ctx.MaintenanceRequests.FindAsync(id);
                if (request != null)
                {
                    _ctx.MaintenanceRequests.Remove(request);
                    await _ctx.SaveChangesAsync();
                }
                TempData["SuccessMsg"] = "Maintenance request removed successfully!";
            }
            catch (DbUpdateException)
            {
                TempData["ErrorMsg"] = "Unable to delete this maintenance request.";
            }
            return RedirectToAction(nameof(Index));
        }

        private void LoadPlotOptions(object? current = null)
        {
            var plots = _ctx.GardenPlots
                .OrderBy(p => p.PlotDesignation)
                .ToListAsync().Result;
            ViewBag.PlotOptions = new SelectList(plots, "PlotIdentifier", "PlotDesignation", current);
        }
    }
}
