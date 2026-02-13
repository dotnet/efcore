// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Skip the entire assembly if cannot connect to CosmosDb

[assembly: CosmosDbConfiguredCondition]

// Emulator could experience performance degradation with more than 10 concurrent containers,
// some tests might create multiple containers
// https://learn.microsoft.com/en-us/azure/cosmos-db/emulator#differences-between-the-emulator-and-cloud-service
[assembly: CollectionBehavior(MaxParallelThreads = 4)]
