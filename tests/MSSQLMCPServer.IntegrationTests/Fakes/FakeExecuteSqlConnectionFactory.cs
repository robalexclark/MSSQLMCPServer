using MSSQLMCPServer.Database;
using MSSQLMCPServer.IntegrationTests.Fakes;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace MSSQLMCPServer.IntegrationTests.Tools
{
    public partial class SqlExecutionToolTests
    {
        private class FakeExecuteSqlConnectionFactory : IDbConnectionFactory
        {
            private readonly DataTable _result;
            private readonly int _rowsAffected;

            public FakeExecuteSqlConnectionFactory(DataTable result, int rowsAffected = 0)
            {
                _result = result;
                _rowsAffected = rowsAffected;
            }

            public DbConnection CreateConnection()
                => new FakeExecuteSqlConnection(_result, _rowsAffected);

            public Task<DbConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken = default)
                => Task.FromResult<DbConnection>(new FakeExecuteSqlConnection(_result, _rowsAffected));

            public Task<bool> ValidateConnectionAsync(CancellationToken cancellationToken = default)
                => Task.FromResult(true);
        }
    }
}