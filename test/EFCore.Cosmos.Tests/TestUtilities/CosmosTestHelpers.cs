// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class CosmosTestHelpers : TestHelpers
    {
        protected CosmosTestHelpers()
        {
        }

        public static CosmosTestHelpers Instance { get; } = new CosmosTestHelpers();

        public override IServiceCollection AddProviderServices(IServiceCollection services)
            => services.AddEntityFrameworkCosmos();

        public override void UseProviderOptions(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseCosmos(
                TestEnvironment.DefaultConnection,
                TestEnvironment.AuthToken,
                "UnitTests");
    }
}
