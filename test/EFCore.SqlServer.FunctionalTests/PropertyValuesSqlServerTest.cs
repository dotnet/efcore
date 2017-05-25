// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore
{
    public class PropertyValuesSqlServerTest
        : PropertyValuesTestBase<SqlServerTestStore, PropertyValuesSqlServerTest.PropertyValuesSqlServerFixture>
    {
        public PropertyValuesSqlServerTest(PropertyValuesSqlServerFixture fixture)
            : base(fixture)
        {
        }

        public class PropertyValuesSqlServerFixture : PropertyValuesFixtureBase
        {
            private const string DatabaseName = "PropertyValues";

            private readonly IServiceProvider _serviceProvider;

            public PropertyValuesSqlServerFixture()
            {
                _serviceProvider = new ServiceCollection()
                    .AddEntityFrameworkSqlServer()
                    .AddSingleton(TestModelSource.GetFactory(OnModelCreating))
                    .BuildServiceProvider();
            }

            public override SqlServerTestStore CreateTestStore()
                => SqlServerTestStore.GetOrCreateShared(DatabaseName, () =>
                    {
                        var optionsBuilder = new DbContextOptionsBuilder()
                            .UseSqlServer(SqlServerTestStore.CreateConnectionString(DatabaseName), b => b.ApplyConfiguration())
                            .UseInternalServiceProvider(_serviceProvider);

                        using (var context = new AdvancedPatternsMasterContext(optionsBuilder.Options))
                        {
                            context.Database.EnsureCreated();
                            Seed(context);
                        }
                    });

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                modelBuilder.Entity<Building>()
                    .Property(b => b.Value).HasColumnType("decimal(18,2)");

                modelBuilder.Entity<CurrentEmployee>()
                    .Property(ce => ce.LeaveBalance).HasColumnType("decimal(18,2)");
            }

            public override DbContext CreateContext(SqlServerTestStore testStore)
            {
                var optionsBuilder = new DbContextOptionsBuilder()
                    .UseSqlServer(testStore.Connection, b => b.ApplyConfiguration())
                    .UseInternalServiceProvider(_serviceProvider);

                var context = new AdvancedPatternsMasterContext(optionsBuilder.Options);
                context.Database.UseTransaction(testStore.Transaction);

                return context;
            }
        }
    }
}
