// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Sqlite.Internal;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Migrations;

public class SqliteHistoryRepositoryTest
{
    [ConditionalFact]
    public void GetCreateScript_works()
    {
        var sql = CreateHistoryRepository().GetCreateScript();

        Assert.Equal(
            """
CREATE TABLE "__EFMigrationsHistory" (
    "MigrationId" TEXT NOT NULL CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
    "ProductVersion" TEXT NOT NULL
);

""", sql, ignoreLineEndingDifferences: true);
    }

    [ConditionalFact]
    public void GetCreateIfNotExistsScript_works()
    {
        var sql = CreateHistoryRepository().GetCreateIfNotExistsScript();

        Assert.Equal(
            """
CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" TEXT NOT NULL CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
    "ProductVersion" TEXT NOT NULL
);

""", sql, ignoreLineEndingDifferences: true);
    }

    [ConditionalFact]
    public void GetDeleteScript_works()
    {
        var sql = CreateHistoryRepository().GetDeleteScript("Migration1");

        Assert.Equal(
            """
DELETE FROM "__EFMigrationsHistory"
WHERE "MigrationId" = 'Migration1';

""", sql, ignoreLineEndingDifferences: true);
    }

    [ConditionalFact]
    public void GetInsertScript_works()
    {
        var sql = CreateHistoryRepository().GetInsertScript(
            new HistoryRow("Migration1", "7.0.0"));

        Assert.Equal(
            """
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('Migration1', '7.0.0');

""", sql, ignoreLineEndingDifferences: true);
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
