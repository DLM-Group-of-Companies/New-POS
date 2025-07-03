using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NLI_POS.Models
{
    public class ProductPrice
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Product")]
        [Display(Name = "Product")]
        public int ProductId { get; set; }

        [ForeignKey("Country")]
        [Display(Name = "Country")]
        public int CountryId { get; set; }

        [Display(Name = "Unit Price")]
        public decimal UnitCost { get; set; }=decimal.Zero;
        [Display(Name = "Regular")]
        public decimal RegPrice { get; set; } = decimal.Zero;
        [Display(Name = "Standard Dist")]
        public decimal DistPrice { get; set; } = decimal.Zero;
        [Display(Name = "Staff")]
        public decimal StaffPrice { get; set; } = decimal.Zero;
        [Display(Name = "BPP")]
        public decimal BPPPrice { get; set; } = decimal.Zero;
        [Display(Name = "Med Pack")]
        public decimal MedPackPrice { get; set; } = decimal.Zero;
        [Display(Name = "Corp Acc")]
        public decimal CorpAccPrice { get; set; } = decimal.Zero;
        [Display(Name = "Naturopath")]
        public decimal NaturoPrice { get; set; } = decimal.Zero;

        public DateTime? EncodeDate { get; set; } 
        public string? EncodedBy { get; set; } 
        public DateTime? UpdateDate { get; set; }
        public string? UpdateddBy { get; set; }

        [ValidateNever]
        public virtual Product? Product { get; set; }
        [ValidateNever]
        public virtual Country? Country { get; set; }

    }
}
