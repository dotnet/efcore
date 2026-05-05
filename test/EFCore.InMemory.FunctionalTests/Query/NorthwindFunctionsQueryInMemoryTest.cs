// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class NorthwindFunctionsQueryInMemoryTest(NorthwindQueryInMemoryFixture<NoopModelCustomizer> fixture)
    : NorthwindFunctionsQueryTestBase<NorthwindQueryInMemoryFixture<NoopModelCustomizer>>(fixture)
{
    // StringComparison.CurrentCulture{,IgnoreCase} and InvariantCulture{,IgnoreCase} are not supported in real providers, but the in-memory
    // provider does support them.
    public override Task String_StartsWith_with_StringComparison_unsupported(bool async)
        => Task.CompletedTask;

    public override Task String_EndsWith_with_StringComparison_unsupported(bool async)
        => Task.CompletedTask;

    public override Task String_Contains_with_StringComparison_unsupported(bool async)
        => Task.CompletedTask;
}
