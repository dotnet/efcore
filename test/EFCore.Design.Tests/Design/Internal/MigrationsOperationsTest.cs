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

    private class TestContext : DbContext;

    private class AssemblyTestContext : DbContext
    {
        public static Assembly MigrationsAssembly { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseSqlServer(o => o.MigrationsAssembly(MigrationsAssembly));
    }
}
