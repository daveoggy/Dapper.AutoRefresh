using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Dapper.AutoRefresh
{
    public interface IDapperAdapter
    {
        Task<IEnumerable<TReturn>> QueryAsync<TReturn>(
            IDbConnection connection,
            string sqlQuery,
            object param,
            IDbTransaction transaction,
            int? commandTimeout,
            CommandType? commandType);

        Task<IEnumerable<TReturn>> QueryAsync<TReturn>(IDbConnection connection, CommandDefinition commandDefinition);
    }
}