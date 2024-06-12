﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable once CheckNamespace

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class CosmosTestHelpers : TestHelpers
{
    protected CosmosTestHelpers()
    {
    }

    public static CosmosTestHelpers Instance { get; } = new();

    public override IServiceCollection AddProviderServices(IServiceCollection services)
        => services.AddEntityFrameworkCosmos();

    public override DbContextOptionsBuilder UseProviderOptions(DbContextOptionsBuilder optionsBuilder)
        => TestEnvironment.UseTokenCredential
        ? optionsBuilder.UseCosmos(
            TestEnvironment.DefaultConnection,
            TestEnvironment.TokenCredential,
            "UnitTests")
        : optionsBuilder.UseCosmos(
            TestEnvironment.DefaultConnection,
            TestEnvironment.AuthToken,
            "UnitTests");
}
