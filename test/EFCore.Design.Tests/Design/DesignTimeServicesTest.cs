// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data.Common;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.EntityFrameworkCore.SqlServer.Design.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Scaffolding.Internal;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.EntityFrameworkCore.Design;

public class DesignTimeServicesTest
{
    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public void Services_are_registered_using_correct_priority(bool useContext)
    {
        using var context = new MyContext(
            new DbContextOptionsBuilder<MyContext>().UseSqlServer()
                .ReplaceService<IMigrationsIdGenerator, ContextMigrationsIdGenerator>()
                .ReplaceService<IHistoryRepository, ContextHistoryRepository>()
                .Options);

        var serviceProvider = CreateDesignServiceProvider(
            @"
using Microsoft.EntityFrameworkCore.Design;

[assembly: DesignTimeServicesReference(""Microsoft.EntityFrameworkCore.Design.DesignTimeServicesTest+TryAddDesignTimeServices, Microsoft.EntityFrameworkCore.Design.Tests"")]
",
            @"
using System;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

#nullable disable

[assembly: DesignTimeServicesReference(""Microsoft.EntityFrameworkCore.SqlServer.Design.Internal.SqlServerNetTopologySuiteDesignTimeServices, Microsoft.EntityFrameworkCore.SqlServer.NetTopologySuite"")]

public class UserDesignTimeServices : IDesignTimeServices
{
    public virtual void ConfigureDesignTimeServices(IServiceCollection serviceCollection)
        => serviceCollection
            .AddSingleton<IMigrationsIdGenerator, UserMigrationsIdGenerator>()
            .AddSingleton<IProviderConfigurationCodeGenerator, UserProviderConfigurationCodeGenerator>();
}

public class UserProviderConfigurationCodeGenerator : IProviderConfigurationCodeGenerator
{
    public MethodCallCodeFragment GenerateContextOptions() => throw new NotImplementedException();

    public MethodCallCodeFragment GenerateProviderOptions() => throw new NotImplementedException();

    public MethodCallCodeFragment GenerateUseProvider(string connectionString, MethodCallCodeFragment providerOptions)
        => throw new NotImplementedException();
}

public class UserMigrationsIdGenerator : IMigrationsIdGenerator
{
    public string GenerateId(string name) => throw new NotImplementedException();

    public string GetName(string id) => throw new NotImplementedException();

    public bool IsValidId(string value) => throw new NotImplementedException();
}
",
            useContext ? context : null).CreateScope().ServiceProvider;

        // Base design-time services are resolved
        Assert.Equal(
            typeof(CSharpMigrationOperationGeneratorDependencies),
            serviceProvider.GetRequiredService<CSharpMigrationOperationGeneratorDependencies>().GetType());
        Assert.Equal(
            typeof(CSharpMigrationsGeneratorDependencies),
            serviceProvider.GetRequiredService<CSharpMigrationsGeneratorDependencies>().GetType());
        Assert.Equal(
            typeof(CSharpSnapshotGeneratorDependencies),
            serviceProvider.GetRequiredService<CSharpSnapshotGeneratorDependencies>().GetType());
        Assert.Equal(typeof(CandidateNamingService), serviceProvider.GetRequiredService<ICandidateNamingService>().GetType());
        Assert.Equal(typeof(CSharpHelper), serviceProvider.GetRequiredService<ICSharpHelper>().GetType());
        Assert.Equal(
            typeof(CSharpMigrationOperationGenerator),
            serviceProvider.GetRequiredService<ICSharpMigrationOperationGenerator>().GetType());
        Assert.Equal(typeof(CSharpSnapshotGenerator), serviceProvider.GetRequiredService<ICSharpSnapshotGenerator>().GetType());
        Assert.Equal(typeof(CSharpUtilities), serviceProvider.GetRequiredService<ICSharpUtilities>().GetType());
        Assert.Equal(typeof(CSharpMigrationsGenerator), serviceProvider.GetRequiredService<IMigrationsCodeGenerator>().GetType());
        Assert.Equal(
            typeof(MigrationsCodeGeneratorSelector), serviceProvider.GetRequiredService<IMigrationsCodeGeneratorSelector>().GetType());
        Assert.Collection(
            serviceProvider.GetServices<IModelCodeGenerator>(),
            s => Assert.Equal(typeof(TextTemplatingModelGenerator), s.GetType()),
            s => Assert.Equal(typeof(CSharpModelGenerator), s.GetType()));
        Assert.Equal(typeof(ModelCodeGeneratorSelector), serviceProvider.GetRequiredService<IModelCodeGeneratorSelector>().GetType());
        Assert.Equal(
            typeof(CSharpRuntimeModelCodeGenerator), serviceProvider.GetRequiredService<ICompiledModelCodeGenerator>().GetType());
        Assert.Equal(
            typeof(CompiledModelCodeGeneratorSelector),
            serviceProvider.GetRequiredService<ICompiledModelCodeGeneratorSelector>().GetType());
        Assert.Equal(typeof(CompiledModelScaffolder), serviceProvider.GetRequiredService<ICompiledModelScaffolder>().GetType());
        Assert.Equal(
            typeof(DesignTimeConnectionStringResolver),
            serviceProvider.GetRequiredService<IDesignTimeConnectionStringResolver>().GetType());
        Assert.Equal(typeof(HumanizerPluralizer), serviceProvider.GetRequiredService<IPluralizer>().GetType());
        Assert.Equal(
            typeof(RelationalScaffoldingModelFactory), serviceProvider.GetRequiredService<IScaffoldingModelFactory>().GetType());
        Assert.Equal(typeof(ScaffoldingTypeMapper), serviceProvider.GetRequiredService<IScaffoldingTypeMapper>().GetType());
        Assert.Equal(
            typeof(MigrationsCodeGeneratorDependencies),
            serviceProvider.GetRequiredService<MigrationsCodeGeneratorDependencies>().GetType());
        Assert.Equal(
            typeof(ModelCodeGeneratorDependencies), serviceProvider.GetRequiredService<ModelCodeGeneratorDependencies>().GetType());
        Assert.Equal(typeof(ReverseEngineerScaffolder), serviceProvider.GetRequiredService<IReverseEngineerScaffolder>().GetType());

        if (useContext)
        {
            Assert.Equal(
                typeof(MigrationsScaffolderDependencies),
                serviceProvider.GetRequiredService<MigrationsScaffolderDependencies>().GetType());
            Assert.Equal(typeof(MigrationsScaffolder), serviceProvider.GetRequiredService<IMigrationsScaffolder>().GetType());
            Assert.Equal(typeof(SnapshotModelProcessor), serviceProvider.GetRequiredService<ISnapshotModelProcessor>().GetType());
        }

        Assert.Equal(typeof(TestOperationReporter), serviceProvider.GetRequiredService<IOperationReporter>().GetType());

        // Provider design-time services are resolved
        Assert.Equal(
            typeof(SqlServerAnnotationCodeGenerator),
            serviceProvider.GetRequiredService<IAnnotationCodeGenerator>().GetType());

        // Extension design-time services are resolved
        Assert.Equal(typeof(ExtensionDatabaseModelFactory), serviceProvider.GetRequiredService<IDatabaseModelFactory>().GetType());
        Assert.Equal(
            typeof(SqlServerNetTopologySuiteCodeGeneratorPlugin),
            serviceProvider.GetRequiredService<IEnumerable<IProviderCodeGeneratorPlugin>>().Single().GetType());

        if (!useContext)
        {
            Assert.Equal(typeof(ExtensionHistoryRepository), serviceProvider.GetRequiredService<IHistoryRepository>().GetType());
        }
        else
        {
            // Replaced services on context are resolved
            Assert.Equal(typeof(ContextHistoryRepository), serviceProvider.GetRequiredService<IHistoryRepository>().GetType());
        }

        // User-specified design-time services are resolved
        Assert.Equal(
            "UserMigrationsIdGenerator",
            serviceProvider.GetRequiredService<IMigrationsIdGenerator>().GetType().Name);
        Assert.Equal(
            "UserProviderConfigurationCodeGenerator",
            serviceProvider.GetRequiredService<IProviderConfigurationCodeGenerator>().GetType().Name);
    }

