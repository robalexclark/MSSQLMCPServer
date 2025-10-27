using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;

namespace MSSQLMCPServer.IntegrationTests.Fakes
{
    [ExcludeFromCodeCoverage]
    internal class FakeExecuteSqlConnection : DbConnection
    {
        private readonly DataTable _result;
        private readonly int _rowsAffected;
        private ConnectionState _state = ConnectionState.Closed;

        public FakeExecuteSqlConnection(DataTable result, int rowsAffected)
        {
            _result = result;
            _rowsAffected = rowsAffected;
        }

        public override string ConnectionString { get; set; } = string.Empty;
        public override string Database => "master";
        public override string DataSource => "in-memory";
        public override string ServerVersion => "1.0";
        public override ConnectionState State => _state;

        public override void ChangeDatabase(string databaseName) { }
        public override void Close() => _state = ConnectionState.Closed;
        public override void Open() => _state = ConnectionState.Open;
        public override Task OpenAsync(CancellationToken cancellationToken)
        {
            _state = ConnectionState.Open;
            return Task.CompletedTask;
        }

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel) => throw new NotImplementedException();

        protected override DbCommand CreateDbCommand() => new FakeExecuteSqlCommand(_result, _rowsAffected);
    }
}
