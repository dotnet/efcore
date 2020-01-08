// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.TestModels.NullSemanticsModel;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class NullSemanticsQueryFixtureBase : SharedStoreFixtureBase<NullSemanticsContext>, IQueryFixtureBase
    {
        public NullSemanticsQueryFixtureBase()
        {
            var entitySorters = new Dictionary<Type, Func<dynamic, object>> { { typeof(NullSemanticsEntity1), e => e?.Id }, { typeof(NullSemanticsEntity2), e => e?.Id } }
                .ToDictionary(e => e.Key, e => (object)e.Value);

            var entityAsserters = new Dictionary<Type, Action<dynamic, dynamic>>
                {
                    {
                        typeof(NullSemanticsEntity1), (e, a) =>
                        {
                            Assert.Equal(e == null, a == null);
                            if (a != null)
                            {
                                Assert.Equal(e.Id, a.Id);
                                Assert.Equal(e.BoolA, a.BoolA);
                                Assert.Equal(e.BoolB, a.BoolB);
                                Assert.Equal(e.BoolC, a.BoolC);
                                Assert.Equal(e.IntA, a.IntA);
                                Assert.Equal(e.IntB, a.IntB);
                                Assert.Equal(e.IntC, a.IntC);
                                Assert.Equal(e.StringA, a.StringA);
                                Assert.Equal(e.StringB, a.StringB);
                                Assert.Equal(e.StringC, a.StringC);
                                Assert.Equal(e.NullableBoolA, a.NullableBoolA);
                                Assert.Equal(e.NullableBoolB, a.NullableBoolB);
                                Assert.Equal(e.NullableBoolC, a.NullableBoolC);
                                Assert.Equal(e.NullableIntA, a.NullableIntA);
                                Assert.Equal(e.NullableIntB, a.NullableIntB);
                                Assert.Equal(e.NullableIntC, a.NullableIntC);
                                Assert.Equal(e.NullableStringA, a.NullableStringA);
                                Assert.Equal(e.NullableStringB, a.NullableStringB);
                                Assert.Equal(e.NullableStringC, a.NullableStringC);
                            }
                        }
                    },
                    {
                        typeof(NullSemanticsEntity2), (e, a) =>
                        {
                            Assert.Equal(e == null, a == null);
                            if (a != null)
                            {
                                Assert.Equal(e.Id, a.Id);
                                Assert.Equal(e.BoolA, a.BoolA);
                                Assert.Equal(e.BoolB, a.BoolB);
                                Assert.Equal(e.BoolC, a.BoolC);
                                Assert.Equal(e.IntA, a.IntA);
                                Assert.Equal(e.IntB, a.IntB);
                                Assert.Equal(e.IntC, a.IntC);
                                Assert.Equal(e.StringA, a.StringA);
                                Assert.Equal(e.StringB, a.StringB);
                                Assert.Equal(e.StringC, a.StringC);
                                Assert.Equal(e.NullableBoolA, a.NullableBoolA);
                                Assert.Equal(e.NullableBoolB, a.NullableBoolB);
                                Assert.Equal(e.NullableBoolC, a.NullableBoolC);
                                Assert.Equal(e.NullableIntA, a.NullableIntA);
                                Assert.Equal(e.NullableIntB, a.NullableIntB);
                                Assert.Equal(e.NullableIntC, a.NullableIntC);
                                Assert.Equal(e.NullableStringA, a.NullableStringA);
                                Assert.Equal(e.NullableStringB, a.NullableStringB);
                                Assert.Equal(e.NullableStringC, a.NullableStringC);
                            }
                        }
                    },
                }.ToDictionary(e => e.Key, e => (object)e.Value);

            QueryAsserter = CreateQueryAsserter(entitySorters, entityAsserters);
        }

        protected virtual QueryAsserter<NullSemanticsContext> CreateQueryAsserter(
            Dictionary<Type, object> entitySorters,
            Dictionary<Type, object> entityAsserters)
            => new QueryAsserter<NullSemanticsContext>(
                CreateContext,
                new NullSemanticsData(),
                entitySorters,
                entityAsserters);

        protected override string StoreName { get; } = "NullSemanticsQueryTest";

        public QueryAsserterBase QueryAsserter { get; set; }

        public new RelationalTestStore TestStore => (RelationalTestStore)base.TestStore;

        public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ListLoggerFactory;

        public override NullSemanticsContext CreateContext()
        {
            var context = base.CreateContext();
            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            return context;
        }

        protected override void Seed(NullSemanticsContext context) => NullSemanticsContext.Seed(context);

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            modelBuilder.Entity<NullSemanticsEntity1>().Property(e => e.Id).ValueGeneratedNever();

            modelBuilder.Entity<NullSemanticsEntity1>().Property(e => e.StringA).IsRequired();
            modelBuilder.Entity<NullSemanticsEntity1>().Property(e => e.StringB).IsRequired();
            modelBuilder.Entity<NullSemanticsEntity1>().Property(e => e.StringC).IsRequired();

            modelBuilder.Entity<NullSemanticsEntity2>().Property(e => e.Id).ValueGeneratedNever();

            modelBuilder.Entity<NullSemanticsEntity2>().Property(e => e.StringA).IsRequired();
            modelBuilder.Entity<NullSemanticsEntity2>().Property(e => e.StringB).IsRequired();
            modelBuilder.Entity<NullSemanticsEntity2>().Property(e => e.StringC).IsRequired();
        }
    }
}
