using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace NLI_POS.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        [Display(Name ="Full Name")]
        public string FullName { get; set; } = "";

        [StringLength(100)]
        public string Designation { get; set; } = "";

        [Display(Name = "Office")]
        [ForeignKey("OfficeCountry")]
        public int OfficeId { get; set; }

        public virtual OfficeCountry OfficeCountry { get; set; }
    }
}
