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

        public override void Query_with_query_type()
        {
            // TODO: #10680
            //base.Query_with_query_type();
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
                modelBuilder.Entity<HasContextProperty<DbContext>>().ToTable("HasContextP_DbContext");
                modelBuilder.Entity<HasContextProperty<WithConstructorsContext>>().ToTable("HasContextP_WithConstructorsContext");
                modelBuilder.Entity<HasContextProperty<OtherContext>>().ToTable("HasContextP_OtherContext");
                modelBuilder.Entity<HasContextPc<DbContext>>().ToTable("HasContextPc_DbContext");
                modelBuilder.Entity<HasContextPc<WithConstructorsContext>>().ToTable("HasContextPc_WithConstructorsContext");
                modelBuilder.Entity<HasContextPc<OtherContext>>().ToTable("HasContextPc_OtherContext");

                modelBuilder.Entity<Blog>(
                    b => b.Property("_blogId").HasColumnName("BlogId"));

                modelBuilder.Entity<Post>(
                    b => b.Property("_id").HasColumnName("Id"));
            }
        }
    }
}
