// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore
{
    public class MigrationsOracleFixture : MigrationsFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory => OracleTestStoreFactory.Instance;

        public override MigrationsContext CreateContext()
        {
            var options = AddOptions(
                    new DbContextOptionsBuilder()
                        .UseOracle(TestStore.ConnectionString, b => b.ApplyConfiguration().CommandTimeout(OracleTestStore.CommandTimeout)))
                .UseInternalServiceProvider(ServiceProvider)
                .Options;

            return new MigrationsContext(options);
        }
    }
}
