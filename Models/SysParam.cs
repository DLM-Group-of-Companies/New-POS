using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NLI_POS.Models
{
    public class SysParam
    {
        [Key]
        public int Id { get; set; }
        public string Background { get; set; }
        public string Theme { get; set; }
    }
}
