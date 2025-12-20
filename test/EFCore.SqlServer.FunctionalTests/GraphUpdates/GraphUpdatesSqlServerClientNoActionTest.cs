// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class GraphUpdatesSqlServerClientNoActionTest(GraphUpdatesSqlServerClientNoActionTest.SqlServerFixture fixture)
    : GraphUpdatesSqlServerTestBase<
        GraphUpdatesSqlServerClientNoActionTest.SqlServerFixture>(fixture)
{
    // These tests require specific delete behaviors that are overridden to ClientNoAction in this fixture
    public override Task ClientSetDefault_with_sentinel_value_sets_FK_to_sentinel_on_delete(bool async)
        => Task.CompletedTask;

    public override Task SetDefault_with_default_value_sets_FK_to_default_on_delete(bool async)
        => Task.CompletedTask;

    protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        => facade.UseTransaction(transaction.GetDbTransaction());

    public class SqlServerFixture : GraphUpdatesSqlServerFixtureBase
    {
        public override bool ForceClientNoAction
            => true;

        protected override string StoreName
            => "GraphClientNoActionUpdatesTest";

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            foreach (var foreignKey in modelBuilder.Model
                         .GetEntityTypes()
                         .SelectMany(e => e.GetDeclaredForeignKeys()))
            {
                foreignKey.DeleteBehavior = DeleteBehavior.ClientNoAction;
            }
        }
    }
}
