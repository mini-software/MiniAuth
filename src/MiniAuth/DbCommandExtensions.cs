using System.Collections.Generic;
using System;
using System.Data.Common;
namespace MiniAuth
{
    internal static class DbCommandExtensions
    {
        public static void AddParameters(this DbCommand command, IDictionary<string, object> parameters)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));

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
