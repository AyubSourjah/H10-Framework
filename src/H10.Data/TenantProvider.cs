using System;
using System.Collections;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using H10.Data.Extensions;
using Microsoft.Extensions.Configuration;

namespace H10.Data
{
	public class TenantProvider : IDisposable
	{
		private DbConnection _masterConnection = null;
		private DbConnection _tenantConnection = null;
		private bool _disposedValue;
		private readonly IConfiguration _configuration;
		private readonly DbProviderFactory _dbProviderFactory;
		private readonly string _subDomain;

		public TenantProvider(IConfiguration configuration, string domain)
		{
			if (string.IsNullOrEmpty(domain))
				throw new ArgumentNullException("Domain name argument cannot be null or empty");

			_configuration = configuration;
			_subDomain = Shared.DomainNameHandler.GetSubDomain(value: domain);
			_dbProviderFactory = DbProviderFactories.GetFactory(_configuration[SettingKeys.RepositoryProvider]);
		}
		internal DbConnection GetTenantConnection()
		{
			if (_tenantConnection != null)
				return _tenantConnection;
			
			using var cnn = GetMasterConnection();
			using var cmd = cnn.CreateCommand();
				
			cmd.CommandType = CommandType.StoredProcedure;
			cmd.CommandText = "sp_GetClientRepositoryDetails";
			cmd.AddParameter("Domain", DbType.String, _subDomain);

			var row = cmd.GetRows()?.SingleOrDefault();
			if (row == null)
				throw new InvalidOperationException("Tenant information not configured");

			var credentials = new DatabaseCredentials(_dbProviderFactory)
			{
				Database = row["Database"],
				UserName = row["UserName"],
				Password = row["Password"],
				Server = row["ServerName"]
			};

			_tenantConnection = _dbProviderFactory.CreateConnection();
			
			Debug.Assert(_tenantConnection != null, nameof(_tenantConnection) + " != null");
			_tenantConnection.ConnectionString = credentials.BuildConnectionString();
			
			return _tenantConnection;
		}
		internal DbConnection GetMasterConnection()
		{
			if (_masterConnection != null)
				return _masterConnection;
			
			var credentials = new DatabaseCredentials(_dbProviderFactory)
			{
				Database = _configuration[SettingKeys.RepositoryCatalog],
				UserName = _configuration[SettingKeys.RepositoryUserName],
				Password = _configuration[SettingKeys.RepositoryPassword],
				Server = _configuration[SettingKeys.RepositoryServer]
			};

			Debug.Assert(_dbProviderFactory != null, nameof(_dbProviderFactory) + " != null");
			_masterConnection = _dbProviderFactory.CreateConnection();
			
			Debug.Assert(_masterConnection != null, nameof(_masterConnection) + " != null");
			_masterConnection.ConnectionString = credentials.BuildConnectionString();
			
			return _masterConnection;
		}
		protected virtual void Dispose(bool disposing)
		{
			if (!_disposedValue)
			{
				if (disposing)
				{
					_masterConnection.Close();
					_masterConnection.Dispose();
					_tenantConnection.Close();
					_tenantConnection.Dispose();
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