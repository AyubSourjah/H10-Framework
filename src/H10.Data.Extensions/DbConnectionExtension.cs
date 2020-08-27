using System;
using System.Data;

namespace H10.Data.Extensions
{
    public static class DbConnectionExtension
    {
        public static void LogonSessionUser(this IDbConnection dbConnection)
        {
            if (dbConnection == null) throw new ArgumentNullException(nameof(dbConnection));

        }
    }
}