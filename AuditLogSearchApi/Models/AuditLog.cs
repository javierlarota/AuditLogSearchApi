using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace AuditLogSearchApi.Models
{
    [Table("audit_logs")]
    public class AuditLog
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Required]
        [Column("timestamp")]
        public DateTime Timestamp { get; set; }

        [Required]
        [Column("user_id")]
        [MaxLength(100)]
        public string UserId { get; set; } = string.Empty;

        [Column("user_name")]
        [MaxLength(255)]
        public string? UserName { get; set; }

        [Required]
        [Column("action")]
        [MaxLength(100)]
        public string Action { get; set; } = string.Empty;

        [Required]
        [Column("resource_type")]
        [MaxLength(100)]
        public string ResourceType { get; set; } = string.Empty;

        [Column("resource_id")]
        [MaxLength(255)]
        public string? ResourceId { get; set; }

        [Column("ip_address")]
        public string? IpAddress { get; set; }

        [Required]
        [Column("status")]
        [MaxLength(50)]
        public string Status { get; set; } = string.Empty;

        [Column("details")]
        public string? Details { get; set; }

        [Column("metadata", TypeName = "jsonb")]
        public string? Metadata { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        // Not mapped - used for search ranking
        [NotMapped]
        public double? Rank { get; set; }
    }
}
