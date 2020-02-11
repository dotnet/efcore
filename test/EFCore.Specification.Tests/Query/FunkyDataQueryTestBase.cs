// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestModels.FunkyDataModel;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable StringStartsWithIsCultureSpecific
// ReSharper disable StringEndsWithIsCultureSpecific
// ReSharper disable RedundantTernaryExpression
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class FunkyDataQueryTestBase<TFixture> : QueryTestBase<TFixture>
        where TFixture : FunkyDataQueryTestBase<TFixture>.FunkyDataQueryFixtureBase, new()
    {
        protected FunkyDataQueryTestBase(TFixture fixture)
            : base(fixture)
        {
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task String_contains_on_argument_with_wildcard_constant(bool async)
        {
            await AssertQuery(
                async,
                ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.Contains("%B")).Select(c => c.FirstName),
                ss => ss.Set<FunkyCustomer>().Where(c => MaybeScalar<bool>(c.FirstName, () => c.FirstName.Contains("%B")) == true)
                    .Select(c => c.FirstName));

            await AssertQuery(
                async,
                ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.Contains("a_")).Select(c => c.FirstName),
                ss => ss.Set<FunkyCustomer>().Where(c => MaybeScalar<bool>(c.FirstName, () => c.FirstName.Contains("a_")) == true)
                    .Select(c => c.FirstName));

            await AssertQuery(
                async,
                ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.Contains(null)).Select(c => c.FirstName),
                ss => ss.Set<FunkyCustomer>().Where(c => false).Select(c => c.FirstName));

            await AssertQuery(
                async,
                ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.Contains("")).Select(c => c.FirstName),
                ss => ss.Set<FunkyCustomer>().Select(c => c.FirstName));

            await AssertQuery(
                async,
                ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.Contains("_Ba_")).Select(c => c.FirstName),
                ss => ss.Set<FunkyCustomer>().Where(c => MaybeScalar<bool>(c.FirstName, () => c.FirstName.Contains("_Ba_")) == true)
                    .Select(c => c.FirstName));

            await AssertQuery(
                async,
                ss => ss.Set<FunkyCustomer>().Where(c => !c.FirstName.Contains("%B%a%r")).Select(c => c.FirstName),
                ss => ss.Set<FunkyCustomer>().Where(c => !MaybeScalar<bool>(c.FirstName, () => c.FirstName.Contains("%B%a%r")) == true)
                    .Select(c => c.FirstName));

            await AssertQuery(
                async,
                ss => ss.Set<FunkyCustomer>().Where(c => !c.FirstName.Contains("")).Select(c => c.FirstName),
                ss => ss.Set<FunkyCustomer>().Where(c => !MaybeScalar<bool>(c.FirstName, () => c.FirstName.Contains("")) == true)
                    .Select(c => c.FirstName));

            await AssertQuery(
                async,
                ss => ss.Set<FunkyCustomer>().Where(c => !c.FirstName.Contains(null)).Select(c => c.FirstName),
                ss => ss.Set<FunkyCustomer>().Where(c => false).Select(c => c.FirstName));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task String_contains_on_argument_with_wildcard_parameter(bool async)
        {
            var prm1 = "%B";
            await AssertQuery(
                async,
                ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.Contains(prm1)).Select(c => c.FirstName),
                ss => ss.Set<FunkyCustomer>().Where(c => MaybeScalar<bool>(c.FirstName, () => c.FirstName.Contains(prm1)) == true)
                    .Select(c => c.FirstName));

            var prm2 = "a_";
            await AssertQuery(
                async,
                ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.Contains(prm2)).Select(c => c.FirstName),
                ss => ss.Set<FunkyCustomer>().Where(c => MaybeScalar<bool>(c.FirstName, () => c.FirstName.Contains(prm2)) == true)
                    .Select(c => c.FirstName));

            var prm3 = (string)null;
            await AssertQuery(
                async,
                ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.Contains(prm3)).Select(c => c.FirstName),
                ss => ss.Set<FunkyCustomer>().Where(c => false).Select(c => c.FirstName));

            var prm4 = "";
            await AssertQuery(
                async,
                ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.Contains(prm4)).Select(c => c.FirstName),
                ss => ss.Set<FunkyCustomer>().Select(c => c.FirstName));

            var prm5 = "_Ba_";
            await AssertQuery(
                async,
                ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.Contains(prm5)).Select(c => c.FirstName),
                ss => ss.Set<FunkyCustomer>().Where(c => MaybeScalar<bool>(c.FirstName, () => c.FirstName.Contains(prm5)) == true)
                    .Select(c => c.FirstName));

            var prm6 = "%B%a%r";
            await AssertQuery(
                async,
                ss => ss.Set<FunkyCustomer>().Where(c => !c.FirstName.Contains(prm6)).Select(c => c.FirstName),
                ss => ss.Set<FunkyCustomer>().Where(c => !MaybeScalar<bool>(c.FirstName, () => c.FirstName.Contains(prm6)) == true)
                    .Select(c => c.FirstName));

            var prm7 = "";
            await AssertQuery(
                async,
                ss => ss.Set<FunkyCustomer>().Where(c => !c.FirstName.Contains(prm7)).Select(c => c.FirstName),
                ss => ss.Set<FunkyCustomer>().Where(c => false).Select(c => c.FirstName));

            var prm8 = (string)null;
            await AssertQuery(
                async,
                ss => ss.Set<FunkyCustomer>().Where(c => !c.FirstName.Contains(prm8)).Select(c => c.FirstName),
                ss => ss.Set<FunkyCustomer>().Where(c => false).Select(c => c.FirstName));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task String_contains_on_argument_with_wildcard_column(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<FunkyCustomer>().Select(c => c.FirstName)
                    .SelectMany(c => ss.Set<FunkyCustomer>().Select(c2 => c2.LastName), (fn, ln) => new { fn, ln })
                    .Where(r => r.fn.Contains(r.ln)),
                ss => ss.Set<FunkyCustomer>().Select(c => c.FirstName)
                    .SelectMany(c => ss.Set<FunkyCustomer>().Select(c2 => c2.LastName), (fn, ln) => new { fn, ln })
                    .Where(r => r.ln == "" || MaybeScalar(r.fn, () => MaybeScalar<bool>(r.ln, () => r.fn.Contains(r.ln))) == true),
                elementSorter: e => (e.fn, e.ln),
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.fn, a.fn);
                    Assert.Equal(e.ln, a.ln);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task String_contains_on_argument_with_wildcard_column_negated(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<FunkyCustomer>().Select(c => c.FirstName)
                    .SelectMany(c => ss.Set<FunkyCustomer>().Select(c2 => c2.LastName), (fn, ln) => new { fn, ln })
                    .Where(r => !r.fn.Contains(r.ln)),
                ss => ss.Set<FunkyCustomer>().Select(c => c.FirstName)
                    .SelectMany(c => ss.Set<FunkyCustomer>().Select(c2 => c2.LastName), (fn, ln) => new { fn, ln })
                    .Where(r => r.ln != "" && !MaybeScalar(r.fn, () => MaybeScalar<bool>(r.ln, () => r.fn.Contains(r.ln))) == true));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task String_starts_with_on_argument_with_wildcard_constant(bool async)
        {
            await AssertQuery(
                async,
                ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.StartsWith("%B")).Select(c => c.FirstName),
                ss => ss.Set<FunkyCustomer>().Where(c => MaybeScalar<bool>(c.FirstName, () => c.FirstName.StartsWith("%B")) == true)
                    .Select(c => c.FirstName));

            await AssertQuery(
                async,
                ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.StartsWith("a_")).Select(c => c.FirstName),
                ss => ss.Set<FunkyCustomer>().Where(c => MaybeScalar<bool>(c.FirstName, () => c.FirstName.StartsWith("a_")) == true)
                    .Select(c => c.FirstName));

            await AssertQuery(
                async,
                ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.StartsWith(null)).Select(c => c.FirstName),
                ss => ss.Set<FunkyCustomer>().Where(c => false).Select(c => c.FirstName));

            await AssertQuery(
                async,
                ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.StartsWith("")).Select(c => c.FirstName),
                ss => ss.Set<FunkyCustomer>().Select(c => c.FirstName));

            await AssertQuery(
                async,
                ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.StartsWith("_Ba_")).Select(c => c.FirstName),
                ss => ss.Set<FunkyCustomer>().Where(c => MaybeScalar<bool>(c.FirstName, () => c.FirstName.StartsWith("_Ba_")) == true)
                    .Select(c => c.FirstName));

            await AssertQuery(
                async,
                ss => ss.Set<FunkyCustomer>().Where(c => !c.FirstName.StartsWith("%B%a%r")).Select(c => c.FirstName),
                ss => ss.Set<FunkyCustomer>().Where(c => !MaybeScalar<bool>(c.FirstName, () => c.FirstName.StartsWith("%B%a%r")) == true)
                    .Select(c => c.FirstName));

            await AssertQuery(
                async,
                ss => ss.Set<FunkyCustomer>().Where(c => !c.FirstName.StartsWith("")).Select(c => c.FirstName),
                ss => ss.Set<FunkyCustomer>().Where(c => !MaybeScalar<bool>(c.FirstName, () => c.FirstName.StartsWith("")) == true)
                    .Select(c => c.FirstName));

            await AssertQuery(
                async,
                ss => ss.Set<FunkyCustomer>().Where(c => !c.FirstName.StartsWith(null)).Select(c => c.FirstName),
                ss => ss.Set<FunkyCustomer>().Where(c => false).Select(c => c.FirstName));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task String_starts_with_on_argument_with_wildcard_parameter(bool async)
        {
            var prm1 = "%B";
            await AssertQuery(
                async,
                ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.StartsWith(prm1)).Select(c => c.FirstName),
                ss => ss.Set<FunkyCustomer>().Where(c => MaybeScalar<bool>(c.FirstName, () => c.FirstName.StartsWith(prm1)) == true)
                    .Select(c => c.FirstName));

            var prm2 = "a_";
            await AssertQuery(
                async,
                ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.StartsWith(prm2)).Select(c => c.FirstName),
                ss => ss.Set<FunkyCustomer>().Where(c => MaybeScalar<bool>(c.FirstName, () => c.FirstName.StartsWith(prm2)) == true)
                    .Select(c => c.FirstName));

            var prm3 = (string)null;
            await AssertQuery(
                async,
                ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.StartsWith(prm3)).Select(c => c.FirstName),
                ss => ss.Set<FunkyCustomer>().Where(c => false).Select(c => c.FirstName));

            var prm4 = "";
            await AssertQuery(
                async,
                ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.StartsWith(prm4)).Select(c => c.FirstName),
                ss => ss.Set<FunkyCustomer>().Select(c => c.FirstName));

            var prm5 = "_Ba_";
            await AssertQuery(
                async,
                ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.StartsWith(prm5)).Select(c => c.FirstName),
                ss => ss.Set<FunkyCustomer>().Where(c => MaybeScalar<bool>(c.FirstName, () => c.FirstName.StartsWith(prm5)) == true)
                    .Select(c => c.FirstName));

            var prm6 = "%B%a%r";
            await AssertQuery(
                async,
                ss => ss.Set<FunkyCustomer>().Where(c => !c.FirstName.StartsWith(prm6)).Select(c => c.FirstName),
                ss => ss.Set<FunkyCustomer>().Where(c => !MaybeScalar<bool>(c.FirstName, () => c.FirstName.StartsWith(prm6)) == true)
                    .Select(c => c.FirstName));

            var prm7 = "";
            await AssertQuery(
                async,
                ss => ss.Set<FunkyCustomer>().Where(c => !c.FirstName.StartsWith(prm7)).Select(c => c.FirstName),
                ss => ss.Set<FunkyCustomer>().Where(c => false).Select(c => c.FirstName));

            var prm8 = (string)null;
            await AssertQuery(
                async,
                ss => ss.Set<FunkyCustomer>().Where(c => !c.FirstName.StartsWith(prm8)).Select(c => c.FirstName),
                ss => ss.Set<FunkyCustomer>().Where(c => false).Select(c => c.FirstName));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task String_starts_with_on_argument_with_bracket(bool async)
        {
            await AssertQuery(
                async,
                ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.StartsWith("[")),
                ss => ss.Set<FunkyCustomer>().Where(c => MaybeScalar<bool>(c.FirstName, () => c.FirstName.StartsWith("[")) == true));

            await AssertQuery(
                async,
                ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.StartsWith("B[")),
                ss => ss.Set<FunkyCustomer>().Where(c => MaybeScalar<bool>(c.FirstName, () => c.FirstName.StartsWith("B[")) == true));

            await AssertQuery(
                async,
                ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.StartsWith("B[[a^")),
                ss => ss.Set<FunkyCustomer>().Where(c => MaybeScalar<bool>(c.FirstName, () => c.FirstName.StartsWith("B[[a^")) == true));

            var prm1 = "[";
            await AssertQuery(
                async,
                ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.StartsWith(prm1)),
                ss => ss.Set<FunkyCustomer>().Where(c => MaybeScalar<bool>(c.FirstName, () => c.FirstName.StartsWith(prm1)) == true));

            var prm2 = "B[";
            await AssertQuery(
                async,
                ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.StartsWith(prm2)),
                ss => ss.Set<FunkyCustomer>().Where(c => MaybeScalar<bool>(c.FirstName, () => c.FirstName.StartsWith(prm2)) == true));

            var prm3 = "B[[a^";
            await AssertQuery(
                async,
                ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.StartsWith(prm3)),
                ss => ss.Set<FunkyCustomer>().Where(c => MaybeScalar<bool>(c.FirstName, () => c.FirstName.StartsWith(prm3)) == true));

            await AssertQuery(
                async,
                ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.StartsWith(c.LastName)),
                ss => ss.Set<FunkyCustomer>().Where(
                    c => c.LastName != null && MaybeScalar<bool>(c.FirstName, () => c.FirstName.StartsWith(c.LastName)) == true));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task String_starts_with_on_argument_with_wildcard_column(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<FunkyCustomer>().Select(c => c.FirstName)
                    .SelectMany(c => ss.Set<FunkyCustomer>().Select(c2 => c2.LastName), (fn, ln) => new { fn, ln })
                    .Where(r => r.fn.StartsWith(r.ln)),
                ss => ss.Set<FunkyCustomer>().Select(c => c.FirstName)
                    .SelectMany(c => ss.Set<FunkyCustomer>().Select(c2 => c2.LastName), (fn, ln) => new { fn, ln })
                    .Where(r => r.ln == "" || MaybeScalar(r.fn, () => MaybeScalar<bool>(r.ln, () => r.fn.StartsWith(r.ln))) == true),
                elementSorter: e => (e.fn, e.ln),
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.fn, a.fn);
                    Assert.Equal(e.ln, a.ln);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task String_starts_with_on_argument_with_wildcard_column_negated(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<FunkyCustomer>().Select(c => c.FirstName)
                    .SelectMany(c => ss.Set<FunkyCustomer>().Select(c2 => c2.LastName), (fn, ln) => new { fn, ln })
                    .Where(r => !r.fn.StartsWith(r.ln)),
                ss => ss.Set<FunkyCustomer>().Select(c => c.FirstName)
                    .SelectMany(c => ss.Set<FunkyCustomer>().Select(c2 => c2.LastName), (fn, ln) => new { fn, ln })
                    .Where(r => r.ln != "" && !MaybeScalar(r.fn, () => MaybeScalar<bool>(r.ln, () => r.fn.StartsWith(r.ln))) == true));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task String_ends_with_on_argument_with_wildcard_constant(bool async)
        {
            await AssertQuery(
                async,
                ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.EndsWith("%B")).Select(c => c.FirstName),
                ss => ss.Set<FunkyCustomer>().Where(c => MaybeScalar<bool>(c.FirstName, () => c.FirstName.EndsWith("%B")) == true)
                    .Select(c => c.FirstName));

            await AssertQuery(
                async,
                ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.EndsWith("a_")).Select(c => c.FirstName),
                ss => ss.Set<FunkyCustomer>().Where(c => MaybeScalar<bool>(c.FirstName, () => c.FirstName.EndsWith("a_")) == true)
                    .Select(c => c.FirstName));

            await AssertQuery(
                async,
                ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.EndsWith(null)).Select(c => c.FirstName),
                ss => ss.Set<FunkyCustomer>().Where(c => false).Select(c => c.FirstName));

            await AssertQuery(
                async,
                ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.EndsWith("")).Select(c => c.FirstName),
                ss => ss.Set<FunkyCustomer>().Select(c => c.FirstName));

            await AssertQuery(
                async,
                ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.EndsWith("_Ba_")).Select(c => c.FirstName),
                ss => ss.Set<FunkyCustomer>().Where(c => MaybeScalar<bool>(c.FirstName, () => c.FirstName.EndsWith("_Ba_")) == true)
                    .Select(c => c.FirstName));

            await AssertQuery(
                async,
                ss => ss.Set<FunkyCustomer>().Where(c => !c.FirstName.EndsWith("%B%a%r")).Select(c => c.FirstName),
                ss => ss.Set<FunkyCustomer>().Where(c => !MaybeScalar<bool>(c.FirstName, () => c.FirstName.EndsWith("%B%a%r")) == true)
                    .Select(c => c.FirstName));

            await AssertQuery(
                async,
                ss => ss.Set<FunkyCustomer>().Where(c => !c.FirstName.EndsWith("")).Select(c => c.FirstName),
                ss => ss.Set<FunkyCustomer>().Where(c => !MaybeScalar<bool>(c.FirstName, () => c.FirstName.EndsWith("")) == true)
                    .Select(c => c.FirstName));

            await AssertQuery(
                async,
                ss => ss.Set<FunkyCustomer>().Where(c => !c.FirstName.EndsWith(null)).Select(c => c.FirstName),
                ss => ss.Set<FunkyCustomer>().Where(c => false).Select(c => c.FirstName));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task String_ends_with_on_argument_with_wildcard_parameter(bool async)
        {
            var prm1 = "%B";
            await AssertQuery(
                async,
                ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.EndsWith(prm1)).Select(c => c.FirstName),
                ss => ss.Set<FunkyCustomer>().Where(c => MaybeScalar<bool>(c.FirstName, () => c.FirstName.EndsWith(prm1)) == true)
                    .Select(c => c.FirstName));

            var prm2 = "a_";
            await AssertQuery(
                async,
                ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.EndsWith(prm2)).Select(c => c.FirstName),
                ss => ss.Set<FunkyCustomer>().Where(c => MaybeScalar<bool>(c.FirstName, () => c.FirstName.EndsWith(prm2)) == true)
                    .Select(c => c.FirstName));

            var prm3 = (string)null;
            await AssertQuery(
                async,
                ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.EndsWith(prm3)).Select(c => c.FirstName),
                ss => ss.Set<FunkyCustomer>().Where(c => false).Select(c => c.FirstName));

            var prm4 = "";
            await AssertQuery(
                async,
                ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.EndsWith(prm4)).Select(c => c.FirstName),
                ss => ss.Set<FunkyCustomer>().Select(c => c.FirstName));

            var prm5 = "_Ba_";
            await AssertQuery(
                async,
                ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.EndsWith(prm5)).Select(c => c.FirstName),
                ss => ss.Set<FunkyCustomer>().Where(c => MaybeScalar<bool>(c.FirstName, () => c.FirstName.EndsWith(prm5)) == true)
                    .Select(c => c.FirstName));

            var prm6 = "%B%a%r";
            await AssertQuery(
                async,
                ss => ss.Set<FunkyCustomer>().Where(c => !c.FirstName.EndsWith(prm6)).Select(c => c.FirstName),
                ss => ss.Set<FunkyCustomer>().Where(c => !MaybeScalar<bool>(c.FirstName, () => c.FirstName.EndsWith(prm6)) == true)
                    .Select(c => c.FirstName));

            var prm7 = "";
            await AssertQuery(
                async,
                ss => ss.Set<FunkyCustomer>().Where(c => !c.FirstName.EndsWith(prm7)).Select(c => c.FirstName),
                ss => ss.Set<FunkyCustomer>().Where(c => false).Select(c => c.FirstName));

            var prm8 = (string)null;
            await AssertQuery(
                async,
                ss => ss.Set<FunkyCustomer>().Where(c => !c.FirstName.EndsWith(prm8)).Select(c => c.FirstName),
                ss => ss.Set<FunkyCustomer>().Where(c => false).Select(c => c.FirstName));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task String_ends_with_on_argument_with_wildcard_column(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<FunkyCustomer>().Select(c => c.FirstName)
                    .SelectMany(c => ss.Set<FunkyCustomer>().Select(c2 => c2.LastName), (fn, ln) => new { fn, ln })
                    .Where(r => r.fn.EndsWith(r.ln)),
                ss => ss.Set<FunkyCustomer>().Select(c => c.FirstName)
                    .SelectMany(c => ss.Set<FunkyCustomer>().Select(c2 => c2.LastName), (fn, ln) => new { fn, ln })
                    .Where(r => r.ln == "" || MaybeScalar(r.fn, () => MaybeScalar<bool>(r.ln, () => r.fn.EndsWith(r.ln))) == true),
                elementSorter: e => (e.fn, e.ln),
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.fn, a.fn);
                    Assert.Equal(e.ln, a.ln);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task String_ends_with_on_argument_with_wildcard_column_negated(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<FunkyCustomer>().Select(c => c.FirstName)
                    .SelectMany(c => ss.Set<FunkyCustomer>().Select(c2 => c2.LastName), (fn, ln) => new { fn, ln })
                    .Where(r => !r.fn.EndsWith(r.ln)),
                ss => ss.Set<FunkyCustomer>().Select(c => c.FirstName)
                    .SelectMany(c => ss.Set<FunkyCustomer>().Select(c2 => c2.LastName), (fn, ln) => new { fn, ln })
                    .Where(r => r.ln != "" && !MaybeScalar(r.fn, () => MaybeScalar<bool>(r.ln, () => r.fn.EndsWith(r.ln))) == true));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task String_ends_with_inside_conditional(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<FunkyCustomer>().Select(c => c.FirstName)
                    .SelectMany(c => ss.Set<FunkyCustomer>().Select(c2 => c2.LastName), (fn, ln) => new { fn, ln })
                    .Where(r => r.fn.EndsWith(r.ln) ? true : false),
                ss => ss.Set<FunkyCustomer>().Select(c => c.FirstName)
                    .SelectMany(c => ss.Set<FunkyCustomer>().Select(c2 => c2.LastName), (fn, ln) => new { fn, ln })
                    .Where(
                        r => r.ln == "" || MaybeScalar(r.fn, () => MaybeScalar<bool>(r.ln, () => r.fn.EndsWith(r.ln))) == true
                            ? true
                            : false),
                elementSorter: e => (e.fn, e.ln),
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.fn, a.fn);
                    Assert.Equal(e.ln, a.ln);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task String_ends_with_inside_conditional_negated(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<FunkyCustomer>().Select(c => c.FirstName)
                    .SelectMany(c => ss.Set<FunkyCustomer>().Select(c2 => c2.LastName), (fn, ln) => new { fn, ln })
                    .Where(r => !r.fn.EndsWith(r.ln) ? true : false),
                ss => ss.Set<FunkyCustomer>().Select(c => c.FirstName)
                    .SelectMany(c => ss.Set<FunkyCustomer>().Select(c2 => c2.LastName), (fn, ln) => new { fn, ln })
                    .Where(
                        r => r.ln != "" && !MaybeScalar(r.fn, () => MaybeScalar<bool>(r.ln, () => r.fn.EndsWith(r.ln))) == true
                            ? true
                            : false));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task String_ends_with_equals_nullable_column(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<FunkyCustomer>().SelectMany(c => ss.Set<FunkyCustomer>(), (c1, c2) => new { c1, c2 })
                    .Where(r => r.c1.FirstName.EndsWith(r.c2.LastName) == r.c1.NullableBool.Value),
                ss => ss.Set<FunkyCustomer>().SelectMany(c => ss.Set<FunkyCustomer>(), (c1, c2) => new { c1, c2 })
                    .Where(
                        r => MaybeScalar(
                                r.c1.FirstName, () => MaybeScalar<bool>(r.c2.LastName, () => r.c1.FirstName.EndsWith(r.c2.LastName)))
                            == true
                            == MaybeScalar<bool>(r.c1.NullableBool, () => r.c1.NullableBool.Value)),
                elementSorter: e => (e.c1.Id, e.c2.Id),
                elementAsserter: (e, a) =>
                {
                    AssertEqual(e.c1, a.c1);
                    AssertEqual(e.c2, a.c2);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task String_ends_with_not_equals_nullable_column(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<FunkyCustomer>().SelectMany(c => ss.Set<FunkyCustomer>(), (c1, c2) => new { c1, c2 })
                    .Where(r => r.c1.FirstName.EndsWith(r.c2.LastName) != r.c1.NullableBool.Value),
                ss => ss.Set<FunkyCustomer>().SelectMany(c => ss.Set<FunkyCustomer>(), (c1, c2) => new { c1, c2 })
                    .Where(
                        r => MaybeScalar(
                                r.c1.FirstName, () => MaybeScalar<bool>(r.c2.LastName, () => r.c1.FirstName.EndsWith(r.c2.LastName)))
                            == true
                            != MaybeScalar<bool>(r.c1.NullableBool, () => r.c1.NullableBool.Value)),
                elementSorter: e => (e.c1.Id, e.c2.Id),
                elementAsserter: (e, a) =>
                {
                    AssertEqual(e.c1, a.c1);
                    AssertEqual(e.c2, a.c2);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task String_equals_with_trailing_whitespace_constant(bool async)
            => AssertQuery(
                async,
                ss => ss.Set<FunkyCustomer>().Where(c => c.LastName == "WithoutTrailing ").Select(c => c.FirstName));

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task String_equals_with_trailing_whitespace_column(bool async)
            => AssertQuery(
                async,
                ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName == "WithTrailing").Select(c => c.FirstName));

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task String_equals_with_like_wildcard(bool async)
            => AssertQuery(
                async,
                ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName == "With%Wildcard").Select(c => c.FirstName));

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task String_equals_with_trailing_whitespace_and_like_wildcard(bool async)
            => AssertQuery(
                async,
                ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName == "WithTrailingAnd%Wildcard").Select(c => c.FirstName));

        protected FunkyDataContext CreateContext() => Fixture.CreateContext();

        protected virtual void ClearLog()
        {
        }

        public abstract class FunkyDataQueryFixtureBase : SharedStoreFixtureBase<FunkyDataContext>, IQueryFixtureBase
        {
            public FunkyDataQueryFixtureBase()
            {
                var entitySorters = new Dictionary<Type, Func<object, object>> { { typeof(FunkyCustomer), e => ((FunkyCustomer)e)?.Id } }
                    .ToDictionary(e => e.Key, e => (object)e.Value);

                var entityAsserters = new Dictionary<Type, Action<object, object>>
                {
                    {
                        typeof(FunkyCustomer), (e, a) =>
                        {
                            Assert.Equal(e == null, a == null);
                            if (a != null)
                            {
                                var ee = (FunkyCustomer)e;
                                var aa = (FunkyCustomer)a;

                                Assert.Equal(ee.Id, aa.Id);
                                Assert.Equal(ee.FirstName, aa.FirstName);
                                Assert.Equal(ee.LastName, aa.LastName);
                                Assert.Equal(ee.NullableBool, aa.NullableBool);
                            }
                        }
                    }
                }.ToDictionary(e => e.Key, e => (object)e.Value);

                QueryAsserter = CreateQueryAsserter(entitySorters, entityAsserters);
            }

            protected virtual QueryAsserter<FunkyDataContext> CreateQueryAsserter(
                Dictionary<Type, object> entitySorters,
                Dictionary<Type, object> entityAsserters)
                => new QueryAsserter<FunkyDataContext>(
                    CreateContext,
                    new FunkyDataData(),
                    entitySorters,
                    entityAsserters);

            protected override string StoreName { get; } = "FunkyDataQueryTest";

            public QueryAsserterBase QueryAsserter { get; set; }

            public override FunkyDataContext CreateContext()
            {
                var context = base.CreateContext();
                context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
                return context;
            }

            protected override void Seed(FunkyDataContext context) => FunkyDataContext.Seed(context);
        }
    }
}
