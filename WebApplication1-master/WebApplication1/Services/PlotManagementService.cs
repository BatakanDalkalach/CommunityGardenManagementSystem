using Microsoft.EntityFrameworkCore;
using WebApplication1.DatabaseContext;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public class PlotManagementService
    {
        private readonly CommunityGardenDatabase _database;

        public PlotManagementService(CommunityGardenDatabase database)
        {
            _database = database;
        }

        /// <summary>
        /// Retrieves all garden plots ordered alphabetically by their designation code,
        /// including the current tenant navigation property.
        /// </summary>
        /// <returns>A list of all <see cref="GardenPlot"/> entities.</returns>
        public async Task<List<GardenPlot>> RetrieveAllPlotsAsync()
        {
            return await _database.GardenPlots
                .Include(plot => plot.CurrentTenant)
                .OrderBy(plot => plot.PlotDesignation)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves a single garden plot by its primary key, including the current tenant.
        /// Returns <c>null</c> if no plot with the given identifier exists.
        /// </summary>
        /// <param name="identifier">The primary key of the garden plot.</param>
        /// <returns>The matching <see cref="GardenPlot"/>, or <c>null</c> if not found.</returns>
        public async Task<GardenPlot?> FindPlotByIdentifierAsync(int identifier)
        {
            return await _database.GardenPlots
                .Include(plot => plot.CurrentTenant)
                .FirstOrDefaultAsync(plot => plot.PlotIdentifier == identifier);
        }

        /// <summary>
        /// Persists a new garden plot to the database.
        /// </summary>
        /// <param name="plot">The <see cref="GardenPlot"/> entity to insert.</param>
        /// <returns>The inserted plot with its generated primary key populated.</returns>
        public async Task<GardenPlot> RegisterNewPlotAsync(GardenPlot plot)
        {
            _database.GardenPlots.Add(plot);
            await _database.SaveChangesAsync();
            return plot;
        }

        /// <summary>
        /// Updates all fields of an existing garden plot.
        /// Throws <see cref="InvalidOperationException"/> if the plot no longer exists
        /// when a concurrency conflict is detected.
        /// </summary>
        /// <param name="plot">The <see cref="GardenPlot"/> entity with updated values.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when a concurrency conflict occurs and the plot cannot be found.
        /// </exception>
        /// <exception cref="DbUpdateConcurrencyException">
        /// Re-thrown when a concurrency conflict occurs and the plot still exists.
        /// </exception>
        public async Task ModifyPlotDetailsAsync(GardenPlot plot)
        {
            _database.Entry(plot).State = EntityState.Modified;
            try
            {
                await _database.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await CheckPlotExistsAsync(plot.PlotIdentifier))
                {
                    throw new InvalidOperationException("Plot not found");
                }
                throw;
            }
        }

        /// <summary>
        /// Removes the garden plot with the specified identifier from the database.
        /// Does nothing if no plot with that identifier exists.
        /// </summary>
        /// <param name="identifier">The primary key of the plot to delete.</param>
        public async Task RemovePlotAsync(int identifier)
        {
            var plotToRemove = await _database.GardenPlots.FindAsync(identifier);
            if (plotToRemove != null)
            {
                _database.GardenPlots.Remove(plotToRemove);
                await _database.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Checks whether a garden plot with the given identifier exists in the database.
        /// </summary>
        /// <param name="identifier">The primary key to check.</param>
        /// <returns><c>true</c> if the plot exists; otherwise <c>false</c>.</returns>
        public async Task<bool> CheckPlotExistsAsync(int identifier)
        {
            return await _database.GardenPlots.AnyAsync(p => p.PlotIdentifier == identifier);
        }

        /// <summary>
        /// Retrieves all unoccupied garden plots, ordered alphabetically by designation code.
        /// </summary>
        /// <returns>A list of <see cref="GardenPlot"/> entities where <c>IsOccupied</c> is <c>false</c>.</returns>
        public async Task<List<GardenPlot>> GetVacantPlotsAsync()
        {
            return await _database.GardenPlots
                .Where(plot => !plot.IsOccupied)
                .OrderBy(plot => plot.PlotDesignation)
                .ToListAsync();
        }
    }
}
