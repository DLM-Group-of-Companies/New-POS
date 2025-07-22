using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace NLI_POS.Models
{
    public class PaymentMethod
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
        public string? Description { get; set; }
        [Precision(10, 4)]
        public decimal? ServiceCharge { get; set; } 
        public bool IsActive { get; set; } = true;
    }
}
