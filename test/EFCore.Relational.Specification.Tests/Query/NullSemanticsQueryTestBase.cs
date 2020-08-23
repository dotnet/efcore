// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestModels.NullSemanticsModel;
using Microsoft.EntityFrameworkCore.TestUtilities;
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
    public abstract class NullSemanticsQueryTestBase<TFixture> : QueryTestBase<TFixture>
        where TFixture : NullSemanticsQueryFixtureBase, new()
    {
        protected NullSemanticsQueryTestBase(TFixture fixture)
            : base(fixture)
        {
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Compare_bool_with_bool_equal(bool async)
        {
            await AssertQueryScalar(async, ss => ss.Set<NullSemanticsEntity1>().Where(e => e.BoolA == e.BoolB).Select(e => e.Id));
            await AssertQueryScalar(async, ss => ss.Set<NullSemanticsEntity1>().Where(e => e.BoolA == e.NullableBoolB).Select(e => e.Id));
            await AssertQueryScalar(async, ss => ss.Set<NullSemanticsEntity1>().Where(e => e.NullableBoolA == e.BoolB).Select(e => e.Id));
            await AssertQueryScalar(
                async, ss => ss.Set<NullSemanticsEntity1>().Where(e => e.NullableBoolA == e.NullableBoolB).Select(e => e.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Compare_negated_bool_with_bool_equal(bool async)
        {
            await AssertQueryScalar(async, ss => ss.Set<NullSemanticsEntity1>().Where(e => !e.BoolA == e.BoolB).Select(e => e.Id));
            await AssertQueryScalar(async, ss => ss.Set<NullSemanticsEntity1>().Where(e => !e.BoolA == e.NullableBoolB).Select(e => e.Id));
            await AssertQueryScalar(async, ss => ss.Set<NullSemanticsEntity1>().Where(e => !e.NullableBoolA == e.BoolB).Select(e => e.Id));
            await AssertQueryScalar(
                async, ss => ss.Set<NullSemanticsEntity1>().Where(e => !e.NullableBoolA == e.NullableBoolB).Select(e => e.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Compare_bool_with_negated_bool_equal(bool async)
        {
            await AssertQueryScalar(async, ss => ss.Set<NullSemanticsEntity1>().Where(e => e.BoolA == !e.BoolB).Select(e => e.Id));
            await AssertQueryScalar(async, ss => ss.Set<NullSemanticsEntity1>().Where(e => e.BoolA == !e.NullableBoolB).Select(e => e.Id));
            await AssertQueryScalar(async, ss => ss.Set<NullSemanticsEntity1>().Where(e => e.NullableBoolA == !e.BoolB).Select(e => e.Id));
            await AssertQueryScalar(
                async, ss => ss.Set<NullSemanticsEntity1>().Where(e => e.NullableBoolA == !e.NullableBoolB).Select(e => e.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Compare_negated_bool_with_negated_bool_equal(bool async)
        {
            await AssertQueryScalar(async, ss => ss.Set<NullSemanticsEntity1>().Where(e => !e.BoolA == !e.BoolB).Select(e => e.Id));
            await AssertQueryScalar(async, ss => ss.Set<NullSemanticsEntity1>().Where(e => !e.BoolA == !e.NullableBoolB).Select(e => e.Id));
            await AssertQueryScalar(async, ss => ss.Set<NullSemanticsEntity1>().Where(e => !e.NullableBoolA == !e.BoolB).Select(e => e.Id));
            await AssertQueryScalar(
                async, ss => ss.Set<NullSemanticsEntity1>().Where(e => !e.NullableBoolA == !e.NullableBoolB).Select(e => e.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Compare_bool_with_bool_equal_negated(bool async)
        {
            await AssertQueryScalar(async, ss => ss.Set<NullSemanticsEntity1>().Where(e => !(e.BoolA == e.BoolB)).Select(e => e.Id));
            await AssertQueryScalar(
                async, ss => ss.Set<NullSemanticsEntity1>().Where(e => !(e.BoolA == e.NullableBoolB)).Select(e => e.Id));
            await AssertQueryScalar(
                async, ss => ss.Set<NullSemanticsEntity1>().Where(e => !(e.NullableBoolA == e.BoolB)).Select(e => e.Id));
            await AssertQueryScalar(
                async, ss => ss.Set<NullSemanticsEntity1>().Where(e => !(e.NullableBoolA == e.NullableBoolB)).Select(e => e.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Compare_negated_bool_with_bool_equal_negated(bool async)
        {
            await AssertQueryScalar(async, ss => ss.Set<NullSemanticsEntity1>().Where(e => !(!e.BoolA == e.BoolB)).Select(e => e.Id));
            await AssertQueryScalar(
                async, ss => ss.Set<NullSemanticsEntity1>().Where(e => !(!e.BoolA == e.NullableBoolB)).Select(e => e.Id));
            await AssertQueryScalar(
                async, ss => ss.Set<NullSemanticsEntity1>().Where(e => !(!e.NullableBoolA == e.BoolB)).Select(e => e.Id));
            await AssertQueryScalar(
                async, ss => ss.Set<NullSemanticsEntity1>().Where(e => !(!e.NullableBoolA == e.NullableBoolB)).Select(e => e.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Compare_bool_with_negated_bool_equal_negated(bool async)
        {
            await AssertQueryScalar(async, ss => ss.Set<NullSemanticsEntity1>().Where(e => !(e.BoolA == !e.BoolB)).Select(e => e.Id));
            await AssertQueryScalar(
                async, ss => ss.Set<NullSemanticsEntity1>().Where(e => !(e.BoolA == !e.NullableBoolB)).Select(e => e.Id));
            await AssertQueryScalar(
                async, ss => ss.Set<NullSemanticsEntity1>().Where(e => !(e.NullableBoolA == !e.BoolB)).Select(e => e.Id));
            await AssertQueryScalar(
                async, ss => ss.Set<NullSemanticsEntity1>().Where(e => !(e.NullableBoolA == !e.NullableBoolB)).Select(e => e.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Compare_negated_bool_with_negated_bool_equal_negated(bool async)
        {
            await AssertQueryScalar(async, ss => ss.Set<NullSemanticsEntity1>().Where(e => !(!e.BoolA == !e.BoolB)).Select(e => e.Id));
            await AssertQueryScalar(
                async, ss => ss.Set<NullSemanticsEntity1>().Where(e => !(!e.BoolA == !e.NullableBoolB)).Select(e => e.Id));
            await AssertQueryScalar(
                async, ss => ss.Set<NullSemanticsEntity1>().Where(e => !(!e.NullableBoolA == !e.BoolB)).Select(e => e.Id));
            await AssertQueryScalar(
                async, ss => ss.Set<NullSemanticsEntity1>().Where(e => !(!e.NullableBoolA == !e.NullableBoolB)).Select(e => e.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Compare_bool_with_bool_not_equal(bool async)
        {
            await AssertQueryScalar(async, ss => ss.Set<NullSemanticsEntity1>().Where(e => e.BoolA != e.BoolB).Select(e => e.Id));
            await AssertQueryScalar(async, ss => ss.Set<NullSemanticsEntity1>().Where(e => e.BoolA != e.NullableBoolB).Select(e => e.Id));
            await AssertQueryScalar(async, ss => ss.Set<NullSemanticsEntity1>().Where(e => e.NullableBoolA != e.BoolB).Select(e => e.Id));
            await AssertQueryScalar(
                async, ss => ss.Set<NullSemanticsEntity1>().Where(e => e.NullableBoolA != e.NullableBoolB).Select(e => e.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Compare_negated_bool_with_bool_not_equal(bool async)
        {
            await AssertQueryScalar(async, ss => ss.Set<NullSemanticsEntity1>().Where(e => !e.BoolA != e.BoolB).Select(e => e.Id));
            await AssertQueryScalar(async, ss => ss.Set<NullSemanticsEntity1>().Where(e => !e.BoolA != e.NullableBoolB).Select(e => e.Id));
            await AssertQueryScalar(async, ss => ss.Set<NullSemanticsEntity1>().Where(e => !e.NullableBoolA != e.BoolB).Select(e => e.Id));
            await AssertQueryScalar(
                async, ss => ss.Set<NullSemanticsEntity1>().Where(e => !e.NullableBoolA != e.NullableBoolB).Select(e => e.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Compare_bool_with_negated_bool_not_equal(bool async)
        {
            await AssertQueryScalar(async, ss => ss.Set<NullSemanticsEntity1>().Where(e => e.BoolA != !e.BoolB).Select(e => e.Id));
            await AssertQueryScalar(async, ss => ss.Set<NullSemanticsEntity1>().Where(e => e.BoolA != !e.NullableBoolB).Select(e => e.Id));
            await AssertQueryScalar(async, ss => ss.Set<NullSemanticsEntity1>().Where(e => e.NullableBoolA != !e.BoolB).Select(e => e.Id));
            await AssertQueryScalar(
                async, ss => ss.Set<NullSemanticsEntity1>().Where(e => e.NullableBoolA != !e.NullableBoolB).Select(e => e.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Compare_negated_bool_with_negated_bool_not_equal(bool async)
        {
            await AssertQueryScalar(async, ss => ss.Set<NullSemanticsEntity1>().Where(e => !e.BoolA != !e.BoolB).Select(e => e.Id));
            await AssertQueryScalar(async, ss => ss.Set<NullSemanticsEntity1>().Where(e => !e.BoolA != !e.NullableBoolB).Select(e => e.Id));
            await AssertQueryScalar(async, ss => ss.Set<NullSemanticsEntity1>().Where(e => !e.NullableBoolA != !e.BoolB).Select(e => e.Id));
            await AssertQueryScalar(
                async, ss => ss.Set<NullSemanticsEntity1>().Where(e => !e.NullableBoolA != !e.NullableBoolB).Select(e => e.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Compare_bool_with_bool_not_equal_negated(bool async)
        {
            await AssertQueryScalar(async, ss => ss.Set<NullSemanticsEntity1>().Where(e => !(e.BoolA != e.BoolB)).Select(e => e.Id));
            await AssertQueryScalar(
                async, ss => ss.Set<NullSemanticsEntity1>().Where(e => !(e.BoolA != e.NullableBoolB)).Select(e => e.Id));
            await AssertQueryScalar(
                async, ss => ss.Set<NullSemanticsEntity1>().Where(e => !(e.NullableBoolA != e.BoolB)).Select(e => e.Id));
            await AssertQueryScalar(
                async, ss => ss.Set<NullSemanticsEntity1>().Where(e => !(e.NullableBoolA != e.NullableBoolB)).Select(e => e.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Compare_negated_bool_with_bool_not_equal_negated(bool async)
        {
            await AssertQueryScalar(async, ss => ss.Set<NullSemanticsEntity1>().Where(e => !(!e.BoolA != e.BoolB)).Select(e => e.Id));
            await AssertQueryScalar(
                async, ss => ss.Set<NullSemanticsEntity1>().Where(e => !(!e.BoolA != e.NullableBoolB)).Select(e => e.Id));
            await AssertQueryScalar(
                async, ss => ss.Set<NullSemanticsEntity1>().Where(e => !(!e.NullableBoolA != e.BoolB)).Select(e => e.Id));
            await AssertQueryScalar(
                async, ss => ss.Set<NullSemanticsEntity1>().Where(e => !(!e.NullableBoolA != e.NullableBoolB)).Select(e => e.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Compare_bool_with_negated_bool_not_equal_negated(bool async)
        {
            await AssertQueryScalar(async, ss => ss.Set<NullSemanticsEntity1>().Where(e => !(e.BoolA != !e.BoolB)).Select(e => e.Id));
            await AssertQueryScalar(
                async, ss => ss.Set<NullSemanticsEntity1>().Where(e => !(e.BoolA != !e.NullableBoolB)).Select(e => e.Id));
            await AssertQueryScalar(
                async, ss => ss.Set<NullSemanticsEntity1>().Where(e => !(e.NullableBoolA != !e.BoolB)).Select(e => e.Id));
            await AssertQueryScalar(
                async, ss => ss.Set<NullSemanticsEntity1>().Where(e => !(e.NullableBoolA != !e.NullableBoolB)).Select(e => e.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Compare_negated_bool_with_negated_bool_not_equal_negated(bool async)
        {
            await AssertQueryScalar(async, ss => ss.Set<NullSemanticsEntity1>().Where(e => !(!e.BoolA != !e.BoolB)).Select(e => e.Id));
            await AssertQueryScalar(
                async, ss => ss.Set<NullSemanticsEntity1>().Where(e => !(!e.BoolA != !e.NullableBoolB)).Select(e => e.Id));
            await AssertQueryScalar(
                async, ss => ss.Set<NullSemanticsEntity1>().Where(e => !(!e.NullableBoolA != !e.BoolB)).Select(e => e.Id));
            await AssertQueryScalar(
                async, ss => ss.Set<NullSemanticsEntity1>().Where(e => !(!e.NullableBoolA != !e.NullableBoolB)).Select(e => e.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Compare_equals_method(bool async)
        {
            await AssertQueryScalar(async, ss => ss.Set<NullSemanticsEntity1>().Where(e => e.BoolA.Equals(e.BoolB)).Select(e => e.Id));
            await AssertQueryScalar(
                async, ss => ss.Set<NullSemanticsEntity1>().Where(e => e.BoolA.Equals(e.NullableBoolB)).Select(e => e.Id));
            await AssertQueryScalar(
                async, ss => ss.Set<NullSemanticsEntity1>().Where(e => e.NullableBoolA.Equals(e.BoolB)).Select(e => e.Id));
            await AssertQueryScalar(
                async, ss => ss.Set<NullSemanticsEntity1>().Where(e => e.NullableBoolA.Equals(e.NullableBoolB)).Select(e => e.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Compare_equals_method_static(bool async)
        {
            await AssertQueryScalar(async, ss => ss.Set<NullSemanticsEntity1>().Where(e => Equals(e.BoolA, e.BoolB)).Select(e => e.Id));
            await AssertQueryScalar(
                async, ss => ss.Set<NullSemanticsEntity1>().Where(e => Equals(e.BoolA, e.NullableBoolB)).Select(e => e.Id));
            await AssertQueryScalar(
                async, ss => ss.Set<NullSemanticsEntity1>().Where(e => Equals(e.NullableBoolA, e.BoolB)).Select(e => e.Id));
            await AssertQueryScalar(
                async, ss => ss.Set<NullSemanticsEntity1>().Where(e => Equals(e.NullableBoolA, e.NullableBoolB)).Select(e => e.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Compare_equals_method_negated(bool async)
        {
            await AssertQueryScalar(async, ss => ss.Set<NullSemanticsEntity1>().Where(e => !e.BoolA.Equals(e.BoolB)).Select(e => e.Id));
            await AssertQueryScalar(
                async, ss => ss.Set<NullSemanticsEntity1>().Where(e => !e.BoolA.Equals(e.NullableBoolB)).Select(e => e.Id));
            await AssertQueryScalar(
                async, ss => ss.Set<NullSemanticsEntity1>().Where(e => !e.NullableBoolA.Equals(e.BoolB)).Select(e => e.Id));
            await AssertQueryScalar(
                async, ss => ss.Set<NullSemanticsEntity1>().Where(e => !e.NullableBoolA.Equals(e.NullableBoolB)).Select(e => e.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Compare_equals_method_negated_static(bool async)
        {
            await AssertQueryScalar(async, ss => ss.Set<NullSemanticsEntity1>().Where(e => !Equals(e.BoolA, e.BoolB)).Select(e => e.Id));
            await AssertQueryScalar(
                async, ss => ss.Set<NullSemanticsEntity1>().Where(e => !Equals(e.BoolA, e.NullableBoolB)).Select(e => e.Id));
            await AssertQueryScalar(
                async, ss => ss.Set<NullSemanticsEntity1>().Where(e => !Equals(e.NullableBoolA, e.BoolB)).Select(e => e.Id));
            await AssertQueryScalar(
                async, ss => ss.Set<NullSemanticsEntity1>().Where(e => !Equals(e.NullableBoolA, e.NullableBoolB)).Select(e => e.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Compare_complex_equal_equal_equal(bool async)
        {
            await AssertQueryScalar(
                async, ss => ss.Set<NullSemanticsEntity1>().Where(e => e.BoolA == e.BoolB == (e.IntA == e.IntB)).Select(e => e.Id));
            await AssertQueryScalar(
                async,
                ss => ss.Set<NullSemanticsEntity1>().Where(e => e.NullableBoolA == e.BoolB == (e.IntA == e.NullableIntB))
                    .Select(e => e.Id));
            await AssertQueryScalar(
                async,
                ss => ss.Set<NullSemanticsEntity1>().Where(e => e.NullableBoolA == e.NullableBoolB == (e.NullableIntA == e.NullableIntB))
                    .Select(e => e.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Compare_complex_equal_not_equal_equal(bool async)
        {
            await AssertQueryScalar(
                async, ss => ss.Set<NullSemanticsEntity1>().Where(e => e.BoolA == e.BoolB != (e.IntA == e.IntB)).Select(e => e.Id));
            await AssertQueryScalar(
                async,
                ss => ss.Set<NullSemanticsEntity1>().Where(e => e.NullableBoolA == e.BoolB != (e.IntA == e.NullableIntB))
                    .Select(e => e.Id));
            await AssertQueryScalar(
                async,
                ss => ss.Set<NullSemanticsEntity1>().Where(e => e.NullableBoolA == e.NullableBoolB != (e.NullableIntA == e.NullableIntB))
                    .Select(e => e.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Compare_complex_not_equal_equal_equal(bool async)
        {
            await AssertQueryScalar(
                async, ss => ss.Set<NullSemanticsEntity1>().Where(e => e.BoolA != e.BoolB == (e.IntA == e.IntB)).Select(e => e.Id));
            await AssertQueryScalar(
                async,
                ss => ss.Set<NullSemanticsEntity1>().Where(e => e.NullableBoolA != e.BoolB == (e.IntA == e.NullableIntB))
                    .Select(e => e.Id));
            await AssertQueryScalar(
                async,
                ss => ss.Set<NullSemanticsEntity1>().Where(e => e.NullableBoolA != e.NullableBoolB == (e.NullableIntA == e.NullableIntB))
                    .Select(e => e.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Compare_complex_not_equal_not_equal_equal(bool async)
        {
            await AssertQueryScalar(
                async, ss => ss.Set<NullSemanticsEntity1>().Where(e => e.BoolA != e.BoolB != (e.IntA == e.IntB)).Select(e => e.Id));
            await AssertQueryScalar(
                async,
                ss => ss.Set<NullSemanticsEntity1>().Where(e => e.NullableBoolA != e.BoolB != (e.IntA == e.NullableIntB))
                    .Select(e => e.Id));
            await AssertQueryScalar(
                async,
                ss => ss.Set<NullSemanticsEntity1>().Where(e => e.NullableBoolA != e.NullableBoolB != (e.NullableIntA == e.NullableIntB))
                    .Select(e => e.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Compare_complex_not_equal_equal_not_equal(bool async)
        {
            await AssertQueryScalar(
                async, ss => ss.Set<NullSemanticsEntity1>().Where(e => e.BoolA != e.BoolB == (e.IntA != e.IntB)).Select(e => e.Id));
            await AssertQueryScalar(
                async,
                ss => ss.Set<NullSemanticsEntity1>().Where(e => e.NullableBoolA != e.BoolB == (e.IntA != e.NullableIntB))
                    .Select(e => e.Id));
            await AssertQueryScalar(
                async,
                ss => ss.Set<NullSemanticsEntity1>().Where(e => e.NullableBoolA != e.NullableBoolB == (e.NullableIntA != e.NullableIntB))
                    .Select(e => e.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Compare_complex_not_equal_not_equal_not_equal(bool async)
        {
            await AssertQueryScalar(
                async, ss => ss.Set<NullSemanticsEntity1>().Where(e => e.BoolA != e.BoolB != (e.IntA != e.IntB)).Select(e => e.Id));
            await AssertQueryScalar(
                async,
                ss => ss.Set<NullSemanticsEntity1>().Where(e => e.NullableBoolA != e.BoolB != (e.IntA != e.NullableIntB))
                    .Select(e => e.Id));
            await AssertQueryScalar(
                async,
                ss => ss.Set<NullSemanticsEntity1>().Where(e => e.NullableBoolA != e.NullableBoolB != (e.NullableIntA != e.NullableIntB))
                    .Select(e => e.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Compare_nullable_with_null_parameter_equal(bool async)
        {
            string prm = null;

            return AssertQueryScalar(async, ss => ss.Set<NullSemanticsEntity1>().Where(e => e.NullableStringA == prm).Select(e => e.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Compare_nullable_with_non_null_parameter_not_equal(bool async)
        {
            var prm = "Foo";

            return AssertQueryScalar(async, ss => ss.Set<NullSemanticsEntity1>().Where(e => e.NullableStringA == prm).Select(e => e.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_uses_database_semantics(bool async)
        {
            return AssertQuery(
                async,
                ss => from e1 in ss.Set<NullSemanticsEntity1>()
                      join e2 in ss.Set<NullSemanticsEntity2>() on e1.NullableIntA equals e2.NullableIntB
                      select new
                      {
                          Id1 = e1.Id,
                          Id2 = e2.Id,
                          e1.NullableIntA,
                          e2.NullableIntB
                      },
                elementSorter: e => (e.Id1, e.Id2));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Contains_with_local_array_closure_with_null(bool async)
        {
            string[] ids = { "Foo", null };

            return AssertQueryScalar(
                async, ss => ss.Set<NullSemanticsEntity1>().Where(e => ids.Contains(e.NullableStringA)).Select(e => e.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Contains_with_local_array_closure_with_multiple_nulls(bool async)
        {
            string[] ids = { null, "Foo", null, null };

            return AssertQueryScalar(
                async, ss => ss.Set<NullSemanticsEntity1>().Where(e => ids.Contains(e.NullableStringA)).Select(e => e.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Contains_with_local_array_closure_false_with_null(bool async)
        {
            string[] ids = { "Foo", null };

            return AssertQueryScalar(
                async, ss => ss.Set<NullSemanticsEntity1>().Where(e => !ids.Contains(e.NullableStringA)).Select(e => e.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Contains_with_local_nullable_array_closure_negated(bool async)
        {
            string[] ids = { "Foo" };

            return AssertQueryScalar(
                async, ss => ss.Set<NullSemanticsEntity1>().Where(e => !ids.Contains(e.NullableStringA)).Select(e => e.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_multiple_ors_with_null(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<NullSemanticsEntity1>()
                    .Where(e => e.NullableStringA == "Foo" || e.NullableStringA == "Blah" || e.NullableStringA == null).Select(e => e.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_multiple_ands_with_null(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<NullSemanticsEntity1>()
                    .Where(e => e.NullableStringA != "Foo" && e.NullableStringA != "Blah" && e.NullableStringA != null).Select(e => e.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_multiple_ors_with_nullable_parameter(bool async)
        {
            string prm = null;

            return AssertQueryScalar(
                async,
                ss => ss.Set<NullSemanticsEntity1>().Where(e => e.NullableStringA == "Foo" || e.NullableStringA == prm).Select(e => e.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_multiple_ands_with_nullable_parameter_and_constant(bool async)
        {
            string prm1 = null;
            string prm2 = null;
            var prm3 = "Blah";

            return AssertQueryScalar(
                async,
                ss => ss.Set<NullSemanticsEntity1>().Where(
                    e => e.NullableStringA != "Foo"
                        && e.NullableStringA != prm1
                        && e.NullableStringA != prm2
                        && e.NullableStringA != prm3).Select(e => e.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_multiple_ands_with_nullable_parameter_and_constant_not_optimized(bool async)
        {
            string prm1 = null;
            string prm2 = null;
            var prm3 = "Blah";

            return AssertQueryScalar(
                async,
                ss => ss.Set<NullSemanticsEntity1>().Where(
                    e => e.NullableStringB != null
                        && e.NullableStringA != "Foo"
                        && e.NullableStringA != prm1
                        && e.NullableStringA != prm2
                        && e.NullableStringA != prm3).Select(e => e.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_coalesce(bool async)
        {
            return AssertQueryScalar(async, ss => ss.Set<NullSemanticsEntity1>().Where(e => e.NullableBoolA ?? true).Select(e => e.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_equal_nullable_with_null_value_parameter(bool async)
        {
            string prm = null;

            return AssertQueryScalar(async, ss => ss.Set<NullSemanticsEntity1>().Where(e => e.NullableStringA == prm).Select(e => e.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_not_equal_nullable_with_null_value_parameter(bool async)
        {
            string prm = null;

            return AssertQueryScalar(async, ss => ss.Set<NullSemanticsEntity1>().Where(e => e.NullableStringA != prm).Select(e => e.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_equal_with_coalesce(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<NullSemanticsEntity1>().Where(e => (e.NullableStringA ?? e.NullableStringB) == e.NullableStringC)
                    .Select(e => e.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_not_equal_with_coalesce(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<NullSemanticsEntity1>().Where(e => (e.NullableStringA ?? e.NullableStringB) != e.NullableStringC)
                    .Select(e => e.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_equal_with_coalesce_both_sides(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<NullSemanticsEntity1>().Where(e => (e.NullableStringA ?? e.NullableStringB) == (e.StringA ?? e.StringB))
                    .Select(e => e.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_not_equal_with_coalesce_both_sides(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<NullSemanticsEntity1>().Where(e => (e.NullableIntA ?? e.NullableIntB) != (e.NullableIntC ?? e.NullableIntB))
                    .Select(e => e.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_equal_with_conditional(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<NullSemanticsEntity1>().Where(
                    e => (e.NullableStringA == e.NullableStringB
                            ? e.NullableStringA
                            : e.NullableStringB)
                        == e.NullableStringC).Select(e => e.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_not_equal_with_conditional(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<NullSemanticsEntity1>().Where(
                    e => e.NullableStringC
                        != (e.NullableStringA == e.NullableStringB
                            ? e.NullableStringA
                            : e.NullableStringB)).Select(e => e.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_equal_with_conditional_non_nullable(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<NullSemanticsEntity1>().Where(
                    e => e.NullableStringC
                        != (e.NullableStringA == e.NullableStringB
                            ? e.StringA
                            : e.StringB)).Select(e => e.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Where_conditional_search_condition_in_result(bool async)
        {
            var prm = true;
            var list = new[] { "Foo", "Bar" };

            await AssertQueryScalar(
                async,
                ss => ss.Set<NullSemanticsEntity1>().Where(e => prm ? list.Contains(e.StringA) : false).Select(e => e.Id));

            await AssertQueryScalar(
                async,
                ss => ss.Set<NullSemanticsEntity1>().Where(e => !prm ? true : e.StringA.StartsWith("A")).Select(e => e.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_nested_conditional_search_condition_in_result(bool async)
        {
            var prm1 = true;
            var prm2 = false;
            var list = new[] { "Foo", "Bar" };

            return AssertQueryScalar(
                async,
                ss => ss.Set<NullSemanticsEntity1>().Where(
                    e => prm1
                        ? (prm2
                            ? (e.BoolA
                                ? e.StringA.StartsWith("A")
                                : false)
                            : true)
                        : (e.BoolB ? list.Contains(e.StringA) : list.Contains(e.StringB))).Select(e => e.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_equal_with_and_and_contains(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<NullSemanticsEntity1>().Where(e => e.NullableStringA.Contains(e.NullableStringB) && e.BoolA).Select(e => e.Id),
                ss => ss.Set<NullSemanticsEntity1>()
                    .Where(e => e.NullableStringA != null && e.NullableStringA.Contains(e.NullableStringB ?? "Blah") && e.BoolA)
                    .Select(e => e.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Null_comparison_in_selector_with_relational_nulls(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<NullSemanticsEntity1>().Select(e => e.NullableStringA != "Foo"));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Null_comparison_in_order_by_with_relational_nulls(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<NullSemanticsEntity1>().OrderBy(e => e.NullableStringA != "Foo").ThenBy(e => e.NullableIntB != 10)
                    .Select(e => e),
                elementSorter: e => e.Id);
        }

        [ConditionalTheory(Skip = "issue #15743")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Null_comparison_in_join_key_with_relational_nulls(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<NullSemanticsEntity1>().Join(
                    ss.Set<NullSemanticsEntity2>(),
                    e1 => e1.NullableStringA != "Foo",
                    e2 => e2.NullableBoolB != true,
                    (o, i) => new { o, i }));
        }

        [ConditionalFact]
        public virtual void Where_equal_using_relational_null_semantics()
        {
            using var context = CreateContext(useRelationalNulls: true);
            context.Entities1
                .Where(e => e.NullableBoolA == e.NullableBoolB)
                .Select(e => e.Id).ToList();
        }

        [ConditionalFact]
        public virtual void Where_contains_on_parameter_array_with_relational_null_semantics()
        {
            using var context = CreateContext(useRelationalNulls: true);
            var names = new[] { "Foo", "Bar" };
            var result = context.Entities1
                .Where(e => names.Contains(e.NullableStringA))
                .Select(e => e.NullableStringA).ToList();

            Assert.True(result.All(r => r == "Foo" || r == "Bar"));
        }

        [ConditionalFact]
        public virtual void Where_contains_on_parameter_empty_array_with_relational_null_semantics()
        {
            using var context = CreateContext(useRelationalNulls: true);
            var names = new string[0];
            var result = context.Entities1
                .Where(e => names.Contains(e.NullableStringA))
                .Select(e => e.NullableStringA).ToList();

            Assert.Equal(0, result.Count);
        }

        [ConditionalFact]
        public virtual void Where_contains_on_parameter_array_with_just_null_with_relational_null_semantics()
        {
            using var context = CreateContext(useRelationalNulls: true);
            var names = new string[] { null };
            var result = context.Entities1
                .Where(e => names.Contains(e.NullableStringA))
                .Select(e => e.NullableStringA).ToList();

            Assert.Equal(0, result.Count);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_nullable_bool(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<NullSemanticsEntity1>().Where(e => e.NullableBoolA.Value).Select(e => e.Id),
                ss => ss.Set<NullSemanticsEntity1>().Where(e => e.NullableBoolA == true).Select(e => e.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_nullable_bool_equal_with_constant(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<NullSemanticsEntity1>().Where(e => e.NullableBoolA == true).Select(e => e.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_nullable_bool_with_null_check(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<NullSemanticsEntity1>().Where(e => e.NullableBoolA != null && e.NullableBoolA.Value).Select(e => e.Id));
        }

        [ConditionalFact]
        public virtual void Where_equal_using_relational_null_semantics_with_parameter()
        {
            using var context = CreateContext(useRelationalNulls: true);
            bool? prm = null;
            context.Entities1
                .Where(e => e.NullableBoolA == prm)
                .Select(e => e.Id).ToList();
        }

        [ConditionalFact]
        public virtual void Where_equal_using_relational_null_semantics_complex_with_parameter()
        {
            using var context = CreateContext(useRelationalNulls: true);
            var prm = false;
            context.Entities1
                .Where(e => e.NullableBoolA == e.NullableBoolB || prm)
                .Select(e => e.Id).ToList();
        }

        [ConditionalFact]
        public virtual void Where_not_equal_using_relational_null_semantics()
        {
            using var context = CreateContext(useRelationalNulls: true);
            context.Entities1
                .Where(e => e.NullableBoolA != e.NullableBoolB)
                .Select(e => e.Id).ToList();
        }

        [ConditionalFact]
        public virtual void Where_not_equal_using_relational_null_semantics_with_parameter()
        {
            using var context = CreateContext(useRelationalNulls: true);
            bool? prm = null;
            context.Entities1
                .Where(e => e.NullableBoolA != prm)
                .Select(e => e.Id).ToList();
        }

        [ConditionalFact]
        public virtual void Where_not_equal_using_relational_null_semantics_complex_with_parameter()
        {
            using var context = CreateContext(useRelationalNulls: true);
            var prm = false;
            context.Entities1
                .Where(e => e.NullableBoolA != e.NullableBoolB || prm)
                .Select(e => e.Id).ToList();
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Where_comparison_null_constant_and_null_parameter(bool async)
        {
            string prm = null;

            await AssertQueryScalar(async, ss => ss.Set<NullSemanticsEntity1>().Where(e => prm == null).Select(e => e.Id));
            await AssertQueryScalar(async, ss => ss.Set<NullSemanticsEntity1>().Where(e => prm != null).Select(e => e.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Where_comparison_null_constant_and_nonnull_parameter(bool async)
        {
            var prm = "Foo";

            await AssertQueryScalar(async, ss => ss.Set<NullSemanticsEntity1>().Where(e => null == prm).Select(e => e.Id));
            await AssertQueryScalar(async, ss => ss.Set<NullSemanticsEntity1>().Where(e => null != prm).Select(e => e.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Where_comparison_nonnull_constant_and_null_parameter(bool async)
        {
            string prm = null;

            await AssertQueryScalar(async, ss => ss.Set<NullSemanticsEntity1>().Where(e => "Foo" == prm).Select(e => e.Id));
            await AssertQueryScalar(async, ss => ss.Set<NullSemanticsEntity1>().Where(e => "Foo" != prm).Select(e => e.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_comparison_null_semantics_optimization_works_with_complex_predicates(bool async)
        {
            string prm = null;

            return AssertQueryScalar(
                async, ss => ss.Set<NullSemanticsEntity1>().Where(e => null == prm && e.NullableStringA == prm).Select(e => e.Id));
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
            using var context = CreateContext();
            var prm = "Foo";
            var query = context.Entities1
                .Where(e => prm == "Foo")
                .Select(e => e.Id);

            var results1 = query.ToList();

            prm = null;

            var results2 = query.ToList();

            Assert.True(results1.Count != results2.Count);
        }

        [ConditionalFact]
        public virtual void From_sql_composed_with_relational_null_comparison()
        {
            using var context = CreateContext(useRelationalNulls: true);
            var actual = context.Entities1
                .FromSqlRaw(NormalizeDelimitersInRawString("SELECT * FROM [Entities1]"))
                .Where(c => c.StringA == c.StringB)
                .ToArray();

            Assert.Equal(15, actual.Length);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Projecting_nullable_bool_with_coalesce(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<NullSemanticsEntity1>().Select(e => new { e.Id, Coalesce = e.NullableBoolA ?? false }),
                elementSorter: e => e.Id,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Id, a.Id);
                    Assert.Equal(e.Coalesce, a.Coalesce);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Projecting_nullable_bool_with_coalesce_nested(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<NullSemanticsEntity1>().Select(e => new { e.Id, Coalesce = e.NullableBoolA ?? (e.NullableBoolB ?? false) }),
                elementSorter: e => e.Id,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Id, a.Id);
                    Assert.Equal(e.Coalesce, a.Coalesce);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Null_semantics_applied_when_comparing_function_with_nullable_argument_to_a_nullable_column(bool async)
        {
            await AssertQueryScalar(
                async,
                ss => ss.Set<NullSemanticsEntity1>().Where(e => e.NullableStringA.IndexOf("oo") == e.NullableIntA).Select(e => e.Id),
                ss => ss.Set<NullSemanticsEntity1>().Where(
                    e => (e.NullableStringA == null && e.NullableIntA == null)
                        || (e.NullableStringA != null && e.NullableStringA.IndexOf("oo") == e.NullableIntA)).Select(e => e.Id));

            await AssertQueryScalar(
                async,
                ss => ss.Set<NullSemanticsEntity1>().Where(e => e.NullableStringA.IndexOf("ar") == e.NullableIntA).Select(e => e.Id),
                ss => ss.Set<NullSemanticsEntity1>().Where(
                    e => (e.NullableStringA == null && e.NullableIntA == null)
                        || (e.NullableStringA != null && e.NullableStringA.IndexOf("ar") == e.NullableIntA)).Select(e => e.Id));

            await AssertQueryScalar(
                async,
                ss => ss.Set<NullSemanticsEntity1>().Where(e => e.NullableStringA.IndexOf("oo") != e.NullableIntB).Select(e => e.Id),
                ss => ss.Set<NullSemanticsEntity1>().Where(
                    e => (e.NullableStringA == null && e.NullableIntB != null)
                        || (e.NullableStringA != null && e.NullableStringA.IndexOf("oo") != e.NullableIntB)).Select(e => e.Id));
        }

        [ConditionalTheory(Skip = "Issue #18773")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_IndexOf_empty(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<NullSemanticsEntity1>().Where(e => e.NullableStringA.IndexOf("") == e.NullableIntA).Select(e => e.Id),
                ss => ss.Set<NullSemanticsEntity1>().Where(e => 0 == e.NullableIntA).Select(e => e.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_IndexOf(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<NullSemanticsEntity1>().OrderBy(e => e.Id).Select(e => (int?)e.NullableStringA.IndexOf("oo")),
                ss => ss.Set<NullSemanticsEntity1>().OrderBy(e => e.Id).Select(e => e.NullableStringA.MaybeScalar(x => x.IndexOf("oo"))),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Null_semantics_applied_when_comparing_two_functions_with_nullable_arguments(bool async)
        {
            await AssertQueryScalar(
                async,
                ss => ss.Set<NullSemanticsEntity1>().Where(e => e.NullableStringA.IndexOf("oo") == e.NullableStringB.IndexOf("ar"))
                    .Select(e => e.Id),
                ss => ss.Set<NullSemanticsEntity1>().Where(
                    e => e.NullableStringA.MaybeScalar(x => x.IndexOf("oo"))
                        == e.NullableStringB.MaybeScalar(x => x.IndexOf("ar"))).Select(e => e.Id));

            await AssertQueryScalar(
                async,
                ss => ss.Set<NullSemanticsEntity1>().Where(e => e.NullableStringA.IndexOf("oo") != e.NullableStringB.IndexOf("ar"))
                    .Select(e => e.Id),
                ss => ss.Set<NullSemanticsEntity1>().Where(
                    e => e.NullableStringA.MaybeScalar(x => x.IndexOf("oo"))
                        != e.NullableStringB.MaybeScalar(x => x.IndexOf("ar"))).Select(e => e.Id));

            await AssertQueryScalar(
                async,
                ss => ss.Set<NullSemanticsEntity1>().Where(e => e.NullableStringA.IndexOf("oo") != e.NullableStringA.IndexOf("ar"))
                    .Select(e => e.Id),
                ss => ss.Set<NullSemanticsEntity1>().Where(
                    e => e.NullableStringA.MaybeScalar(x => x.IndexOf("oo"))
                        != e.NullableStringA.MaybeScalar(x => x.IndexOf("ar"))).Select(e => e.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Null_semantics_applied_when_comparing_two_functions_with_multiple_nullable_arguments(bool async)
        {
            await AssertQueryScalar(
                async,
                ss => ss.Set<NullSemanticsEntity1>()
                    .Where(e => e.NullableStringA.Replace(e.NullableStringB, e.NullableStringC) == e.NullableStringA).Select(e => e.Id),
                ss => ss.Set<NullSemanticsEntity1>().Where(
                    e =>
                        (e.NullableStringA == null && (e.NullableStringA == null || e.NullableStringB == null || e.NullableStringC == null))
                        || (e.NullableStringA != null
                            && e.NullableStringB != null
                            && e.NullableStringC != null
                            && e.NullableStringA.Replace(e.NullableStringB, e.NullableStringC) == e.NullableStringA)).Select(e => e.Id));

            await AssertQueryScalar(
                async,
                ss => ss.Set<NullSemanticsEntity1>()
                    .Where(e => e.NullableStringA.Replace(e.NullableStringB, e.NullableStringC) != e.NullableStringA).Select(e => e.Id),
                ss => ss.Set<NullSemanticsEntity1>().Where(
                    e =>
                        ((e.NullableStringA == null || e.NullableStringB == null || e.NullableStringC == null) && e.NullableStringA != null)
                        || (e.NullableStringA != null
                            && e.NullableStringB != null
                            && e.NullableStringC != null
                            && e.NullableStringA.Replace(e.NullableStringB, e.NullableStringC) != e.NullableStringA)).Select(e => e.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Null_semantics_coalesce(bool async)
        {
            await AssertQueryScalar(
                async, ss => ss.Set<NullSemanticsEntity1>().Where(e => e.NullableBoolA == (e.NullableBoolB ?? e.BoolC)).Select(e => e.Id));
            await AssertQueryScalar(
                async,
                ss => ss.Set<NullSemanticsEntity1>().Where(e => e.NullableBoolA == (e.NullableBoolB ?? e.NullableBoolC)).Select(e => e.Id));
            await AssertQueryScalar(
                async, ss => ss.Set<NullSemanticsEntity1>().Where(e => (e.NullableBoolB ?? e.BoolC) != e.NullableBoolA).Select(e => e.Id));
            await AssertQueryScalar(
                async,
                ss => ss.Set<NullSemanticsEntity1>().Where(e => (e.NullableBoolB ?? e.NullableBoolC) != e.NullableBoolA).Select(e => e.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Null_semantics_conditional(bool async)
        {
            await AssertQueryScalar(
                async,
                ss => ss.Set<NullSemanticsEntity1>().Where(e => e.BoolA == (e.BoolB ? e.NullableBoolB : e.NullableBoolC))
                    .Select(e => e.Id));
            await AssertQueryScalar(
                async,
                ss => ss.Set<NullSemanticsEntity1>().Where(e => (e.NullableBoolA != e.NullableBoolB ? e.BoolB : e.BoolC) == e.BoolA)
                    .Select(e => e.Id));
            await AssertQueryScalar(
                async,
                ss => ss.Set<NullSemanticsEntity1>().Where(
                    e => (e.BoolA ? e.NullableBoolA != e.NullableBoolB : e.BoolC) != e.BoolB
                        ? e.BoolA
                        : e.NullableBoolB == e.NullableBoolC).Select(e => e.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Null_semantics_function(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<NullSemanticsEntity1>().Where(e => e.NullableStringA.Substring(0, e.IntA) != e.NullableStringB)
                    .Select(e => e.Id),
                ss => ss.Set<NullSemanticsEntity1>().Where(e => e.NullableStringA.Maybe(x => x.Substring(0, e.IntA)) != e.NullableStringB)
                    .Select(e => e.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Null_semantics_join_with_composite_key(bool async)
        {
            return AssertQuery(
                async,
                ss => from e1 in ss.Set<NullSemanticsEntity1>()
                      join e2 in ss.Set<NullSemanticsEntity2>()
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
                      select new { e1, e2 },
                elementSorter: e => (e.e1.Id, e.e2.Id),
                elementAsserter: (e, a) =>
                {
                    AssertEqual(e.e1, a.e1);
                    AssertEqual(e.e2, a.e2);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Null_semantics_contains(bool async)
        {
            var ids = new List<int?> { 1, 2 };
            await AssertQueryScalar(async, ss => ss.Set<NullSemanticsEntity1>().Where(e => ids.Contains(e.NullableIntA)).Select(e => e.Id));
            await AssertQueryScalar(
                async, ss => ss.Set<NullSemanticsEntity1>().Where(e => !ids.Contains(e.NullableIntA)).Select(e => e.Id));

            var ids2 = new List<int?>
            {
                1,
                2,
                null
            };
            await AssertQueryScalar(
                async, ss => ss.Set<NullSemanticsEntity1>().Where(e => ids2.Contains(e.NullableIntA)).Select(e => e.Id));
            await AssertQueryScalar(
                async, ss => ss.Set<NullSemanticsEntity1>().Where(e => !ids2.Contains(e.NullableIntA)).Select(e => e.Id));

            await AssertQueryScalar(
                async, ss => ss.Set<NullSemanticsEntity1>().Where(e => new List<int?> { 1, 2 }.Contains(e.NullableIntA)).Select(e => e.Id));
            await AssertQueryScalar(
                async,
                ss => ss.Set<NullSemanticsEntity1>().Where(e => !new List<int?> { 1, 2 }.Contains(e.NullableIntA)).Select(e => e.Id));
            await AssertQueryScalar(
                async, ss => ss.Set<NullSemanticsEntity1>().Where(
                    e => new List<int?>
                    {
                        1,
                        2,
                        null
                    }.Contains(e.NullableIntA)).Select(e => e.Id));
            await AssertQueryScalar(
                async, ss => ss.Set<NullSemanticsEntity1>().Where(
                    e => !new List<int?>
                    {
                        1,
                        2,
                        null
                    }.Contains(e.NullableIntA)).Select(e => e.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Null_semantics_contains_array_with_no_values(bool async)
        {
            var ids = new List<int?>();
            await AssertQueryScalar(async, ss => ss.Set<NullSemanticsEntity1>().Where(e => ids.Contains(e.NullableIntA)).Select(e => e.Id));
            await AssertQueryScalar(
                async, ss => ss.Set<NullSemanticsEntity1>().Where(e => !ids.Contains(e.NullableIntA)).Select(e => e.Id));

            var ids2 = new List<int?> { null };
            await AssertQueryScalar(
                async, ss => ss.Set<NullSemanticsEntity1>().Where(e => ids2.Contains(e.NullableIntA)).Select(e => e.Id));
            await AssertQueryScalar(
                async, ss => ss.Set<NullSemanticsEntity1>().Where(e => !ids2.Contains(e.NullableIntA)).Select(e => e.Id));

            await AssertQueryScalar(
                async, ss => ss.Set<NullSemanticsEntity1>().Where(e => new List<int?>().Contains(e.NullableIntA)).Select(e => e.Id));
            await AssertQueryScalar(
                async, ss => ss.Set<NullSemanticsEntity1>().Where(e => !new List<int?>().Contains(e.NullableIntA)).Select(e => e.Id));
            await AssertQueryScalar(
                async, ss => ss.Set<NullSemanticsEntity1>().Where(e => new List<int?> { null }.Contains(e.NullableIntA)).Select(e => e.Id));
            await AssertQueryScalar(
                async,
                ss => ss.Set<NullSemanticsEntity1>().Where(e => !new List<int?> { null }.Contains(e.NullableIntA)).Select(e => e.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Null_semantics_contains_non_nullable_argument(bool async)
        {
            var ids = new List<int?>
            {
                1,
                2,
                null
            };
            await AssertQueryScalar(async, ss => ss.Set<NullSemanticsEntity1>().Where(e => ids.Contains(e.IntA)).Select(e => e.Id));
            await AssertQueryScalar(async, ss => ss.Set<NullSemanticsEntity1>().Where(e => !ids.Contains(e.IntA)).Select(e => e.Id));

            var ids2 = new List<int?>
            {
                1, 2,
            };
            await AssertQueryScalar(async, ss => ss.Set<NullSemanticsEntity1>().Where(e => ids2.Contains(e.IntA)).Select(e => e.Id));
            await AssertQueryScalar(async, ss => ss.Set<NullSemanticsEntity1>().Where(e => !ids2.Contains(e.IntA)).Select(e => e.Id));

            var ids3 = new List<int?>();
            await AssertQueryScalar(async, ss => ss.Set<NullSemanticsEntity1>().Where(e => ids3.Contains(e.IntA)).Select(e => e.Id));
            await AssertQueryScalar(async, ss => ss.Set<NullSemanticsEntity1>().Where(e => !ids3.Contains(e.IntA)).Select(e => e.Id));

            var ids4 = new List<int?> { null };
            await AssertQueryScalar(async, ss => ss.Set<NullSemanticsEntity1>().Where(e => ids4.Contains(e.IntA)).Select(e => e.Id));
            await AssertQueryScalar(async, ss => ss.Set<NullSemanticsEntity1>().Where(e => !ids4.Contains(e.IntA)).Select(e => e.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Null_semantics_with_null_check_simple(bool async)
        {
            await AssertQueryScalar(
                async,
                ss => ss.Set<NullSemanticsEntity1>().Where(e => e.NullableIntA != null && e.NullableIntA == e.NullableIntB)
                    .Select(e => e.Id));

            await AssertQueryScalar(
                async,
                ss => ss.Set<NullSemanticsEntity1>().Where(e => e.NullableIntA != null && e.NullableIntA != e.NullableIntB)
                    .Select(e => e.Id));

            await AssertQueryScalar(
                async,
                ss => ss.Set<NullSemanticsEntity1>().Where(e => e.NullableIntA != null && e.NullableIntA == e.IntC).Select(e => e.Id));

            await AssertQueryScalar(
                async,
                ss => ss.Set<NullSemanticsEntity1>()
                    .Where(e => e.NullableIntA != null && e.NullableIntB != null && e.NullableIntA == e.NullableIntB).Select(e => e.Id));

            await AssertQueryScalar(
                async,
                ss => ss.Set<NullSemanticsEntity1>()
                    .Where(e => e.NullableIntA != null && e.NullableIntB != null && e.NullableIntA != e.NullableIntB).Select(e => e.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Null_semantics_with_null_check_complex(bool async)
        {
            await AssertQueryScalar(
                async,
                ss => ss.Set<NullSemanticsEntity1>()
                    .Where(
                        e => e.NullableIntA != null
                            && ((e.NullableIntC != e.NullableIntA)
                                || (e.NullableIntB != null && e.NullableIntA != e.NullableIntB))).Select(e => e.Id));

            await AssertQueryScalar(
                async,
                ss => ss.Set<NullSemanticsEntity1>()
                    .Where(e => e.NullableIntA != null && ((e.NullableIntC != e.NullableIntA) || (e.NullableIntA != e.NullableIntB)))
                    .Select(e => e.Id));

            await AssertQueryScalar(
                async,
                ss => ss.Set<NullSemanticsEntity1>()
                    .Where(e => (e.NullableIntA != null || e.NullableIntB != null) && e.NullableIntA == e.NullableIntC).Select(e => e.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Null_semantics_with_null_check_complex2(bool async)
        {
            await AssertQueryScalar(
                async,
                ss => ss.Set<NullSemanticsEntity1>()
                    .Where(
                        e => ((e.NullableBoolA != null)
                                && (e.NullableBoolB != null)
                                && ((e.NullableBoolB != e.NullableBoolA) || (e.NullableBoolC != null))
                                && (e.NullableBoolC != e.NullableBoolB))
                            || (e.NullableBoolC != e.BoolB)).Select(e => e.Id));

            await AssertQueryScalar(
                async,
                ss => ss.Set<NullSemanticsEntity1>()
                    .Where(
                        e => ((e.NullableBoolA != null)
                                && (e.NullableBoolB != null)
                                && ((e.NullableBoolB != e.NullableBoolA) || (e.NullableBoolC != null))
                                && (e.NullableBoolC != e.NullableBoolB))
                            || (e.NullableBoolB != e.BoolB)).Select(e => e.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task IsNull_on_complex_expression(bool async)
        {
            await AssertQueryScalar(async, ss => ss.Set<NullSemanticsEntity1>().Where(e => -e.NullableIntA != null).Select(e => e.Id));
            await AssertQueryScalar(
                async, ss => ss.Set<NullSemanticsEntity1>().Where(e => (e.NullableIntA + e.NullableIntB) == null).Select(e => e.Id));
            await AssertQueryScalar(
                async, ss => ss.Set<NullSemanticsEntity1>().Where(e => (e.NullableIntA ?? e.NullableIntB) == null).Select(e => e.Id));
            await AssertQueryScalar(
                async, ss => ss.Set<NullSemanticsEntity1>().Where(e => (e.NullableIntA ?? e.NullableIntB) != null).Select(e => e.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Coalesce_not_equal(bool async)
        {
            return AssertQueryScalar(async, ss => ss.Set<NullSemanticsEntity1>().Where(e => (e.NullableIntA ?? 0) != 0).Select(e => e.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Negated_order_comparison_on_non_nullable_arguments_gets_optimized(bool async)
        {
            var i = 1;

            await AssertQueryScalar(async, ss => ss.Set<NullSemanticsEntity1>().Where(e => !(e.IntA > i)).Select(e => e.Id));
            await AssertQueryScalar(async, ss => ss.Set<NullSemanticsEntity1>().Where(e => !(e.IntA >= i)).Select(e => e.Id));
            await AssertQueryScalar(async, ss => ss.Set<NullSemanticsEntity1>().Where(e => !(e.IntA < i)).Select(e => e.Id));
            await AssertQueryScalar(async, ss => ss.Set<NullSemanticsEntity1>().Where(e => !(e.IntA <= i)).Select(e => e.Id));
        }

        [ConditionalTheory(Skip = "issue #9544")]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Negated_order_comparison_on_nullable_arguments_doesnt_get_optimized(bool async)
        {
            var i = 1;

            await AssertQueryScalar(async, ss => ss.Set<NullSemanticsEntity1>().Where(e => !(e.NullableIntA > i)).Select(e => e.Id));
            await AssertQueryScalar(async, ss => ss.Set<NullSemanticsEntity1>().Where(e => !(e.NullableIntA >= i)).Select(e => e.Id));
            await AssertQueryScalar(async, ss => ss.Set<NullSemanticsEntity1>().Where(e => !(e.NullableIntA < i)).Select(e => e.Id));
            await AssertQueryScalar(async, ss => ss.Set<NullSemanticsEntity1>().Where(e => !(e.NullableIntA <= i)).Select(e => e.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Nullable_column_info_propagates_inside_binary_AndAlso(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<NullSemanticsEntity1>()
                    .Where(e => e.NullableStringA != null && e.NullableStringB != null && e.NullableStringA != e.NullableStringB)
                    .Select(e => e.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Nullable_column_info_doesnt_propagate_inside_binary_OrElse(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<NullSemanticsEntity1>().Where(
                        e => (e.NullableStringA != null || e.NullableStringB != null) && e.NullableStringA != e.NullableStringB)
                    .Select(e => e.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Nullable_column_info_propagates_inside_binary_OrElse_when_info_is_duplicated(bool async)
        {
            await AssertQueryScalar(
                async,
                ss => ss.Set<NullSemanticsEntity1>().Where(
                    e => ((e.NullableStringA != null && e.NullableStringB != null) || (e.NullableStringA != null))
                        && e.NullableStringA != e.NullableStringB).Select(e => e.Id));

            await AssertQueryScalar(
                async,
                ss => ss.Set<NullSemanticsEntity1>().Where(
                    e => ((e.NullableStringA != null && e.NullableStringB != null)
                            || (e.NullableStringB != null && e.NullableStringA != null))
                        && e.NullableStringA != e.NullableStringB).Select(e => e.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Nullable_column_info_propagates_inside_conditional(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<NullSemanticsEntity1>().Select(e => e.NullableStringA != null ? e.NullableStringA != e.StringA : e.BoolA));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Nullable_column_info_doesnt_propagate_between_projections(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<NullSemanticsEntity1>()
                    .Select(e => new { Foo = e.NullableStringA != null, Bar = e.NullableStringA != e.StringA }),
                elementSorter: e => (e.Foo, e.Bar));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Nullable_column_info_doesnt_propagate_between_different_parts_of_select(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => from e1 in ss.Set<NullSemanticsEntity1>()
                      join e2 in ss.Set<NullSemanticsEntity1>() on e1.NullableBoolA != null equals false
                      where e1.NullableBoolA != e2.NullableBoolB
                      select e1.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Nullable_column_info_propagation_complex(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<NullSemanticsEntity1>().Where(
                    e => (e.NullableStringA != null && e.NullableBoolB != null && e.NullableStringC != null)
                        && ((e.NullableStringA != null || e.NullableBoolC != null)
                            && e.NullableBoolB != e.NullableBoolC)).Select(e => e.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task String_concat_with_both_arguments_being_null(bool async)
        {
            var prm = default(string);
            await AssertQuery(async, ss => ss.Set<NullSemanticsEntity1>().Select(x => prm + prm));
            await AssertQuery(async, ss => ss.Set<NullSemanticsEntity1>().Select(x => prm + null));
            await AssertQuery(async, ss => ss.Set<NullSemanticsEntity1>().Select(x => prm + x.NullableStringA));

            await AssertQuery(async, ss => ss.Set<NullSemanticsEntity1>().Select(x => null + prm));
            await AssertQuery(async, ss => ss.Set<NullSemanticsEntity1>().Select(x => (string)null + null));
            await AssertQuery(async, ss => ss.Set<NullSemanticsEntity1>().Select(x => null + x.NullableStringA));

            await AssertQuery(async, ss => ss.Set<NullSemanticsEntity1>().Select(x => x.NullableStringB + prm));
            await AssertQuery(async, ss => ss.Set<NullSemanticsEntity1>().Select(x => x.NullableStringB + null));
            await AssertQuery(async, ss => ss.Set<NullSemanticsEntity1>().Select(x => x.NullableStringB + x.NullableStringA));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Empty_subquery_with_contains_returns_false(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<NullSemanticsEntity1>().Where(
                    e => ss.Set<NullSemanticsEntity2>().Where(x => false).Select(x => x.NullableIntA).Contains(e.NullableIntA)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Empty_subquery_with_contains_negated_returns_true(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<NullSemanticsEntity1>().Where(
                    e => !ss.Set<NullSemanticsEntity2>().Where(x => false).Select(x => x.NullableIntA).Contains(e.NullableIntA)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Nullable_string_FirstOrDefault_compared_to_nullable_string_LastOrDefault(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<NullSemanticsEntity1>().Where(e => e.NullableStringA.FirstOrDefault() == e.NullableStringB.LastOrDefault()),
                ss => ss.Set<NullSemanticsEntity1>().Where(
                    e => e.NullableStringA.MaybeScalar(x => x.FirstOrDefault())
                        == e.NullableStringB.MaybeScalar(x => x.LastOrDefault())));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Null_semantics_applied_to_CompareTo_equality(bool async)
        {
            await AssertQuery(
                async,
                ss => ss.Set<NullSemanticsEntity1>().Where(e => e.NullableStringA.CompareTo(e.NullableStringB) == 0),
                ss => ss.Set<NullSemanticsEntity1>().Where(e => e.NullableStringA == e.NullableStringB));

            await AssertQuery(
                async,
                ss => ss.Set<NullSemanticsEntity1>().Where(e => 0 == e.NullableStringA.CompareTo(e.NullableStringB)),
                ss => ss.Set<NullSemanticsEntity1>().Where(e => e.NullableStringA == e.NullableStringB));

            await AssertQuery(
                async,
                ss => ss.Set<NullSemanticsEntity1>().Where(e => e.NullableStringA.CompareTo(e.NullableStringB) != 0),
                ss => ss.Set<NullSemanticsEntity1>().Where(e => e.NullableStringA != e.NullableStringB));

            await AssertQuery(
                async,
                ss => ss.Set<NullSemanticsEntity1>().Where(e => 0 != e.NullableStringA.CompareTo(e.NullableStringB)),
                ss => ss.Set<NullSemanticsEntity1>().Where(e => e.NullableStringA != e.NullableStringB));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Nested_CompareTo_optimized(bool async)
        {
            await AssertQuery(
                async,
                ss => ss.Set<NullSemanticsEntity1>().Where(e => e.NullableStringA.CompareTo(e.NullableStringB).CompareTo(0) == 0),
                ss => ss.Set<NullSemanticsEntity1>().Where(e => e.NullableStringA == e.NullableStringB));

            await AssertQuery(
                async,
                ss => ss.Set<NullSemanticsEntity1>().Where(e => 0 == e.NullableStringA.CompareTo(e.NullableStringB).CompareTo(0)),
                ss => ss.Set<NullSemanticsEntity1>().Where(e => e.NullableStringA == e.NullableStringB));

            await AssertQuery(
                async,
                ss => ss.Set<NullSemanticsEntity1>().Where(e => e.NullableStringA.CompareTo(e.NullableStringB).CompareTo(0) != 0),
                ss => ss.Set<NullSemanticsEntity1>().Where(e => e.NullableStringA != e.NullableStringB));

            await AssertQuery(
                async,
                ss => ss.Set<NullSemanticsEntity1>().Where(e => 0 != e.NullableStringA.CompareTo(e.NullableStringB).CompareTo(0)),
                ss => ss.Set<NullSemanticsEntity1>().Where(e => e.NullableStringA != e.NullableStringB));
        }

        private string NormalizeDelimitersInRawString(string sql)
            => Fixture.TestStore.NormalizeDelimitersInRawString(sql);

        protected abstract NullSemanticsContext CreateContext(bool useRelationalNulls = false);

        protected virtual void ClearLog()
        {
        }
    }
}
