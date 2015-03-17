// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.FunctionalTests.TestModels.NullSemantics;
using Microsoft.Data.Entity.FunctionalTests.TestModels.NullSemanticsModel;
using Xunit;

namespace Microsoft.Data.Entity.FunctionalTests
{
    public abstract class NullSemanticsQueryTestBase<TTestStore, TFixture> : IClassFixture<TFixture>, IDisposable
        where TTestStore : TestStore
        where TFixture : NullSemanticsQueryFixtureBase<TTestStore>, new()
    {
        NullSemanticsData _oracleData = new NullSemanticsData();

        protected NullSemanticsContext CreateContext()
        {
            return Fixture.CreateContext(TestStore);
        }

        protected NullSemanticsQueryTestBase(TFixture fixture)
        {
            Fixture = fixture;

            TestStore = Fixture.CreateTestStore();
        }

        protected TFixture Fixture { get; }

        protected TTestStore TestStore { get; }

        public void Dispose()
        {
            TestStore.Dispose();
        }

        ////[Fact]
        public virtual void Compare_bool_with_bool_equal()
        {
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => e.BoolA == e.BoolB));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => e.NullableBoolA == e.NullableBoolB));
        }

        ////[Fact]
        public virtual void Compare_negated_bool_with_bool_equal()
        {
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !e.BoolA == e.BoolB));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !e.NullableBoolA == e.NullableBoolB));
        }

        ////[Fact]
        public virtual void Compare_bool_with_negated_bool_equal()
        {
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => e.BoolA == !e.BoolB));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => e.NullableBoolA == !e.NullableBoolB));
        }

        ////[Fact]
        public virtual void Compare_negated_bool_with_negated_bool_equal()
        {
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !e.BoolA == !e.BoolB));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !e.NullableBoolA == !e.NullableBoolB));
        }

        ////[Fact]
        public virtual void Compare_bool_with_bool_negated_equal()
        {
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !(e.BoolA == e.BoolB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !(e.NullableBoolA == e.NullableBoolB)));
        }

        ////[Fact]
        public virtual void Compare_negated_bool_with_bool_negated_equal()
        {
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !(!e.BoolA == e.BoolB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !(!e.NullableBoolA == e.NullableBoolB)));
        }

        ////[Fact]
        public virtual void Compare_negated_bool_with_negated_bool_negated_equal()
        {
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !(!e.BoolA == !e.BoolB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !(!e.NullableBoolA == !e.NullableBoolB)));
        }

        ////[Fact]
        public virtual void Compare_bool_with_bool_not_equal()
        {
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => e.BoolA != e.BoolB));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => e.NullableBoolA != e.NullableBoolB));
        }

        ////[Fact]
        public virtual void Compare_negated_bool_with_bool_not_equal()
        {
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !e.BoolA != e.BoolB));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !e.NullableBoolA != e.NullableBoolB));
        }

        ////[Fact]
        public virtual void Compare_bool_with_negated_bool_not_equal()
        {
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => e.BoolA != !e.BoolB));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => e.NullableBoolA != !e.NullableBoolB));
        }

        ////[Fact]
        public virtual void Compare_negated_bool_with_negated_bool_not_equal()
        {
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !e.BoolA != !e.BoolB));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !e.NullableBoolA != !e.NullableBoolB));
        }

        ////[Fact]
        public virtual void Compare_bool_with_bool_negated_not_equal()
        {
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !(e.BoolA != e.BoolB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !(e.NullableBoolA != e.NullableBoolB)));
        }

        ////[Fact]
        public virtual void Compare_negated_bool_with_bool_negated_not_equal()
        {
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !(!e.BoolA != e.BoolB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !(!e.NullableBoolA != e.NullableBoolB)));
        }

        ////[Fact]
        public virtual void Compare_negated_bool_with_negated_bool_negated_not_equal()
        {
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !(!e.BoolA != !e.BoolB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !(!e.NullableBoolA != !e.NullableBoolB)));
        }

        ////[Fact]
        public virtual void Compare_complex_equal_equal_equal()
        {
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => (e.BoolA == e.BoolB) == (e.IntA == e.IntB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => (e.NullableBoolA == e.NullableBoolB) == (e.NullableIntA == e.NullableIntB)));
        }

        ////[Fact]
        public virtual void Compare_complex_equal_not_equal_equal()
        {
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => (e.BoolA == e.BoolB) != (e.IntA == e.IntB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => (e.NullableBoolA == e.NullableBoolB) != (e.NullableIntA == e.NullableIntB)));
        }

        ////[Fact]
        public virtual void Compare_complex_not_equal_equal_equal()
        {
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => (e.BoolA != e.BoolB) == (e.IntA == e.IntB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => (e.NullableBoolA != e.NullableBoolB) == (e.NullableIntA == e.NullableIntB)));
        }

        ////[Fact]
        public virtual void Compare_complex_not_equal_not_equal_equal()
        {
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => (e.BoolA != e.BoolB) != (e.IntA == e.IntB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => (e.NullableBoolA != e.NullableBoolB) != (e.NullableIntA == e.NullableIntB)));
        }

        ////[Fact]
        public virtual void Compare_complex_not_equal_equal_not_equal()
        {
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => (e.BoolA != e.BoolB) == (e.IntA != e.IntB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => (e.NullableBoolA != e.NullableBoolB) == (e.NullableIntA != e.NullableIntB)));
        }

        ////[Fact]
        public virtual void Compare_complex_not_equal_not_equal_not_equal()
        {
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => (e.BoolA != e.BoolB) != (e.IntA != e.IntB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => (e.NullableBoolA != e.NullableBoolB) != (e.NullableIntA != e.NullableIntB)));
        }

        protected void AssertQuery<TItem>(Func<IQueryable<TItem>, IQueryable<TItem>> query)
            where TItem : NullSemanticsEntityBase
        {
            var actualIds = new List<int>();
            var expectedIds = new List<int>();

            expectedIds.AddRange(query(_oracleData.Set<TItem>().ToList().AsQueryable()).Select(e => e.Id).OrderBy(k => k));

            using (var productContext = CreateContext())
            {
                actualIds.AddRange(query(productContext.Set<TItem>()).Select(e => e.Id).ToList().OrderBy(k => k));
            }

            Assert.Equal(expectedIds.Count, actualIds.Count);
            for (int i = 0; i < expectedIds.Count; i++)
            {
                Assert.Equal(expectedIds[i], actualIds[i]);
            }
        }
    }
}