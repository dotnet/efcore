// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.EntityFrameworkCore.SqlServer.Design.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Scaffolding.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Design
{
    public class DesignTimeServicesTest
    {
        [ConditionalTheory]
        [InlineData(true)]
        [InlineData(false)]
        public void Services_are_registered_using_correct_priority(bool useContext)
        {
            using var context = new MyContext(new DbContextOptionsBuilder<MyContext>().UseSqlServer()
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
            Assert.Equal("HumanizerPluralizer", serviceProvider.GetRequiredService<IPluralizer>().GetType().Name);

            // Provider design-time services are resolved
            Assert.Equal(typeof(SqlServerAnnotationCodeGenerator),
                serviceProvider.GetRequiredService<IAnnotationCodeGenerator>().GetType());

            // Extension design-time services are resolved
            Assert.Equal(typeof(ExtensionDatabaseModelFactory), serviceProvider.GetRequiredService<IDatabaseModelFactory>().GetType());
            Assert.Equal(typeof(SqlServerNetTopologySuiteCodeGeneratorPlugin),
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
            Assert.Equal("UserMigrationsIdGenerator",
                serviceProvider.GetRequiredService<IMigrationsIdGenerator>().GetType().Name);
            Assert.Equal("UserProviderConfigurationCodeGenerator",
                serviceProvider.GetRequiredService<IProviderConfigurationCodeGenerator>().GetType().Name);
        }

        public class TryAddDesignTimeServices : IDesignTimeServices
        {
            public virtual void ConfigureDesignTimeServices(IServiceCollection serviceCollection)
            {
                serviceCollection.TryAddSingleton<IDatabaseModelFactory, ExtensionDatabaseModelFactory>();
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
            public string GenerateId(string name) => throw new NotImplementedException();

            public string GetName(string id) => throw new NotImplementedException();

            public bool IsValidId(string value) => throw new NotImplementedException();
        }

        public class ExtensionHistoryRepository : IHistoryRepository
        {
            public bool Exists() => throw new NotImplementedException();

            public Task<bool> ExistsAsync(CancellationToken cancellationToken = default)
                => throw new NotImplementedException();

            public IReadOnlyList<HistoryRow> GetAppliedMigrations() => throw new NotImplementedException();

            public Task<IReadOnlyList<HistoryRow>> GetAppliedMigrationsAsync(CancellationToken cancellationToken = default)
                => throw new NotImplementedException();

            public string GetBeginIfExistsScript(string migrationId) => throw new NotImplementedException();

            public string GetBeginIfNotExistsScript(string migrationId) => throw new NotImplementedException();

            public string GetCreateIfNotExistsScript() => throw new NotImplementedException();

            public string GetCreateScript() => throw new NotImplementedException();

            public string GetDeleteScript(string migrationId) => throw new NotImplementedException();

            public string GetEndIfScript() => throw new NotImplementedException();

            public string GetInsertScript(HistoryRow row) => throw new NotImplementedException();
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
            public string GenerateId(string name) => throw new NotImplementedException();

            public string GetName(string id) => throw new NotImplementedException();

            public bool IsValidId(string value) => throw new NotImplementedException();
        }

        public class ContextHistoryRepository : IHistoryRepository
        {
            public bool Exists() => throw new NotImplementedException();

            public Task<bool> ExistsAsync(CancellationToken cancellationToken = default)
                => throw new NotImplementedException();

            public IReadOnlyList<HistoryRow> GetAppliedMigrations() => throw new NotImplementedException();

            public Task<IReadOnlyList<HistoryRow>> GetAppliedMigrationsAsync(CancellationToken cancellationToken = default)
                => throw new NotImplementedException();

            public string GetBeginIfExistsScript(string migrationId) => throw new NotImplementedException();

            public string GetBeginIfNotExistsScript(string migrationId) => throw new NotImplementedException();

            public string GetCreateIfNotExistsScript() => throw new NotImplementedException();

            public string GetCreateScript() => throw new NotImplementedException();

            public string GetDeleteScript(string migrationId) => throw new NotImplementedException();

            public string GetEndIfScript() => throw new NotImplementedException();

            public string GetInsertScript(HistoryRow row) => throw new NotImplementedException();
        }

        public class MyContext : DbContext
        {
            public MyContext(DbContextOptions<MyContext> options)
                : base(options)
            {
            }
        }

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
            var servicesBuilder = new DesignTimeServicesBuilder(assembly, startupAssembly, reporter, new string[0]);

            return (context == null
                ? servicesBuilder
                    .CreateServiceCollection("Microsoft.EntityFrameworkCore.SqlServer")
                : servicesBuilder
                    .CreateServiceCollection(context))
                .BuildServiceProvider();
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
}
