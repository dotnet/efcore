// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.Query.Internal;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class CommandConfigurationTest : IClassFixture<CommandConfigurationTest.CommandConfigurationTestFixture>
    {
        private const string DatabaseName = "NotKettleChips";

        private readonly CommandConfigurationTestFixture _fixture;

        public CommandConfigurationTest(CommandConfigurationTestFixture fixture)
        {
            _fixture = fixture;
            _fixture.CreateDatabase();
        }

        public class CommandConfigurationTestFixture : IDisposable
        {
            public IServiceProvider ServiceProvider { get; } = new ServiceCollection()
                .AddEntityFramework()
                .AddSqlServer()
                .ServiceCollection()
                .BuildServiceProvider();

            public virtual void CreateDatabase()
            {
                SqlServerTestStore.GetOrCreateShared(DatabaseName, () =>
                    {
                        using (var context = new ChipsContext(ServiceProvider))
                        {
                            context.Database.EnsureDeleted();
                            context.Database.EnsureCreated();
                        }
                    });
            }

            public void Dispose()
            {
                using (var context = new ChipsContext(ServiceProvider))
                {
                    context.Database.EnsureDeleted();
                }
            }
        }

        [Theory]
        [InlineData(51, 6)]
        [InlineData(50, 5)]
        [InlineData(20, 2)]
        [InlineData(2, 1)]
        public void Keys_generated_in_batches(int count, int expected)
        {
            var loggerFactory = new TestSqlLoggerFactory();
            var serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddSqlServer()
                .ServiceCollection()
                .AddInstance<ILoggerFactory>(loggerFactory)
                .BuildServiceProvider();

            using (var context = new ChipsContext(serviceProvider))
            {
                context.Database.EnsureCreated();

                for (var i = 0; i < count; i++)
                {
                    context.Chips.Add(new KettleChips { BestBuyDate = DateTime.Now, Name = "Doritos Locos Tacos " + i });
                }
                context.SaveChanges();
            }

            Assert.Equal(expected, CountLinesContaining(Sql, "SELECT NEXT VALUE FOR"));
        }

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
            public ChipsContext(IServiceProvider serviceProvider)
                : base(serviceProvider)
            {
            }

            public DbSet<KettleChips> Chips { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder
                    .UseSqlServer(SqlServerTestStore.CreateConnectionString(DatabaseName))
                    .CommandTimeout(77);
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.UseSqlServerSequenceHiLo();
            }
        }

        private class KettleChips
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public DateTime BestBuyDate { get; set; }
        }

        private static string Sql => TestSqlLoggerFactory.Sql;
    }
}
