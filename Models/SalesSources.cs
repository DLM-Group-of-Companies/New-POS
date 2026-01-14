using NLI_POS.Models.Base;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace NLI_POS.Models
{
    public class SalesSources : AuditableEntity
    {
        [Key]
        public int id { get; set; }
        [DisplayName("Source Name")]
        public string Name { get; set; }
        [StringLength(200)]
        [DisplayName("Description")]
        public string Description { get; set; }
        [DisplayName("Active")]
        public bool isActive { get; set; }
    }
}
