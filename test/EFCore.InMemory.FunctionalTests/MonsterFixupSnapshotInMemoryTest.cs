// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class MonsterFixupSnapshotInMemoryTest : MonsterFixupTestBase<
        MonsterFixupSnapshotInMemoryTest.MonsterFixupSnapshotInMemoryFixture>
    {
        public MonsterFixupSnapshotInMemoryTest(MonsterFixupSnapshotInMemoryFixture fixture)
            : base(fixture)
        {
        }

        [ConditionalFact(Skip = "Include Issue#15711")]
        public override void Can_build_monster_model_and_seed_data_using_all_navigations()
        {
            base.Can_build_monster_model_and_seed_data_using_all_navigations();
        }

        [ConditionalFact(Skip = "Include Issue#15711")]
        public override void Can_build_monster_model_and_seed_data_using_dependent_navigations()
        {
            base.Can_build_monster_model_and_seed_data_using_dependent_navigations();
        }

        [ConditionalFact(Skip = "Include Issue#15711")]
        public override void Can_build_monster_model_and_seed_data_using_FKs()
        {
            base.Can_build_monster_model_and_seed_data_using_FKs();
        }

        [ConditionalFact(Skip = "Include Issue#15711")]
        public override void Can_build_monster_model_and_seed_data_using_navigations_with_deferred_add()
        {
            base.Can_build_monster_model_and_seed_data_using_navigations_with_deferred_add();
        }

        [ConditionalFact(Skip = "Include Issue#15711")]
        public override void Can_build_monster_model_and_seed_data_using_principal_navigations()
        {
            base.Can_build_monster_model_and_seed_data_using_principal_navigations();
        }

        [ConditionalFact(Skip = "Include Issue#15711")]
        public override void One_to_one_fixup_happens_when_FKs_change_test()
        {
            base.One_to_one_fixup_happens_when_FKs_change_test();
        }

        [ConditionalFact(Skip = "Include Issue#15711")]
        public override void One_to_one_fixup_happens_when_reference_change_test()
        {
            base.One_to_one_fixup_happens_when_reference_change_test();
        }

        [ConditionalFact(Skip = "Include Issue#15711")]
        public override void Composite_fixup_happens_when_FKs_change_test()
        {
            base.Composite_fixup_happens_when_FKs_change_test();
        }

        public class MonsterFixupSnapshotInMemoryFixture : MonsterFixupSnapshotFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => InMemoryTestStoreFactory.Instance;

            protected override void OnModelCreating<TMessage, TProduct, TProductPhoto, TProductReview, TComputerDetail, TDimensions>(
                ModelBuilder builder)
            {
                base.OnModelCreating<TMessage, TProduct, TProductPhoto, TProductReview, TComputerDetail, TDimensions>(builder);

                builder.Entity<TMessage>().Property(e => e.MessageId).ValueGeneratedOnAdd();
                builder.Entity<TProductPhoto>().Property(e => e.PhotoId).ValueGeneratedOnAdd();
                builder.Entity<TProductReview>().Property(e => e.ReviewId).ValueGeneratedOnAdd();
            }
        }
    }
}
