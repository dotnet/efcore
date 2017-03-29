// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests.Utilities;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests.DatabaseCreatorTest
{
    public class SqlServerDatabaseCreatorTestCommon
    {
        public static IRelationalDatabaseCreator GetDatabaseCreator(SqlServerTestStore testStore)
            => new BloggingContext(testStore).GetInfrastructure().GetRequiredService<IRelationalDatabaseCreator>();

        // ReSharper disable once ClassNeverInstantiated.Local
        private class TestSqlServerExecutionStrategyFactory : SqlServerExecutionStrategyFactory
        {
            public TestSqlServerExecutionStrategyFactory(ExecutionStrategyContextDependencies dependencies)
                : base(dependencies)
            {
            }

            protected override IExecutionStrategy CreateDefaultStrategy(ExecutionStrategyContext context) => NoopExecutionStrategy.Instance;
        }

        public class TestDatabaseCreator : SqlServerDatabaseCreator
        {
            public TestDatabaseCreator(
                RelationalDatabaseCreatorDependencies dependencies,
                ISqlServerConnection connection,
                IRawSqlCommandBuilder rawSqlCommandBuilder)
                : base(dependencies, connection, rawSqlCommandBuilder)
            {
            }

            public bool HasTablesBase() => HasTables();

            public Task<bool> HasTablesAsyncBase(CancellationToken cancellationToken = default(CancellationToken))
                => HasTablesAsync(cancellationToken);

            public IExecutionStrategyFactory ExecutionStrategyFactory => Dependencies.ExecutionStrategyFactory;
        }

        private static IServiceProvider CreateServiceProvider()
            => new ServiceCollection()
            .AddEntityFrameworkSqlServer()
            .AddScoped<IExecutionStrategyFactory, TestSqlServerExecutionStrategyFactory>()
            .AddScoped<IRelationalDatabaseCreator, TestDatabaseCreator>()
            .BuildServiceProvider();

        public class BloggingContext : DbContext
        {
            private readonly SqlServerTestStore _testStore;

            public BloggingContext(SqlServerTestStore testStore)
            {
                _testStore = testStore;
            }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .UseSqlServer(_testStore.ConnectionString, b => b.ApplyConfiguration().CommandTimeout(600))
                    .UseInternalServiceProvider(CreateServiceProvider());

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Blog>(b =>
                {
                    b.HasKey(e => new { e.Key1, e.Key2 });
                    b.Property(e => e.AndRow).IsConcurrencyToken().ValueGeneratedOnAddOrUpdate();
                });
            }

            public DbSet<Blog> Blogs { get; set; }
        }

        public class Blog
        {
            public string Key1 { get; set; }
            public byte[] Key2 { get; set; }
            public string Cheese { get; set; }
            public int ErMilan { get; set; }
            public bool George { get; set; }
            public Guid TheGu { get; set; }
            public DateTime NotFigTime { get; set; }
            public byte ToEat { get; set; }
            public char CupOfChar { get; set; }
            public double OrNothing { get; set; }
            public short Fuse { get; set; }
            public long WayRound { get; set; }
            public float On { get; set; }
            public byte[] AndChew { get; set; }
            public byte[] AndRow { get; set; }
        }
    }
}
