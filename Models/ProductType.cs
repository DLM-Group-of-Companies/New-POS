using System.ComponentModel.DataAnnotations;

namespace NLI_POS.Models
{
    public class ProductType
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(50)]
        public string Name { get; set; }
        [StringLength(100)]
        public string Description { get; set; }
        public bool IsActive { get; set; }

        public DateTime EncodeDate { get; set; }
        public string EncodedBy { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string? UpdateddBy { get; set; }
    }
}
