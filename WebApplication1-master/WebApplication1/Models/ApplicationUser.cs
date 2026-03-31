using Microsoft.AspNetCore.Identity;

namespace WebApplication1.Models
{
    // ApplicationUser extends IdentityUser to support custom profile fields.
    // ApplicationUser разширява IdentityUser за поддръжка на персонализирани полета.
    public class ApplicationUser : IdentityUser
    {
        // Optional display name for the user
        // Незадължително показвано име на потребителя
        public string? FullName { get; set; }
    }
}
