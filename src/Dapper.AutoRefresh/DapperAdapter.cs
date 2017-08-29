using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Dapper.AutoRefresh
{
    public class DapperAdapter : IDapperAdapter
    {
        public async Task<IEnumerable<TReturn>> QueryAsync<TReturn>(
            IDbConnection connection, 
            string sqlQuery, 
            object param, 
            IDbTransaction transaction,
            int? commandTimeout, 
            CommandType? commandType)
        {
            return await connection.QueryAsync<TReturn>(
                sqlQuery, 
                param, 
                transaction, 
                commandTimeout, 
                commandType);
        }

        public async Task<IEnumerable<TReturn>> QueryAsync<TReturn>(IDbConnection connection, CommandDefinition commandDefinition)
        {
            return await connection.QueryAsync<TReturn>(commandDefinition);
        }
    }
}