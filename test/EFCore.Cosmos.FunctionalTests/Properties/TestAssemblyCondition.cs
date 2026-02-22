// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Skip the entire assembly if cannot connect to CosmosDb

[assembly: CosmosDbConfiguredCondition]

[assembly: CollectionBehavior(MaxParallelThreads = -1)] // We handle concurrency in the test store to improve performance.
