// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class StoreGeneratedSqlServerTest
        : StoreGeneratedTestBase<SqlServerTestStore, StoreGeneratedSqlServerTest.StoreGeneratedSqlServerFixture>
    {
        public StoreGeneratedSqlServerTest(StoreGeneratedSqlServerFixture fixture)
            : base(fixture)
        {
        }

        public class StoreGeneratedSqlServerFixture : StoreGeneratedFixtureBase
        {
            private const string DatabaseName = "StoreGeneratedTest";

            private readonly IServiceProvider _serviceProvider;

            public StoreGeneratedSqlServerFixture()
            {
                _serviceProvider = new ServiceCollection()
                    .AddEntityFramework()
                    .AddSqlServer()
                    .ServiceCollection()
                    .AddSingleton(TestSqlServerModelSource.GetFactory(OnModelCreating))
                    .BuildServiceProvider();
            }

            public override SqlServerTestStore CreateTestStore()
            {
                return SqlServerTestStore.GetOrCreateShared(DatabaseName, () =>
                    {
                        var optionsBuilder = new DbContextOptionsBuilder();
                        optionsBuilder.UseSqlServer(SqlServerTestStore.CreateConnectionString(DatabaseName));

                        using (var context = new StoreGeneratedContext(_serviceProvider, optionsBuilder.Options))
                        {
                            context.Database.EnsureDeleted();
                            context.Database.EnsureCreated();
                        }
                    });
            }

            public override DbContext CreateContext(SqlServerTestStore testStore)
            {
                var optionsBuilder = new DbContextOptionsBuilder();
                optionsBuilder.UseSqlServer(testStore.Connection);

                var context = new StoreGeneratedContext(_serviceProvider, optionsBuilder.Options);
                context.Database.UseTransaction(testStore.Transaction);

                return context;
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                modelBuilder.Entity<Gumball>(b =>
                    {
                        b.Property(e => e.Id)
                            .UseSqlServerIdentityColumn();

                        b.Property(e => e.Identity)
                            .HasDefaultValue("Banana Joe");

                        b.Property(e => e.IdentityReadOnlyBeforeSave)
                            .HasDefaultValue("Doughnut Sheriff");

                        b.Property(e => e.IdentityReadOnlyAfterSave)
                            .HasDefaultValue("Anton");

                        b.Property(e => e.AlwaysIdentity)
                            .HasDefaultValue("Banana Joe");

                        b.Property(e => e.AlwaysIdentityReadOnlyBeforeSave)
                            .HasDefaultValue("Doughnut Sheriff");

                        b.Property(e => e.AlwaysIdentityReadOnlyAfterSave)
                            .HasDefaultValue("Anton");

                        b.Property(e => e.Computed)
                            .HasDefaultValue("Alan");

                        b.Property(e => e.ComputedReadOnlyBeforeSave)
                            .HasDefaultValue("Carmen");

                        b.Property(e => e.ComputedReadOnlyAfterSave)
                            .HasDefaultValue("Tina Rex");

                        b.Property(e => e.AlwaysComputed)
                            .HasDefaultValue("Alan");

                        b.Property(e => e.AlwaysComputedReadOnlyBeforeSave)
                            .HasDefaultValue("Carmen");

                        b.Property(e => e.AlwaysComputedReadOnlyAfterSave)
                            .HasDefaultValue("Tina Rex");
                    });
            }
        }
    }
}
