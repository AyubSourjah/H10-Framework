using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;

namespace H10.Data.Extensions
{
    public static class DbCommandExtension
    {
        public static void AddParameter(this IDbCommand dbCommand, string parameterName, 
            DbType parameterType, object value)
        {
            if (dbCommand == null) throw new ArgumentNullException(nameof(dbCommand));

            var parameter = dbCommand.CreateParameter();
            parameter.ParameterName = parameterName;
            parameter.DbType = parameterType;
            parameter.Value = value;
            parameter.Direction = ParameterDirection.Input;

            dbCommand.Parameters.Add(parameter);
        }
        public static IEnumerable<dynamic> GetRows(this IDbCommand dbCommand)
        {
            if (dbCommand == null) throw new ArgumentNullException(nameof(dbCommand));
            
            using var reader = dbCommand.ExecuteReader();
            var names = Enumerable.Range(0, reader.FieldCount).Select(reader.GetName).ToList();

            foreach (IDataRecord record in (IEnumerable) reader)
            {
                var expando = new ExpandoObject() as IDictionary<string, object>;
                foreach (var name in names)
                    expando[name] = record[name];

                yield return expando;
            }
        }
    }
}