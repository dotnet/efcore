// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class AdHocMiscellaneousQueryInMemoryTest : AdHocMiscellaneousQueryTestBase
{
    protected override ITestStoreFactory TestStoreFactory
        => InMemoryTestStoreFactory.Instance;

    public override Task Explicitly_compiled_query_does_not_add_cache_entry()
        => Task.CompletedTask;

    public override Task Inlined_dbcontext_is_not_leaking()
        => Task.CompletedTask;

    public override Task Relational_command_cache_creates_new_entry_when_parameter_nullability_changes()
        => Task.CompletedTask;

    public override Task Variable_from_closure_is_parametrized()
        => Task.CompletedTask;
}
