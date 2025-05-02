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
        public int ProductId { get; set; }
        public int Quantity { get; set; }

        public virtual Product Products { get; set; }
    }
}
