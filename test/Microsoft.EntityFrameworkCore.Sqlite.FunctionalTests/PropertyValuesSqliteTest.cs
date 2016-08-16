// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Sqlite.FunctionalTests
{
    public class PropertyValuesSqliteTest
        : PropertyValuesTestBase<SqliteTestStore, PropertyValuesSqliteTest.PropertyValuesSqliteFixture>
    {
        public PropertyValuesSqliteTest(PropertyValuesSqliteFixture fixture)
            : base(fixture)
        {
        }

        // Disabled due to Issue #6337
        public override Task Scalar_store_values_of_a_derived_object_can_be_accessed_as_a_property_dictionary() => Task.FromResult(true);
        public override Task Scalar_store_values_of_a_derived_object_can_be_accessed_asynchronously_as_a_property_dictionary() => Task.FromResult(true);
        public override Task Store_values_can_be_copied_into_a_cloned_dictionary() => Task.FromResult(true);
        public override Task Store_values_can_be_copied_into_a_cloned_dictionary_asynchronously() => Task.FromResult(true);
        public override Task Scalar_store_values_can_be_accessed_as_a_property_dictionary_using_IProperty() => Task.FromResult(true);
        public override Task Scalar_store_values_can_be_accessed_asynchronously_as_a_property_dictionary_using_IProperty() => Task.FromResult(true);
        public override Task Scalar_store_values_of_a_derived_object_can_be_accessed_as_a_non_generic_property_dictionary() => Task.FromResult(true);
        public override Task Scalar_store_values_of_a_derived_object_can_be_accessed_asynchronously_as_a_non_generic_property_dictionary() => Task.FromResult(true);
        public override Task Scalar_store_values_can_be_accessed_as_a_non_generic_property_dictionary() => Task.FromResult(true);
        public override Task Scalar_store_values_can_be_accessed_asynchronously_as_a_non_generic_property_dictionary() => Task.FromResult(true);
        public override Task Store_values_really_are_store_values_not_current_or_original_values() => Task.FromResult(true);
        public override Task Store_values_really_are_store_values_not_current_or_original_values_async() => Task.FromResult(true);
        public override Task Store_values_can_be_copied_into_an_object() => Task.FromResult(true);
        public override Task Store_values_can_be_copied_into_an_object_asynchronously() => Task.FromResult(true);
        public override Task Scalar_store_values_can_be_accessed_as_a_non_generic_property_dictionary_using_IProperty() => Task.FromResult(true);
        public override Task Scalar_store_values_can_be_accessed_asynchronously_as_a_non_generic_property_dictionary_using_IProperty() => Task.FromResult(true);
        public override Task Store_values_for_derived_object_can_be_copied_into_an_object() => Task.FromResult(true);
        public override Task Store_values_for_derived_object_can_be_copied_into_an_object_asynchronously() => Task.FromResult(true);
        public override Task Scalar_store_values_can_be_accessed_as_a_property_dictionary() => Task.FromResult(true);
        public override Task Scalar_store_values_can_be_accessed_asynchronously_as_a_property_dictionary() => Task.FromResult(true);
        public override Task Store_values_can_be_copied_into_a_non_generic_cloned_dictionary() => Task.FromResult(true);
        public override Task Store_values_can_be_copied_asynchronously_into_a_non_generic_cloned_dictionary() => Task.FromResult(true);
        public override Task Store_values_can_be_copied_non_generic_property_dictionary_into_an_object() => Task.FromResult(true);
        public override Task Store_values_can_be_copied_asynchronously_non_generic_property_dictionary_into_an_object() => Task.FromResult(true);

        public class PropertyValuesSqliteFixture : PropertyValuesFixtureBase
        {
            private const string DatabaseName = "PropertyValues";

            private readonly IServiceProvider _serviceProvider;

            public PropertyValuesSqliteFixture()
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

                        using (var context = new AdvancedPatternsMasterContext(optionsBuilder.Options))
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

                var context = new AdvancedPatternsMasterContext(optionsBuilder.Options);
                context.Database.UseTransaction(testStore.Transaction);

                return context;
            }
        }
    }
}
