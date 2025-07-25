using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace NLI_POS.Models
{
    public class InventoryLocation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } // e.g. "Main Warehouse", "Stockroom", "Cebu Office"

        [Required]
        public string LocationType { get; set; } // "Warehouse", "Stockroom", "Office"

        public int? OfficeId { get; set; }

        public bool IsActive { get; set; } = true;

        [ValidateNever]
        public virtual OfficeCountry? Office { get; set; }

        public virtual ICollection<InventoryStock> Stocks { get; set; }
    }

}
