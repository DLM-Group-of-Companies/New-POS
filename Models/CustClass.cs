using NLI_POS.Models.Base;
using System.ComponentModel.DataAnnotations;

namespace NLI_POS.Models
{
    public class CustClass: AuditableEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
