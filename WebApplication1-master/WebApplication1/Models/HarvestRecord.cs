using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class HarvestRecord
    {
        public int RecordId { get; set; }

        // EN: Primary key of the harvest record.
        // BG: Основен ключ на записа за реколта.
        [Required(ErrorMessage = "Plot selection is required")]
        public int PlotIdentifier { get; set; }

        // EN: Navigation property to the associated GardenPlot entity.
        // BG: Навигационно свойство към свързания обект GardenPlot.
        [ForeignKey(nameof(PlotIdentifier))]
        public virtual GardenPlot? SourcePlot { get; set; }

        [Required(ErrorMessage = "Member selection is required")]
        public int MemberId { get; set; }

        // EN: Foreign key referencing the member who harvested.
        // BG: Външен ключ към члена, който е събрал реколтата.
        [ForeignKey(nameof(MemberId))]
        public virtual GardenMember? Harvester { get; set; }
        
        // EN: Name of the harvested crop or vegetable.
        // Must be between 2 and 60 characters.
        // BG: Име на събраната култура или зеленчук.
        // Трябва да бъде между 2 и 60 символа.
        [Required(ErrorMessage = "Crop name is required")]
        [StringLength(60, MinimumLength = 2, ErrorMessage = "Crop name must be between 2 and 60 characters")]
        [Display(Name = "Crop/Vegetable Name")]
        public string CropName { get; set; } = string.Empty;

        // EN: Quantity of harvested crop in kilograms.
        // Allowed range: 0.1 to 1000 kg.
        // BG: Количество на реколтата в килограми.
        // Допустим диапазон: 0.1 до 1000 кг.
        [Required]
        [Range(0.1, 1000.0, ErrorMessage = "Quantity: 0.1-1000 kg")]
        [Display(Name = "Quantity Harvested (kg)")]
        public double QuantityKilograms { get; set; }

        [Required(ErrorMessage = "Harvest date is required")]
        [Display(Name = "Harvest Date")]
        [DataType(DataType.Date)]
        public DateTime CollectionDate { get; set; } = DateTime.Today;

        [Required]
        [Display(Name = "Quality Rating")]
        [Range(1, 5, ErrorMessage = "Rating: 1 (poor) to 5 (excellent)")]
        public int QualityScore { get; set; } = 3;

        [StringLength(400, ErrorMessage = "Notes cannot exceed 400 characters")]
        [Display(Name = "Observations")]
        public string? HarvestNotes { get; set; }

        [Display(Name = "Organic Certified")]
        public bool IsOrganicCertified { get; set; } = false;
    }
}
