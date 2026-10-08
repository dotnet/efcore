// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public abstract class ComplexTypesTrackingRelationalTestBase<TFixture> : ComplexTypesTrackingTestBase<TFixture>
    where TFixture : ComplexTypesTrackingRelationalTestBase<TFixture>.RelationalFixtureBase
{
    protected ComplexTypesTrackingRelationalTestBase(TFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        fixture.TestSqlLoggerFactory.Clear();
        fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        => facade.UseTransaction(transaction.GetDbTransaction());

    public abstract class RelationalFixtureBase : FixtureBase
    {
        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder.Entity<PubWithCollections>(b =>
            {
                b.ComplexCollection(
                    e => e.Activities, b => b.ToJson());
            });

            modelBuilder.Entity<PubWithRecordCollections>(b =>
            {
                b.ComplexCollection(
                    e => e.Activities, b => b.ToJson());
            });

            modelBuilder.Entity<PubWithArrayCollections>(b =>
            {
                b.ComplexCollection(
                    e => e.Activities, b => b.ToJson());
            });

            // TODO: Issue #31411
            //modelBuilder.Entity<PubWithStructArrayCollections>(
            //    b =>
            //    {
            //        b.ComplexCollection(
            //            e => e.Activities, b => b.ToJson());
            //    });

            //modelBuilder.Entity<PubWithReadonlyStructArrayCollections>(
            //    b =>
            //    {
            //        b.ComplexCollection(
            //            e => e.Activities, b => b.ToJson());
            //    });

            modelBuilder.Entity<PubWithRecordArrayCollections>(b =>
            {
                b.ComplexCollection(
                    e => e.Activities, b => b.ToJson());
            });

            modelBuilder.Entity<PubWithPropertyBagCollections>(b =>
            {
                b.ComplexCollection(
                    e => e.Activities, b => b.ToJson());
            });

            if (!UseProxies)
            {
                modelBuilder.Entity<FieldPubWithCollections>(b =>
                {
                    b.ComplexCollection(
                        e => e.Activities, b => b.ToJson());
                });

                modelBuilder.Entity<FieldPubWithRecordCollections>(b =>
                {
                    b.ComplexCollection(
                        e => e.Activities, b => b.ToJson());
                });
            }
        }
    }
}
