# MSSQL MCP Server

A lightweight Model Context Protocol (MCP) server for Microsoft SQL Server, written in .NET 9. It exposes tools to execute T-SQL and to list tables, communicating with MCP-capable clients over stdio.

## Features
- Execute T-SQL: Run SELECT/INSERT/UPDATE/DELETE and DDL statements.
- List tables: Enumerates tables across accessible, non-system databases with row counts.
- Stdio transport: Designed to be launched by an MCP client (e.g., Claude Desktop) via stdio.
- Safe startup: Validates the database connection on launch; fails fast with a clear error.
- Friendly output: SELECT results are returned as a plain-text table.
- Graceful handling of spatial/UDT values: Returns readable hints when types like `geography`, `geometry`, or `hierarchyid` cannot be materialized.

## Requirements
- .NET SDK 9.0+
- A reachable Microsoft SQL Server instance
- Environment variable `MSSQL_CONNECTION_STRING` (required)

Example connection string:
```
Server=YOUR_SQL_SERVER;Database=YOUR_DB;User Id=USER;Password=SECRET;TrustServerCertificate=True;MultipleActiveResultSets=True
```

## Quick Start (local)
1. Set the environment variable:
   - PowerShell: `$env:MSSQL_CONNECTION_STRING = "..."`
   - Bash: `export MSSQL_CONNECTION_STRING="..."`
2. Run the server from the repo root:
   - `dotnet run --project src/MSSQLMCPServer`

On startup, the server tests the connection. If successful, it writes "Database connection test succeeded." and then waits for MCP stdio messages from a client.

## Using with an MCP Client
This server is intended to be launched by an MCP-capable client via stdio. Example Claude Desktop snippet (adjust paths as needed):

```
"mcpServers": {
  "mssql": {
    "command": "dotnet",
    "args": ["run", "--project", "src/MSSQLMCPServer"],
    "env": {
      "MSSQL_CONNECTION_STRING": "Server=...;Database=...;User Id=...;Password=...;TrustServerCertificate=True"
    }
  }
}
```

## Tools

### ExecuteSql
- Purpose: Execute a T-SQL statement against the configured server.
- Signature: `ExecuteSql(query: string)`
- Input rules:
  - Query must be valid Microsoft SQL Server T-SQL.
  - Must start with a T-SQL keyword such as `SELECT`, `WITH`, `INSERT`, `UPDATE`, `DELETE`, `CREATE`, `ALTER`, `DROP`, `EXEC`, etc.
  - Do not include explanations, markdown, or fences - only the SQL statement.
- Behavior:
  - SELECT/WITH: returns a plain-text table of results. If no rows are returned, the exact message is: "Query executed successfully. No rows returned.".
  - Non-SELECT (DML/DDL): returns rows affected.
  - Spatial/UDT values are returned with a hint if they cannot be materialized (consider casting to text or using server-side functions like `STAsText()`).
- Examples:
  - `SELECT TOP 10 * FROM dbo.Users ORDER BY Created DESC;`
  - `INSERT INTO dbo.Products (Name, Price) VALUES ('Widget', 19.99);`
  - `CREATE TABLE dbo.Orders (Id int PRIMARY KEY, CustomerId int NOT NULL);`

### ListTables
- Purpose: List all tables in all accessible, non-system databases for the current login.
- Signature: `ListTables()`
- Columns: `DatabaseName`, `SchemaName`, `TableName`, `RowCount`, `TableType`.
- Notes:
  - Excludes system databases using `database_id > 4` and `HAS_DBACCESS(name) = 1`.
  - Optional env var `DatabaseList` (comma-separated database names) restricts results to those databases; parameters are used to avoid injection.
  - Results are returned as a plain-text table.

## Logging & Errors
- Logs to console; important events and failures are written with context.
- Startup fails (exit code 1) if `MSSQL_CONNECTION_STRING` is not set or the connection test fails.
- SQL errors are returned as `SQL Error: ...` with the underlying message when available.

## Development
- Build: `dotnet build`
- Run: `dotnet run --project src/MSSQLMCPServer`
- Test harness: `src/TestHarness` contains a small console app to invoke tools directly during local development.

## Container Image (optional)
This project enables .NET SDK container support. You can publish a local image with:

```
dotnet publish src/MSSQLMCPServer -c Release /t:PublishContainer \
  /p:ContainerRepository=mssql-mcp \
  /p:ContainerImageTags=latest
```

Then run the container (ensure the database is reachable from the container network):

```
docker run --rm -e MSSQL_CONNECTION_STRING="..." mssql-mcp:latest
```

## Notes & Limitations
- T-SQL only (SQL Server). Other dialects are not supported.
- Queries are executed as provided; exercise caution with DDL/DML operations.
- Output is plain text (ASCII table) for readability in chat contexts.

