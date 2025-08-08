using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NLI_POS.Models
{
    public class SalesQuota
    {
        [Key]
        public int Id { get; set; }

        public string SalesPersonId { get; set; }

        [ForeignKey(nameof(SalesPersonId))]
        public ApplicationUser SalesPerson { get; set; }

        public string SalesPersonUserName { get; set; }

        [DataType(DataType.Date)]
        public DateTime QuotaDate { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal QuotaAmount { get; set; }
    }
}
