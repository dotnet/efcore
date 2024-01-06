// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Reflection;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Design;
using Microsoft.EntityFrameworkCore.SqlServer.TestModels.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore.SqlServer;

public class MigrationTests
{
    private delegate string MigrationCodeGetter(string migrationName, string rootNamespace);

    private delegate string SnapshotCodeGetter(string rootNamespace);

    [ConditionalFact]
    public void Migration_and_snapshot_generate_with_typed_array()
    {
        using var db = new TypedArraySeedContext();
        ValidateMigrationAndSnapshotCode(db, db.GetExpectedMigrationCode, db.GetExpectedSnapshotCode);
    }

    [ConditionalFact]
    public void Migration_and_snapshot_generate_with_anonymous_array()
    {
        using var db = new AnonymousArraySeedContext();
        ValidateMigrationAndSnapshotCode(db, db.GetExpectedMigrationCode, db.GetExpectedSnapshotCode);
    }

    private static void ValidateMigrationAndSnapshotCode(
        DbContext context,
        MigrationCodeGetter migrationCodeGetter,
        SnapshotCodeGetter snapshotCodeGetter)
    {
        const string migrationName = "MyMigration";
        const string rootNamespace = "MyApp.Data";

        var expectedMigration = migrationCodeGetter(migrationName, rootNamespace);
        var expectedSnapshot = snapshotCodeGetter(rootNamespace);

        var reporter = new OperationReporter(
            new OperationReportHandler(
                m => Console.WriteLine($"  error: {m}"),
                m => Console.WriteLine($"   warn: {m}"),
                m => Console.WriteLine($"   info: {m}"),
                m => Console.WriteLine($"verbose: {m}")));

        var assembly = Assembly.GetExecutingAssembly();

        //this works because we have placed the DesignTimeServicesReferenceAttribute
        //in the test project's properties, which simulates
        //the nuget package's build target
        var migration = new DesignTimeServicesBuilder(assembly, assembly, reporter, [])
            .Build(context)
            .GetRequiredService<IMigrationsScaffolder>()
            .ScaffoldMigration(migrationName, rootNamespace);

        Assert.Equal(expectedMigration, migration.MigrationCode, ignoreLineEndingDifferences: true);
        Assert.Equal(expectedSnapshot, migration.SnapshotCode, ignoreLineEndingDifferences: true);
    }
}
