// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class BookStoreDbFunctionsQueryInMemoryTest : BookStoreTestBase<BookStoreDbFunctionsQueryInMemoryTest.BookStoreDbFunctionsQueryInMemoryFixture>
    {
        public BookStoreDbFunctionsQueryInMemoryTest(BookStoreDbFunctionsQueryInMemoryFixture fixture)
            : base(fixture)
        {
        }

        public class BookStoreDbFunctionsQueryInMemoryFixture : BookStoreFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => InMemoryTestStoreFactory.Instance;

            public override IDisposable BeginTransaction(DbContext context) => new InMemoryCleaner(context);

            private class InMemoryCleaner : IDisposable
            {
                private readonly DbContext _context;
                public InMemoryCleaner(DbContext context) => _context = context;
                public void Dispose() => _context.Database.EnsureDeleted();
            }
        }
    }
}
