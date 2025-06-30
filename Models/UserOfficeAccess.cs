using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace NLI_POS.Models
{
    public class UserOfficeAccess
    {
        [Required]
        public int Id { get; set; }

        [Display(Name = "User")]
        [ForeignKey("User")]
        public string UserId { get; set; }

        [Display(Name = "Office")]
        [ForeignKey("OfficeCountry")]
        public int OfficeId {  get; set; }

        public DateTime EncodeDate { get; set; }
        public string EncodedBy { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string? UpdateddBy { get; set; }

        [ValidateNever]
        public virtual ApplicationUser User { get; set; }
        [ValidateNever]
        public virtual OfficeCountry OfficeCountry { get; set; }
    }
}
