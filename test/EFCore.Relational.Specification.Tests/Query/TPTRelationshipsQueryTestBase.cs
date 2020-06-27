// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class TPTRelationshipsQueryTestBase<TFixture> : InheritanceRelationshipsQueryTestBase<TFixture>
        where TFixture : TPTRelationshipsQueryRelationalFixture, new()
    {
        protected TPTRelationshipsQueryTestBase(TFixture fixture)
            : base(fixture)
        {
        }

        [ConditionalFact(Skip = "Issue #2266")]
        public override void Changes_in_derived_related_entities_are_detected()
        {
        }

        [ConditionalFact(Skip = "Issue #2266")]
        public override void Include_reference_with_inheritance()
        {
        }

        [ConditionalFact(Skip = "Issue #2266")]
        public override void Include_reference_with_inheritance_reverse()
        {
        }

        [ConditionalFact(Skip = "Issue #2266")]
        public override void Include_self_reference_with_inheritance()
        {
        }

        [ConditionalFact(Skip = "Issue #2266")]
        public override void Include_self_reference_with_inheritance_reverse()
        {
        }

        [ConditionalFact(Skip = "Issue #2266")]
        public override void Include_reference_with_inheritance_with_filter()
        {
        }

        [ConditionalFact(Skip = "Issue #2266")]
        public override void Include_reference_with_inheritance_with_filter_reverse()
        {
        }

        [ConditionalFact(Skip = "Issue #2266")]
        public override void Include_reference_without_inheritance()
        {
        }

        [ConditionalFact(Skip = "Issue #2266")]
        public override void Include_reference_without_inheritance_reverse()
        {
        }

        [ConditionalFact(Skip = "Issue #2266")]
        public override void Include_reference_without_inheritance_with_filter()
        {
        }

        [ConditionalFact(Skip = "Issue #2266")]
        public override void Include_reference_without_inheritance_with_filter_reverse()
        {
        }

        [ConditionalFact(Skip = "Issue #2266")]
        public override void Include_collection_with_inheritance()
        {
        }

        [ConditionalFact(Skip = "Issue #2266")]
        public override void Include_collection_with_inheritance_reverse()
        {
        }

        [ConditionalFact(Skip = "Issue #2266")]
        public override void Include_collection_with_inheritance_with_filter()
        {
        }

        [ConditionalFact(Skip = "Issue #2266")]
        public override void Include_collection_with_inheritance_with_filter_reverse()
        {
        }

        [ConditionalFact(Skip = "Issue #2266")]
        public override void Include_collection_without_inheritance()
        {
        }

        [ConditionalFact(Skip = "Issue #2266")]
        public override void Include_collection_without_inheritance_reverse()
        {
        }

        [ConditionalFact(Skip = "Issue #2266")]
        public override void Include_collection_without_inheritance_with_filter()
        {
        }

        [ConditionalFact(Skip = "Issue #2266")]
        public override void Include_collection_without_inheritance_with_filter_reverse()
        {
        }

        [ConditionalFact(Skip = "Issue #2266")]
        public override void Include_reference_with_inheritance_on_derived1()
        {
        }

        [ConditionalFact(Skip = "Issue #2266")]
        public override void Include_reference_with_inheritance_on_derived2()
        {
        }

        [ConditionalFact(Skip = "Issue #2266")]
        public override void Include_reference_with_inheritance_on_derived4()
        {
        }

        [ConditionalFact(Skip = "Issue #2266")]
        public override void Include_reference_with_inheritance_on_derived_reverse()
        {
        }

        [ConditionalFact(Skip = "Issue #2266")]
        public override void Include_reference_with_inheritance_on_derived_with_filter1()
        {
        }

        [ConditionalFact(Skip = "Issue #2266")]
        public override void Include_reference_with_inheritance_on_derived_with_filter2()
        {
        }

        [ConditionalFact(Skip = "Issue #2266")]
        public override void Include_reference_with_inheritance_on_derived_with_filter4()
        {
        }

        [ConditionalFact(Skip = "Issue #2266")]
        public override void Include_reference_with_inheritance_on_derived_with_filter_reverse()
        {
        }

        [ConditionalFact(Skip = "Issue #2266")]
        public override void Include_reference_without_inheritance_on_derived1()
        {
        }

        [ConditionalFact(Skip = "Issue #2266")]
        public override void Include_reference_without_inheritance_on_derived2()
        {
        }

        [ConditionalFact(Skip = "Issue #2266")]
        public override void Include_reference_without_inheritance_on_derived_reverse()
        {
        }

        [ConditionalFact(Skip = "Issue #2266")]
        public override void Include_collection_with_inheritance_on_derived1()
        {
        }

        [ConditionalFact(Skip = "Issue #2266")]
        public override void Include_collection_with_inheritance_on_derived2()
        {
        }

        [ConditionalFact(Skip = "Issue #2266")]
        public override void Include_collection_with_inheritance_on_derived3()
        {
        }

        [ConditionalFact(Skip = "Issue #2266")]
        public override void Include_collection_with_inheritance_on_derived_reverse()
        {
        }

        [ConditionalFact(Skip = "Issue #2266")]
        public override void Nested_include_with_inheritance_reference_reference()
        {
        }

        [ConditionalFact(Skip = "Issue #2266")]
        public override void Nested_include_with_inheritance_reference_reference_on_base()
        {
        }

        [ConditionalFact(Skip = "Issue #2266")]
        public override void Nested_include_with_inheritance_reference_reference_reverse()
        {
        }

        [ConditionalFact(Skip = "Issue #2266")]
        public override void Nested_include_with_inheritance_reference_collection()
        {
        }

        [ConditionalFact(Skip = "Issue #2266")]
        public override void Nested_include_with_inheritance_reference_collection_on_base()
        {
        }

        [ConditionalFact(Skip = "Issue #2266")]
        public override void Nested_include_with_inheritance_reference_collection_reverse()
        {
        }

        [ConditionalFact(Skip = "Issue #2266")]
        public override void Nested_include_with_inheritance_collection_reference()
        {
        }

        [ConditionalFact(Skip = "Issue #2266")]
        public override void Nested_include_with_inheritance_collection_reference_reverse()
        {
        }

        [ConditionalFact(Skip = "Issue #2266")]
        public override void Nested_include_with_inheritance_collection_collection()
        {
        }

        [ConditionalFact(Skip = "Issue #2266")]
        public override void Nested_include_with_inheritance_collection_collection_reverse()
        {
        }

        [ConditionalFact(Skip = "Issue #2266")]
        public override void Nested_include_collection_reference_on_non_entity_base()
        {
        }
    }
}
