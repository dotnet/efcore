// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore
{
    public class GraphUpdatesSqlServerIdentityTest : GraphUpdatesSqlServerTestBase<GraphUpdatesSqlServerIdentityTest.SqlServerFixture>
    {
        public GraphUpdatesSqlServerIdentityTest(SqlServerFixture fixture)
            : base(fixture)
        {
        }

        protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
            => facade.UseTransaction(transaction.GetDbTransaction());

        public class SqlServerFixture : GraphUpdatesSqlServerFixtureBase
        {
            protected override string StoreName { get; } = "GraphIdentityUpdatesTest";

            protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            {
                modelBuilder.UseIdentityColumns();

                base.OnModelCreating(modelBuilder, context);
            }
        }
    }
}
