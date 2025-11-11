# Unit Tests Documentation

## Overview

This test project contains comprehensive unit tests for the Audit Log Search API, covering the query parser and controller functionality.

## Test Files

### 1. SearchQueryParserTests.cs (17 tests)

Tests the query parsing logic that converts user queries into PostgreSQL full-text search queries.

**Test Categories:**

#### Simple Queries
- `Parse_SimpleQuery_ReturnsFullTextSearch` - Tests basic keyword searches
- `Parse_EmptyQuery_ReturnsEmptyResult` - Validates empty query handling
- `Parse_WhitespaceQuery_ReturnsEmptyResult` - Tests whitespace-only queries

#### Boolean Operators
- `Parse_AndOperator_ReturnsCorrectQuery` - Tests AND logic (converts to &)
- `Parse_OrOperator_ReturnsCorrectQuery` - Tests OR logic (converts to |)
- `Parse_NotOperator_ReturnsCorrectQuery` - Tests NOT logic (converts to !)
- `Parse_ComplexQuery_WithMultipleOperators` - Tests combined operators

#### Column-Specific Searches
- `Parse_ColumnSpecificQuery_ReturnsConditions` - Tests `user_name:John` syntax
- `Parse_ColumnSpecificWithAnd_ReturnsMultipleConditions` - Tests multiple column searches with AND
- `Parse_ColumnSpecificWithOr_ReturnsMultipleConditions` - Tests multiple column searches with OR
- `Parse_MixedQuery_WithColumnAndFullText` - Tests combined query types
- `Parse_VariousColumnQueries_ParsedCorrectly` - Parameterized tests for different columns

#### Edge Cases
- `Parse_QuotedPhrase_HandledCorrectly` - Tests phrase searches
- `Parse_CaseSensitivity_OperatorsNotCaseSensitive` - Validates case-insensitive operators

### 2. AuditLogsControllerTests.cs (11 tests)

Tests the API controller endpoints using mocked dependencies.

**Test Categories:**

#### Search Endpoint
- `Search_ValidRequest_ReturnsOkResult` - Tests successful search with results
- `Search_EmptyQuery_ReturnsResults` - Tests empty query handling
- `Search_WithDateFilter_CallsRepositoryWithDates` - Validates date filtering
- `Search_WithSorting_CallsRepositoryWithSortParameters` - Tests sorting functionality

#### GetById Endpoint
- `GetById_ExistingId_ReturnsAuditLog` - Tests successful retrieval by ID
- `GetById_NonExistingId_ReturnsNotFound` - Tests 404 response for missing records

#### GetAll Endpoint
- `GetAll_ValidParameters_ReturnsResults` - Tests successful pagination
- `GetAll_InvalidFrom_ReturnsBadRequest` - Validates page number constraints
- `GetAll_InvalidSize_ReturnsBadRequest` - Validates page size constraints

#### Health Endpoint
- `Health_ReturnsOk` - Tests health check endpoint

## Running the Tests

### Prerequisites

```bash
# Ensure you have .NET 9.0 SDK installed
dotnet --version

# Restore packages
dotnet restore AuditLogSearchApi.Tests/AuditLogSearchApi.Tests.csproj
```

### Execute Tests

```bash
# Run all tests
dotnet test AuditLogSearchApi.Tests/AuditLogSearchApi.Tests.csproj

# Run with verbose output
dotnet test AuditLogSearchApi.Tests/AuditLogSearchApi.Tests.csproj --verbosity detailed

# Run specific test
dotnet test --filter "FullyQualifiedName~SearchQueryParserTests.Parse_SimpleQuery_ReturnsFullTextSearch"

# Generate code coverage
dotnet test AuditLogSearchApi.Tests/AuditLogSearchApi.Tests.csproj --collect:"XPlat Code Coverage"
```

## Test Dependencies

- **xUnit 2.9.3** - Testing framework
- **NSubstitute 5.1.0** - Mocking library (lightweight and easy to use)
- **Microsoft.EntityFrameworkCore.InMemory 9.0.0** - In-memory database for testing

## Troubleshooting

### Package Resolution Issues

If you encounter "type or namespace could not be found" errors:

```bash
# Clear NuGet caches
dotnet nuget locals all --clear

# Delete obj and bin folders
Remove-Item -Recurse -Force AuditLogSearchApi.Tests/obj,AuditLogSearchApi.Tests/bin -ErrorAction SilentlyContinue

# Restore with force
dotnet restore AuditLogSearchApi.Tests/AuditLogSearchApi.Tests.csproj --force

# Rebuild
dotnet build AuditLogSearchApi.Tests/AuditLogSearchApi.Tests.csproj
```

### IDE Issues

If Visual Studio or VS Code shows errors but the command line works:

1. Close the IDE
2. Delete `.vs` folder (Visual Studio) or restart VS Code
3. Run `dotnet clean` and `dotnet build`
4. Reopen the IDE

## Test Coverage

The tests provide coverage for:

- ✅ Query parsing logic (all operators and syntax)
- ✅ Controller endpoints (all CRUD operations)
- ✅ Input validation
- ✅ Error handling
- ✅ Pagination logic
- ✅ Filtering and sorting

## Future Test Additions

Consider adding:

1. **Integration Tests** - Test with actual PostgreSQL database
2. **Performance Tests** - Benchmark query performance with large datasets
3. **Edge Case Tests** - SQL injection attempts, malformed queries
4. **Concurrency Tests** - Multiple simultaneous requests
