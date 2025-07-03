using System.ComponentModel.DataAnnotations;

namespace NLI_POS.Models
{
    public class PaymentMethods
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