    public class TryAddDesignTimeServices : IDesignTimeServices
    {
        public virtual void ConfigureDesignTimeServices(IServiceCollection serviceCollection)
        {
            serviceCollection.TryAddScoped<IDatabaseModelFactory, ExtensionDatabaseModelFactory>();
            serviceCollection.TryAddSingleton<IMigrationsIdGenerator, ExtensionMigrationsIdGenerator>();
            serviceCollection.TryAddSingleton<IHistoryRepository, ExtensionHistoryRepository>();
            serviceCollection.TryAddSingleton<IProviderConfigurationCodeGenerator, ExtensionProviderConfigurationCodeGenerator>();
        }
    }

    public class ExtensionDatabaseModelFactory : IDatabaseModelFactory
    {
        public DatabaseModel Create(string connectionString, DatabaseModelFactoryOptions options)
            => throw new NotImplementedException();

        public DatabaseModel Create(DbConnection connection, DatabaseModelFactoryOptions options)
            => throw new NotImplementedException();
    }

    public class ExtensionMigrationsIdGenerator : IMigrationsIdGenerator
    {
        public string GenerateId(string name)
            => throw new NotImplementedException();

        public string GetName(string id)
            => throw new NotImplementedException();

        public bool IsValidId(string value)
            => throw new NotImplementedException();
    }

