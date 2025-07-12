using System.ComponentModel.DataAnnotations;

namespace NLI_POS.Models
{
    public class AuditLog
    {
        [Key]
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Activity { get; set; }
        public string Path { get; set; }
        public DateTime Timestamp { get; set; }
        public string? IP { get; set; }

        public ApplicationUser User { get; set; }
    }
}
