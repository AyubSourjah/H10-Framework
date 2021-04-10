using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using H10.Data.Extensions;
using Microsoft.Extensions.Configuration;

namespace H10.Data
{
    public class DatabaseProvider : IDisposable
    {
        private DbConnection _masterConnection = null;
        private DbConnection _tenantConnection = null;

        private bool _disposedValue;

        private readonly IConfiguration _configuration;
        private readonly DbProviderFactory _dbProviderFactory;
        private readonly string _subDomain;

        public string UserDbUsername { get; private set; }
        public string UserDbPassword { get; private set; }

        public DatabaseProvider(IConfiguration configuration, string domain)
        {
            if (string.IsNullOrEmpty(domain))
                throw new ArgumentNullException(nameof(domain), "Domain cannot be null");

            _configuration = configuration;
            _subDomain = Shared.DomainNameHandler.GetSubDomain(value: domain);
            _dbProviderFactory = DbProviderFactories.GetFactory(_configuration[SettingKeys.RepositoryProvider]);
        }

        public void SetClaimsContext(System.Security.Claims.ClaimsPrincipal cp)
        {
            UserDbUsername = cp.Claims
                ?.FirstOrDefault(x => x.Type.Equals("DBUserName", StringComparison.OrdinalIgnoreCase))?.Value;
            UserDbPassword = cp.Claims
                ?.FirstOrDefault(x => x.Type.Equals("DBPassword", StringComparison.OrdinalIgnoreCase))?.Value;

            if (_tenantConnection != null)
            {
                _tenantConnection.Close();
                _tenantConnection.Dispose();
                _tenantConnection = null;   
            }
        }

        internal DbConnection GetTenantConnection()
        {
            if (_tenantConnection != null) return _tenantConnection;
            
            var cnn = GetMasterConnection();
            
            using var cmd = cnn.CreateCommand();

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "sp_GetClientRepositoryDetails";
            cmd.AddParameter("@paramDomain", DbType.String, _subDomain);

            var row = cmd.GetRows()?.SingleOrDefault();
            if (row == null)
                throw new InvalidOperationException("Tenant information not configured");

            var credentials = new DatabaseCredentials(_dbProviderFactory)
            {
                Database = row.Database,
                UserName = this.UserDbUsername ?? row.UserName,
                Password = this.UserDbPassword ?? row.Password,
                Server = row.Servername,
                Schema = row.Schema
            };

            _tenantConnection = _dbProviderFactory.CreateConnection();

            Debug.Assert(_tenantConnection != null, nameof(_tenantConnection) + " cannot be null");
            _tenantConnection.ConnectionString = credentials.BuildConnectionString();
            _tenantConnection.Open();

            return _tenantConnection;
        }

        internal DbConnection GetMasterConnection()
        {
            if (_masterConnection != null) return _masterConnection;

            var credentials = new DatabaseCredentials(_dbProviderFactory)
            {
                Database = _configuration[SettingKeys.RepositoryCatalog],
                UserName = _configuration[SettingKeys.RepositoryUserName],
                Password = _configuration[SettingKeys.RepositoryPassword],
                Server = _configuration[SettingKeys.RepositoryServer]
            };

            Debug.Assert(_dbProviderFactory != null, nameof(_dbProviderFactory) + " cannot be null");
            _masterConnection = _dbProviderFactory.CreateConnection();

            Debug.Assert(_masterConnection != null, nameof(_masterConnection) + " cannot be null");
            _masterConnection.ConnectionString = credentials.BuildConnectionString();
            _masterConnection.Open();

            return _masterConnection;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    if (_masterConnection != null)
                    {
                        _masterConnection.Close();
                        _masterConnection.Dispose();
                    }

                    if (_tenantConnection != null)
                    {
                        _tenantConnection.Close();
                        _tenantConnection.Dispose();
                    }
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