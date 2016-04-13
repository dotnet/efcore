// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.FunctionalTests
{
    public abstract class UpdatesRelationalTestBase<TFixture, TTestStore> : UpdatesTestBase<TFixture, TTestStore>
        where TFixture : UpdatesFixtureBase<TTestStore>
        where TTestStore : TestStore
    {
        protected UpdatesRelationalTestBase(TFixture fixture)
            : base(fixture)
        {
        }

        protected override string UpdateConcurrencyMessage
            => RelationalStrings.UpdateConcurrencyException(1, 0);
    }
}
