using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace H10.Data
{
    public class TenantProvider : IDisposable
    {
        private DbConnection _masterConnection = null;
        private DbConnection _tenantConnection = null;
        private bool disposedValue;
        private readonly IConfiguration _configuration;
        private readonly DbProviderFactory _databaseFactory;
        private readonly string _subDomain;
        private readonly string _tenantConnectionString = string.Empty;

        public TenantProvider(IConfiguration configuration, string domain)
        {
            if (string.IsNullOrEmpty(domain))
                throw new ArgumentNullException("Domain name argument cannot be null or empty");

            this._configuration = configuration;
            this._subDomain = H10.Shared.DomainNameHandler.GetSubDomain(value: domain);
            this._databaseFactory = DbProviderFactories.GetFactory(_configuration[SettingKeys.RepositoryProvider]);
            this._tenantConnectionString = this.GetTenantConnectionString();
        }
        internal DbConnection GetTenantConnection()
        {
            if (_tenantConnection == null)
            {
                _tenantConnection = _databaseFactory.CreateConnection();
                _tenantConnection.ConnectionString = _tenantConnectionString;
            }

            return _tenantConnection;
        }
        internal DbConnection GetMasterConnection()
        {
            if (_masterConnection == null)
            {
                string repServer = _configuration[SettingKeys.RepositoryServer];
                string repCatalog = _configuration[SettingKeys.RepositoryCatalog];
                string repUserName = _configuration[SettingKeys.RepositoryUserName];
                string repPassword = _configuration[SettingKeys.RepositoryPassword];

                var dbConnectionStringBuilder = _databaseFactory.CreateConnectionStringBuilder();

                if (dbConnectionStringBuilder.GetType() == typeof(SqlConnectionStringBuilder))
                {
                    dbConnectionStringBuilder[SqlKeys.Server] = repServer;
                    dbConnectionStringBuilder[SqlKeys.Database] = repCatalog;
                    dbConnectionStringBuilder[SqlKeys.UserName] = repUserName;
                    dbConnectionStringBuilder[SqlKeys.Password] = repPassword;
                    dbConnectionStringBuilder[SqlKeys.Trusted] = "True";

                    _masterConnection = _databaseFactory.CreateConnection();
                    _masterConnection.ConnectionString = dbConnectionStringBuilder.ConnectionString;

                    return _masterConnection;
                }
                else throw new NotSupportedException("Database provider not supported");
            }

            return _masterConnection;
        }
        private string GetTenantConnectionString()
        {
            if (String.IsNullOrEmpty(_tenantConnectionString) == true)
            {
                using var cnn = this.GetMasterConnection();
                cnn.Open();

                using var cmd = cnn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_GetClientRepositoryDetails";

                using var reader = cmd.ExecuteReader();

                if (reader.Read() == true)
                {

                }

                cnn.Close();
            }

            return _tenantConnectionString;
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _masterConnection.Dispose();
                    _tenantConnection.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
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