// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.FunkyDataModel;

// ReSharper disable StringStartsWithIsCultureSpecific
// ReSharper disable StringEndsWithIsCultureSpecific
// ReSharper disable RedundantTernaryExpression
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

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
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.MaybeScalar(x => x.Contains("%B")) == true).Select(c => c.FirstName));

        await AssertQuery(
            async,
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.Contains("a_")).Select(c => c.FirstName),
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.MaybeScalar(x => x.Contains("a_")) == true).Select(c => c.FirstName));

        await AssertQuery(
            async,
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.Contains(null)).Select(c => c.FirstName),
            ss => ss.Set<FunkyCustomer>().Where(c => false).Select(c => c.FirstName),
            assertEmpty: true);

        await AssertQuery(
            async,
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.Contains("")).Select(c => c.FirstName),
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName != null).Select(c => c.FirstName));

        await AssertQuery(
            async,
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.Contains("_Ba_")).Select(c => c.FirstName),
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.MaybeScalar(x => x.Contains("_Ba_")) == true).Select(c => c.FirstName));

        await AssertQuery(
            async,
            ss => ss.Set<FunkyCustomer>().Where(c => !c.FirstName.Contains("%B%a%r")).Select(c => c.FirstName),
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.MaybeScalar(x => x.Contains("%B%a%r")) != true)
                .Select(c => c.FirstName));

        await AssertQuery(
            async,
            ss => ss.Set<FunkyCustomer>().Where(c => !c.FirstName.Contains("")).Select(c => c.FirstName),
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName == null).Select(c => c.FirstName));

        await AssertQuery(
            async,
            ss => ss.Set<FunkyCustomer>().Where(c => !c.FirstName.Contains(null)).Select(c => c.FirstName),
            ss => ss.Set<FunkyCustomer>().Where(c => true).Select(c => c.FirstName));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task String_contains_on_argument_with_wildcard_parameter(bool async)
    {
        var prm1 = "%B";
        await AssertQuery(
            async,
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.Contains(prm1)).Select(c => c.FirstName),
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.MaybeScalar(x => x.Contains(prm1)) == true).Select(c => c.FirstName));

        var prm2 = "a_";
        await AssertQuery(
            async,
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.Contains(prm2)).Select(c => c.FirstName),
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.MaybeScalar(x => x.Contains(prm2)) == true).Select(c => c.FirstName));

        var prm3 = (string)null;
        await AssertQuery(
            async,
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.Contains(prm3)).Select(c => c.FirstName),
            ss => ss.Set<FunkyCustomer>().Where(c => false).Select(c => c.FirstName),
            assertEmpty: true);

        var prm4 = "";
        await AssertQuery(
            async,
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.Contains(prm4)).Select(c => c.FirstName),
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName != null).Select(c => c.FirstName));

        var prm5 = "_Ba_";
        await AssertQuery(
            async,
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.Contains(prm5)).Select(c => c.FirstName),
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.MaybeScalar(x => x.Contains(prm5)) == true).Select(c => c.FirstName));

        var prm6 = "%B%a%r";
        await AssertQuery(
            async,
            ss => ss.Set<FunkyCustomer>().Where(c => !c.FirstName.Contains(prm6)).Select(c => c.FirstName),
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.MaybeScalar(x => x.Contains(prm6)) != true)
                .Select(c => c.FirstName));

        var prm7 = "";
        await AssertQuery(
            async,
            ss => ss.Set<FunkyCustomer>().Where(c => !c.FirstName.Contains(prm7)).Select(c => c.FirstName),
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName == null).Select(c => c.FirstName));

        var prm8 = (string)null;
        await AssertQuery(
            async,
            ss => ss.Set<FunkyCustomer>().Where(c => !c.FirstName.Contains(prm8)).Select(c => c.FirstName),
            ss => ss.Set<FunkyCustomer>().Where(c => true).Select(c => c.FirstName));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task String_contains_on_argument_with_wildcard_column(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<FunkyCustomer>().Select(c => c.FirstName)
                .SelectMany(c => ss.Set<FunkyCustomer>().Select(c2 => c2.LastName), (fn, ln) => new { fn, ln })
                .Where(r => r.fn.Contains(r.ln)),
            ss => ss.Set<FunkyCustomer>().Select(c => c.FirstName)
                .SelectMany(c => ss.Set<FunkyCustomer>().Select(c2 => c2.LastName), (fn, ln) => new { fn, ln })
                .Where(r => r.fn.MaybeScalar(x => r.ln.MaybeScalar(xx => x.Contains(xx))) == true),
            elementSorter: e => (e.fn, e.ln),
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.fn, a.fn);
                Assert.Equal(e.ln, a.ln);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task String_contains_on_argument_with_wildcard_column_negated(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<FunkyCustomer>().Select(c => c.FirstName)
                .SelectMany(c => ss.Set<FunkyCustomer>().Select(c2 => c2.LastName), (fn, ln) => new { fn, ln })
                .Where(r => !r.fn.Contains(r.ln)),
            ss => ss.Set<FunkyCustomer>().Select(c => c.FirstName)
                .SelectMany(c => ss.Set<FunkyCustomer>().Select(c2 => c2.LastName), (fn, ln) => new { fn, ln })
                .Where(r => r.fn.MaybeScalar(x => r.ln.MaybeScalar(xx => x.Contains(xx))) != true));
    // .Where(r => r.ln != "" && !r.fn.MaybeScalar(x => r.ln.MaybeScalar(xx => x.Contains(xx))) == true));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task String_starts_with_on_argument_with_wildcard_constant(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.StartsWith("%B")).Select(c => c.FirstName),
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.MaybeScalar(x => x.StartsWith("%B")) == true).Select(c => c.FirstName));

        await AssertQuery(
            async,
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.StartsWith("_B")).Select(c => c.FirstName),
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.MaybeScalar(x => x.StartsWith("_B")) == true).Select(c => c.FirstName));

        await AssertQuery(
            async,
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.StartsWith(null)).Select(c => c.FirstName),
            ss => ss.Set<FunkyCustomer>().Where(c => false).Select(c => c.FirstName),
            assertEmpty: true);

        await AssertQuery(
            async,
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.StartsWith("")).Select(c => c.FirstName),
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName != null).Select(c => c.FirstName));

        await AssertQuery(
            async,
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.StartsWith("_Ba_")).Select(c => c.FirstName),
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.MaybeScalar(x => x.StartsWith("_Ba_")) == true).Select(c => c.FirstName));

        await AssertQuery(
            async,
            ss => ss.Set<FunkyCustomer>().Where(c => !c.FirstName.StartsWith("%B%a%r")).Select(c => c.FirstName),
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.MaybeScalar(x => x.StartsWith("%B%a%r")) != true)
                .Select(c => c.FirstName));

        await AssertQuery(
            async,
            ss => ss.Set<FunkyCustomer>().Where(c => !c.FirstName.StartsWith("")).Select(c => c.FirstName),
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName == null)
                .Select(c => c.FirstName));

        await AssertQuery(
            async,
            ss => ss.Set<FunkyCustomer>().Where(c => !c.FirstName.StartsWith(null)).Select(c => c.FirstName),
            ss => ss.Set<FunkyCustomer>().Where(c => true).Select(c => c.FirstName));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task String_starts_with_on_argument_with_wildcard_parameter(bool async)
    {
        var prm1 = "%B";
        await AssertQuery(
            async,
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.StartsWith(prm1)).Select(c => c.FirstName),
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.MaybeScalar(x => x.StartsWith(prm1)) == true).Select(c => c.FirstName));

        var prm2 = "_B";
        await AssertQuery(
            async,
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.StartsWith(prm2)).Select(c => c.FirstName),
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.MaybeScalar(x => x.StartsWith(prm2)) == true).Select(c => c.FirstName));

        var prm3 = (string)null;
        await AssertQuery(
            async,
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.StartsWith(prm3)).Select(c => c.FirstName),
            ss => ss.Set<FunkyCustomer>().Where(c => false).Select(c => c.FirstName),
            assertEmpty: true);

        var prm4 = "";
        await AssertQuery(
            async,
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.StartsWith(prm4)).Select(c => c.FirstName),
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName != null).Select(c => c.FirstName));

        var prm5 = "_Ba_";
        await AssertQuery(
            async,
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.StartsWith(prm5)).Select(c => c.FirstName),
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.MaybeScalar(x => x.StartsWith(prm5)) == true).Select(c => c.FirstName));

        var prm6 = "%B%a%r";
        await AssertQuery(
            async,
            ss => ss.Set<FunkyCustomer>().Where(c => !c.FirstName.StartsWith(prm6)).Select(c => c.FirstName),
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.MaybeScalar(x => x.StartsWith(prm6)) != true)
                .Select(c => c.FirstName));

        var prm7 = "";
        await AssertQuery(
            async,
            ss => ss.Set<FunkyCustomer>().Where(c => !c.FirstName.StartsWith(prm7)).Select(c => c.FirstName),
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName == null).Select(c => c.FirstName));

        var prm8 = (string)null;
        await AssertQuery(
            async,
            ss => ss.Set<FunkyCustomer>().Where(c => !c.FirstName.StartsWith(prm8)).Select(c => c.FirstName),
            ss => ss.Set<FunkyCustomer>().Where(c => true).Select(c => c.FirstName));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task String_starts_with_on_argument_with_bracket(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.StartsWith("[")),
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.MaybeScalar(x => x.StartsWith("[")) == true));

        await AssertQuery(
            async,
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.StartsWith("B[")),
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.MaybeScalar(x => x.StartsWith("B[")) == true));

        await AssertQuery(
            async,
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.StartsWith("B[[a^")),
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.MaybeScalar(x => x.StartsWith("B[[a^")) == true));

        var prm1 = "[";
        await AssertQuery(
            async,
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.StartsWith(prm1)),
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.MaybeScalar(x => x.StartsWith(prm1)) == true));

        var prm2 = "B[";
        await AssertQuery(
            async,
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.StartsWith(prm2)),
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.MaybeScalar(x => x.StartsWith(prm2)) == true));

        var prm3 = "B[[a^";
        await AssertQuery(
            async,
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.StartsWith(prm3)),
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.MaybeScalar(x => x.StartsWith(prm3)) == true));

        await AssertQuery(
            async,
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.StartsWith(c.LastName)),
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.MaybeScalar(x => c.LastName.MaybeScalar(xx => x.StartsWith(xx))) == true));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task String_starts_with_on_argument_with_wildcard_column(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<FunkyCustomer>().Select(c => c.FirstName)
                .SelectMany(c => ss.Set<FunkyCustomer>().Select(c2 => c2.LastName), (fn, ln) => new { fn, ln })
                .Where(r => r.fn.StartsWith(r.ln)),
            ss => ss.Set<FunkyCustomer>().Select(c => c.FirstName)
                .SelectMany(c => ss.Set<FunkyCustomer>().Select(c2 => c2.LastName), (fn, ln) => new { fn, ln })
                .Where(r => r.fn.MaybeScalar(x => r.ln.MaybeScalar(xx => x.StartsWith(xx))) == true),
            elementSorter: e => (e.fn, e.ln),
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.fn, a.fn);
                Assert.Equal(e.ln, a.ln);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task String_starts_with_on_argument_with_wildcard_column_negated(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<FunkyCustomer>().Select(c => c.FirstName)
                .SelectMany(c => ss.Set<FunkyCustomer>().Select(c2 => c2.LastName), (fn, ln) => new { fn, ln })
                .Where(r => !r.fn.StartsWith(r.ln)),
            ss => ss.Set<FunkyCustomer>().Select(c => c.FirstName)
                .SelectMany(c => ss.Set<FunkyCustomer>().Select(c2 => c2.LastName), (fn, ln) => new { fn, ln })
                .Where(r => !(r.fn.MaybeScalar(x => r.ln.MaybeScalar(xx => x.StartsWith(xx))) == true)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task String_ends_with_on_argument_with_wildcard_constant(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.EndsWith("%r")).Select(c => c.FirstName),
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.MaybeScalar(x => x.EndsWith("%r")) == true).Select(c => c.FirstName));

        await AssertQuery(
            async,
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.EndsWith("r_")).Select(c => c.FirstName),
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.MaybeScalar(x => x.EndsWith("r_")) == true).Select(c => c.FirstName));

        await AssertQuery(
            async,
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.EndsWith(null)).Select(c => c.FirstName),
            ss => ss.Set<FunkyCustomer>().Where(c => false).Select(c => c.FirstName),
            assertEmpty: true);

        await AssertQuery(
            async,
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.EndsWith("")).Select(c => c.FirstName),
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName != null).Select(c => c.FirstName));

        await AssertQuery(
            async,
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.EndsWith("_r_")).Select(c => c.FirstName),
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.MaybeScalar(x => x.EndsWith("_r_")) == true).Select(c => c.FirstName));

        await AssertQuery(
            async,
            ss => ss.Set<FunkyCustomer>().Where(c => !c.FirstName.EndsWith("a%r%")).Select(c => c.FirstName),
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.MaybeScalar(x => x.EndsWith("a%r%")) != true).Select(c => c.FirstName));

        await AssertQuery(
            async,
            ss => ss.Set<FunkyCustomer>().Where(c => !c.FirstName.EndsWith("")).Select(c => c.FirstName),
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.MaybeScalar(x => x.EndsWith("")) != true).Select(c => c.FirstName));

        await AssertQuery(
            async,
            ss => ss.Set<FunkyCustomer>().Where(c => !c.FirstName.EndsWith(null)).Select(c => c.FirstName),
            ss => ss.Set<FunkyCustomer>().Where(c => true).Select(c => c.FirstName));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task String_ends_with_on_argument_with_wildcard_parameter(bool async)
    {
        var prm1 = "%r";
        await AssertQuery(
            async,
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.EndsWith(prm1)).Select(c => c.FirstName),
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.MaybeScalar(x => x.EndsWith(prm1)) == true).Select(c => c.FirstName));

        var prm2 = "r_";
        await AssertQuery(
            async,
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.EndsWith(prm2)).Select(c => c.FirstName),
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.MaybeScalar(x => x.EndsWith(prm2)) == true).Select(c => c.FirstName));

        var prm3 = (string)null;
        await AssertQuery(
            async,
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.EndsWith(prm3)).Select(c => c.FirstName),
            ss => ss.Set<FunkyCustomer>().Where(c => false).Select(c => c.FirstName),
            assertEmpty: true);

        var prm4 = "";
        await AssertQuery(
            async,
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.EndsWith(prm4)).Select(c => c.FirstName),
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.MaybeScalar(x => x.EndsWith(prm4)) == true).Select(c => c.FirstName));

        var prm5 = "_r_";
        await AssertQuery(
            async,
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.EndsWith(prm5)).Select(c => c.FirstName),
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.MaybeScalar(x => x.EndsWith(prm5)) == true).Select(c => c.FirstName));

        var prm6 = "a%r%";
        await AssertQuery(
            async,
            ss => ss.Set<FunkyCustomer>().Where(c => !c.FirstName.EndsWith(prm6)).Select(c => c.FirstName),
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.MaybeScalar(x => x.EndsWith(prm6)) != true).Select(c => c.FirstName));

        var prm7 = "";
        await AssertQuery(
            async,
            ss => ss.Set<FunkyCustomer>().Where(c => !c.FirstName.EndsWith(prm7)).Select(c => c.FirstName),
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName == null).Select(c => c.FirstName));

        var prm8 = (string)null;
        await AssertQuery(
            async,
            ss => ss.Set<FunkyCustomer>().Where(c => !c.FirstName.EndsWith(prm8)).Select(c => c.FirstName),
            ss => ss.Set<FunkyCustomer>().Where(c => true).Select(c => c.FirstName));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task String_ends_with_on_argument_with_wildcard_column(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<FunkyCustomer>().Select(c => c.FirstName)
                .SelectMany(c => ss.Set<FunkyCustomer>().Select(c2 => c2.LastName), (fn, ln) => new { fn, ln })
                .Where(r => r.fn.EndsWith(r.ln)),
            ss => ss.Set<FunkyCustomer>().Select(c => c.FirstName)
                .SelectMany(c => ss.Set<FunkyCustomer>().Select(c2 => c2.LastName), (fn, ln) => new { fn, ln })
                .Where(r => r.fn.MaybeScalar(x => r.ln.MaybeScalar(xx => x.EndsWith(xx))) == true),
            elementSorter: e => (e.fn, e.ln),
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.fn, a.fn);
                Assert.Equal(e.ln, a.ln);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task String_ends_with_on_argument_with_wildcard_column_negated(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<FunkyCustomer>().Select(c => c.FirstName)
                .SelectMany(c => ss.Set<FunkyCustomer>().Select(c2 => c2.LastName), (fn, ln) => new { fn, ln })
                .Where(r => !r.fn.EndsWith(r.ln)),
            ss => ss.Set<FunkyCustomer>().Select(c => c.FirstName)
                .SelectMany(c => ss.Set<FunkyCustomer>().Select(c2 => c2.LastName), (fn, ln) => new { fn, ln })
                .Where(r => !(r.fn.MaybeScalar(x => r.ln.MaybeScalar(xx => x.EndsWith(xx))) == true)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task String_ends_with_inside_conditional(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<FunkyCustomer>().Select(c => c.FirstName)
                .SelectMany(c => ss.Set<FunkyCustomer>().Select(c2 => c2.LastName), (fn, ln) => new { fn, ln })
                .Where(r => r.fn.EndsWith(r.ln) ? true : false),
            ss => ss.Set<FunkyCustomer>().Select(c => c.FirstName)
                .SelectMany(c => ss.Set<FunkyCustomer>().Select(c2 => c2.LastName), (fn, ln) => new { fn, ln })
                .Where(r => r.fn.MaybeScalar(x => r.ln.MaybeScalar(xx => x.EndsWith(xx))) == true),
            elementSorter: e => (e.fn, e.ln),
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.fn, a.fn);
                Assert.Equal(e.ln, a.ln);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task String_ends_with_inside_conditional_negated(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<FunkyCustomer>().Select(c => c.FirstName)
                .SelectMany(c => ss.Set<FunkyCustomer>().Select(c2 => c2.LastName), (fn, ln) => new { fn, ln })
                .Where(r => !r.fn.EndsWith(r.ln) ? true : false),
            ss => ss.Set<FunkyCustomer>().Select(c => c.FirstName)
                .SelectMany(c => ss.Set<FunkyCustomer>().Select(c2 => c2.LastName), (fn, ln) => new { fn, ln })
                .Where(
                    r => !(r.fn.MaybeScalar(x => r.ln.MaybeScalar(xx => x.EndsWith(xx))) == true)
                        ? true
                        : false));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task String_ends_with_equals_nullable_column(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<FunkyCustomer>().SelectMany(c => ss.Set<FunkyCustomer>(), (c1, c2) => new { c1, c2 })
                .Where(r => r.c1.FirstName.EndsWith(r.c2.LastName) == r.c1.NullableBool.Value),
            ss => ss.Set<FunkyCustomer>().SelectMany(c => ss.Set<FunkyCustomer>(), (c1, c2) => new { c1, c2 })
                .Where(
                    r => (r.c1.FirstName != null && r.c2.LastName != null && r.c1.FirstName.EndsWith(r.c2.LastName)) == r.c1.NullableBool),
            elementSorter: e => (e.c1.Id, e.c2.Id),
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.c1, a.c1);
                AssertEqual(e.c2, a.c2);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task String_ends_with_not_equals_nullable_column(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<FunkyCustomer>().SelectMany(c => ss.Set<FunkyCustomer>(), (c1, c2) => new { c1, c2 })
                .Where(r => r.c1.FirstName.EndsWith(r.c2.LastName) != r.c1.NullableBool.Value),
            ss => ss.Set<FunkyCustomer>().SelectMany(c => ss.Set<FunkyCustomer>(), (c1, c2) => new { c1, c2 })
                .Where(
                    r => (r.c1.FirstName != null && r.c2.LastName != null && r.c1.FirstName.EndsWith(r.c2.LastName)) != r.c1.NullableBool),
            elementSorter: e => (e.c1.Id, e.c2.Id),
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.c1, a.c1);
                AssertEqual(e.c2, a.c2);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task String_FirstOrDefault_and_LastOrDefault(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<FunkyCustomer>().OrderBy(e => e.Id).Select(
                e => new { first = (char?)e.FirstName.FirstOrDefault(), last = (char?)e.FirstName.LastOrDefault() }),
            ss => ss.Set<FunkyCustomer>().OrderBy(e => e.Id).Select(
                e => new
                {
                    first = e.FirstName.MaybeScalar(x => x.FirstOrDefault()), last = e.FirstName.MaybeScalar(x => x.LastOrDefault())
                }),
            assertOrder: true,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.first, a.first);
                AssertEqual(e.last, a.last);
            });


    [ConditionalTheory] // #32432
    [MemberData(nameof(IsAsyncData))]
    public virtual Task String_Contains_and_StartsWith_with_same_parameter(bool async)
    {
        var s = "B";

        return AssertQuery(
            async,
            ss => ss.Set<FunkyCustomer>().Where(
                c => c.FirstName.Contains(s) || c.LastName.StartsWith(s)),
            ss => ss.Set<FunkyCustomer>().Where(
                c => c.FirstName.MaybeScalar(f => f.Contains(s)) == true || c.LastName.MaybeScalar(l => l.StartsWith(s)) == true));
    }

    protected FunkyDataContext CreateContext()
        => Fixture.CreateContext();

    protected virtual void ClearLog()
    {
    }

    public abstract class FunkyDataQueryFixtureBase : SharedStoreFixtureBase<FunkyDataContext>, IQueryFixtureBase
    {
        public Func<DbContext> GetContextCreator()
            => () => CreateContext();

        public virtual ISetSource GetExpectedData()
            => FunkyDataData.Instance;

        public IReadOnlyDictionary<Type, object> EntitySorters { get; } =
            new Dictionary<Type, Func<object, object>> { { typeof(FunkyCustomer), e => ((FunkyCustomer)e)?.Id } }
                .ToDictionary(e => e.Key, e => (object)e.Value);

        public IReadOnlyDictionary<Type, object> EntityAsserters { get; } = new Dictionary<Type, Action<object, object>>
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

        protected override string StoreName
            => "FunkyDataQueryTest";

        public override FunkyDataContext CreateContext()
        {
            var context = base.CreateContext();
            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            return context;
        }

        protected override Task SeedAsync(FunkyDataContext context)
            => FunkyDataContext.SeedAsync(context);
    }
}
