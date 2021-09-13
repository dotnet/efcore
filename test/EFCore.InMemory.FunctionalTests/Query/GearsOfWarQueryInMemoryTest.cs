// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.InMemory.Internal;
using Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class GearsOfWarQueryInMemoryTest : GearsOfWarQueryTestBase<GearsOfWarQueryInMemoryFixture>
    {
        public GearsOfWarQueryInMemoryTest(GearsOfWarQueryInMemoryFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            //TestLoggerFactory.TestOutputHelper = testOutputHelper;
        }

        public override Task Client_member_and_unsupported_string_Equals_in_the_same_query(bool async)
        {
            return AssertTranslationFailedWithDetails(() => base.Client_member_and_unsupported_string_Equals_in_the_same_query(async),
                CoreStrings.QueryUnableToTranslateMember(nameof(Gear.IsMarcus), nameof(Gear)));
        }

        [ConditionalTheory(Skip = "issue #17540")]
        public override Task
            Null_semantics_is_correctly_applied_for_function_comparisons_that_take_arguments_from_optional_navigation_complex(bool async)
            => base.Null_semantics_is_correctly_applied_for_function_comparisons_that_take_arguments_from_optional_navigation_complex(
                async);

        [ConditionalTheory(Skip = "issue #19683")]
        public override Task Group_by_on_StartsWith_with_null_parameter_as_argument(bool async)
            => base.Group_by_on_StartsWith_with_null_parameter_as_argument(async);

        [ConditionalTheory(Skip = "issue #24325")]
        public override Task Projecting_entity_as_well_as_correlated_collection_followed_by_Distinct(bool async)
            => base.Projecting_entity_as_well_as_correlated_collection_followed_by_Distinct(async);

        [ConditionalTheory(Skip = "issue #24325")]
        public override Task Projecting_entity_as_well_as_complex_correlated_collection_followed_by_Distinct(bool async)
            => base.Projecting_entity_as_well_as_complex_correlated_collection_followed_by_Distinct(async);

        [ConditionalTheory(Skip = "issue #24325")]
        public override Task Projecting_entity_as_well_as_correlated_collection_of_scalars_followed_by_Distinct(bool async)
            => base.Projecting_entity_as_well_as_correlated_collection_of_scalars_followed_by_Distinct(async);

        [ConditionalTheory(Skip = "issue #24325")]
        public override Task Correlated_collection_with_distinct_3_levels(bool async)
            => base.Correlated_collection_with_distinct_3_levels(async);

        public override async Task Projecting_correlated_collection_followed_by_Distinct(bool async)
        {
            var message = (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Projecting_correlated_collection_followed_by_Distinct(async))).Message;

            Assert.Equal(InMemoryStrings.DistinctOnSubqueryNotSupported, message);
        }

        public override async Task Projecting_some_properties_as_well_as_correlated_collection_followed_by_Distinct(bool async)
        {
            var message = (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Projecting_some_properties_as_well_as_correlated_collection_followed_by_Distinct(async))).Message;

            Assert.Equal(InMemoryStrings.DistinctOnSubqueryNotSupported, message);
        }
    }
}
