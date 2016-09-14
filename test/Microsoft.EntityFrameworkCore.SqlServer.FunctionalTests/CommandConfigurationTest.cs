// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestUtilities.Xunit;
using Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests
{
    public class CommandConfigurationTest : IClassFixture<CommandConfigurationTest.CommandConfigurationTestFixture>, IDisposable
    {
        private readonly CommandConfigurationTestFixture _fixture;
        private readonly SqlServerTestStore _testStore;

        public CommandConfigurationTest(CommandConfigurationTestFixture fixture)
        {
            _fixture = fixture;
            _testStore = _fixture.CreateDatabase();
        }

        public virtual void Dispose() => _testStore.Dispose();

        public class CommandConfigurationTestFixture
        {
            public IServiceProvider ServiceProvider { get; } = new ServiceCollection()
                .AddEntityFrameworkSqlServer()
                .BuildServiceProvider();

            public virtual SqlServerTestStore CreateDatabase() => SqlServerTestStore.GetOrCreateShared("CommandConfiguration", null);
        }

        [Fact]
        public void Constructed_select_query_CommandBuilder_throws_when_negative_CommandTimeout_is_used()
        {
            using (var context = new ConfiguredChipsContext(_fixture.ServiceProvider, _testStore.Name))
            {
                Assert.Throws<ArgumentException>(() => context.Database.SetCommandTimeout(-5));
            }
        }

        [ConditionalTheory]
        [SqlServerCondition(SqlServerCondition.SupportsSequences)]
        [InlineData(51, 6)]
        [InlineData(50, 5)]
        [InlineData(20, 2)]
        [InlineData(2, 1)]
        public void Keys_generated_in_batches(int count, int expected)
        {
            var loggerFactory = new TestSqlLoggerFactory();
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkSqlServer()
                .AddSingleton<ILoggerFactory>(loggerFactory)
                .BuildServiceProvider();

            using (var context = new ConfiguredChipsContext(serviceProvider, _testStore.Name))
            {
                context.Database.EnsureCreated();

                for (var i = 0; i < count; i++)
                {
                    context.Chips.Add(new KettleChips { BestBuyDate = DateTime.Now, Name = "Doritos Locos Tacos " + i });
                }
                context.SaveChanges();
            }

            Assert.Equal(expected, CountSqlLinesContaining("SELECT NEXT VALUE FOR"));
        }

        public int CountSqlLinesContaining(string searchTerm)
            => CountLinesContaining(Sql, searchTerm);

        public int CountLinesContaining(string source, string searchTerm)
        {
            var text = source.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            var matchQuery = from word in text
                             where word.Contains(searchTerm)
                             select word;

            return matchQuery.Count();
        }

        private class ChipsContext : DbContext
        {
            private readonly IServiceProvider _serviceProvider;

            public ChipsContext(IServiceProvider serviceProvider, string databaseName)
            {
                _serviceProvider = serviceProvider;
                DatabaseName = databaseName;
            }

            public DbSet<KettleChips> Chips { get; set; }
            protected string DatabaseName { get; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .UseSqlServer(SqlServerTestStore.CreateConnectionString(DatabaseName), b => b.ApplyConfiguration())
                    .UseInternalServiceProvider(_serviceProvider);

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                if (TestEnvironment.GetFlag(nameof(SqlServerCondition.SupportsSequences)) ?? true)
                {
                    modelBuilder.ForSqlServerUseSequenceHiLo();
                }
            }
        }

        private class KettleChips
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public DateTime BestBuyDate { get; set; }
        }

        private class ConfiguredChipsContext : ChipsContext
        {
            public ConfiguredChipsContext(IServiceProvider serviceProvider, string databaseName)
                : base(serviceProvider, databaseName)
            {
            }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => base.OnConfiguring(
                    optionsBuilder.UseSqlServer("Database=" + DatabaseName, b =>
                        {
                            b.ApplyConfiguration();
                            b.CommandTimeout(77);
                        }));
        }

        private static string Sql => TestSqlLoggerFactory.Sql;
    }
}
