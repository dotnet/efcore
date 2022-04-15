// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.InMemory.Internal;

namespace Microsoft.EntityFrameworkCore;

public class UpdatesInMemoryWithoutSensitiveDataLoggingTest : UpdatesInMemoryTestBase<UpdatesInMemoryWithoutSensitiveDataLoggingFixture>
{
    public UpdatesInMemoryWithoutSensitiveDataLoggingTest(UpdatesInMemoryWithoutSensitiveDataLoggingFixture fixture)
        : base(fixture)
    {
    }

    protected override string UpdateConcurrencyTokenMessage
        => InMemoryStrings.UpdateConcurrencyTokenException("Product", "{'Price'}");
}
