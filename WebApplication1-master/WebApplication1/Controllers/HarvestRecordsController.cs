using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApplication1.DatabaseContext;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class HarvestRecordsController : Controller
    {
        private readonly CommunityGardenDatabase _ctx;

        public HarvestRecordsController(CommunityGardenDatabase ctx)
        {
            _ctx = ctx;
        }

        // GET: /HarvestRecords
        public async Task<IActionResult> Index(string? crop = null, int page = 1)
        {
            const int pageSize = 8;

            var query = _ctx.HarvestRecords
                .Include(h => h.SourcePlot)
                .Include(h => h.Harvester)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(crop))
                query = query.Where(h => h.CropName.Contains(crop));

            var allRecords = await query.OrderByDescending(h => h.CollectionDate).ToListAsync();

            var totalPages = (int)Math.Ceiling(allRecords.Count / (double)pageSize);
            page = Math.Max(1, Math.Min(page, Math.Max(1, totalPages)));

            ViewBag.TotalCount = allRecords.Count;
            ViewBag.OrganicCount = allRecords.Count(h => h.IsOrganicCertified);
            ViewBag.TotalKg = allRecords.Sum(h => h.QuantityKilograms);
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.CropFilter = crop;

            return View(allRecords.Skip((page - 1) * pageSize).Take(pageSize).ToList());
        }

        // GET: /HarvestRecords/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var record = await _ctx.HarvestRecords
                .Include(h => h.SourcePlot)
                .Include(h => h.Harvester)
                .FirstOrDefaultAsync(h => h.RecordId == id);

            return record == null ? NotFound() : View(record);
        }

        [Authorize]
        public IActionResult Create()
        {
            LoadDropdowns();
            return View();
        }

        [Authorize]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(HarvestRecord record)
        {
            if (!ModelState.IsValid)
            {
                LoadDropdowns(record.PlotIdentifier, record.MemberId);
                return View(record);
            }

            _ctx.HarvestRecords.Add(record);
            await _ctx.SaveChangesAsync();

            TempData["SuccessMsg"] = $"Harvest of {record.CropName} logged successfully!";
            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var record = await _ctx.HarvestRecords.FindAsync(id);
            if (record == null) return NotFound();

            LoadDropdowns(record.PlotIdentifier, record.MemberId);
            return View(record);
        }

        [Authorize]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, HarvestRecord record)
        {
            if (id != record.RecordId) return NotFound();

            if (!ModelState.IsValid)
            {
                LoadDropdowns(record.PlotIdentifier, record.MemberId);
                return View(record);
            }

            try
            {
                _ctx.HarvestRecords.Update(record);
                await _ctx.SaveChangesAsync();
                TempData["SuccessMsg"] = "Harvest record updated successfully!";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _ctx.HarvestRecords.AnyAsync(h => h.RecordId == id))
                    return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var record = await _ctx.HarvestRecords
                .Include(h => h.SourcePlot)
                .Include(h => h.Harvester)
                .FirstOrDefaultAsync(h => h.RecordId == id);

            return record == null ? NotFound() : View(record);
        }

        [Authorize]
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var record = await _ctx.HarvestRecords.FindAsync(id);
            if (record != null)
            {
                _ctx.HarvestRecords.Remove(record);
                await _ctx.SaveChangesAsync();
            }

            TempData["SuccessMsg"] = "Harvest record removed successfully!";
            return RedirectToAction(nameof(Index));
        }

        private void LoadDropdowns(object? currentPlot = null, object? currentMember = null)
        {
            var plots = _ctx.GardenPlots.OrderBy(p => p.PlotDesignation).ToListAsync().Result;
            var members = _ctx.GardenMembers.OrderBy(m => m.FullLegalName).ToListAsync().Result;

            ViewBag.PlotOptions = new SelectList(plots, "PlotIdentifier", "PlotDesignation", currentPlot);
            ViewBag.MemberOptions = new SelectList(members, "MemberId", "FullLegalName", currentMember);
        }
    }
}
