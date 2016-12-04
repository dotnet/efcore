// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Sqlite.FunctionalTests
{
    public abstract class FieldMappingSqliteTest
    {
        public abstract class FieldMappingSqliteTestBase<TFixture> : FieldMappingTestBase<SqliteTestStore, TFixture>
            where TFixture : FieldMappingSqliteTestBase<TFixture>.FieldMappingSqliteFixtureBase, new()
        {
            protected FieldMappingSqliteTestBase(TFixture fixture)
                : base(fixture)
            {
            }

            protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
                => facade.UseTransaction(transaction.GetDbTransaction());

            public abstract class FieldMappingSqliteFixtureBase : FieldMappingFixtureBase
            {
                protected abstract string DatabaseName { get; }

                private readonly IServiceProvider _serviceProvider;

                protected FieldMappingSqliteFixtureBase()
                {
                    _serviceProvider = new ServiceCollection()
                        .AddEntityFrameworkSqlite()
                        .AddSingleton(TestSqliteModelSource.GetFactory(OnModelCreating))
                        .BuildServiceProvider();
                }

                public override SqliteTestStore CreateTestStore()
                {
                    return SqliteTestStore.GetOrCreateShared(DatabaseName, () =>
                        {
                            var optionsBuilder = new DbContextOptionsBuilder()
                                .UseSqlite(SqliteTestStore.CreateConnectionString(DatabaseName))
                                .UseInternalServiceProvider(_serviceProvider);

                            using (var context = new FieldMappingContext(optionsBuilder.Options))
                            {
                                context.Database.EnsureClean();
                                Seed(context);
                            }
                        });
                }

                public override DbContext CreateContext(SqliteTestStore testStore)
                {
                    var optionsBuilder = new DbContextOptionsBuilder()
                        .UseSqlite(testStore.Connection)
                        .UseInternalServiceProvider(_serviceProvider);

                    var context = new FieldMappingContext(optionsBuilder.Options);
                    context.Database.UseTransaction(testStore.Transaction);

                    return context;
                }
            }
        }

        public class DefaultMappingTest
            : FieldMappingSqliteTestBase<DefaultMappingTest.DefaultMappingFixture>
        {
            public DefaultMappingTest(DefaultMappingFixture fixture)
                : base(fixture)
            {
            }

            public class DefaultMappingFixture : FieldMappingSqliteFixtureBase
            {
                protected override string DatabaseName => "FieldMappingTest";
            }
        }

        public class EnforceFieldTest
            : FieldMappingSqliteTestBase<EnforceFieldTest.EnforceFieldFixture>
        {
            public EnforceFieldTest(EnforceFieldFixture fixture)
                : base(fixture)
            {
            }

            public class EnforceFieldFixture : FieldMappingSqliteFixtureBase
            {
                protected override string DatabaseName => "FieldMappingEnforceFieldTest";

                protected override void OnModelCreating(ModelBuilder modelBuilder)
                {
                    modelBuilder.UsePropertyAccessMode(PropertyAccessMode.Field);
                    base.OnModelCreating(modelBuilder);
                }
            }
        }

        public class EnforceFieldForQueryTest
            : FieldMappingSqliteTestBase<EnforceFieldForQueryTest.EnforceFieldForQueryFixture>
        {
            public EnforceFieldForQueryTest(EnforceFieldForQueryFixture fixture)
                : base(fixture)
            {
            }

            public class EnforceFieldForQueryFixture : FieldMappingSqliteFixtureBase
            {
                protected override string DatabaseName => "FieldMappingFieldQueryTest";

                protected override void OnModelCreating(ModelBuilder modelBuilder)
                {
                    modelBuilder.UsePropertyAccessMode(PropertyAccessMode.FieldDuringConstruction);
                    base.OnModelCreating(modelBuilder);
                }
            }
        }

        public class EnforcePropertyTest
            : FieldMappingSqliteTestBase<EnforcePropertyTest.EnforcePropertyFixture>
        {
            public EnforcePropertyTest(EnforcePropertyFixture fixture)
                : base(fixture)
            {
            }

            // Cannot force property access when properties missing getter/setter
            public override void Include_collection_read_only_props()
            {
            }

            public override void Include_reference_read_only_props()
            {
            }

            public override void Load_collection_read_only_props()
            {
            }

            public override void Load_reference_read_only_props()
            {
            }

            public override void Query_with_conditional_constant_read_only_props()
            {
            }

            public override void Query_with_conditional_param_read_only_props()
            {
            }

            public override void Projection_read_only_props()
            {
            }

            public override void Update_read_only_props()
            {
            }

            public override void Include_collection_read_only_props_with_named_fields()
            {
            }

            public override void Include_reference_read_only_props_with_named_fields()
            {
            }

            public override void Load_collection_read_only_props_with_named_fields()
            {
            }

            public override void Load_reference_read_only_props_with_named_fields()
            {
            }

            public override void Query_with_conditional_constant_read_only_props_with_named_fields()
            {
            }

            public override void Query_with_conditional_param_read_only_props_with_named_fields()
            {
            }

            public override void Projection_read_only_props_with_named_fields()
            {
            }

            public override void Update_read_only_props_with_named_fields()
            {
            }

            public override void Include_collection_write_only_props()
            {
            }

            public override void Include_reference_write_only_props()
            {
            }

            public override void Load_collection_write_only_props()
            {
            }

            public override void Load_reference_write_only_props()
            {
            }

            public override void Query_with_conditional_constant_write_only_props()
            {
            }

            public override void Query_with_conditional_param_write_only_props()
            {
            }

            public override void Projection_write_only_props()
            {
            }

            public override void Update_write_only_props()
            {
            }

            public override void Include_collection_write_only_props_with_named_fields()
            {
            }

            public override void Include_reference_write_only_props_with_named_fields()
            {
            }

            public override void Load_collection_write_only_props_with_named_fields()
            {
            }

            public override void Load_reference_write_only_props_with_named_fields()
            {
            }

            public override void Query_with_conditional_constant_write_only_props_with_named_fields()
            {
            }

            public override void Query_with_conditional_param_write_only_props_with_named_fields()
            {
            }

            public override void Projection_write_only_props_with_named_fields()
            {
            }

            public override void Update_write_only_props_with_named_fields()
            {
            }

            public override void Include_collection_fields_only()
            {
            }

            public override void Include_reference_fields_only()
            {
            }

            public override void Load_collection_fields_only()
            {
            }

            public override void Load_reference_fields_only()
            {
            }

            public override void Query_with_conditional_constant_fields_only()
            {
            }

            public override void Query_with_conditional_param_fields_only()
            {
            }

            public override void Projection_fields_only()
            {
            }

            public override void Update_fields_only()
            {
            }

            public class EnforcePropertyFixture : FieldMappingSqliteFixtureBase
            {
                protected override string DatabaseName => "FieldMappingEnforcePropertyTest";

                protected override void OnModelCreating(ModelBuilder modelBuilder)
                {
                    modelBuilder.UsePropertyAccessMode(PropertyAccessMode.Property);
                    base.OnModelCreating(modelBuilder);
                }
            }
        }
    }
}
