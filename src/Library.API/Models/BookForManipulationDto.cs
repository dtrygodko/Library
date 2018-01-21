using System.ComponentModel.DataAnnotations;

namespace Library.API.Models
{
    public abstract class BookForManipulationDto
    {
        [Required(ErrorMessage = "Fill the title.")]
        [MaxLength(100, ErrorMessage = "Title shouln't have more that 100 characters.")]
        public string Title { get; set; }

        [MaxLength(500, ErrorMessage = "Description shouln't have more that 500 characters.")]
        public virtual string Description { get; set; }
    }
}