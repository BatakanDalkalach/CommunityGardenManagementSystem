namespace WebApplication1.Models
{
    public class ProfileViewModel
    {
        public ApplicationUser User { get; set; } = null!;
        public GardenMember? LinkedMember { get; set; }
    }
}
