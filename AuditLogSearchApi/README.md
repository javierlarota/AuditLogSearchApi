# Audit Log Search API

A full-text search solution for audit logs using PostgreSQL and ASP.NET Core, with an API similar to AWS OpenSearch.

## Features

- **Full-Text Search**: PostgreSQL-powered full-text search across all columns
- **Boolean Operators**: Support for AND, OR, and NOT operations
- **Column-Specific Search**: Search within specific columns (e.g., `user_name:John AND action:LOGIN`)
- **Pagination**: OpenSearch-style pagination with configurable page size
- **Date Filtering**: Filter results by date range
- **Sorting**: Sort by any column with ascending/descending order
- **Ranking**: Relevance-based ranking for search results
- **RESTful API**: Clean REST API with Swagger/OpenAPI documentation

## Prerequisites

- .NET 9.0 SDK or later
- PostgreSQL 12 or later
- Visual Studio, VS Code, or any C# IDE

## Installation

### 1. Clone or Download the Project

```bash
cd AuditLogSearchApi
```

### 2. Install NuGet Packages

```bash
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Swashbuckle.AspNetCore
```

Or manually add to `AuditLogSearchApi.csproj`:

```xml
<ItemGroup>
  <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="9.0.0" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="9.0.0" />
  <PackageReference Include="Swashbuckle.AspNetCore" Version="7.2.0" />
</ItemGroup>
```

Then run:

```bash
dotnet restore
```

### 3. Configure Database Connection

Edit `appsettings.json` and update the connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=auditlog;Username=postgres;Password=your_password_here"
  }
}
```

### 4. Create Database and Tables

Run the SQL scripts in order:

```bash
# Connect to PostgreSQL
psql -U postgres

# Create database
CREATE DATABASE auditlog;

# Connect to the new database
\c auditlog

# Run the schema creation script
\i SQL/01_CreateSchema.sql

# Run the sample data script
\i SQL/02_SampleData.sql
```

Or using a PostgreSQL client, execute:
1. `SQL/01_CreateSchema.sql` - Creates tables, indices, triggers, and functions
2. `SQL/02_SampleData.sql` - Populates sample data (30+ audit log entries)

## Running the Application

```bash
dotnet build
dotnet run
```

The API will be available at:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`
- Swagger UI: `http://localhost:5000` or `https://localhost:5001`

## API Endpoints

### 1. Search Audit Logs (POST)

**Endpoint**: `POST /api/auditlogs/_search`

**Description**: Full-text search with pagination (OpenSearch-style)

**Request Body**:
```json
{
  "query": "login AND failed",
  "from": 1,
  "size": 10,
  "sort": "timestamp",
  "sortDescending": true,
  "fromDate": "2024-01-01T00:00:00Z",
  "toDate": "2024-12-31T23:59:59Z"
}
```

**Query Examples**:
- Simple search: `"login"`
- AND operator: `"login AND failed"`
- OR operator: `"login OR logout"`
- Column-specific: `"user_name:John AND action:LOGIN"`
- Combined: `"user_name:admin AND (action:CREATE OR action:DELETE)"`
- NOT operator: `"login NOT failed"`

**Response**:
```json
{
  "Total": 150,
  "From": 1,
  "Size": 10,
  "Hits": [
    {
      "Id": 1,
      "Timestamp": "2024-01-15T08:30:00Z",
      "UserId": "user001",
      "UserName": "John Smith",
      "Action": "LOGIN",
      "ResourceType": "Authentication",
      "ResourceId": "session-12345",
      "IpAddress": "192.168.1.100",
      "Status": "SUCCESS",
      "Details": "User logged in successfully",
      "Metadata": "{\"browser\": \"Chrome\"}",
      "CreatedAt": "2024-01-15T08:30:00Z"
    }
  ],
  "Took": 45,
  "HasMore": true,
  "TotalPages": 15
}
```

### 2. Get All Audit Logs (GET)

**Endpoint**: `GET /api/auditlogs?from=1&size=10&fromDate=2024-01-01&toDate=2024-12-31`

**Description**: Retrieve all audit logs with pagination

**Query Parameters**:
- `from` (int): Page number (default: 1)
- `size` (int): Results per page (default: 10, max: 1000)
- `fromDate` (DateTime?): Filter start date
- `toDate` (DateTime?): Filter end date

### 3. Get Audit Log by ID (GET)

**Endpoint**: `GET /api/auditlogs/{id}`

**Description**: Retrieve a specific audit log by ID

### 4. Health Check (GET)

**Endpoint**: `GET /api/auditlogs/_health`

**Description**: API health check

## Search Query Syntax

### Basic Search
Search across all columns:
```
"login"
```

### AND Operator
Both terms must be present:
```
"login AND failed"
```

### OR Operator
Either term must be present:
```
"login OR logout"
```

### NOT Operator
Exclude results containing the term:
```
"login NOT failed"
```

### Column-Specific Search
Search within a specific column:
```
"user_name:John"
"action:LOGIN"
"status:FAILED"
```

