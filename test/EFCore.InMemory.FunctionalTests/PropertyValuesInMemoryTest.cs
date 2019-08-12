// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class PropertyValuesInMemoryTest : PropertyValuesTestBase<PropertyValuesInMemoryTest.PropertyValuesInMemoryFixture>
    {
        public PropertyValuesInMemoryTest(PropertyValuesInMemoryFixture fixture)
            : base(fixture)
        {
        }

        [ConditionalFact(Skip = "Issue #16963")]
        public override Task Current_values_for_derived_object_can_be_copied_into_an_object()
            => Task.CompletedTask;

        [ConditionalFact(Skip = "Issue #16963")]
        public override Task GetDatabaseValuesAsync_for_derived_entity_not_in_the_store_returns_null()
            => Task.CompletedTask;

        [ConditionalFact(Skip = "Issue #16963")]
        public override Task GetDatabaseValuesAsync_for_the_wrong_type_in_the_store_returns_null()
            => Task.CompletedTask;

        [ConditionalFact(Skip = "Issue #16963")]
        public override Task GetDatabaseValues_for_derived_entity_not_in_the_store_returns_null()
            => Task.CompletedTask;

        [ConditionalFact(Skip = "Issue #16963")]
        public override Task GetDatabaseValues_for_the_wrong_type_in_the_store_returns_null()
            => Task.CompletedTask;

        [ConditionalFact(Skip = "Issue #16963")]
        public override Task NonGeneric_GetDatabaseValuesAsync_for_derived_entity_not_in_the_store_returns_null()
            => Task.CompletedTask;

        [ConditionalFact(Skip = "Issue #16963")]
        public override Task NonGeneric_GetDatabaseValuesAsync_for_the_wrong_type_in_the_store_throws()
            => Task.CompletedTask;

        [ConditionalFact(Skip = "Issue #16963")]
        public override Task NonGeneric_GetDatabaseValues_for_derived_entity_not_in_the_store_returns_null()
            => Task.CompletedTask;

        [ConditionalFact(Skip = "Issue #16963")]
        public override Task NonGeneric_GetDatabaseValues_for_the_wrong_type_in_the_store_throws()
            => Task.CompletedTask;

        [ConditionalFact(Skip = "Issue #16963")]
        public override Task Original_values_for_derived_object_can_be_copied_into_an_object()
            => Task.CompletedTask;

        [ConditionalFact(Skip = "Issue #16963")]
        public override Task Scalar_current_values_of_a_derived_object_can_be_accessed_as_a_non_generic_property_dictionary()
            => Task.CompletedTask;

        [ConditionalFact(Skip = "Issue #16963")]
        public override Task Scalar_current_values_of_a_derived_object_can_be_accessed_as_a_property_dictionary()
            => Task.CompletedTask;

        [ConditionalFact(Skip = "Issue #16963")]
        public override Task Scalar_original_values_of_a_derived_object_can_be_accessed_as_a_non_generic_property_dictionary()
            => Task.CompletedTask;

        [ConditionalFact(Skip = "Issue #16963")]
        public override Task Scalar_original_values_of_a_derived_object_can_be_accessed_as_a_property_dictionary()
            => Task.CompletedTask;

        [ConditionalFact(Skip = "Issue #16963")]
        public override Task Scalar_store_values_of_a_derived_object_can_be_accessed_as_a_non_generic_property_dictionary()
            => Task.CompletedTask;

        [ConditionalFact(Skip = "Issue #16963")]
        public override Task Scalar_store_values_of_a_derived_object_can_be_accessed_as_a_property_dictionary()
            => Task.CompletedTask;

        [ConditionalFact(Skip = "Issue #16963")]
        public override Task Scalar_store_values_of_a_derived_object_can_be_accessed_asynchronously_as_a_non_generic_property_dictionary()
            => Task.CompletedTask;

        [ConditionalFact(Skip = "Issue #16963")]
        public override Task Scalar_store_values_of_a_derived_object_can_be_accessed_asynchronously_as_a_property_dictionary()
            => Task.CompletedTask;

        [ConditionalFact(Skip = "Issue #16963")]
        public override Task Store_values_for_derived_object_can_be_copied_into_an_object()
            => Task.CompletedTask;

        [ConditionalFact(Skip = "Issue #16963")]
        public override Task Store_values_for_derived_object_can_be_copied_into_an_object_asynchronously()
            => Task.CompletedTask;

        [ConditionalFact(Skip = "Issue #16963")]
        public override void Using_bad_IProperty_instances_throws_derived()
        {
        }

        [ConditionalFact(Skip = "Issue #16963")]
        public override void Using_bad_property_names_throws_derived()
        {
        }

        public class PropertyValuesInMemoryFixture : PropertyValuesFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => InMemoryTestStoreFactory.Instance;
        }
    }
}
