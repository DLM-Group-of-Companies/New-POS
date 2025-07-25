using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using NLI_POS.Models.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NLI_POS.Models
{
    public class InventoryStock : AuditableEntity
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Product")]
        public int ProductId { get; set; }

        [ForeignKey("Location")]
        public int LocationId { get; set; }

        public int StockQty { get; set; }

        [StringLength(300)]
        public string? Remarks { get; set; }

        [ValidateNever]
        public virtual Product Product { get; set; }
        [ValidateNever]
        public virtual InventoryLocation Location { get; set; }
    }

}
