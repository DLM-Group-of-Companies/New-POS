using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace NLI_POS.Models
{
    public class InventoryTransaction
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Product")]
        public int ProductId { get; set; }

        public int Quantity { get; set; }

        // Can be "Purchase", "Transfer", "Sale", "Adjustment"
        [Column(TypeName = "varchar(30)")]
        [StringLength(30)]
        public string TransactionType { get; set; }

        [Column(TypeName = "varchar(50)")]
        [StringLength(50)]
        public string? OrderNo { get; set; }

        [ForeignKey("SourceLocation")]
        public int? FromLocationId { get; set; } // Nullable for initial purchases

        [ForeignKey("DestinationLocation")]
        public int? ToLocationId { get; set; } // Nullable for consumption/sale

        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "varchar(255)")]
        [StringLength(255)]
        public string? Remarks { get; set; }

        [Column(TypeName = "varchar(20)")]
        [StringLength(20)]
        public string EncodedBy { get; set; }

        public virtual Product Product { get; set; }
        public virtual InventoryLocation SourceLocation { get; set; }
        public virtual InventoryLocation DestinationLocation { get; set; }
    }

}
