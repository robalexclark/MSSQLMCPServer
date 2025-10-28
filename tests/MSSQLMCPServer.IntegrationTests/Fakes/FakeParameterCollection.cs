using System;
using System.Data.Common;
using System.Linq;
using System.Diagnostics.CodeAnalysis;

namespace MSSQLMCPServer.IntegrationTests.Fakes
{
    [ExcludeFromCodeCoverage]
    internal class FakeParameterCollection : DbParameterCollection
    {
        private readonly System.Collections.Generic.List<DbParameter> _parameters = new System.Collections.Generic.List<DbParameter>();
        public override int Add(object value) { _parameters.Add((DbParameter)value); return _parameters.Count - 1; }
        public override void AddRange(Array values) { foreach (object v in values) _parameters.Add((DbParameter)v); }
        public override void Clear() { _parameters.Clear(); }
        public override bool Contains(object value) => _parameters.Contains(value);
        public override bool Contains(string value) => _parameters.Any(p => p.ParameterName == value);
        public override void CopyTo(Array array, int index) { _parameters.CopyTo((DbParameter[])array, index); }
        public override int Count => _parameters.Count;
        public override System.Collections.IEnumerator GetEnumerator() => _parameters.GetEnumerator();
        public override int IndexOf(object value) => _parameters.IndexOf((DbParameter)value);
        public override int IndexOf(string parameterName) => _parameters.FindIndex(p => p.ParameterName == parameterName);
        public override void Insert(int index, object value) { _parameters.Insert(index, (DbParameter)value); }
        public override bool IsFixedSize => false;
        public override bool IsReadOnly => false;
        public override bool IsSynchronized => false;
        public override void Remove(object value) { _parameters.Remove((DbParameter)value); }
        public override void RemoveAt(int index) { _parameters.RemoveAt(index); }
        public override void RemoveAt(string parameterName) { _parameters.RemoveAll(p => p.ParameterName == parameterName); }
        protected override DbParameter GetParameter(int index) => _parameters[index];
        protected override DbParameter GetParameter(string parameterName) => _parameters.First(p => p.ParameterName == parameterName);
        protected override void SetParameter(int index, DbParameter value) { _parameters[index] = value; }
        protected override void SetParameter(string parameterName, DbParameter value) { int index = IndexOf(parameterName); if (index >= 0) _parameters[index] = value; else _parameters.Add(value); }
        public override object SyncRoot => new object();
    }
}
