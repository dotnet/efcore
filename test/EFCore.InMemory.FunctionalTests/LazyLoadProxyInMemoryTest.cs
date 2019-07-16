// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class LazyLoadProxyInMemoryTest : LazyLoadProxyTestBase<LazyLoadProxyInMemoryTest.LoadInMemoryFixture>
    {
        public LazyLoadProxyInMemoryTest(LoadInMemoryFixture fixture)
            : base(fixture)
        {
        }

        [ConditionalTheory(Skip = "Issue#15711")]
        public override void Lazy_load_collection_already_loaded(EntityState state, CascadeTiming cascadeDeleteTiming)
        {
            base.Lazy_load_collection_already_loaded(state, cascadeDeleteTiming);
        }

        [ConditionalFact(Skip = "Issue#15711")]
        public override void Lazy_load_collection_for_no_tracking_does_not_throw_if_populated()
        {
            base.Lazy_load_collection_for_no_tracking_does_not_throw_if_populated();
        }

        [ConditionalFact(Skip = "Issue#15711")]
        public override void Lazy_load_reference_to_principal_for_no_tracking_does_not_throw_if_populated()
        {
            base.Lazy_load_reference_to_principal_for_no_tracking_does_not_throw_if_populated();
        }

        [ConditionalFact(Skip = "Issue#15711")]
        public override void Lazy_load_reference_to_dependent_for_no_does_not_throw_if_populated()
        {
            base.Lazy_load_reference_to_dependent_for_no_does_not_throw_if_populated();
        }

        [ConditionalTheory(Skip = "Issue#15711")]
        public override void Lazy_load_many_to_one_reference_to_principal_already_loaded(EntityState state, CascadeTiming cascadeDeleteTiming)
        {
            base.Lazy_load_many_to_one_reference_to_principal_already_loaded(state, cascadeDeleteTiming);
        }

        [ConditionalTheory(Skip = "Issue#15711")]
        public override void Lazy_load_one_to_one_PK_to_PK_reference_to_dependent_already_loaded(EntityState state)
        {
            base.Lazy_load_one_to_one_PK_to_PK_reference_to_dependent_already_loaded(state);
        }

        [ConditionalTheory(Skip = "Issue#15711")]
        public override void Lazy_load_one_to_one_PK_to_PK_reference_to_principal_already_loaded(EntityState state)
        {
            base.Lazy_load_one_to_one_PK_to_PK_reference_to_principal_already_loaded(state);
        }

        [ConditionalTheory(Skip = "Issue#15711")]
        public override void Lazy_load_one_to_one_reference_to_dependent_already_loaded(EntityState state, CascadeTiming cascadeDeleteTiming)
        {
            base.Lazy_load_one_to_one_reference_to_dependent_already_loaded(state, cascadeDeleteTiming);
        }

        [ConditionalTheory(Skip = "Issue#15711")]
        public override void Lazy_load_one_to_one_reference_to_principal_already_loaded(EntityState state)
        {
            base.Lazy_load_one_to_one_reference_to_principal_already_loaded(state);
        }

        [ConditionalTheory(Skip = "Include Issue#15711")]
        public override void Lazy_loading_finds_correct_entity_type_with_multiple_queries_using_Count()
        {
            base.Lazy_loading_finds_correct_entity_type_with_multiple_queries_using_Count();
        }

        [ConditionalTheory(Skip = "Owned Issue#15711")]
        public override void Lazy_loading_finds_correct_entity_type_with_already_loaded_owned_types()
        {
            base.Lazy_loading_finds_correct_entity_type_with_already_loaded_owned_types();
        }

        [ConditionalTheory(Skip = "Owned Issue#15711")]
        public override void Lazy_loading_finds_correct_entity_type_with_alternate_model()
        {
            base.Lazy_loading_finds_correct_entity_type_with_alternate_model();
        }

        [ConditionalTheory(Skip = "Owned Issue#15711")]
        public override void Lazy_loading_finds_correct_entity_type_with_multiple_queries()
        {
            base.Lazy_loading_finds_correct_entity_type_with_multiple_queries();
        }

        [ConditionalTheory(Skip = "Owned Issue#15711")]
        public override void Lazy_loading_finds_correct_entity_type_with_opaque_predicate_and_multiple_queries()
        {
            base.Lazy_loading_finds_correct_entity_type_with_opaque_predicate_and_multiple_queries();
        }

        public class LoadInMemoryFixture : LoadFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => InMemoryTestStoreFactory.Instance;
        }
    }
}
