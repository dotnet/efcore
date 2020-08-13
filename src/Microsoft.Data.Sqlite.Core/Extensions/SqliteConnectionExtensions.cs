// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

// ReSharper disable once CheckNamespace
namespace Microsoft.Data.Sqlite
{
    internal static class SqliteConnectionExtensions
    {
        public static int ExecuteNonQuery(
            this SqliteConnection connection,
            string commandText,
            params SqliteParameter[] parameters)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = commandText;
                command.Parameters.AddRange(parameters);

                return command.ExecuteNonQuery();
            }
        }

        public static T ExecuteScalar<T>(
            this SqliteConnection connection,
            string commandText,
            params SqliteParameter[] parameters)
            => (T)connection.ExecuteScalar(commandText, parameters);

        private static object ExecuteScalar(
            this SqliteConnection connection,
            string commandText,
            params SqliteParameter[] parameters)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = commandText;
                command.Parameters.AddRange(parameters);

                return command.ExecuteScalar();
            }
        }
    }
}
