// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.NullSemanticsModel;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestUtilities.Xunit;
using Xunit;

// ReSharper disable ReturnValueOfPureMethodIsNotUsed
// ReSharper disable NegativeEqualityExpression

namespace Microsoft.EntityFrameworkCore.Specification.Tests
{
    public abstract class NullSemanticsQueryTestBase<TTestStore, TFixture> : IClassFixture<TFixture>, IDisposable
        where TTestStore : TestStore
        where TFixture : NullSemanticsQueryRelationalFixture<TTestStore>, new()
    {
        private readonly NullSemanticsData _oracleData = new NullSemanticsData();

        protected NullSemanticsContext CreateContext(bool useRelationalNulls = false)
            => Fixture.CreateContext(TestStore, useRelationalNulls);

        protected NullSemanticsQueryTestBase(TFixture fixture)
        {
            Fixture = fixture;

            TestStore = Fixture.CreateTestStore();
        }

        protected TFixture Fixture { get; }

        protected TTestStore TestStore { get; }

        public void Dispose() => TestStore.Dispose();

        [Fact]
        public virtual void Compare_bool_with_bool_equal()
        {
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => e.BoolA == e.BoolB));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => e.BoolA == e.NullableBoolB));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => e.NullableBoolA == e.BoolB));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => e.NullableBoolA == e.NullableBoolB));
        }

        [Fact]
        public virtual void Compare_negated_bool_with_bool_equal()
        {
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !e.BoolA == e.BoolB));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !e.BoolA == e.NullableBoolB));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !e.NullableBoolA == e.BoolB));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !e.NullableBoolA == e.NullableBoolB));
        }

        [Fact]
        public virtual void Compare_bool_with_negated_bool_equal()
        {
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => e.BoolA == !e.BoolB));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => e.BoolA == !e.NullableBoolB));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => e.NullableBoolA == !e.BoolB));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => e.NullableBoolA == !e.NullableBoolB));
        }

        [Fact]
        public virtual void Compare_negated_bool_with_negated_bool_equal()
        {
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !e.BoolA == !e.BoolB));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !e.BoolA == !e.NullableBoolB));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !e.NullableBoolA == !e.BoolB));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !e.NullableBoolA == !e.NullableBoolB));
        }

        [Fact]
        public virtual void Compare_bool_with_bool_equal_negated()
        {
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !(e.BoolA == e.BoolB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !(e.BoolA == e.NullableBoolB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !(e.NullableBoolA == e.BoolB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !(e.NullableBoolA == e.NullableBoolB)));
        }

        [Fact]
        public virtual void Compare_negated_bool_with_bool_equal_negated()
        {
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !(!e.BoolA == e.BoolB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !(!e.BoolA == e.NullableBoolB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !(!e.NullableBoolA == e.BoolB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !(!e.NullableBoolA == e.NullableBoolB)));
        }

        [Fact]
        public virtual void Compare_bool_with_negated_bool_equal_negated()
        {
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !(e.BoolA == !e.BoolB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !(e.BoolA == !e.NullableBoolB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !(e.NullableBoolA == !e.BoolB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !(e.NullableBoolA == !e.NullableBoolB)));
        }

        [Fact]
        public virtual void Compare_negated_bool_with_negated_bool_equal_negated()
        {
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !(!e.BoolA == !e.BoolB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !(!e.BoolA == !e.NullableBoolB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !(!e.NullableBoolA == !e.BoolB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !(!e.NullableBoolA == !e.NullableBoolB)));
        }

        [Fact]
        public virtual void Compare_bool_with_bool_not_equal()
        {
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => e.BoolA != e.BoolB));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => e.BoolA != e.NullableBoolB));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => e.NullableBoolA != e.BoolB));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => e.NullableBoolA != e.NullableBoolB));
        }

        [Fact]
        public virtual void Compare_negated_bool_with_bool_not_equal()
        {
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !e.BoolA != e.BoolB));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !e.BoolA != e.NullableBoolB));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !e.NullableBoolA != e.BoolB));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !e.NullableBoolA != e.NullableBoolB));
        }

        [Fact]
        public virtual void Compare_bool_with_negated_bool_not_equal()
        {
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => e.BoolA != !e.BoolB));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => e.BoolA != !e.NullableBoolB));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => e.NullableBoolA != !e.BoolB));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => e.NullableBoolA != !e.NullableBoolB));
        }

        [Fact]
        public virtual void Compare_negated_bool_with_negated_bool_not_equal()
        {
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !e.BoolA != !e.BoolB));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !e.BoolA != !e.NullableBoolB));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !e.NullableBoolA != !e.BoolB));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !e.NullableBoolA != !e.NullableBoolB));
        }

        [Fact]
        public virtual void Compare_bool_with_bool_not_equal_negated()
        {
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !(e.BoolA != e.BoolB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !(e.BoolA != e.NullableBoolB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !(e.NullableBoolA != e.BoolB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !(e.NullableBoolA != e.NullableBoolB)));
        }

        [Fact]
        public virtual void Compare_negated_bool_with_bool_not_equal_negated()
        {
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !(!e.BoolA != e.BoolB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !(!e.BoolA != e.NullableBoolB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !(!e.NullableBoolA != e.BoolB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !(!e.NullableBoolA != e.NullableBoolB)));
        }

        [Fact]
        public virtual void Compare_bool_with_negated_bool_not_equal_negated()
        {
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !(e.BoolA != !e.BoolB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !(e.BoolA != !e.NullableBoolB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !(e.NullableBoolA != !e.BoolB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !(e.NullableBoolA != !e.NullableBoolB)));
        }

        [Fact]
        public virtual void Compare_negated_bool_with_negated_bool_not_equal_negated()
        {
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !(!e.BoolA != !e.BoolB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !(!e.BoolA != !e.NullableBoolB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !(!e.NullableBoolA != !e.BoolB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !(!e.NullableBoolA != !e.NullableBoolB)));
        }

        [Fact]
        public virtual void Compare_equals_method()
        {
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => e.BoolA.Equals(e.BoolB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => e.BoolA.Equals(e.NullableBoolB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => e.NullableBoolA.Equals(e.BoolB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => e.NullableBoolA.Equals(e.NullableBoolB)));
        }

        [Fact]
        public virtual void Compare_equals_method_negated()
        {
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !e.BoolA.Equals(e.BoolB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !e.BoolA.Equals(e.NullableBoolB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !e.NullableBoolA.Equals(e.BoolB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !e.NullableBoolA.Equals(e.NullableBoolB)));
        }

        [Fact]
        public virtual void Compare_complex_equal_equal_equal()
        {
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => (e.BoolA == e.BoolB) == (e.IntA == e.IntB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => (e.NullableBoolA == e.BoolB) == (e.IntA == e.NullableIntB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => (e.NullableBoolA == e.NullableBoolB) == (e.NullableIntA == e.NullableIntB)));
        }

        [Fact]
        public virtual void Compare_complex_equal_not_equal_equal()
        {
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => (e.BoolA == e.BoolB) != (e.IntA == e.IntB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => (e.NullableBoolA == e.BoolB) != (e.IntA == e.NullableIntB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => (e.NullableBoolA == e.NullableBoolB) != (e.NullableIntA == e.NullableIntB)));
        }

        [Fact]
        public virtual void Compare_complex_not_equal_equal_equal()
        {
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => (e.BoolA != e.BoolB) == (e.IntA == e.IntB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => (e.NullableBoolA != e.BoolB) == (e.IntA == e.NullableIntB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => (e.NullableBoolA != e.NullableBoolB) == (e.NullableIntA == e.NullableIntB)));
        }

        [Fact]
        public virtual void Compare_complex_not_equal_not_equal_equal()
        {
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => (e.BoolA != e.BoolB) != (e.IntA == e.IntB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => (e.NullableBoolA != e.BoolB) != (e.IntA == e.NullableIntB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => (e.NullableBoolA != e.NullableBoolB) != (e.NullableIntA == e.NullableIntB)));
        }

        [Fact]
        public virtual void Compare_complex_not_equal_equal_not_equal()
        {
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => (e.BoolA != e.BoolB) == (e.IntA != e.IntB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => (e.NullableBoolA != e.BoolB) == (e.IntA != e.NullableIntB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => (e.NullableBoolA != e.NullableBoolB) == (e.NullableIntA != e.NullableIntB)));
        }

        [Fact]
        public virtual void Compare_complex_not_equal_not_equal_not_equal()
        {
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => (e.BoolA != e.BoolB) != (e.IntA != e.IntB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => (e.NullableBoolA != e.BoolB) != (e.IntA != e.NullableIntB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => (e.NullableBoolA != e.NullableBoolB) != (e.NullableIntA != e.NullableIntB)));
        }

        [Fact]
        public virtual void Compare_nullable_with_null_parameter_equal()
        {
            string prm = null;

            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => e.NullableStringA == prm));
        }

        [Fact]
        public virtual void Compare_nullable_with_non_null_parameter_not_equal()
        {
            var prm = "Foo";

            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => e.NullableStringA == prm));
        }

        [ConditionalFact]
        public virtual void Join_uses_database_semantics()
        {
            using (var context = CreateContext())
            {
                var query = from e1 in context.Entities1
                            join e2 in context.Entities2 on e1.NullableIntA equals e2.NullableIntB
                            select new { Id1 = e1.Id, Id2 = e2.Id, e1.NullableIntA, e2.NullableIntB };

                query.ToList();
            }
        }

        [Fact]
        public virtual void Contains_with_local_array_closure_with_null()
        {
            string[] ids = { "Foo", null };
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => ids.Contains(e.NullableStringA)));
        }

        [Fact]
        public virtual void Contains_with_local_array_closure_with_multiple_nulls()
        {
            string[] ids = { null, "Foo", null, null };
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => ids.Contains(e.NullableStringA)));
        }

        [Fact]
        public virtual void Contains_with_local_array_closure_false_with_null()
        {
            string[] ids = { "Foo", null };
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !ids.Contains(e.NullableStringA)));
        }

        [Fact]
        public virtual void Where_multiple_ors_with_null()
        {
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => e.NullableStringA == "Foo" || e.NullableStringA == "Blah" || e.NullableStringA == null));
        }

        [Fact]
        public virtual void Where_multiple_ands_with_null()
        {
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => e.NullableStringA != "Foo" && e.NullableStringA != "Blah" && e.NullableStringA != null));
        }

        [Fact]
        public virtual void Where_multiple_ors_with_nullable_parameter()
        {
            string prm = null;
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => e.NullableStringA == "Foo" || e.NullableStringA == prm));
        }

        [Fact]
        public virtual void Where_multiple_ands_with_nullable_parameter_and_constant()
        {
            string prm1 = null;
            string prm2 = null;
            var prm3 = "Blah";

            AssertQuery<NullSemanticsEntity1>(es => es.Where(e =>
                e.NullableStringA != "Foo"
                && e.NullableStringA != prm1
                && e.NullableStringA != prm2
                && e.NullableStringA != prm3));
        }

        [Fact]
        public virtual void Where_multiple_ands_with_nullable_parameter_and_constant_not_optimized()
        {
            string prm1 = null;
            string prm2 = null;
            var prm3 = "Blah";

            AssertQuery<NullSemanticsEntity1>(es => es.Where(e =>
                e.NullableStringB != null
                && e.NullableStringA != "Foo"
                && e.NullableStringA != prm1
                && e.NullableStringA != prm2
                && e.NullableStringA != prm3));
        }

        [Fact]
        public virtual void Where_equal_nullable_with_null_value_parameter()
        {
            string prm = null;

            AssertQuery<NullSemanticsEntity1>(es => es.Where(e =>
                e.NullableStringA == prm));
        }

        [Fact]
        public virtual void Where_not_equal_nullable_with_null_value_parameter()
        {
            string prm = null;

            AssertQuery<NullSemanticsEntity1>(es => es.Where(e =>
                e.NullableStringA != prm));
        }

        [Fact]
        public virtual void Where_equal_with_coalesce()
        {
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e =>
                (e.NullableStringA ?? e.NullableStringB) == e.NullableStringC));
        }

        [Fact]
        public virtual void Where_not_equal_with_coalesce()
        {
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e =>
                (e.NullableStringA ?? e.NullableStringB) != e.NullableStringC));
        }

        [Fact]
        public virtual void Where_equal_with_coalesce_both_sides()
        {
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e =>
                (e.NullableStringA ?? e.NullableStringB) == (e.StringA ?? e.StringB)));
        }

        [Fact]
        public virtual void Where_not_equal_with_coalesce_both_sides()
        {
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e =>
                (e.NullableIntA ?? e.NullableIntB) != (e.NullableIntC ?? e.NullableIntB)));
        }

        [Fact]
        public virtual void Where_equal_with_conditional()
        {
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e =>
                (e.NullableStringA == e.NullableStringB
                    ? e.NullableStringA
                    : e.NullableStringB) == e.NullableStringC));
        }

        [Fact]
        public virtual void Where_not_equal_with_conditional()
        {
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e =>
                e.NullableStringC != (e.NullableStringA == e.NullableStringB
                    ? e.NullableStringA
                    : e.NullableStringB)));
        }

        [Fact]
        public virtual void Where_equal_with_conditional_non_nullable()
        {
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e =>
                e.NullableStringC != (e.NullableStringA == e.NullableStringB
                    ? e.StringA
                    : e.StringB)));
        }

        [Fact]
        public virtual void Where_equal_with_and_and_contains()
        {
            AssertQuery<NullSemanticsEntity1>(
                es => es.Where(e => e.NullableStringA.Contains(e.NullableStringB) && e.BoolA),
                es => es.Where(e =>
                    (e.NullableStringA != null && e.NullableStringA.Contains(e.NullableStringB ?? "Blah")) && e.BoolA),
                useRelationalNulls: false);
        }

        protected void AssertQuery<TItem>(
            Func<IQueryable<TItem>, IQueryable<TItem>> query,
            bool useDatabaseNullSemantics = false)
            where TItem : NullSemanticsEntityBase
            => AssertQuery(query, query, useDatabaseNullSemantics);

        [Fact]
        public virtual void Where_equal_using_relational_null_semantics()
        {
            using (var context = CreateContext(useRelationalNulls: true))
            {
                context.Entities1
                    .Where(e => e.NullableBoolA == e.NullableBoolB)
                    .Select(e => e.Id).ToList();
            }
        }

        [Fact]
        public virtual void Where_nullable_bool()
        {
            using (var context = CreateContext())
            {
                context.Entities1
                    .Where(e => e.NullableBoolA.Value)
                    .Select(e => e.Id).ToList();
            }
        }

        [Fact]
        public virtual void Where_nullable_bool_equal_with_constant()
        {
            using (var context = CreateContext())
            {
                context.Entities1
                    .Where(e => e.NullableBoolA == true)
                    .Select(e => e.Id).ToList();
            }
        }

        [Fact]
        public virtual void Where_nullable_bool_with_null_check()
        {
            using (var context = CreateContext())
            {
                context.Entities1
                    .Where(e => e.NullableBoolA != null && e.NullableBoolA.Value)
                    .Select(e => e.Id).ToList();
            }
        }

        [Fact]
        public virtual void Where_equal_using_relational_null_semantics_with_parameter()
        {
            using (var context = CreateContext(useRelationalNulls: true))
            {
                bool? prm = null;
                context.Entities1
                    .Where(e => e.NullableBoolA == prm)
                    .Select(e => e.Id).ToList();
            }
        }

        [Fact]
        public virtual void Where_equal_using_relational_null_semantics_complex_with_parameter()
        {
            using (var context = CreateContext(useRelationalNulls: true))
            {
                var prm = false;
                context.Entities1
                    .Where(e => e.NullableBoolA == e.NullableBoolB || prm)
                    .Select(e => e.Id).ToList();
            }
        }

        [Fact]
        public virtual void Where_not_equal_using_relational_null_semantics()
        {
            using (var context = CreateContext(useRelationalNulls: true))
            {
                context.Entities1
                    .Where(e => e.NullableBoolA != e.NullableBoolB)
                    .Select(e => e.Id).ToList();
            }
        }

        [Fact]
        public virtual void Where_not_equal_using_relational_null_semantics_with_parameter()
        {
            using (var context = CreateContext(useRelationalNulls: true))
            {
                bool? prm = null;
                context.Entities1
                    .Where(e => e.NullableBoolA != prm)
                    .Select(e => e.Id).ToList();
            }
        }

        [Fact]
        public virtual void Where_not_equal_using_relational_null_semantics_complex_with_parameter()
        {
            using (var context = CreateContext(useRelationalNulls: true))
            {
                var prm = false;
                context.Entities1
                    .Where(e => e.NullableBoolA != e.NullableBoolB || prm)
                    .Select(e => e.Id).ToList();
            }
        }

        [Fact]
        public virtual void Where_comparison_null_constant_and_null_parameter()
        {
            string prm = null;
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => prm == null));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => prm != null));
        }

        [Fact]
        public virtual void Where_comparison_null_constant_and_nonnull_parameter()
        {
            string prm = "Foo";
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => null == prm));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => null != prm));
        }

        [Fact]
        public virtual void Where_comparison_nonnull_constant_and_null_parameter()
        {
            string prm = null;
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => "Foo" == prm));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => "Foo" != prm));
        }

        [Fact]
        public virtual void Where_comparison_null_semantics_optimization_works_with_complex_predicates()
        {
            string prm = null;
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => null == prm && e.NullableStringA == prm));
        }

        [Fact]
        public virtual void Switching_null_semantics_produces_different_cache_entry()
        {
            List<int> results1, results2;

            using (var context = CreateContext())
            {
                var query = context.Entities1
                    .Where(e => e.NullableBoolA == e.NullableBoolB)
                    .Select(e => e.Id);

                results1 = query.ToList();
            }

            using (var context = CreateContext(useRelationalNulls: true))
            {
                var query = context.Entities1
                    .Where(e => e.NullableBoolA == e.NullableBoolB)
                    .Select(e => e.Id);

                results2 = query.ToList();
            }

            Assert.True(results1.Count != results2.Count);
        }

        [Fact]
        public virtual void Switching_parameter_value_to_null_produces_different_cache_entry()
        {
            using (var context = CreateContext())
            {
                var prm = "Foo";
                var query = context.Entities1
                    .Where(e => prm == "Foo")
                    .Select(e => e.Id);

                var results1 = query.ToList();

                prm = null;

                var results2 = query.ToList();

                Assert.True(results1.Count != results2.Count);
            }
        }

        [Fact]
        public virtual void From_sql_composed_with_relational_null_comparison()
        {
            using (var context = CreateContext(useRelationalNulls: true))
            {
                var actual = context.Entities1
                    .FromSql(@"SELECT * FROM ""NullSemanticsEntity1""")
                    .Where(c => c.StringA == c.StringB)
                    .ToArray();

                Assert.Equal(15, actual.Length);
            }
        }

        protected void AssertQuery<TItem>(
            Func<IQueryable<TItem>, IQueryable<TItem>> l2eQuery,
            Func<IQueryable<TItem>, IQueryable<TItem>> l2oQuery,
            bool useRelationalNulls)
            where TItem : NullSemanticsEntityBase
        {
            var actualIds = new List<int>();
            var expectedIds = new List<int>();

            expectedIds.AddRange(
                l2oQuery(_oracleData.Set<TItem>().ToList().AsQueryable())
                    .Select(e => e.Id)
                    .OrderBy(k => k));

            using (var context = CreateContext(useRelationalNulls))
            {
                actualIds.AddRange(
                    l2eQuery(context.Set<TItem>())
                        .Select(e => e.Id)
                        .ToList()
                        .OrderBy(k => k));
            }

            if (!useRelationalNulls)
            {
                Assert.Equal(expectedIds.Count, actualIds.Count);
                for (var i = 0; i < expectedIds.Count; i++)
                {
                    Assert.Equal(expectedIds[i], actualIds[i]);
                }
            }
        }
    }
}
