using System.ComponentModel.DataAnnotations;

namespace NLI_POS.Models
{
    public class OrderPayment
    {
        [Key]
        public int Id { get; set; }
        public int OrderId { get; set; }

        public string PayMethod { get; set; }     // e.g., "Cash", "GCash", "Credit"
        public decimal Amount { get; set; }
        public string? ReferenceNo { get; set; }

        public virtual Order Order { get; set; }
    }

}
