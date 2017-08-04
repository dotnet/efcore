// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore
{
    public class CommandConfigurationTest : SharedStoreFixtureBase<DbContext>
    {
        public CommandConfigurationTest()
        {
            TestSqlLoggerFactory.Clear();
        }

        [Fact]
        public void Constructed_select_query_CommandBuilder_throws_when_negative_CommandTimeout_is_used()
        {
            using (var context = CreateContext())
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
            DbContextHelpers.ExecuteWithStrategyInTransaction(
                CreateContext, UseTransaction,
                context =>
                    {
                        for (var i = 0; i < count; i++)
                        {
                            context.Set<KettleChips>().Add(new KettleChips { BestBuyDate = DateTime.Now, Name = "Doritos Locos Tacos " + i });
                        }
                        context.SaveChanges();
                    });

            Assert.Equal(expected, CountSqlLinesContaining("SELECT NEXT VALUE FOR", TestSqlLoggerFactory.Sql));
        }

        protected void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
            => facade.UseTransaction(transaction.GetDbTransaction());

        public int CountSqlLinesContaining(string searchTerm, string sql)
            => CountLinesContaining(sql, searchTerm);

        public int CountLinesContaining(string source, string searchTerm)
        {
            var text = source.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            var matchQuery = from word in text
                             where word.Contains(searchTerm)
                             select word;

            return matchQuery.Count();
        }

        protected override string StoreName { get; } = "CommandConfiguration";
        protected override Type ContextType { get; } = typeof(ChipsContext);
        protected override ITestStoreFactory<TestStore> TestStoreFactory => SqlServerTestStoreFactory.Instance;
        public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ServiceProvider.GetRequiredService<ILoggerFactory>();

        private class ChipsContext : DbContext
        {
            public ChipsContext(DbContextOptions options)
                : base(options)
            {
            }

            public DbSet<KettleChips> Chips { get; set; }

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
            // ReSharper disable once UnusedMember.Local
            public int Id { get; set; }
            public string Name { get; set; }
            public DateTime BestBuyDate { get; set; }
        }
    }
}
