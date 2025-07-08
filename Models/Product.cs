using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using NLI_POS.Models.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NLI_POS.Models
{
    public class Product: AuditableEntity
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Product Code")]
        [StringLength(20)]
        public string ProductCode { get; set; }

        [Display(Name = "Product Name")]
        [StringLength(100)]
        public string ProductName { get; set; }

        [Display(Name = "Product Description")]
        [StringLength(200)]
        public string ProductDescription { get; set; } = string.Empty;

        [Display(Name = "Product Type")]
        [StringLength(20)]
        [Column(TypeName = "varchar(20)")]
        public string? ProductType { get; set; }

        [Display(Name = "Category")]
        [StringLength(20)]
        public string ProductCategory { get; set; }

        [Display(Name = "Class")]
        [StringLength(10)]
        public string? ProductClass { get; set; } //Main or Collateral

        public string? SKU { get; set; }

        [Display(Name = "Freebie Available")]
        public bool isStaffAvailable { get; set; }
        [Display(Name = "Staff Available")]
        public bool isFreebieAvailable { get; set; }
        [Display(Name = "Active")]
        public bool IsActive { get; set; }


    }
}
