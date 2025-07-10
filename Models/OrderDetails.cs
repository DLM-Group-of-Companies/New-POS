using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace NLI_POS.Models
{
    public class OrderDetails
    {
        public int Id { get; set; }
        public int OrderId { get; set; } //Liink to Orders 

        [Display(Name = "Product Category")]
        [StringLength(20)]
        public string? ProductCategory { get; set; }

        [ForeignKey("Products")]
        [Display(Name = "Product")]
        public int? ProductId { get; set; }

        [Display(Name = "Product Combination")]
        [ForeignKey("ProductCombos")]
        public int? ComboId { get; set; }

        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal TotalPrice => Quantity * Price;

        [ValidateNever]
        public virtual Product Products { get; set; }
        [ValidateNever]
        public virtual ProductCombo ProductCombos { get; set; }
    }
}
