using System.Data;
using System.Data.Common;
using System.Linq;
using System.Diagnostics.CodeAnalysis;

namespace MSSQLMCPServer.IntegrationTests.Fakes
{
    [ExcludeFromCodeCoverage]
    internal class FakeSqlCommand : DbCommand
    {
        private readonly DataTable _table;

        public FakeSqlCommand(DataTable table)
        {
            _table = table;
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
        public override int ExecuteNonQuery() => 0;
        public override object ExecuteScalar() => null;
        public override void Prepare() { }

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            if (DbParameterCollection.Count > 0)
            {
                DataTable filteredTable = _table.Clone();
                System.Collections.Generic.HashSet<string> parameterValues = new System.Collections.Generic.HashSet<string>(DbParameterCollection.Cast<DbParameter>().Select(p => p.Value.ToString()));
                foreach (DataRow row in _table.Rows)
                {
                    if (parameterValues.Contains(row[0].ToString()))
                    {
                        filteredTable.ImportRow(row);
                    }
                }
                return filteredTable.CreateDataReader();
            }
            else if (CommandText.Contains("UNION ALL"))
            {
                DataTable filteredTable = _table.Clone();
                System.Collections.Generic.List<string> databases = new System.Collections.Generic.List<string>();
                System.Text.RegularExpressions.MatchCollection matches = System.Text.RegularExpressions.Regex.Matches(CommandText, @"SELECT '([^']*)' AS DatabaseName");
                foreach (System.Text.RegularExpressions.Match match in matches)
                {
                    databases.Add(match.Groups[1].Value);
                }

                foreach (DataRow row in _table.Rows)
                {
                    if (databases.Contains(row["DatabaseName"].ToString()))
                    {
                        filteredTable.ImportRow(row);
                    }
                }
                return filteredTable.CreateDataReader();
            }
            return _table.CreateDataReader();
        }

        protected override DbParameter CreateDbParameter()
            => new FakeDbParameter();
    }
}
