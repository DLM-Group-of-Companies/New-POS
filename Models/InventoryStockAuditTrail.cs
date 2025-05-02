using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NLI_POS.Models
{
    public class InventoryStockAuditTrail
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("Products")]
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public string TransactionType { get; set; }    
        public string OrderNo { get; set; }
        [ForeignKey("Office")]
        public int OfficeId { get; set; }

        public DateTime TransactionDate { get; set; }
        public string EncodedBy { get; set; }

        public virtual Product Products { get; set; }
        public virtual OfficeCountry Office { get; set; }
    }
}
