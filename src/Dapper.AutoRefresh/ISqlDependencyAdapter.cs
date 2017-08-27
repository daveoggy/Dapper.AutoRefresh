using System;
using System.Data.Common;
using System.Data.SqlClient;

namespace Dapper.AutoRefresh
{
    public interface ISqlDependencyAdapter : IDisposable
    {
        event OnChangeEventHandler OnChange;
        bool HasChanges { get; }
        void AddCommandDependency(SqlCommand command);
    }
}