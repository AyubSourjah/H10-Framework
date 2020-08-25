using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Dynamic;
using System.Linq;

namespace H10.Data.Extensions
{
    public static class DbCommandExtension
    {
        public static void AddParameter(this DbCommand dbCommand, string parameterName, 
            DbType parameterType, object value)
        {
            var parameter = dbCommand.CreateParameter();
            parameter.ParameterName = parameterName;
            parameter.DbType = parameterType;
            parameter.Value = value;
            parameter.Direction = ParameterDirection.Input;
        }
        public static IEnumerable<dynamic> GetRows(this DbCommand dbCommand)
        {
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