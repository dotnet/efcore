// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class InheritanceInMemoryTest : InheritanceTestBase<InheritanceInMemoryFixture>
    {
        public InheritanceInMemoryTest(InheritanceInMemoryFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            //TestLoggerFactory.TestOutputHelper = testOutputHelper;
        }

        [ConditionalFact(Skip = "Issue #16963")]
        public override void Can_use_is_kiwi()
        {
        }

        [ConditionalFact(Skip = "Issue #16963")]
        public override void Can_use_is_kiwi_with_other_predicate()
        {
        }

        [ConditionalFact(Skip = "Issue #16963")]
        public override void Subquery_OfType()
        {
        }

        [ConditionalFact(Skip = "Issue #16963")]
        public override void Discriminator_used_when_projection_over_of_type()
        {
        }

        [ConditionalFact(Skip = "Issue #16963")]
        public override void Can_use_of_type_animal()
        {
        }

        [ConditionalFact(Skip = "Issue #16963")]
        public override void Can_use_of_type_bird()
        {
        }

        [ConditionalFact(Skip = "Issue #16963")]
        public override void Can_use_of_type_bird_first()
        {
        }

        [ConditionalFact(Skip = "Issue #16963")]
        public override void Can_use_of_type_bird_predicate()
        {
        }

        [ConditionalFact(Skip = "Issue #16963")]
        public override void Can_use_of_type_bird_with_projection()
        {
        }

        [ConditionalFact(Skip = "Issue #16963")]
        public override void Can_use_of_type_kiwi()
        {
        }

        [ConditionalFact(Skip = "Issue #16963")]
        public override void Can_use_of_type_kiwi_where_north_on_derived_property()
        {
        }

        [ConditionalFact(Skip = "Issue #16963")]
        public override void Can_use_of_type_kiwi_where_south_on_derived_property()
        {
        }

        [ConditionalFact(Skip = "Issue #16963")]
        public override void Can_use_of_type_rose()
        {
        }

        [ConditionalFact(Skip = "Issue #16963")]
        public override void Can_query_all_animal_views()
        {
            Assert.Equal(
                CoreStrings.TranslationFailed("OrderBy<AnimalQuery, int>(    source: Select<Bird, AnimalQuery>(        source: DbSet<Bird>,         selector: (b) => MaterializeView(b)),     keySelector: (a) => a.CountryId)"),
                Assert.Throws<InvalidOperationException>(() => base.Can_query_all_animal_views())
                    .Message.Replace("\r", "").Replace("\n", ""));
        }

        protected override bool EnforcesFkConstraints => false;
    }
}
