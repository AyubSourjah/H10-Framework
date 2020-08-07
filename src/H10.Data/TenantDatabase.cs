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
        private bool disposedValue;

        public TenantDatabase(TenantProvider tenantProvider)
        {
            this._tenantProvider = tenantProvider;
            this._tenantConnection = tenantProvider.GetTenantConnection();
            this._masterConnection = tenantProvider.GetMasterConnection();
        }
        public DbCommand CreateCommand(TargetEnvironment targetEnvironment = TargetEnvironment.Tenant)
        {
            return targetEnvironment == TargetEnvironment.Tenant ?
                this._tenantConnection.CreateCommand() : this._masterConnection.CreateCommand();
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    this._tenantProvider.Dispose();
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
