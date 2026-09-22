
using System.ComponentModel.DataAnnotations;

namespace TechBox.Models
{
    public class SCCMAction
    {
        public int Id { get; set; }

        [Required]
        public required string Name { get; set; }

        [Required]
        public required string ClientAction { get; set; }

        [Required]
        public bool IsEnabled { get; set; } = false;
    }
}
