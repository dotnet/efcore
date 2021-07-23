// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore
{
    public class MusicStoreInMemoryTest : MusicStoreTestBase<MusicStoreInMemoryTest.MusicStoreInMemoryFixture>
    {
        public MusicStoreInMemoryTest(MusicStoreInMemoryFixture fixture)
            : base(fixture)
        {
        }

        public class MusicStoreInMemoryFixture : MusicStoreFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory
                => InMemoryTestStoreFactory.Instance;

            public override IDisposable BeginTransaction(DbContext context)
                => new InMemoryCleaner(context);

            private class InMemoryCleaner : IDisposable
            {
                private readonly DbContext _context;

                public InMemoryCleaner(DbContext context)
                    => _context = context;

                public void Dispose()
                    => _context.Database.EnsureDeleted();
            }
        }
    }
}
