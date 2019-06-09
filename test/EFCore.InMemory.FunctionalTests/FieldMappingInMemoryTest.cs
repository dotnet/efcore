// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class FieldMappingInMemoryTest : FieldMappingTestBase<FieldMappingInMemoryTest.FieldMappingInMemoryFixture>
    {
        public FieldMappingInMemoryTest(FieldMappingInMemoryFixture fixture)
            : base(fixture)
        {
        }

        [ConditionalFact(Skip = "Issue#15711")]
        public override void Field_mapping_with_conversion_does_not_throw()
        {
            base.Field_mapping_with_conversion_does_not_throw();
        }

        [ConditionalTheory(Skip = "Issue#15711")]
        public override void Include_collection_auto_props(bool tracking)
        {
            base.Include_collection_auto_props(tracking);
        }

        [ConditionalTheory(Skip = "Issue#15711")]
        public override void Include_collection_fields_only(bool tracking)
        {
            base.Include_collection_fields_only(tracking);
        }

        [ConditionalTheory(Skip = "Issue#15711")]
        public override void Include_collection_fields_only_for_navs_too(bool tracking)
        {
            base.Include_collection_fields_only_for_navs_too(tracking);
        }

        [ConditionalTheory(Skip = "Issue#15711")]
        public override void Include_collection_full_props(bool tracking)
        {
            base.Include_collection_full_props(tracking);
        }

        [ConditionalTheory(Skip = "Issue#15711")]
        public override void Include_collection_full_props_with_named_fields(bool tracking)
        {
            base.Include_collection_full_props_with_named_fields(tracking);
        }

        [ConditionalTheory(Skip = "Issue#15711")]
        public override void Include_collection_hiding_props(bool tracking)
        {
            base.Include_collection_hiding_props(tracking);
        }

        [ConditionalTheory(Skip = "Issue#15711")]
        public override void Include_collection_read_only_props(bool tracking)
        {
            base.Include_collection_read_only_props(tracking);
        }

        [ConditionalTheory(Skip = "Issue#15711")]
        public override void Include_collection_read_only_props_with_named_fields(bool tracking)
        {
            base.Include_collection_read_only_props_with_named_fields(tracking);
        }

        [ConditionalTheory(Skip = "Issue#15711")]
        public override void Include_collection_write_only_props(bool tracking)
        {
            base.Include_collection_write_only_props(tracking);
        }

        [ConditionalTheory(Skip = "Issue#15711")]
        public override void Include_collection_write_only_props_with_named_fields(bool tracking)
        {
            base.Include_collection_write_only_props_with_named_fields(tracking);
        }

        [ConditionalTheory(Skip = "Issue#15711")]
        public override void Include_reference_auto_props(bool tracking)
        {
            base.Include_reference_auto_props(tracking);
        }

        [ConditionalTheory(Skip = "Issue#15711")]
        public override void Include_reference_fields_only(bool tracking)
        {
            base.Include_reference_fields_only(tracking);
        }

        [ConditionalTheory(Skip = "Issue#15711")]
        public override void Include_reference_fields_only_only_for_navs_too(bool tracking)
        {
            base.Include_reference_fields_only_only_for_navs_too(tracking);
        }

        [ConditionalTheory(Skip = "Issue#15711")]
        public override void Include_reference_full_props(bool tracking)
        {
            base.Include_reference_full_props(tracking);
        }

        [ConditionalTheory(Skip = "Issue#15711")]
        public override void Include_reference_full_props_with_named_fields(bool tracking)
        {
            base.Include_reference_full_props_with_named_fields(tracking);
        }

        [ConditionalTheory(Skip = "Issue#15711")]
        public override void Include_reference_hiding_props(bool tracking)
        {
            base.Include_reference_hiding_props(tracking);
        }

        [ConditionalTheory(Skip = "Issue#15711")]
        public override void Include_reference_read_only_props(bool tracking)
        {
            base.Include_reference_read_only_props(tracking);
        }

        [ConditionalTheory(Skip = "Issue#15711")]
        public override void Include_reference_read_only_props_with_named_fields(bool tracking)
        {
            base.Include_reference_read_only_props_with_named_fields(tracking);
        }

        [ConditionalTheory(Skip = "Issue#15711")]
        public override void Include_reference_write_only_props(bool tracking)
        {
            base.Include_reference_write_only_props(tracking);
        }

        [ConditionalTheory(Skip = "Issue#15711")]
        public override void Include_reference_write_only_props_with_named_fields(bool tracking)
        {
            base.Include_reference_write_only_props_with_named_fields(tracking);
        }

        protected override void Update<TBlog>(string navigation)
        {
            base.Update<TBlog>(navigation);

            Fixture.Reseed();
        }

        public class FieldMappingInMemoryFixture : FieldMappingFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => InMemoryTestStoreFactory.Instance;

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
                => base.AddOptions(builder).ConfigureWarnings(w => w.Log(InMemoryEventId.TransactionIgnoredWarning));
        }
    }
}
