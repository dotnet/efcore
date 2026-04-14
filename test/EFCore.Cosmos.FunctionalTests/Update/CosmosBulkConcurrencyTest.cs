// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Update;

public class CosmosBulkConcurrencyTest(CosmosBulkConcurrencyTest.ConcurrencyFixture fixture) : CosmosConcurrencyTest(fixture), IClassFixture<CosmosBulkConcurrencyTest.ConcurrencyFixture>
{
    // https://github.com/Azure/azure-cosmos-db-emulator-docker/issues/292 (Transactional batch limits not enforced)
    [CosmosCondition(CosmosCondition.IsNotLinuxEmulator)]
    public override Task Updating_then_deleting_the_same_entity_results_in_DbUpdateConcurrencyException()
        => base.Updating_then_deleting_the_same_entity_results_in_DbUpdateConcurrencyException();

    // https://github.com/Azure/azure-cosmos-db-emulator-docker/issues/292 (Transactional batch limits not enforced)
    [CosmosCondition(CosmosCondition.IsNotLinuxEmulator)]
    public override Task Updating_then_updating_the_same_entity_results_in_DbUpdateConcurrencyException()
        => base.Updating_then_updating_the_same_entity_results_in_DbUpdateConcurrencyException();

    public class ConcurrencyFixture : CosmosConcurrencyTest.CosmosFixture
    {
        public override ConcurrencyContext CreateContext()
        {
            var context = base.CreateContext();
            context.Database.AutoTransactionBehavior = AutoTransactionBehavior.Never;
            return context;
        }

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).UseCosmos(x => x.BulkExecutionEnabled());
    }

    protected override ConcurrencyContext CreateContext(DbContextOptions options)
    {
        var context = base.CreateContext(options);
        context.Database.AutoTransactionBehavior = AutoTransactionBehavior.Never;
        return context;
    }
}
