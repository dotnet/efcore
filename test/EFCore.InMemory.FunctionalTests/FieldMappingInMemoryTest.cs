// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public class FieldMappingInMemoryTest(FieldMappingInMemoryTest.FieldMappingInMemoryFixture fixture)
    : FieldMappingTestBase<FieldMappingInMemoryTest.FieldMappingInMemoryFixture>(fixture)
{
    protected override async Task UpdateAsync<TBlog>(string navigation)
    {
        await base.UpdateAsync<TBlog>(navigation);
        await Fixture.ReseedAsync();
    }

    public class FieldMappingInMemoryFixture : FieldMappingFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => InMemoryTestStoreFactory.Instance;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).ConfigureWarnings(w => w.Log(InMemoryEventId.TransactionIgnoredWarning));
    }
}
