using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NLI_POS.Models
{
    public class Product
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

        [ForeignKey("ProductTypes")]
        [Display(Name = "Product Type")]
        public int ProducTypeId { get; set; }

        [Display(Name = "Category")]
        [StringLength(20)]
        public string ProductCategory { get; set; }

        public string SKU { get; set; }

        [Display(Name = "Unit Cost")]
        public decimal UnitCost { get; set; }
        [Display(Name = "Regular Price")]
        public decimal RegPrice { get; set; }
        [Display(Name = "Member Price")]
        public decimal MemPrice { get; set; }
        [Display(Name = "Staff Price")]
        public decimal StaffPrice { get; set; }

        public bool IsActive { get; set; }

        public DateTime EncodeDate { get; set; }
        public string EncodedBy { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string? UpdateddBy { get; set; }

        public virtual ProductType ProductTypes { get; set; }
    }
}
