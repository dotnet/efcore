// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.FunctionalTests;
using Microsoft.Data.Entity.Relational.Query;
using Microsoft.Data.Entity.Relational.Query.Expressions;
using Microsoft.Data.Entity.Relational.Query.Sql;
using Microsoft.Data.Entity.Relational.Update;
using Microsoft.Data.Entity.SqlServer.Query;
using Microsoft.Data.Entity.SqlServer.Update;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class CommandConfigurationTests : IDisposable
    {
        private readonly IServiceProvider _serviceProvider = new ServiceCollection()
            .AddEntityFramework()
            .AddSqlServer()
            .ServiceCollection()
            .BuildServiceProvider();

        [Fact]
        public void Constructed_select_query_uses_default_when_commandTimeout_not_configured_and_can_be_changed()
        {
            using (var context = new ChipsContext(_serviceProvider, "KettleChips"))
            {
                var commandBuilder = setupCommandBuilder();

                var relationalConnection = context.Database.AsRelational().Connection;
                var command = commandBuilder.Build(relationalConnection, new Dictionary<string, object>());

                Assert.Equal(30, command.CommandTimeout);

                context.Database.AsRelational().Connection.CommandTimeout = 77;
                var command2 = commandBuilder.Build(relationalConnection, new Dictionary<string, object>());

                Assert.Equal(77, command2.CommandTimeout);
            }
        }

        [Fact]
        public void Constructed_select_query_honors_configured_commandTimeout_configured_in_context()
        {
            using (var context = new ConfiguredChipsContext(_serviceProvider, "KettleChips"))
            {
                var commandBuilder = setupCommandBuilder();

                var relationalConnection = context.Database.AsRelational().Connection;
                var command = commandBuilder.Build(relationalConnection, new Dictionary<string, object>());

                Assert.Equal(77, command.CommandTimeout);
            }
        }

        [Fact]
        public void Constructed_select_query_honors_latest_configured_commandTimeout_configured_in_context()
        {
            using (var context = new ConfiguredChipsContext(_serviceProvider, "KettleChips"))
            {
                var commandBuilder = setupCommandBuilder();

                context.Database.AsRelational().Connection.CommandTimeout = 88;
                var relationalConnection = context.Database.AsRelational().Connection;
                var command = commandBuilder.Build(relationalConnection, new Dictionary<string, object>());

                Assert.Equal(88, command.CommandTimeout);

                context.Database.AsRelational().Connection.CommandTimeout = 99;
                relationalConnection = context.Database.AsRelational().Connection;
                var command2 = commandBuilder.Build(relationalConnection, new Dictionary<string, object>());

                Assert.Equal(99, command2.CommandTimeout);
            }
        }

        [Fact]
        public void Constructed_select_query_CommandBuilder_throws_when_negative_CommandTimeout_is_used()
        {
            using (var context = new ConfiguredChipsContext(_serviceProvider, "KettleChips"))
            {
                Assert.Throws<ArgumentException>(() => context.Database.AsRelational().Connection.CommandTimeout = -5);
            }
        }

        [Fact]
        public void Constructed_select_query_CommandBuilder_uses_default_when_null()
        {
            using (var context = new ConfiguredChipsContext(_serviceProvider, "KettleChips"))
            {
                var commandBuilder = setupCommandBuilder();

                var relationalConnection = context.Database.AsRelational().Connection;
                context.Database.AsRelational().Connection.CommandTimeout = null;
                var command = commandBuilder.Build(relationalConnection, new Dictionary<string, object>());

                Assert.Equal(30, command.CommandTimeout);
            }
        }

        private CommandBuilder setupCommandBuilder()
        {
            var selectExpression = new SelectExpression();

            return new CommandBuilder(new DefaultSqlQueryGenerator(selectExpression));
        }

        [Fact]
        public void Constructed_update_statement_uses_default_when_commandTimeout_not_configured()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddSqlServer()
                .ServiceCollection()
                .AddSingleton<ISqlServerModificationCommandBatchFactory, TestSqlServerModificationCommandBatchFactory>()
                .BuildServiceProvider();

            using (var context = new ChipsContext(serviceProvider, "KettleChips"))
            {
                context.Database.EnsureCreated();

                context.Chips.Add(new KettleChips { BestBuyDate = DateTime.Now, Name = "Doritos Locos Tacos" });
                context.SaveChanges();
                Assert.Null(globalCommandTimeout);
            }
        }

        [Fact]
        public void Constructed_update_statement_uses_commandTimeout_configured_in_Context()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddSqlServer()
                .ServiceCollection()
                .AddSingleton<ISqlServerModificationCommandBatchFactory, TestSqlServerModificationCommandBatchFactory>()
                .BuildServiceProvider();

            using (var context = new ConfiguredChipsContext(serviceProvider, "KettleChips"))
            {
                context.Database.EnsureCreated();

                context.Chips.Add(new KettleChips { BestBuyDate = DateTime.Now, Name = "Doritos Locos Tacos" });
                context.SaveChanges();
                Assert.Equal(77, globalCommandTimeout);
            }
        }

        [Fact]
        public void Constructed_update_statement_uses_commandTimeout_not_configured_in_context()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddSqlServer()
                .ServiceCollection()
                .AddSingleton<ISqlServerModificationCommandBatchFactory, TestSqlServerModificationCommandBatchFactory>()
                .BuildServiceProvider();

            using (var context = new ChipsContext(serviceProvider, "KettleChips"))
            {
                context.Database.EnsureCreated();

                context.Database.AsRelational().Connection.CommandTimeout = 88;
                context.Chips.Add(new KettleChips { BestBuyDate = DateTime.Now, Name = "Doritos Locos Tacos" });
                context.SaveChanges();
                Assert.Equal(88, globalCommandTimeout);
            }
        }

        [Fact]
        public void Constructed_update_statement_uses_commandTimeout_overriding_configured_in_context()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddSqlServer()
                .ServiceCollection()
                .AddSingleton<ISqlServerModificationCommandBatchFactory, TestSqlServerModificationCommandBatchFactory>()
                .BuildServiceProvider();

            using (var context = new ConfiguredChipsContext(serviceProvider, "KettleChips"))
            {
                context.Database.EnsureCreated();

                context.Database.AsRelational().Connection.CommandTimeout = 88;
                context.Chips.Add(new KettleChips { BestBuyDate = DateTime.Now, Name = "Doritos Locos Tacos" });
                context.SaveChanges();
                Assert.Equal(88, globalCommandTimeout);
            }
        }

        [Fact]
        public async void Constructed_update_statement_uses_default_commandTimeout_can_override_not_configured_in_context_async()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddSqlServer()
                .ServiceCollection()
                .AddSingleton<ISqlServerModificationCommandBatchFactory, TestSqlServerModificationCommandBatchFactory>()
                .BuildServiceProvider();

            using (var context = new ChipsContext(serviceProvider, "KettleChips"))
            {
                context.Database.EnsureCreated();

                context.Chips.Add(new KettleChips { BestBuyDate = DateTime.Now, Name = "Doritos Locos Tacos" });
                await context.SaveChangesAsync();
                Assert.Null(globalCommandTimeout);

                context.Database.AsRelational().Connection.CommandTimeout = 88;

                context.Chips.Add(new KettleChips { BestBuyDate = DateTime.Now, Name = "Doritos Locos Tacos" });
                await context.SaveChangesAsync();
                Assert.Equal(88, globalCommandTimeout);
            }
        }

        [Fact]
        public async void Constructed_update_statement_uses_default_commandTimeout_can_override_configured_in_context_async()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddSqlServer()
                .ServiceCollection()
                .AddSingleton<ISqlServerModificationCommandBatchFactory, TestSqlServerModificationCommandBatchFactory>()
                .BuildServiceProvider();

            using (var context = new ConfiguredChipsContext(serviceProvider, "KettleChips"))
            {
                context.Database.EnsureCreated();

                context.Chips.Add(new KettleChips { BestBuyDate = DateTime.Now, Name = "Doritos Locos Tacos" });
                await context.SaveChangesAsync();
                Assert.Equal(77, globalCommandTimeout);

                context.Database.AsRelational().Connection.CommandTimeout = 88;

                context.Chips.Add(new KettleChips { BestBuyDate = DateTime.Now, Name = "Doritos Locos Tacos" });
                await context.SaveChangesAsync();
                Assert.Equal(88, globalCommandTimeout);
            }
        }

        [Fact]
        public async void Overridden_commandTimeout_overrides_timeout_configured_in_context_async()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddSqlServer()
                .ServiceCollection()
                .AddSingleton<ISqlServerModificationCommandBatchFactory, TestSqlServerModificationCommandBatchFactory>()
                .BuildServiceProvider();

            using (var context = new ConfiguredChipsContext(serviceProvider, "KettleChips"))
            {
                context.Database.EnsureCreated();

                context.Database.AsRelational().Connection.CommandTimeout = 88;

                context.Chips.Add(new KettleChips { BestBuyDate = DateTime.Now, Name = "Doritos Locos Tacos" });
                await context.SaveChangesAsync();
                Assert.Equal(88, globalCommandTimeout);
            }
        }

        [Theory]
        [InlineData(51, 6)]
        [InlineData(50, 5)]
        [InlineData(20, 5)]
        [InlineData(2, 2)]
        public void Keys_generated_in_batches(int count, int expected)
        {
            var loggerFactory = new TestSqlLoggerFactory();
            var serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddSqlServer()
                .ServiceCollection()
                .AddInstance<ILoggerFactory>(loggerFactory)
                .AddSingleton<ISqlServerModificationCommandBatchFactory, TestSqlServerModificationCommandBatchFactory>()
                .BuildServiceProvider();

            using (var context = new ConfiguredChipsContext(serviceProvider, "KettleChips"))
            {
                context.Database.EnsureCreated();

                for (int i = 0; i < count; i++)
                {
                    context.Chips.Add(new KettleChips { BestBuyDate = DateTime.Now, Name = "Doritos Locos Tacos " + i });
                }
                context.SaveChanges();
            }

            Assert.Equal(expected, CountSqlLinesContaining("SELECT NEXT VALUE FOR"));
        }

        public int CountSqlLinesContaining(string searchTerm)
        {
            return CountLinesContaining(Sql, searchTerm);
        }

        public int CountLinesContaining(string source, string searchTerm)
        {
            string[] text = source.Split(new string[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);

            var matchQuery = from word in text
                             where word.Contains(searchTerm)
                             select word;

            return matchQuery.Count();
        }

        public void Dispose()
        {
            using (var context = new ChipsContext(_serviceProvider, "KettleChips"))
            {
                context.Database.EnsureDeleted();
            }
        }

        public static int? globalCommandTimeout;

        public class TestSqlServerModificationCommandBatch : SqlServerModificationCommandBatch
        {
            protected override DbCommand CreateStoreCommand(string commandText, DbTransaction transaction, IRelationalTypeMapper typeMapper, int? commandTimeout)
            {
                globalCommandTimeout = commandTimeout;
                return base.CreateStoreCommand(commandText, transaction, typeMapper, commandTimeout);
            }

            public TestSqlServerModificationCommandBatch(
                ISqlServerSqlGenerator sqlGenerator,
                int? maxBatchSize)
                : base(sqlGenerator, maxBatchSize)
            {
            }
        }

        public class TestSqlServerModificationCommandBatchFactory : SqlServerModificationCommandBatchFactory
        {
            public TestSqlServerModificationCommandBatchFactory(
                ISqlServerSqlGenerator sqlGenerator)
                : base(sqlGenerator)
            {
            }

            public override ModificationCommandBatch Create(IDbContextOptions options)
            {
                var optionsExtension = options.Extensions.OfType<SqlServerOptionsExtension>().FirstOrDefault();

                var maxBatchSize = optionsExtension?.MaxBatchSize;

                return new TestSqlServerModificationCommandBatch((ISqlServerSqlGenerator)SqlGenerator, maxBatchSize);
            }
        }

        private class ChipsContext : DbContext
        {
            private readonly string _databaseName;

            public ChipsContext(IServiceProvider serviceProvider, string databaseName)
                : base(serviceProvider)
            {
                _databaseName = databaseName;
            }

            public DbSet<KettleChips> Chips { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseSqlServer(SqlServerTestStore.CreateConnectionString(_databaseName));
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
            {
                optionsBuilder.UseSqlServer("Database=Crunchie").CommandTimeout(77);

                base.OnConfiguring(optionsBuilder);
            }
        }

        private static string Sql
        {
            get { return TestSqlLoggerFactory.Sql; }
        }
    }
}
