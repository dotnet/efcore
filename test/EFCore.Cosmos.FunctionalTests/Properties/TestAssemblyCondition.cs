// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit.Sdk;

// Use a custom test framework that manages the Cosmos emulator container as an assembly-wide fixture
[assembly: TestFramework(
    "Microsoft.EntityFrameworkCore.TestUtilities.CosmosTestFramework",
    "Microsoft.EntityFrameworkCore.Cosmos.FunctionalTests")]

// Skip the entire assembly if cannot connect to CosmosDb
[assembly: CosmosDbConfiguredCondition]

// Waiting on Task causes deadlocks when run in parallel
[assembly: CollectionBehavior(DisableTestParallelization = true)]
