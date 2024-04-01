// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class GraphUpdatesSqlServerClientNoActionTest(GraphUpdatesSqlServerClientNoActionTest.SqlServerFixture fixture) : GraphUpdatesSqlServerTestBase<
    GraphUpdatesSqlServerClientNoActionTest.SqlServerFixture>(fixture)
{
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
