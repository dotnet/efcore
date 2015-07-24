// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Query.Expressions;
using Microsoft.Data.Entity.Query.Sql;
using Microsoft.Data.Entity.Sqlite.Update;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Update;
using Microsoft.Data.Sqlite;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;
using Xunit;

namespace Microsoft.Data.Entity.Sqlite.FunctionalTests
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
            private SqliteTestStore _store;

            public IServiceProvider ServiceProvider { get; } = new ServiceCollection()
                .AddEntityFramework()
                .AddSqlite()
                .ServiceCollection()
                .BuildServiceProvider();

            public virtual void CreateDatabase()
            {
                _store = SqliteTestStore.GetOrCreateShared(DatabaseName, () =>
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
                _store.Dispose();
            }
        }

        [Fact]
        public void Constructed_select_query_uses_default_when_CommandTimeout_not_configured_and_can_be_changed()
        {
            using (var context = new ChipsContext(_fixture.ServiceProvider))
            {
                var commandBuilder = SetupCommandBuilder();

                var command = commandBuilder.Build(context.GetService<IRelationalConnection>(), new Dictionary<string, object>());

                Assert.Equal(new SqliteCommand().CommandTimeout, command.CommandTimeout);

                context.Database.SetCommandTimeout(77);
                var command2 = commandBuilder.Build(context.GetService<IRelationalConnection>(), new Dictionary<string, object>());

                Assert.Equal(77, command2.CommandTimeout);
            }
        }

        [Fact]
        public void Constructed_select_query_honors_latest_configured_CommandTimeout_configured_in_context()
        {
            using (var context = new ConfiguredChipsContext(_fixture.ServiceProvider))
            {
                var commandBuilder = SetupCommandBuilder();

                context.Database.SetCommandTimeout(88);
                var command = commandBuilder.Build(context.GetService<IRelationalConnection>(), new Dictionary<string, object>());

                Assert.Equal(88, command.CommandTimeout);

                context.Database.SetCommandTimeout(99);
                var command2 = commandBuilder.Build(context.GetService<IRelationalConnection>(), new Dictionary<string, object>());

                Assert.Equal(99, command2.CommandTimeout);
            }
        }

        [Fact]
        public void Constructed_select_query_CommandBuilder_throws_when_negative_CommandTimeout_is_used()
        {
            using (var context = new ConfiguredChipsContext(_fixture.ServiceProvider))
            {
                Assert.Throws<ArgumentException>(() => context.Database.SetCommandTimeout(-5));
            }
        }

        [Fact]
        public void Constructed_select_query_CommandBuilder_uses_default_when_null()
        {
            using (var context = new ConfiguredChipsContext(_fixture.ServiceProvider))
            {
                var commandBuilder = SetupCommandBuilder();

                context.Database.SetCommandTimeout(null);
                var command = commandBuilder.Build(context.GetService<IRelationalConnection>(), new Dictionary<string, object>());

                Assert.Equal(new SqliteCommand().CommandTimeout, command.CommandTimeout);
            }
        }

        private CommandBuilder SetupCommandBuilder()
        {
            var selectExpression = new SelectExpression();

            return new CommandBuilder(
                () => new DefaultQuerySqlGenerator(selectExpression, new SqliteTypeMapper()), new UntypedValueBufferFactoryFactory());
        }

        [Fact]
        public void Constructed_update_statement_uses_default_when_CommandTimeout_not_configured()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddSqlite()
                .ServiceCollection()
                .AddScoped<SqliteModificationCommandBatchFactory, TestSqliteModificationCommandBatchFactory>()
                .BuildServiceProvider();

            using (var context = new ChipsContext(serviceProvider))
            {
                context.Database.EnsureCreated();

                context.Chips.Add(new KettleChips { BestBuyDate = DateTime.Now, Name = "Doritos Locos Tacos" });
                context.SaveChanges();
                Assert.Null(GlobalCommandTimeout);
            }
        }

        [Fact]
        public async void Constructed_update_statement_uses_default_CommandTimeout_can_override()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddSqlite()
                .ServiceCollection()
                .AddScoped<SqliteModificationCommandBatchFactory, TestSqliteModificationCommandBatchFactory>()
                .BuildServiceProvider();

            using (var context = new ChipsContext(serviceProvider))
            {
                context.Database.EnsureCreated();

                context.Chips.Add(new KettleChips { BestBuyDate = DateTime.Now, Name = "Doritos Locos Tacos" });
                await context.SaveChangesAsync();
                Assert.Null(GlobalCommandTimeout);

                context.Database.SetCommandTimeout(88);

                context.Chips.Add(new KettleChips { BestBuyDate = DateTime.Now, Name = "Doritos Locos Tacos" });
                await context.SaveChangesAsync();
                Assert.Equal(88, GlobalCommandTimeout);
            }
        }

        public static int? GlobalCommandTimeout;

        public class TestSqliteModificationCommandBatch : SingularModificationCommandBatch
        {
            protected override DbCommand CreateStoreCommand(string commandText, IRelationalConnection connection, IRelationalTypeMapper typeMapper, int? commandTimeout)
            {
                GlobalCommandTimeout = commandTimeout;
                return base.CreateStoreCommand(commandText, connection, typeMapper, commandTimeout);
            }

            public TestSqliteModificationCommandBatch(IUpdateSqlGenerator sqlGenerator)
                : base(sqlGenerator)
            {
            }
        }

        public class TestSqliteModificationCommandBatchFactory : SqliteModificationCommandBatchFactory
        {
            public TestSqliteModificationCommandBatchFactory(
                IUpdateSqlGenerator sqlGenerator)
                : base(sqlGenerator)
            {
            }

            public override ModificationCommandBatch Create(
                IDbContextOptions options,
                IRelationalMetadataExtensionProvider metadataExtensionProvider) => new TestSqliteModificationCommandBatch(UpdateSqlGenerator);
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
                optionsBuilder.UseSqlite(SqliteTestStore.CreateConnectionString(DatabaseName));
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
            public ConfiguredChipsContext(IServiceProvider serviceProvider)
                : base(serviceProvider)
            {
            }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseSqlite($"Data Source=./{DatabaseName}.db");

                base.OnConfiguring(optionsBuilder);
            }
        }

        private static string Sql => TestSqlLoggerFactory.Sql;
    }
}
