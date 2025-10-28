using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace MSSQLMCPServer.Database;

public interface IDbConnectionFactory
{
    DbConnection CreateConnection();
    Task<DbConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken = default);
    Task<bool> ValidateConnectionAsync(CancellationToken cancellationToken = default);
}
