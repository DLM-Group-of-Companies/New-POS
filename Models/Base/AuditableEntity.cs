namespace NLI_POS.Models.Base
{
    public abstract class AuditableEntity
    {
        public DateTime? EncodeDate { get; set; }
        public string? EncodedBy { get; set; }

        public DateTime? UpdateDate { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
