using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NLI_POS.Models
{
    //Product List for Combo or Promo
    public class ProductCombo
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Products")]
        public int ProductId { get; set; } //Main Product 
        public string ProductIdList { get; set; } //Product Combination
        public string ProductsDesc { get; set; }
        public string QuantityList { get; set; }

        public virtual Product Products { get; set; }
    }
}
