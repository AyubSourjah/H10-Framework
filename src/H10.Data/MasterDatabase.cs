using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;

namespace H10.Data
{
    public class MasterDatabase : IDisposable
    {
        private DbConnection _masterConnection = null;
        private readonly DbProviderFactory _dbProviderFactory;
        private readonly IConfiguration _configuration;
        private bool _disposedValue;

        public MasterDatabase(IConfiguration configuration)
        {
            _configuration = configuration;
            _dbProviderFactory = DbProviderFactories.GetFactory(_configuration[SettingKeys.RepositoryProvider]);
            
            InitializeMasterConnection();
        }
        private void InitializeMasterConnection()
        {
            var credentials = new DatabaseCredentials(_dbProviderFactory)
            {
                Database = _configuration[SettingKeys.RepositoryCatalog],
                UserName = _configuration[SettingKeys.RepositoryUserName],
                Password = _configuration[SettingKeys.RepositoryPassword],
                Server = _configuration[SettingKeys.RepositoryServer]
            };
            
            _masterConnection = _dbProviderFactory.CreateConnection();
            
            Debug.Assert(_masterConnection != null, nameof(_masterConnection) + " cannot be null");
            _masterConnection.ConnectionString = credentials.BuildConnectionString();
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
                    _masterConnection.Close();
                    _masterConnection.Dispose();
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