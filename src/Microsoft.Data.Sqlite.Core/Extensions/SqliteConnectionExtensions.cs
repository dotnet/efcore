// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Data.Sqlite
{
    internal static class SqliteConnectionExtensions
    {
        public static int ExecuteNonQuery(
            this SqliteConnection connection,
            string commandText)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = commandText;

                return command.ExecuteNonQuery();
            }
        }

        public static T ExecuteScalar<T>(
            this SqliteConnection connection,
            string commandText)
            => (T)connection.ExecuteScalar(commandText);

        private static object ExecuteScalar(this SqliteConnection connection, string commandText)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = commandText;

                return command.ExecuteScalar();
            }
        }
    }
}
