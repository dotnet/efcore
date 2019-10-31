// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.TestModels.NullSemanticsModel;
using Xunit;

// ReSharper disable SimplifyConditionalTernaryExpression
// ReSharper disable AccessToModifiedClosure
// ReSharper disable PossibleMultipleEnumeration
// ReSharper disable StringStartsWithIsCultureSpecific
// ReSharper disable InconsistentNaming
// ReSharper disable ReturnValueOfPureMethodIsNotUsed
// ReSharper disable NegativeEqualityExpression

#pragma warning disable RCS1068 // Simplify logical negation.

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class NullSemanticsQueryTestBase<TFixture> : IClassFixture<TFixture>
        where TFixture : NullSemanticsQueryRelationalFixture, new()
    {
        private readonly NullSemanticsData _clientData = new NullSemanticsData();

        protected NullSemanticsQueryTestBase(TFixture fixture) => Fixture = fixture;

        protected TFixture Fixture { get; }

        [ConditionalFact]
        public virtual void Compare_bool_with_bool_equal()
        {
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => e.BoolA == e.BoolB));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => e.BoolA == e.NullableBoolB));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => e.NullableBoolA == e.BoolB));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => e.NullableBoolA == e.NullableBoolB));
        }

        [ConditionalFact]
        public virtual void Compare_negated_bool_with_bool_equal()
        {
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !e.BoolA == e.BoolB));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !e.BoolA == e.NullableBoolB));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !e.NullableBoolA == e.BoolB));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !e.NullableBoolA == e.NullableBoolB));
        }

        [ConditionalFact]
        public virtual void Compare_bool_with_negated_bool_equal()
        {
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => e.BoolA == !e.BoolB));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => e.BoolA == !e.NullableBoolB));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => e.NullableBoolA == !e.BoolB));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => e.NullableBoolA == !e.NullableBoolB));
        }

        [ConditionalFact]
        public virtual void Compare_negated_bool_with_negated_bool_equal()
        {
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !e.BoolA == !e.BoolB));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !e.BoolA == !e.NullableBoolB));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !e.NullableBoolA == !e.BoolB));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !e.NullableBoolA == !e.NullableBoolB));
        }

        [ConditionalFact]
        public virtual void Compare_bool_with_bool_equal_negated()
        {
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !(e.BoolA == e.BoolB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !(e.BoolA == e.NullableBoolB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !(e.NullableBoolA == e.BoolB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !(e.NullableBoolA == e.NullableBoolB)));
        }

        [ConditionalFact]
        public virtual void Compare_negated_bool_with_bool_equal_negated()
        {
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !(!e.BoolA == e.BoolB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !(!e.BoolA == e.NullableBoolB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !(!e.NullableBoolA == e.BoolB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !(!e.NullableBoolA == e.NullableBoolB)));
        }

        [ConditionalFact]
        public virtual void Compare_bool_with_negated_bool_equal_negated()
        {
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !(e.BoolA == !e.BoolB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !(e.BoolA == !e.NullableBoolB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !(e.NullableBoolA == !e.BoolB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !(e.NullableBoolA == !e.NullableBoolB)));
        }

        [ConditionalFact]
        public virtual void Compare_negated_bool_with_negated_bool_equal_negated()
        {
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !(!e.BoolA == !e.BoolB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !(!e.BoolA == !e.NullableBoolB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !(!e.NullableBoolA == !e.BoolB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !(!e.NullableBoolA == !e.NullableBoolB)));
        }

        [ConditionalFact]
        public virtual void Compare_bool_with_bool_not_equal()
        {
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => e.BoolA != e.BoolB));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => e.BoolA != e.NullableBoolB));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => e.NullableBoolA != e.BoolB));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => e.NullableBoolA != e.NullableBoolB));
        }

        [ConditionalFact]
        public virtual void Compare_negated_bool_with_bool_not_equal()
        {
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !e.BoolA != e.BoolB));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !e.BoolA != e.NullableBoolB));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !e.NullableBoolA != e.BoolB));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !e.NullableBoolA != e.NullableBoolB));
        }

        [ConditionalFact]
        public virtual void Compare_bool_with_negated_bool_not_equal()
        {
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => e.BoolA != !e.BoolB));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => e.BoolA != !e.NullableBoolB));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => e.NullableBoolA != !e.BoolB));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => e.NullableBoolA != !e.NullableBoolB));
        }

        [ConditionalFact]
        public virtual void Compare_negated_bool_with_negated_bool_not_equal()
        {
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !e.BoolA != !e.BoolB));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !e.BoolA != !e.NullableBoolB));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !e.NullableBoolA != !e.BoolB));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !e.NullableBoolA != !e.NullableBoolB));
        }

        [ConditionalFact]
        public virtual void Compare_bool_with_bool_not_equal_negated()
        {
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !(e.BoolA != e.BoolB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !(e.BoolA != e.NullableBoolB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !(e.NullableBoolA != e.BoolB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !(e.NullableBoolA != e.NullableBoolB)));
        }

        [ConditionalFact]
        public virtual void Compare_negated_bool_with_bool_not_equal_negated()
        {
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !(!e.BoolA != e.BoolB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !(!e.BoolA != e.NullableBoolB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !(!e.NullableBoolA != e.BoolB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !(!e.NullableBoolA != e.NullableBoolB)));
        }

        [ConditionalFact]
        public virtual void Compare_bool_with_negated_bool_not_equal_negated()
        {
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !(e.BoolA != !e.BoolB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !(e.BoolA != !e.NullableBoolB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !(e.NullableBoolA != !e.BoolB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !(e.NullableBoolA != !e.NullableBoolB)));
        }

        [ConditionalFact]
        public virtual void Compare_negated_bool_with_negated_bool_not_equal_negated()
        {
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !(!e.BoolA != !e.BoolB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !(!e.BoolA != !e.NullableBoolB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !(!e.NullableBoolA != !e.BoolB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !(!e.NullableBoolA != !e.NullableBoolB)));
        }

        [ConditionalFact]
        public virtual void Compare_equals_method()
        {
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => e.BoolA.Equals(e.BoolB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => e.BoolA.Equals(e.NullableBoolB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => e.NullableBoolA.Equals(e.BoolB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => e.NullableBoolA.Equals(e.NullableBoolB)));
        }

        [ConditionalFact]
        public virtual void Compare_equals_method_static()
        {
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => Equals(e.BoolA, e.BoolB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => Equals(e.BoolA, e.NullableBoolB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => Equals(e.NullableBoolA, e.BoolB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => Equals(e.NullableBoolA, e.NullableBoolB)));
        }

        [ConditionalFact]
        public virtual void Compare_equals_method_negated()
        {
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !e.BoolA.Equals(e.BoolB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !e.BoolA.Equals(e.NullableBoolB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !e.NullableBoolA.Equals(e.BoolB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !e.NullableBoolA.Equals(e.NullableBoolB)));
        }

        [ConditionalFact]
        public virtual void Compare_equals_method_negated_static()
        {
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !Equals(e.BoolA, e.BoolB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !Equals(e.BoolA, e.NullableBoolB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !Equals(e.NullableBoolA, e.BoolB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !Equals(e.NullableBoolA, e.NullableBoolB)));
        }

        [ConditionalFact]
        public virtual void Compare_complex_equal_equal_equal()
        {
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => e.BoolA == e.BoolB == (e.IntA == e.IntB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => e.NullableBoolA == e.BoolB == (e.IntA == e.NullableIntB)));
            AssertQuery<NullSemanticsEntity1>(
                es => es.Where(e => e.NullableBoolA == e.NullableBoolB == (e.NullableIntA == e.NullableIntB)));
        }

        [ConditionalFact]
        public virtual void Compare_complex_equal_not_equal_equal()
        {
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => e.BoolA == e.BoolB != (e.IntA == e.IntB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => e.NullableBoolA == e.BoolB != (e.IntA == e.NullableIntB)));
            AssertQuery<NullSemanticsEntity1>(
                es => es.Where(e => e.NullableBoolA == e.NullableBoolB != (e.NullableIntA == e.NullableIntB)));
        }

        [ConditionalFact]
        public virtual void Compare_complex_not_equal_equal_equal()
        {
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => e.BoolA != e.BoolB == (e.IntA == e.IntB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => e.NullableBoolA != e.BoolB == (e.IntA == e.NullableIntB)));
            AssertQuery<NullSemanticsEntity1>(
                es => es.Where(e => e.NullableBoolA != e.NullableBoolB == (e.NullableIntA == e.NullableIntB)));
        }

        [ConditionalFact]
        public virtual void Compare_complex_not_equal_not_equal_equal()
        {
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => e.BoolA != e.BoolB != (e.IntA == e.IntB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => e.NullableBoolA != e.BoolB != (e.IntA == e.NullableIntB)));
            AssertQuery<NullSemanticsEntity1>(
                es => es.Where(e => e.NullableBoolA != e.NullableBoolB != (e.NullableIntA == e.NullableIntB)));
        }

        [ConditionalFact]
        public virtual void Compare_complex_not_equal_equal_not_equal()
        {
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => e.BoolA != e.BoolB == (e.IntA != e.IntB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => e.NullableBoolA != e.BoolB == (e.IntA != e.NullableIntB)));
            AssertQuery<NullSemanticsEntity1>(
                es => es.Where(e => e.NullableBoolA != e.NullableBoolB == (e.NullableIntA != e.NullableIntB)));
        }

        [ConditionalFact]
        public virtual void Compare_complex_not_equal_not_equal_not_equal()
        {
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => e.BoolA != e.BoolB != (e.IntA != e.IntB)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => e.NullableBoolA != e.BoolB != (e.IntA != e.NullableIntB)));
            AssertQuery<NullSemanticsEntity1>(
                es => es.Where(e => e.NullableBoolA != e.NullableBoolB != (e.NullableIntA != e.NullableIntB)));
        }

        [ConditionalFact]
        public virtual void Compare_nullable_with_null_parameter_equal()
        {
            string prm = null;

            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => e.NullableStringA == prm));
        }

        [ConditionalFact]
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
                            select new
                            {
                                Id1 = e1.Id,
                                Id2 = e2.Id,
                                e1.NullableIntA,
                                e2.NullableIntB
                            };

                query.ToList();
            }
        }

        [ConditionalFact]
        public virtual void Contains_with_local_array_closure_with_null()
        {
            string[] ids = { "Foo", null };
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => ids.Contains(e.NullableStringA)));
        }

        [ConditionalFact]
        public virtual void Contains_with_local_array_closure_with_multiple_nulls()
        {
            string[] ids = { null, "Foo", null, null };
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => ids.Contains(e.NullableStringA)));
        }

        [ConditionalFact]
        public virtual void Contains_with_local_array_closure_false_with_null()
        {
            string[] ids = { "Foo", null };
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !ids.Contains(e.NullableStringA)));
        }

        [ConditionalFact(Skip = "issue #14171")]
        public virtual void Contains_with_local_nullable_array_closure_negated()
        {
            string[] ids = { "Foo" };
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => !ids.Contains(e.NullableStringA)));
        }

        [ConditionalFact]
        public virtual void Where_multiple_ors_with_null()
        {
            AssertQuery<NullSemanticsEntity1>(
                es => es.Where(e => e.NullableStringA == "Foo" || e.NullableStringA == "Blah" || e.NullableStringA == null));
        }

        [ConditionalFact]
        public virtual void Where_multiple_ands_with_null()
        {
            AssertQuery<NullSemanticsEntity1>(
                es => es.Where(e => e.NullableStringA != "Foo" && e.NullableStringA != "Blah" && e.NullableStringA != null));
        }

        [ConditionalFact]
        public virtual void Where_multiple_ors_with_nullable_parameter()
        {
            string prm = null;
            AssertQuery<NullSemanticsEntity1>(
                es => es.Where(e => e.NullableStringA == "Foo" || e.NullableStringA == prm));
        }

        [ConditionalFact]
        public virtual void Where_multiple_ands_with_nullable_parameter_and_constant()
        {
            string prm1 = null;
            string prm2 = null;
            var prm3 = "Blah";

            AssertQuery<NullSemanticsEntity1>(
                es => es.Where(
                    e =>
                        e.NullableStringA != "Foo"
                        && e.NullableStringA != prm1
                        && e.NullableStringA != prm2
                        && e.NullableStringA != prm3));
        }

        [ConditionalFact]
        public virtual void Where_multiple_ands_with_nullable_parameter_and_constant_not_optimized()
        {
            string prm1 = null;
            string prm2 = null;
            var prm3 = "Blah";

            AssertQuery<NullSemanticsEntity1>(
                es => es.Where(
                    e =>
                        e.NullableStringB != null
                        && e.NullableStringA != "Foo"
                        && e.NullableStringA != prm1
                        && e.NullableStringA != prm2
                        && e.NullableStringA != prm3));
        }

        [ConditionalFact]
        public virtual void Where_coalesce()
        {
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => e.NullableBoolA ?? true));
        }

        [ConditionalFact]
        public virtual void Where_equal_nullable_with_null_value_parameter()
        {
            string prm = null;

            AssertQuery<NullSemanticsEntity1>(
                es => es.Where(
                    e => e.NullableStringA == prm));
        }

        [ConditionalFact]
        public virtual void Where_not_equal_nullable_with_null_value_parameter()
        {
            string prm = null;

            AssertQuery<NullSemanticsEntity1>(
                es => es.Where(
                    e => e.NullableStringA != prm));
        }

        [ConditionalFact]
        public virtual void Where_equal_with_coalesce()
        {
            AssertQuery<NullSemanticsEntity1>(
                es => es.Where(
                    e => (e.NullableStringA ?? e.NullableStringB) == e.NullableStringC));
        }

        [ConditionalFact]
        public virtual void Where_not_equal_with_coalesce()
        {
            AssertQuery<NullSemanticsEntity1>(
                es => es.Where(
                    e => (e.NullableStringA ?? e.NullableStringB) != e.NullableStringC));
        }

        [ConditionalFact]
        public virtual void Where_equal_with_coalesce_both_sides()
        {
            AssertQuery<NullSemanticsEntity1>(
                es => es.Where(
                    e => (e.NullableStringA ?? e.NullableStringB) == (e.StringA ?? e.StringB)));
        }

        [ConditionalFact]
        public virtual void Where_not_equal_with_coalesce_both_sides()
        {
            AssertQuery<NullSemanticsEntity1>(
                es => es.Where(
                    e => (e.NullableIntA ?? e.NullableIntB) != (e.NullableIntC ?? e.NullableIntB)));
        }

        [ConditionalFact]
        public virtual void Where_equal_with_conditional()
        {
            AssertQuery<NullSemanticsEntity1>(
                es => es.Where(
                    e => (e.NullableStringA == e.NullableStringB
                            ? e.NullableStringA
                            : e.NullableStringB)
                        == e.NullableStringC));
        }

        [ConditionalFact]
        public virtual void Where_not_equal_with_conditional()
        {
            AssertQuery<NullSemanticsEntity1>(
                es => es.Where(
                    e => e.NullableStringC
                        != (e.NullableStringA == e.NullableStringB
                            ? e.NullableStringA
                            : e.NullableStringB)));
        }

        [ConditionalFact]
        public virtual void Where_equal_with_conditional_non_nullable()
        {
            AssertQuery<NullSemanticsEntity1>(
                es => es.Where(
                    e => e.NullableStringC
                        != (e.NullableStringA == e.NullableStringB
                            ? e.StringA
                            : e.StringB)));
        }

        [ConditionalFact]
        public virtual void Where_conditional_search_condition_in_result()
        {
            var prm = true;
            var list = new[] { "Foo", "Bar" };

            AssertQuery<NullSemanticsEntity1>(
                es => es.Where(
                    e => prm ? list.Contains(e.StringA) : false));

            AssertQuery<NullSemanticsEntity1>(
                es => es.Where(
                    e => !prm ? true : e.StringA.StartsWith("A")));
        }

        [ConditionalFact]
        public virtual void Where_nested_conditional_search_condition_in_result()
        {
            var prm1 = true;
            var prm2 = false;
            var list = new[] { "Foo", "Bar" };

            AssertQuery<NullSemanticsEntity1>(
                es => es.Where(
                    e => prm1
                        ? (prm2
                            ? (e.BoolA
                                ? e.StringA.StartsWith("A")
                                : false)
                            : true)
                        : (e.BoolB ? list.Contains(e.StringA) : list.Contains(e.StringB))));
        }

        [ConditionalFact]
        public virtual void Where_equal_with_and_and_contains()
        {
            AssertQuery<NullSemanticsEntity1>(
                es => es.Where(e => e.NullableStringA.Contains(e.NullableStringB) && e.BoolA),
                es => es.Where(
                    e =>
                        e.NullableStringA != null && e.NullableStringA.Contains(e.NullableStringB ?? "Blah") && e.BoolA),
                useRelationalNulls: false);
        }

        [ConditionalFact]
        public virtual void Null_comparison_in_selector_with_relational_nulls()
        {
            using (var ctx = CreateContext(useRelationalNulls: true))
            {
                var query = ctx.Entities1.Select(e => e.NullableStringA != "Foo");
                var result = query.ToList();

                Assert.Equal(27, result.Count);
                Assert.Equal(9, result.Where(r => r).Count());
            }
        }

        [ConditionalFact]
        public virtual void Null_comparison_in_order_by_with_relational_nulls()
        {
            using (var ctx = CreateContext(useRelationalNulls: true))
            {
                var query = ctx.Entities1.OrderBy(e => e.NullableStringA != "Foo").ThenBy(e => e.NullableIntB != 10);
                var result = query.ToList();

                Assert.Equal(27, result.Count);
            }
        }

        [ConditionalFact(Skip = "issue #15743")]
        public virtual void Null_comparison_in_join_key_with_relational_nulls()
        {
            using (var ctx = CreateContext(useRelationalNulls: true))
            {
                var query = ctx.Entities1.Join(
                    ctx.Entities2, e1 => e1.NullableStringA != "Foo", e2 => e2.NullableBoolB != true, (o, i) => new { o, i });

                var result = query.ToList();
                Assert.Equal(162, result.Count);
            }
        }

        protected void AssertQuery<TItem>(
            Func<IQueryable<TItem>, IQueryable<TItem>> query,
            bool useDatabaseNullSemantics = false)
            where TItem : NullSemanticsEntityBase
            => AssertQuery(query, query, useDatabaseNullSemantics);

        [ConditionalFact]
        public virtual void Where_equal_using_relational_null_semantics()
        {
            using (var context = CreateContext(useRelationalNulls: true))
            {
                context.Entities1
                    .Where(e => e.NullableBoolA == e.NullableBoolB)
                    .Select(e => e.Id).ToList();
            }
        }

        [ConditionalFact]
        public virtual void Where_nullable_bool()
        {
            using (var context = CreateContext())
            {
                context.Entities1
                    .Where(e => e.NullableBoolA.Value)
                    .Select(e => e.Id).ToList();
            }
        }

        [ConditionalFact]
        public virtual void Where_nullable_bool_equal_with_constant()
        {
            using (var context = CreateContext())
            {
                context.Entities1
                    .Where(e => e.NullableBoolA == true)
                    .Select(e => e.Id).ToList();
            }
        }

        [ConditionalFact]
        public virtual void Where_nullable_bool_with_null_check()
        {
            using (var context = CreateContext())
            {
                context.Entities1
                    .Where(e => e.NullableBoolA != null && e.NullableBoolA.Value)
                    .Select(e => e.Id).ToList();
            }
        }

        [ConditionalFact]
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

        [ConditionalFact]
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

        [ConditionalFact]
        public virtual void Where_not_equal_using_relational_null_semantics()
        {
            using (var context = CreateContext(useRelationalNulls: true))
            {
                context.Entities1
                    .Where(e => e.NullableBoolA != e.NullableBoolB)
                    .Select(e => e.Id).ToList();
            }
        }

        [ConditionalFact]
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

        [ConditionalFact]
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

        [ConditionalFact]
        public virtual void Where_comparison_null_constant_and_null_parameter()
        {
            string prm = null;
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => prm == null));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => prm != null));
        }

        [ConditionalFact]
        public virtual void Where_comparison_null_constant_and_nonnull_parameter()
        {
            var prm = "Foo";
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => null == prm));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => null != prm));
        }

        [ConditionalFact]
        public virtual void Where_comparison_nonnull_constant_and_null_parameter()
        {
            string prm = null;
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => "Foo" == prm));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => "Foo" != prm));
        }

        [ConditionalFact]
        public virtual void Where_comparison_null_semantics_optimization_works_with_complex_predicates()
        {
            string prm = null;
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => null == prm && e.NullableStringA == prm));
        }

        [ConditionalFact]
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

        [ConditionalFact]
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

        [ConditionalFact]
        public virtual void From_sql_composed_with_relational_null_comparison()
        {
            using (var context = CreateContext(useRelationalNulls: true))
            {
                var actual = context.Entities1
                    .FromSqlRaw(NormalizeDelimitersInRawString("SELECT * FROM [Entities1]"))
                    .Where(c => c.StringA == c.StringB)
                    .ToArray();

                Assert.Equal(15, actual.Length);
            }
        }

        [ConditionalFact]
        public virtual void Projecting_nullable_bool_with_coalesce()
        {
            using (var context = CreateContext())
            {
                var expected = context.Entities1.ToList()
                    .Select(
                        e => new { e.Id, Coalesce = e.NullableBoolA ?? false });

                ClearLog();

                var query = context.Entities1
                    .Select(
                        e => new { e.Id, Coalesce = e.NullableBoolA ?? false });

                var results = query.ToList();
                Assert.Equal(expected.Count(), results.Count);
                foreach (var result in results)
                {
                    expected.Contains(result);
                }
            }
        }

        [ConditionalFact]
        public virtual void Projecting_nullable_bool_with_coalesce_nested()
        {
            using (var context = CreateContext())
            {
                var expected = context.Entities1.ToList()
                    .Select(
                        e => new { e.Id, Coalesce = e.NullableBoolA ?? (e.NullableBoolB ?? false) });

                ClearLog();

                var query = context.Entities1
                    .Select(
                        e => new { e.Id, Coalesce = e.NullableBoolA ?? (e.NullableBoolB ?? false) });

                var results = query.ToList();
                Assert.Equal(expected.Count(), results.Count);
                foreach (var result in results)
                {
                    expected.Contains(result);
                }
            }
        }

        [ConditionalFact]
        public virtual void Null_semantics_applied_when_comparing_function_with_nullable_argument_to_a_nullable_column()
        {
            AssertQuery<NullSemanticsEntity1>(
                es => es.Where(e => e.NullableStringA.IndexOf("oo") == e.NullableIntA),
                es => es.Where(
                    e => (e.NullableStringA == null && e.NullableIntA == null)
                        || (e.NullableStringA != null && e.NullableStringA.IndexOf("oo") == e.NullableIntA)),
                useRelationalNulls: false);

            AssertQuery<NullSemanticsEntity1>(
                es => es.Where(e => e.NullableStringA.IndexOf("ar") == e.NullableIntA),
                es => es.Where(
                    e => (e.NullableStringA == null && e.NullableIntA == null)
                        || (e.NullableStringA != null && e.NullableStringA.IndexOf("ar") == e.NullableIntA)),
                useRelationalNulls: false);

            AssertQuery<NullSemanticsEntity1>(
                es => es.Where(e => e.NullableStringA.IndexOf("oo") != e.NullableIntB),
                es => es.Where(
                    e => (e.NullableStringA == null && e.NullableIntB != null)
                        || (e.NullableStringA != null && e.NullableStringA.IndexOf("oo") != e.NullableIntB)),
                useRelationalNulls: false);
        }

        [ConditionalFact]
        public virtual void Null_semantics_applied_when_comparing_two_functions_with_nullable_arguments()
        {
            AssertQuery<NullSemanticsEntity1>(
                es => es.Where(e => e.NullableStringA.IndexOf("oo") == e.NullableStringB.IndexOf("ar")),
                es => es.Where(
                    e => MaybeScalar<int>(e.NullableStringA, () => e.NullableStringA.IndexOf("oo"))
                        == MaybeScalar<int>(
                            e.NullableStringB, () => e.NullableStringB.IndexOf("ar"))),
                useRelationalNulls: false);

            AssertQuery<NullSemanticsEntity1>(
                es => es.Where(e => e.NullableStringA.IndexOf("oo") != e.NullableStringB.IndexOf("ar")),
                es => es.Where(
                    e => MaybeScalar<int>(e.NullableStringA, () => e.NullableStringA.IndexOf("oo"))
                        != MaybeScalar<int>(
                            e.NullableStringB, () => e.NullableStringB.IndexOf("ar"))),
                useRelationalNulls: false);

            AssertQuery<NullSemanticsEntity1>(
                es => es.Where(e => e.NullableStringA.IndexOf("oo") != e.NullableStringA.IndexOf("ar")),
                es => es.Where(
                    e => MaybeScalar<int>(e.NullableStringA, () => e.NullableStringA.IndexOf("oo"))
                        != MaybeScalar<int>(
                            e.NullableStringA, () => e.NullableStringA.IndexOf("ar"))),
                useRelationalNulls: false);
        }

        [ConditionalFact]
        public virtual void Null_semantics_applied_when_comparing_two_functions_with_multiple_nullable_arguments()
        {
            AssertQuery<NullSemanticsEntity1>(
                es => es.Where(e => e.NullableStringA.Replace(e.NullableStringB, e.NullableStringC) == e.NullableStringA),
                es => es.Where(
                    e =>
                        (e.NullableStringA == null && (e.NullableStringA == null || e.NullableStringB == null || e.NullableStringC == null))
                        || (e.NullableStringA != null
                            && e.NullableStringB != null
                            && e.NullableStringC != null
                            && e.NullableStringA.Replace(e.NullableStringB, e.NullableStringC) == e.NullableStringA)),
                useRelationalNulls: false);

            AssertQuery<NullSemanticsEntity1>(
                es => es.Where(e => e.NullableStringA.Replace(e.NullableStringB, e.NullableStringC) != e.NullableStringA),
                es => es.Where(
                    e =>
                        ((e.NullableStringA == null || e.NullableStringB == null || e.NullableStringC == null) && e.NullableStringA != null)
                        || (e.NullableStringA != null
                            && e.NullableStringB != null
                            && e.NullableStringC != null
                            && e.NullableStringA.Replace(e.NullableStringB, e.NullableStringC) != e.NullableStringA)),
                useRelationalNulls: false);
        }

        [ConditionalFact]
        public virtual void Null_semantics_coalesce()
        {
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => e.NullableBoolA == (e.NullableBoolB ?? e.BoolC)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => e.NullableBoolA == (e.NullableBoolB ?? e.NullableBoolC)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => (e.NullableBoolB ?? e.BoolC) != e.NullableBoolA));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => (e.NullableBoolB ?? e.NullableBoolC) != e.NullableBoolA));
        }

        [ConditionalFact]
        public virtual void Null_semantics_conditional()
        {
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => e.BoolA == (e.BoolB ? e.NullableBoolB : e.NullableBoolC)));
            AssertQuery<NullSemanticsEntity1>(es => es.Where(e => (e.NullableBoolA != e.NullableBoolB ? e.BoolB : e.BoolC) == e.BoolA));
            AssertQuery<NullSemanticsEntity1>(
                es => es.Where(
                    e => (e.BoolA ? e.NullableBoolA != e.NullableBoolB : e.BoolC) != e.BoolB
                        ? e.BoolA
                        : e.NullableBoolB == e.NullableBoolC));
        }

        [ConditionalFact]
        public virtual void Null_semantics_function()
        {
            AssertQuery<NullSemanticsEntity1>(
                es => es.Where(e => e.NullableStringA.Substring(0, e.IntA) != e.NullableStringB),
                es => es.Where(e => Maybe(e.NullableIntA, () => e.NullableStringA.Substring(0, e.IntA)) != e.NullableStringB),
                useRelationalNulls: false);
        }

        [ConditionalFact]
        public virtual void Null_semantics_join_with_composite_key()
        {
            using (var ctx = CreateContext())
            {
                var query = from e1 in ctx.Entities1
                            join e2 in ctx.Entities2
                                on new
                                {
                                    one = e1.NullableStringA,
                                    two = e1.NullableStringB != e1.NullableStringC,
                                    three = true
                                }
                                equals new
                                {
                                    one = e2.NullableStringB,
                                    two = e2.NullableBoolA ?? e2.BoolC,
                                    three = true
                                }
                            select new { e1, e2 };

                var result = query.ToList();

                var expected = (from e1 in ctx.Entities1.ToList()
                                join e2 in ctx.Entities2.ToList()
                                    on new
                                    {
                                        one = e1.NullableStringA,
                                        two = e1.NullableStringB != e1.NullableStringC,
                                        three = true
                                    }
                                    equals new
                                    {
                                        one = e2.NullableStringB,
                                        two = e2.NullableBoolA ?? e2.BoolC,
                                        three = true
                                    }
                                select new { e1, e2 }).ToList();

                Assert.Equal(result.Count, expected.Count);
            }
        }

        [ConditionalFact(Skip = "issue #14171")]
        public virtual void Null_semantics_contains()
        {
            using (var ctx = CreateContext())
            {
                var ids = new List<int?> { 1, 2 };
                var query1 = ctx.Entities1.Where(e => ids.Contains(e.NullableIntA));
                var result1 = query1.ToList();

                var query2 = ctx.Entities1.Where(e => !ids.Contains(e.NullableIntA));
                var result2 = query2.ToList();

                var ids2 = new List<int?>
                {
                    1,
                    2,
                    null
                };
                var query3 = ctx.Entities1.Where(e => ids.Contains(e.NullableIntA));
                var result3 = query3.ToList();

                var query4 = ctx.Entities1.Where(e => !ids.Contains(e.NullableIntA));
                var result4 = query4.ToList();

                var query5 = ctx.Entities1.Where(e => !new List<int?> { 1, 2 }.Contains(e.NullableIntA));
                var result5 = query5.ToList();

                var query6 = ctx.Entities1.Where(
                    e => !new List<int?>
                    {
                        1,
                        2,
                        null
                    }.Contains(e.NullableIntA));
                var result6 = query6.ToList();
            }
        }

        [ConditionalFact]
        public virtual void Null_semantics_with_null_check_simple()
        {
            using (var ctx = CreateContext())
            {
                var query1 = ctx.Entities1.Where(e => e.NullableIntA != null && e.NullableIntA == e.NullableIntB);
                var result1 = query1.ToList();

                var query2 = ctx.Entities1.Where(e => e.NullableIntA != null && e.NullableIntA != e.NullableIntB);
                var result2 = query2.ToList();

                var query3 = ctx.Entities1.Where(e => e.NullableIntA != null && e.NullableIntA == e.IntC);
                var result3 = query3.ToList();

                var query4 = ctx.Entities1.Where(e => e.NullableIntA != null && e.NullableIntB != null && e.NullableIntA == e.NullableIntB);
                var result4 = query4.ToList();

                var query5 = ctx.Entities1.Where(e => e.NullableIntA != null && e.NullableIntB != null && e.NullableIntA != e.NullableIntB);
                var result5 = query5.ToList();
            }
        }

        [ConditionalFact]
        public virtual void Null_semantics_with_null_check_complex()
        {
            using (var ctx = CreateContext())
            {
                var query1 = ctx.Entities1.Where(
                    e => e.NullableIntA != null
                        && ((e.NullableIntC != e.NullableIntA)
                            || (e.NullableIntB != null && e.NullableIntA != e.NullableIntB)));
                var result1 = query1.ToList();

                var query2 = ctx.Entities1.Where(
                    e => e.NullableIntA != null && ((e.NullableIntC != e.NullableIntA) || (e.NullableIntA != e.NullableIntB)));
                var result2 = query2.ToList();

                var query3 = ctx.Entities1.Where(
                    e => (e.NullableIntA != null || e.NullableIntB != null) && e.NullableIntA == e.NullableIntC);
                var result3 = query3.ToList();
            }
        }

        [ConditionalFact]
        public virtual void IsNull_on_complex_expression()
        {
            using (var ctx = CreateContext())
            {
                var query1 = ctx.Entities1.Where(e => -e.NullableIntA != null).ToList();
                Assert.Equal(18, query1.Count);

                var query2 = ctx.Entities1.Where(e => (e.NullableIntA + e.NullableIntB) == null).ToList();
                Assert.Equal(15, query2.Count);

                var query3 = ctx.Entities1.Where(e => (e.NullableIntA ?? e.NullableIntB) == null).ToList();
                Assert.Equal(3, query3.Count);

                var query4 = ctx.Entities1.Where(e => (e.NullableIntA ?? e.NullableIntB) != null).ToList();
                Assert.Equal(24, query4.Count);
            }
        }

        protected static TResult Maybe<TResult>(object caller, Func<TResult> expression)
            where TResult : class
        {
            return caller == null ? null : expression();
        }

        protected static TResult? MaybeScalar<TResult>(object caller, Func<TResult?> expression)
            where TResult : struct
        {
            return caller == null ? null : expression();
        }

        private string NormalizeDelimitersInRawString(string sql)
            => Fixture.TestStore.NormalizeDelimitersInRawString(sql);

        private FormattableString NormalizeDelimetersInInterpolatedString(FormattableString sql)
            => Fixture.TestStore.NormalizeDelimitersInInterpolatedString(sql);

        protected abstract NullSemanticsContext CreateContext(bool useRelationalNulls = false);

        protected void AssertQuery<TItem>(
            Func<IQueryable<TItem>, IQueryable<TItem>> l2eQuery,
            Func<IQueryable<TItem>, IQueryable<TItem>> l2oQuery,
            bool useRelationalNulls)
            where TItem : NullSemanticsEntityBase
        {
            var actualIds = new List<int>();
            var expectedIds = new List<int>();

            expectedIds.AddRange(
                l2oQuery(_clientData.Set<TItem>().ToList().AsQueryable())
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

        protected virtual void ClearLog()
        {
        }
    }
}
