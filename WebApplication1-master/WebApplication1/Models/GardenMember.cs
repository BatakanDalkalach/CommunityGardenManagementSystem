using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class GardenMember
    {
        public int MemberId { get; set; }

        [Required(ErrorMessage = "Member's full name is required")]
        [StringLength(80, MinimumLength = 3, ErrorMessage = "Name length: 3-80 characters")]
        [Display(Name = "Full Name")]
        public string FullLegalName { get; set; } = string.Empty;

        [Required]
        [EmailAddress(ErrorMessage = "Provide a valid email")]
        [Display(Name = "Email Contact")]
        public string EmailContact { get; set; } = string.Empty;

        [Required]
        [StringLength(50, ErrorMessage = "Membership tier cannot exceed 50 characters")]
        [Display(Name = "Membership Type")]
        public string MembershipTier { get; set; } = "Basic";

        [Display(Name = "Join Date")]
        [DataType(DataType.Date)]
        public DateTime RegistrationDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Years of experience is required")]
        [Range(0, 50, ErrorMessage = "Experience: 0-50 years")]
        [Display(Name = "Gardening Experience (years)")]
        public int YearsOfExperience { get; set; }

        [Display(Name = "Prefers Organic Methods")]
        public bool PreferOrganicOnly { get; set; } = true;

        [StringLength(300)]
        [Display(Name = "Special Interests")]
        public string? GardeningInterests { get; set; }

        public virtual ICollection<GardenPlot>? ManagedPlots { get; set; }
        public virtual ICollection<HarvestRecord>? RecordedHarvests { get; set; }
    }
}
