// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class OwnedQueryRelationalTestBase<TFixture> : OwnedQueryTestBase<TFixture>
        where TFixture : OwnedQueryRelationalTestBase<TFixture>.RelationalOwnedQueryFixture, new()
    {
        protected OwnedQueryRelationalTestBase(TFixture fixture)
            : base(fixture)
        {
        }

        protected virtual bool CanExecuteQueryString => false;

        protected override QueryAsserter CreateQueryAsserter(TFixture fixture)
            => new RelationalQueryAsserter(fixture, RewriteExpectedQueryExpression, RewriteServerQueryExpression, canExecuteQueryString: CanExecuteQueryString);

        public abstract class RelationalOwnedQueryFixture : OwnedQueryFixtureBase
        {
            public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ListLoggerFactory;

            protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            {
                base.OnModelCreating(modelBuilder, context);

                // TODO: See issue#20334
                //modelBuilder.Entity<OwnedPerson>().OwnsOne(e => e.PersonAddress).Property(e => e.PlaceType).HasColumnName("PlaceType");
                //modelBuilder.Entity<Branch>().OwnsOne(e => e.BranchAddress).Property(e => e.PlaceType).HasColumnName("PlaceType");
                //modelBuilder.Entity<LeafA>().OwnsOne(e => e.LeafAAddress).Property(e => e.PlaceType).HasColumnName("PlaceType");
                //modelBuilder.Entity<LeafB>().OwnsOne(e => e.LeafBAddress).Property(e => e.PlaceType).HasColumnName("PlaceType");
            }
        }
    }
}
