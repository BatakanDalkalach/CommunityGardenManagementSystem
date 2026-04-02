using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class GardenPlot
    {
        public int PlotIdentifier { get; set; }

        [Required(ErrorMessage = "Plot designation is mandatory")]
        [StringLength(4, MinimumLength = 4, ErrorMessage = "Plot code must be exactly 4 characters (e.g., A001)")]
        [RegularExpression(@"^[A-Z]\d{3}$", ErrorMessage = "Format must be: Letter followed by 3 digits (e.g., A001)")]
        [Display(Name = "Plot Code")]
        public string PlotDesignation { get; set; } = string.Empty;

        [Required]
        [Range(5.0, 100.0, ErrorMessage = "Area must be 5-100 square meters")]
        [Display(Name = "Plot Area (sq m)")]
        public double SquareMeters { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "Soil type cannot exceed 50 characters")]
        [Display(Name = "Soil Quality")]
        public string SoilType { get; set; } = "Loamy";

        [Display(Name = "Has Water Access")]
        public bool WaterAccessAvailable { get; set; } = true;

        [Display(Name = "Currently Occupied")]
        public bool IsOccupied { get; set; } = false;

        [Required(ErrorMessage = "Annual fee is required")]
        [Range(0, 10000, ErrorMessage = "Fee must be between 0 and 10000")]
        [Display(Name = "Annual Fee")]
        public decimal YearlyRentalFee { get; set; }

        [Display(Name = "Last Maintenance")]
        [DataType(DataType.Date)]
        public DateTime LastMaintenanceDate { get; set; } = DateTime.Today;

        public int? AssignedGardenerId { get; set; }
        public virtual GardenMember? CurrentTenant { get; set; }

        public virtual ICollection<HarvestRecord>? HarvestHistory { get; set; }
    }
}
