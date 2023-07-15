// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Data.Sqlite;

SQLitePCL.Batteries_V2.Init();

using var connection = new SqliteConnection("Data Source=:memory:");

connection.Open();

var command = connection.CreateCommand();
command.CommandText = "SELECT 1; SELECT 2;";

using var reader = command.ExecuteReader();
var hasData = reader.Read();

if (reader.GetInt64(0) != 1L)
{
    return -1;
}

var hasResults = reader.NextResult();
hasData = reader.Read();
if (reader.GetInt64(0) != 2L)
{
    return -1;
}

if (reader.NextResult())
{
    return -1;
}

return 100;
