// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Sqlite.Internal;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Migrations;

public class SqliteHistoryRepositoryTest
{
    private static string EOL
        => Environment.NewLine;

    [ConditionalFact]
    public void GetCreateScript_works()
    {
        var sql = CreateHistoryRepository().GetCreateScript();

        Assert.Equal(
            "CREATE TABLE \"__EFMigrationsHistory\" ("
            + EOL
            + "    \"MigrationId\" TEXT NOT NULL CONSTRAINT \"PK___EFMigrationsHistory\" PRIMARY KEY,"
            + EOL
            + "    \"ProductVersion\" TEXT NOT NULL"
            + EOL
            + ");"
            + EOL,
            sql);
    }

    [ConditionalFact]
    public void GetCreateIfNotExistsScript_works()
    {
        var sql = CreateHistoryRepository().GetCreateIfNotExistsScript();

        Assert.Equal(
            "CREATE TABLE IF NOT EXISTS \"__EFMigrationsHistory\" ("
            + EOL
            + "    \"MigrationId\" TEXT NOT NULL CONSTRAINT \"PK___EFMigrationsHistory\" PRIMARY KEY,"
            + EOL
            + "    \"ProductVersion\" TEXT NOT NULL"
            + EOL
            + ");"
            + EOL,
            sql);
    }

    [ConditionalFact]
    public void GetDeleteScript_works()
    {
        var sql = CreateHistoryRepository().GetDeleteScript("Migration1");

        Assert.Equal(
            "DELETE FROM \"__EFMigrationsHistory\"" + EOL + "WHERE \"MigrationId\" = 'Migration1';" + EOL,
            sql);
    }

    [ConditionalFact]
    public void GetInsertScript_works()
    {
        var sql = CreateHistoryRepository().GetInsertScript(
            new HistoryRow("Migration1", "7.0.0"));

        Assert.Equal(
            "INSERT INTO \"__EFMigrationsHistory\" (\"MigrationId\", \"ProductVersion\")"
            + EOL
            + "VALUES ('Migration1', '7.0.0');"
            + EOL,
            sql);
    }

    [ConditionalFact]
    public void GetBeginIfNotExistsScript_works()
    {
        var repository = CreateHistoryRepository();
        var ex = Assert.Throws<NotSupportedException>(() => repository.GetBeginIfNotExistsScript("Migration1"));

        Assert.Equal(SqliteStrings.MigrationScriptGenerationNotSupported, ex.Message);
    }

    [ConditionalFact]
    public void GetBeginIfExistsScript_works()
    {
        var repository = CreateHistoryRepository();
        var ex = Assert.Throws<NotSupportedException>(() => repository.GetBeginIfExistsScript("Migration1"));

        Assert.Equal(SqliteStrings.MigrationScriptGenerationNotSupported, ex.Message);
    }

    [ConditionalFact]
    public void GetEndIfScript_works()
    {
        var repository = CreateHistoryRepository();
        var ex = Assert.Throws<NotSupportedException>(() => repository.GetEndIfScript());

        Assert.Equal(SqliteStrings.MigrationScriptGenerationNotSupported, ex.Message);
    }

    private static IHistoryRepository CreateHistoryRepository()
        => SqliteTestHelpers.Instance.CreateContextServices()
            .GetRequiredService<IHistoryRepository>();
}
