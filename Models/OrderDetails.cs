using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace NLI_POS.Models
{
    public class OrderDetails
    {
        public int Id { get; set; }
        public int OrderId { get; set; } //Liink to Orders 

        [ForeignKey("OrderId")]
        [ValidateNever]
        public virtual Order Order { get; set; }

        [Display(Name = "Product Category")]
        [StringLength(20)]
        public string? ProductCategory { get; set; }
        
        [Display(Name = "Product")]
        public int? ProductId { get; set; }

        [Display(Name = "Product Combination")]        
        public int? ComboId { get; set; }

        public int Quantity { get; set; }

        public decimal Price { get; set; }
        public decimal TotalPrice => Quantity * Price;

        [ValidateNever]
        [ForeignKey("ProductId")]
        public virtual Product Products { get; set; }

        [ValidateNever]
        [ForeignKey("ComboId")]
        public virtual ProductCombo ProductCombos { get; set; }
    }
}
