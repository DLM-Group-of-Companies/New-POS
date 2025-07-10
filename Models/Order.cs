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
        public string OrderType { get; set; }


        public decimal TotPaidAmount { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        public bool IsVoided { get; set; }
        public DateTime? VoidedDate { get; set; }
        public string? VoidedBy { get; set; }
        public string? VoidReason { get; set; }

        public virtual ICollection<ProductItem> ProductItems { get; set; } = new List<ProductItem>();

        [ValidateNever]
        public virtual Customer Customers { get; set; }
        [ValidateNever]
        public virtual OfficeCountry Office { get; set; }

        public virtual ICollection<OrderPayment> Payments { get; set; }
    }
}
