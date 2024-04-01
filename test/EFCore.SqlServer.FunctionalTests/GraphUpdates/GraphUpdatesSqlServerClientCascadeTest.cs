// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class GraphUpdatesSqlServerClientCascadeTest(GraphUpdatesSqlServerClientCascadeTest.SqlServerFixture fixture) : GraphUpdatesSqlServerTestBase<
    GraphUpdatesSqlServerClientCascadeTest.SqlServerFixture>(fixture)
{
    protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        => facade.UseTransaction(transaction.GetDbTransaction());

    public class SqlServerFixture : GraphUpdatesSqlServerFixtureBase
    {
        public override bool NoStoreCascades
            => true;

        protected override string StoreName
            => "GraphClientCascadeUpdatesTest";

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            foreach (var foreignKey in modelBuilder.Model
                         .GetEntityTypes()
                         .SelectMany(e => e.GetDeclaredForeignKeys())
                         .Where(e => e.DeleteBehavior == DeleteBehavior.Cascade))
            {
                foreignKey.DeleteBehavior = DeleteBehavior.ClientCascade;
            }
        }
    }
}
