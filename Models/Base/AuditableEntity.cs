using System.ComponentModel.DataAnnotations;

namespace NLI_POS.Models.Base
{
    public abstract class AuditableEntity
    {
        public DateTime? EncodeDate { get; set; }
        [StringLength(30)]
        public string? EncodedBy { get; set; }        
        public DateTime? UpdateDate { get; set; }
        [StringLength(30)]
        public string? UpdatedBy { get; set; }
    }
}
