using System.ComponentModel.DataAnnotations;

namespace CourseLibrary.API.Models
{
    public class CourseForManipulationDto
    {
        [Required(ErrorMessage = "You should fill in the title")]
        [MaxLength(100, ErrorMessage = "The maximum length for the title is 100 characters")]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1500, ErrorMessage = "The maximum length for the description is 500 characters")]
        public virtual string Description { get; set; } = string.Empty;
    }
}
