using Microsoft.Extensions.Logging;
using MSSQLMCPServer.Database;
using MSSQLMCPServer.Tools;

namespace TestHarness;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        string? connectionString = Environment.GetEnvironmentVariable("MSSQL_CONNECTION_STRING");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            await Console.Error.WriteLineAsync("Error: MSSQL_CONNECTION_STRING environment variable is not set.");
            return 1;
        }

        using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .SetMinimumLevel(LogLevel.Information)
                .AddSimpleConsole(options =>
                {
                    options.IncludeScopes = false;
                    options.SingleLine = true;
                    options.TimestampFormat = "HH:mm:ss ";
                });
        });
        ILogger<SqlExecutionTool> logger = loggerFactory.CreateLogger<SqlExecutionTool>();

        IDbConnectionFactory connectionFactory = new SqlConnectionFactory(connectionString);
        SqlExecutionTool tool = new SqlExecutionTool(connectionFactory, logger);

        try
        {
            Console.WriteLine("Invoking ListTables()...\n");
            string result = await tool.ListTables();
            Console.WriteLine(result);
            return 0;
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Unhandled exception: {ex}");
            return 1;
        }
    }
}