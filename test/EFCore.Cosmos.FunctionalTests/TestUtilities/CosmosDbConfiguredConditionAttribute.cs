// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly)]
public class CosmosDbConfiguredConditionAttribute : Attribute, ITestCondition
{
    public string SkipReason
        => "Unable to connect to Cosmos DB. Please install/start the emulator service or configure a valid endpoint.";

    public ValueTask<bool> IsMetAsync()
        => CosmosTestStore.IsConnectionAvailableAsync();
}
