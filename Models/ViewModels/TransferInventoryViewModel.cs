using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace NLI_POS.Models.ViewModels
{
    public class TransferInventoryViewModel
    {
        [Required]
        public int FromLocationId { get; set; }

        [Required]
        public int ToLocationId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }

        public List<SelectListItem> LocationOptions { get; set; } = new();
        public List<SelectListItem> ProductOptions { get; set; } = new();

        public bool LockFrom { get; set; } // Used to disable FromLocation select
    }

}
