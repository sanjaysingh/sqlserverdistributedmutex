using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlDistributedMutexPattern
{
    public class SqlDistributedMutex : IDisposable
    {
        private DbConnection dbConnection;
        private DbTransaction dbTransaction;
        private static readonly TimeSpan defaultTimeout = TimeSpan.FromSeconds(5);

        private SqlDistributedMutex(DbConnection dbConnection, DbTransaction dbTransaction)
        {
            this.dbConnection = dbConnection;
            this.dbTransaction = dbTransaction;
        }

        public static IDisposable TryAcquire(string connectionString, string lockName, TimeSpan timeout = default(TimeSpan))
        {
            if (timeout == default(TimeSpan))
            {
                timeout = defaultTimeout;
            }

            var connection = new SqlConnection(connectionString);
            connection.Open();
            var transaction = connection.BeginTransaction(IsolationLevel.ReadUncommitted);
            SqlParameter returnValue;

            using (var command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = "dbo.sp_getapplock";
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = (int)timeout.TotalMilliseconds;
                command.Parameters.AddWithValue("Resource", lockName);
                command.Parameters.AddWithValue("LockMode", "Exclusive");
                command.Parameters.AddWithValue("LockTimeout", 0);
                command.Parameters.Add(returnValue = new SqlParameter { Direction = ParameterDirection.ReturnValue });

                command.ExecuteNonQuery();

                var returnCode = (int)returnValue.Value;

                if (returnCode == 0)
                {
                   return new SqlDistributedMutex(connection, transaction);
                }
            }

            return null;
        }

        protected virtual void OnDispose(bool disposing)
        {
            if (this.dbTransaction != null)
            {
                this.dbTransaction.Dispose();
                this.dbTransaction = null;
            }
            if (this.dbConnection != null)
            {
                this.dbConnection.Dispose();
                this.dbConnection = null;
            }
        }

        public void Dispose()
        {
            this.OnDispose(true);
        }
    }
}
