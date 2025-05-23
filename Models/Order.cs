using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NLI_POS.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Order No.")]
        public string OrderNo { get; set; }

        [Display(Name = "Order Date")]
        [DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime OrderDate { get; set; } 
        public int ItemNo { get; set; }

        [ForeignKey("Customers")]
        [Display(Name = "Customer")]
        public int CustomerId { get; set; }

        [Display(Name = "Office")]
        [ForeignKey("Office")]
        public int OfficeId { get; set; }

        [Display(Name = "Order Type")]
        public string OrderType { get; set; }

        [Display(Name = "Product Category")]
        [StringLength(20)]
        public string ProductCategory { get; set; }

        [ForeignKey("Products")]
        [Display(Name = "Product")]
        public int ProductId { get; set; }

        [Display(Name = "Quantity")]
        public int Qty { get; set; }

        public decimal Amount { get; set; }

        public DateTime EncodeDate { get; set; } = DateTime.UtcNow.AddHours(8);
        public string EncodedBy { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string? UpdateddBy { get; set; }

        public virtual Product Products { get; set; }
        public virtual Customer Customers { get; set; }
        public virtual OfficeCountry Office { get; set; }

    }
}
