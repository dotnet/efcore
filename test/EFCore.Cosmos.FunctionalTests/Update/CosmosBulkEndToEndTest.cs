// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Update;

public class CosmosBulkEndToEndTest(NonSharedFixture fixture) : EndToEndCosmosTest(fixture), IClassFixture<NonSharedFixture>
{
    protected override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        => base.AddOptions(builder).UseCosmos(x => x.BulkExecutionEnabled()).ConfigureWarnings(x => x.Ignore(CosmosEventId.BulkExecutionWithTransactionalBatch));
}
