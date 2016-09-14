// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Specification.Tests
{
    public abstract class UpdatesRelationalTestBase<TFixture, TTestStore> : UpdatesTestBase<TFixture, TTestStore>
        where TFixture : UpdatesFixtureBase<TTestStore>
        where TTestStore : TestStore
    {
        protected UpdatesRelationalTestBase(TFixture fixture)
            : base(fixture)
        {
        }

        protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
            => facade.UseTransaction(transaction.GetDbTransaction());

        protected override string UpdateConcurrencyMessage
            => RelationalStrings.UpdateConcurrencyException(1, 0);
    }
}
