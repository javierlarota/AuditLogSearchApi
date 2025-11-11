using AuditLogSearchApi.DTOs;
using AuditLogSearchApi.Models;
using AuditLogSearchApi.Repositories;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace AuditLogSearchApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuditLogsController : ControllerBase
    {
        private readonly IAuditLogRepository _repository;
        private readonly ILogger<AuditLogsController> _logger;

        public AuditLogsController(IAuditLogRepository repository, ILogger<AuditLogsController> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        /// <summary>
        /// Search audit logs with full-text search and pagination (OpenSearch-style API)
        /// </summary>
        /// <param name="request">Search request parameters</param>
        /// <returns>Paginated search results</returns>
        [HttpPost("_search")]
        [ProducesResponseType(typeof(SearchResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<SearchResponse>> Search([FromBody] SearchRequest request)
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();

                var (results, totalCount) = await _repository.SearchAsync(
                    request.Query,
                    request.From,
                    request.Size,
                    request.FromDate,
                    request.ToDate,
                    request.Sort,
                    request.SortDescending
                );

                stopwatch.Stop();

                var response = new SearchResponse
                {
                    Total = totalCount,
                    From = request.From,
                    Size = request.Size,
                    Hits = results,
                    Took = stopwatch.ElapsedMilliseconds
                };

                _logger.LogInformation(
                    "Search executed: Query='{Query}', Total={Total}, From={From}, Size={Size}, Took={Took}ms",
                    request.Query, totalCount, request.From, request.Size, stopwatch.ElapsedMilliseconds);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing search: {Message}", ex.Message);
                return StatusCode(500, new { error = "An error occurred while searching", message = ex.Message });
            }
        }

        /// <summary>
        /// Get audit log by ID
        /// </summary>
        /// <param name="id">Audit log ID</param>
        /// <returns>Audit log entry</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(AuditLog), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AuditLog>> GetById(long id)
        {
            try
            {
                var result = await _repository.GetByIdAsync(id);

                if (result == null)
                {
                    return NotFound(new { error = "Audit log not found", id });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving audit log {Id}: {Message}", id, ex.Message);
                return StatusCode(500, new { error = "An error occurred while retrieving audit log", message = ex.Message });
            }
        }

        /// <summary>
        /// Get all audit logs with pagination
        /// </summary>
        /// <param name="from">Page number (1-based)</param>
        /// <param name="size">Number of results per page</param>
        /// <param name="fromDate">Filter by start date</param>
        /// <param name="toDate">Filter by end date</param>
        /// <returns>Paginated audit logs</returns>
        [HttpGet]
        [ProducesResponseType(typeof(SearchResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<SearchResponse>> GetAll(
            [FromQuery] int from = 1,
            [FromQuery] int size = 10,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                if (from < 1)
                {
                    return BadRequest(new { error = "Parameter 'from' must be greater than 0" });
                }

                if (size < 1 || size > 1000)
                {
                    return BadRequest(new { error = "Parameter 'size' must be between 1 and 1000" });
                }

                var stopwatch = Stopwatch.StartNew();

                var (results, totalCount) = await _repository.GetAllAsync(from, size, fromDate, toDate);

                stopwatch.Stop();

                var response = new SearchResponse
                {
                    Total = totalCount,
                    From = from,
                    Size = size,
                    Hits = results,
                    Took = stopwatch.ElapsedMilliseconds
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving audit logs: {Message}", ex.Message);
                return StatusCode(500, new { error = "An error occurred while retrieving audit logs", message = ex.Message });
            }
        }

        /// <summary>
        /// Health check endpoint
        /// </summary>
        [HttpGet("_health")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Health()
        {
            return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
        }
    }
}
