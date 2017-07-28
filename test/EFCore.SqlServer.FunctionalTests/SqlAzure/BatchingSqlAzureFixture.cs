// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.SqlAzure.Model;

namespace Microsoft.EntityFrameworkCore.SqlAzure
{
    public class BatchingSqlAzureFixture : SqlAzureFixture
    {
        public AdventureWorksContext CreateContext(int maxBatchSize)
        {
            var optionsBuilder = new DbContextOptionsBuilder(CreateOptions());

            new SqlServerDbContextOptionsBuilder(optionsBuilder).MaxBatchSize(maxBatchSize);

            return new AdventureWorksContext(optionsBuilder.Options);
        }
    }
}
