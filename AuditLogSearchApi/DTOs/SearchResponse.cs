using AuditLogSearchApi.Models;
using System.Collections.Generic;

namespace AuditLogSearchApi.DTOs
{
    /// <summary>
    /// Search response model similar to AWS OpenSearch API
    /// </summary>
    public class SearchResponse
    {
        /// <summary>
        /// Total number of matching documents
        /// </summary>
        public long Total { get; set; }

        /// <summary>
        /// Current page number
        /// </summary>
        public int From { get; set; }

        /// <summary>
        /// Number of results per page
        /// </summary>
        public int Size { get; set; }

        /// <summary>
        /// Search results (hits)
        /// </summary>
        public List<AuditLog> Hits { get; set; } = new List<AuditLog>();

        /// <summary>
        /// Time taken to execute the query in milliseconds
        /// </summary>
        public long Took { get; set; }

        /// <summary>
        /// Indicates if there are more results
        /// </summary>
        public bool HasMore => (From - 1) * Size + Hits.Count < Total;

        /// <summary>
        /// Total number of pages
        /// </summary>
        public long TotalPages => (Total + Size - 1) / Size;
    }
}
