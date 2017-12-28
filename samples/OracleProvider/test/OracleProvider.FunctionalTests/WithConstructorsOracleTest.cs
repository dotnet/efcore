// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore
{
    public class WithConstructorsOracleTest : WithConstructorsTestBase<WithConstructorsOracleTest.WithConstructorsOracleFixture>
    {
        public WithConstructorsOracleTest(WithConstructorsOracleFixture fixture)
            : base(fixture)
        {
        }

        protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
            => facade.UseTransaction(transaction.GetDbTransaction());

        public class WithConstructorsOracleFixture : WithConstructorsFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => OracleTestStoreFactory.Instance;

            protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            {
                base.OnModelCreating(modelBuilder, context);

                modelBuilder.Entity<HasContext<DbContext>>().ToTable("HasContext_DbContext");
                modelBuilder.Entity<HasContext<WithConstructorsContext>>().ToTable("HasContext_WithConstructorsContext");
                modelBuilder.Entity<HasContext<OtherContext>>().ToTable("HasContext_OtherContext");

                modelBuilder.Entity<Blog>(
                    b => { b.Property("_blogId").HasColumnName("BlogId"); });

                modelBuilder.Entity<Post>(
                    b => { b.Property("_id").HasColumnName("Id"); });
            }
        }
    }
}
