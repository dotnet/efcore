// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.Common;

namespace Microsoft.Data.Sqlite.Utilities
{
    internal static class DbConnectionExtensions
    {
        public static int ExecuteNonQuery(
            this DbConnection connection,
            string commandText,
            int timeout = SqliteCommand.DefaultCommandTimeout)
        {
            var command = connection.CreateCommand();
            command.CommandTimeout = timeout;
            command.CommandText = commandText;

            return command.ExecuteNonQuery();
        }

        public static T ExecuteScalar<T>(
            this DbConnection connection,
            string commandText,
            int timeout = SqliteCommand.DefaultCommandTimeout)
            => (T)connection.ExecuteScalar(commandText, timeout);

        private static object ExecuteScalar(this DbConnection connection, string commandText, int timeout)
        {
            var command = connection.CreateCommand();
            command.CommandTimeout = timeout;
            command.CommandText = commandText;

            return command.ExecuteScalar();
        }
    }
}
