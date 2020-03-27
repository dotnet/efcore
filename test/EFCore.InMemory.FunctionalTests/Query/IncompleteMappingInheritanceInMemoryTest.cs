// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class IncompleteMappingInheritanceInMemoryTest : InheritanceTestBase<IncompleteMappingInheritanceInMemoryFixture>
    {
        public IncompleteMappingInheritanceInMemoryTest(IncompleteMappingInheritanceInMemoryFixture fixture)
            : base(fixture)
        {
        }

        [ConditionalFact]
        public override void Can_query_all_animal_views()
        {
            var message = Assert.Throws<InvalidOperationException>(() => base.Can_query_all_animal_views()).Message;

            Assert.Equal(
                CoreStrings.TranslationFailed(
                    @"DbSet<Bird>()
    .Select(b => InheritanceInMemoryFixture.MaterializeView(b))
    .OrderBy(a => a.CountryId)"),
                message,
                ignoreLineEndingDifferences: true);
        }

        protected override bool EnforcesFkConstraints => false;
    }
}
