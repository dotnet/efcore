// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.TestModels.BookStore;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class BookStoreTestBase<TFixture> : IClassFixture<TFixture>
        where TFixture : BookStoreTestBase<TFixture>.BookStoreFixtureBase, new()
    {
        protected BookStoreTestBase(TFixture fixture)
        {
            Fixture = fixture;
            fixture.ListLoggerFactory.Clear();
        }

        protected TFixture Fixture { get; }

        protected BookStoreContext CreateContext() => Fixture.CreateContext();

        public abstract class BookStoreFixtureBase : SharedStoreFixtureBase<BookStoreContext>
        {
            public virtual IDisposable BeginTransaction(DbContext context) => context.Database.BeginTransaction();

            protected override void Seed(BookStoreContext context) => BookStoreContext.Seed(context);

            protected override string StoreName { get; } = "BookStore";

            protected override bool UsePooling => false;
        }
    }
}
