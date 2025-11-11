# Audit Log Search API Solution

A full-featured ASP.NET Core Web API implementing full-text search for audit logs using PostgreSQL, with an AWS OpenSearch-like API interface.

## 📁 Project Structure

```
Test/
├── Test.sln                           # Solution file
├── AuditLogSearchApi/                 # Main API project
│   ├── Controllers/                   # API endpoints
│   ├── Data/                          # EF Core DbContext
│   ├── DTOs/                          # Request/Response models
│   ├── Models/                        # Entity models
│   ├── Repositories/                  # Data access layer
│   ├── Services/                      # Business logic (query parsing)
│   ├── SQL/                           # Database scripts
│   │   ├── 01_create_database.sql    # Database setup
│   │   ├── 02_create_tables.sql      # Table definitions
│   │   ├── 03_create_indices.sql     # Full-text search indices
│   │   ├── 04_create_triggers.sql    # Auto-update triggers
│   │   └── 05_sample_data.sql        # Test data (100 records)
│   ├── README.md                      # API documentation
│   ├── SETUP_INSTRUCTIONS.md          # Setup guide
│   └── AuditLogSearchApi.http         # API test requests
└── AuditLogSearchApi.Tests/           # Unit test project
    ├── SearchQueryParserTests.cs      # Query parser tests (17 tests)
    ├── AuditLogsControllerTests.cs    # Controller tests (11 tests)
    └── README_TESTS.md                # Test documentation
```

## 🚀 Quick Start

### Prerequisites

- .NET 9.0 SDK
- PostgreSQL 12+ with full-text search support
- IDE: Visual Studio 2022 / VS Code / Rider

### 1. Build the Solution

```bash
# Restore dependencies and build
dotnet restore Test.sln
dotnet build Test.sln

# Or build specific projects
dotnet build AuditLogSearchApi/AuditLogSearchApi.csproj
dotnet build AuditLogSearchApi.Tests/AuditLogSearchApi.Tests.csproj
```

### 2. Setup PostgreSQL Database

Execute the SQL scripts in order:

```bash
psql -U postgres -f AuditLogSearchApi/SQL/01_create_database.sql
psql -U postgres -d auditlogdb -f AuditLogSearchApi/SQL/02_create_tables.sql
psql -U postgres -d auditlogdb -f AuditLogSearchApi/SQL/03_create_indices.sql
psql -U postgres -d auditlogdb -f AuditLogSearchApi/SQL/04_create_triggers.sql
psql -U postgres -d auditlogdb -f AuditLogSearchApi/SQL/05_sample_data.sql
```

### 3. Configure Connection String

Update `AuditLogSearchApi/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=auditlogdb;Username=postgres;Password=your_password"
  }
}
```

### 4. Run the API

```bash
cd AuditLogSearchApi
dotnet run
```

API will be available at:
- HTTP: http://localhost:5000
- HTTPS: https://localhost:5001
- Swagger UI: https://localhost:5001/swagger

### 5. Run Tests

```bash
# Run all tests
dotnet test

# Run with detailed output
dotnet test --verbosity detailed

# Run tests in a specific project
dotnet test AuditLogSearchApi.Tests/AuditLogSearchApi.Tests.csproj
```

## 📊 API Endpoints

### Search Endpoint (POST /api/auditlogs/search)

AWS OpenSearch-like paginated search with full-text queries:

```bash
curl -X POST "https://localhost:5001/api/auditlogs/search" \
  -H "Content-Type: application/json" \
  -d '{
    "query": "login AND success",
    "from": 1,
    "size": 10,
    "fromDate": "2024-01-01T00:00:00Z",
    "toDate": "2024-12-31T23:59:59Z",
    "sort": "timestamp",
    "sortDescending": true
  }'
```

### Query Syntax

- **Simple search**: `"login"`
- **AND operator**: `"login AND success"`
- **OR operator**: `"error OR failure"`
- **NOT operator**: `"login NOT failure"`
- **Column-specific**: `"user_name:John action:LOGIN"`
- **Complex queries**: `"(login OR logout) AND user_name:admin"`

### Other Endpoints

- `GET /api/auditlogs/{id}` - Get audit log by ID
- `GET /api/auditlogs` - Get all logs (paginated)
- `GET /health` - Health check

## 🧪 Testing

The solution includes 28 comprehensive unit tests:

### Query Parser Tests (17 tests)
- Simple keyword searches
- Boolean operators (AND, OR, NOT)
- Column-specific searches
- Complex multi-operator queries
- Edge cases and validation

### Controller Tests (11 tests)
- API endpoint functionality
- Input validation
- Error handling
- Pagination
- Filtering and sorting

**Test Framework**: xUnit 2.9.3  
**Mocking Library**: NSubstitute 5.1.0

## 🔧 Technology Stack

- **Framework**: ASP.NET Core 9.0
- **Database**: PostgreSQL 12+ with full-text search
- **ORM**: Entity Framework Core 9.0
- **Testing**: xUnit + NSubstitute
- **API Documentation**: Swagger/OpenAPI

## 📖 Full-Text Search Features

- **tsvector indices** for blazing-fast searches
- **Auto-updating triggers** keep search indices in sync
- **Multiple search configurations**: Simple (default) and English
- **Rank-based sorting** for relevance
- **Column-specific searches** for precise filtering
- **Boolean operators** for complex queries

## 📝 Documentation

- **[API Documentation](AuditLogSearchApi/README.md)** - Detailed API guide
- **[Setup Instructions](AuditLogSearchApi/SETUP_INSTRUCTIONS.md)** - Complete setup guide
- **[Test Documentation](AuditLogSearchApi.Tests/README_TESTS.md)** - Test suite details

## 🐛 Troubleshooting

### Build Errors

```bash
# Clear caches and rebuild
dotnet nuget locals all --clear
dotnet clean Test.sln
dotnet restore Test.sln
dotnet build Test.sln
```

### Database Connection Issues

1. Verify PostgreSQL is running: `pg_isready`
2. Check connection string in `appsettings.json`
3. Ensure database exists: `psql -l`
4. Test connection: `psql -U postgres -d auditlogdb`

### Test Failures

```bash
# Run tests with detailed output
dotnet test --verbosity detailed

# Run specific test
dotnet test --filter "FullyQualifiedName~SearchQueryParserTests"
```

## 📄 License

This is a sample project for demonstration purposes.

## 🤝 Contributing

This is a reference implementation. Feel free to adapt it for your needs!

---

**Built with ❤️ using .NET 9.0 and PostgreSQL**
