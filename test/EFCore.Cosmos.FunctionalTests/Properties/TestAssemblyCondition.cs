// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Cosmos.TestUtilities;
using Xunit;

// Skip the entire assembly if cannot connect to CosmosDb
[assembly: TestFramework("Microsoft.EntityFrameworkCore.TestUtilities.Xunit.ConditionalTestFramework", "Microsoft.EntityFrameworkCore.Specification.Tests")]
[assembly: CosmosDbConfiguredCondition]
