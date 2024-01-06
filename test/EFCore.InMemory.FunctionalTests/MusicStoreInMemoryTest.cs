// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public class MusicStoreInMemoryTest(MusicStoreInMemoryTest.MusicStoreInMemoryFixture fixture) : MusicStoreTestBase<MusicStoreInMemoryTest.MusicStoreInMemoryFixture>(fixture)
{
    public class MusicStoreInMemoryFixture : MusicStoreFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => InMemoryTestStoreFactory.Instance;

        public override IDisposable BeginTransaction(DbContext context)
            => new InMemoryCleaner(context);

        private class InMemoryCleaner(DbContext context) : IDisposable
        {
            private readonly DbContext _context = context;

            public void Dispose()
                => _context.Database.EnsureDeleted();
        }
    }
}
