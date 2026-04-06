using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.DatabaseContext;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private readonly CommunityGardenDatabase _db;

        public HomeController(CommunityGardenDatabase db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var total = await _db.GardenPlots.CountAsync();
            var available = await _db.GardenPlots.CountAsync(p => !p.IsOccupied);

            ViewBag.TotalPlots = total;
            ViewBag.AvailablePlots = available;
            ViewBag.OccupiedPlots = total - available;

            var latestHarvests = await _db.HarvestRecords
                .Include(h => h.SourcePlot)
                .Include(h => h.Harvester)
                .OrderByDescending(h => h.CollectionDate)
                .Take(5)
                .ToListAsync();

            ViewBag.LatestHarvests = latestHarvests;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
