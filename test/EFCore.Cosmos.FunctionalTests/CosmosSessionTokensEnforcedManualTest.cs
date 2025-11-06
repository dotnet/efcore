// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Infrastructure;

namespace Microsoft.EntityFrameworkCore;

public class CosmosSessionTokensEnforcedManualTest(NonSharedFixture fixture) : CosmosSessionTokensNonFullyAutomaticTestBase(fixture)
{
    protected override SessionTokenManagementMode Mode => SessionTokenManagementMode.EnforcedManual;

    [ConditionalTheory, InlineData(true), InlineData(false)]
    public async virtual Task SaveChanges_throws_without_token(bool async)
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();


    }
}
