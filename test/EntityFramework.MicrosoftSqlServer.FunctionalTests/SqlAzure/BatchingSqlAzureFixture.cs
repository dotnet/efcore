// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.SqlServer.FunctionalTests.SqlAzure.Model;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests.SqlAzure
{
    public class BatchingSqlAzureFixture : SqlAzureFixture
    {
        public AdventureWorksContext CreateContext(int maxBatchSize)
        {
            var optionsBuilder = new DbContextOptionsBuilder(Options);
            new SqlServerDbContextOptionsBuilder(optionsBuilder).MaxBatchSize(maxBatchSize);
            return new AdventureWorksContext(Services, optionsBuilder.Options);
        }
    }
}
