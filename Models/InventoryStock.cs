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

        [ForeignKey("Products")] // ✅ must match the navigation property name
        [Display(Name = "Product")]
        public int ProductId { get; set; }

        [Display(Name = "Stock")]
        public int StockQty { get; set; }

        [ForeignKey("Office")] // ✅ must match navigation property
        [Display(Name = "Office")]
        public int OfficeId { get; set; }

        [StringLength(300)]
        public string? Remarks { get; set; }

        //public DateTime? EncodeDate { get; set; }
        //public string? EncodedBy { get; set; }
        //public DateTime? UpdateDate { get; set; }
        //public string? UpdateddBy { get; set; }

        [ValidateNever]
        public virtual Product Products { get; set; } // ✅ singular

        [ValidateNever]
        public virtual OfficeCountry Office { get; set; }
    }

}
