using AuditLogSearchApi.Controllers;
using AuditLogSearchApi.DTOs;
using AuditLogSearchApi.Models;
using AuditLogSearchApi.Repositories;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using NSubstitute;

namespace AuditLogSearchApi.Tests
{
    public class AuditLogsControllerTests
    {
        private readonly IAuditLogRepository _mockRepository;
        private readonly ILogger<AuditLogsController> _mockLogger;
        private readonly AuditLogsController _controller;

        public AuditLogsControllerTests()
        {
            _mockRepository = Substitute.For<IAuditLogRepository>();
            _mockLogger = Substitute.For<ILogger<AuditLogsController>>();
            _controller = new AuditLogsController(_mockRepository, _mockLogger);
        }

        [Fact]
        public async Task Search_ValidRequest_ReturnsOkResult()
        {
            // Arrange
            var request = new SearchRequest
            {
                Query = "login",
                From = 1,
                Size = 10
            };

            var mockResults = new List<AuditLog>
            {
                new AuditLog
                {
                    Id = 1,
                    UserId = "user001",
                    UserName = "John Doe",
                    Action = "LOGIN",
                    Status = "SUCCESS",
                    Timestamp = DateTime.UtcNow
                }
            };

            _mockRepository.SearchAsync(
                request.Query,
                request.From,
                request.Size,
                request.FromDate,
                request.ToDate,
                request.Sort,
                request.SortDescending)
                .Returns((mockResults, 1L));

            // Act
            var result = await _controller.Search(request);

            // Assert
            var okResult = Assert.IsType<ActionResult<SearchResponse>>(result);
            var okObjectResult = Assert.IsType<OkObjectResult>(okResult.Result);
            var response = Assert.IsType<SearchResponse>(okObjectResult.Value);
            Assert.Equal(1, response.Total);
            Assert.Single(response.Hits);
        }

        [Fact]
        public async Task Search_EmptyQuery_ReturnsResults()
        {
            // Arrange
            var request = new SearchRequest
            {
                Query = "",
                From = 1,
                Size = 10
            };

            _mockRepository.SearchAsync(
                Arg.Any<string>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<DateTime?>(),
                Arg.Any<DateTime?>(),
                Arg.Any<string?>(),
                Arg.Any<bool>())
                .Returns((new List<AuditLog>(), 0L));

            // Act
            var result = await _controller.Search(request);

            // Assert
            var okResult = Assert.IsType<ActionResult<SearchResponse>>(result);
            var okObjectResult = Assert.IsType<OkObjectResult>(okResult.Result);
            var response = Assert.IsType<SearchResponse>(okObjectResult.Value);
            Assert.Equal(0, response.Total);
        }

        [Fact]
        public async Task GetById_ExistingId_ReturnsAuditLog()
        {
            // Arrange
            var auditLog = new AuditLog
            {
                Id = 1,
                UserId = "user001",
                UserName = "John Doe",
                Action = "LOGIN",
                Status = "SUCCESS",
                Timestamp = DateTime.UtcNow
            };

            _mockRepository.GetByIdAsync(1).Returns(auditLog);

            // Act
            var result = await _controller.GetById(1);

            // Assert
            var okResult = Assert.IsType<ActionResult<AuditLog>>(result);
            var okObjectResult = Assert.IsType<OkObjectResult>(okResult.Result);
            var returnedLog = Assert.IsType<AuditLog>(okObjectResult.Value);
            Assert.Equal(1, returnedLog.Id);
        }

        [Fact]
        public async Task GetById_NonExistingId_ReturnsNotFound()
        {
            // Arrange
            _mockRepository.GetByIdAsync(999).Returns((AuditLog?)null);

            // Act
            var result = await _controller.GetById(999);

            // Assert
            var actionResult = Assert.IsType<ActionResult<AuditLog>>(result);
            Assert.IsType<NotFoundObjectResult>(actionResult.Result);
        }

        [Fact]
        public async Task GetAll_ValidParameters_ReturnsResults()
        {
            // Arrange
            var mockResults = new List<AuditLog>
            {
                new AuditLog { Id = 1, Action = "LOGIN", Timestamp = DateTime.UtcNow },
                new AuditLog { Id = 2, Action = "LOGOUT", Timestamp = DateTime.UtcNow }
            };

            _mockRepository.GetAllAsync(1, 10, null, null).Returns((mockResults, 2L));

            // Act
            var result = await _controller.GetAll(1, 10);

            // Assert
            var okResult = Assert.IsType<ActionResult<SearchResponse>>(result);
            var okObjectResult = Assert.IsType<OkObjectResult>(okResult.Result);
            var response = Assert.IsType<SearchResponse>(okObjectResult.Value);
            Assert.Equal(2, response.Total);
            Assert.Equal(2, response.Hits.Count);
        }

        [Fact]
        public async Task GetAll_InvalidFrom_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.GetAll(0, 10);

            // Assert
            var actionResult = Assert.IsType<ActionResult<SearchResponse>>(result);
            Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        }

        [Fact]
        public async Task GetAll_InvalidSize_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.GetAll(1, 2000);

            // Assert
            var actionResult = Assert.IsType<ActionResult<SearchResponse>>(result);
            Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        }

        [Fact]
        public async Task Health_ReturnsOk()
        {
            // Act
            var result = _controller.Health();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task Search_WithDateFilter_CallsRepositoryWithDates()
        {
            // Arrange
            var fromDate = DateTime.UtcNow.AddDays(-7);
            var toDate = DateTime.UtcNow;
            var request = new SearchRequest
            {
                Query = "login",
                From = 1,
                Size = 10,
                FromDate = fromDate,
                ToDate = toDate
            };

            _mockRepository.SearchAsync(
                request.Query,
                request.From,
                request.Size,
                fromDate,
                toDate,
                request.Sort,
                request.SortDescending)
                .Returns((new List<AuditLog>(), 0L));

            // Act
            await _controller.Search(request);

            // Assert
            await _mockRepository.Received(1).SearchAsync(
                request.Query,
                request.From,
                request.Size,
                fromDate,
                toDate,
                request.Sort,
                request.SortDescending);
        }

        [Fact]
        public async Task Search_WithSorting_CallsRepositoryWithSortParameters()
        {
            // Arrange
            var request = new SearchRequest
            {
                Query = "login",
                From = 1,
                Size = 10,
                Sort = "timestamp",
                SortDescending = false
            };

            _mockRepository.SearchAsync(
                Arg.Any<string>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<DateTime?>(),
                Arg.Any<DateTime?>(),
                "timestamp",
                false)
                .Returns((new List<AuditLog>(), 0L));

            // Act
            await _controller.Search(request);

            // Assert
            await _mockRepository.Received(1).SearchAsync(
                request.Query,
                request.From,
                request.Size,
                request.FromDate,
                request.ToDate,
                "timestamp",
                false);
        }
    }
}
