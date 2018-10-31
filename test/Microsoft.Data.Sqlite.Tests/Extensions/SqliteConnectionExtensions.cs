// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Data.Sqlite
{
    internal static class SqliteConnectionExtensions
    {
        public static SqliteDataReader ExecuteReader(this SqliteConnection connection, string commandText)
        {
            var command = connection.CreateCommand();
            command.CommandText = commandText;

            return command.ExecuteReader();
        }
    }
}
