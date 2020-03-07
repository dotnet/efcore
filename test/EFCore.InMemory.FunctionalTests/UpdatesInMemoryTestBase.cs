// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.InMemory.Internal;
using Microsoft.EntityFrameworkCore.TestModels.UpdatesModel;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public abstract class UpdatesInMemoryTestBase<TFixture> : UpdatesTestBase<TFixture>
        where TFixture : UpdatesInMemoryFixtureBase
    {
        protected UpdatesInMemoryTestBase(TFixture fixture)
            : base(fixture)
        {
        }

        [ConditionalFact(Skip = "Issue #14042")]
        public override void Mutation_of_tracked_values_does_not_mutate_values_in_store()
        {
        }

        protected override string UpdateConcurrencyMessage
            => InMemoryStrings.UpdateConcurrencyException;

        protected override void ExecuteWithStrategyInTransaction(
            Action<UpdatesContext> testOperation,
            Action<UpdatesContext> nestedTestOperation1 = null,
            Action<UpdatesContext> nestedTestOperation2 = null)
        {
            base.ExecuteWithStrategyInTransaction(testOperation, nestedTestOperation1, nestedTestOperation2);
            Fixture.Reseed();
        }

        protected override async Task ExecuteWithStrategyInTransactionAsync(
            Func<UpdatesContext, Task> testOperation,
            Func<UpdatesContext, Task> nestedTestOperation1 = null,
            Func<UpdatesContext, Task> nestedTestOperation2 = null)
        {
            await base.ExecuteWithStrategyInTransactionAsync(testOperation, nestedTestOperation1, nestedTestOperation2);
            Fixture.Reseed();
        }
    }
}
