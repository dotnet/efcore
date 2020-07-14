// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Design.Internal
{
    public class DbContextOperationsTest
    {
        [ConditionalFact]
        public void CreateContext_gets_service()
        {
            CreateOperations(typeof(TestProgram)).CreateContext(typeof(TestContext).FullName);
        }

        [ConditionalFact]
        public void CreateContext_gets_service_without_AddDbContext()
        {
            CreateOperations(typeof(TestProgramWithoutAddDbContext)).CreateContext(typeof(TestContext).FullName);
        }

        [ConditionalFact]
        public void CreateContext_gets_service_when_context_factory_used()
        {
            CreateOperations(typeof(TestProgramWithContextFactory)).CreateContext(typeof(TestContextFromFactory).FullName);
        }

        [ConditionalFact]
        public void Can_pass_null_args()
        {
            // Even though newer versions of the tools will pass an empty array
            // older versions of the tools can pass null args.
            var assembly = MockAssembly.Create(typeof(TestContext));
            _ = new TestDbContextOperations(
                new TestOperationReporter(),
                assembly,
                assembly,
                args: null,
                new TestAppServiceProviderFactory(assembly));
        }

        [ConditionalFact]
        public void CreateContext_uses_exact_factory_method()
        {
            var assembly = MockAssembly.Create(typeof(BaseContext), typeof(DerivedContext), typeof(HierarchyContextFactory));
            var operations = new TestDbContextOperations(
                new TestOperationReporter(),
                assembly,
                assembly,
                args: Array.Empty<string>(),
                new TestAppServiceProviderFactory(assembly));

            var baseContext = Assert.IsType<BaseContext>(operations.CreateContext(nameof(BaseContext)));
            Assert.Equal(nameof(BaseContext), baseContext.FactoryUsed);

            var derivedContext = Assert.IsType<DerivedContext>(operations.CreateContext(nameof(DerivedContext)));
            Assert.Equal(nameof(DerivedContext), derivedContext.FactoryUsed);
        }

        [ConditionalFact]
        public void GetContextInfo_returns_correct_info()
        {
            var info = CreateOperations(typeof(TestProgramRelational)).GetContextInfo(nameof(TestContext));

            Assert.Equal("Test", info.DatabaseName);
            Assert.Equal(@"(localdb)\mssqllocaldb", info.DataSource);
            Assert.Equal("None", info.Options);
            Assert.Equal("Microsoft.EntityFrameworkCore.SqlServer", info.ProviderName);
        }

        [ConditionalFact]
        public void GetContextInfo_does_not_throw_if_DbConnection_cannot_be_created()
        {
            Exception expected = null;
            try
            {
                new SqlConnection("Cake=None");
            }
            catch (Exception e)
            {
                expected = e;
            }

            var info = CreateOperations(typeof(TestProgramRelationalBad)).GetContextInfo(nameof(TestContext));

            Assert.Equal(DesignStrings.BadConnection(expected.Message), info.DatabaseName);
            Assert.Equal(DesignStrings.BadConnection(expected.Message), info.DataSource);
            Assert.Equal("None", info.Options);
            Assert.Equal("Microsoft.EntityFrameworkCore.SqlServer", info.ProviderName);
        }

        [ConditionalFact]
        public void GetContextInfo_does_not_throw_if_provider_not_relational()
        {
            var info = CreateOperations(typeof(TestProgram)).GetContextInfo(nameof(TestContext));

            Assert.Equal(DesignStrings.NoRelationalConnection, info.DatabaseName);
            Assert.Equal(DesignStrings.NoRelationalConnection, info.DataSource);
            Assert.Equal("StoreName=In-memory test database", info.Options);
            Assert.Equal("Microsoft.EntityFrameworkCore.InMemory", info.ProviderName);
        }

        private static class TestProgram
        {
            private static TestWebHost BuildWebHost(string[] args)
                => CreateWebHost(b => b.UseInMemoryDatabase("In-memory test database"));
        }

        private static class TestProgramWithoutAddDbContext
        {
            private static TestWebHost BuildWebHost(string[] args)
                => new TestWebHost(
                    new ServiceCollection()
                        .AddSingleton(
                            new TestContext(
                                new DbContextOptionsBuilder<TestContext>()
                                    .UseInMemoryDatabase("In-memory test database")
                                    .EnableServiceProviderCaching(false)
                                    .Options))
                        .BuildServiceProvider());
        }

        private static class TestProgramWithContextFactory
        {
            private static TestWebHost BuildWebHost(string[] args)
                => new TestWebHost(
                    new ServiceCollection()
                        .AddDbContextFactory<TestContextFromFactory>(b => b.UseInMemoryDatabase("In-memory test database"))
                        .BuildServiceProvider());
        }

        private static class TestProgramRelational
        {
            private static TestWebHost BuildWebHost(string[] args)
                => CreateWebHost(b => b.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=Test;ConnectRetryCount=0"));
        }

        private static class TestProgramRelationalBad
        {
            private static TestWebHost BuildWebHost(string[] args)
                => CreateWebHost(b => b.UseSqlServer(@"Cake=None"));
        }

        private static TestDbContextOperations CreateOperations(Type testProgramType)
        {
            var assembly = MockAssembly.Create(testProgramType, typeof(TestContext));
            return new TestDbContextOperations(
                new TestOperationReporter(),
                assembly,
                assembly,
                /* args: */ Array.Empty<string>(),
                new TestAppServiceProviderFactory(assembly));
        }

        private static TestWebHost CreateWebHost(Func<DbContextOptionsBuilder, DbContextOptionsBuilder> configureProvider)
            => new TestWebHost(
                new ServiceCollection()
                    .AddDbContext<TestContext>(
                        b =>
                            configureProvider(b.EnableServiceProviderCaching(false)))
                    .BuildServiceProvider());

        private class TestContext : DbContext
        {
            public TestContext()
            {
                throw new Exception("This isn't the constructor you're looking for.");
            }

            public TestContext(DbContextOptions<TestContext> options)
                : base(options)
            {
            }
        }

        private class TestContextFromFactory : DbContext
        {
            private TestContextFromFactory()
            {
                throw new Exception("This isn't the constructor you're looking for.");
            }

            public TestContextFromFactory(DbContextOptions<TestContextFromFactory> options)
                : base(options)
            {
            }
        }

        private class BaseContext : DbContext
        {
            public BaseContext(string factoryUsed)
            {
                FactoryUsed = factoryUsed;
            }

            protected override void OnConfiguring(DbContextOptionsBuilder options)
                => options.UseInMemoryDatabase(GetType().Name);

            public string FactoryUsed { get; }
        }

        private class DerivedContext : BaseContext
        {
            public DerivedContext(string factoryUsed)
                : base(factoryUsed)
            {
            }
        }

        private class HierarchyContextFactory : IDesignTimeDbContextFactory<BaseContext>, IDesignTimeDbContextFactory<DerivedContext>
        {
            BaseContext IDesignTimeDbContextFactory<BaseContext>.CreateDbContext(string[] args)
                => new BaseContext(nameof(BaseContext));

            DerivedContext IDesignTimeDbContextFactory<DerivedContext>.CreateDbContext(string[] args)
                => new DerivedContext(nameof(DerivedContext));
        }
    }
}
