using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace H10.Data
{
    public class MasterDatabase : IDisposable
    {
        private DbConnection _masterConnection = null;
        private readonly DbProviderFactory _databaseFactory;
        private readonly IConfiguration _configuration;
        private bool _disposedValue;

        public MasterDatabase(IConfiguration configuration)
        {
            _configuration = configuration;
            _databaseFactory = DbProviderFactories.GetFactory(_configuration[SettingKeys.RepositoryProvider]);
            
            InitializeMasterConnection();
        }
        private void InitializeMasterConnection()
        {
            if (_masterConnection == null)
            {
                var repServer = _configuration[SettingKeys.RepositoryServer];
                var repCatalog = _configuration[SettingKeys.RepositoryCatalog];
                var repUserName = _configuration[SettingKeys.RepositoryUserName];
                var repPassword = _configuration[SettingKeys.RepositoryPassword];

                var dbConnectionStringBuilder = _databaseFactory?.CreateConnectionStringBuilder();
                if (dbConnectionStringBuilder == null)
                    throw new InvalidOperationException("Database provider not initialized");
                
                if (dbConnectionStringBuilder is SqlConnectionStringBuilder)
                {
                    dbConnectionStringBuilder[SqlKeys.Server] = repServer;
                    dbConnectionStringBuilder[SqlKeys.Database] = repCatalog;
                    dbConnectionStringBuilder[SqlKeys.UserName] = repUserName;
                    dbConnectionStringBuilder[SqlKeys.Password] = repPassword;
                    dbConnectionStringBuilder[SqlKeys.Trusted] = "True";

                    _masterConnection = _databaseFactory.CreateConnection();
                    // ReSharper disable once PossibleNullReferenceException
                    _masterConnection.ConnectionString = dbConnectionStringBuilder.ConnectionString;
                }
                else throw new NotSupportedException("Database provider not supported");
            }
        }
        public DbConnection GetConnection()
        {
            if (_masterConnection.State == ConnectionState.Closed)
                _masterConnection.Open();
            
            return _masterConnection;
        }
        public DbCommand CreateCommand()
        {
            if (_masterConnection.State == ConnectionState.Closed) 
                _masterConnection.Open();
                
            return _masterConnection.CreateCommand();
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _masterConnection?.Close();
                    _masterConnection?.Dispose();
                }

                _disposedValue = true;
            }
        }
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}