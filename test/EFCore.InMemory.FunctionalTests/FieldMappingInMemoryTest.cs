// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

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
        protected override ITestStoreFactory TestStoreFactory
            => InMemoryTestStoreFactory.Instance;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).ConfigureWarnings(w => w.Log(InMemoryEventId.TransactionIgnoredWarning));
    }
}
