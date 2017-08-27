namespace Dapper.AutoRefresh
{
    public interface ISqlDependencyAdapterFactory
    {
        ISqlDependencyAdapter Create(string connectionString);
    }
}