    public class ExtensionHistoryRepository : IHistoryRepository
    {
        public bool Exists()
            => throw new NotImplementedException();

        public Task<bool> ExistsAsync(CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public IReadOnlyList<HistoryRow> GetAppliedMigrations()
            => throw new NotImplementedException();

        public Task<IReadOnlyList<HistoryRow>> GetAppliedMigrationsAsync(CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public string GetBeginIfExistsScript(string migrationId)
            => throw new NotImplementedException();

        public string GetBeginIfNotExistsScript(string migrationId)
            => throw new NotImplementedException();

        public string GetCreateIfNotExistsScript()
            => throw new NotImplementedException();

        public string GetCreateScript()
            => throw new NotImplementedException();

        public string GetDeleteScript(string migrationId)
            => throw new NotImplementedException();

        public string GetEndIfScript()
            => throw new NotImplementedException();

        public string GetInsertScript(HistoryRow row)
            => throw new NotImplementedException();
    }

    public class ExtensionProviderConfigurationCodeGenerator : IProviderConfigurationCodeGenerator
    {
        public MethodCallCodeFragment GenerateContextOptions()
            => throw new NotImplementedException();

        public MethodCallCodeFragment GenerateProviderOptions()
            => throw new NotImplementedException();

        public MethodCallCodeFragment GenerateUseProvider(string connectionString, MethodCallCodeFragment providerOptions)
            => throw new NotImplementedException();
    }

    public class ContextMigrationsIdGenerator : IMigrationsIdGenerator
    {
        public string GenerateId(string name)
            => throw new NotImplementedException();

        public string GetName(string id)
            => throw new NotImplementedException();

        public bool IsValidId(string value)
            => throw new NotImplementedException();
    }

    public class ContextHistoryRepository : IHistoryRepository
    {
        public bool Exists()
            => throw new NotImplementedException();

        public Task<bool> ExistsAsync(CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public IReadOnlyList<HistoryRow> GetAppliedMigrations()
            => throw new NotImplementedException();

        public Task<IReadOnlyList<HistoryRow>> GetAppliedMigrationsAsync(CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public string GetBeginIfExistsScript(string migrationId)
            => throw new NotImplementedException();

        public string GetBeginIfNotExistsScript(string migrationId)
            => throw new NotImplementedException();

        public string GetCreateIfNotExistsScript()
            => throw new NotImplementedException();

        public string GetCreateScript()
            => throw new NotImplementedException();

        public string GetDeleteScript(string migrationId)
            => throw new NotImplementedException();

        public string GetEndIfScript()
            => throw new NotImplementedException();

        public string GetInsertScript(HistoryRow row)
            => throw new NotImplementedException();
    }

    public class MyContext(DbContextOptions<MyContext> options) : DbContext(options);

    private ServiceProvider CreateDesignServiceProvider(
        string assemblyCode,
        string startupAssemblyCode = null,
        DbContext context = null)
    {
        var assembly = Compile(assemblyCode);
        var startupAssembly = startupAssemblyCode == null
            ? assembly
            : Compile(startupAssemblyCode);

        var reporter = new TestOperationReporter();
        var servicesBuilder = new DesignTimeServicesBuilder(assembly, startupAssembly, reporter, []);

        return (context == null
                ? servicesBuilder
                    .CreateServiceCollection("Microsoft.EntityFrameworkCore.SqlServer")
                : servicesBuilder
                    .CreateServiceCollection(context))
            .BuildServiceProvider(validateScopes: true);
    }

    private Assembly Compile(string assemblyCode)
    {
        var build = new BuildSource
        {
            References =
            {
                BuildReference.ByName("Microsoft.EntityFrameworkCore"),
                BuildReference.ByName("Microsoft.EntityFrameworkCore.Design.Tests"),
                BuildReference.ByName("Microsoft.EntityFrameworkCore.Relational"),
                BuildReference.ByName("Microsoft.EntityFrameworkCore.SqlServer"),
                BuildReference.ByName("Microsoft.EntityFrameworkCore.SqlServer.NetTopologySuite"),
                BuildReference.ByName("Microsoft.Extensions.DependencyInjection.Abstractions")
            },
            Sources = { { "Startup.cs", assemblyCode } }
        };

        return build.BuildInMemory();
    }
}
