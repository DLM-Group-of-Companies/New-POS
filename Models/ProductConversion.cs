using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace NLI_POS.Models
{
    namespace NLI_POS.Models
    {
        public class ProductConversion
        {
            [Key]
            public int Id { get; set; }

            public int FromProductId { get; set; } // Cleanse Box
            public int ToProductId { get; set; }   // Cleanse Sachet

            public int ConversionQty { get; set; } // 15

            [ValidateNever]
            public Product FromProduct { get; set; }
            [ValidateNever]
            public Product ToProduct { get; set; }
        }
    }

}
