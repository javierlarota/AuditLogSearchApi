using System;
using System.ComponentModel.DataAnnotations;

namespace AuditLogSearchApi.DTOs
{
    /// <summary>
    /// Search request model similar to AWS OpenSearch API
    /// </summary>
    public class SearchRequest
    {
        /// <summary>
        /// Search query string. Supports:
        /// - Simple text: "login failed"
        /// - AND operator: "login AND failed"
        /// - OR operator: "login OR logout"
        /// - Column-specific: "user_name:John AND action:LOGIN"
        /// </summary>
        [Required]
        public string Query { get; set; } = string.Empty;

        /// <summary>
        /// Page number (1-based)
        /// </summary>
        [Range(1, int.MaxValue)]
        public int From { get; set; } = 1;

        /// <summary>
        /// Number of results per page
        /// </summary>
        [Range(1, 1000)]
        public int Size { get; set; } = 10;

        /// <summary>
        /// Sort field (timestamp, user_id, action, status, etc.)
        /// </summary>
        public string? Sort { get; set; }

        /// <summary>
        /// Sort order: true for descending, false for ascending
        /// </summary>
        public bool SortDescending { get; set; } = true;

        /// <summary>
        /// Filter by start date (ISO 8601 format)
        /// </summary>
        public DateTime? FromDate { get; set; }

        /// <summary>
        /// Filter by end date (ISO 8601 format)
        /// </summary>
        public DateTime? ToDate { get; set; }
    }
}
