using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NLI_POS.Models
{
    public class OfficeCountry
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        [Required]
        [StringLength(3)]
        public string OffCode { get; set; }

        [StringLength(200)]
        public string? Address { get; set; }

        [StringLength(200)]
        public string? Email { get; set; }
        [StringLength(200)]
        [Display(Name = "Contact Number")]
        public string? ContactNo { get; set; }

        [StringLength(500)]
        public string? Remarks { get; set; }
        [Display(Name = "Active")]
        public bool isActive { get; set; } = false;

        [ForeignKey("Country")]
        [Required]
        [Display(Name = "Country")]
        public int CountryId { get; set; }

        public DateTime EncodeDate { get; set; }
        public string EncodedBy { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string? UpdateddBy { get; set; }

        public virtual Country Country { get; set; }
    }
}
