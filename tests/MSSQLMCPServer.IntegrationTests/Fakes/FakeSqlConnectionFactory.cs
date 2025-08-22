using MSSQLMCPServer.Database;
using MSSQLMCPServer.IntegrationTests.Fakes;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;

namespace MSSQLMCPServer.IntegrationTests.Tools
{
    [ExcludeFromCodeCoverage]
    public partial class SqlExecutionToolTests
    {
        [ExcludeFromCodeCoverage]
        private class FakeSqlConnectionFactory : IDbConnectionFactory
        {
            private readonly IReadOnlyDictionary<string, List<(string Schema, string Table)>> _tables;

            public FakeSqlConnectionFactory(IReadOnlyDictionary<string, List<(string Schema, string Table)>> tables)
            {
                _tables = tables;
            }

            public DbConnection CreateConnection() => new FakeSqlConnection(_tables);

            public Task<DbConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken = default)
                => Task.FromResult<DbConnection>(new FakeSqlConnection(_tables));

            public Task<bool> ValidateConnectionAsync(CancellationToken cancellationToken = default)
                => Task.FromResult(true);
        }
    }
}
