using AuditLogSearchApi.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AuditLogSearchApi.Repositories
{
    public interface IAuditLogRepository
    {
        /// <summary>
        /// Search audit logs using full-text search with pagination
        /// </summary>
        Task<(List<AuditLog> Results, long TotalCount)> SearchAsync(
            string query,
            int page = 1,
            int pageSize = 10,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            string? sortBy = null,
            bool sortDescending = true);

        /// <summary>
        /// Get audit log by ID
        /// </summary>
        Task<AuditLog?> GetByIdAsync(long id);

        /// <summary>
        /// Get all audit logs with pagination
        /// </summary>
        Task<(List<AuditLog> Results, long TotalCount)> GetAllAsync(
            int page = 1,
            int pageSize = 10,
            DateTime? fromDate = null,
            DateTime? toDate = null);
    }
}
