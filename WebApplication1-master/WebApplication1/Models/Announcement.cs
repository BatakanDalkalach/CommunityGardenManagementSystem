using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    // Represents a public announcement posted by admins to garden members.
    // Представлява публично съобщение, публикувано от администраторите за членовете на градината.
    public class Announcement
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 200 characters")]
        [Display(Name = "Title")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Content is required")]
        [StringLength(5000, MinimumLength = 10, ErrorMessage = "Content must be between 10 and 5000 characters")]
        [Display(Name = "Content")]
        [DataType(DataType.MultilineText)]
        public string Content { get; set; } = string.Empty;

        [Display(Name = "Created At")]
        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Published")]
        public bool IsPublished { get; set; } = false;
    }
}
