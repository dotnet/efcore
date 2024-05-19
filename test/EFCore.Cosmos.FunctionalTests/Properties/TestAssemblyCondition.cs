// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Skip the entire assembly if cannot connect to CosmosDb

using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;

[assembly: CosmosDbConfiguredCondition]

// Waiting on Task causes deadlocks when run in parallel
[assembly: CollectionBehavior(DisableTestParallelization = true)]

// Cosmos doesn't support sync I/O
[assembly: SkipSyncTests]
