using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApplication1.DatabaseContext;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    public class PlotsController : Controller
    {
        private readonly PlotManagementService _svc;
        private readonly CommunityGardenDatabase _ctx;

        public PlotsController(PlotManagementService svc, CommunityGardenDatabase ctx)
        {
            _svc = svc;
            _ctx = ctx;
        }

        public async Task<IActionResult> Index(string? search = null, string? soilType = null, int page = 1)
        {
            const int pageSize = 6;
            var allPlots = await _svc.RetrieveAllPlotsAsync();

            if (!string.IsNullOrWhiteSpace(search))
                allPlots = allPlots.Where(p => p.PlotDesignation.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
            if (!string.IsNullOrWhiteSpace(soilType))
                allPlots = allPlots.Where(p => p.SoilType.Contains(soilType, StringComparison.OrdinalIgnoreCase)).ToList();

            var totalPages = (int)Math.Ceiling(allPlots.Count / (double)pageSize);
            page = Math.Max(1, Math.Min(page, Math.Max(1, totalPages)));

            ViewBag.TotalCount = allPlots.Count;
            ViewBag.FreeCount = allPlots.Count(p => !p.IsOccupied);
            ViewBag.OccupiedCount = allPlots.Count(p => p.IsOccupied);
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.SearchDesignation = search;
            ViewBag.SearchSoilType = soilType;

            return View(allPlots.Skip((page - 1) * pageSize).Take(pageSize).ToList());
        }

        public async Task<IActionResult> ViewDetails(int? id)
        {
            if (!id.HasValue) return NotFound();
            
            var entity = await _svc.FindPlotByIdentifierAsync(id.Value);
            return entity == null ? NotFound() : View(entity);
        }

        // Require login to add, modify, or remove plots
        // Изисква вход за добавяне, промяна или премахване на парцели
        [Authorize]
        public IActionResult AddNew()
        {
            LoadMemberOptions();
            return View();
        }

        [Authorize]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddNew(GardenPlot entity)
        {
            if (!ModelState.IsValid)
            {
                LoadMemberOptions(entity.AssignedGardenerId);
                return View(entity);
            }

            await _svc.RegisterNewPlotAsync(entity);
            TempData["SuccessMsg"] = "Plot successfully added!";
            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        public async Task<IActionResult> Modify(int? id)
        {
            if (!id.HasValue) return NotFound();
            
            var entity = await _svc.FindPlotByIdentifierAsync(id.Value);
            if (entity == null) return NotFound();
            
            LoadMemberOptions(entity.AssignedGardenerId);
            return View(entity);
        }

        [Authorize]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Modify(int id, GardenPlot entity)
        {
            if (id != entity.PlotIdentifier) return NotFound();

            if (!ModelState.IsValid)
            {
                LoadMemberOptions(entity.AssignedGardenerId);
                return View(entity);
            }

            try
            {
                await _svc.ModifyPlotDetailsAsync(entity);
                TempData["SuccessMsg"] = "Plot updated successfully!";
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        public async Task<IActionResult> Remove(int? id)
        {
            if (!id.HasValue) return NotFound();
            
            var entity = await _svc.FindPlotByIdentifierAsync(id.Value);
            return entity == null ? NotFound() : View(entity);
        }

        [Authorize]
        [HttpPost, ActionName("Remove"), ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmRemoval(int id)
        {
            await _svc.RemovePlotAsync(id);
            TempData["SuccessMsg"] = "Plot removed successfully!";
            return RedirectToAction(nameof(Index));
        }

        private void LoadMemberOptions(object? current = null)
        {
            var items = _ctx.GardenMembers.OrderBy(x => x.FullLegalName).ToListAsync().Result;
            ViewBag.MemberOptions = new SelectList(items, "MemberId", "FullLegalName", current);
        }
    }
}
