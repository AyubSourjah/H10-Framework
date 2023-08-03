using System;
using System.Data.Common;
using System.Data.SqlClient;

namespace H10.Data
{
    public class DatabaseCredentials
    {
        private readonly DbProviderFactory _dbProviderFactory;

        public DatabaseCredentials(DbProviderFactory dbProviderFactory)
        {
            _dbProviderFactory = dbProviderFactory ?? throw new ArgumentNullException(nameof(dbProviderFactory));
        }
        public string? Server { get; set; }
        public string? UserName { get; set; }
        public string? Password { get; set; }
        public string? Database { get; set; }
        public string Schema { get; set; }
        public string BuildConnectionString()
        {
            var dbConnectionStringBuilder = _dbProviderFactory.CreateConnectionStringBuilder();
            if (dbConnectionStringBuilder is SqlConnectionStringBuilder)
            {
                dbConnectionStringBuilder[SqlKeys.Server] = this.Server;
                dbConnectionStringBuilder[SqlKeys.Database] = this.Database;
                dbConnectionStringBuilder[SqlKeys.UserName] = this.UserName;
                dbConnectionStringBuilder[SqlKeys.Password] = this.Password;
                //dbConnectionStringBuilder[SqlKeys.Trusted] = "True";

                return dbConnectionStringBuilder.ConnectionString;
            }
            else throw new NotSupportedException("Database provider not supported");
        }
    }
}