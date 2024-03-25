// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.InMemory.Internal;
using Microsoft.EntityFrameworkCore.TestModels.UpdatesModel;

namespace Microsoft.EntityFrameworkCore;

public abstract class UpdatesInMemoryTestBase<TFixture> : UpdatesTestBase<TFixture>
    where TFixture : UpdatesInMemoryTestBase<TFixture>.UpdatesInMemoryFixtureBase
{
    protected UpdatesInMemoryTestBase(TFixture fixture)
        : base(fixture)
    {
    }

    protected override string UpdateConcurrencyMessage
        => InMemoryStrings.UpdateConcurrencyException;

    protected override async Task ExecuteWithStrategyInTransactionAsync(
        Func<UpdatesContext, Task> testOperation,
        Func<UpdatesContext, Task> nestedTestOperation1 = null,
        Func<UpdatesContext, Task> nestedTestOperation2 = null)
    {
        try
        {
            await base.ExecuteWithStrategyInTransactionAsync(testOperation, nestedTestOperation1, nestedTestOperation2);
        }
        finally
        {
            await Fixture.ReseedAsync();
        }
    }

    // Issue #29875
    public override Task Can_change_type_of_pk_to_pk_dependent_by_replacing_with_new_dependent(bool async)
        => Assert.ThrowsAsync<DbUpdateConcurrencyException>(
            () => base.Can_change_type_of_pk_to_pk_dependent_by_replacing_with_new_dependent(async));

    public abstract class UpdatesInMemoryFixtureBase : UpdatesFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => InMemoryTestStoreFactory.Instance;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).ConfigureWarnings(w => w.Log(InMemoryEventId.TransactionIgnoredWarning));
    }
}
