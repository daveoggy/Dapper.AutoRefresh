using System.Data;

namespace Dapper.AutoRefresh
{
    public interface IDbConnectionFactory
    {
        IDbConnection Create(string connectionString, ISqlDependencyAdapter sqlDependencyAdapter);
    }
}