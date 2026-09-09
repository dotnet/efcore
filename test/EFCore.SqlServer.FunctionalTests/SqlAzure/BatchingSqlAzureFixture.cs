// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.SqlAzure.Model;

namespace Microsoft.EntityFrameworkCore.SqlAzure;

#nullable disable

public class BatchingSqlAzureFixture : SqlAzureFixture
{
    public AdventureWorksContext CreateContext(int maxBatchSize)
    {
        var optionsBuilder = new DbContextOptionsBuilder(CreateOptions());

        new SqlServerDbContextOptionsBuilder(optionsBuilder).MaxBatchSize(maxBatchSize);

        return new AdventureWorksContext(optionsBuilder.Options);
    }
}
