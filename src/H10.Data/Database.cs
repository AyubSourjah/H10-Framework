using System;
using System.Data.Common;

namespace H10.Data
{
    public enum TargetEnvironment
    {
        Master = 0,
        Tenant = 1,
        User = 2
    }

    public class Database : IDisposable
    {
        private readonly DbConnection _tenantConnection;
        private readonly DbConnection _masterConnection;
        private bool _disposedValue;

        public Database(DatabaseProvider databaseProvider)
        {
            var tenantProvider = databaseProvider ?? throw new ArgumentNullException(nameof(databaseProvider));

            _masterConnection = tenantProvider.GetMasterConnection();
            _tenantConnection = tenantProvider.GetTenantConnection();
        }

        public DbCommand CreateCommand(TargetEnvironment targetEnvironment = TargetEnvironment.User)
        {
            DbCommand command = null;

            switch (targetEnvironment)
            {
                case TargetEnvironment.Master:
                    command = _masterConnection.CreateCommand();
                    break;
                case TargetEnvironment.Tenant:
                    command = _tenantConnection.CreateCommand();
                    break;
                case TargetEnvironment.User:
                    command = _tenantConnection.CreateCommand();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(targetEnvironment), targetEnvironment, null);
            }

            return command;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    //Tenant and Master database connections to be disposed by the Tenant Provider
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