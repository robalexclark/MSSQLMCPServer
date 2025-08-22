using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace MSSQLMCPServer.IntegrationTests.Fakes
{
    internal class FakeSqlConnection : DbConnection
    {
        private readonly Queue<DataTable> _tables;
        private ConnectionState _state = ConnectionState.Closed;

        public FakeSqlConnection(IReadOnlyDictionary<string, List<(string Schema, string Table)>> tables)
        {
            DataTable dbTable = new DataTable();
            dbTable.Columns.Add("name", typeof(string));
            foreach (string db in tables.Keys)
            {
                dbTable.Rows.Add(db);
            }

            DataTable tableTable = new DataTable();
            tableTable.Columns.Add("DatabaseName", typeof(string));
            tableTable.Columns.Add("SchemaName", typeof(string));
            tableTable.Columns.Add("TableName", typeof(string));
            tableTable.Columns.Add("RowCount", typeof(int));
            tableTable.Columns.Add("TableType", typeof(string));
            foreach (KeyValuePair<string, List<(string Schema, string Table)>> kvp in tables)
            {
                foreach ((string schema, string table) in kvp.Value)
                {
                    tableTable.Rows.Add(kvp.Key, schema, table, 0, "BASE TABLE");
                }
            }

            _tables = new Queue<DataTable>(new[] { dbTable, tableTable });
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

        protected override DbCommand CreateDbCommand()
        {
            var table = _tables.Dequeue();
            return new FakeSqlCommand(table);
        }
    }
}
