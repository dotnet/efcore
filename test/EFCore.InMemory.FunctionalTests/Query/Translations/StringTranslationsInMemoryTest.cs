// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Translations;

public class StringTranslationsInMemoryTest(BasicTypesQueryInMemoryFixture fixture)
    : StringTranslationsTestBase<BasicTypesQueryInMemoryFixture>(fixture)
{
    // StringComparison.CurrentCulture{,IgnoreCase} and InvariantCulture{,IgnoreCase} are not supported in real providers, but the in-memory
    // provider does support them.
    public override Task StartsWith_with_StringComparison_unsupported()
        => Task.CompletedTask;

    public override Task EndsWith_with_StringComparison_unsupported()
        => Task.CompletedTask;

    public override Task Contains_with_StringComparison_unsupported()
        => Task.CompletedTask;
}
