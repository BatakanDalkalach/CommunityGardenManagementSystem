using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.DatabaseContext;

namespace WebApplication1.Controllers
{
    [Route("api/harvest")]
    [ApiController]
    public class HarvestApiController : ControllerBase
    {
        private readonly CommunityGardenDatabase _db;

        public HarvestApiController(CommunityGardenDatabase db)
        {
            _db = db;
        }

        // GET /api/harvest/latest
        // Returns the 5 most recent harvest records with plot and member info.
        [HttpGet("latest")]
        public async Task<IActionResult> GetLatestHarvests()
        {
            var records = await _db.HarvestRecords
                .Include(h => h.SourcePlot)
                .Include(h => h.Harvester)
                .OrderByDescending(h => h.CollectionDate)
                .Take(5)
                .Select(h => new
                {
                    h.RecordId,
                    h.CropName,
                    h.QuantityKilograms,
                    CollectionDate = h.CollectionDate.ToString("MMM dd, yyyy"),
                    h.QualityScore,
                    h.IsOrganicCertified,
                    PlotDesignation = h.SourcePlot != null ? h.SourcePlot.PlotDesignation : "N/A",
                    HarvesterName = h.Harvester != null ? h.Harvester.FullLegalName : "Unknown"
                })
                .ToListAsync();

            return Ok(records);
        }

        // GET /api/harvest/plot-availability
        // Returns the total, available, and occupied plot counts.
        [HttpGet("plot-availability")]
        public async Task<IActionResult> GetPlotAvailability()
        {
            var total = await _db.GardenPlots.CountAsync();
            var available = await _db.GardenPlots.CountAsync(p => !p.IsOccupied);

            return Ok(new
            {
                total,
                available,
                occupied = total - available
            });
        }
    }
}
