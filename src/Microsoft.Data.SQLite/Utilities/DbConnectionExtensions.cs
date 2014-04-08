// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Data.Common;
using System.Diagnostics;

namespace Microsoft.Data.SQLite.Utilities
{
    internal static class DbConnectionExtensions
    {
        public static int ExecuteNonQuery(this DbConnection connection, string commandText)
        {
            Debug.Assert(connection != null, "connection is null.");
            Debug.Assert(!string.IsNullOrWhiteSpace(commandText), "commandText is null or empty.");

            using (var command = connection.CreateCommand())
            {
                command.CommandText = commandText;

                return command.ExecuteNonQuery();
            }
        }

        public static T ExecuteScalar<T>(this DbConnection connection, string commandText)
        {
            Debug.Assert(connection != null, "connection is null.");
            Debug.Assert(!string.IsNullOrWhiteSpace(commandText), "commandText is null or empty.");

            var value = connection.ExecuteScalar(commandText);
            if (!(value is T))
                return default(T);

            return (T)value;
        }

        private static object ExecuteScalar(this DbConnection connection, string commandText)
        {
            Debug.Assert(connection != null, "connection is null.");
            Debug.Assert(!string.IsNullOrWhiteSpace(commandText), "commandText is null or empty.");

            using (var command = connection.CreateCommand())
            {
                command.CommandText = commandText;

                return command.ExecuteScalar();
            }
        }
    }
}
