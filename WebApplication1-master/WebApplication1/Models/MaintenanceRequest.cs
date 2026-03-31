using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    // Represents a maintenance request submitted for a specific garden plot.
    // Представлява заявка за поддръжка, подадена за конкретен градински парцел.
    public class MaintenanceRequest
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Plot is required")]
        [Display(Name = "Plot")]
        public int PlotId { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 1000 characters")]
        [Display(Name = "Description")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Request Date")]
        [DataType(DataType.Date)]
        public DateTime RequestDate { get; set; } = DateTime.Today;

        [Display(Name = "Status")]
        public MaintenanceStatus Status { get; set; } = MaintenanceStatus.Pending;

        // Navigation property
        // Навигационно свойство
        [ForeignKey(nameof(PlotId))]
        public virtual GardenPlot? Plot { get; set; }
    }
}
