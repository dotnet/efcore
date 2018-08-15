// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Cosmos.Sql.TestUtilities
{
    public class CosmosSqlTestHelpers : TestHelpers
    {
        protected CosmosSqlTestHelpers()
        {
        }

        public static CosmosSqlTestHelpers Instance { get; } = new CosmosSqlTestHelpers();

        public override IServiceCollection AddProviderServices(IServiceCollection services)
        {
            return services.AddEntityFrameworkCosmosSql();
        }

        protected override void UseProviderOptions(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseCosmosSql(
                new Uri(TestEnvironment.DefaultConnection),
                TestEnvironment.AuthToken,
                "UnitTests");
        }
    }
}
