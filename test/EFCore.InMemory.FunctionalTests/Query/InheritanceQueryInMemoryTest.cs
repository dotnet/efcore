// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class InheritanceQueryInMemoryTest : InheritanceQueryTestBase<InheritanceQueryInMemoryFixture>
    {
        public InheritanceQueryInMemoryTest(InheritanceQueryInMemoryFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            //TestLoggerFactory.TestOutputHelper = testOutputHelper;
        }

        [ConditionalFact]
        public override void Can_query_all_animal_views()
        {
            var message = Assert.Throws<InvalidOperationException>(() => base.Can_query_all_animal_views()).Message;

            Assert.Equal(
                CoreStrings.TranslationFailed(
                    @"DbSet<Bird>()
    .Select(b => InheritanceQueryInMemoryFixture.MaterializeView(b))
    .OrderBy(a => a.CountryId)"),
                message,
                ignoreLineEndingDifferences: true);
        }

        protected override bool EnforcesFkConstraints => false;
    }
}
