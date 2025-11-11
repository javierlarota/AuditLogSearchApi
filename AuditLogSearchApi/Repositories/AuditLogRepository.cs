using AuditLogSearchApi.Data;
using AuditLogSearchApi.Models;
using AuditLogSearchApi.Services;

using Microsoft.EntityFrameworkCore;

using Npgsql;

using System.Data;
using System.Text;

namespace AuditLogSearchApi.Repositories
{
    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly AuditLogDbContext _context;
        private readonly SearchQueryParser _queryParser;

        public AuditLogRepository(AuditLogDbContext context, SearchQueryParser queryParser)
        {
            _context = context;
            _queryParser = queryParser;
        }

        public async Task<(List<AuditLog> Results, long TotalCount)> SearchAsync(
            string query,
            int page = 1,
            int pageSize = 10,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            string? sortBy = null,
            bool sortDescending = true)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return await GetAllAsync(page, pageSize, fromDate, toDate);
            }

            // Parse the query
            var parsedQuery = _queryParser.Parse(query);

            // Build the SQL query based on whether it's column-specific or full-text
            if (parsedQuery.IsColumnSpecific && parsedQuery.Conditions.Any())
            {
                return await SearchByColumnsAsync(parsedQuery.Conditions, page, pageSize, fromDate, toDate, sortBy, sortDescending);
            }
            else if (!string.IsNullOrEmpty(parsedQuery.PostgresQuery))
            {
                return await SearchFullTextAsync(parsedQuery.PostgresQuery, page, pageSize, fromDate, toDate, sortBy, sortDescending);
            }

            return (new List<AuditLog>(), 0);
        }

        private async Task<(List<AuditLog> Results, long TotalCount)> SearchFullTextAsync(
            string tsQuery,
            int page,
            int pageSize,
            DateTime? fromDate,
            DateTime? toDate,
            string? sortBy,
            bool sortDescending)
        {
            var offset = (page - 1) * pageSize;
            var sortColumn = GetSortColumn(sortBy);
            var sortDirection = sortDescending ? "DESC" : "ASC";

            // Build WHERE clause for date filtering
            var whereClause = new StringBuilder();
            var parameters = new List<NpgsqlParameter>();

            if (fromDate.HasValue || toDate.HasValue)
            {
                whereClause.Append(" AND ");
                if (fromDate.HasValue && toDate.HasValue)
                {
                    whereClause.Append("timestamp BETWEEN @fromDate AND @toDate");
                    parameters.Add(new NpgsqlParameter("@fromDate", fromDate.Value));
                    parameters.Add(new NpgsqlParameter("@toDate", toDate.Value));
                }
                else if (fromDate.HasValue)
                {
                    whereClause.Append("timestamp >= @fromDate");
                    parameters.Add(new NpgsqlParameter("@fromDate", fromDate.Value));
                }
                else
                {
                    whereClause.Append("timestamp <= @toDate");
                    parameters.Add(new NpgsqlParameter("@toDate", toDate.Value));
                }
            }

            // Count query
            var countSql = $@"
                SELECT COUNT(*) AS ""Value""
                FROM audit_logs
                WHERE search_vector @@ to_tsquery('english', @query)
                {whereClause}";

            parameters.Insert(0, new NpgsqlParameter("@query", tsQuery));

            var totalCount = await _context.Database
                .SqlQueryRaw<long>(countSql, parameters.ToArray())
                .FirstOrDefaultAsync();

            // Data query with ranking
            var dataSql = $@"
                SELECT 
                    id, timestamp, user_id, user_name, action, resource_type,
                    resource_id, ip_address, status, details, metadata, created_at,
                    ts_rank(search_vector, to_tsquery('english', @query)) as rank
                FROM audit_logs
                WHERE search_vector @@ to_tsquery('english', @query)
                {whereClause}
                ORDER BY {(sortBy == "rank" || sortBy == "relevance" ? "rank DESC" : $"{sortColumn} {sortDirection}")}
                LIMIT @limit OFFSET @offset";

            var dataParameters = new List<NpgsqlParameter>(parameters)
            {
                new NpgsqlParameter("@limit", pageSize),
                new NpgsqlParameter("@offset", offset)
            };

            var results = await _context.AuditLogs
                .FromSqlRaw(dataSql, dataParameters.ToArray())
                .AsNoTracking()
                .ToListAsync();

            return (results, totalCount);
        }

        private async Task<(List<AuditLog> Results, long TotalCount)> SearchByColumnsAsync(
            List<SearchQueryParser.QueryCondition> conditions,
            int page,
            int pageSize,
            DateTime? fromDate,
            DateTime? toDate,
            string? sortBy,
            bool sortDescending)
        {
            var offset = (page - 1) * pageSize;
            var sortColumn = GetSortColumn(sortBy);
            var sortDirection = sortDescending ? "DESC" : "ASC";

            // Build WHERE clause from conditions
            var whereBuilder = new StringBuilder();
            var parameters = new List<NpgsqlParameter>();
            var paramIndex = 0;

            for (int i = 0; i < conditions.Count; i++)
            {
                var condition = conditions[i];

                if (i > 0)
                {
                    whereBuilder.Append($" {condition.Operator} ");
                }

                var paramName = $"@p{paramIndex++}";
                var searchPattern = $"%{condition.SearchTerm}%";

                // Use ILIKE for case-insensitive search, or exact match for specific columns
                if (condition.Column?.ToLower() == "id")
                {
                    // For ID, try exact match first, fallback to LIKE
                    whereBuilder.Append($"({condition.Column}::text = {paramName} OR {condition.Column}::text LIKE {paramName})");
                    parameters.Add(new NpgsqlParameter(paramName, condition.SearchTerm));
                }
                else if (condition.Column?.ToLower() == "timestamp" || condition.Column?.ToLower() == "created_at")
                {
                    // For timestamps, convert to text and search
                    whereBuilder.Append($"{condition.Column}::text ILIKE {paramName}");
                    parameters.Add(new NpgsqlParameter(paramName, searchPattern));
                }
                else if (condition.Column?.ToLower() == "metadata")
                {
                    // For JSONB metadata, search within the JSON text
                    whereBuilder.Append($"{condition.Column}::text ILIKE {paramName}");
                    parameters.Add(new NpgsqlParameter(paramName, searchPattern));
                }
                else
                {
                    // For regular text columns
                    whereBuilder.Append($"{condition.Column}::text ILIKE {paramName}");
                    parameters.Add(new NpgsqlParameter(paramName, searchPattern));
                }
            }

            var whereClause = whereBuilder.ToString();

            // Add date filtering
            if (fromDate.HasValue || toDate.HasValue)
            {
                whereClause += " AND ";
                if (fromDate.HasValue && toDate.HasValue)
                {
                    whereClause += $"timestamp BETWEEN @fromDate AND @toDate";
                    parameters.Add(new NpgsqlParameter("@fromDate", fromDate.Value));
                    parameters.Add(new NpgsqlParameter("@toDate", toDate.Value));
                }
                else if (fromDate.HasValue)
                {
                    whereClause += $"timestamp >= @fromDate";
                    parameters.Add(new NpgsqlParameter("@fromDate", fromDate.Value));
                }
                else
                {
                    whereClause += $"timestamp <= @toDate";
                    parameters.Add(new NpgsqlParameter("@toDate", toDate.Value));
                }
            }

            // Count query
            var countSql = $@"
                SELECT COUNT(*) AS ""Value""
                FROM audit_logs
                WHERE {whereClause}";

            var totalCountResult = await _context.Database
                .SqlQueryRaw<long>(countSql, parameters.ToArray())
                .FirstOrDefaultAsync();
            var totalCount = totalCountResult;

            // Data query
            var dataSql = $@"
                SELECT 
                    id, timestamp, user_id, user_name, action, resource_type,
                    resource_id, ip_address, status, details, metadata, created_at
                FROM audit_logs
                WHERE {whereClause}
                ORDER BY {sortColumn} {sortDirection}
                LIMIT @limit OFFSET @offset";

            var dataParameters = new List<NpgsqlParameter>(parameters)
            {
                new NpgsqlParameter("@limit", pageSize),
                new NpgsqlParameter("@offset", offset)
            };

            var results = await _context.AuditLogs
                .FromSqlRaw(dataSql, dataParameters.ToArray())
                .AsNoTracking()
                .ToListAsync();

            return (results, totalCount);
        }

        public async Task<(List<AuditLog> Results, long TotalCount)> GetAllAsync(
            int page = 1,
            int pageSize = 10,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            var query = _context.AuditLogs.AsQueryable();

            if (fromDate.HasValue)
            {
                query = query.Where(a => a.Timestamp >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(a => a.Timestamp <= toDate.Value);
            }

            var totalCount = await query.LongCountAsync();

            var results = await query
                .OrderByDescending(a => a.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            return (results, totalCount);
        }

        public async Task<AuditLog?> GetByIdAsync(long id)
        {
            return await _context.AuditLogs
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        private string GetSortColumn(string? sortBy)
        {
            return sortBy?.ToLower() switch
            {
                "timestamp" => "timestamp",
                "user_id" => "user_id",
                "user_name" => "user_name",
                "action" => "action",
                "resource_type" => "resource_type",
                "status" => "status",
                "created_at" => "created_at",
                _ => "timestamp"
            };
        }
    }
}
