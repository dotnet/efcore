// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Data.Sqlite;
using SQLitePCL;

Batteries_V2.Init();

using var connection = new SqliteConnection("Data Source=:memory:");

connection.Open();

var command = connection.CreateCommand();
command.CommandText = "SELECT 1; SELECT 2;";

using var reader = command.ExecuteReader();
var schema = reader.GetSchemaTable();

if (schema.Rows.Count != 1)
{
    return -1;
}

if ((Type)schema.Rows[0]["DataType"] != typeof(long))
{
    return -2;
}

if ((string)schema.Rows[0]["DataTypeName"] != "INTEGER")
{
    return -3;
}

var hasData = reader.Read();

if (reader.GetInt64(0) != 1L)
{
    return -4;
}

var hasResults = reader.NextResult();
hasData = reader.Read();
if (reader.GetInt64(0) != 2L)
{
    return -5;
}

if (reader.NextResult())
{
    return -6;
}

return 100;
