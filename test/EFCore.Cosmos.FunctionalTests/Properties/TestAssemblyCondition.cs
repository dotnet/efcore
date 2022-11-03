// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Skip the entire assembly if cannot connect to CosmosDb

[assembly: CosmosDbConfiguredCondition]

// Waiting on Task causes deadlocks when run in parallel
[assembly: CollectionBehavior(DisableTestParallelization = true)]
