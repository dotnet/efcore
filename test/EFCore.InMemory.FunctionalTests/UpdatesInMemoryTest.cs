// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.TestModels.UpdatesModel;

namespace Microsoft.EntityFrameworkCore
{
    public class UpdatesInMemoryTest : UpdatesTestBase<UpdatesInMemoryFixture>
    {
        public UpdatesInMemoryTest(UpdatesInMemoryFixture fixture)
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
