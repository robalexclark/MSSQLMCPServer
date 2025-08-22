#nullable disable
using Microsoft.Extensions.Logging.Abstractions;
using MSSQLMCPServer.Database;
using MSSQLMCPServer.Tools;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MSSQLMCPServer.IntegrationTests.Tools
{
    public partial class SqlExecutionToolTests
    {
        [Fact]
        public async Task ListTables_ReturnsTablesFromMultipleDatabasesAndSchemas()
        {
            Dictionary<string, List<(string Schema, string Table)>> tables = new() 
            {
                ["DbOne"] = new() { ("dbo", "Users"), ("sales", "Orders") },
                ["DbTwo"] = new() { ("dbo", "Customers"), ("hr", "Employees") }
            };

            IDbConnectionFactory factory = new FakeSqlConnectionFactory(tables);
            SqlExecutionTool tool = new SqlExecutionTool(factory, NullLogger<SqlExecutionTool>.Instance);

            string result = await tool.ListTables();

            // Parse the tabular output into rows for easier assertions
            System.Collections.Generic.List<string[]> rows = result
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Skip(2) // Skip header and separator
                .Where(l => !l.StartsWith("("))
                .Select(l => l.Split('|').Select(c => c.Trim()).ToArray())
                .ToList();

            Assert.Contains(rows, r => r[0] == "DbOne" && r[1] == "dbo" && r[2] == "Users");
            Assert.Contains(rows, r => r[0] == "DbOne" && r[1] == "sales" && r[2] == "Orders");
            Assert.Contains(rows, r => r[0] == "DbTwo" && r[1] == "dbo" && r[2] == "Customers");
            Assert.Contains(rows, r => r[0] == "DbTwo" && r[1] == "hr" && r[2] == "Employees");

            Assert.Contains("DatabaseName", result);
            Assert.Contains("SchemaName", result);
            Assert.Contains("TableName", result);
        }

        [Fact]
        public async Task ListTables_WithDatabaseListEnvVar_FiltersDatabases()
        {
            Dictionary<string, List<(string Schema, string Table)>> tables = new() 
            {
                ["DbOne"] = new() { ("dbo", "Users") },
                ["DbTwo"] = new() { ("dbo", "Customers") },
                ["DbThree"] = new() { ("dbo", "Products") }
            };

            IDbConnectionFactory factory = new FakeSqlConnectionFactory(tables);
            SqlExecutionTool tool = new SqlExecutionTool(factory, NullLogger<SqlExecutionTool>.Instance);

            try
            {
                Environment.SetEnvironmentVariable("DatabaseList", "DbOne,DbTwo");

                string result = await tool.ListTables();

                System.Collections.Generic.List<string[]> rows = result
                    .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                    .Skip(2) // Skip header and separator
                    .Where(l => !l.StartsWith("("))
                    .Select(l => l.Split('|').Select(c => c.Trim()).ToArray())
                    .ToList();

                Assert.Contains(rows, r => r[0] == "DbOne" && r[2] == "Users");
                Assert.Contains(rows, r => r[0] == "DbTwo" && r[2] == "Customers");
                Assert.DoesNotContain(rows, r => r[0] == "DbThree");
            }
            finally
            {
                Environment.SetEnvironmentVariable("DatabaseList", null);
            }
        }

        [Fact]
        public async Task ExecuteSql_SelectQuery_ReturnsFormattedResults()
        {
            DataTable table = new DataTable();
            table.Columns.Add("Id", typeof(int));
            table.Columns.Add("Name", typeof(string));
            table.Rows.Add(1, "Alice");

            IDbConnectionFactory factory = new FakeExecuteSqlConnectionFactory(table);
            SqlExecutionTool tool = new SqlExecutionTool(factory, NullLogger<SqlExecutionTool>.Instance);

            string result = await tool.ExecuteSql("SELECT Id, Name FROM Users");

            string[] lines = result.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            string header = lines[0];
            string[] data = lines[2].Split('|').Select(c => c.Trim()).ToArray();

            Assert.Contains("Id", header);
            Assert.Contains("Name", header);
            Assert.Equal("1", data[0]);
            Assert.Equal("Alice", data[1]);
        }

        [Fact]
        public async Task ExecuteSql_NonSelectQuery_ReturnsRowsAffected()
        {
            IDbConnectionFactory factory = new FakeExecuteSqlConnectionFactory(new DataTable(), rowsAffected: 2);
            SqlExecutionTool tool = new SqlExecutionTool(factory, NullLogger<SqlExecutionTool>.Instance);

            string result = await tool.ExecuteSql("UPDATE Users SET Name = 'Bob' WHERE Id = 1");

            Assert.Equal("Query executed successfully. Rows affected: 2", result);
        }

        [Fact]
        public async Task ExecuteSql_InvalidQuery_ReturnsError()
        {
            IDbConnectionFactory factory = new FakeExecuteSqlConnectionFactory(new DataTable());
            SqlExecutionTool tool = new SqlExecutionTool(factory, NullLogger<SqlExecutionTool>.Instance);

            string result = await tool.ExecuteSql("Tell me all users");

            Assert.Contains("Invalid T-SQL syntax", result);
        }
    }
}