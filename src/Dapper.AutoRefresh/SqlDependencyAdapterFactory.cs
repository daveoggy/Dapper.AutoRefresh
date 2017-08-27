namespace Dapper.AutoRefresh
{
    public class SqlDependencyAdapterFactory : ISqlDependencyAdapterFactory
    {
        public ISqlDependencyAdapter Create(string connectionString)
        {
            return new SqlDependencyAdapter(connectionString);
        }
    }
}