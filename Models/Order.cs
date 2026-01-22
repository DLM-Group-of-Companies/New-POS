using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using NLI_POS.Models.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static NLI_POS.Pages.Orders.NewModel;

namespace NLI_POS.Models
{
    public class Order : AuditableEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Order No.")]
        public string OrderNo { get; set; }

        [Display(Name = "Order Date")]
        [DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime OrderDate { get; set; } 

        public int? ItemNo { get; set; }

        [ForeignKey("Customers")]
        [Display(Name = "Customer")]
        public int CustomerId { get; set; }

        [Display(Name = "Office")]
        [ForeignKey("Office")]
        public int OfficeId { get; set; }

        [Display(Name = "Order Type")]
        [StringLength(40)]
        public string OrderType { get; set; }

        public decimal TotAmount { get; set; } //Total Amount of Transaction
        public decimal TotPaidAmount { get; set; } //Total payment of Customer. Maybe higher than TotAmount
        
        [StringLength(500)]
        public string? Notes { get; set; }

        [StringLength(30)]
        [Required]
        public string SalesBy { get; set; } = "";

        [StringLength(50)]
        [Display(Name = "Sales Source")]
        public string? SalesSource { get; set; } = "";

        public bool IsVoided { get; set; }
        public DateTime? VoidedDate { get; set; }
        [StringLength(30)]
        public string? VoidedBy { get; set; }
        [StringLength(300)]
        public string? VoidReason { get; set; }

        public virtual ICollection<ProductItem> ProductItems { get; set; } = new List<ProductItem>();

        [ValidateNever]
        public virtual Customer Customers { get; set; }
        [ValidateNever]
        public virtual OfficeCountry Office { get; set; }
        [ValidateNever]
        public virtual ICollection<OrderDetails> OrderDetails { get; set; }

        public virtual ICollection<OrderPayment> Payments { get; set; }
    }
}
