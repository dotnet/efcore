// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using Oracle.ManagedDataAccess.Client;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class OracleTestHelpers : TestHelpers
    {
        protected OracleTestHelpers()
        {
        }

        public static OracleTestHelpers Instance { get; } = new OracleTestHelpers();

        public override IServiceCollection AddProviderServices(IServiceCollection services)
            => services.AddEntityFrameworkOracle();

        protected override void UseProviderOptions(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseOracle(new OracleConnection(OracleTestStore.CreateConnectionString("dummy")));
    }
}
