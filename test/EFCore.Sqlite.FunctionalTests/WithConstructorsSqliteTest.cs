// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class WithConstructorsSqliteTest(WithConstructorsSqliteTest.WithConstructorsSqliteFixture fixture) : WithConstructorsTestBase<WithConstructorsSqliteTest.WithConstructorsSqliteFixture>(fixture)
{
    protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        => facade.UseTransaction(transaction.GetDbTransaction());

    public class WithConstructorsSqliteFixture : WithConstructorsFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqliteTestStoreFactory.Instance;

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder.Entity<BlogQuery>().HasNoKey().ToSqlQuery("SELECT * FROM Blog");
        }
    }
}
