using System;
using System.Data.Common;

namespace H10.Data
{
    public enum TargetEnvironment
    {
        Tenant = 0,
        Master = 1
    }
    public class TenantDatabase : IDisposable
    {
        private readonly TenantProvider _tenantProvider;
        private readonly DbConnection _tenantConnection;
        private readonly DbConnection _masterConnection;
        private bool _disposedValue;

        public TenantDatabase(TenantProvider tenantProvider)
        {
            _tenantProvider = tenantProvider ?? throw new ArgumentNullException(nameof(tenantProvider));
            _tenantConnection = tenantProvider.GetTenantConnection();
            _masterConnection = tenantProvider.GetMasterConnection();
        }
        public DbCommand CreateCommand(TargetEnvironment targetEnvironment = TargetEnvironment.Tenant)
        {
            return targetEnvironment == TargetEnvironment.Tenant ?
                _tenantConnection.CreateCommand() : _masterConnection.CreateCommand();
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _tenantProvider.Dispose();
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
