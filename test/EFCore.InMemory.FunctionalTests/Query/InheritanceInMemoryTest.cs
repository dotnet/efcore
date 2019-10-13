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

        [ConditionalFact]
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