### Combined Queries
Combine operators and column searches:
```
"user_name:John AND action:LOGIN"
"(user_name:John OR user_name:Jane) AND status:SUCCESS"
"resource_type:Document AND action:DELETE"
```

### Available Columns for Search
- `id` - Audit log ID
- `timestamp` - Event timestamp
- `user_id` - User identifier
- `user_name` - User display name
- `action` - Action performed (LOGIN, CREATE, UPDATE, DELETE, etc.)
- `resource_type` - Type of resource (Document, User, Database, etc.)
- `resource_id` - Resource identifier
- `ip_address` - IP address
- `status` - Status (SUCCESS, FAILED, WARNING)
- `details` - Additional details
- `metadata` - JSON metadata
- `created_at` - Record creation timestamp

## Database Schema

### audit_logs Table

| Column | Type | Description |
|--------|------|-------------|
| id | BIGSERIAL | Primary key |
| timestamp | TIMESTAMPTZ | Event timestamp |
| user_id | VARCHAR(100) | User identifier |
| user_name | VARCHAR(255) | User display name |
| action | VARCHAR(100) | Action performed |
| resource_type | VARCHAR(100) | Resource type |
| resource_id | VARCHAR(255) | Resource identifier |
| ip_address | INET | IP address |
| status | VARCHAR(50) | Status code |
| details | TEXT | Additional details |
| metadata | JSONB | JSON metadata |
| search_vector | TSVECTOR | Full-text search vector (auto-maintained) |
| created_at | TIMESTAMPTZ | Record creation time |

### Indices

- **B-Tree indices** on timestamp, user_id, action, resource_type, status for fast filtering
- **GIN index** on search_vector for full-text search performance
- **GIN index** on metadata for JSON queries
- **GIN trigram indices** for fuzzy/partial matching on text columns

### Triggers

- **audit_logs_search_vector_update**: Automatically updates the search_vector column on INSERT/UPDATE

## Performance Considerations

1. **Full-Text Search**: Uses PostgreSQL's built-in full-text search with GIN indices for optimal performance
2. **Indices**: Comprehensive indexing strategy for fast queries on all searchable columns
3. **Pagination**: Efficient offset-based pagination
4. **Connection Pooling**: Npgsql handles connection pooling automatically
5. **Async Operations**: All database operations are async for better scalability

## Example Usage with cURL

### Search for failed login attempts:
```bash
curl -X POST "http://localhost:5000/api/auditlogs/_search" \
  -H "Content-Type: application/json" \
  -d '{
    "query": "login AND failed",
    "from": 1,
    "size": 10
  }'
```

### Search by user:
```bash
curl -X POST "http://localhost:5000/api/auditlogs/_search" \
  -H "Content-Type: application/json" \
  -d '{
    "query": "user_name:John AND action:LOGIN",
    "from": 1,
    "size": 10,
    "sort": "timestamp",
    "sortDescending": true
  }'
```

### Get all audit logs with date filter:
```bash
curl "http://localhost:5000/api/auditlogs?from=1&size=20&fromDate=2024-01-01&toDate=2024-12-31"
```

### Get specific audit log:
```bash
curl "http://localhost:5000/api/auditlogs/1"
```

## Testing with Sample Data

The project includes 30+ sample audit log entries covering various scenarios:
- User authentication (login/logout)
- Document operations (create, update, delete, view)
- Database operations (backup, restore, optimize)
- User management (create, update, disable)
- API calls
- Security events
- File operations
- Configuration changes
- Batch processes
- System alerts

## Project Structure

```
AuditLogSearchApi/
├── SQL/
│   ├── 01_CreateSchema.sql      # Database schema, indices, triggers
│   └── 02_SampleData.sql        # Sample data population
├── Models/
│   └── AuditLog.cs              # Entity model
├── Data/
│   └── AuditLogDbContext.cs     # EF Core DbContext
├── DTOs/
│   ├── SearchRequest.cs         # Search request model
│   └── SearchResponse.cs        # Search response model
├── Services/
│   └── SearchQueryParser.cs     # Query parser for AND/OR logic
├── Repositories/
│   ├── IAuditLogRepository.cs   # Repository interface
│   └── AuditLogRepository.cs    # Repository implementation
├── Controllers/
│   └── AuditLogsController.cs   # API controller
├── Program.cs                    # Application entry point
├── appsettings.json             # Configuration
└── README.md                     # This file
```

## Troubleshooting

### Connection Issues
- Verify PostgreSQL is running: `pg_isready`
- Check connection string in `appsettings.json`
- Ensure PostgreSQL user has necessary permissions

### Search Not Working
- Verify pg_trgm extension is installed: `CREATE EXTENSION IF NOT EXISTS pg_trgm;`
- Check that triggers are created: `\dft` in psql
- Rebuild search vectors: `UPDATE audit_logs SET search_vector = search_vector;`

### Performance Issues
- Ensure all indices are created: `\di` in psql
- Run `VACUUM ANALYZE audit_logs;` to update statistics
- Check query execution plan: `EXPLAIN ANALYZE SELECT ...`

## License

This project is provided as-is for educational and commercial use.

## Support

For issues or questions, please refer to the API documentation in Swagger UI or review the source code comments.
