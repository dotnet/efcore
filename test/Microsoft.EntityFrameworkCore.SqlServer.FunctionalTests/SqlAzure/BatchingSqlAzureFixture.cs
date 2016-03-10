// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests.SqlAzure.Model;

namespace Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests.SqlAzure
{
    public class BatchingSqlAzureFixture : SqlAzureFixture
    {
        public AdventureWorksContext CreateContext(int maxBatchSize)
        {
            var optionsBuilder = new DbContextOptionsBuilder(Options).UseInternalServiceProvider(Services);

            new SqlServerDbContextOptionsBuilder(optionsBuilder).MaxBatchSize(maxBatchSize);

            return new AdventureWorksContext(optionsBuilder.Options);
        }
    }
}
