using System.ComponentModel.DataAnnotations;

namespace NLI_POS.Models
{
    public class Country
    {
        [Key]
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; } 
        
    }
}
