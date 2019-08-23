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
