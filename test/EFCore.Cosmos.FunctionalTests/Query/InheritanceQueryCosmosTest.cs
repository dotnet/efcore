// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class InheritanceQueryCosmosTest : InheritanceQueryTestBase<InheritanceQueryCosmosFixture>
    {
        public InheritanceQueryCosmosTest(InheritanceQueryCosmosFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            ClearLog();
            //TestLoggerFactory.TestOutputHelper = testOutputHelper;
        }

        public override async Task Can_query_when_shared_column(bool async)
        {
            await base.Can_query_when_shared_column(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Coke"")
OFFSET 0 LIMIT 2",
                //
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Lilt"")
OFFSET 0 LIMIT 2",
                //
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Tea"")
OFFSET 0 LIMIT 2");
        }

        public override async Task Can_query_all_types_when_shared_column(bool async)
        {
            await base.Can_query_all_types_when_shared_column(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE c[""Discriminator""] IN (""Drink"", ""Coke"", ""Lilt"", ""Tea"")");
        }

        public override async Task Can_use_of_type_animal(bool async)
        {
            await base.Can_use_of_type_animal(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE c[""Discriminator""] IN (""Eagle"", ""Kiwi"")
ORDER BY c[""Species""]");
        }

        public override async Task Can_use_is_kiwi(bool async)
        {
            await base.Can_use_is_kiwi(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] IN (""Eagle"", ""Kiwi"") AND (c[""Discriminator""] = ""Kiwi""))");
        }

        public override async Task Can_use_backwards_is_animal(bool async)
        {
            await base.Can_use_backwards_is_animal(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Kiwi"")");
        }

        public override async Task Can_use_is_kiwi_with_other_predicate(bool async)
        {
            await base.Can_use_is_kiwi_with_other_predicate(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] IN (""Eagle"", ""Kiwi"") AND ((c[""Discriminator""] = ""Kiwi"") AND (c[""CountryId""] = 1)))");
        }

        public override async Task Can_use_is_kiwi_in_projection(bool async)
        {
            await base.Can_use_is_kiwi_in_projection(async);

            AssertSql(
                @"SELECT VALUE {""c"" : (c[""Discriminator""] = ""Kiwi"")}
FROM root c
WHERE c[""Discriminator""] IN (""Eagle"", ""Kiwi"")");
        }

        public override async Task Can_use_of_type_bird(bool async)
        {
            await base.Can_use_of_type_bird(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] IN (""Eagle"", ""Kiwi"") AND c[""Discriminator""] IN (""Eagle"", ""Kiwi""))
ORDER BY c[""Species""]");
        }

        public override async Task Can_use_of_type_bird_predicate(bool async)
        {
            await base.Can_use_of_type_bird_predicate(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] IN (""Eagle"", ""Kiwi"") AND (c[""CountryId""] = 1)) AND c[""Discriminator""] IN (""Eagle"", ""Kiwi""))
