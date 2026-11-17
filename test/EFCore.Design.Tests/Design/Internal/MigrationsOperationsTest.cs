// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Design.Internal;

public class MigrationsOperationsTest
{
    [Fact]
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

    [Fact]
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

    [Fact]
    public void AddMigration_throws_when_name_is_empty()
    {
        var assembly = MockAssembly.Create(typeof(AssemblyTestContext));
        var operations = new TestMigrationsOperations(
            new TestOperationReporter(),
            assembly,
            assembly,
            "projectDir",
            "RootNamespace",
            "C#",
            nullable: false,
            args: []);

        var exception = Assert.Throws<OperationException>(() => operations.AddMigration("", null, null, null, dryRun: true));

        Assert.Equal(DesignStrings.MigrationNameRequired, exception.Message);
    }

    [Fact]
    public void AddMigration_throws_when_name_is_whitespace()
    {
        var assembly = MockAssembly.Create(typeof(AssemblyTestContext));
        var operations = new TestMigrationsOperations(
            new TestOperationReporter(),
            assembly,
            assembly,
            "projectDir",
            "RootNamespace",
            "C#",
            nullable: false,
            args: []);

        var exception = Assert.Throws<OperationException>(() => operations.AddMigration("   ", null, null, null, dryRun: true));

        Assert.Equal(DesignStrings.MigrationNameRequired, exception.Message);
    }

    // Note: the single-context tests below are guards for the early return from #34082; the multi-context tests
    // exercise the best-effort service provider discovery.
    // Regression tests for https://github.com/dotnet/efcore/issues/25053: when design-time factories supply the
    // contexts, a broken application service provider (e.g. missing connection string) must not break the tools.
    [Fact]
    public void ScriptMigration_uses_design_time_factory_when_service_provider_throws()
    {
        var operations = CreateOperationsWithThrowingHost(typeof(FactoryContext), typeof(FactoryContextFactory));

        Assert.NotNull(operations.ScriptMigration(null, null, MigrationsSqlGenerationOptions.Default, contextType: null));
        Assert.NotNull(operations.ScriptMigration(null, null, MigrationsSqlGenerationOptions.Default, nameof(FactoryContext)));
        Assert.NotNull(operations.ScriptMigration(null, null, MigrationsSqlGenerationOptions.Default, contextType: "*"));
    }

    [Fact]
    public void AddMigration_uses_design_time_factory_when_service_provider_throws()
    {
        var operations = CreateOperationsWithThrowingHost(typeof(FactoryContext), typeof(FactoryContextFactory));

        var files = operations.AddMigration("Test", null, null, null, dryRun: true);

        Assert.NotNull(files.MigrationFile);
    }

    [Fact]
    public void Named_context_is_used_with_multiple_design_time_factories_when_service_provider_throws()
    {
        var operations = CreateOperationsWithThrowingHost(
            typeof(FactoryContext), typeof(SecondFactoryContext), typeof(FactoryContextFactory), typeof(SecondFactoryContextFactory));

        Assert.NotNull(operations.ScriptMigration(null, null, MigrationsSqlGenerationOptions.Default, nameof(SecondFactoryContext)));
        Assert.NotNull(operations.AddMigration("Test", null, nameof(FactoryContext), null, dryRun: true).MigrationFile);
    }

    [Fact]
    public void Unnamed_context_with_multiple_design_time_factories_reports_ambiguity_when_service_provider_throws()
    {
        var operations = CreateOperationsWithThrowingHost(
            typeof(FactoryContext), typeof(SecondFactoryContext), typeof(FactoryContextFactory), typeof(SecondFactoryContextFactory));

        Assert.Equal(
            DesignStrings.MultipleContexts,
            Assert.Throws<OperationException>(()
                => operations.ScriptMigration(null, null, MigrationsSqlGenerationOptions.Default, contextType: null)).Message);
        Assert.Equal(
            DesignStrings.MultipleContexts,
            Assert.Throws<OperationException>(() => operations.AddMigration("Test", null, null, null, dryRun: true)).Message);
    }

    [Fact]
    public void Service_provider_failure_is_still_reported_when_context_has_no_design_time_factory()
    {
        var operations = CreateOperationsWithThrowingHost(typeof(FactoryContext), typeof(FactoryContextFactory), typeof(NoFactoryContext));

        Assert.Equal(
            DesignStrings.CannotFindDbContextTypes("Bang!"),
            Assert.Throws<OperationException>(()
                => operations.ScriptMigration(null, null, MigrationsSqlGenerationOptions.Default, contextType: null)).Message);
    }

    private static TestMigrationsOperations CreateOperationsWithThrowingHost(params Type[] types)
    {
        var assembly = MockAssembly.Create([typeof(ThrowingProgram), .. types]);
        MigrationsAssembly = assembly;
        return new TestMigrationsOperations(
            new TestOperationReporter(),
            assembly,
            assembly,
            "projectDir",
            "RootNamespace",
            "C#",
            nullable: false,
            args: []);
    }

    // Factories are created reflectively by the tools, so the mock assembly is passed through a static.
    private static Assembly MigrationsAssembly { get; set; } = null!;

    private static class ThrowingProgram
    {
        private static TestWebHost BuildWebHost(string[] args)
            => new(
                new ServiceCollection()
                    .AddDbContext<FactoryContext>(_ => throw new Exception("Bang!"))
                    .AddDbContext<NoFactoryContext>(_ => throw new Exception("Bang!"))
                    .BuildServiceProvider(validateScopes: true));
    }

    private class NoFactoryContext(DbContextOptions<NoFactoryContext> options) : DbContext(options);

    private class FactoryContext(DbContextOptions options) : DbContext(options);

    private class FactoryContextFactory : IDesignTimeDbContextFactory<FactoryContext>
    {
        public FactoryContext CreateDbContext(string[] args)
            => new(new DbContextOptionsBuilder<FactoryContext>().UseSqlServer(o => o.MigrationsAssembly(MigrationsAssembly)).Options);
    }

    private class SecondFactoryContext(DbContextOptions options) : DbContext(options);

    private class SecondFactoryContextFactory : IDesignTimeDbContextFactory<SecondFactoryContext>
    {
        public SecondFactoryContext CreateDbContext(string[] args)
            => new(new DbContextOptionsBuilder<SecondFactoryContext>().UseSqlServer(o => o.MigrationsAssembly(MigrationsAssembly)).Options);
    }

    private class TestContext : DbContext;

    private class AssemblyTestContext : DbContext
    {
        public static Assembly MigrationsAssembly { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseSqlServer(o => o.MigrationsAssembly(MigrationsAssembly));
    }
}
