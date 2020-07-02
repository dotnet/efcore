// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.


// ReSharper disable InconsistentNaming
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class TPTInheritanceQueryTestBase<TFixture> : InheritanceQueryTestBase<TFixture>
        where TFixture : TPTInheritanceQueryFixture, new()
    {
        public TPTInheritanceQueryTestBase(TFixture fixture)
            : base(fixture)
        {
        }

        [ConditionalTheory(Skip = "Issue#2266")]
        public override Task Byte_enum_value_constant_used_in_projection(bool async)
        {
            return base.Byte_enum_value_constant_used_in_projection(async);
        }

        [ConditionalTheory(Skip = "Issue#2266")]
        public override Task Can_filter_all_animals(bool async)
        {
            return base.Can_filter_all_animals(async);
        }

        [ConditionalTheory(Skip = "Issue#2266")]
        public override Task Can_include_animals(bool async)
        {
            return base.Can_include_animals(async);
        }

        [ConditionalTheory(Skip = "Issue#2266")]
        public override Task Can_include_prey(bool async)
        {
            return base.Can_include_prey(async);
        }

        [ConditionalTheory(Skip = "Issue#2266")]
        public override void Can_insert_update_delete()
        {
            base.Can_insert_update_delete();
        }

        [ConditionalTheory(Skip = "Issue#2266")]
        public override Task Can_query_all_animals(bool async)
        {
            return base.Can_query_all_animals(async);
        }

        [ConditionalTheory(Skip = "Issue#2266")]
        public override Task Can_query_all_animal_views(bool async)
        {
            return base.Can_query_all_animal_views(async);
        }

        [ConditionalTheory(Skip = "Issue#2266")]
        public override Task Can_query_all_birds(bool async)
        {
            return base.Can_query_all_birds(async);
        }

        [ConditionalTheory(Skip = "Issue#2266")]
        public override Task Can_query_all_plants(bool async)
        {
            return base.Can_query_all_plants(async);
        }

        [ConditionalTheory(Skip = "Issue#2266")]
        public override Task Can_query_all_types_when_shared_column(bool async)
        {
            return base.Can_query_all_types_when_shared_column(async);
        }

        [ConditionalTheory(Skip = "Issue#2266")]
        public override Task Can_query_just_kiwis(bool async)
        {
            return base.Can_query_just_kiwis(async);
        }

        [ConditionalTheory(Skip = "Issue#2266")]
        public override Task Can_query_just_roses(bool async)
        {
            return base.Can_query_just_roses(async);
        }

        [ConditionalTheory(Skip = "Issue#2266")]
        public override Task Can_query_when_shared_column(bool async)
        {
            return base.Can_query_when_shared_column(async);
        }

        [ConditionalTheory(Skip = "Issue#2266")]
        public override Task Can_use_backwards_is_animal(bool async)
        {
            return base.Can_use_backwards_is_animal(async);
        }

        [ConditionalTheory(Skip = "Issue#2266")]
        public override Task Can_use_backwards_of_type_animal(bool async)
        {
            return base.Can_use_backwards_of_type_animal(async);
        }

        [ConditionalTheory(Skip = "Issue#2266")]
        public override Task Can_use_is_kiwi(bool async)
        {
            return base.Can_use_is_kiwi(async);
        }

        [ConditionalTheory(Skip = "Issue#2266")]
        public override Task Can_use_is_kiwi_in_projection(bool async)
        {
            return base.Can_use_is_kiwi_in_projection(async);
        }

        [ConditionalTheory(Skip = "Issue#2266")]
        public override Task Can_use_is_kiwi_with_other_predicate(bool async)
        {
            return base.Can_use_is_kiwi_with_other_predicate(async);
        }

        [ConditionalTheory(Skip = "Issue#2266")]
        public override Task Can_use_of_type_animal(bool async)
        {
            return base.Can_use_of_type_animal(async);
        }

        [ConditionalTheory(Skip = "Issue#2266")]
        public override Task Can_use_of_type_bird(bool async)
        {
            return base.Can_use_of_type_bird(async);
        }

        [ConditionalTheory(Skip = "Issue#2266")]
        public override Task Can_use_of_type_bird_first(bool async)
        {
            return base.Can_use_of_type_bird_first(async);
        }

        [ConditionalTheory(Skip = "Issue#2266")]
        public override Task Can_use_of_type_bird_predicate(bool async)
        {
            return base.Can_use_of_type_bird_predicate(async);
        }

        [ConditionalTheory(Skip = "Issue#2266")]
        public override Task Can_use_of_type_bird_with_projection(bool async)
        {
            return base.Can_use_of_type_bird_with_projection(async);
        }

        [ConditionalTheory(Skip = "Issue#2266")]
        public override Task Can_use_of_type_kiwi(bool async)
        {
            return base.Can_use_of_type_kiwi(async);
        }

        [ConditionalTheory(Skip = "Issue#2266")]
        public override Task Can_use_of_type_kiwi_where_north_on_derived_property(bool async)
        {
            return base.Can_use_of_type_kiwi_where_north_on_derived_property(async);
        }

        [ConditionalTheory(Skip = "Issue#2266")]
        public override Task Can_use_of_type_kiwi_where_south_on_derived_property(bool async)
        {
            return base.Can_use_of_type_kiwi_where_south_on_derived_property(async);
        }

        [ConditionalTheory(Skip = "Issue#2266")]
        public override Task Can_use_of_type_rose(bool async)
        {
            return base.Can_use_of_type_rose(async);
        }

        [ConditionalTheory(Skip = "Issue#2266")]
        public override Task Discriminator_used_when_projection_over_derived_type(bool async)
        {
            return base.Discriminator_used_when_projection_over_derived_type(async);
        }

        [ConditionalTheory(Skip = "Issue#2266")]
        public override Task Discriminator_used_when_projection_over_derived_type2(bool async)
        {
            return base.Discriminator_used_when_projection_over_derived_type2(async);
        }

        [ConditionalTheory(Skip = "Issue#2266")]
        public override Task Discriminator_used_when_projection_over_of_type(bool async)
        {
            return base.Discriminator_used_when_projection_over_of_type(async);
        }

        [ConditionalTheory(Skip = "Issue#2266")]
        public override Task Discriminator_with_cast_in_shadow_property(bool async)
        {
            return base.Discriminator_with_cast_in_shadow_property(async);
        }

        [ConditionalFact(Skip = "Issue#2266")]
        public override void Member_access_on_intermediate_type_works()
        {
            base.Member_access_on_intermediate_type_works();
        }

        [ConditionalTheory(Skip = "Issue#2266")]
        public override Task OfType_Union_OfType(bool async)
        {
            return base.OfType_Union_OfType(async);
        }

        [ConditionalTheory(Skip = "Issue#2266")]
        public override Task OfType_Union_subquery(bool async)
        {
            return base.OfType_Union_subquery(async);
        }

        [ConditionalFact(Skip = "Issue#2266")]
        public override void Setting_foreign_key_to_a_different_type_throws()
        {
            base.Setting_foreign_key_to_a_different_type_throws();
        }

        [ConditionalTheory(Skip = "Issue#2266")]
        public override Task Subquery_OfType(bool async)
        {
            return base.Subquery_OfType(async);
        }

        [ConditionalTheory(Skip = "Issue#2266")]
        public override Task Union_entity_equality(bool async)
        {
            return base.Union_entity_equality(async);
        }

        [ConditionalTheory(Skip = "Issue#2266")]
        public override Task Union_siblings_with_duplicate_property_in_subquery(bool async)
        {
            return base.Union_siblings_with_duplicate_property_in_subquery(async);
        }
    }
}
