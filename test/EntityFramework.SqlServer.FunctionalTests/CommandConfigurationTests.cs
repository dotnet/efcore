// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Query;
using Microsoft.Data.Entity.Relational.Query.Expressions;
using Microsoft.Data.Entity.Relational.Query.Methods;
using Microsoft.Data.Entity.Relational.Update;
using Microsoft.Data.Entity.SqlServer.Update;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;
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
                var commandBuilder = setupCommandBuilder(context);

                var relationalConnection = (IRelationalConnection)context.Database.Connection;
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
                var commandBuilder = setupCommandBuilder(context);

                var relationalConnection = (IRelationalConnection)context.Database.Connection;
                var command = commandBuilder.Build(relationalConnection, new Dictionary<string, object>());

                Assert.Equal(77, command.CommandTimeout);
            }
        }

        [Fact]
        public void Constructed_select_query_honors_latest_configured_commandTimeout_configured_in_context()
        {
            using (var context = new ConfiguredChipsContext(_serviceProvider, "KettleChips"))
            {
                var commandBuilder = setupCommandBuilder(context);

                context.Database.AsRelational().Connection.CommandTimeout = 88;
                var relationalConnection = (IRelationalConnection)context.Database.Connection;
                var command = commandBuilder.Build(relationalConnection, new Dictionary<string, object>());

                Assert.Equal(88, command.CommandTimeout);

                context.Database.AsRelational().Connection.CommandTimeout = 99;
                relationalConnection = (IRelationalConnection)context.Database.Connection;
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
                var commandBuilder = setupCommandBuilder(context);

                var relationalConnection = (IRelationalConnection)context.Database.Connection;
                context.Database.AsRelational().Connection.CommandTimeout = null;
                var command = commandBuilder.Build(relationalConnection, new Dictionary<string, object>());

                Assert.Equal(30, command.CommandTimeout);
            }
        }

        private CommandBuilder setupCommandBuilder(DbContext context)
        {
            var source = new EntityMaterializerSource(new MemberMapper(new FieldMatcher()));

            var loggerFactory = new LoggerFactory();

            var selectExpression = new SelectExpression();
            var queryCompilationContext = new RelationalQueryCompilationContext(
                context.Model,
                loggerFactory.Create("new"),
                new LinqOperatorProvider(),
                new RelationalResultOperatorHandler(),
                source,
                new EntityKeyFactorySource(),
                new AsyncQueryMethodProvider(),
                new CompositeMethodCallTranslator());

            return new CommandBuilder(selectExpression, queryCompilationContext);
        }

        [Fact]
        public void Constructed_update_statement_uses_default_when_commandTimeout_not_configured()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddSqlServer()
                .ServiceCollection()
                .AddSingleton<SqlServerModificationCommandBatchFactory, TestSqlServerModificationCommandBatchFactory>()
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
                .AddSingleton<SqlServerModificationCommandBatchFactory, TestSqlServerModificationCommandBatchFactory>()
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
                .AddSingleton<SqlServerModificationCommandBatchFactory, TestSqlServerModificationCommandBatchFactory>()
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
                .AddSingleton<SqlServerModificationCommandBatchFactory, TestSqlServerModificationCommandBatchFactory>()
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
                .AddSingleton<SqlServerModificationCommandBatchFactory, TestSqlServerModificationCommandBatchFactory>()
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
                .AddSingleton<SqlServerModificationCommandBatchFactory, TestSqlServerModificationCommandBatchFactory>()
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
                .AddSingleton<SqlServerModificationCommandBatchFactory, TestSqlServerModificationCommandBatchFactory>()
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
            protected override DbCommand CreateStoreCommand(string commandText, DbTransaction transaction, RelationalTypeMapper typeMapper, int? commandTimeout)
            {
                globalCommandTimeout = commandTimeout;
                return base.CreateStoreCommand(commandText, transaction, typeMapper, commandTimeout);
            }

            public TestSqlServerModificationCommandBatch(SqlServerSqlGenerator sqlGenerator, int? maxBatchSize)
                : base(sqlGenerator, maxBatchSize)
            {
            }
        }

        public class TestSqlServerModificationCommandBatchFactory : SqlServerModificationCommandBatchFactory
        {
            public TestSqlServerModificationCommandBatchFactory(SqlServerSqlGenerator sqlGenerator)
                : base(sqlGenerator)
            {
            }

            public override ModificationCommandBatch Create(IDbContextOptions options)
            {
                var optionsExtension = options.Extensions.OfType<SqlServerOptionsExtension>().FirstOrDefault();

                var maxBatchSize = optionsExtension?.MaxBatchSize;

                return new TestSqlServerModificationCommandBatch((SqlServerSqlGenerator)SqlGenerator, maxBatchSize);
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

            protected override void OnConfiguring(DbContextOptions options)
            {
                options.UseSqlServer(SqlServerTestStore.CreateConnectionString(_databaseName));
                base.OnConfiguring(options);
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

            protected override void OnConfiguring(DbContextOptions options)
            {
                options.UseSqlServer().CommandTimeout(77);
                base.OnConfiguring(options);
            }
        }
    }
}
