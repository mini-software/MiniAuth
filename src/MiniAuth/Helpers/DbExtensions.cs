using System.Collections.Generic;
using System;
using System.Data.Common;
namespace MiniAuth.Helpers
{
    internal static class DbExtensions
    {
        public static int ExecuteNonQuery(this DbConnection connection, string sql, IDictionary<string, object> parameters = null)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));
            if (parameters == null) parameters = new Dictionary<string, object>();
            var command = connection.CreateCommand();
            command.CommandText = sql;
            command.AddParameters(parameters);
            return command.ExecuteNonQuery();
        }
        public static void AddParameters(this DbCommand command, IDictionary<string, object> parameters = null)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));
            if (parameters == null) parameters = new Dictionary<string, object>();
            foreach (var param in parameters)
            {
                DbParameter dbParam = command.CreateParameter();
                dbParam.ParameterName = param.Key;
                dbParam.Value = param.Value ?? DBNull.Value;
                command.Parameters.Add(dbParam);
            }
        }
    }
}
