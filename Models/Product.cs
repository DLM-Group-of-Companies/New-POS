using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NLI_POS.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Product Code")]
        [StringLength(10)]
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
        public int Quantity { get; set; }
        public bool IsActive { get; set; }

        public DateTime EncodeDate { get; set; }
        public string EncodedBy { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string? UpdateddBy { get; set; }

        public virtual ProductType ProductTypes { get; set; }
    }
}
