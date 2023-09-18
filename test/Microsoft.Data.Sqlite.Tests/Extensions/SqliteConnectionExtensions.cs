// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.Data.Sqlite;

internal static class SqliteConnectionExtensions
{
    public static SqliteDataReader ExecuteReader(
        this SqliteConnection connection,
        string commandText,
        params SqliteParameter[] parameters)
    {
        var command = connection.CreateCommand();
        command.CommandText = commandText;
        command.Parameters.AddRange(parameters);

        return command.ExecuteReader();
    }
}