ORDER BY c[""Species""]");
        }

        public override async Task Can_use_of_type_bird_with_projection(bool async)
        {
            await base.Can_use_of_type_bird_with_projection(async);

            AssertSql(
                @"SELECT c[""EagleId""]
FROM root c
WHERE (c[""Discriminator""] IN (""Eagle"", ""Kiwi"") AND c[""Discriminator""] IN (""Eagle"", ""Kiwi""))");
        }

        public override async Task Can_use_of_type_bird_first(bool async)
        {
            await base.Can_use_of_type_bird_first(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] IN (""Eagle"", ""Kiwi"") AND c[""Discriminator""] IN (""Eagle"", ""Kiwi""))
ORDER BY c[""Species""]
OFFSET 0 LIMIT 1");
        }

        public override async Task Can_use_of_type_kiwi(bool async)
        {
            await base.Can_use_of_type_kiwi(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] IN (""Eagle"", ""Kiwi"") AND (c[""Discriminator""] = ""Kiwi""))");
        }

        public override async Task Can_use_backwards_of_type_animal(bool async)
        {
            await base.Can_use_backwards_of_type_animal(async);

            AssertSql(" ");
        }

        public override async Task Can_use_of_type_rose(bool async)
        {
            await base.Can_use_of_type_rose(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] IN (""Daisy"", ""Rose"") AND (c[""Discriminator""] = ""Rose""))");
        }

        public override async Task Can_query_all_animals(bool async)
        {
            await base.Can_query_all_animals(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE c[""Discriminator""] IN (""Eagle"", ""Kiwi"")
ORDER BY c[""Species""]");
        }

        [ConditionalTheory(Skip = "Issue#17246 Views are not supported")]
        public override async Task Can_query_all_animal_views(bool async)
        {
            await base.Can_query_all_animal_views(async);

            AssertSql(" ");
        }

        public override async Task Can_query_all_plants(bool async)
        {
            await base.Can_query_all_plants(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE c[""Discriminator""] IN (""Daisy"", ""Rose"")
ORDER BY c[""Species""]");
        }

        public override async Task Can_filter_all_animals(bool async)
        {
            await base.Can_filter_all_animals(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] IN (""Eagle"", ""Kiwi"") AND (c[""Name""] = ""Great spotted kiwi""))
ORDER BY c[""Species""]");
        }

        public override async Task Can_query_all_birds(bool async)
        {
            await base.Can_query_all_birds(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE c[""Discriminator""] IN (""Eagle"", ""Kiwi"")
ORDER BY c[""Species""]");
        }

        public override async Task Can_query_just_kiwis(bool async)
        {
            await base.Can_query_just_kiwis(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Kiwi"")
OFFSET 0 LIMIT 2");
        }

        public override async Task Can_query_just_roses(bool async)
        {
            await base.Can_query_just_roses(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Rose"")
OFFSET 0 LIMIT 2");
        }

        [ConditionalTheory(Skip = "Issue#17246 Non-embedded Include")]
        public override async Task Can_include_animals(bool async)
        {
            await base.Can_include_animals(async);

            AssertSql(" ");
        }

        [ConditionalTheory(Skip = "Issue#17246 Non-embedded Include")]
        public override async Task Can_include_prey(bool async)
        {
            await base.Can_include_prey(async);

            AssertSql(" ");
        }

        public override async Task Can_use_of_type_kiwi_where_south_on_derived_property(bool async)
        {
            await base.Can_use_of_type_kiwi_where_south_on_derived_property(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] IN (""Eagle"", ""Kiwi"") AND (c[""Discriminator""] = ""Kiwi"")) AND (c[""FoundOn""] = 1))");
        }

        public override async Task Can_use_of_type_kiwi_where_north_on_derived_property(bool async)
        {
            await base.Can_use_of_type_kiwi_where_north_on_derived_property(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] IN (""Eagle"", ""Kiwi"") AND (c[""Discriminator""] = ""Kiwi"")) AND (c[""FoundOn""] = 0))");
        }

        public override async Task Discriminator_used_when_projection_over_derived_type(bool async)
        {
            await base.Discriminator_used_when_projection_over_derived_type(async);

            AssertSql(
                @"SELECT c[""FoundOn""]
FROM root c
WHERE (c[""Discriminator""] = ""Kiwi"")");
        }

        public override async Task Discriminator_used_when_projection_over_derived_type2(bool async)
        {
            await base.Discriminator_used_when_projection_over_derived_type2(async);

            AssertSql(
                @"SELECT c[""IsFlightless""], c[""Discriminator""]
FROM root c
WHERE c[""Discriminator""] IN (""Eagle"", ""Kiwi"")");
        }

        public override async Task Discriminator_with_cast_in_shadow_property(bool async)
        {
            await base.Discriminator_with_cast_in_shadow_property(async);

            AssertSql(
                @"SELECT VALUE {""Predator"" : c[""EagleId""]}
FROM root c
WHERE (c[""Discriminator""] IN (""Eagle"", ""Kiwi"") AND (""Kiwi"" = c[""Discriminator""]))");
        }

        public override async Task Discriminator_used_when_projection_over_of_type(bool async)
        {
            await base.Discriminator_used_when_projection_over_of_type(async);

            AssertSql(
                @"SELECT c[""FoundOn""]
FROM root c
WHERE (c[""Discriminator""] IN (""Eagle"", ""Kiwi"") AND (c[""Discriminator""] = ""Kiwi""))");
        }

        [ConditionalFact(Skip = "Issue#17246 Transations not supported")]
        public override void Can_insert_update_delete()
        {
            base.Can_insert_update_delete();

            AssertSql(" ");
        }

        public override async Task Union_siblings_with_duplicate_property_in_subquery(bool async)
        {
            await base.Union_siblings_with_duplicate_property_in_subquery(async);

            AssertSql(" ");
        }

        public override async Task OfType_Union_subquery(bool async)
        {
            await base.OfType_Union_subquery(async);

            AssertSql(" ");
        }

        public override async Task OfType_Union_OfType(bool async)
        {
            await base.OfType_Union_OfType(async);

            AssertSql(" ");
        }

        public override async Task Subquery_OfType(bool async)
        {
            await base.Subquery_OfType(async);

            AssertSql(
                @"@__p_0='5'

SELECT DISTINCT c
FROM root c
WHERE (c[""Discriminator""] IN (""Eagle"", ""Kiwi"") AND (c[""Discriminator""] = ""Kiwi""))
ORDER BY c[""Species""]
OFFSET 0 LIMIT @__p_0");
        }

        public override async Task Union_entity_equality(bool async)
        {
            await base.Union_entity_equality(async);

            AssertSql(" ");
        }

        public override void Setting_foreign_key_to_a_different_type_throws()
        {
            base.Setting_foreign_key_to_a_different_type_throws();

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Kiwi"")
OFFSET 0 LIMIT 2");
        }

        public override async Task Byte_enum_value_constant_used_in_projection(bool async)
        {
            await base.Byte_enum_value_constant_used_in_projection(async);

            AssertSql(
                @"SELECT VALUE {""c"" : (c[""IsFlightless""] ? 0 : 1)}
FROM root c
WHERE (c[""Discriminator""] = ""Kiwi"")");
        }

        public override void Member_access_on_intermediate_type_works()
        {
            base.Member_access_on_intermediate_type_works();

            AssertSql(
                @"SELECT c[""Name""]
FROM root c
WHERE (c[""Discriminator""] = ""Kiwi"")
ORDER BY c[""Name""]");
        }

        [ConditionalTheory(Skip = "Issue#17246 subquery usage")]
        public override async Task Is_operator_on_result_of_FirstOrDefault(bool async)
        {
            await base.Is_operator_on_result_of_FirstOrDefault(async);

            AssertSql(" ");
        }

        public override async Task Selecting_only_base_properties_on_base_type(bool async)
        {
            await base.Selecting_only_base_properties_on_base_type(async);

            AssertSql(
                @"SELECT c[""Name""]
FROM root c
WHERE c[""Discriminator""] IN (""Eagle"", ""Kiwi"")");
        }

        public override async Task Selecting_only_base_properties_on_derived_type(bool async)
        {
            await base.Selecting_only_base_properties_on_derived_type(async);

            AssertSql(
                @"SELECT c[""Name""]
FROM root c
WHERE c[""Discriminator""] IN (""Eagle"", ""Kiwi"")");
        }

        protected override bool EnforcesFkConstraints
            => false;

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        protected override void ClearLog()
            => Fixture.TestSqlLoggerFactory.Clear();
    }
}
