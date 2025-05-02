using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NLI_POS.Models
{
    public class InventoryStock
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Products")]
        [Display(Name = "Product")]
        public int ProductId { get; set; }

        [Display(Name = "Stock")]
        public int StockQty { get; set; }
        
        [ForeignKey("Office")]
        [Display(Name = "Office")]
        public int OfficeId { get; set; }

        public virtual Product Products { get; set; }
        public virtual OfficeCountry Office { get; set; }

    }
}
