using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechBox.Models
{
	public class Setting
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[Required] 
		public required string Name { get; set; }
		public string? Value { get; set; } = string.Empty;
	}
}
