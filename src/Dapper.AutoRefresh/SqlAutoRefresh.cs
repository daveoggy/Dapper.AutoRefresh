using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Dapper.AutoRefresh
{
    public class SqlAutoRefresh<TReturn>
    {
        private readonly string _sqlQuery;
        private readonly string _connectionString;
        private readonly CancellationToken _cancellationToken;
        private readonly SqlDependencyAsync _sqlDependencyAsync;

        public SqlAutoRefresh(string sqlQuery, string connectionString)
        {
            _sqlQuery = sqlQuery;
            _connectionString = connectionString;
            _cancellationToken = CancellationToken.None;
            _sqlDependencyAsync = new SqlDependencyAsync(_cancellationToken);
        }

        public SqlAutoRefresh(string sqlQuery, string connectionString, CancellationToken cancellationToken)
        {
            _sqlQuery = sqlQuery;
            _connectionString = connectionString;
            _cancellationToken = cancellationToken;
            _sqlDependencyAsync = new SqlDependencyAsync(_cancellationToken);
        }

        public async Task<ICollection<TReturn>> GetLatest()
        {
            ICollection<TReturn> collection = null;
            while (collection == null)
            {
                try
                {
                    await _sqlDependencyAsync;

                    _cancellationToken.ThrowIfCancellationRequested();

                    using (var sqlConnection = new SqlConnection(_connectionString))
                    using (var connection = new SqlConnectionWithDependency(sqlConnection, _sqlDependencyAsync.SqlDependency))
                    {
                        // Dapper uses an array for the results when not setting buffered=true; which we aren't
                        collection = await connection.QueryAsync<TReturn>(_sqlQuery, _cancellationToken)
                            as ICollection<TReturn>;

                        return collection;
                    }
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.Message);
                }

                _sqlDependencyAsync.Reset();
                await Task.Delay(TimeSpan.FromSeconds(5), _cancellationToken);
            }

            return collection;
        }

        /// <summary>
        /// The sole purpose is to intercept the <see cref="DbConnection.CreateDbCommand()"/> call
        /// and associate it with the <see cref="SqlDependency"/> before Dapper executes the command
        /// </summary>
        private class SqlConnectionWithDependency : DbConnection
        {
            private readonly SqlConnection _sqlConnection;
            private readonly SqlDependency _sqlDependency;

            public SqlConnectionWithDependency(SqlConnection sqlConnection, SqlDependency sqlDependency)
            {
                _sqlConnection = sqlConnection;
                _sqlDependency = sqlDependency;
            }

            protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
            {
                return _sqlConnection.BeginTransaction(isolationLevel);
            }

            public override void Close()
            {
                _sqlConnection.Close();
            }

            public override void ChangeDatabase(string databaseName)
            {
                _sqlConnection.ChangeDatabase(databaseName);
            }

            public override void Open()
            {
                _sqlConnection.Open();
            }

            public override string ConnectionString
            {
                get { return _sqlConnection.ConnectionString; }
                set { _sqlConnection.ConnectionString = value; }
            }

            public override string Database => _sqlConnection.Database;

            public override ConnectionState State => _sqlConnection.State;

            public override string DataSource => _sqlConnection.DataSource;

            public override string ServerVersion => _sqlConnection.ServerVersion;

            protected override DbCommand CreateDbCommand()
            {
                var command = _sqlConnection.CreateCommand();
                _sqlDependency.AddCommandDependency(command);
                return command;
            }
        }

        private class SqlDependencyAsync : INotifyCompletion
        {
            private Action _continuation;

            public SqlDependencyAsync(CancellationToken cancellationToken)
            {
                cancellationToken.Register(() => OnChangeHandler(null, null));
            }

            public SqlDependency SqlDependency { get; private set; }

            // ReSharper disable once UnusedMethodReturnValue.Local
            // Used via reflection
            public SqlDependencyAsync GetAwaiter()
            {
                return this;
            }

            private void OnChangeHandler(object _, SqlNotificationEventArgs e)
            {
                if (e.Source == SqlNotificationSource.Timeout)
                {
                    throw new TimeoutException();
                }

                if (e.Source != SqlNotificationSource.Data)
                {
                    Console.WriteLine("Unhandled change notification {0}/{1} ({2})", e.Type, e.Info, e.Source);
                    throw new InvalidOperationException($"SqlNotificationEventArgs represent and unknown state: Source: {e.Source} | Info: {e.Info} | Type: {e.Type}");
                }

                Console.WriteLine("Notification Info: " + e.Info);
                Console.WriteLine("Notification source: " + e.Source);
                Console.WriteLine("Notification type: " + e.Type);

                SqlDependency.OnChange -= OnChangeHandler;

                _continuation();
            }

            public void OnCompleted(Action continuation)
            {
                SqlDependency.OnChange += OnChangeHandler;
                _continuation = continuation;
            }

            public bool IsCompleted => SqlDependency?.HasChanges ?? true;

            public void GetResult()
            {
                SqlDependency = new SqlDependency();
            }

            public void Reset()
            {
                SqlDependency = null;
            }
        }
    }
}