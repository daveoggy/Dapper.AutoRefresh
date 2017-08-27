using System.Collections.Concurrent;
using System.Data.SqlClient;

namespace Dapper.AutoRefresh
{
    public class SqlDependencyAdapter : ISqlDependencyAdapter
    {
        private static readonly ConcurrentDictionary<string, bool> ActiveConnections;
        private readonly string _connectionString;
        private readonly SqlDependency _sqlDependency;
        static SqlDependencyAdapter()
        {
            ActiveConnections = new ConcurrentDictionary<string, bool>();
        }

        public SqlDependencyAdapter(string connectionString)
        {
            _connectionString = connectionString;
            if (ActiveConnections.ContainsKey(_connectionString))
            {
                SqlDependency.Start(_connectionString);
            }

            _sqlDependency = new SqlDependency();
        }

        public bool HasChanges => _sqlDependency.HasChanges;

        public event OnChangeEventHandler OnChange
        {
            add => _sqlDependency.OnChange += value;
            remove => _sqlDependency.OnChange -= value;
        }

        public void AddCommandDependency(SqlCommand command)
        {
            _sqlDependency.AddCommandDependency(command);
        }

        public void Dispose()
        {
            SqlDependency.Stop(_connectionString);
            bool removed;
            do
            {
                ActiveConnections.TryRemove(_connectionString, out removed);
            } while (!removed);
        }
    }
}
