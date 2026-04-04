namespace WebApplication1.Models
{
    public class PlotStatisticsViewModel
    {
        public int TotalPlots { get; set; }
        public int OccupiedCount { get; set; }
        public int FreeCount { get; set; }
        public double AveragePlotSize { get; set; }
        public string MostCommonSoilType { get; set; } = "N/A";
        public decimal TotalRentalIncome { get; set; }
        public Dictionary<string, int> PlotsBySoilType { get; set; } = new();
    }
}
