using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using NLI_POS.Models.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NLI_POS.Models
{
    public class Customer : AuditableEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Customer Code")]
        public string CustCode { get; set; }

        [Required]
        [ForeignKey("CustClasses")]
        [Display(Name = "Customer Class")]
        public int CustClass { get; set; }

        [Required]
        [StringLength(200)]
        public string FirstName { get; set; }
 
        [StringLength(200)]
        public string? MiddleName { get; set; }

        [Required]
        [StringLength(200)]
        public string LastName { get; set; }

        [StringLength(200)]
        public string? Email { get; set; }

        [StringLength(20)]
        [Display(Name = "Mobile Number")]
        public string? MobileNo { get; set; }

        [StringLength(20)]
        [Display(Name = "Landline Number")]
        public string? LandlineNo { get; set; }

        [Required]
        public string Gender { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Birthday { get; set; }

        [Display(Name = "Civil Status")]
        public string? CivilStat { get; set; }

        [StringLength(200)]
        public string? Address1 { get; set; }

        [StringLength(200)]
        public string? Address2 { get; set; }

        public string? Province { get; set; }
        public string? City { get; set; }

        public string? Country { get; set; }
        public string? PostalCode { get; set; }

        [Display(Name = "Office")]
        [ForeignKey("OfficeCountry")]        
        public int OfficeId { get; set; }

        [StringLength(30)]
        public string? Nationality { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        //public DateTime EncodeDate { get; set; }
        //public string EncodedBy { get; set; }
        //public DateTime? UpdateDate { get; set; }
        //public string? UpdateddBy { get; set; }

        [ValidateNever]
        [Display(Name = "Office")]
        public virtual OfficeCountry OfficeCountry { get; set; }
        [ValidateNever]
        [Display(Name = "Customer Class")]
        public virtual CustClass CustClasses { get; set; }
  
    }
}
