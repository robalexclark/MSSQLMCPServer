using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace MSSQLMCPServer.IntegrationTests.Fakes
{
    [ExcludeFromCodeCoverage]
    internal class FakeExecuteSqlCommand : DbCommand
    {
        private readonly DataTable _result;
        private readonly int _rowsAffected;

        public FakeExecuteSqlCommand(DataTable result, int rowsAffected)
        {
            _result = result;
            _rowsAffected = rowsAffected;
        }

        public override string CommandText { get; set; }
        public override int CommandTimeout { get; set; }
        public override CommandType CommandType { get; set; }
        public override bool DesignTimeVisible { get; set; }
        public override UpdateRowSource UpdatedRowSource { get; set; }
        protected override DbConnection DbConnection { get; set; }
        protected override DbParameterCollection DbParameterCollection { get; } = new FakeParameterCollection();
        protected override DbTransaction DbTransaction { get; set; }

        public override void Cancel() { }
        public override int ExecuteNonQuery() => _rowsAffected;
        public override object ExecuteScalar() => null;
        public override void Prepare() { }

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
            => _result.CreateDataReader();

        protected override DbParameter CreateDbParameter() => new FakeDbParameter();
    }
}
