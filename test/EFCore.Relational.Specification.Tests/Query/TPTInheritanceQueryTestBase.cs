// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.


// ReSharper disable InconsistentNaming
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

        [ConditionalFact(Skip = "Issue#2266")]
        public override void Byte_enum_value_constant_used_in_projection()
        {
            base.Byte_enum_value_constant_used_in_projection();
        }

        [ConditionalFact(Skip = "Issue#2266")]
        public override void Can_filter_all_animals()
        {
            base.Can_filter_all_animals();
        }

        [ConditionalFact(Skip = "Issue#2266")]
        public override void Can_include_animals()
        {
            base.Can_include_animals();
        }

        [ConditionalFact(Skip = "Issue#2266")]
        public override void Can_include_prey()
        {
            base.Can_include_prey();
        }

        [ConditionalFact(Skip = "Issue#2266")]
        public override void Can_insert_update_delete()
        {
            base.Can_insert_update_delete();
        }

        [ConditionalFact(Skip = "Issue#2266")]
        public override void Can_query_all_animals()
        {
            base.Can_query_all_animals();
        }

        [ConditionalFact(Skip = "Issue#2266")]
        public override void Can_query_all_animal_views()
        {
            base.Can_query_all_animal_views();
        }

        [ConditionalFact(Skip = "Issue#2266")]
        public override void Can_query_all_birds()
        {
            base.Can_query_all_birds();
        }

        [ConditionalFact(Skip = "Issue#2266")]
        public override void Can_query_all_plants()
        {
            base.Can_query_all_plants();
        }

        [ConditionalFact(Skip = "Issue#2266")]
        public override void Can_query_all_types_when_shared_column()
        {
            base.Can_query_all_types_when_shared_column();
        }

        [ConditionalFact(Skip = "Issue#2266")]
        public override void Can_query_just_kiwis()
        {
            base.Can_query_just_kiwis();
        }

        [ConditionalFact(Skip = "Issue#2266")]
        public override void Can_query_just_roses()
        {
            base.Can_query_just_roses();
        }

        [ConditionalFact(Skip = "Issue#2266")]
        public override void Can_query_when_shared_column()
        {
            base.Can_query_when_shared_column();
        }

        [ConditionalFact(Skip = "Issue#2266")]
        public override void Can_use_backwards_is_animal()
        {
            base.Can_use_backwards_is_animal();
        }

        [ConditionalFact(Skip = "Issue#2266")]
        public override void Can_use_backwards_of_type_animal()
        {
            base.Can_use_backwards_of_type_animal();
        }

        [ConditionalFact(Skip = "Issue#2266")]
        public override void Can_use_is_kiwi()
        {
            base.Can_use_is_kiwi();
        }

        [ConditionalFact(Skip = "Issue#2266")]
        public override void Can_use_is_kiwi_in_projection()
        {
            base.Can_use_is_kiwi_in_projection();
        }

        [ConditionalFact(Skip = "Issue#2266")]
        public override void Can_use_is_kiwi_with_other_predicate()
        {
            base.Can_use_is_kiwi_with_other_predicate();
        }

        [ConditionalFact(Skip = "Issue#2266")]
        public override void Can_use_of_type_animal()
        {
            base.Can_use_of_type_animal();
        }

        [ConditionalFact(Skip = "Issue#2266")]
        public override void Can_use_of_type_bird()
        {
            base.Can_use_of_type_bird();
        }

        [ConditionalFact(Skip = "Issue#2266")]
        public override void Can_use_of_type_bird_first()
        {
            base.Can_use_of_type_bird_first();
        }

        [ConditionalFact(Skip = "Issue#2266")]
        public override void Can_use_of_type_bird_predicate()
        {
            base.Can_use_of_type_bird_predicate();
        }

        [ConditionalFact(Skip = "Issue#2266")]
        public override void Can_use_of_type_bird_with_projection()
        {
            base.Can_use_of_type_bird_with_projection();
        }

        [ConditionalFact(Skip = "Issue#2266")]
        public override void Can_use_of_type_kiwi()
        {
            base.Can_use_of_type_kiwi();
        }

        [ConditionalFact(Skip = "Issue#2266")]
        public override void Can_use_of_type_kiwi_where_north_on_derived_property()
        {
            base.Can_use_of_type_kiwi_where_north_on_derived_property();
        }

        [ConditionalFact(Skip = "Issue#2266")]
        public override void Can_use_of_type_kiwi_where_south_on_derived_property()
        {
            base.Can_use_of_type_kiwi_where_south_on_derived_property();
        }

        [ConditionalFact(Skip = "Issue#2266")]
        public override void Can_use_of_type_rose()
        {
            base.Can_use_of_type_rose();
        }

        [ConditionalFact(Skip = "Issue#2266")]
        public override void Discriminator_used_when_projection_over_derived_type()
        {
            base.Discriminator_used_when_projection_over_derived_type();
        }

        [ConditionalFact(Skip = "Issue#2266")]
        public override void Discriminator_used_when_projection_over_derived_type2()
        {
            base.Discriminator_used_when_projection_over_derived_type2();
        }

        [ConditionalFact(Skip = "Issue#2266")]
        public override void Discriminator_used_when_projection_over_of_type()
        {
            base.Discriminator_used_when_projection_over_of_type();
        }

        [ConditionalFact(Skip = "Issue#2266")]
        public override void Discriminator_with_cast_in_shadow_property()
        {
            base.Discriminator_with_cast_in_shadow_property();
        }

        [ConditionalFact(Skip = "Issue#2266")]
        public override void Member_access_on_intermediate_type_works()
        {
            base.Member_access_on_intermediate_type_works();
        }

        [ConditionalFact(Skip = "Issue#2266")]
        public override void OfType_Union_OfType()
        {
            base.OfType_Union_OfType();
        }

        [ConditionalFact(Skip = "Issue#2266")]
        public override void OfType_Union_subquery()
        {
            base.OfType_Union_subquery();
        }

        [ConditionalFact(Skip = "Issue#2266")]
        public override void Setting_foreign_key_to_a_different_type_throws()
        {
            base.Setting_foreign_key_to_a_different_type_throws();
        }

        [ConditionalFact(Skip = "Issue#2266")]
        public override void Subquery_OfType()
        {
            base.Subquery_OfType();
        }

        [ConditionalFact(Skip = "Issue#2266")]
        public override void Union_entity_equality()
        {
            base.Union_entity_equality();
        }

        [ConditionalFact(Skip = "Issue#2266")]
        public override void Union_siblings_with_duplicate_property_in_subquery()
        {
            base.Union_siblings_with_duplicate_property_in_subquery();
        }
    }
}
