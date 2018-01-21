using System.ComponentModel.DataAnnotations;

namespace Library.API.Models
{
    public class UpdateBookDto : BookForManipulationDto
    {
        [Required(ErrorMessage = "Fill the description.")]
        public override string Description { get; set; }
    }
}