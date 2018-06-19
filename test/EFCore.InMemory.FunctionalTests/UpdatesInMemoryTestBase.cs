// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestModels.UpdatesModel;
using Microsoft.EntityFrameworkCore.InMemory.Internal;

namespace Microsoft.EntityFrameworkCore
{
    public abstract class UpdatesInMemoryTestBase<TFixture> : UpdatesTestBase<TFixture>
        where TFixture : UpdatesInMemoryFixtureBase
    {
        protected UpdatesInMemoryTestBase(TFixture fixture)
            : base(fixture)
        {
        }

        protected override string UpdateConcurrencyMessage
            => InMemoryStrings.UpdateConcurrencyException;

        protected override void ExecuteWithStrategyInTransaction(
            Action<UpdatesContext> testOperation, Action<UpdatesContext> nestedTestOperation1 = null)
        {
            base.ExecuteWithStrategyInTransaction(testOperation, nestedTestOperation1);
            Fixture.Reseed();
        }

        protected override async Task ExecuteWithStrategyInTransactionAsync(
            Func<UpdatesContext, Task> testOperation, Func<UpdatesContext, Task> nestedTestOperation1 = null)
        {
            await base.ExecuteWithStrategyInTransactionAsync(testOperation, nestedTestOperation1);
            Fixture.Reseed();
        }
    }
}
