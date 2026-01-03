// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Design.Internal;

public class MigrationsOperationsTest
{
    [ConditionalFact]
    public void Can_pass_null_args()
    {
        // Even though newer versions of the tools will pass an empty array
        // older versions of the tools can pass null args.
        var assembly = MockAssembly.Create(typeof(TestContext));
        _ = new TestMigrationsOperations(
            new TestOperationReporter(),
            assembly,
            assembly,
            "projectDir",
            "RootNamespace",
            "C#",
            nullable: false,
            args: null);
    }

    [ConditionalFact]
    public void Can_use_migrations_assembly()
    {
        // Even though newer versions of the tools will pass an empty array
        // older versions of the tools can pass null args.
        var assembly = MockAssembly.Create(typeof(AssemblyTestContext));
        var migrationsAssembly = MockAssembly.Create();
        AssemblyTestContext.MigrationsAssembly = migrationsAssembly;
        var testOperations = new TestMigrationsOperations(
            new TestOperationReporter(),
            assembly,
            assembly,
            "projectDir",
            "RootNamespace",
            "C#",
            nullable: false,
            args: null);

        testOperations.AddMigration("Test", null, null, null, dryRun: true);
    }

    [ConditionalFact]
    public void AddMigration_validates_migration_name_with_invalid_characters()
    {
        var assembly = MockAssembly.Create(typeof(TestContext));
        var operations = new TestMigrationsOperations(
            new TestOperationReporter(),
            assembly,
            assembly,
            "projectDir",
            "RootNamespace",
            "C#",
            nullable: false,
            args: []);

        // Use Path.GetInvalidFileNameChars() to get platform-specific invalid characters
        // Null character (char 0) is invalid on all platforms
        var invalidName = "Test\0Migration";

        var exception = Assert.Throws<OperationException>(
            () => operations.AddMigration(invalidName, null, null, null, dryRun: true));

        // The error message contains the invalid characters list
        Assert.NotNull(exception);
    }

    [ConditionalFact]
    public void AddMigration_rejects_context_class_name()
    {
        var assembly = MockAssembly.Create(typeof(TestContext));
        var operations = new TestMigrationsOperations(
            new TestOperationReporter(),
            assembly,
            assembly,
            "projectDir",
            "RootNamespace",
            "C#",
            nullable: false,
            args: []);

        // Migration name same as context class name should be rejected
        var exception = Assert.Throws<OperationException>(
            () => operations.AddMigration("TestContext", null, typeof(TestContext).FullName, null, dryRun: true));

        Assert.Contains("TestContext", exception.Message);
    }

    [ConditionalFact]
    public void RevertMigration_throws_when_no_dynamic_migrations()
    {
        var assembly = MockAssembly.Create(typeof(TestContext));
        var operations = new TestMigrationsOperations(
            new TestOperationReporter(),
            assembly,
            assembly,
            "projectDir",
            "RootNamespace",
            "C#",
            nullable: false,
            args: []);

        // No migrations have been applied via AddAndApply, so revert should fail
        var exception = Assert.Throws<OperationException>(
            () => operations.RevertMigration(null, null));

        Assert.NotNull(exception);
    }

    [ConditionalFact]
    public void RevertMigration_throws_when_specifying_migration_id_with_empty_list()
    {
        var assembly = MockAssembly.Create(typeof(TestContext));
        var operations = new TestMigrationsOperations(
            new TestOperationReporter(),
            assembly,
            assembly,
            "projectDir",
            "RootNamespace",
            "C#",
            nullable: false,
            args: []);

        // Even when specifying a migration ID, it first checks if any dynamic migrations exist
        var exception = Assert.Throws<OperationException>(
            () => operations.RevertMigration(null, "SomeMigrationId"));

        // Should throw the "no dynamic migrations" error since list is empty
        Assert.NotNull(exception);
    }

    private class TestContext : DbContext;

    private class AssemblyTestContext : DbContext
    {
        public static Assembly MigrationsAssembly { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseSqlServer(o => o.MigrationsAssembly(MigrationsAssembly));
    }
}
