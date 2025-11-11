# Quick Setup Instructions

## Step-by-Step Guide

### 1. Install Required NuGet Packages

The following packages need to be installed. Due to network restrictions, they may not have been installed automatically. Please run:

```bash
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL --version 9.0.0
dotnet add package Microsoft.EntityFrameworkCore.Design --version 9.0.0
dotnet add package Swashbuckle.AspNetCore --version 7.2.0
dotnet restore
```

### 2. Set Up PostgreSQL Database

#### Option A: Using psql Command Line
```bash
docker compose up -d
docker compose down -v

docker exec -it postgres-db psql -U admin1 -d my_database

# Connect to PostgreSQL
psql -U postgres

# Create database
CREATE DATABASE auditlog;

# Exit and reconnect to the new database
\q
psql -U postgres -d auditlog

# Run schema script
\i SQL/01_CreateSchema.sql

# Run sample data script
\i SQL/02_SampleData.sql

# Verify data
SELECT COUNT(*) FROM audit_logs;
```

#### Option B: Using pgAdmin or Another GUI Tool
1. Create a new database named `auditlog`
2. Open and execute `SQL/01_CreateSchema.sql`
3. Open and execute `SQL/02_SampleData.sql`
4. Verify by running: `SELECT COUNT(*) FROM audit_logs;`

### 3. Configure Connection String

Edit `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=auditlog;Username=postgres;Password=YOUR_PASSWORD"
  }
}
```

Replace `YOUR_PASSWORD` with your actual PostgreSQL password.

### 4. Build and Run

```bash
dotnet build
dotnet run
```

### 5. Test the API

Open your browser to:
- Swagger UI: `http://localhost:5000` or `https://localhost:5001`

Or use curl:

```bash
# Simple search
curl -X POST "http://localhost:5000/api/auditlogs/_search" \
  -H "Content-Type: application/json" \
  -d '{"query": "login", "from": 1, "size": 5}'

# AND search
curl -X POST "http://localhost:5000/api/auditlogs/_search" \
  -H "Content-Type: application/json" \
  -d '{"query": "login AND failed", "from": 1, "size": 5}'

# Column-specific search
curl -X POST "http://localhost:5000/api/auditlogs/_search" \
  -H "Content-Type: application/json" \
  -d '{"query": "user_name:John AND action:LOGIN", "from": 1, "size": 5}'

# Get all logs
curl "http://localhost:5000/api/auditlogs?from=1&size=5"

# Get by ID
curl "http://localhost:5000/api/auditlogs/1"

# Health check
curl "http://localhost:5000/api/auditlogs/_health"
```

## Test Queries to Try

Once the API is running, try these searches in Swagger UI or via curl:

1. **Find all login events**:
   ```json
   {"query": "login", "from": 1, "size": 10}
   ```

2. **Find failed authentication attempts**:
   ```json
   {"query": "login AND failed", "from": 1, "size": 10}
   ```

3. **Find actions by a specific user**:
   ```json
   {"query": "user_name:John", "from": 1, "size": 10}
   ```

4. **Find document operations**:
   ```json
   {"query": "resource_type:Document", "from": 1, "size": 10}
   ```

5. **Find successful database operations**:
   ```json
   {"query": "resource_type:Database AND status:SUCCESS", "from": 1, "size": 10}
   ```

6. **Find admin actions**:
   ```json
   {"query": "user_name:admin OR user_id:admin", "from": 1, "size": 10}
   ```

7. **Find file uploads or downloads**:
   ```json
   {"query": "action:UPLOAD OR action:DOWNLOAD", "from": 1, "size": 10}
   ```

8. **Search in details field**:
   ```json
   {"query": "financial report", "from": 1, "size": 10}
   ```

## Common Issues and Solutions

### Issue: Cannot connect to database
**Solution**: 
- Verify PostgreSQL is running: `pg_isready`
- Check connection string in `appsettings.json`
- Test connection: `psql -U postgres -d auditlog`

### Issue: Compilation errors about missing packages
**Solution**: 
Run these commands:
```bash
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Swashbuckle.AspNetCore
dotnet restore
dotnet build
```

### Issue: No search results returned
**Solution**:
- Verify data exists: `SELECT COUNT(*) FROM audit_logs;`
- Check triggers are created: `\dft` in psql
- Rebuild search vectors: `UPDATE audit_logs SET search_vector = search_vector;`

### Issue: Slow search performance
**Solution**:
- Verify indices exist: `\di` in psql
- Run: `VACUUM ANALYZE audit_logs;`
- Check query plan: `EXPLAIN ANALYZE SELECT * FROM audit_logs WHERE search_vector @@ to_tsquery('english', 'test');`

## Project Files Overview

### SQL Scripts
- `SQL/01_CreateSchema.sql` - Database schema, tables, indices, triggers
- `SQL/02_SampleData.sql` - 30+ sample audit log entries

### Application Code
- `Models/AuditLog.cs` - Entity model
- `Data/AuditLogDbContext.cs` - EF Core database context
- `DTOs/SearchRequest.cs` - Request model for search API
- `DTOs/SearchResponse.cs` - Response model with pagination info
- `Services/SearchQueryParser.cs` - Parses AND/OR/NOT queries
- `Repositories/IAuditLogRepository.cs` - Repository interface
- `Repositories/AuditLogRepository.cs` - Repository implementation with full-text search
- `Controllers/AuditLogsController.cs` - REST API endpoints
- `Program.cs` - Application startup and DI configuration
- `appsettings.json` - Configuration including connection string

### Documentation
- `README.md` - Complete documentation
- `SETUP_INSTRUCTIONS.md` - This file

## Next Steps

1. Customize the audit log schema for your specific needs
2. Add authentication/authorization to the API endpoints
3. Implement audit log writing endpoints (POST, PUT, DELETE)
4. Add more complex query features as needed
5. Set up logging and monitoring
6. Deploy to your production environment

## Support

For detailed API documentation, see `README.md` or visit the Swagger UI when the application is running.
