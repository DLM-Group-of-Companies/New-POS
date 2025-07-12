using NLI_POS.Models.Base;
using System.ComponentModel.DataAnnotations;

namespace NLI_POS.Models
{
    public class ProductType : AuditableEntity
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(50)]
        public string Name { get; set; }
        [StringLength(100)]
        public string Description { get; set; }
        public bool IsActive { get; set; }

    }
}
