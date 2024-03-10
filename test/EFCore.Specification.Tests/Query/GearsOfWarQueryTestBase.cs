// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel;
using Xunit.Sdk;

// ReSharper disable AccessToModifiedClosure
// ReSharper disable SimplifyConditionalTernaryExpression
// ReSharper disable ArgumentsStyleAnonymousFunction
// ReSharper disable ArgumentsStyleOther
// ReSharper disable PossibleMultipleEnumeration
// ReSharper disable EqualExpressionComparison
// ReSharper disable InconsistentNaming
// ReSharper disable AccessToDisposedClosure
// ReSharper disable StringEndsWithIsCultureSpecific
// ReSharper disable ReplaceWithSingleCallToSingle
// ReSharper disable once CheckNamespace

#pragma warning disable RCS1202 // Avoid NullReferenceException.

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class GearsOfWarQueryTestBase<TFixture> : QueryTestBase<TFixture>
    where TFixture : GearsOfWarQueryFixtureBase, new()
{
    protected GearsOfWarQueryTestBase(TFixture fixture)
        : base(fixture)
    {
    }

    protected override Expression RewriteExpectedQueryExpression(Expression expectedQueryExpression)
        => new ExpectedQueryRewritingVisitor(Fixture.GetShadowPropertyMappings())
            .Visit(expectedQueryExpression);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Entity_equality_empty(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>().Where(g => g == new Gear()),
            assertEmpty: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_multiple_one_to_one_and_one_to_many(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<CogTag>().Include(t => t.Gear.Weapons),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<CogTag>(t => t.Gear),
                new ExpectedInclude<Gear>(g => g.Weapons, "Gear"),
                new ExpectedInclude<Officer>(o => o.Weapons, "Gear")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task ToString_guid_property_projection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<CogTag>().Select(
                ct => new { A = ct.GearNickName, B = ct.Id.ToString() }),
            elementSorter: e => e.B,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.A, a.A);
                Assert.Equal(e.B.ToLower(), a.B.ToLower());
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task ToString_string_property_projection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Weapon>().Select(w => w.Name.ToString()));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task ToString_boolean_property_non_nullable(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Weapon>().Select(w => w.IsAutomatic.ToString()));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task ToString_boolean_property_nullable(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<LocustHorde>().Select(lh => lh.Eradicated.ToString()));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_multiple_one_to_one_and_one_to_many_self_reference(bool async)
        => Assert.ThrowsAsync<InvalidOperationException>(
            () => AssertQuery(async, ss => ss.Set<Weapon>().Include(w => w.Owner.Weapons)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_multiple_one_to_one_optional_and_one_to_one_required(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<CogTag>().Include(t => t.Gear.Squad),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<CogTag>(t => t.Gear),
                new ExpectedInclude<Gear>(g => g.Squad, "Gear"),
                new ExpectedInclude<Officer>(o => o.Squad, "Gear")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_multiple_one_to_one_and_one_to_one_and_one_to_many(bool async)
        => Assert.ThrowsAsync<InvalidOperationException>(
            () => AssertQuery(async, ss => ss.Set<CogTag>().Include(t => t.Gear.Squad.Members)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_multiple_circular(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>().Include(g => g.CityOfBirth.StationedGears),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<Gear>(g => g.CityOfBirth),
                new ExpectedInclude<Officer>(o => o.CityOfBirth),
                new ExpectedInclude<City>(c => c.StationedGears, "CityOfBirth")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_multiple_circular_with_filter(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>().Include(g => g.CityOfBirth.StationedGears).Where(g => g.Nickname == "Marcus"),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<Gear>(g => g.CityOfBirth),
                new ExpectedInclude<Officer>(o => o.CityOfBirth),
                new ExpectedInclude<City>(c => c.StationedGears, "CityOfBirth")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_using_alternate_key(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>().Include(g => g.Weapons).Where(g => g.Nickname == "Marcus"),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<Gear>(g => g.Weapons),
                new ExpectedInclude<Officer>(o => o.Weapons)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_multiple_include_then_include(bool async)
        => Assert.ThrowsAsync<InvalidOperationException>(
            () => AssertQuery(
                async,
                ss => ss.Set<Gear>()
                    .Include(g => g.AssignedCity.BornGears).ThenInclude(g => g.Tag)
                    .Include(g => g.AssignedCity.StationedGears).ThenInclude(g => g.Tag)
                    .Include(g => g.CityOfBirth.BornGears).ThenInclude(g => g.Tag)
                    .Include(g => g.CityOfBirth.StationedGears).ThenInclude(g => g.Tag)
                    .OrderBy(g => g.Nickname)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_navigation_on_derived_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>().OfType<Officer>().Include(o => o.Reports),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Officer>(o => o.Reports)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task String_based_Include_navigation_on_derived_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>().OfType<Officer>().Include("Reports"),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Officer>(o => o.Reports)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task EF_Property_based_Include_navigation_on_derived_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>().OfType<Officer>().Include(o => EF.Property<Officer>(o, "Reports")),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Officer>(o => o.Reports)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_Where_Navigation_Included(bool async)
        => AssertQuery(
            async,
            ss => from t in ss.Set<CogTag>().Include(o => o.Gear)
                  where t.Gear.Nickname == "Marcus"
                  select t,
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<CogTag>(t => t.Gear)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_with_join_reference1(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>().Join(
                ss.Set<CogTag>(),
                g => new { SquadId = (int?)g.SquadId, g.Nickname },
                t => new { SquadId = t.GearSquadId, Nickname = t.GearNickName },
                (g, t) => g).Include(g => g.CityOfBirth),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<Gear>(g => g.CityOfBirth),
                new ExpectedInclude<Officer>(o => o.CityOfBirth)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_with_join_reference2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<CogTag>().Join(
                ss.Set<Gear>(),
                t => new { SquadId = t.GearSquadId, Nickname = t.GearNickName },
                g => new { SquadId = (int?)g.SquadId, g.Nickname },
                (t, g) => g).Include(g => g.CityOfBirth),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<Gear>(g => g.CityOfBirth),
                new ExpectedInclude<Officer>(o => o.CityOfBirth)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_with_join_collection1(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>().Join(
                ss.Set<CogTag>(),
                g => new { SquadId = (int?)g.SquadId, g.Nickname },
                t => new { SquadId = t.GearSquadId, Nickname = t.GearNickName },
                (g, t) => g).Include(g => g.Weapons),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<Gear>(g => g.Weapons),
                new ExpectedInclude<Officer>(o => o.Weapons)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_with_join_collection2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<CogTag>().Join(
                ss.Set<Gear>(),
                t => new { SquadId = t.GearSquadId, Nickname = t.GearNickName },
                g => new { SquadId = (int?)g.SquadId, g.Nickname },
                (t, g) => g).Include(g => g.Weapons),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<Gear>(g => g.Weapons),
                new ExpectedInclude<Officer>(o => o.Weapons)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Include_where_list_contains_navigation(bool async)
    {
        using var context = CreateContext();
        context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;

        var tags = context.Tags.Select(t => (Guid?)t.Id).ToList();

        var query = context.Gears
            .Include(g => g.Tag)
            .Where(g => g.Tag != null && tags.Contains(g.Tag.Id));

        var gears = async
            ? (await query.ToListAsync())
            : query.ToList();

        Assert.Equal(5, gears.Count);

        context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Include_where_list_contains_navigation2(bool async)
    {
        using var context = CreateContext();
        context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;

        var tags = context.Tags.Select(t => (Guid?)t.Id).ToList();

        var query = context.Gears
            .Include(g => g.Tag)
            .Where(g => g.CityOfBirth.Location != null && tags.Contains(g.Tag.Id));

        var gears = async
            ? (await query.ToListAsync())
            : query.ToList();

        Assert.Equal(5, gears.Count);

        context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Navigation_accessed_twice_outside_and_inside_subquery(bool async)
    {
        using var context = CreateContext();
        context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;

        var tags = context.Tags.Select(t => (Guid?)t.Id).ToList();

        var query = context.Gears
            .Where(g => g.Tag != null && tags.Contains(g.Tag.Id));

        var gears = async
            ? (await query.ToListAsync())
            : query.ToList();

        Assert.Equal(5, gears.Count);

        context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_with_join_multi_level(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>().Join(
                ss.Set<CogTag>(),
                g => new { SquadId = (int?)g.SquadId, g.Nickname },
                t => new { SquadId = t.GearSquadId, Nickname = t.GearNickName },
                (g, t) => g).Include(g => g.CityOfBirth.StationedGears),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<Gear>(g => g.CityOfBirth),
                new ExpectedInclude<Officer>(o => o.CityOfBirth),
                new ExpectedInclude<City>(c => c.StationedGears, "CityOfBirth")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_with_join_and_inheritance1(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<CogTag>().Join(
                ss.Set<Gear>().OfType<Officer>(),
                t => new { SquadId = t.GearSquadId, Nickname = t.GearNickName },
                o => new { SquadId = (int?)o.SquadId, o.Nickname },
                (t, o) => o).Include(o => o.CityOfBirth),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Officer>(o => o.CityOfBirth)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_with_join_and_inheritance_with_orderby_before_and_after_include(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<CogTag>().Join(
                    ss.Set<Gear>().OfType<Officer>().OrderBy(ee => ee.SquadId),
                    t => new { SquadId = t.GearSquadId, Nickname = t.GearNickName },
                    o => new { SquadId = (int?)o.SquadId, o.Nickname },
                    (t, o) => o).OrderBy(ee => ee.FullName).Include(o => o.Reports).OrderBy(oo => oo.HasSoulPatch)
                .ThenByDescending(oo => oo.Nickname),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Officer>(o => o.Reports)),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_with_join_and_inheritance2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>().OfType<Officer>().Join(
                ss.Set<CogTag>(),
                o => new { SquadId = (int?)o.SquadId, o.Nickname },
                t => new { SquadId = t.GearSquadId, Nickname = t.GearNickName },
                (o, t) => o).Include(g => g.Weapons),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Officer>(o => o.Weapons)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_with_join_and_inheritance3(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<CogTag>().Join(
                ss.Set<Gear>().OfType<Officer>(),
                t => new { SquadId = t.GearSquadId, Nickname = t.GearNickName },
                g => new { SquadId = (int?)g.SquadId, g.Nickname },
                (t, o) => o).Include(o => o.Reports),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Officer>(o => o.Reports)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_with_nested_navigation_in_order_by(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Weapon>()
                .Include(w => w.Owner)
                .Where(w => w.Owner.Nickname != "Paduk")
                .OrderBy(e => e.Owner.CityOfBirth.Name).ThenBy(e => e.Id),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Weapon>(w => w.Owner)),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_enum(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>().Where(g => g.Rank == MilitaryRank.Sergeant));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_nullable_enum_with_constant(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Weapon>().Where(w => w.AmmunitionType == AmmunitionType.Cartridge));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_nullable_enum_with_null_constant(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Weapon>().Where(w => w.AmmunitionType == null));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_nullable_enum_with_non_nullable_parameter(bool async)
    {
        var ammunitionType = AmmunitionType.Cartridge;

        return AssertQuery(
            async,
            ss => ss.Set<Weapon>().Where(w => w.AmmunitionType == ammunitionType));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Where_nullable_enum_with_nullable_parameter(bool async)
    {
        AmmunitionType? ammunitionType = AmmunitionType.Cartridge;

        await AssertQuery(
            async,
            ss => ss.Set<Weapon>().Where(w => w.AmmunitionType == ammunitionType));

        ammunitionType = null;

        await AssertQuery(
            async,
            ss => ss.Set<Weapon>().Where(w => w.AmmunitionType == ammunitionType));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Where_bitwise_and_enum(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Gear>().Where(g => (g.Rank & MilitaryRank.Corporal) > 0));

        await AssertQuery(
            async,
            ss => ss.Set<Gear>().Where(g => (g.Rank & MilitaryRank.Corporal) == MilitaryRank.Corporal));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Where_bitwise_and_integral(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Gear>().Where(g => ((int)g.Rank & 1) == 1));

        await AssertQuery(
            async,
            ss => ss.Set<Gear>().Where(g => ((long)g.Rank & 1L) == 1L));

        await AssertQuery(
            async,
            ss => ss.Set<Gear>().Where(g => ((short)g.Rank & 1) == 1));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_bitwise_and_nullable_enum_with_constant(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Weapon>().Where(w => (w.AmmunitionType & AmmunitionType.Cartridge) > 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_bitwise_and_nullable_enum_with_null_constant(bool async)
    {
        return AssertQuery(
            async,
#pragma warning disable CS0458 // The result of the expression is always 'null'
            ss => ss.Set<Weapon>().Where(w => (w.AmmunitionType & null) > 0),
#pragma warning restore CS0458 // The result of the expression is always 'null'
            assertEmpty: true);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_bitwise_and_nullable_enum_with_non_nullable_parameter(bool async)
    {
        var ammunitionType = AmmunitionType.Cartridge;

        return AssertQuery(
            async,
            ss => ss.Set<Weapon>().Where(w => (w.AmmunitionType & ammunitionType) > 0));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Where_bitwise_and_nullable_enum_with_nullable_parameter(bool async)
    {
        AmmunitionType? ammunitionType = AmmunitionType.Cartridge;

        await AssertQuery(
            async,
            ss => ss.Set<Weapon>().Where(w => (w.AmmunitionType & ammunitionType) > 0));

        ammunitionType = null;

        await AssertQuery(
            async,
            ss => ss.Set<Weapon>().Where(w => (w.AmmunitionType & ammunitionType) > 0),
            assertEmpty: true);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_bitwise_or_enum(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>().Where(g => (g.Rank | MilitaryRank.Corporal) > 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Bitwise_projects_values_in_select(bool async)
        => AssertFirst(
            async,
            ss => ss.Set<Gear>()
                .Where(g => (g.Rank & MilitaryRank.Corporal) == MilitaryRank.Corporal)
                .Select(
                    b => new
                    {
                        BitwiseTrue = (b.Rank & MilitaryRank.Corporal) == MilitaryRank.Corporal,
                        BitwiseFalse = (b.Rank & MilitaryRank.Corporal) == MilitaryRank.Sergeant,
                        BitwiseValue = b.Rank & MilitaryRank.Corporal
                    }));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Where_enum_has_flag(bool async)
    {
        // Constant
        await AssertQuery(
            async,
            ss => ss.Set<Gear>().Where(g => g.Rank.HasFlag(MilitaryRank.Corporal)));

        // Expression
        await AssertQuery(
            async,
            ss => ss.Set<Gear>().Where(g => g.Rank.HasFlag(MilitaryRank.Corporal | MilitaryRank.Captain)),
            assertEmpty: true);

        // Casting
        await AssertQuery(
            async,
            ss => ss.Set<Gear>().Where(g => g.Rank.HasFlag((MilitaryRank)1)));

        // Casting to nullable
        await AssertQuery(
            async,
            ss => ss.Set<Gear>().Where(g => g.Rank.HasFlag((MilitaryRank?)1)));

        // QuerySource
        await AssertQuery(
            async,
            ss => ss.Set<Gear>().Where(g => MilitaryRank.Corporal.HasFlag(g.Rank)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Where_enum_has_flag_subquery(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Gear>().Where(
                g => g.Rank.HasFlag(
                    ss.Set<Gear>().OrderBy(x => x.Nickname).ThenBy(x => x.SquadId).Select(x => x.Rank).FirstOrDefault())));

        await AssertQuery(
            async,
            ss => ss.Set<Gear>().Where(
                g => MilitaryRank.Corporal.HasFlag(
                    ss.Set<Gear>().OrderBy(x => x.Nickname).ThenBy(x => x.SquadId).Select(x => x.Rank).FirstOrDefault())));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Where_enum_has_flag_subquery_with_pushdown(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Gear>().Where(
                g => g.Rank.HasFlag(ss.Set<Gear>().OrderBy(x => x.Nickname).ThenBy(x => x.SquadId).FirstOrDefault().Rank)));

        await AssertQuery(
            async,
            ss => ss.Set<Gear>().Where(
                g => MilitaryRank.Corporal.HasFlag(
                    ss.Set<Gear>().OrderBy(x => x.Nickname).ThenBy(x => x.SquadId).FirstOrDefault().Rank)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_enum_has_flag_subquery_client_eval(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>().Where(
                g => g.Rank.HasFlag(ss.Set<Gear>().OrderBy(x => x.Nickname).ThenBy(x => x.SquadId).First().Rank)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_enum_has_flag_with_non_nullable_parameter(bool async)
    {
        var parameter = MilitaryRank.Corporal;

        return AssertQuery(
            async,
            ss => ss.Set<Gear>().Where(g => g.Rank.HasFlag(parameter)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_has_flag_with_nullable_parameter(bool async)
    {
        MilitaryRank? parameter = MilitaryRank.Corporal;

        return AssertQuery(
            async,
            ss => ss.Set<Gear>().Where(g => g.Rank.HasFlag(parameter)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_enum_has_flag(bool async)
        => AssertFirst(
            async,
            ss => ss.Set<Gear>()
                .Where(g => g.Rank.HasFlag(MilitaryRank.Corporal))
                .Select(
                    b => new
                    {
                        hasFlagTrue = b.Rank.HasFlag(MilitaryRank.Corporal), hasFlagFalse = b.Rank.HasFlag(MilitaryRank.Sergeant)
                    }));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_count_subquery_without_collision(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>().Where(w => w.Weapons.Count == 2));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_any_subquery_without_collision(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>().Where(w => w.Weapons.Any()));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_inverted_boolean(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Weapon>()
                .Where(w => w.IsAutomatic)
                .Select(
                    w => new { w.Id, Manual = !w.IsAutomatic }),
            elementSorter: e => e.Id);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Select_comparison_with_null(bool async)
    {
        AmmunitionType? ammunitionType = AmmunitionType.Cartridge;

        await AssertQuery(
            async,
            ss => ss.Set<Weapon>()
                .Where(w => w.AmmunitionType == ammunitionType)
                .Select(
                    w => new { w.Id, Cartridge = w.AmmunitionType == ammunitionType }),
            elementSorter: e => e.Id);

        ammunitionType = null;

        await AssertQuery(
            async,
            ss => ss.Set<Weapon>()
                .Where(w => w.AmmunitionType == ammunitionType)
                .Select(
                    w => new { w.Id, Cartridge = w.AmmunitionType == ammunitionType }),
            elementSorter: e => e.Id);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Select_null_parameter(bool async)
    {
        AmmunitionType? ammunitionType = AmmunitionType.Cartridge;

        await AssertQuery(
            async,
            ss => ss.Set<Weapon>()
                .Select(
                    w => new { w.Id, AmmoType = ammunitionType }),
            elementSorter: e => e.Id);

        ammunitionType = null;

        await AssertQuery(
            async,
            ss => ss.Set<Weapon>()
                .Select(
                    w => new { w.Id, AmmoType = ammunitionType }),
            elementSorter: e => e.Id);

        ammunitionType = AmmunitionType.Shell;

        await AssertQuery(
            async,
            ss => ss.Set<Weapon>()
                .Select(
                    w => new { w.Id, AmmoType = ammunitionType }),
            elementSorter: e => e.Id);

        ammunitionType = null;

        await AssertQuery(
            async,
            ss => ss.Set<Weapon>()
                .Select(
                    w => new { w.Id, AmmoType = ammunitionType }),
            elementSorter: e => e.Id);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_ternary_operation_with_boolean(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Weapon>().Select(
                w => new { w.Id, Num = w.IsAutomatic ? 1 : 0 }),
            elementSorter: e => e.Id);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_ternary_operation_with_inverted_boolean(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Weapon>().Select(
                w => new { w.Id, Num = !w.IsAutomatic ? 1 : 0 }),
            elementSorter: e => e.Id);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_ternary_operation_with_has_value_not_null(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Weapon>()
                .Where(w => w.AmmunitionType.HasValue && w.AmmunitionType == AmmunitionType.Cartridge)
                .Select(
                    w => new
                    {
                        w.Id,
                        IsCartridge = w.AmmunitionType.HasValue && w.AmmunitionType.Value == AmmunitionType.Cartridge ? "Yes" : "No"
                    }),
            elementSorter: e => e.Id);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_ternary_operation_multiple_conditions(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Weapon>().Select(
                w => new { w.Id, IsCartridge = w.AmmunitionType == AmmunitionType.Shell && w.SynergyWithId == 1 ? "Yes" : "No" }),
            elementSorter: e => e.Id);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_ternary_operation_multiple_conditions_2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Weapon>().Select(
                w => new { w.Id, IsCartridge = !w.IsAutomatic && w.SynergyWithId == 1 ? "Yes" : "No" }),
            elementSorter: e => e.Id);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_multiple_conditions(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Weapon>().Select(
                w => new { w.Id, IsCartridge = !w.IsAutomatic && w.SynergyWithId == 1 }),
            elementSorter: e => e.Id);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_nested_ternary_operations(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Weapon>().Select(
                w => new
                {
                    w.Id,
                    IsManualCartridge = !w.IsAutomatic
                        ? w.AmmunitionType == AmmunitionType.Cartridge ? "ManualCartridge" : "Manual"
                        : "Auto"
                }),
            elementSorter: e => e.Id);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Null_propagation_optimization1(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>().Where(g => (g == null ? null : g.LeaderNickname) == "Marcus" == (bool?)true));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Null_propagation_optimization2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>().Where(
                g => (g.LeaderNickname == null ? null : g.LeaderNickname.EndsWith("us")) == true));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Null_propagation_optimization3(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>().Where(
                g => (g.LeaderNickname != null ? g.LeaderNickname.EndsWith("us") : null) == true));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Null_propagation_optimization4(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>().Where(
                g => (null == EF.Property<string>(g, "LeaderNickname") ? null : g.LeaderNickname.Length) == 5 == (bool?)true));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Null_propagation_optimization5(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>().Where(
                g => (null != g.LeaderNickname ? EF.Property<string>(g, "LeaderNickname").Length : null)
                    == 5
                    == (bool?)true));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Null_propagation_optimization6(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>().Where(
                g => (null != g.LeaderNickname ? EF.Property<string>(g, "LeaderNickname").Length : null)
                    == 5
                    == (bool?)true));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_null_propagation_optimization7(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>().Select(g => null != g.LeaderNickname ? g.LeaderNickname + g.LeaderNickname : null));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_null_propagation_optimization8(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>().Select(g => g != null ? g.LeaderNickname + g.LeaderNickname : null));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_null_propagation_optimization9(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Gear>().Select(g => g != null ? g.FullName.Length : (int?)null));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_null_propagation_negative1(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Gear>().Select(g => g.LeaderNickname != null ? g.Nickname.Length == 5 : (bool?)null));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_null_propagation_negative2(bool async)
        => AssertQuery(
            async,
            ss => from g1 in ss.Set<Gear>()
                  from g2 in ss.Set<Gear>()
                  select g1.LeaderNickname != null ? g2.LeaderNickname : null);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_null_propagation_negative3(bool async)
        => AssertQuery(
            async,
            ss => from g1 in ss.Set<Gear>()
                  join g2 in ss.Set<Gear>() on g1.HasSoulPatch equals true into grouping
                  from g2 in grouping.DefaultIfEmpty()
                  orderby g2.Nickname
                  select new { g2.Nickname, Condition = g2 != null ? g2.LeaderNickname != null : (bool?)null },
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_null_propagation_negative4(bool async)
        => AssertQuery(
            async,
            ss => from g1 in ss.Set<Gear>()
                  join g2 in ss.Set<Gear>() on g1.HasSoulPatch equals true into grouping
                  from g2 in grouping.DefaultIfEmpty()
                  orderby g2.Nickname
                  select g2 != null ? new Tuple<string, int>(g2.Nickname, 5) : null,
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_null_propagation_negative5(bool async)
        => AssertQuery(
            async,
            ss => from g1 in ss.Set<Gear>()
                  join g2 in ss.Set<Gear>() on g1.HasSoulPatch equals true into grouping
                  from g2 in grouping.DefaultIfEmpty()
                  orderby g2.Nickname
                  select g2 != null
                      ? new { g2.Nickname, Five = 5 }
                      : null,
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_null_propagation_negative6(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Gear>().Select(
                g => null != g.LeaderNickname
                    ? EF.Property<string>(g, "LeaderNickname").Length != EF.Property<string>(g, "LeaderNickname").Length
                    : (bool?)null));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_null_propagation_negative7(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Gear>().Select(g => null != g.LeaderNickname ? g.LeaderNickname == g.LeaderNickname : (bool?)null));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_null_propagation_negative8(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<CogTag>().Select(t => t.Gear.Squad != null ? t.Gear.AssignedCity.Name : null));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_null_propagation_negative9(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Gear>().Select(
                g => g.LeaderNickname != null
                    ? (bool?)(g.Nickname.Length == 5) ?? default
                    : (bool?)null));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_null_propagation_works_for_navigations_with_composite_keys(bool async)
    {
        return AssertQuery(
            async,
            ss => from t in ss.Set<CogTag>()
#pragma warning disable IDE0031 // Use null propagation
                  select t.Gear != null ? t.Gear.Nickname : null);
#pragma warning restore IDE0031 // Use null propagation
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_null_propagation_works_for_multiple_navigations_with_composite_keys(bool async)
        => AssertQuery(
            async,
            ss => from t in ss.Set<CogTag>()
                  select EF.Property<City>(EF.Property<CogTag>(t.Gear, "Tag").Gear, "AssignedCity") != null
                      ? EF.Property<string>(EF.Property<Gear>(t.Gear.Tag, "Gear").AssignedCity, "Name")
                      : null);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_conditional_with_anonymous_type_and_null_constant(bool async)
        => AssertQuery(
            async,
            ss => from g in ss.Set<Gear>()
                  orderby g.Nickname
                  select g.LeaderNickname != null
                      ? new { g.HasSoulPatch }
                      : null,
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_conditional_with_anonymous_types(bool async)
        => AssertQuery(
            async,
            ss => from g in ss.Set<Gear>()
                  orderby g.Nickname
                  select g.LeaderNickname != null
                      ? new { Name = g.Nickname }
                      : new { Name = g.FullName },
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_conditional_equality_1(bool async)
        => AssertQuery(
            async,
            ss => from g in ss.Set<Gear>()
                  orderby g.Nickname
                  where (g.LeaderNickname != null
                          ? g.HasSoulPatch
                          : null)
                      == null
                  select g.Nickname,
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_conditional_equality_2(bool async)
        => AssertQuery(
            async,
            ss => from g in ss.Set<Gear>()
                  orderby g.Nickname
                  where (g.LeaderNickname == null
                          ? null
                          : g.HasSoulPatch)
                      == null
                  select g.Nickname,
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_conditional_equality_3(bool async)
        => AssertQuery(
            async,
            ss => from g in ss.Set<Gear>()
                  orderby g.Nickname
                  where (g.LeaderNickname != null
                          ? (int?)null
                          : null)
                      == null
                  select g.Nickname,
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_coalesce_with_anonymous_types(bool async)
        => AssertQuery(
            async,
            ss => from g in ss.Set<Gear>()
                  orderby g.Nickname
                  select new { Name = g.LeaderNickname } ?? new { Name = g.FullName },
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_coalesce_with_anonymous_types(bool async)
        => AssertQuery(
            async,
            ss => from g in ss.Set<Gear>()
                  where (new { Name = g.LeaderNickname } ?? new { Name = g.FullName }) != null
                  select g.Nickname);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_compare_anonymous_types(bool async)
        => AssertQuery(
            async,
            ss => from g in ss.Set<Gear>()
                  from o in ss.Set<Gear>().OfType<Officer>()
                  where new
                      {
                          Name = g.LeaderNickname,
                          Squad = g.LeaderSquadId,
                          Five = 5
                      }
                      == new
                      {
                          Name = o.Nickname,
                          Squad = o.SquadId,
                          Five = 5
                      }
                  select g.Nickname,
            ss => from g in ss.Set<Gear>()
                  from o in ss.Set<Gear>().OfType<Officer>()
                  where g.LeaderNickname == o.Nickname && g.LeaderSquadId == o.SquadId
                  select g.Nickname);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_member_access_on_anonymous_type(bool async)
        => AssertQuery(
            async,
            ss => from g in ss.Set<Gear>()
                  where new { Name = g.LeaderNickname, Squad = g.LeaderSquadId }.Name == "Marcus"
                  select g.Nickname);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_compare_anonymous_types_with_uncorrelated_members(bool async)
        => AssertQuery(
            async,
            // ReSharper disable once EqualExpressionComparison
            ss => from g in ss.Set<Gear>()
                  where new { Five = 5 } == new { Five = 5 }
                  select g.Nickname,
            assertEmpty: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_Where_Navigation(bool async)
        => AssertQuery(
            async,
            ss => from t in ss.Set<CogTag>()
                  where t.Gear.Nickname == "Marcus"
                  select t);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_Where_Navigation_Scalar_Equals_Navigation_Scalar(bool async)
        => AssertQuery(
            async,
            ss => from t1 in ss.Set<CogTag>()
                  from t2 in ss.Set<CogTag>()
                  where t1.Gear.Nickname == t2.Gear.Nickname
                  select new { Tag1 = t1, Tag2 = t2 },
            elementSorter: e => (e.Tag1.Id, e.Tag2.Id),
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.Tag1, a.Tag1);
                AssertEqual(e.Tag2, a.Tag2);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_Where_Navigation_Scalar_Equals_Navigation_Scalar_Projected(bool async)
        => AssertQuery(
            async,
            ss => from t1 in ss.Set<CogTag>()
                  from t2 in ss.Set<CogTag>()
                  where t1.Gear.Nickname == t2.Gear.Nickname
                  select new { Id1 = t1.Id, Id2 = t2.Id },
            elementSorter: e => (e.Id1, e.Id2));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Optional_Navigation_Null_Coalesce_To_Clr_Type(bool async)
        => AssertFirst(
            async,
            ss => ss.Set<Weapon>().OrderBy(w => w.Id).Select(
                w => new Weapon { IsAutomatic = (bool?)w.SynergyWith.IsAutomatic ?? false }));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_subquery_boolean(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>().Where(g => g.Weapons.OrderBy(w => w.Id).Select(w => w.IsAutomatic).FirstOrDefault()));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_subquery_boolean_with_pushdown(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>().Where(g => g.Weapons.OrderBy(w => w.Id).FirstOrDefault().IsAutomatic));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_subquery_distinct_firstordefault_boolean(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>().Where(
                g => g.HasSoulPatch && g.Weapons.Distinct().OrderBy(w => w.Id).Select(w => w.IsAutomatic).FirstOrDefault()));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_subquery_distinct_firstordefault_boolean_with_pushdown(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>().Where(g => g.HasSoulPatch && g.Weapons.Distinct().OrderBy(w => w.Id).FirstOrDefault().IsAutomatic));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_subquery_distinct_first_boolean(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>().OrderBy(g => g.Nickname)
                .Where(g => g.HasSoulPatch && g.Weapons.Distinct().OrderBy(w => w.Id).First().IsAutomatic),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_subquery_distinct_singleordefault_boolean1(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>().OrderBy(g => g.Nickname).Where(
                g => g.HasSoulPatch
                    && g.Weapons.Where(w => w.Name.Contains("Lancer")).Distinct().Select(w => w.IsAutomatic)
                        .SingleOrDefault()),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_subquery_distinct_singleordefault_boolean2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>().OrderBy(g => g.Nickname).Where(
                g => g.HasSoulPatch
                    && g.Weapons.Where(w => w.Name.Contains("Lancer")).Select(w => w.IsAutomatic).Distinct()
                        .SingleOrDefault()),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_subquery_distinct_singleordefault_boolean_with_pushdown(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>().OrderBy(g => g.Nickname).Where(
                g => g.HasSoulPatch && g.Weapons.Where(w => w.Name.Contains("Lancer")).Distinct().SingleOrDefault().IsAutomatic),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_subquery_distinct_lastordefault_boolean(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>()
                .OrderBy(g => g.Nickname)
                .Where(g => !g.Weapons.Distinct().OrderBy(w => w.Id).LastOrDefault().IsAutomatic),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_subquery_distinct_last_boolean(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>()
                .OrderBy(g => g.Nickname)
                .Where(g => !g.HasSoulPatch && g.Weapons.Distinct().OrderBy(w => w.Id).Last().IsAutomatic),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_subquery_distinct_orderby_firstordefault_boolean(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>().Where(
                g => g.HasSoulPatch && g.Weapons.Distinct().OrderBy(w => w.Id).Select(w => w.IsAutomatic).FirstOrDefault()));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_subquery_distinct_orderby_firstordefault_boolean_with_pushdown(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>().Where(g => g.HasSoulPatch && g.Weapons.Distinct().OrderBy(w => w.Id).FirstOrDefault().IsAutomatic));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_subquery_union_firstordefault_boolean(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>().Where(
                g => g.HasSoulPatch && g.Weapons.Union(g.Weapons).OrderBy(w => w.Id).FirstOrDefault().IsAutomatic));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_subquery_join_firstordefault_boolean(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>().Where(
                g => g.HasSoulPatch
                    && g.Weapons.Join(g.Weapons, e => e.Id, e => e.Id, (e1, e2) => e1).OrderBy(w => w.Id).FirstOrDefault()
                        .IsAutomatic));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_subquery_left_join_firstordefault_boolean(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>().Where(
                g => g.HasSoulPatch
                    && (from o in g.Weapons
                        join i in g.Weapons on o.Id equals i.Id into grouping
                        from i in grouping.DefaultIfEmpty()
                        select o).OrderBy(w => w.Id).FirstOrDefault().IsAutomatic));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_subquery_concat_firstordefault_boolean(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>().Where(
                g => g.HasSoulPatch && g.Weapons.Concat(g.Weapons).OrderBy(w => w.Id).FirstOrDefault().IsAutomatic));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Concat_with_count(bool async)
        => AssertCount(
            async,
            ss => ss.Set<Gear>().Concat(ss.Set<Gear>()));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Concat_scalars_with_count(bool async)
        => AssertCount(
            async,
            ss => ss.Set<Gear>().Select(g => g.Nickname).Concat(ss.Set<Gear>().Select(g2 => g2.FullName)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Concat_anonymous_with_count(bool async)
        => AssertCount(
            async,
            ss => ss.Set<Gear>()
                .Select(
                    g => new { Gear = g, Name = g.Nickname })
                .Concat(
                    ss.Set<Gear>().Select(
                        g2 => new { Gear = g2, Name = g2.FullName })));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Concat_with_scalar_projection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>().Concat(ss.Set<Gear>()).Select(g => g.Nickname));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_navigation_with_concat_and_count(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Gear>().Where(g => !g.HasSoulPatch).Select(g => g.Weapons.Concat(g.Weapons).Count()));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Concat_with_collection_navigations(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Gear>().Where(g => g.HasSoulPatch).Select(g => g.Weapons.Union(g.Weapons).Count()));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Union_with_collection_navigations(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Gear>().OfType<Officer>().Select(o => o.Reports.Union(o.Reports).Count()));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_subquery_distinct_firstordefault(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>().Where(g => g.HasSoulPatch).Select(g => g.Weapons.Distinct().OrderBy(w => w.Id).FirstOrDefault().Name));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_Where_Navigation_Client(bool async)
        => AssertTranslationFailedWithDetails(
            () => AssertQuery(
                async,
                ss => from t in ss.Set<CogTag>()
                      where t.Gear != null && t.Gear.IsMarcus
                      select t),
            CoreStrings.QueryUnableToTranslateMember(nameof(Gear.IsMarcus), nameof(Gear)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_Where_Navigation_Null(bool async)
        => AssertQuery(
            async,
            ss => from t in ss.Set<CogTag>()
                  where t.Gear == null
                  select t);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_Where_Navigation_Null_Reverse(bool async)
        => AssertQuery(
            async,
            ss => from t in ss.Set<CogTag>()
                  where null == t.Gear
                  select t);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_Where_Navigation_Equals_Navigation(bool async)
        => AssertQuery(
            async,
            ss => from t1 in ss.Set<CogTag>()
                  from t2 in ss.Set<CogTag>()
                  where t1.Gear == t2.Gear
                  select new { t1, t2 },
            elementSorter: e => e.t1.Id + " " + e.t2.Id,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.t1, a.t1);
                AssertEqual(e.t2, e.t2);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Singleton_Navigation_With_Member_Access(bool async)
        => AssertQuery(
            async,
            ss => from ct in ss.Set<CogTag>()
                  where ct.Gear.Nickname == "Marcus"
                  where ct.Gear.CityOfBirthName != "Ephyra"
                  select new { B = ct.Gear.CityOfBirthName },
            elementSorter: e => e.B);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_Singleton_Navigation_With_Member_Access(bool async)
        => AssertQuery(
            async,
            ss => from ct in ss.Set<CogTag>()
                  where ct.Gear.Nickname == "Marcus"
                  where ct.Gear.CityOfBirthName != "Ephyra"
                  select new { A = ct.Gear, B = ct.Gear.CityOfBirthName },
            elementSorter: e => e.A.Nickname,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.A, a.A);
                Assert.Equal(e.B, e.B);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GroupJoin_Composite_Key(bool async)
        => AssertQuery(
            async,
            ss =>
                from t in ss.Set<CogTag>()
                join g in ss.Set<Gear>()
                    on new { N = t.GearNickName, S = t.GearSquadId }
                    equals new { N = g.Nickname, S = (int?)g.SquadId } into grouping
                from g in grouping
                select g);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Join_navigation_translated_to_subquery_composite_key(bool async)
        => AssertQuery(
            async,
            ss => from g in ss.Set<Gear>()
                  join t in ss.Set<CogTag>() on g.FullName equals t.Gear.FullName
                  select new { g.FullName, t.Note },
            elementSorter: e => e.FullName);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Join_with_order_by_on_inner_sequence_navigation_translated_to_subquery_composite_key(bool async)
        => AssertQuery(
            async,
            ss => from g in ss.Set<Gear>()
                  join t in ss.Set<CogTag>().OrderBy(tt => tt.Id) on g.FullName equals t.Gear.FullName
                  select new { g.FullName, t.Note },
            elementSorter: e => e.FullName);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Join_with_order_by_without_skip_or_take(bool async)
        => AssertQuery(
            async,
            ss => from g in ss.Set<Gear>()
                  join w in ss.Set<Weapon>().OrderBy(ww => ww.Name) on g.FullName equals w.OwnerFullName
                  select new { w.Name, g.FullName },
            elementSorter: w => w.Name);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Join_with_order_by_without_skip_or_take_nested(bool async)
        => AssertQuery(
            async,
            ss => from s in ss.Set<Squad>()
                  join g in ss.Set<Gear>().OrderByDescending(gg => gg.SquadId) on s.Id equals g.SquadId
                  join w in ss.Set<Weapon>().OrderBy(ww => ww.Name) on g.FullName equals w.OwnerFullName
                  select new { w.Name, g.FullName },
            elementSorter: w => w.Name);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Collection_with_inheritance_and_join_include_joined(bool async)
        => AssertQuery(
            async,
            ss => (from t in ss.Set<CogTag>()
                   join g in ss.Set<Gear>().OfType<Officer>() on new { id1 = t.GearSquadId, id2 = t.GearNickName }
                       equals new { id1 = (int?)g.SquadId, id2 = g.Nickname }
                   select g).Include(g => g.Tag),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Officer>(o => o.Tag)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Collection_with_inheritance_and_join_include_source(bool async)
        => AssertQuery(
            async,
            ss => (from g in ss.Set<Gear>().OfType<Officer>()
                   join t in ss.Set<CogTag>() on new { id1 = (int?)g.SquadId, id2 = g.Nickname }
                       equals new { id1 = t.GearSquadId, id2 = t.GearNickName }
                   select g).Include(g => g.Tag),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Officer>(o => o.Tag)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Non_unicode_string_literal_is_used_for_non_unicode_column(bool async)
        => AssertQuery(
            async,
            ss => from c in ss.Set<City>()
                  where c.Location == "Unknown"
                  select c);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Non_unicode_string_literal_is_used_for_non_unicode_column_right(bool async)
        => AssertQuery(
            async,
            ss => from c in ss.Set<City>()
                  where "Unknown" == c.Location
                  select c);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Non_unicode_parameter_is_used_for_non_unicode_column(bool async)
    {
        var value = "Unknown";

        return AssertQuery(
            async,
            ss => from c in ss.Set<City>()
                  where c.Location == value
                  select c);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Non_unicode_string_literals_in_contains_is_used_for_non_unicode_column(bool async)
    {
        var cities = new List<string>
        {
            "Unknown",
            "Jacinto's location",
            "Ephyra's location"
        };

        return AssertQuery(
            async,
            ss => from c in ss.Set<City>()
                  where cities.Contains(c.Location)
                  select c);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Non_unicode_string_literals_is_used_for_non_unicode_column_with_subquery(bool async)
        => AssertQuery(
            async,
            ss => from c in ss.Set<City>()
                  where c.Location == "Unknown" && c.BornGears.Count(g => g.Nickname == "Paduk") == 1
                  select c);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Non_unicode_string_literals_is_used_for_non_unicode_column_in_subquery(bool async)
        => AssertQuery(
            async,
            ss => from g in ss.Set<Gear>()
                  where g.Nickname == "Marcus" && g.CityOfBirth.Location == "Jacinto's location"
                  select g);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Non_unicode_string_literals_is_used_for_non_unicode_column_with_contains(bool async)
        => AssertQuery(
            async,
            ss => from c in ss.Set<City>()
                  where c.Location.Contains("Jacinto")
                  select c);

    [ConditionalTheory] // Issue #32325
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Unicode_string_literals_is_used_for_non_unicode_column_with_concat(bool async)
        => AssertQuery(
            async,
            ss => from c in ss.Set<City>()
                  where (c.Location + "Added").Contains("Add")
                  select c);

    [ConditionalFact]
    public virtual void Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result1()
    {
        using var context = CreateContext();
        var query = from g1 in context.Gears.Include(g => g.Weapons)
                    join g2 in context.Gears
                        on g1.LeaderNickname equals g2.Nickname into grouping
                    from g2 in grouping.DefaultIfEmpty()
                    orderby g1.Nickname
                    select g2 ?? g1;

        var result = query.ToList();

        Assert.Equal(new[] { "Marcus", "Marcus", "Marcus", "Marcus", "Baird" }, result.Select(g => g.Nickname));
        Assert.Equal(new[] { 0, 0, 0, 2, 0 }, result.Select(g => g.Weapons.Count));
    }

    [ConditionalFact]
    public virtual void Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result2()
    {
        using var context = CreateContext();
        var query = from g1 in context.Gears
                    join g2 in context.Gears.Include(g => g.Weapons)
                        on g1.LeaderNickname equals g2.Nickname into grouping
                    from g2 in grouping.DefaultIfEmpty()
                    orderby g1.Nickname
                    select g2 ?? g1;

        var result = query.ToList();

        Assert.Equal(new[] { "Marcus", "Marcus", "Marcus", "Marcus", "Baird" }, result.Select(g => g.Nickname));
        Assert.Equal(new[] { 2, 2, 2, 0, 2 }, result.Select(g => g.Weapons.Count));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result3(bool async)
        => AssertQuery(
            async,
            ss => from g1 in ss.Set<Gear>().Include(g => g.Weapons)
                  join g2 in ss.Set<Gear>().Include(g => g.Weapons)
                      on g1.LeaderNickname equals g2.Nickname into grouping
                  from g2 in grouping.DefaultIfEmpty()
                  select g2 ?? g1,
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<Gear>(g => g.Weapons),
                new ExpectedInclude<Officer>(g => g.Weapons)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result4(bool async)
    {
        var expectedIncludes = new IExpectedInclude[]
        {
            new ExpectedInclude<Gear>(g => g.Weapons), new ExpectedInclude<Officer>(g => g.Weapons)
        };

        return AssertQuery(
            async,
            ss => from g1 in ss.Set<Gear>().Include(g => g.Weapons)
                  join g2 in ss.Set<Gear>().Include(g => g.Weapons)
                      on g1.LeaderNickname equals g2.Nickname into grouping
                  from g2 in grouping.DefaultIfEmpty()
                  select new
                  {
                      g1,
                      g2,
                      coalesce = g2 ?? g1
                  },
            elementAsserter: (e, a) =>
            {
                AssertInclude(e.g1, a.g1, expectedIncludes);
                AssertInclude(e.g2, a.g2, expectedIncludes);
                AssertInclude(e.coalesce, a.coalesce, expectedIncludes);
            },
            elementSorter: e => e.g1.Nickname);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_inheritance_and_coalesce_result(bool async)
        => AssertQuery(
            async,
            ss => from g1 in ss.Set<Gear>().Include(g => g.Weapons)
                  join g2 in ss.Set<Gear>().OfType<Officer>().Include(g => g.Weapons)
                      on g1.LeaderNickname equals g2.Nickname into grouping
                  from g2 in grouping.DefaultIfEmpty()
                  select g2 ?? g1,
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<Gear>(g => g.Weapons),
                new ExpectedInclude<Officer>(g => g.Weapons)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_conditional_result(bool async)
        => AssertQuery(
            async,
            ss => from g1 in ss.Set<Gear>().Include(g => g.Weapons)
                  join g2 in ss.Set<Gear>().Include(g => g.Weapons)
                      on g1.LeaderNickname equals g2.Nickname into grouping
                  from g2 in grouping.DefaultIfEmpty()
#pragma warning disable IDE0029 // Use coalesce expression
                  select g2 != null ? g2 : g1,
#pragma warning restore IDE0029 // Use coalesce expression
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<Gear>(g => g.Weapons),
                new ExpectedInclude<Officer>(g => g.Weapons)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_complex_projection_result(bool async)
    {
        var expectedIncludes = new IExpectedInclude[]
        {
            new ExpectedInclude<Gear>(g => g.Weapons), new ExpectedInclude<Officer>(g => g.Weapons)
        };

        return AssertQuery(
            async,
            ss => from g1 in ss.Set<Gear>().Include(g => g.Weapons)
                  join g2 in ss.Set<Gear>().Include(g => g.Weapons)
                      on g1.LeaderNickname equals g2.Nickname into grouping
                  from g2 in grouping.DefaultIfEmpty()
#pragma warning disable IDE0029 // Use coalesce expression
                  select new
                  {
                      g1,
                      g2,
                      coalesce = g2 ?? g1,
                      conditional = g2 != null ? g2 : g1
                  },
#pragma warning restore IDE0029 // Use coalesce expression
            elementAsserter: (e, a) =>
            {
                AssertInclude(e.g1, a.g1, expectedIncludes);
                AssertInclude(e.g2, a.g2, expectedIncludes);
                AssertInclude(e.coalesce, a.coalesce, expectedIncludes);
                AssertInclude(e.conditional, a.conditional, expectedIncludes);
            },
            elementSorter: e => e.g1.Nickname + " " + e.g2?.Nickname);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Coalesce_operator_in_predicate(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Weapon>().Where(w => (bool?)w.IsAutomatic ?? false));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Coalesce_operator_in_predicate_with_other_conditions(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Weapon>().Where(w => w.AmmunitionType == AmmunitionType.Cartridge && ((bool?)w.IsAutomatic ?? false)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Coalesce_operator_in_projection_with_other_conditions(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Weapon>().Select(w => w.AmmunitionType == AmmunitionType.Cartridge && ((bool?)w.IsAutomatic ?? false)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Optional_navigation_type_compensation_works_with_predicate(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<CogTag>().Where(t => t.Note != "K.I.A." && t.Gear.HasSoulPatch));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Optional_navigation_type_compensation_works_with_predicate2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<CogTag>().Where(t => t.Gear.HasSoulPatch),
            ss => ss.Set<CogTag>().Where(t => t.Gear.MaybeScalar(x => x.HasSoulPatch) == true));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Optional_navigation_type_compensation_works_with_predicate_negated(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<CogTag>().Where(t => !t.Gear.HasSoulPatch),
            ss => ss.Set<CogTag>().Where(t => !t.Gear.MaybeScalar(x => x.HasSoulPatch) == true));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Optional_navigation_type_compensation_works_with_predicate_negated_complex1(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<CogTag>().Where(t => !(t.Gear.HasSoulPatch ? true : t.Gear.HasSoulPatch)),
            ss => ss.Set<CogTag>().Where(
                t => !(t.Gear.MaybeScalar(x => x.HasSoulPatch) == true
                        ? true
                        : t.Gear.MaybeScalar(x => x.HasSoulPatch))
                    == true));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Optional_navigation_type_compensation_works_with_predicate_negated_complex2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<CogTag>().Where(t => !(!t.Gear.HasSoulPatch ? false : t.Gear.HasSoulPatch)),
            ss => ss.Set<CogTag>().Where(
                t => !(t.Gear.MaybeScalar(x => x.HasSoulPatch) == false
                        ? false
                        : t.Gear.MaybeScalar(x => x.HasSoulPatch))
                    == true));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Optional_navigation_type_compensation_works_with_conditional_expression(bool async)
        => AssertQuery(
            async,
            // ReSharper disable once RedundantTernaryExpression
            ss => ss.Set<CogTag>().Where(t => t.Gear.HasSoulPatch ? true : false));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Optional_navigation_type_compensation_works_with_binary_expression(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<CogTag>().Where(t => t.Gear.HasSoulPatch || t.Note.Contains("Cole")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Optional_navigation_type_compensation_works_with_binary_and_expression(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<CogTag>().Select(t => t.Gear.HasSoulPatch && t.Note.Contains("Cole")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Optional_navigation_type_compensation_works_with_projection(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<CogTag>().Where(t => t.Note != "K.I.A.").Select(t => t.Gear.SquadId));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Optional_navigation_type_compensation_works_with_projection_into_anonymous_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<CogTag>().Where(t => t.Note != "K.I.A.").Select(
                t => new { t.Gear.SquadId }),
            elementSorter: e => e.SquadId);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Optional_navigation_type_compensation_works_with_DTOs(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<CogTag>().Where(t => t.Note != "K.I.A.").Select(
                t => new Squad { Id = t.Gear.SquadId }));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Optional_navigation_type_compensation_works_with_list_initializers(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<CogTag>().Where(t => t.Note != "K.I.A.").OrderBy(t => t.Note).Select(
                t => new List<int>
                {
                    t.Gear.SquadId,
                    t.Gear.SquadId + 1,
                    42
                }),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Optional_navigation_type_compensation_works_with_array_initializers(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<CogTag>().Where(t => t.Note != "K.I.A.").Select(t => new[] { t.Gear.SquadId }),
            elementSorter: e => e[0]);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Optional_navigation_type_compensation_works_with_orderby(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<CogTag>().Where(t => t.Note != "K.I.A.").OrderBy(t => t.Gear.SquadId).Select(t => t));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Optional_navigation_type_compensation_works_with_all(bool async)
        => AssertAll(
            async,
            ss => ss.Set<CogTag>().Where(t => t.Note != "K.I.A."),
            predicate: t => t.Gear.HasSoulPatch);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Optional_navigation_type_compensation_works_with_negated_predicate(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<CogTag>().Where(t => t.Note != "K.I.A.").Where(t => !t.Gear.HasSoulPatch));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Optional_navigation_type_compensation_works_with_contains(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<CogTag>().Where(t => t.Note != "K.I.A." && ss.Set<Gear>().Select(g => g.SquadId).Contains(t.Gear.SquadId)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Optional_navigation_type_compensation_works_with_skip(bool async)
        => AssertInvalidMaterializationType(
            () => AssertQuery(
                async,
                ss => ss.Set<CogTag>().Where(t => t.Note != "K.I.A.").OrderBy(t => t.Note)
                    .Select(t => ss.Set<Gear>().OrderBy(g => g.Nickname).Skip(t.Gear.SquadId)),
                assertOrder: true,
                elementAsserter: (e, a) => AssertCollection(e, a, ordered: true)),
            "IEnumerable<T>");

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Optional_navigation_type_compensation_works_with_take(bool async)
        => AssertInvalidMaterializationType(
            () => AssertQuery(
                async,
                ss => ss.Set<CogTag>().Where(t => t.Note != "K.I.A.").OrderBy(t => t.Note)
                    .Select(t => ss.Set<Gear>().OrderBy(g => g.Nickname).Take(t.Gear.SquadId)),
                assertOrder: true,
                elementAsserter: (e, a) => AssertCollection(e, a, ordered: true)),
            "IEnumerable<T>");

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_correlated_filtered_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>()
                .Where(g => g.CityOfBirth.Name == "Ephyra" || g.CityOfBirth.Name == "Hanover")
                .OrderBy(g => g.Nickname)
                .Select(g => g.Weapons.Where(w => w.Name != "Lancer").ToList()),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_correlated_filtered_collection_with_composite_key(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>().OfType<Officer>().OrderBy(g => g.Nickname)
                .Select(g => g.Reports.Where(r => r.Nickname != "Dom").ToList()),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_correlated_filtered_collection_returning_queryable_throws(bool async)
        => AssertInvalidMaterializationType(
            () => AssertQuery(
                async,
                ss => ss.Set<CogTag>().OrderBy(t => t.Note).Select(t => ss.Set<Gear>().Where(g => g.Nickname == t.GearNickName)),
                assertOrder: true,
                elementAsserter: (e, a) => AssertCollection(e, a)),
            typeof(IQueryable<Gear>).ShortDisplayName());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_correlated_filtered_collection_works_with_caching(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<CogTag>().OrderBy(t => t.Note).Select(t => ss.Set<Gear>().Where(g => g.Nickname == t.GearNickName).ToList()),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Join_predicate_value_equals_condition(bool async)
        => AssertQuery(
            async,
            ss =>
                from g in ss.Set<Gear>()
                join w in ss.Set<Weapon>()
                    on true equals w.SynergyWithId != null
                select g);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Join_predicate_value(bool async)
        => AssertQuery(
            async,
            ss =>
                from g in ss.Set<Gear>()
                join w in ss.Set<Weapon>()
                    on g.HasSoulPatch equals true
                select g);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Join_predicate_condition_equals_condition(bool async)
        => AssertQuery(
            async,
            ss =>
                from g in ss.Set<Gear>()
                join w in ss.Set<Weapon>()
                    on g.FullName != null equals w.SynergyWithId != null
                select g);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Left_join_predicate_value_equals_condition(bool async)
        => AssertQuery(
            async,
            ss =>
                from g in ss.Set<Gear>()
                join w in ss.Set<Weapon>()
                    on true equals w.SynergyWithId != null
                    into group1
                from w in group1.DefaultIfEmpty()
                select g);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Left_join_predicate_value(bool async)
        => AssertQuery(
            async,
            ss =>
                from g in ss.Set<Gear>()
                join w in ss.Set<Weapon>()
                    on g.HasSoulPatch equals true
                    into group1
                from w in group1.DefaultIfEmpty()
                select g);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Left_join_predicate_condition_equals_condition(bool async)
        => AssertQuery(
            async,
            ss =>
                from g in ss.Set<Gear>()
                join w in ss.Set<Weapon>()
                    on g.FullName != null equals w.SynergyWithId != null
                    into group1
                from w in group1.DefaultIfEmpty()
                select g);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_datetimeoffset_now(bool async)
        => AssertQuery(
            async,
            ss => from m in ss.Set<Mission>()
                  where m.Timeline != DateTimeOffset.Now
                  select m);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_datetimeoffset_utcnow(bool async)
        => AssertQuery(
            async,
            ss => from m in ss.Set<Mission>()
                  where m.Timeline != DateTimeOffset.UtcNow
                  select m);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_datetimeoffset_date_component(bool async)
        => AssertQuery(
            async,
            ss => from m in ss.Set<Mission>()
                  where m.Timeline.Date > new DateTimeOffset().Date
                  select m);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_datetimeoffset_year_component(bool async)
        => AssertQuery(
            async,
            ss => from m in ss.Set<Mission>()
                  where m.Timeline.Year == 2
                  select m);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_datetimeoffset_month_component(bool async)
        => AssertQuery(
            async,
            ss => from m in ss.Set<Mission>()
                  where m.Timeline.Month == 1
                  select m);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_datetimeoffset_dayofyear_component(bool async)
        => AssertQuery(
            async,
            ss => from m in ss.Set<Mission>()
                  where m.Timeline.DayOfYear == 2
                  select m);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_datetimeoffset_day_component(bool async)
        => AssertQuery(
            async,
            ss => from m in ss.Set<Mission>()
                  where m.Timeline.Day == 2
                  select m);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_datetimeoffset_hour_component(bool async)
        => AssertQuery(
            async,
            ss => from m in ss.Set<Mission>()
                  where m.Timeline.Hour == 10
                  select m);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_datetimeoffset_minute_component(bool async)
        => AssertQuery(
            async,
            ss => from m in ss.Set<Mission>()
                  where m.Timeline.Minute == 0
                  select m);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_datetimeoffset_second_component(bool async)
        => AssertQuery(
            async,
            ss => from m in ss.Set<Mission>()
                  where m.Timeline.Second == 0
                  select m);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_datetimeoffset_millisecond_component(bool async)
        => AssertQuery(
            async,
            ss => from m in ss.Set<Mission>()
                  where m.Timeline.Millisecond == 0
                  select m);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task DateTimeOffset_DateAdd_AddYears(bool async)
        => AssertQueryScalar(
            async,
            ss => from m in ss.Set<Mission>()
                  select m.Timeline.AddYears(1));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task DateTimeOffset_DateAdd_AddMonths(bool async)
        => AssertQueryScalar(
            async,
            ss => from m in ss.Set<Mission>()
                  select m.Timeline.AddMonths(1));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task DateTimeOffset_DateAdd_AddDays(bool async)
        => AssertQueryScalar(
            async,
            ss => from m in ss.Set<Mission>()
                  select m.Timeline.AddDays(1));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task DateTimeOffset_DateAdd_AddHours(bool async)
        => AssertQueryScalar(
            async,
            ss => from m in ss.Set<Mission>()
                  select m.Timeline.AddHours(1));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task DateTimeOffset_DateAdd_AddMinutes(bool async)
        => AssertQueryScalar(
            async,
            ss => from m in ss.Set<Mission>()
                  select m.Timeline.AddMinutes(1));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task DateTimeOffset_DateAdd_AddSeconds(bool async)
        => AssertQueryScalar(
            async,
            ss => from m in ss.Set<Mission>()
                  select m.Timeline.AddSeconds(1));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task DateTimeOffset_DateAdd_AddMilliseconds(bool async)
        => AssertQueryScalar(
            async,
            ss => from m in ss.Set<Mission>()
                  select m.Timeline.AddMilliseconds(300));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_datetimeoffset_milliseconds_parameter_and_constant(bool async)
    {
        var dateTimeOffset = new DateTimeOffset(599898024001234567, new TimeSpan(1, 30, 0));

        // Literal where clause
        var p = Expression.Parameter(typeof(Mission), "i");
        var dynamicWhere = Expression.Lambda<Func<Mission, bool>>(
            Expression.Equal(
                Expression.Property(p, "Timeline"),
                Expression.Constant(dateTimeOffset)
            ), p);

        return AssertCount(
            async,
            ss => ss.Set<Mission>().Where(dynamicWhere),
            ss => ss.Set<Mission>().Where(m => m.Timeline == dateTimeOffset));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Orderby_added_for_client_side_GroupJoin_composite_dependent_to_principal_LOJ_when_incomplete_key_is_used(bool async)
        => AssertTranslationFailed(
            () => AssertQuery(
                async,
                ss =>
                    from t in ss.Set<CogTag>()
                    join g in ss.Set<Gear>() on t.GearNickName equals g.Nickname into grouping
                    from g in ClientDefaultIfEmpty(grouping)
#pragma warning disable IDE0031 // Use null propagation
                    select new { t.Note, Nickname = g != null ? g.Nickname : null },
#pragma warning restore IDE0031 // Use null propagation
                elementSorter: e => e.Note));

    private static IEnumerable<TElement> ClientDefaultIfEmpty<TElement>(IEnumerable<TElement> source)
        // ReSharper disable PossibleMultipleEnumeration
        => source?.Count() == 0 ? new[] { default(TElement) } : source;
    // ReSharper restore PossibleMultipleEnumeration

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Complex_predicate_with_AndAlso_and_nullable_bool_property(bool async)
        => AssertQuery(
            async,
            ss => from w in ss.Set<Weapon>()
                  where w.Id != 50 && !w.Owner.HasSoulPatch
                  select w,
            ss => from w in ss.Set<Weapon>()
                  where w.Id != 50 && w.Owner.MaybeScalar(x => x.HasSoulPatch) == false
                  select w);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Distinct_with_optional_navigation_is_translated_to_sql(bool async)
        => AssertQueryScalar(
            async,
            ss => (from g in ss.Set<Gear>()
                   where g.Tag.Note != "Foo"
                   select g.HasSoulPatch).Distinct());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Sum_with_optional_navigation_is_translated_to_sql(bool async)
        => AssertSum(
            async,
            ss => (from g in ss.Set<Gear>()
                   where g.Tag.Note != "Foo"
                   select g.SquadId));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Count_with_optional_navigation_is_translated_to_sql(bool async)
        => AssertCount(
            async,
            ss => (from g in ss.Set<Gear>()
                   where g.Tag.Note != "Foo"
                   select g.HasSoulPatch));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task FirstOrDefault_with_manually_created_groupjoin_is_translated_to_sql(bool async)
        => AssertFirstOrDefault(
            async,
            ss => from s in ss.Set<Squad>()
                  join g in ss.Set<Gear>() on s.Id equals g.SquadId into grouping
                  from g in grouping.DefaultIfEmpty()
                  where s.Name == "Kilo"
                  select s);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Any_with_optional_navigation_as_subquery_predicate_is_translated_to_sql(bool async)
        => AssertQuery(
            async,
            ss => from s in ss.Set<Squad>()
                  where !s.Members.Any(m => m.Tag.Note == "Dom's Tag")
                  select s.Name);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task All_with_optional_navigation_is_translated_to_sql(bool async)
        => AssertAll(
            async,
            ss => from g in ss.Set<Gear>()
                  select g,
            predicate: g => g.Tag.Note != "Foo");

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Client_side_equality_with_parameter_works_with_optional_navigations(bool async)
    {
        var prm = "Marcus' Tag";

        return AssertQuery(
            async,
            ss => ss.Set<Gear>().Where(g => ClientEquals(g.Tag.Note, prm)));
    }

    private static bool ClientEquals(string first, string second)
        => first == second;

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Contains_with_local_nullable_guid_list_closure(bool async)
    {
        var ids = new List<Guid?>
        {
            Guid.Parse("DF36F493-463F-4123-83F9-6B135DEEB7BA"),
            Guid.Parse("23CBCF9B-CE14-45CF-AAFA-2C2667EBFDD3"),
            Guid.Parse("AB1B82D7-88DB-42BD-A132-7EEF9AA68AF4")
        };

        return AssertQuery(
            async,
            ss => ss.Set<CogTag>().Where(e => ids.Contains(e.Id)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Unnecessary_include_doesnt_get_added_complex_when_projecting_EF_Property(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>()
                .OrderBy(g => g.Rank)
                .Include(g => g.Tag)
                .Where(g => g.HasSoulPatch)
                .Select(g => new { FullName = EF.Property<string>(g, "FullName") }),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Multiple_order_bys_are_properly_lifted_from_subquery_created_by_include(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>()
                .OrderBy(g => g.Rank)
                .Include(g => g.Tag)
                .OrderByDescending(g => g.Nickname)
                .Include(g => g.CityOfBirth)
                .OrderBy(g => g.FullName)
                .Where(g => !g.HasSoulPatch)
                .Select(g => g.FullName),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Order_by_is_properly_lifted_from_subquery_with_same_order_by_in_the_outer_query(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>()
                .Include(g => g.CityOfBirth)
                .OrderBy(g => g.FullName)
                .Where(g => !g.HasSoulPatch)
                .Select(g => g.FullName),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_is_properly_lifted_from_subquery_created_by_include(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>()
                .Where(g => g.FullName != "Augustus Cole")
                .Include(g => g.Tag)
                .OrderBy(g => g.FullName)
                .Where(g => !g.HasSoulPatch)
                .Select(g => g),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Gear>(e => e.Tag)),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Subquery_is_lifted_from_main_from_clause_of_SelectMany(bool async)
        => AssertQuery(
            async,
            ss => from g1 in ss.Set<Gear>().OrderBy(g => g.Rank).Include(g => g.Tag)
                  from g2 in ss.Set<Gear>()
                  orderby g1.FullName
                  where g1.HasSoulPatch && !g2.HasSoulPatch
                  select new { Name1 = g1.FullName, Name2 = g2.FullName });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Subquery_containing_SelectMany_projecting_main_from_clause_gets_lifted(bool async)
        => AssertQuery(
            async,
            ss =>
                from g in (from gear in ss.Set<Gear>()
                           from tag in ss.Set<CogTag>()
                           where gear.HasSoulPatch
                           orderby tag.Note
                           select gear).AsTracking()
                orderby g.FullName
                select g.FullName,
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Subquery_containing_join_projecting_main_from_clause_gets_lifted(bool async)
        => AssertQuery(
            async,
            ss =>
                from g in (from gear in ss.Set<Gear>()
                           join tag in ss.Set<CogTag>() on gear.Nickname equals tag.GearNickName
                           orderby tag.Note
                           select gear).AsTracking()
                orderby g.Nickname
                select g.Nickname,
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Subquery_containing_left_join_projecting_main_from_clause_gets_lifted(bool async)
        => AssertQuery(
            async,
            ss =>
                from g in (from gear in ss.Set<Gear>()
                           join tag in ss.Set<CogTag>() on gear.Nickname equals tag.GearNickName into grouping
                           from tag in grouping.DefaultIfEmpty()
                           orderby gear.Rank
                           select gear).AsTracking()
                orderby g.Nickname
                select g.Nickname,
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Subquery_containing_join_gets_lifted_clashing_names(bool async)
        => AssertQuery(
            async,
            ss =>
                from gear in (from gear in ss.Set<Gear>()
                              join tag in ss.Set<CogTag>() on gear.Nickname equals tag.GearNickName
                              orderby tag.Note
                              where tag.GearNickName != "Cole Train"
                              select gear).AsTracking()
                join tag in ss.Set<CogTag>() on gear.Nickname equals tag.GearNickName
                orderby gear.Nickname, tag.Id
                select gear.Nickname,
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Subquery_created_by_include_gets_lifted_nested(bool async)
        => AssertQuery(
            async,
            ss => from gear in ss.Set<Gear>().OrderBy(g => g.Rank).Where(g => g.Weapons.Any()).Include(g => g.CityOfBirth)
                  where !gear.HasSoulPatch
                  orderby gear.Nickname
                  select gear,
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Gear>(e => e.CityOfBirth)),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Subquery_is_lifted_from_additional_from_clause(bool async)
        => AssertQuery(
            async,
            ss =>
                from g1 in ss.Set<Gear>()
                from g2 in ss.Set<Gear>().OrderBy(g => g.Rank).Include(g => g.Tag)
                orderby g1.FullName
                where g1.HasSoulPatch && !g2.HasSoulPatch
                select new { Name1 = g1.FullName, Name2 = g2.FullName },
            elementSorter: e => (e.Name1, e.Name2));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Subquery_with_result_operator_is_not_lifted(bool async)
        => AssertQuery(
            async,
            ss => from g in ss.Set<Gear>().Where(g => !g.HasSoulPatch).OrderBy(g => g.FullName).Take(2).AsTracking()
                  orderby g.Rank
                  select g.FullName,
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Skip_with_orderby_followed_by_orderBy_is_pushed_down(bool async)
        => AssertQuery(
            async,
            ss => from g in ss.Set<Gear>().Where(g => !g.HasSoulPatch).OrderBy(g => g.FullName).Skip(1)
                  orderby g.Rank
                  select g.FullName,
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Take_without_orderby_followed_by_orderBy_is_pushed_down1(bool async)
        => AssertQuery(
            async,
            ss => from g in ss.Set<Gear>().Where(g => !g.HasSoulPatch).Take(999).OrderBy(g => g.FullName)
                  orderby g.Rank
                  select g.FullName,
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Take_without_orderby_followed_by_orderBy_is_pushed_down2(bool async)
        => AssertQuery(
            async,
            ss => from g in ss.Set<Gear>().Where(g => !g.HasSoulPatch).Take(999)
                  orderby g.FullName
                  orderby g.Rank
                  select g.FullName,
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Take_without_orderby_followed_by_orderBy_is_pushed_down3(bool async)
        => AssertQuery(
            async,
            ss => from g in ss.Set<Gear>().Where(g => !g.HasSoulPatch).Take(999)
                  orderby g.FullName, g.Rank
                  select g.FullName,
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_length_of_string_property(bool async)
        => AssertQuery(
            async,
            ss => from w in ss.Set<Weapon>()
                  select new { w.Name, w.Name.Length },
            elementSorter: e => e.Name);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Client_method_on_collection_navigation_in_predicate(bool async)
        => AssertTranslationFailed(
            () => AssertQuery(
                async,
                ss => from g in ss.Set<Gear>()
                      where g.HasSoulPatch && FavoriteWeapon(g.Weapons).Name == "Marcus' Lancer"
                      select g.Nickname));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Client_method_on_collection_navigation_in_predicate_accessed_by_ef_property(bool async)
        => AssertTranslationFailed(
            () => AssertQuery(
                async,
                ss => from g in ss.Set<Gear>()
                      where !g.HasSoulPatch && FavoriteWeapon(EF.Property<List<Weapon>>(g, "Weapons")).Name == "Cole's Gnasher"
                      select g.Nickname));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Client_method_on_collection_navigation_in_order_by(bool async)
        => AssertTranslationFailed(
            () => AssertQuery(
                async,
                ss => from g in ss.Set<Gear>()
                      where !g.HasSoulPatch
                      orderby FavoriteWeapon(g.Weapons).Name descending
                      select g.Nickname,
                assertOrder: true));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Client_method_on_collection_navigation_in_additional_from_clause(bool async)
        => AssertTranslationFailed(
            () => AssertQuery(
                async,
                ss => from g in ss.Set<Gear>().OfType<Officer>()
                      from v in Veterans(g.Reports)
                      select new { g = g.Nickname, v = v.Nickname },
                elementSorter: e => e.g + e.v));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Client_method_on_collection_navigation_in_outer_join_key(bool async)
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(
            () => AssertQuery(
                async,
                ss => from o in ss.Set<Gear>().OfType<Officer>()
                      join g in ss.Set<Gear>() on FavoriteWeapon(o.Weapons).Name equals FavoriteWeapon(g.Weapons).Name
                      where o.HasSoulPatch
                      select new { o = o.Nickname, g = g.Nickname },
                elementSorter: e => e.o + e.g))).Message;
    }

    private static Weapon FavoriteWeapon(IEnumerable<Weapon> weapons)
        => weapons.OrderBy(w => w.Id).FirstOrDefault();

    private static IEnumerable<Gear> Veterans(IEnumerable<Gear> gears)
        => gears.Where(g => g.Nickname is "Marcus" or "Dom" or "Cole Train" or "Baird");

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Member_access_on_derived_entity_using_cast(bool async)
        => AssertQuery(
            async,
            ss => from f in ss.Set<Faction>()
                  where f is LocustHorde
                  orderby ((LocustHorde)f).Name
                  select new { ((LocustHorde)f).Name, ((LocustHorde)f).Eradicated },
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Member_access_on_derived_materialized_entity_using_cast(bool async)
        => AssertQuery(
            async,
            ss => from f in ss.Set<Faction>()
                  where f is LocustHorde
                  orderby f.Name
                  select new { f, ((LocustHorde)f).Eradicated },
            assertOrder: true,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.f, a.f);
                Assert.Equal(e.Eradicated, a.Eradicated);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Member_access_on_derived_entity_using_cast_and_let(bool async)
        => AssertQuery(
            async,
            ss => from f in ss.Set<Faction>()
                  where f is LocustHorde
                  let horde = (LocustHorde)f
                  orderby horde.Name
                  select new { horde.Name, horde.Eradicated },
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Property_access_on_derived_entity_using_cast(bool async)
        => AssertQuery(
            async,
            ss => from f in ss.Set<Faction>()
                  where f is LocustHorde
                  let horde = (LocustHorde)f
                  orderby f.Name
                  select new { Name = EF.Property<string>(horde, "Name"), Eradicated = EF.Property<bool>((LocustHorde)f, "Eradicated") },
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Navigation_access_on_derived_entity_using_cast(bool async)
        => AssertQuery(
            async,
            ss => from f in ss.Set<Faction>()
                  where f is LocustHorde
                  orderby f.Name
                  select new { f.Name, Threat = ((LocustHorde)f).Commander.ThreatLevel },
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Navigation_access_on_derived_materialized_entity_using_cast(bool async)
        => AssertQuery(
            async,
            ss => from f in ss.Set<Faction>()
                  where f is LocustHorde
                  orderby f.Name
                  select new
                  {
                      f,
                      f.Name,
                      Threat = ((LocustHorde)f).Commander.ThreatLevel
                  },
            assertOrder: true,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.f, a.f);
                Assert.Equal(e.Name, a.Name);
                Assert.Equal(e.Threat, a.Threat);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Navigation_access_via_EFProperty_on_derived_entity_using_cast(bool async)
        => AssertQuery(
            async,
            ss => from f in ss.Set<Faction>()
                  where f is LocustHorde
                  orderby f.Name
                  select new { f.Name, Threat = EF.Property<LocustCommander>((LocustHorde)f, "Commander").ThreatLevel },
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Navigation_access_fk_on_derived_entity_using_cast(bool async)
        => AssertQuery(
            async,
            ss => from f in ss.Set<Faction>()
                  where f is LocustHorde
                  orderby f.Name
                  select new { f.Name, CommanderName = ((LocustHorde)f).Commander.Name },
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Collection_navigation_access_on_derived_entity_using_cast(bool async)
        => AssertQuery(
            async,
            ss => from f in ss.Set<Faction>()
                  where f is LocustHorde
                  orderby f.Name
                  select new { f.Name, LeadersCount = ((LocustHorde)f).Leaders.Count },
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Collection_navigation_access_on_derived_entity_using_cast_in_SelectMany(bool async)
        => AssertQuery(
            async,
            ss => from f in ss.Set<Faction>().Where(f => f is LocustHorde)
                  from l in ((LocustHorde)f).Leaders
                  orderby l.Name
                  select new { f.Name, LeaderName = l.Name },
            elementSorter: e => (e.Name, e.LeaderName));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_on_derived_entity_using_OfType(bool async)
        => AssertQuery(
            async,
            ss => from lh in ss.Set<Faction>().OfType<LocustHorde>().Include(h => h.Commander).Include(h => h.Leaders)
                  orderby lh.Name
                  select lh,
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<LocustHorde>(e1 => e1.Commander),
                new ExpectedInclude<LocustHorde>(e2 => e2.Leaders)),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_on_derived_entity_with_cast(bool async)
        // TODO: should we disable this scenario? see #14671
        => AssertQuery(
            async,
            ss => (from f in ss.Set<Faction>()
                   where f is LocustHorde
                   orderby f.Id
                   select f).Include(f => f.Capital),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Faction>(e1 => e1.Capital)),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Distinct_on_subquery_doesnt_get_lifted(bool async)
        => AssertQueryScalar(
            async,
            ss => from g in (from ig in ss.Set<Gear>()
                             select ig).Distinct()
                  select g.HasSoulPatch);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Cast_result_operator_on_subquery_is_properly_lifted_to_a_convert(bool async)
        => AssertQueryScalar(
            async,
            ss => from lh in (from f in ss.Set<Faction>()
                              select f).Cast<LocustHorde>()
                  select lh.Eradicated);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Comparing_two_collection_navigations_composite_key(bool async)
        => AssertQuery(
            async,
            ss => from g1 in ss.Set<Gear>()
                  from g2 in ss.Set<Gear>()
                  // ReSharper disable once PossibleUnintendedReferenceComparison
                  where g1.Weapons == g2.Weapons
                  orderby g1.Nickname
                  select new { Nickname1 = g1.Nickname, Nickname2 = g2.Nickname },
            elementSorter: e => (e.Nickname1, e.Nickname2));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Comparing_two_collection_navigations_inheritance(bool async)
        => AssertQuery(
            async,
            ss => from f in ss.Set<Faction>()
                  from o in ss.Set<Gear>().OfType<Officer>()
                  where f is LocustHorde && o.HasSoulPatch
                  // ReSharper disable once PossibleUnintendedReferenceComparison
                  where ((LocustHorde)f).Commander.DefeatedBy.Weapons == o.Weapons
                  select new { f.Name, o.Nickname },
            elementSorter: e => (e.Name, e.Nickname));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Comparing_entities_using_Equals_inheritance(bool async)
        => AssertQuery(
            async,
            ss => from g in ss.Set<Gear>()
                  from o in ss.Set<Gear>().OfType<Officer>()
                  where g.Equals(o)
                  orderby g.Nickname, o.Nickname
                  select new { Nickname1 = g.Nickname, Nickname2 = o.Nickname },
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Contains_on_nullable_array_produces_correct_sql(bool async)
    {
        var cities = new[] { "Ephyra", null };

        return AssertQuery(
            async,
            ss => ss.Set<Gear>().Where(g => g.SquadId < 2 && cities.Contains(g.AssignedCity.Name)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Optional_navigation_with_collection_composite_key(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<CogTag>().Where(t => t.Gear is Officer && ((Officer)t.Gear).Reports.Count(r => r.Nickname == "Dom") > 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_null_conditional_with_inheritance(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Faction>()
                .Where(f => f is LocustHorde)
                .Select(f => EF.Property<string>((LocustHorde)f, "CommanderName") != null ? ((LocustHorde)f).CommanderName : null));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_null_conditional_with_inheritance_negative(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Faction>()
                .Where(f => f is LocustHorde)
                .Select(f => EF.Property<string>((LocustHorde)f, "CommanderName") != null ? ((LocustHorde)f).Eradicated : null));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_collection_navigation_with_inheritance1(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Faction>().OfType<LocustHorde>()
                .Select(
                    h => new { h.Id, Leaders = EF.Property<ICollection<LocustLeader>>(h.Commander.CommandingFaction, "Leaders") }),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id, a.Id);
                AssertCollection(e.Leaders, a.Leaders);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_collection_navigation_with_inheritance2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Faction>().OfType<LocustHorde>()
                .Select(
                    h => new { h.Id, Gears = EF.Property<ICollection<Gear>>((Officer)h.Commander.DefeatedBy, "Reports") }),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id, a.Id);
                AssertCollection(e.Gears ?? new List<Gear>(), a.Gears);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_collection_navigation_with_inheritance3(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Faction>()
                .Where(f => f is LocustHorde)
                .Select(
                    f => new { f.Id, Gears = EF.Property<ICollection<Gear>>((Officer)((LocustHorde)f).Commander.DefeatedBy, "Reports") }),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id, a.Id);
                AssertCollection(e.Gears ?? new List<Gear>(), a.Gears);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_reference_on_derived_type_using_string(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<LocustLeader>().Include("DefeatedBy"),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<LocustCommander>(lc => lc.DefeatedBy)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_reference_on_derived_type_using_EF_Property(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<LocustLeader>().Include(lc => EF.Property<Gear>((LocustCommander)lc, "DefeatedBy")),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<LocustCommander>(lc => lc.DefeatedBy)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_reference_on_derived_type_using_string_nested1(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<LocustLeader>().Include("DefeatedBy.Squad"),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<LocustCommander>(lc => lc.DefeatedBy),
                new ExpectedInclude<Gear>(g => g.Squad, "DefeatedBy")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_reference_on_derived_type_using_string_nested2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<LocustLeader>().Include("DefeatedBy.Reports.CityOfBirth"),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<LocustCommander>(lc => lc.DefeatedBy),
                new ExpectedInclude<Officer>(o => o.Reports, "DefeatedBy"),
                new ExpectedInclude<Gear>(g => g.CityOfBirth, "DefeatedBy.Reports")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_reference_on_derived_type_using_lambda(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<LocustLeader>().Include(ll => ((LocustCommander)ll).DefeatedBy),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<LocustCommander>(lc => lc.DefeatedBy)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_reference_on_derived_type_using_lambda_with_soft_cast(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<LocustLeader>().Include(ll => (ll as LocustCommander).DefeatedBy),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<LocustCommander>(lc => lc.DefeatedBy)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_reference_on_derived_type_using_lambda_with_tracking(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<LocustLeader>().AsTracking().Include(ll => ((LocustCommander)ll).DefeatedBy),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<LocustCommander>(lc => lc.DefeatedBy)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_collection_on_derived_type_using_string(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>().Include("Reports"),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Officer>(o => o.Reports)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_collection_on_derived_type_using_EF_Property(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>().Include(o => EF.Property<ICollection<Gear>>((Officer)o, "Reports")),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Officer>(o => o.Reports)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_collection_on_derived_type_using_lambda(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>().Include(g => ((Officer)g).Reports),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Officer>(o => o.Reports)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_collection_on_derived_type_using_lambda_with_soft_cast(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>().Include(g => (g as Officer).Reports),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Officer>(o => o.Reports)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_base_navigation_on_derived_entity(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>().Include(g => ((Officer)g).Tag).Include(g => ((Officer)g).Weapons),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<Officer>(e1 => e1.Tag),
                new ExpectedInclude<Officer>(e2 => e2.Weapons)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task ThenInclude_collection_on_derived_after_base_reference(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<CogTag>().Include(t => t.Gear).ThenInclude(g => (g as Officer).Weapons),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<CogTag>(e1 => e1.Gear),
                new ExpectedInclude<Officer>(e2 => e2.Weapons, "Gear")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task ThenInclude_collection_on_derived_after_derived_reference(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Faction>().Include(f => (f as LocustHorde).Commander).ThenInclude(c => (c.DefeatedBy as Officer).Reports),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<LocustHorde>(e1 => e1.Commander),
                new ExpectedInclude<LocustCommander>(e2 => e2.DefeatedBy, "Commander"),
                new ExpectedInclude<Officer>(e3 => e3.Reports, "Commander.DefeatedBy")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task ThenInclude_collection_on_derived_after_derived_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>().Include(g => ((Officer)g).Reports).ThenInclude(g => ((Officer)g).Reports),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<Officer>(e1 => e1.Reports),
                new ExpectedInclude<Officer>(e2 => e2.Reports, "Reports")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task ThenInclude_reference_on_derived_after_derived_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Faction>().Include(f => ((LocustHorde)f).Leaders).ThenInclude(l => ((LocustCommander)l).DefeatedBy),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<LocustHorde>(e1 => e1.Leaders),
                new ExpectedInclude<LocustCommander>(e2 => e2.DefeatedBy, "Leaders")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Multiple_derived_included_on_one_method(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Faction>().Include(f => (((LocustHorde)f).Commander.DefeatedBy as Officer).Reports),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<LocustHorde>(e1 => e1.Commander),
                new ExpectedInclude<LocustCommander>(e2 => e2.DefeatedBy, "Commander"),
                new ExpectedInclude<Officer>(e3 => e3.Reports, "Commander.DefeatedBy")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_on_derived_multi_level(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>().Include(g => ((Officer)g).Reports).ThenInclude(g => g.Squad.Missions),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<Officer>(e1 => e1.Reports),
                new ExpectedInclude<Gear>(e2 => e2.Squad, "Reports"),
                new ExpectedInclude<Squad>(e3 => e3.Missions, "Reports.Squad")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Include_collection_and_invalid_navigation_using_string_throws(bool async)
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(
            () => AssertQuery(
                async,
                ss => ss.Set<Gear>().Include("Reports.Foo")))).Message;

        Assert.Contains(
            CoreResources.LogInvalidIncludePath(new TestLogger<TestLoggingDefinitions>())
                .GenerateMessage("Foo", "Reports.Foo"), message);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Projecting_nullable_bool_in_conditional_works(bool async)
        => AssertQuery(
            async,
            ss =>
                ss.Set<CogTag>().Select(
                    cg =>
                        new { Prop = cg.Gear != null ? cg.Gear.HasSoulPatch : false }),
            e => e.Prop);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Enum_ToString_is_client_eval(bool async)
        => AssertQuery(
            async,
            ss =>
                ss.Set<Gear>().OrderBy(g => g.SquadId)
                    .ThenBy(g => g.Nickname)
                    .Select(g => g.Rank.ToString()));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Correlated_collections_naked_navigation_with_ToList(bool async)
        => AssertQuery(
            async,
            ss => from g in ss.Set<Gear>()
                  where g.Nickname != "Marcus"
                  orderby g.Nickname
                  select g.Weapons.ToList(),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Correlated_collections_naked_navigation_with_ToList_followed_by_projecting_count(bool async)
        => AssertQueryScalar(
            async,
            ss => (from g in ss.Set<Gear>()
                   where g.Nickname != "Marcus"
                   orderby g.Nickname
                   select g.Weapons.ToList()).Select(e => e.Count),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Correlated_collections_naked_navigation_with_ToArray(bool async)
        => AssertQuery(
            async,
            ss => from g in ss.Set<Gear>()
                  where g.Nickname != "Marcus"
                  orderby g.Nickname
                  select g.Weapons.ToArray(),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Correlated_collections_basic_projection(bool async)
        => AssertQuery(
            async,
            ss => from g in ss.Set<Gear>()
                  where g.Nickname != "Marcus"
                  orderby g.Nickname
                  select (from w in g.Weapons
                          where w.IsAutomatic || w.Name != "foo"
                          select w).ToList(),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Correlated_collections_basic_projection_explicit_to_list(bool async)
        => AssertQuery(
            async,
            ss => from g in ss.Set<Gear>()
                  where g.Nickname != "Marcus"
                  orderby g.Nickname
                  select (from w in g.Weapons
                          where w.IsAutomatic || w.Name != "foo"
                          select w).ToList(),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Correlated_collections_basic_projection_explicit_to_array(bool async)
        => AssertQuery(
            async,
            ss => from g in ss.Set<Gear>()
                  where g.Nickname != "Marcus"
                  orderby g.Nickname
                  select (from w in g.Weapons
                          where w.IsAutomatic || w.Name != "foo"
                          select w).ToArray(),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Correlated_collections_basic_projection_ordered(bool async)
        => AssertQuery(
            async,
            ss => from g in ss.Set<Gear>()
                  where g.Nickname != "Marcus"
                  orderby g.Nickname
                  select (from w in g.Weapons
                          where w.IsAutomatic || w.Name != "foo"
                          orderby w.Name descending
                          select w).ToList(),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a, ordered: true));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Correlated_collections_basic_projection_composite_key(bool async)
        => AssertQuery(
            async,
            ss =>
                from o in ss.Set<Gear>().OfType<Officer>()
                where o.Nickname != "Foo"
                select new
                {
                    o.Nickname,
                    Collection = (from r in o.Reports
                                  where !r.HasSoulPatch
                                  select new { r.Nickname, r.FullName }).ToArray()
                },
            elementSorter: e => e.Nickname,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Nickname, a.Nickname);
                AssertCollection(
                    e.Collection,
                    a.Collection,
                    elementSorter: ee => ee.FullName,
                    elementAsserter: (ee, aa) =>
                    {
                        Assert.Equal(ee.FullName, aa.FullName);
                        Assert.Equal(ee.Nickname, aa.Nickname);
                    });
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Correlated_collections_basic_projecting_single_property(bool async)
        => AssertQuery(
            async,
            ss => from g in ss.Set<Gear>()
                  where g.Nickname != "Marcus"
                  orderby g.Nickname
                  select (from w in g.Weapons
                          where w.IsAutomatic || w.Name != "foo"
                          select w.Name).ToList(),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Correlated_collections_basic_projecting_constant(bool async)
        => AssertQuery(
            async,
            ss => from g in ss.Set<Gear>()
                  where g.Nickname != "Marcus"
                  orderby g.Nickname
                  select (from w in g.Weapons
                          where w.IsAutomatic || w.Name != "foo"
                          select "BFG").ToList(),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Correlated_collections_basic_projecting_constant_bool(bool async)
        => AssertQuery(
            async,
            ss => from g in ss.Set<Gear>()
                  where g.Nickname != "Marcus"
                  orderby g.Nickname
                  select (from w in g.Weapons
                          where w.IsAutomatic || w.Name != "foo"
                          select true).ToList(),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Correlated_collections_projection_of_collection_thru_navigation(bool async)
        => AssertQuery(
            async,
            ss => from g in ss.Set<Gear>()
                  orderby g.FullName
                  where g.Nickname != "Marcus"
                  select g.Squad.Missions.Where(m => m.MissionId != 17).ToList(),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Correlated_collections_project_anonymous_collection_result(bool async)
        => AssertQuery(
            async,
            ss => from s in ss.Set<Squad>()
                  where s.Id < 20
                  select new
                  {
                      s.Name,
                      Collection = (from m in s.Members
                                    select new { m.FullName, m.Rank }).ToList()
                  },
            elementSorter: e => e.Name,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Name, a.Name);
                AssertCollection(e.Collection, a.Collection, elementSorter: ee => ee.FullName);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Correlated_collections_nested(bool async)
        => AssertQuery(
            async,
            ss => from s in ss.Set<Squad>()
                  select (from m in s.Missions
                          where m.MissionId < 42
                          select (from ps in m.Mission.ParticipatingSquads
                                  where ps.SquadId < 7
                                  select ps).ToList()).ToList(),
            elementSorter: e => e.Count(),
            elementAsserter: (e, a) => AssertCollection(
                e,
                a,
                elementSorter: ee => ee.Count(),
                elementAsserter: (ee, aa) => AssertCollection(ee, aa)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Correlated_collections_nested_mixed_streaming_with_buffer1(bool async)
        => AssertQuery(
            async,
            ss => from s in ss.Set<Squad>()
                  select (from m in s.Missions
                          where m.MissionId < 3
                          select (from ps in m.Mission.ParticipatingSquads
                                  where ps.SquadId < 2
                                  select ps).ToList()),
            elementSorter: e => e.Count(),
            elementAsserter: (e, a) => AssertCollection(
                e,
                a,
                elementSorter: ee => ee.Count(),
                elementAsserter: (ee, aa) => AssertCollection(ee, aa)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Correlated_collections_nested_mixed_streaming_with_buffer2(bool async)
        => AssertQuery(
            async,
            ss => from s in ss.Set<Squad>()
                  select (from m in s.Missions
                          where m.MissionId < 42
                          select (from ps in m.Mission.ParticipatingSquads
                                  where ps.SquadId < 7
                                  select ps)).ToList(),
            elementSorter: e => e.Count(),
            elementAsserter: (e, a) => AssertCollection(
                e,
                a,
                elementSorter: ee => ee.Count(),
                elementAsserter: (ee, aa) => AssertCollection(ee, aa)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Correlated_collections_nested_with_custom_ordering(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>()
                .OfType<Officer>()
                .OrderByDescending(o => o.HasSoulPatch)
                .Select(
                    o => new
                    {
                        o.FullName,
                        OuterCollection = o.Reports
                            .Where(r => r.FullName != "Foo")
                            .OrderBy(r => r.Rank)
                            .Select(
                                g => new
                                {
                                    g.FullName,
                                    InnerCollection = g.Weapons
                                        .Where(w => w.Name != "Bar")
                                        .OrderBy(w => w.IsAutomatic).ToList()
                                }).ToList()
                    }),
            elementSorter: e => e.FullName,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.FullName, a.FullName);
                AssertCollection(
                    e.OuterCollection,
                    a.OuterCollection,
                    elementSorter: ee => ee.FullName,
                    elementAsserter: (ee, aa) =>
                    {
                        Assert.Equal(ee.FullName, aa.FullName);
                        AssertCollection(ee.InnerCollection, aa.InnerCollection);
                    });
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Correlated_collections_same_collection_projected_multiple_times(bool async)
        => AssertQuery(
            async,
            ss =>
                from g in ss.Set<Gear>()
                select new
                {
                    g.FullName,
                    First = g.Weapons.Where(w1 => w1.IsAutomatic).ToList(),
                    Second = g.Weapons.Where(w2 => w2.IsAutomatic).ToList()
                },
            elementSorter: e => e.FullName,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.FullName, a.FullName);
                AssertCollection(e.First, a.First);
                AssertCollection(e.Second, a.Second);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Correlated_collections_similar_collection_projected_multiple_times(bool async)
        => AssertQuery(
            async,
            ss =>
                from g in ss.Set<Gear>()
                orderby g.Rank
                select new
                {
                    g.FullName,
                    First = g.Weapons.OrderBy(w1 => w1.OwnerFullName).Where(w1 => w1.IsAutomatic).ToList(),
                    Second = g.Weapons.OrderBy(w2 => w2.IsAutomatic).Where(w2 => !w2.IsAutomatic).ToArray()
                },
            elementSorter: e => e.FullName,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.FullName, a.FullName);
                AssertCollection(e.First, a.First);
                AssertCollection(e.Second, a.Second);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Correlated_collections_different_collections_projected(bool async)
        => AssertQuery(
            async,
            ss =>
                from o in ss.Set<Gear>().OfType<Officer>()
                orderby o.FullName
                select new
                {
                    o.Nickname,
                    First = o.Weapons.Where(w => w.IsAutomatic).Select(
                        w => new { w.Name, w.IsAutomatic }).ToArray(),
                    Second = o.Reports.OrderBy(r => r.FullName).Select(
                        r => new { r.Nickname, r.Rank }).ToList()
                },
            assertOrder: true,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Nickname, a.Nickname);
                AssertCollection(e.First, a.First, elementSorter: ee => ee.Name);
                AssertCollection(e.Second, a.Second, ordered: true);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Multiple_orderby_with_navigation_expansion_on_one_of_the_order_bys(bool async)
        => AssertQuery(
            async,
            ss =>
                from o in ss.Set<Gear>().OfType<Officer>()
                orderby o.HasSoulPatch descending, o.Tag.Note
                where o.Reports.Any()
                select o.FullName,
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Multiple_orderby_with_navigation_expansion_on_one_of_the_order_bys_inside_subquery(bool async)
        => AssertQuery(
            async,
            ss =>
                from o in ss.Set<Gear>().OfType<Officer>()
                orderby o.HasSoulPatch descending, o.Tag.Note
                where o.Reports.Any()
                select new
                {
                    o.FullName,
                    OuterCollection2 = (from www in o.Tag.Gear.Weapons
                                        orderby www.IsAutomatic, www.Owner.Nickname descending
                                        select www).ToList()
                },
            elementSorter: e => e.FullName,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.FullName, a.FullName);
                AssertCollection(e.OuterCollection2, a.OuterCollection2, ordered: true);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Multiple_orderby_with_navigation_expansion_on_one_of_the_order_bys_inside_subquery_duplicated_orderings(
        bool async)
        => AssertQuery(
            async,
            ss =>
                from o in ss.Set<Gear>().OfType<Officer>()
                orderby o.HasSoulPatch descending, o.Tag.Note
                where o.Reports.Any()
                select new
                {
                    o.FullName,
                    OuterCollection2 = (from www in o.Tag.Gear.Weapons
                                        orderby www.IsAutomatic, www.Owner.Nickname descending
                                        orderby www.IsAutomatic, www.Owner.Nickname descending
                                        select www).ToList()
                },
            elementSorter: e => e.FullName,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.FullName, a.FullName);
                AssertCollection(e.OuterCollection2, a.OuterCollection2, ordered: true);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Multiple_orderby_with_navigation_expansion_on_one_of_the_order_bys_inside_subquery_complex_orderings(
        bool async)
        => AssertQuery(
            async,
            ss =>
                from o in ss.Set<Gear>().OfType<Officer>()
                orderby o.HasSoulPatch descending, o.Tag.Note
                where o.Reports.Any()
                select new
                {
                    o.FullName,
                    OuterCollection2 = (from www in o.Tag.Gear.Weapons
                                        orderby www.Id descending, www.Owner.Weapons.Count
                                        select www).ToList()
                },
            elementSorter: e => e.FullName,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.FullName, a.FullName);
                AssertCollection(e.OuterCollection2, a.OuterCollection2, ordered: true);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Correlated_collections_multiple_nested_complex_collections(bool async)
        => AssertQuery(
            async,
            ss =>
                from o in ss.Set<Gear>().OfType<Officer>()
                orderby o.HasSoulPatch descending, o.Tag.Note
                where o.Reports.Any()
                select new
                {
                    o.FullName,
                    OuterCollection = (from r in o.Reports
                                       where r.FullName != "Foo"
                                       orderby r.Rank
                                       select new
                                       {
                                           r.FullName,
                                           InnerCollection = (from w in r.Weapons
                                                              where w.Name != "Bar"
                                                              orderby w.IsAutomatic
                                                              select new
                                                              {
                                                                  w.Id,
                                                                  InnerFirst = w.Owner.Weapons.Select(
                                                                      ww => new { ww.Name, ww.IsAutomatic }).ToList(),
                                                                  InnerSecond = w.Owner.Squad.Members.OrderBy(mm => mm.Nickname).Select(
                                                                      mm => new { mm.Nickname, mm.HasSoulPatch }).ToList()
                                                              }).ToList()
                                       }).ToList(),
                    OuterCollection2 = (from www in o.Tag.Gear.Weapons
                                        orderby www.IsAutomatic, www.Owner.Nickname descending
                                        select www).ToList()
                },
            elementSorter: e => e.FullName,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.FullName, a.FullName);
                AssertCollection(
                    e.OuterCollection,
                    a.OuterCollection,
                    elementSorter: ee => ee.FullName,
                    elementAsserter: (ee, aa) =>
                    {
                        Assert.Equal(ee.FullName, aa.FullName);
                        AssertCollection(
                            ee.InnerCollection,
                            aa.InnerCollection,
                            elementSorter: eee => eee.Id,
                            elementAsserter: (eee, aaa) =>
                            {
                                Assert.Equal(eee.Id, aaa.Id);
                                AssertCollection(
                                    eee.InnerFirst,
                                    aaa.InnerFirst,
                                    elementSorter: eeee => eeee.Name);
                                AssertCollection(
                                    eee.InnerSecond,
                                    aaa.InnerSecond,
                                    elementSorter: eeee => eeee.Nickname);
                            });
                    });
                AssertCollection(e.OuterCollection2, a.OuterCollection2, ordered: true);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Correlated_collections_inner_subquery_selector_references_outer_qsre(bool async)
        => AssertQuery(
            async,
            ss =>
                from o in ss.Set<Gear>().OfType<Officer>()
                select new
                {
                    o.FullName,
                    Collection = from r in o.Reports
                                 select new { ReportName = r.FullName, OfficerName = o.FullName }
                },
            elementSorter: e => e.FullName,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.FullName, a.FullName);
                AssertCollection(e.Collection, a.Collection, elementSorter: ee => (ee.ReportName, ee.OfficerName));
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Correlated_collections_inner_subquery_predicate_references_outer_qsre(bool async)
        => AssertQuery(
            async,
            ss =>
                from o in ss.Set<Gear>().OfType<Officer>()
                select new
                {
                    o.FullName,
                    Collection = from r in o.Reports
                                 where o.FullName != "Foo"
                                 select new { ReportName = r.FullName }
                },
            elementSorter: e => e.FullName,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.FullName, a.FullName);
                AssertCollection(e.Collection, a.Collection, elementSorter: ee => ee.ReportName);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Correlated_collections_nested_inner_subquery_references_outer_qsre_one_level_up(bool async)
        => AssertQuery(
            async,
            ss =>
                from o in ss.Set<Gear>().OfType<Officer>()
                select new
                {
                    o.FullName,
                    OuterCollection = (from r in o.Reports
                                       where r.FullName != "Foo"
                                       select new
                                       {
                                           r.FullName,
                                           InnerCollection = (from w in r.Weapons
                                                              where w.Name != "Bar"
                                                              select new { w.Name, r.Nickname }).ToList()
                                       }).ToList()
                },
            elementSorter: e => e.FullName,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.FullName, a.FullName);
                AssertCollection(
                    e.OuterCollection,
                    a.OuterCollection,
                    elementSorter: ee => ee.FullName,
                    elementAsserter: (ee, aa) =>
                    {
                        Assert.Equal(ee.FullName, aa.FullName);
                        AssertCollection(ee.InnerCollection, aa.InnerCollection, elementSorter: eee => (eee.Name, eee.Nickname));
                    });
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Correlated_collections_nested_inner_subquery_references_outer_qsre_two_levels_up(bool async)
        => AssertQuery(
            async,
            ss =>
                from o in ss.Set<Gear>().OfType<Officer>()
                select new
                {
                    o.FullName,
                    OuterCollection = from r in o.Reports
                                      where r.FullName != "Foo"
                                      select new
                                      {
                                          r.FullName,
                                          InnerCollection = from w in r.Weapons
                                                            where w.Name != "Bar"
                                                            select new { w.Name, o.Nickname }
                                      }
                },
            elementSorter: e => e.FullName,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.FullName, a.FullName);
                AssertCollection(
                    e.OuterCollection,
                    a.OuterCollection,
                    elementSorter: ee => ee.FullName,
                    elementAsserter: (ee, aa) =>
                    {
                        Assert.Equal(ee.FullName, aa.FullName);
                        AssertCollection(ee.InnerCollection, aa.InnerCollection, elementSorter: eee => (eee.Name, eee.Nickname));
                    });
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Correlated_collections_on_select_many(bool async)
        => AssertQuery(
            async,
            ss =>
                from g in ss.Set<Gear>()
                from s in ss.Set<Squad>()
                where g.HasSoulPatch
                orderby g.Nickname, s.Id descending
                select new
                {
                    GearNickname = g.Nickname,
                    SquadName = s.Name,
                    Collection1 = from w in g.Weapons
                                  where w.IsAutomatic || w.Name != "foo"
                                  select w,
                    Collection2 = from m in s.Members
                                  where !m.HasSoulPatch
                                  select m
                },
            assertOrder: true,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.GearNickname, e.GearNickname);
                Assert.Equal(e.SquadName, e.SquadName);

                AssertCollection(e.Collection1, a.Collection1);
                AssertCollection(e.Collection2, a.Collection2);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Correlated_collections_with_Skip(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Squad>().OrderBy(s => s.Name).Select(s => s.Members.OrderBy(m => m.Nickname).Skip(1)),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a, ordered: true));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Correlated_collections_with_Take(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Squad>().OrderBy(s => s.Name).Select(s => s.Members.OrderBy(m => m.Nickname).Take(2)),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a, ordered: true));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Correlated_collections_with_Distinct(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Squad>().OrderBy(s => s.Name).Select(s => s.Members.OrderBy(m => m.Nickname).Skip(0).Distinct()),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a, ordered: true));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Correlated_collections_with_FirstOrDefault(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Squad>().OrderBy(s => s.Name)
                .Select(s => s.Members.OrderBy(m => m.Nickname).Select(m => m.FullName).FirstOrDefault()),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Correlated_collections_on_left_join_with_predicate(bool async)
        => AssertQuery(
            async,
            ss => from t in ss.Set<CogTag>()
                  join g in ss.Set<Gear>() on t.GearNickName equals g.Nickname into grouping
                  from g in grouping.DefaultIfEmpty()
                  where !g.HasSoulPatch
                  select new { g.Nickname, WeaponNames = g.Weapons.Select(w => w.Name).ToList() },
            ss => from t in ss.Set<CogTag>()
                  join g in ss.Set<Gear>() on t.GearNickName equals g.Nickname into grouping
                  from g in grouping.DefaultIfEmpty()
                  where g != null && !g.HasSoulPatch
                  select new { g.Nickname, WeaponNames = g.Weapons.Select(w => w.Name).ToList() },
            elementSorter: e => e.Nickname,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Nickname, a.Nickname);
                AssertCollection(e.WeaponNames, a.WeaponNames);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Correlated_collections_on_left_join_with_null_value(bool async)
        => AssertQuery(
            async,
            ss => from t in ss.Set<CogTag>()
                  join g in ss.Set<Gear>() on t.GearNickName equals g.Nickname into grouping
                  from g in grouping.DefaultIfEmpty()
                  orderby t.Note
                  select g.Weapons.Select(w => w.Name).ToList(),
            ss =>
                from t in ss.Set<CogTag>()
                join g in ss.Set<Gear>() on t.GearNickName equals g.Nickname into grouping
                from g in grouping.DefaultIfEmpty()
                orderby t.Note
                select g != null ? g.Weapons.Select(w => w.Name).ToList() : new List<string>(),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Correlated_collections_left_join_with_self_reference(bool async)
        => AssertQuery(
            async,
            ss =>
                from t in ss.Set<CogTag>()
                join o in ss.Set<Gear>().OfType<Officer>() on t.GearNickName equals o.Nickname into grouping
                from o in grouping.DefaultIfEmpty()
                select new { t.Note, ReportNames = o.Reports.Select(r => r.FullName).ToList() },
            ss =>
                from t in ss.Set<CogTag>()
                join o in ss.Set<Gear>().OfType<Officer>() on t.GearNickName equals o.Nickname into grouping
                from o in grouping.DefaultIfEmpty()
                select new { t.Note, ReportNames = o != null ? o.Reports.Select(r => r.FullName).ToList() : new List<string>() },
            elementSorter: e => e.Note,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Note, a.Note);
                AssertCollection(e.ReportNames, a.ReportNames);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Correlated_collections_deeply_nested_left_join(bool async)
        => AssertQuery(
            async,
            ss =>
                from t in ss.Set<CogTag>()
                join g in ss.Set<Gear>() on t.GearNickName equals g.Nickname into grouping
                from g in grouping.DefaultIfEmpty()
                orderby t.Note, g.Nickname descending
                select g.Squad.Members.Where(m => m.HasSoulPatch).Select(
                    m => new { m.Nickname, AutomaticWeapons = m.Weapons.Where(w => w.IsAutomatic).ToList() }).ToList(),
            ss =>
                from t in ss.Set<CogTag>()
                join g in ss.Set<Gear>() on t.GearNickName equals g.Nickname into grouping
                from g in grouping.DefaultIfEmpty()
                orderby t.Note, g.Nickname descending
                select
                    g != null
                        ? g.Squad.Members.Where(m => m.HasSoulPatch).OrderBy(m => m.Nickname)
                            .Select(m => new { m.Nickname, AutomaticWeapons = m.Weapons.Where(w => w.IsAutomatic).ToList() }).ToList()
                        : Enumerable.Empty<int>().Select(x => new { Nickname = (string)null, AutomaticWeapons = new List<Weapon>() })
                            .ToList(),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(
                e,
                a,
                elementSorter: e => e.Nickname,
                elementAsserter: (ee, aa) =>
                {
                    Assert.Equal(ee.Nickname, aa.Nickname);
                    AssertCollection(ee.AutomaticWeapons, aa.AutomaticWeapons);
                }));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Correlated_collections_from_left_join_with_additional_elements_projected_of_that_join(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Weapon>().OrderBy(w => w.Name).Select(
                w => w.Owner.Squad.Members.OrderByDescending(m => m.FullName).Select(
                    m => new { Weapons = m.Weapons.Where(ww => !ww.IsAutomatic).OrderBy(ww => ww.Id).ToList(), m.Rank }).ToList()),
            ss => ss.Set<Weapon>().OrderBy(w => w.Name).Select(
                w => w.Owner != null
                    ? w.Owner.Squad.Members.OrderByDescending(m => m.FullName).Select(
                        m => new { Weapons = m.Weapons.Where(ww => !ww.IsAutomatic).OrderBy(ww => ww.Id).ToList(), m.Rank }).ToList()
                    : Enumerable.Empty<int>().Select(x => new { Weapons = new List<Weapon>(), Rank = default(MilitaryRank) })),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(
                e, a, ordered: true, elementAsserter: (ee, aa) =>
                {
                    Assert.Equal(ee.Rank, aa.Rank);
                    AssertCollection(ee.Weapons, aa.Weapons, ordered: true);
                }));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Correlated_collections_complex_scenario1(bool async)
        => AssertQuery(
            async,
            ss =>
                from r in ss.Set<Gear>()
                select new
                {
                    r.FullName,
                    OuterCollection = (from w in r.Weapons
                                       select new
                                       {
                                           w.Id,
                                           InnerCollection = w.Owner.Squad.Members.OrderBy(mm => mm.Nickname).Select(
                                               mm => new { mm.Nickname, mm.HasSoulPatch }).ToList()
                                       }).ToList()
                },
            elementSorter: e => e.FullName,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.FullName, a.FullName);
                AssertCollection(
                    e.OuterCollection,
                    a.OuterCollection,
                    elementSorter: ee => ee.Id,
                    elementAsserter: (ee, aa) =>
                    {
                        Assert.Equal(ee.Id, aa.Id);
                        AssertCollection(ee.InnerCollection, aa.InnerCollection, elementSorter: eee => eee.Nickname);
                    });
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Correlated_collections_complex_scenario2(bool async)
        => AssertQuery(
            async,
            ss =>
                from o in ss.Set<Gear>().OfType<Officer>()
                select new
                {
                    o.FullName,
                    OuterCollection = (from r in o.Reports
                                       select new
                                       {
                                           r.FullName,
                                           InnerCollection = (from w in r.Weapons
                                                              select new
                                                              {
                                                                  w.Id,
                                                                  InnerSecond = w.Owner.Squad.Members.OrderBy(mm => mm.Nickname).Select(
                                                                      mm => new { mm.Nickname, mm.HasSoulPatch }).ToList()
                                                              }).ToList()
                                       }).ToList()
                },
            elementSorter: e => e.FullName,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.FullName, a.FullName);
                AssertCollection(
                    e.OuterCollection,
                    a.OuterCollection,
                    elementSorter: ee => ee.FullName,
                    elementAsserter: (ee, aa) =>
                    {
                        Assert.Equal(ee.FullName, aa.FullName);
                        AssertCollection(
                            ee.InnerCollection,
                            aa.InnerCollection,
                            elementSorter: eee => eee.Id,
                            elementAsserter: (eee, aaa) =>
                            {
                                Assert.Equal(eee.Id, aaa.Id);
                                AssertCollection(eee.InnerSecond, aaa.InnerSecond, elementSorter: eeee => eeee.Nickname);
                            });
                    });
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Correlated_collections_with_funky_orderby_complex_scenario1(bool async)
        => AssertQuery(
            async,
            ss =>
                from r in ss.Set<Gear>()
                orderby r.FullName, r.Nickname descending, r.FullName
                select new
                {
                    r.FullName,
                    OuterCollection = (from w in r.Weapons
                                       select new
                                       {
                                           w.Id,
                                           InnerCollection = w.Owner.Squad.Members.OrderBy(mm => mm.Nickname).Select(
                                               mm => new { mm.Nickname, mm.HasSoulPatch }).ToList()
                                       }).ToList()
                },
            assertOrder: true,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.FullName, a.FullName);
                AssertCollection(
                    e.OuterCollection,
                    a.OuterCollection,
                    elementSorter: ee => ee.Id,
                    elementAsserter: (ee, aa) =>
                    {
                        Assert.Equal(ee.Id, aa.Id);
                        AssertCollection(ee.InnerCollection, aa.InnerCollection, elementSorter: eee => eee.Nickname);
                    });
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Correlated_collections_with_funky_orderby_complex_scenario2(bool async)
        => AssertQuery(
            async,
            ss =>
                from o in ss.Set<Gear>().OfType<Officer>()
                orderby o.HasSoulPatch, o.LeaderNickname, o.HasSoulPatch descending, o.LeaderNickname descending, o.FullName
                select new
                {
                    o.FullName,
                    OuterCollection = (from r in o.Reports
                                       orderby r.FullName, r.HasSoulPatch descending, r.FullName descending
                                       select new
                                       {
                                           r.FullName,
                                           InnerCollection = (from w in r.Weapons
                                                              orderby w.IsAutomatic, w.Name descending, w.Name, w.IsAutomatic descending
                                                                  , w.IsAutomatic descending
                                                              select new
                                                              {
                                                                  w.Id,
                                                                  InnerSecond = w.Owner.Squad.Members.OrderBy(mm => mm.Nickname).Select(
                                                                      mm => new { mm.Nickname, mm.HasSoulPatch }).ToList()
                                                              }).ToList()
                                       }).ToList()
                },
            assertOrder: true,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.FullName, a.FullName);
                AssertCollection(
                    e.OuterCollection,
                    a.OuterCollection,
                    ordered: true,
                    elementAsserter: (ee, aa) =>
                    {
                        Assert.Equal(ee.FullName, aa.FullName);
                        AssertCollection(
                            ee.InnerCollection,
                            aa.InnerCollection,
                            ordered: true,
                            elementAsserter: (eee, aaa) =>
                            {
                                Assert.Equal(eee.Id, aaa.Id);
                                AssertCollection(eee.InnerSecond, aaa.InnerSecond, ordered: true);
                            });
                    });
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Correlated_collection_with_top_level_FirstOrDefault(bool async)
        => AssertFirstOrDefault(
            async,
            ss => ss.Set<Gear>().OrderBy(g => g.Nickname).Select(g => g.Weapons),
            asserter: (e, a) => AssertCollection(e, a));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Correlated_collection_with_top_level_Count(bool async)
        => AssertCount(
            async,
            ss => ss.Set<Gear>().Select(g => g.Weapons));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Correlated_collection_with_top_level_Last_with_orderby_on_outer(bool async)
        => AssertLast(
            async,
            ss => ss.Set<Gear>().OrderByDescending(g => g.FullName).Select(g => g.Weapons),
            asserter: (e, a) => AssertCollection(e, a));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Correlated_collection_with_top_level_Last_with_order_by_on_inner(bool async)
        => AssertLast(
            async,
            ss => ss.Set<Gear>().OrderBy(g => g.FullName).Select(g => g.Weapons.OrderBy(w => w.Name).ToList()),
            asserter: (e, a) => AssertCollection(e, a, ordered: true));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Null_semantics_on_nullable_bool_from_inner_join_subquery_is_fully_applied(bool async)
        => AssertQuery(
            async,
            ss =>
                from ll in ss.Set<LocustLeader>()
                join h in ss.Set<Faction>().OfType<LocustHorde>().Where(f => f.Name == "Swarm") on ll.Name equals h.CommanderName
                where h.Eradicated != true
                select h);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Null_semantics_on_nullable_bool_from_left_join_subquery_is_fully_applied(bool async)
        => AssertQuery(
            async,
            ss =>
                from ll in ss.Set<LocustLeader>()
                join h in ss.Set<Faction>().OfType<LocustHorde>().Where(f => f.Name == "Swarm") on ll.Name equals h.CommanderName into
                    grouping
                from h in grouping.DefaultIfEmpty()
                where h.Eradicated != true
                select h);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_on_derived_type_with_order_by_and_paging(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<LocustLeader>().Include(ll => ((LocustCommander)ll).DefeatedBy).ThenInclude(g => g.Weapons)
                .OrderBy(ll => ((LocustCommander)ll).DefeatedBy.Tag.Note).Take(10),
            ss => ss.Set<LocustLeader>().Take(10),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<LocustCommander>(e1 => e1.DefeatedBy),
                new ExpectedInclude<Gear>(e2 => e2.Weapons, "DefeatedBy")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_required_navigation_on_derived_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<LocustLeader>().Select(ll => ((LocustCommander)ll).HighCommand.Name),
            ss => ss.Set<LocustLeader>().Select(ll => ll is LocustCommander ? ((LocustCommander)ll).HighCommand.Name : null));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_required_navigation_on_the_same_type_with_cast(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>().Select(g => g.CityOfBirth.Name),
            ss => ss.Set<Gear>().Select(g => g.CityOfBirth.Name));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_required_navigation_on_derived_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<LocustLeader>().Where(ll => ((LocustCommander)ll).HighCommand.IsOperational),
            ss => ss.Set<LocustLeader>().Where(ll => ll is LocustCommander ? ((LocustCommander)ll).HighCommand.IsOperational : false));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Outer_parameter_in_join_key(bool async)
        => AssertQuery(
            async,
            ss =>
                from o in ss.Set<Gear>().OfType<Officer>()
                orderby o.Nickname
                select new
                {
                    Collection = (from t in ss.Set<CogTag>()
                                  join g in ss.Set<Gear>() on o.FullName equals g.FullName
                                  select t.Note).ToList()
                },
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e.Collection, a.Collection));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Outer_parameter_in_join_key_inner_and_outer(bool async)
        => AssertQuery(
            async,
            ss =>
                from o in ss.Set<Gear>().OfType<Officer>()
                orderby o.Nickname
                select new
                {
                    Collection = (from t in ss.Set<CogTag>()
                                  join g in ss.Set<Gear>() on o.FullName equals o.Nickname
                                  select t.Note).ToList()
                },
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e.Collection, a.Collection));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Outer_parameter_in_group_join_with_DefaultIfEmpty(bool async)
        => AssertQuery(
            async,
            ss =>
                from o in ss.Set<Gear>().OfType<Officer>()
                orderby o.Nickname
                select new
                {
                    Collection = (from t in ss.Set<CogTag>()
                                  join g in ss.Set<Gear>() on o.FullName equals g.FullName into grouping
                                  from g in grouping.DefaultIfEmpty()
                                  select t.Note).ToList()
                },
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e.Collection, a.Collection));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Include_with_concat(bool async)
        => Assert.Equal(
            CoreStrings.SetOperationWithDifferentIncludesInOperands,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => AssertQuery(
                    async,
                    ss => ss.Set<Gear>().Include(g => g.Squad).Concat(ss.Set<Gear>()),
                    elementAsserter: (e, a) => AssertInclude(
                        e, a,
                        new ExpectedInclude<Gear>(g => g.Squad),
                        new ExpectedInclude<Officer>(o => o.Squad))))).Message);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Negated_bool_ternary_inside_anonymous_type_in_projection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<CogTag>().Select(
                t => new { c = !(t.Gear.HasSoulPatch ? true : ((bool?)t.Gear.HasSoulPatch ?? true)) }),
            ss => ss.Set<CogTag>().Select(
                t => new
                {
                    c = !(t.Gear.MaybeScalar(x => x.HasSoulPatch) ?? false
                        ? true
                        : (t.Gear.MaybeScalar(x => x.HasSoulPatch) ?? true))
                }),
            elementSorter: e => e.c);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Order_by_entity_qsre(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>().OrderBy(g => g.AssignedCity).ThenByDescending(g => g.Nickname).Select(f => f.FullName),
            ss => ss.Set<Gear>().OrderBy(g => g.AssignedCity.Name).ThenByDescending(g => g.Nickname).Select(f => f.FullName),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Order_by_entity_qsre_with_inheritance(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<LocustLeader>().OfType<LocustCommander>().OrderBy(lc => lc.HighCommand).ThenBy(lc => lc.Name)
                .Select(lc => lc.Name),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Order_by_entity_qsre_composite_key(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Weapon>().OrderBy(w => w.Owner).ThenBy(w => w.Id).Select(w => w.Name),
            ss => ss.Set<Weapon>().OrderBy(w => w.Owner.Nickname)
                .ThenBy(w => w.Owner.MaybeScalar(x => x.SquadId))
                .ThenBy(w => w.Id).Select(w => w.Name),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Order_by_entity_qsre_with_other_orderbys(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Weapon>().OrderBy(w => w.IsAutomatic).ThenByDescending(w => w.Owner).ThenBy(w => w.SynergyWith)
                .ThenBy(w => w.Name),
            ss => ss.Set<Weapon>()
                .OrderBy(w => w.IsAutomatic)
                .ThenByDescending(w => w.Owner.Nickname)
                .ThenByDescending(w => w.Owner.MaybeScalar(x => x.SquadId))
                .ThenBy(w => w.SynergyWith.MaybeScalar(x => x.Id))
                .ThenBy(w => w.Name),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Join_on_entity_qsre_keys(bool async)
        => AssertQuery(
            async,
            ss => from w1 in ss.Set<Weapon>()
                  join w2 in ss.Set<Weapon>() on w1 equals w2
                  select new { Name1 = w1.Name, Name2 = w2.Name },
            elementSorter: e => e.Name1 + " " + e.Name2);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Join_on_entity_qsre_keys_composite_key(bool async)
        => AssertQuery(
            async,
            ss => from g1 in ss.Set<Gear>()
                  join g2 in ss.Set<Gear>() on g1 equals g2
                  select new { GearName1 = g1.FullName, GearName2 = g2.FullName },
            elementSorter: e => (e.GearName1, e.GearName2));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Join_on_entity_qsre_keys_inheritance(bool async)
        => AssertQuery(
            async,
            ss => from g in ss.Set<Gear>()
                  join o in ss.Set<Gear>().OfType<Officer>() on g equals o
                  select new { GearName = g.FullName, OfficerName = o.FullName },
            elementSorter: e => (e.GearName, e.OfficerName));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Join_on_entity_qsre_keys_outer_key_is_navigation(bool async)
        => AssertQuery(
            async,
            ss => from w1 in ss.Set<Weapon>()
                  join w2 in ss.Set<Weapon>() on w1.SynergyWith equals w2
                  select new { Name1 = w1.Name, Name2 = w2.Name },
            elementSorter: e => (e.Name1, e.Name2));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Join_on_entity_qsre_keys_inner_key_is_navigation(bool async)
        => AssertQuery(
            async,
            ss =>
                from c in ss.Set<City>()
                join g in ss.Set<Gear>() on c equals g.AssignedCity
                select new { CityName = c.Name, GearNickname = g.Nickname },
            elementSorter: e => (e.CityName, e.GearNickname));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Join_on_entity_qsre_keys_inner_key_is_navigation_composite_key(bool async)
        => AssertQuery(
            async,
            ss =>
                from g in ss.Set<Gear>()
                join t in ss.Set<CogTag>().Where(tt => tt.Note == "Cole's Tag" || tt.Note == "Dom's Tag") on g equals t.Gear
                select new { g.Nickname, t.Note },
            elementSorter: e => (e.Nickname, e.Note));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Join_on_entity_qsre_keys_inner_key_is_nested_navigation(bool async)
        => AssertQuery(
            async,
            ss =>
                from s in ss.Set<Squad>()
                join w in ss.Set<Weapon>().Where(ww => ww.IsAutomatic) on s equals w.Owner.Squad
                select new { SquadName = s.Name, WeaponName = w.Name },
            elementSorter: e => (e.SquadName, e.WeaponName));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GroupJoin_on_entity_qsre_keys_inner_key_is_nested_navigation(bool async)
        => AssertQuery(
            async,
            ss => from s in ss.Set<Squad>()
                  join w in ss.Set<Weapon>() on s equals w.Owner.Squad into grouping
                  from w in grouping.DefaultIfEmpty()
                  select new { SquadName = s.Name, WeaponName = w.Name },
            elementSorter: e => (e.SquadName, e.WeaponName));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Join_with_complex_key_selector(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Squad>()
                .Join(ss.Set<CogTag>().Where(t => t.Note == "Marcus' Tag"), o => true, i => true, (o, i) => new { o, i })
                .GroupJoin(
                    ss.Set<Gear>(),
                    oo => oo.o.Members.FirstOrDefault(v => v.Tag == oo.i),
                    ii => ii,
                    (k, g) => new
                    {
                        k.o,
                        k.i,
                        value = g.OrderBy(gg => gg.FullName).FirstOrDefault()
                    })
                .Select(r => new { r.o.Id, TagId = r.i.Id }),
            elementSorter: e => (e.Id, e.TagId));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Streaming_correlated_collection_issue_11403_returning_ordered_enumerable_throws(bool async)
        => AssertInvalidMaterializationType(
            () => AssertFirstOrDefault(
                async,
                ss => ss.Set<Gear>()
                    .OrderBy(g => g.Nickname)
                    .Select(g => g.Weapons.Where(w => !w.IsAutomatic).OrderBy(w => w.Id)),
                asserter: (e, a) => AssertCollection(e, a, ordered: true)),
            typeof(IOrderedEnumerable<Weapon>).ShortDisplayName());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Streaming_correlated_collection_issue_11403(bool async)
        => AssertFirstOrDefault(
            async,
            ss => ss.Set<Gear>()
                .OrderBy(g => g.Nickname)
                .Select(g => g.Weapons.Where(w => !w.IsAutomatic).OrderBy(w => w.Id).ToList()),
            asserter: (e, a) => AssertCollection(e, a, ordered: true));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_one_value_type_from_empty_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Squad>().Where(s => s.Name == "Kilo").Select(
                s => new { s.Name, SquadId = s.Members.Where(m => m.HasSoulPatch).Select(m => m.SquadId).FirstOrDefault() }));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_one_value_type_converted_to_nullable_from_empty_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Squad>().Where(s => s.Name == "Kilo").Select(
                s => new { s.Name, SquadId = s.Members.Where(m => m.HasSoulPatch).Select(m => (int?)m.SquadId).FirstOrDefault() }));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_one_value_type_with_client_projection_from_empty_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Squad>().Where(s => s.Name == "Kilo").Select(
                s => new
                {
                    s.Name,
                    SquadId = s.Members.Where(m => m.HasSoulPatch).Select(m => ClientFunction(m.SquadId, m.LeaderSquadId))
                        .FirstOrDefault()
                }),
            elementSorter: s => s.Name);

    private static int ClientFunction(int a, int b)
        => a + b + 1;

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filter_on_subquery_projecting_one_value_type_from_empty_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Squad>().Where(s => s.Name == "Kilo")
                .Where(s => s.Members.Where(m => m.HasSoulPatch).Select(m => m.SquadId).FirstOrDefault() != 0).Select(s => s.Name),
            assertEmpty: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_subquery_projecting_single_constant_int(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Squad>().Select(
                s => new { s.Name, Gear = s.Members.Where(g => g.HasSoulPatch).Select(g => 42).FirstOrDefault() }));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_subquery_projecting_single_constant_string(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Squad>().Select(
                s => new { s.Name, Gear = s.Members.Where(g => g.HasSoulPatch).Select(g => "Foo").FirstOrDefault() }));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_subquery_projecting_single_constant_bool(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Squad>().Select(
                s => new { s.Name, Gear = s.Members.Where(g => g.HasSoulPatch).Select(g => true).FirstOrDefault() }));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_subquery_projecting_single_constant_inside_anonymous(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Squad>().Select(
                s => new { s.Name, Gear = s.Members.Where(g => g.HasSoulPatch).Select(g => new { One = 1 }).FirstOrDefault() }));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_subquery_projecting_multiple_constants_inside_anonymous(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Squad>().Select(
                s => new
                {
                    s.Name,
                    Gear = s.Members.Where(g => g.HasSoulPatch).Select(
                        g => new { True1 = true, False1 = false }).FirstOrDefault()
                }));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_with_order_by_constant(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Squad>().Include(s => s.Members).OrderBy(s => 42).Select(s => s),
            expectedQuery: ss => ss.Set<Squad>(),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Squad>(s => s.Members)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Correlated_collection_order_by_constant(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>().OrderByDescending(s => 1).Select(
                g => new { g.Nickname, Weapons = g.Weapons.Select(w => w.Name).ToList() }),
            elementSorter: e => e.Nickname,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Nickname, a.Nickname);
                AssertCollection(e.Weapons, a.Weapons);
            });

    public class MyDTO;

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_subquery_projecting_single_constant_null_of_non_mapped_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Squad>().Select(
                s => new { s.Name, Gear = s.Members.Where(g => g.HasSoulPatch).Select(g => (MyDTO)null).FirstOrDefault() }));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_subquery_projecting_single_constant_of_non_mapped_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Squad>().Select(
                s => new { s.Name, Gear = s.Members.Where(g => g.HasSoulPatch).Select(g => new MyDTO()).FirstOrDefault() }),
            elementSorter: e => e.Name,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Name, a.Name);
                if (e.Gear == null)
                {
                    Assert.Null(a.Gear);
                }
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Correlated_collection_order_by_constant_null_of_non_mapped_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>().OrderByDescending(s => (MyDTO)null).Select(
                g => new { g.Nickname, Weapons = g.Weapons.Select(w => w.Name).ToList() }),
            elementSorter: e => e.Nickname,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Nickname, a.Nickname);
                AssertCollection(e.Weapons, a.Weapons);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_collection_OrderBy_aggregate(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>().OfType<Officer>()
                .Include(o => o.Reports)
                .OrderBy(o => o.Weapons.Count).ThenBy(o => o.Nickname),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Officer>(o => o.Reports)),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_collection_with_complex_OrderBy2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>().OfType<Officer>()
                .Include(o => o.Reports)
                .OrderBy(o => o.Weapons.OrderBy(w => w.Id).FirstOrDefault().IsAutomatic).ThenBy(o => o.Nickname),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Officer>(o => o.Reports)),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_collection_with_complex_OrderBy3(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>().OfType<Officer>()
                .Include(o => o.Reports)
                .OrderBy(o => o.Weapons.OrderBy(w => w.Id).Select(w => w.IsAutomatic).FirstOrDefault()).ThenBy(o => o.Nickname),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Officer>(o => o.Reports)),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Correlated_collection_with_complex_OrderBy(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>().OfType<Officer>()
                .OrderBy(o => o.Weapons.Count).ThenBy(g => g.Nickname)
                .Select(o => o.Reports.Where(g => !g.HasSoulPatch).ToList()),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Correlated_collection_with_very_complex_order_by(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>().OfType<Officer>()
                .OrderBy(
                    o => o.Weapons.Where(
                            w => w.IsAutomatic
                                == ss.Set<Gear>().Where(g => g.Nickname == "Marcus").Select(g => g.HasSoulPatch)
                                    .FirstOrDefault())
                        .Count()).ThenBy(g => g.Nickname)
                .Select(o => o.Reports.Where(g => !g.HasSoulPatch).ToList()),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Cast_to_derived_type_causes_client_eval(bool async)
        => Assert.ThrowsAsync<InvalidCastException>(() => AssertQuery(async, ss => ss.Set<Gear>().Cast<Officer>()));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Cast_to_derived_type_after_OfType_works(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>().OfType<Officer>().Cast<Officer>());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_subquery_boolean(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Gear>().Select(g => g.Weapons.OrderBy(w => w.Id).Select(w => w.IsAutomatic).FirstOrDefault()));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_subquery_boolean_with_pushdown(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Gear>().Select(g => g.Weapons.OrderBy(w => w.Id).FirstOrDefault().IsAutomatic));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_subquery_int_with_inside_cast_and_coalesce(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Gear>().Select(g => g.Weapons.OrderBy(w => w.Id).Select(w => (int?)w.Id).FirstOrDefault() ?? 42));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_subquery_int_with_outside_cast_and_coalesce(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Gear>().Select(g => (int?)g.Weapons.OrderBy(w => w.Id).Select(w => w.Id).FirstOrDefault() ?? 42));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_subquery_int_with_pushdown_and_coalesce(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Gear>().Select(g => (int?)g.Weapons.OrderBy(w => w.Id).FirstOrDefault().Id ?? 42));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_subquery_int_with_pushdown_and_coalesce2(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Gear>().Select(
                g => (int?)g.Weapons.OrderBy(w => w.Id).FirstOrDefault().Id ?? g.Weapons.OrderBy(w => w.Id).FirstOrDefault().Id));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_subquery_boolean_empty(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Gear>().Select(
                g => g.Weapons.Where(w => w.Name == "BFG").OrderBy(w => w.Id).Select(w => w.IsAutomatic).FirstOrDefault()));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_subquery_boolean_empty_with_pushdown(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Gear>().Select(
                g => (bool?)g.Weapons.Where(w => w.Name == "BFG").OrderBy(w => w.Id).FirstOrDefault().IsAutomatic),
            ss => ss.Set<Gear>().Select(g => (bool?)null));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_subquery_distinct_singleordefault_boolean1(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Gear>().Where(g => g.HasSoulPatch)
                .Select(g => g.Weapons.Where(w => w.Name.Contains("Lancer")).Distinct().Select(w => w.IsAutomatic).SingleOrDefault()),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_subquery_distinct_singleordefault_boolean2(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Gear>().Where(g => g.HasSoulPatch)
                .Select(g => g.Weapons.Where(w => w.Name.Contains("Lancer")).Select(w => w.IsAutomatic).Distinct().SingleOrDefault()),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_subquery_distinct_singleordefault_boolean_with_pushdown(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Gear>().Where(g => g.HasSoulPatch)
                .Select(g => g.Weapons.Where(w => w.Name.Contains("Lancer")).Distinct().SingleOrDefault().IsAutomatic),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_subquery_distinct_singleordefault_boolean_empty1(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Gear>().Where(g => g.HasSoulPatch)
                .Select(g => g.Weapons.Where(w => w.Name == "BFG").Distinct().Select(w => w.IsAutomatic).SingleOrDefault()));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_subquery_distinct_singleordefault_boolean_empty2(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Gear>().Where(g => g.HasSoulPatch)
                .Select(g => g.Weapons.Where(w => w.Name == "BFG").Select(w => w.IsAutomatic).Distinct().SingleOrDefault()));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_subquery_distinct_singleordefault_boolean_empty_with_pushdown(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Gear>().Where(g => g.HasSoulPatch)
                .Select(g => (bool?)g.Weapons.Where(w => w.Name == "BFG").Distinct().SingleOrDefault().IsAutomatic),
            ss => ss.Set<Gear>().Where(g => g.HasSoulPatch).Select(g => (bool?)null),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Cast_subquery_to_base_type_using_typed_ToList(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<City>().Where(c => c.Name == "Ephyra").Select(
                c => c.StationedGears.Select(
                    g => new Officer
                    {
                        CityOfBirthName = g.CityOfBirthName,
                        FullName = g.FullName,
                        HasSoulPatch = g.HasSoulPatch,
                        LeaderNickname = g.LeaderNickname,
                        LeaderSquadId = g.LeaderSquadId,
                        Nickname = g.Nickname,
                        Rank = g.Rank,
                        SquadId = g.SquadId
                    }).ToList<Gear>()),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Cast_ordered_subquery_to_base_type_using_typed_ToArray(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<City>().Where(c => c.Name == "Ephyra").Select(
                c => c.StationedGears.OrderByDescending(g => g.Nickname).Select(
                    g => new Officer
                    {
                        CityOfBirthName = g.CityOfBirthName,
                        FullName = g.FullName,
                        HasSoulPatch = g.HasSoulPatch,
                        LeaderNickname = g.LeaderNickname,
                        LeaderSquadId = g.LeaderSquadId,
                        Nickname = g.Nickname,
                        Rank = g.Rank,
                        SquadId = g.SquadId
                    }).ToArray<Gear>()),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a, ordered: true));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Correlated_collection_with_complex_order_by_funcletized_to_constant_bool(bool async)
    {
        var nicknames = new List<string>();
        return AssertQuery(
            async,
            ss => from g in ss.Set<Gear>()
                  orderby nicknames.Contains(g.Nickname) descending
                  select new { g.Nickname, Weapons = g.Weapons.Select(w => w.Name).ToList() },
            elementSorter: e => e.Nickname,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Nickname, a.Nickname);
                AssertCollection(e.Weapons, a.Weapons);
            });
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Double_order_by_on_nullable_bool_coming_from_optional_navigation(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Weapon>().Select(w => w.SynergyWith).OrderBy(w => w.IsAutomatic).OrderBy(w => w.IsAutomatic).ThenBy(w => w.Id),
            ss => ss.Set<Weapon>().Select(w => w.SynergyWith).OrderBy(w => w != null ? w.IsAutomatic : false)
                .ThenBy(w => w != null ? (int?)w.Id : null),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Double_order_by_on_Like(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Weapon>().Select(w => w.SynergyWith).OrderBy(w => EF.Functions.Like(w.Name, "%Lancer"))
                .OrderBy(w => EF.Functions.Like(w.Name, "%Lancer")).Select(w => w),
            ss => ss.Set<Weapon>().Select(w => w.SynergyWith).OrderBy(w => w != null ? w.Name.EndsWith("Lancer") : false)
                .Select(w => w));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Double_order_by_on_is_null(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Weapon>().Select(w => w.SynergyWith).OrderBy(w => w.Name == null).OrderBy(w => w.Name == null).Select(w => w),
            ss => ss.Set<Weapon>().Select(w => w.SynergyWith).OrderBy(w => w != null ? w.Name == null : false).Select(w => w));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Double_order_by_on_string_compare(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Weapon>().OrderBy(w => w.Name.CompareTo("Marcus' Lancer") == 0)
                .OrderBy(w => w.Name.CompareTo("Marcus' Lancer") == 0).ThenBy(w => w.Id),
            ss => ss.Set<Weapon>().OrderBy(w => w != null ? w.Name.CompareTo("Marcus' Lancer") == 0 : false).ThenBy(w => w.Id),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Double_order_by_binary_expression(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Weapon>().OrderBy(w => w.Id + 2).OrderBy(w => w.Id + 2).Select(w => new { Binary = w.Id + 2 }));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task String_compare_with_null_conditional_argument(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Weapon>().Select(w => w.SynergyWith).OrderBy(w => w.Name.CompareTo("Marcus' Lancer") == 0).Select(c => c),
            ss => ss.Set<Weapon>().Select(w => w.SynergyWith).OrderBy(w => w != null ? w.Name.CompareTo("Marcus' Lancer") == 0 : false)
                .Select(c => c));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task String_compare_with_null_conditional_argument2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Weapon>().Select(w => w.SynergyWith).OrderBy(w => "Marcus' Lancer".CompareTo(w.Name) == 0).Select(w => w),
            ss => ss.Set<Weapon>().Select(w => w.SynergyWith).OrderBy(w => w != null ? "Marcus' Lancer".CompareTo(w.Name) == 0 : false)
                .Select(w => w));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task String_concat_with_null_conditional_argument(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Weapon>().Select(w => w.SynergyWith).OrderBy(w => w.Name + 5),
            ss => ss.Set<Weapon>().Select(w => w.SynergyWith).OrderBy(w => w != null ? w.Name + 5 : null),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task String_concat_with_null_conditional_argument2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Weapon>().Select(w => w.SynergyWith).OrderBy(w => string.Concat(w.Name, "Marcus' Lancer")),
            ss => ss.Set<Weapon>().Select(w => w.SynergyWith).OrderBy(w => w != null ? string.Concat(w.Name, "Marcus' Lancer") : null),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task String_concat_nullable_expressions_are_coalesced(bool async)
    {
        object nullableParam = null;

        return AssertQuery(
            async,
            ss => ss.Set<Gear>().Select(w => w.FullName + null + w.LeaderNickname + nullableParam),
            ss => ss.Set<Gear>().Select(
                w => w.FullName + string.Empty + w.LeaderNickname ?? string.Empty + nullableParam ?? string.Empty));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task String_concat_on_various_types(bool async)
        => AssertQuery(
            async,
            ss => from g in ss.Set<Gear>()
                  from m in ss.Set<Mission>()
                  orderby g.Nickname, m.Id
                  select new
                  {
                      HasSoulPatch = string.Concat("HasSoulPatch " + g.HasSoulPatch, " HasSoulPatch"),
                      Rank = string.Concat("Rank " + g.Rank, " Rank"),
                      SquadId = string.Concat("SquadId " + g.SquadId, " SquadId"),
                      Rating = string.Concat("Rating " + m.Rating, " Rating"),
                      Timeline = string.Concat("Timeline " + m.Timeline, " Timeline")
                  },
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Time_of_day_datetimeoffset(bool async)
        => AssertQueryScalar(
            async,
            ss => from m in ss.Set<Mission>()
                  select m.Timeline.TimeOfDay);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_Property_Include_Select_Average(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Gear>().Include(g => g.CityOfBirth).GroupBy(g => g.Rank).Select(g => g.Average(gg => gg.SquadId)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_Property_Include_Select_Sum(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Gear>().Include(g => g.CityOfBirth).GroupBy(g => g.Rank).Select(g => g.Sum(gg => gg.SquadId)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_Property_Include_Select_Count(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Gear>().Include(g => g.CityOfBirth).GroupBy(g => g.Rank).Select(g => g.Count()));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_Property_Include_Select_LongCount(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Gear>().Include(g => g.CityOfBirth).GroupBy(g => g.Rank).Select(g => g.LongCount()));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_Property_Include_Select_Max(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Gear>().Include(g => g.CityOfBirth).GroupBy(g => g.Rank).Select(g => g.Max(gg => gg.SquadId)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_Property_Include_Select_Min(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Gear>().Include(g => g.CityOfBirth).GroupBy(g => g.Rank).Select(g => g.Min(gg => gg.SquadId)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_Property_Include_Aggregate_with_anonymous_selector(bool async)
        => AssertQuery(
            async,
            ss =>
                ss.Set<Gear>().Include(g => g.CityOfBirth).GroupBy(g => g.Nickname).OrderBy(g => g.Key)
                    .Select(
                        g => new { g.Key, c = g.Count() }),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Group_by_with_include_with_entity_in_result_selector(bool async)
        => AssertQuery(
            async,
            ss =>
                ss.Set<Gear>().Include(g => g.CityOfBirth).GroupBy(g => g.Rank).OrderBy(g => g.Key)
                    .Select(
                        g => new
                        {
                            g.Key,
                            c = g.Count(),
                            element = g.OrderBy(gg => gg.Nickname).FirstOrDefault()
                        }),
            assertOrder: true,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Key, a.Key);
                Assert.Equal(e.c, a.c);
                Assert.Equal(e.element.Nickname, a.element.Nickname);
                Assert.Equal(e.element.CityOfBirth == null, a.element.CityOfBirth == null);
                if (e.element.CityOfBirth != null)
                {
                    Assert.Equal(e.element.CityOfBirth.Name, a.element.CityOfBirth.Name);
                }
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_with_group_by_and_FirstOrDefault_gets_properly_applied(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>().Include(g => g.CityOfBirth).GroupBy(g => g.Rank).Select(g => g.FirstOrDefault(gg => gg.HasSoulPatch)),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<Gear>(e1 => e1.CityOfBirth),
                new ExpectedInclude<Officer>(e2 => e2.CityOfBirth)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_collection_with_Cast_to_base(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>().OfType<Officer>().Include(o => o.Weapons).Cast<Gear>(),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Gear>(e => e.Weapons)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_with_client_method_and_member_access_still_applies_includes(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>()
                .Include(g => g.Tag)
                .Select(g => new { g.Nickname, Client(g).FullName }));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_with_projection_of_unmapped_property_still_gets_applied(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Gear>().Include(g => g.Weapons).Select(g => g.IsMarcus));

    [ConditionalFact]
    public virtual Task Multiple_includes_with_client_method_around_entity_and_also_projecting_included_collection()
    {
        using var ctx = CreateContext();
        var query = ctx.Squads
            .Include(s => s.Members)
            .ThenInclude(g => g.Weapons)
            .Where(s => s.Name == "Delta")
            .Select(s => new { s.Name, Client(s).Members });

        var result = query.ToList();

        Assert.Single(result);

        var topLevel = result[0];

        Assert.Equal(4, topLevel.Members.Count);
        Assert.True(topLevel.Members.First().Weapons.Count > 0);

        return Task.CompletedTask;
    }

    public static TEntity Client<TEntity>(TEntity entity)
        => entity;

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OrderBy_same_expression_containing_IsNull_correctly_deduplicates_the_ordering(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Gear>().Select(g => g.LeaderNickname != null ? g.Nickname.Length == 5 : (bool?)null)
                .OrderBy(e => e.HasValue)
                .ThenBy(e => e.HasValue).Select(e => e));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GetValueOrDefault_in_projection(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Weapon>().Select(w => w.SynergyWithId.GetValueOrDefault()));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GetValueOrDefault_in_filter(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Weapon>().Where(w => w.SynergyWithId.GetValueOrDefault() == 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GetValueOrDefault_in_filter_non_nullable_column(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Weapon>().Where(w => ((int?)w.Id).GetValueOrDefault() == 0),
            assertEmpty: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GetValueOrDefault_on_DateTimeOffset(bool async)
    {
        var defaultValue = default(DateTimeOffset);

        return AssertQuery(
            async,
            ss => ss.Set<Mission>().Where(m => ((DateTimeOffset?)m.Timeline).GetValueOrDefault() == defaultValue),
            assertEmpty: true);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GetValueOrDefault_in_order_by(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Weapon>().OrderBy(w => w.SynergyWithId.GetValueOrDefault()).ThenBy(w => w.Id),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GetValueOrDefault_with_argument(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Weapon>().Where(w => w.SynergyWithId.GetValueOrDefault(w.Id) == 1));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GetValueOrDefault_with_argument_complex(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Weapon>().Where(w => w.SynergyWithId.GetValueOrDefault(w.Name.Length + 42) > 10),
            ss => ss.Set<Weapon>().Where(w => (w.SynergyWithId == null ? w.Name.Length + 42 : w.SynergyWithId) > 10));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filter_with_complex_predicate_containing_subquery(bool async)
        => AssertQuery(
            async,
            ss => from g in ss.Set<Gear>()
                  where g.FullName != "Dom" && g.Weapons.OrderBy(w => w.Id).FirstOrDefault(w => w.IsAutomatic) != null
                  select g);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Query_with_complex_let_containing_ordering_and_filter_projecting_firstOrDefault_element_of_let(bool async)
        => AssertQuery(
            async,
            ss => from g in ss.Set<Gear>()
                  where g.Nickname != "Dom"
                  let automaticWeapons
                      = g.Weapons
                          .OrderByDescending(w => w.AmmunitionType)
                          .Where(w => w.IsAutomatic)
                  select new { g.Nickname, WeaponName = automaticWeapons.FirstOrDefault().Name },
            elementSorter: e => e.Nickname,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Nickname, a.Nickname);
                Assert.Equal(e.WeaponName, a.WeaponName);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Null_semantics_is_correctly_applied_for_function_comparisons_that_take_arguments_from_optional_navigation(
        bool async)
        => AssertQuery(
            async,
            ss => ss.Set<CogTag>().Where(t => t.Note.Substring(0, t.Gear.SquadId) == t.GearNickName),
            ss => ss.Set<CogTag>().Where(t => t.Gear.Maybe(x => t.Note.Substring(0, x.SquadId)) == t.GearNickName));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task
        Null_semantics_is_correctly_applied_for_function_comparisons_that_take_arguments_from_optional_navigation_complex(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<CogTag>().Where(t => t.Note.Substring(0, t.Gear.Squad.Name.Length) == t.GearNickName),
            ss => ss.Set<CogTag>().Where(t => t.Gear.Maybe(x => t.Note.Substring(0, x.Squad.Name.Length)) == t.GearNickName));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filter_with_new_Guid(bool async)
        => AssertQuery(
            async,
            ss => from t in ss.Set<CogTag>()
                  where t.Id == new Guid("DF36F493-463F-4123-83F9-6B135DEEB7BA")
                  select t);

    public virtual async Task Filter_with_new_Guid_closure(bool async)
    {
        var guid = "DF36F493-463F-4123-83F9-6B135DEEB7BD";

        await AssertQuery(
            async,
            ss => from t in ss.Set<CogTag>()
                  where t.Id == new Guid(guid)
                  select t);

        guid = "B39A6FBA-9026-4D69-828E-FD7068673E57";

        await AssertQuery(
            async,
            ss => from t in ss.Set<CogTag>()
                  where t.Id == new Guid(guid)
                  select t);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OfTypeNav1(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>().Where(g => g.Tag.Note != "Foo").OfType<Officer>().Where(o => o.Tag.Note != "Bar"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OfTypeNav2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>().Where(g => g.Tag.Note != "Foo").OfType<Officer>().Where(o => o.AssignedCity.Location != "Bar"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OfTypeNav3(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>()
                .Where(g => g.Tag.Note != "Foo")
                .Join(
                    ss.Set<Weapon>(),
                    g => g.FullName,
                    w => w.OwnerFullName,
                    (o, i) => o)
                .OfType<Officer>()
                .Where(o => o.Tag.Note != "Bar"));

    [ConditionalFact]
    public virtual Task Nav_rewrite_Distinct_with_convert()
        // Issue #17328.
        => AssertTranslationFailed(
            () =>
            {
                using var ctx = CreateContext();
                _ = ctx.Factions.Include(f => ((LocustHorde)f).Commander)
                    .Where(f => f.Capital.Name != "Foo").Select(f => (LocustHorde)f)
                    .Distinct().Where(lh => lh.Commander.Name != "Bar").ToList();
                return Task.CompletedTask;
            });

    [ConditionalFact]
    public virtual Task Nav_rewrite_Distinct_with_convert_anonymous()
        // Issue #17328.
        => AssertTranslationFailed(
            () =>
            {
                using var ctx = CreateContext();
                _ = ctx.Factions.Include(f => ((LocustHorde)f).Commander)
                    .Where(f => f.Capital.Name != "Foo").Select(f => new { horde = (LocustHorde)f })
                    .Distinct().Where(lh => lh.horde.Commander.Name != "Bar").ToList();
                return Task.CompletedTask;
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Nav_rewrite_with_convert1(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Faction>().Where(f => f.Capital.Name != "Foo").Select(f => ((LocustHorde)f).Commander));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Nav_rewrite_with_convert2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Faction>()
                .Where(f => f.Capital.Name != "Foo")
                .Select(f => (LocustHorde)f)
                .Where(lh => lh.Commander.Name != "Bar"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Nav_rewrite_with_convert3(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Faction>()
                .Where(f => f.Capital.Name != "Foo")
                .Select(f => new { horde = (LocustHorde)f })
                .Where(x => x.horde.Commander.Name != "Bar"),
            elementSorter: e => e.horde.Id,
            elementAsserter: (e, a) => AssertEqual(e.horde, a.horde));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_contains_on_navigation_with_composite_keys(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>().Where(g => ss.Set<City>().Any(c => c.BornGears.Contains(g))));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_derivied_entity_with_convert_to_parent(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Faction>().OfType<LocustHorde>().Select(f => (Faction)f));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_with_complex_order_by(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>()
                .Include(g => g.Weapons)
                .OrderBy(g => g.Weapons.FirstOrDefault(w => w.Name.Contains("Gnasher")).Name)
                .ThenBy(g => g.Nickname),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Gear>(e => e.Weapons)),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Anonymous_projection_take_followed_by_projecting_single_element_from_collection_navigation(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>().Select(g => new { Gear = g }).Take(25)
                .Select(e => e.Gear.Weapons.OrderBy(w => w.Id).FirstOrDefault()));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Bool_projection_from_subquery_treated_appropriately_in_where(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<City>().Where(
                c => ss.Set<Gear>().OrderBy(g => g.Nickname).ThenBy(g => g.SquadId).FirstOrDefault().HasSoulPatch));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task DateTimeOffset_Contains_Less_than_Greater_than(bool async)
    {
        var dto = new DateTimeOffset(599898024001234567, new TimeSpan(1, 30, 0));
        var start = dto.AddDays(-1);
        var end = dto.AddDays(1);
        var dates = new[] { dto };

        return AssertQuery(
            async,
            ss => ss.Set<Mission>().Where(
                m => start <= m.Timeline.Date && m.Timeline < end && dates.Contains(m.Timeline)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task DateTimeOffsetNow_minus_timespan(bool async)
    {
        var timeSpan = new TimeSpan(1000);

        return AssertQuery(
            async,
            ss => ss.Set<Mission>().Where(e => e.Timeline > DateTimeOffset.Now - timeSpan),
            assertEmpty: true);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Navigation_inside_interpolated_string_expanded(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Weapon>().Select(
                w => w.SynergyWithId.HasValue ? $"SynergyWithOwner: {w.SynergyWith.OwnerFullName}" : string.Empty));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Left_join_projection_using_coalesce_tracking(bool async)
        => AssertQuery(
            async,
            ss => from g1 in ss.Set<Gear>().AsTracking()
                  join g2 in ss.Set<Gear>()
                      on g1.LeaderNickname equals g2.Nickname into grouping
                  from g2 in grouping.DefaultIfEmpty()
                  select g2 ?? g1);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Left_join_projection_using_conditional_tracking(bool async)
        => AssertQuery(
            async,
            ss => from g1 in ss.Set<Gear>().AsTracking()
                  join g2 in ss.Set<Gear>()
                      on g1.LeaderNickname equals g2.Nickname into grouping
                  from g2 in grouping.DefaultIfEmpty()
                  select g2 == null ? g1 : g2);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_collection_navigation_nested_with_take_composite_key(bool async)
        => AssertQuery(
            async,
            ss => from t in ss.Set<CogTag>()
                  where t.Gear is Officer
                  select ((Officer)t.Gear).Reports.Take(50),
            elementSorter: e => e?.Count() ?? 0,
            elementAsserter: (e, a) => AssertCollection(e, a));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_collection_navigation_nested_composite_key(bool async)
        => AssertQuery(
            async,
            ss => from t in ss.Set<CogTag>()
                  where t.Gear is Officer
                  select ((Officer)t.Gear).Reports,
            elementSorter: e => e?.Count ?? 0,
            elementAsserter: (e, a) => AssertCollection(e, a));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Null_checks_in_correlated_predicate_are_correctly_translated(bool async)
        => AssertQuery(
            async,
            ss => from t in ss.Set<CogTag>()
                  select new
                  {
                      key = t.Id,
                      collection = (from g in ss.Set<Gear>()
                                    where t.GearNickName == g.Nickname
                                        && t.GearSquadId != null
                                        && t.GearSquadId == g.SquadId
                                        && t.GearNickName != null
                                        && t.Note != null
                                        && null != t.Note
                                    select g).ToList()
                  },
            elementSorter: e => e.key,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.key, a.key);
                AssertCollection(e.collection, a.collection);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_Where_DefaultIfEmpty_with_navigation_in_the_collection_selector(bool async)
    {
        var isAutomatic = true;

        return AssertQuery(
            async,
            ss => from g in ss.Set<Gear>()
                  from w in g.Weapons.Where(ww => ww.IsAutomatic == isAutomatic).DefaultIfEmpty()
                  select new
                  {
                      g.Nickname,
                      g.FullName,
                      Collection = w != null
                  });
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_Where_DefaultIfEmpty_with_navigation_in_the_collection_selector_not_equal(bool async)
    {
        var isAutomatic = true;

        return AssertQuery(
            async,
            ss => from g in ss.Set<Gear>()
                  from w in g.Weapons.Where(ww => ww.IsAutomatic != isAutomatic).DefaultIfEmpty()
                  select new
                  {
                      g.Nickname,
                      g.FullName,
                      Collection = w != null
                  });
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_Where_DefaultIfEmpty_with_navigation_in_the_collection_selector_order_comparison(bool async)
    {
        var prm = 1;

        return AssertQuery(
            async,
            ss => from g in ss.Set<Gear>()
                  from w in g.Weapons.Where(ww => ww.Id > prm).DefaultIfEmpty()
                  select new
                  {
                      g.Nickname,
                      g.FullName,
                      Collection = w != null
                  });
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Join_with_inner_being_a_subquery_projecting_single_property(bool async)
        => AssertQuery(
            async,
            ss => from g in ss.Set<Gear>()
                  join inner in (
                      from g2 in ss.Set<Gear>()
                      select g2.Nickname
                  ) on g.Nickname equals inner
                  select g);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Join_with_inner_being_a_subquery_projecting_anonymous_type_with_single_property(bool async)
        => AssertQuery(
            async,
            ss => from g in ss.Set<Gear>()
                  join inner in (
                      from g2 in ss.Set<Gear>()
                      select new { g2.Nickname }
                  ) on g.Nickname equals inner.Nickname
                  select g);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Navigation_based_on_complex_expression1(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Faction>().Where(f => f is LocustHorde ? (f as LocustHorde).Commander != null : false));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Navigation_based_on_complex_expression2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Faction>().Where(f => f is LocustHorde).Where(f => ((LocustHorde)f).Commander != null));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Navigation_based_on_complex_expression3(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Faction>().Where(f => f is LocustHorde).Select(f => ((LocustHorde)f).Commander));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Navigation_based_on_complex_expression4(bool async)
        // Nav expansion. Issue #17782.
        => await Assert.ThrowsAsync<EqualException>(
                () => AssertQuery(
                    async,
                    ss => from lc1 in ss.Set<Faction>().Select(f => (f is LocustHorde) ? ((LocustHorde)f).Commander : null)
                          from lc2 in ss.Set<LocustLeader>().OfType<LocustCommander>()
                          select (lc1 ?? lc2).DefeatedBy));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Navigation_based_on_complex_expression5(bool async)
        // Nav expansion. Issue #17782.
        => await Assert.ThrowsAsync<EqualException>(
                () => AssertQuery(
                    async,
                    ss => from lc1 in ss.Set<Faction>().OfType<LocustHorde>().Select(lh => lh.Commander)
                          join lc2 in ss.Set<LocustLeader>().OfType<LocustCommander>() on true equals true
                          select (lc1 ?? lc2).DefeatedBy));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Navigation_based_on_complex_expression6(bool async)
        // Nav expansion. Issue #17782.
        => await Assert.ThrowsAsync<EqualException>(
            () => AssertQuery(
                async,
                ss => from lc1 in ss.Set<Faction>().OfType<LocustHorde>().Select(lh => lh.Commander)
                      join lc2 in ss.Set<LocustLeader>().OfType<LocustCommander>() on true equals true
                      select (lc1.Name == "Queen Myrrah" ? lc1 : lc2).DefeatedBy));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_as_operator(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<LocustLeader>().Select(ll => ll as LocustCommander));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_datetimeoffset_comparison_in_projection(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Mission>().Select(m => m.Timeline > DateTimeOffset.Now));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OfType_in_subquery_works(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Officer>().SelectMany(o => o.Reports.OfType<Officer>().Select(o1 => o1.AssignedCity)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Nullable_bool_comparison_is_translated_to_server(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<LocustHorde>().Select(lh => new { IsEradicated = lh.Eradicated == true }));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Accessing_reference_navigation_collection_composition_generates_single_query(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>().OrderBy(g => g.Nickname).Select(
                g => new
                {
                    Weapons = g.Weapons.Select(
                        w => new
                        {
                            w.Id,
                            w.IsAutomatic,
                            w.SynergyWith.Name
                        })
                }),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e.Weapons, a.Weapons, elementSorter: ee => ee.Id));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Reference_include_chain_loads_correctly_when_middle_is_null(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<CogTag>().AsTracking().OrderBy(t => t.Note).Include(t => t.Gear).ThenInclude(g => g.Squad),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<CogTag>(t => t.Gear), new ExpectedInclude<Gear>(t => t.Squad, "Gear")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Accessing_property_of_optional_navigation_in_child_projection_works(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<CogTag>().OrderBy(e => e.Note).Select(
                t => new
                {
                    Items = t.Gear != null
                        ? t.Gear.Weapons.Select(w => new { w.Owner.Nickname }).ToList()
                        : null
                }),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e.Items, a.Items, elementSorter: ee => ee.Nickname));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Collection_navigation_ofType_filter_works(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<City>().Where(c => c.BornGears.OfType<Officer>().Any(o => o.Nickname == "Marcus")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Include_after_select_with_cast_throws(bool async)
        => Assert.Equal(
            CoreStrings.IncludeOnNonEntity("h => h.Commander"),
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => AssertQuery(
                    async,
                    ss => ss.Set<Faction>().Where(f => f is LocustHorde).Select(f => (LocustHorde)f).Include(h => h.Commander))))
            .Message);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Include_after_select_with_entity_projection_throws(bool async)
        => Assert.Equal(
            CoreStrings.IncludeOnNonEntity("c => c.BornGears"),
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => AssertQuery(
                    async,
                    ss => ss.Set<Faction>().Select(f => f.Capital).Include(c => c.BornGears)))).Message);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Include_after_select_anonymous_projection_throws(bool async)
        => Assert.Equal(
            CoreStrings.IncludeOnNonEntity("x => x.f.Capital"),
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => AssertQuery(
                    async,
                    ss => ss.Set<Faction>().Select(f => new { f }).Include(x => x.f.Capital)))).Message);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_after_Select_throws(bool async)
        => AssertQuery(async, ss => ss.Set<Faction>().Select(f => f).Include(h => h.Capital));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_after_SelectMany_throws(bool async)
        => AssertQuery(async, ss => ss.Set<Faction>().SelectMany(f => f.Capital.BornGears).Include(g => g.Squad));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Query_reusing_parameter_doesnt_declare_duplicate_parameter(bool async)
    {
        var prm = new ComplexParameter { Inner = new ComplexParameterInner { Nickname = "Marcus" } };

        return AssertQuery(
            async,
            ss => ss.Set<Gear>()
                .Where(g => g.Nickname != prm.Inner.Nickname)
                .Distinct()
                .Where(g => g.Nickname != prm.Inner.Nickname)
                .OrderBy(g => g.FullName),
            assertOrder: true);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Query_reusing_parameter_with_inner_query_doesnt_declare_duplicate_parameter(bool async)
    {
        var squadId = 1;

        return AssertQuery(
            async,
            ss =>
            {
                var innerQuery = ss.Set<Squad>().Where(s => s.Id == squadId);
                var outerQuery = ss.Set<Gear>().Where(g => innerQuery.Contains(g.Squad));
                return outerQuery.Concat(outerQuery).OrderBy(g => g.FullName);
            },
            assertOrder: true);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Query_reusing_parameter_with_inner_query_expression_doesnt_declare_duplicate_parameter(bool async)
    {
        var gearId = 1;
        Expression<Func<Gear, bool>> predicate = s => s.SquadId == gearId;

        return AssertQuery(
            async,
            ss => ss.Set<Squad>().Where(s => s.Members.AsQueryable().Where(predicate).Where(predicate).Any()));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Query_reusing_parameter_doesnt_declare_duplicate_parameter_complex(bool async)
    {
        var prm = new ComplexParameter { Inner = new ComplexParameterInner { Squad = new Squad { Id = 1 } } };

        return AssertQuery(
            async,
            ss => ss.Set<Gear>()
                .Where(g => g.Squad == prm.Inner.Squad)
                .Distinct()
                .Where(g => g.Squad == prm.Inner.Squad)
                .OrderBy(g => g.FullName),
            ss => ss.Set<Gear>()
                .Where(g => g.Squad.Id == prm.Inner.Squad.Id)
                .Distinct()
                .Where(g => g.Squad.Id == prm.Inner.Squad.Id)
                .OrderBy(g => g.FullName),
            assertOrder: true);
    }

    private class ComplexParameter
    {
        public ComplexParameterInner Inner { get; set; }
    }

    private class ComplexParameterInner
    {
        public string Nickname { get; set; }
        public Squad Squad { get; set; }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_entity_and_collection_element(bool async)
        // can't use AssertIncludeQuery here, see #18191
        => AssertQuery(
            async,
            ss => ss.Set<Gear>()
                .Include(g => g.Squad)
                .Include(g => g.Weapons)
                .Select(g => new { gear = g, weapon = g.Weapons.OrderBy(w => w.Id).FirstOrDefault() }),
            elementSorter: e => e.gear.Nickname,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.gear, a.gear);
                AssertEqual(e.gear.Squad, a.gear.Squad);
                AssertCollection(e.gear.Weapons, a.gear.Weapons);
                AssertEqual(e.weapon, a.weapon);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Complex_GroupBy_after_set_operator(bool async)
        => AssertQuery(
            async,
            ss => (from g in ss.Set<Gear>()
                   select new { g.AssignedCity.Name, Count = g.Weapons.Count() }).Concat(
                    from g in ss.Set<Gear>()
                    select new { g.CityOfBirth.Name, Count = g.Weapons.Count() })
                .GroupBy(x => new { x.Name, x.Count })
                .Select(
                    g => new
                    {
                        g.Key.Name,
                        g.Key.Count,
                        Sum = g.Sum(xx => xx.Count)
                    }),
            elementSorter: e => (e.Name, e.Count, e.Sum));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Complex_GroupBy_after_set_operator_using_result_selector(bool async)
        => AssertQuery(
            async,
            ss => (from g in ss.Set<Gear>()
                   select new { g.AssignedCity.Name, Count = g.Weapons.Count() }).Concat(
                    from g in ss.Set<Gear>()
                    select new { g.CityOfBirth.Name, Count = g.Weapons.Count() })
                .GroupBy(
                    x => new { x.Name, x.Count },
                    (k, g) => new
                    {
                        k.Name,
                        k.Count,
                        Sum = g.Sum(xx => xx.Count)
                    }),
            elementSorter: e => (e.Name, e.Count, e.Sum));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Left_join_with_GroupBy_with_composite_group_key(bool async)
        => AssertQuery(
            async,
            ss => from g in ss.Set<Gear>()
                  join s in ss.Set<Squad>() on g.SquadId equals s.Id
                  join t in ss.Set<CogTag>() on g.Nickname equals t.GearNickName into grouping
                  from t in grouping.DefaultIfEmpty()
                  group g by new { g.CityOfBirthName, g.HasSoulPatch }
                  into groupby
                  select new { groupby.Key.CityOfBirthName, groupby.Key.HasSoulPatch },
            elementSorter: e => (e.CityOfBirthName, e.HasSoulPatch));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_with_boolean_grouping_key(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>()
                .Select(
                    g => new
                    {
                        g.Nickname,
                        g.CityOfBirthName,
                        g.HasSoulPatch,
                        IsMarcus = g.Nickname == "Marcus"
                    })
                .GroupBy(
                    g => new
                    {
                        g.CityOfBirthName,
                        g.HasSoulPatch,
                        g.IsMarcus
                    })
                .Select(
                    x => new
                    {
                        x.Key.CityOfBirthName,
                        x.Key.HasSoulPatch,
                        x.Key.IsMarcus,
                        Count = x.Count()
                    }),
            elementSorter: e => (e.CityOfBirthName, e.HasSoulPatch, e.IsMarcus, e.Count));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_with_boolean_groupin_key_thru_navigation_access(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<CogTag>()
                .GroupBy(t => new { HasSoulPatch = (bool?)t.Gear.HasSoulPatch, t.Gear.Squad.Name })
                .Select(g => new { g.Key.HasSoulPatch, Name = g.Key.Name.ToLower() }),
            ss => ss.Set<CogTag>()
                .GroupBy(
                    t => new { HasSoulPatch = t.Gear.MaybeScalar(x => x.HasSoulPatch), t.Gear.Squad.Name })
                .Select(g => new { g.Key.HasSoulPatch, Name = g.Key.Name.Maybe(x => x.ToLower()) }),
            elementSorter: e => (e.HasSoulPatch, e.Name));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Group_by_over_projection_with_multiple_properties_accessed_thru_navigation(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>()
                .Select(
                    g => new
                    {
                        g.Nickname,
                        AssignedCityName = g.AssignedCity.Name,
                        CityOfBirthName = g.CityOfBirth.Name,
                        SquadName = g.Squad.Name
                    })
                .GroupBy(x => x.CityOfBirthName)
                .Select(g => g.Key));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Group_by_with_aggregate_max_on_entity_type(bool async)
        => Assert.ThrowsAsync<InvalidOperationException>(
            () => AssertQuery(
                async,
                ss => ss.Set<Gear>()
                    .GroupBy(g => g.CityOfBirthName)
                    .Select(g => new { g.Key, Aggregate = g.Max() })));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Group_by_on_StartsWith_with_null_parameter_as_argument(bool async)
    {
        var prm = (string)null;

        return AssertQueryScalar(
            async,
            ss => ss.Set<Gear>().GroupBy(g => g.FullName.StartsWith(prm)).Select(g => g.Key),
            ss => ss.Set<Gear>().GroupBy(g => false).Select(g => g.Key));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Group_by_with_having_StartsWith_with_null_parameter_as_argument(bool async)
    {
        var prm = (string)null;

        return AssertQuery(
            async,
            ss => ss.Set<Gear>().GroupBy(g => g.FullName).Where(g => g.Key.StartsWith(prm)).Select(g => g.Key),
            ss => ss.Set<Gear>().GroupBy(g => g.FullName).Where(g => false).Select(g => g.Key),
            assertEmpty: true);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_StartsWith_with_null_parameter_as_argument(bool async)
    {
        var prm = (string)null;

        return AssertQueryScalar(
            async,
            ss => ss.Set<Gear>().Select(g => g.FullName.StartsWith(prm)),
            ss => ss.Set<Gear>().Select(g => false));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_null_parameter_is_not_null(bool async)
    {
        var prm = (string)null;

        return AssertQueryScalar(
            async,
            ss => ss.Set<Gear>().Select(g => prm != null),
            ss => ss.Set<Gear>().Select(g => false));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_null_parameter_is_not_null(bool async)
    {
        var prm = (string)null;

        return AssertQuery(
            async,
            ss => ss.Set<Gear>().Where(g => prm != null),
            ss => ss.Set<Gear>().Where(g => false),
            assertEmpty: true);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OrderBy_StartsWith_with_null_parameter_as_argument(bool async)
    {
        var prm = (string)null;

        return AssertQuery(
            async,
            ss => ss.Set<Gear>().OrderBy(g => g.FullName.StartsWith(prm)).ThenBy(g => g.Nickname),
            ss => ss.Set<Gear>().OrderBy(g => false).ThenBy(g => g.Nickname),
            assertOrder: true);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OrderBy_Contains_empty_list(bool async)
    {
        var ids = new List<int>();

        return AssertQuery(
            async,
            ss => ss.Set<Gear>().OrderBy(g => ids.Contains(g.SquadId)).Select(g => g),
            ss => ss.Set<Gear>().OrderBy(g => ids.Contains(g.SquadId)).Select(g => g));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Where_with_enum_flags_parameter(bool async)
    {
        MilitaryRank? rank = MilitaryRank.Private;
        await AssertQuery(
            async,
            ss => ss.Set<Gear>().Where(g => (g.Rank & rank) == rank));

        rank = null;
        await AssertQuery(
            async,
            ss => ss.Set<Gear>().Where(g => (g.Rank & rank) == rank));

        rank = MilitaryRank.Corporal;
        await AssertQuery(
            async,
            ss => ss.Set<Gear>().Where(g => (g.Rank | rank) != rank));

        rank = null;
        await AssertQuery(
            async,
            ss => ss.Set<Gear>().Where(g => (g.Rank | rank) != rank),
            assertEmpty: true);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task FirstOrDefault_navigation_access_entity_equality_in_where_predicate_apply_peneding_selector(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>()
                .Where(g => g.AssignedCity == ss.Set<Gear>().OrderBy(s => s.Nickname).FirstOrDefault().CityOfBirth));

    //=> AssertQuery(
    //    async,
    //    ss => ss.Set<Faction>()
    //        .Where(f => f.Capital == ss.Set<Gear>().OrderBy(s => s.Nickname).FirstOrDefault().CityOfBirth));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Conditional_expression_with_test_being_simplified_to_constant_simple(bool isAsync)
    {
        var prm = true;
        var prm2 = (string)null;

        return AssertQuery(
            isAsync,
            ss => ss.Set<Gear>().Where(
                g => g.HasSoulPatch == prm
                    ? true
                    : g.CityOfBirthName == prm2));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Conditional_expression_with_test_being_simplified_to_constant_complex(bool isAsync)
    {
        var prm = true;
        var prm2 = "Marcus' Lancer";
        var prm3 = (string)null;

        return AssertQuery(
            isAsync,
            ss => ss.Set<Gear>().Where(
                g => g.HasSoulPatch == prm
                    ? ss.Set<Weapon>().Where(w => w.Id == g.SquadId).Single().Name == prm2
                    : g.CityOfBirthName == prm3));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Bitwise_operation_with_non_null_parameter_optimizes_null_checks(bool async)
    {
        var ranks = MilitaryRank.Corporal | MilitaryRank.Sergeant | MilitaryRank.General;

        await AssertQuery(
            async,
            ss => ss.Set<Gear>().Where(g => (g.Rank & ranks) != 0));

        await AssertQueryScalar(
            async,
            ss => ss.Set<Gear>().Select(g => (g.Rank | ranks) == ranks));

        await AssertQueryScalar(
            async,
            ss => ss.Set<Gear>().Select(g => (g.Rank | (g.Rank | (ranks | (g.Rank | ranks)))) == ranks));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Bitwise_operation_with_null_arguments(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Weapon>().Where(w => (w.AmmunitionType & AmmunitionType.Cartridge) == null));

        await AssertQuery(
            async,
            ss => ss.Set<Weapon>().Where(w => (w.AmmunitionType | AmmunitionType.Shell) == null));

        AmmunitionType? prm = null;
        await AssertQuery(
            async,
            ss => ss.Set<Weapon>().Where(w => (w.AmmunitionType | AmmunitionType.Shell) == prm));

        await AssertQuery(
            async,
            ss => ss.Set<Weapon>().Where(w => (w.AmmunitionType & prm) == prm));

        prm = AmmunitionType.Shell;
        await AssertQuery(
            async,
            ss => ss.Set<Weapon>().Where(w => (w.AmmunitionType & prm) != 0));

        prm = AmmunitionType.Cartridge;
        await AssertQuery(
            async,
            ss => ss.Set<Weapon>().Where(w => (w.AmmunitionType & prm) == prm));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Logical_operation_with_non_null_parameter_optimizes_null_checks(bool async)
    {
        var prm = true;
        await AssertQuery(
            async,
            ss => ss.Set<Gear>().Where(g => (g.HasSoulPatch && prm) != prm));

        prm = false;
        await AssertQuery(
            async,
            ss => ss.Set<Gear>().Where(g => (g.HasSoulPatch || prm) != prm));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Cast_OfType_works_correctly(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>().Cast<Gear>().OfType<Officer>().Select(o => o.FullName));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Join_inner_source_custom_projection_followed_by_filter(bool async)
        => AssertQuery(
            async,
            ss => from ll in ss.Set<LocustLeader>()
                  join h in ss.Set<Faction>().OfType<LocustHorde>()
                          .Select(
                              f => new
                              {
                                  IsEradicated = f.Name == "Locust" ? (bool?)true : null,
                                  f.CommanderName,
                                  f.Name
                              })
                      on ll.Name equals h.CommanderName
                  where h.IsEradicated != true
                  select h);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Byte_array_contains_literal(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Squad>().Where(s => s.Banner.Contains((byte)1)),
            ss => ss.Set<Squad>().Where(s => s.Banner != null && s.Banner.Contains((byte)1)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Byte_array_contains_parameter(bool async)
    {
        var someByte = (byte)1;
        return AssertQuery(
            async,
            ss => ss.Set<Squad>().Where(s => s.Banner.Contains(someByte)),
            ss => ss.Set<Squad>().Where(s => s.Banner != null && s.Banner.Contains(someByte)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Byte_array_filter_by_length_literal_does_not_cast_on_varbinary_n(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Squad>().Where(w => w.Banner5.Length == 5),
            ss => ss.Set<Squad>().Where(w => w.Banner5 != null && w.Banner5.Length == 5));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Byte_array_filter_by_length_literal(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Squad>().Where(w => w.Banner.Length == 2),
            ss => ss.Set<Squad>().Where(w => w.Banner != null && w.Banner.Length == 2));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Byte_array_filter_by_length_parameter(bool async)
    {
        var someByteArr = new[] { (byte)42, (byte)24};
        return AssertQuery(
            async,
            ss => ss.Set<Squad>().Where(w => w.Banner.Length == someByteArr.Length),
            ss => ss.Set<Squad>().Where(w => w.Banner != null && w.Banner.Length == someByteArr.Length));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OrderBy_bool_coming_from_optional_navigation(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Weapon>().Select(w => w.SynergyWith).OrderBy(w => w.IsAutomatic),
            ss => ss.Set<Weapon>().Select(w => w.SynergyWith).OrderBy(w => w.MaybeScalar(x => x.IsAutomatic)),
            assertOrder: true);

    [ConditionalFact]
    public virtual void Byte_array_filter_by_length_parameter_compiled()
    {
        var query = EF.CompileQuery(
            (GearsOfWarContext context, byte[] byteArrayParam)
                => context.Squads.Where(w => w.Banner.Length == byteArrayParam.Length).Count());

        using var context = CreateContext();
        var byteQueryParam = new[] { (byte)42, (byte)128 };

        Assert.Equal(2, query(context, byteQueryParam));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task DateTimeOffset_Date_returns_datetime(bool async)
    {
        var dateTimeOffset = new DateTimeOffset(2, 3, 1, 8, 0, 0, new TimeSpan(-5, 0, 0));

        return AssertQuery(
            async,
            ss => ss.Set<Mission>().Where(m => m.Timeline.Date >= dateTimeOffset.Date));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Conditional_with_conditions_evaluating_to_false_gets_optimized(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>().Select(g => g.Nickname == null && g.Nickname != null ? g.CityOfBirthName : g.FullName));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Conditional_with_conditions_evaluating_to_true_gets_optimized(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>().Select(g => g.Nickname == null || g.Nickname != null ? g.CityOfBirthName : g.FullName));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Projecting_required_string_column_compared_to_null_parameter(bool async)
    {
        var nullParameter = default(string);

        return AssertQueryScalar(
            async,
            ss => ss.Set<Gear>().Select(g => g.Nickname == nullParameter));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Byte_array_filter_by_SequenceEqual(bool async)
    {
        var byteArrayParam = new byte[] { 0x04, 0x05, 0x06, 0x07, 0x08 };

        return AssertQuery(
            async,
            ss => ss.Set<Squad>().Where(s => s.Banner5.SequenceEqual(byteArrayParam)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Group_by_nullable_property_HasValue_and_project_the_grouping_key(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Weapon>()
                .GroupBy(w => w.SynergyWithId.HasValue)
                .Select(g => g.Key));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Group_by_nullable_property_and_project_the_grouping_key_HasValue(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Weapon>()
                .GroupBy(w => w.SynergyWithId)
                .Select(g => g.Key.HasValue));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Checked_context_with_cast_does_not_fail(bool isAsync)
    {
        checked
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<LocustLeader>().Where(w => (byte)w.ThreatLevel >= (short?)5));
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Checked_context_with_addition_does_not_fail(bool isAsync)
    {
        checked
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<LocustLeader>().Where(w => w.ThreatLevel <= (5 + (long?)w.ThreatLevel)));
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Checked_context_throws_on_client_evaluation(bool isAsync)
    {
        checked
        {
            return Assert.ThrowsAsync<InvalidOperationException>(
                () => AssertQueryScalar(
                    isAsync,
                    ss => ss.Set<LocustLeader>().Select(w => w.ThreatLevel >= (byte)GetThreatLevel()))
            );
        }
    }

    private int GetThreatLevel()
        => 256;

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task TimeSpan_Hours(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Mission>()
                .Select(m => m.Duration.Hours));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task TimeSpan_Minutes(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Mission>()
                .Select(m => m.Duration.Minutes));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task TimeSpan_Seconds(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Mission>()
                .Select(m => m.Duration.Seconds));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task TimeSpan_Milliseconds(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Mission>()
                .Select(m => m.Duration.Milliseconds));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_TimeSpan_Hours(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Mission>()
                .Where(m => m.Duration.Hours == 1));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_TimeSpan_Minutes(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Mission>()
                .Where(m => m.Duration.Minutes == 2));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_TimeSpan_Seconds(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Mission>()
                .Where(m => m.Duration.Seconds == 3));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_TimeSpan_Milliseconds(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Mission>()
                .Where(m => m.Duration.Milliseconds == 456));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Contains_on_collection_of_byte_subquery(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<LocustLeader>()
                .Where(l => ss.Set<LocustLeader>().Select(ll => ll.ThreatLevelByte).Contains(l.ThreatLevelByte)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Contains_on_collection_of_nullable_byte_subquery(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<LocustLeader>().Where(
                l => ss.Set<LocustLeader>().Select(ll => ll.ThreatLevelNullableByte).Contains(l.ThreatLevelNullableByte)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Contains_on_collection_of_nullable_byte_subquery_null_constant(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<LocustLeader>().Where(l => ss.Set<LocustLeader>().Select(ll => ll.ThreatLevelNullableByte).Contains(null)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Contains_on_collection_of_nullable_byte_subquery_null_parameter(bool async)
    {
        var prm = default(byte?);

        return AssertQuery(
            async,
            ss => ss.Set<LocustLeader>().Where(l => ss.Set<LocustLeader>().Select(ll => ll.ThreatLevelNullableByte).Contains(prm)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Contains_on_byte_array_property_using_byte_column(bool async)
        => AssertQuery(
            async,
            ss => from s in ss.Set<Squad>()
                  from l in ss.Set<LocustLeader>()
                  where s.Banner.Contains(l.ThreatLevelByte)
                  select new { s, l },
            elementSorter: e => (e.s.Id, e.l.Name),
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.s, a.s);
                AssertEqual(e.l, a.l);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Subquery_projecting_non_nullable_scalar_contains_non_nullable_value_doesnt_need_null_expansion(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<LocustLeader>()
                .SelectMany(
                    l => ss.Set<Gear>()
                        .Where(g => ss.Set<LocustLeader>().Select(x => x.ThreatLevelByte).Contains(l.ThreatLevelByte))));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Subquery_projecting_non_nullable_scalar_contains_non_nullable_value_doesnt_need_null_expansion_negated(
        bool async)
        => AssertQuery(
            async,
            ss => ss.Set<LocustLeader>()
                .SelectMany(
                    l => ss.Set<Gear>()
                        .Where(g => !ss.Set<LocustLeader>().Select(x => x.ThreatLevelByte).Contains(l.ThreatLevelByte))),
            assertEmpty: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Subquery_projecting_nullable_scalar_contains_nullable_value_needs_null_expansion(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<LocustLeader>()
                .SelectMany(
                    l => ss.Set<Gear>()
                        .Where(
                            g => ss.Set<LocustLeader>().Select(x => x.ThreatLevelNullableByte).Contains(l.ThreatLevelNullableByte))));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Subquery_projecting_nullable_scalar_contains_nullable_value_needs_null_expansion_negated(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<LocustLeader>()
                .SelectMany(
                    l => ss.Set<Gear>()
                        .Where(
                            g => !ss.Set<LocustLeader>().Select(x => x.ThreatLevelNullableByte).Contains(l.ThreatLevelNullableByte))),
            assertEmpty: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Enum_closure_typed_as_underlying_type_generates_correct_parameter_type(bool async)
    {
        var prm = (int)AmmunitionType.Cartridge;

        return AssertQuery(
            async,
            ss => ss.Set<Weapon>().Where(w => prm == (int?)w.AmmunitionType),
            ss => ss.Set<Weapon>().Where(w => w.AmmunitionType != null && prm == (int)w.AmmunitionType));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Enum_flags_closure_typed_as_underlying_type_generates_correct_parameter_type(bool async)
    {
        var prm = (int)MilitaryRank.Private + (int)MilitaryRank.Sergeant + (int)MilitaryRank.General;

        return AssertQuery(
            async,
            ss => ss.Set<Gear>()
                .Where(g => (prm & (int)g.Rank) == (int)g.Rank));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Enum_flags_closure_typed_as_different_type_generates_correct_parameter_type(bool async)
    {
        var prm = (byte)MilitaryRank.Private + (byte)MilitaryRank.Sergeant;

        return AssertQuery(
            async,
            ss => ss.Set<Gear>()
                .Where(g => (prm & (short)g.Rank) == (short)g.Rank));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Constant_enum_with_same_underlying_value_as_previously_parameterized_int(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Gear>()
                .OrderBy(g => g.Nickname)
                .Take(1)
                .Select(g => g.Rank & MilitaryRank.Private));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Enum_array_contains(bool async)
    {
        var types = new[] { (AmmunitionType?)null, AmmunitionType.Cartridge };

        return AssertQuery(
            async,
            ss => ss.Set<Weapon>()
                .Where(w => w.SynergyWith != null && types.Contains(w.SynergyWith.AmmunitionType)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Client_eval_followed_by_aggregate_operation(bool async)
    {
        await AssertSum(
            async,
            ss => ss.Set<Mission>().Select(m => m.Duration.Ticks));

        await AssertAverage(
            async,
            ss => ss.Set<Mission>().Select(m => m.Duration.Ticks));

        await AssertMin(
            async,
            ss => ss.Set<Mission>().Select(m => m.Duration.Ticks));

        await AssertMax(
            async,
            ss => ss.Set<Mission>().Select(m => m.Duration.Ticks));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Trying_to_access_unmapped_property_throws_informative_error(bool async)
        => AssertTranslationFailedWithDetails(
            () => AssertQuery(
                async,
                ss => ss.Set<Gear>().Where(g => g.IsMarcus)),
            CoreStrings.QueryUnableToTranslateMember(nameof(Gear.IsMarcus), nameof(Gear)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Trying_to_access_unmapped_property_in_projection(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Gear>().Select(g => g.IsMarcus));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Trying_to_access_unmapped_property_inside_aggregate(bool async)
        => AssertTranslationFailedWithDetails(
            () => AssertQuery(
                async,
                ss => ss.Set<City>().Where(c => c.BornGears.Count(g => g.IsMarcus) > 0)),
            CoreStrings.QueryUnableToTranslateMember(nameof(Gear.IsMarcus), nameof(Gear)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Trying_to_access_unmapped_property_inside_subquery(bool async)
        => AssertTranslationFailedWithDetails(
            () => AssertQuery(
                async,
                ss => ss.Set<City>().Where(
                    c => ss.Set<Gear>().Where(g => g.IsMarcus).Select(g => g.Nickname).FirstOrDefault() == "Marcus")),
            CoreStrings.QueryUnableToTranslateMember(nameof(Gear.IsMarcus), nameof(Gear)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Trying_to_access_unmapped_property_inside_join_key_selector(bool async)
        => AssertTranslationFailedWithDetails(
            () => AssertQuery(
                async,
                ss => from w in ss.Set<Weapon>()
                      join g in ss.Set<Gear>() on w.IsAutomatic equals g.IsMarcus into grouping
                      from g in grouping.DefaultIfEmpty()
                      select new { w, g }),
            CoreStrings.QueryUnableToTranslateMember(nameof(Gear.IsMarcus), nameof(Gear)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Client_projection_with_nested_unmapped_property_bubbles_up_translation_failure_info(bool async)
        => AssertTranslationFailedWithDetails(
            () => AssertQuery(
                async,
                ss => ss.Set<Gear>().Select(g => new { nested = ss.Set<Gear>().Where(gg => gg.IsMarcus).ToList() })),
            CoreStrings.QueryUnableToTranslateMember(nameof(Gear.IsMarcus), nameof(Gear)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Client_member_and_unsupported_string_Equals_in_the_same_query(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>().Where(g => g.FullName.Equals(g.Nickname, StringComparison.InvariantCulture) || g.IsMarcus));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task CompareTo_used_with_non_unicode_string_column_and_constant(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<City>().Where(c => c.Location.CompareTo("Unknown") == 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Coalesce_used_with_non_unicode_string_column_and_constant(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<City>().Select(c => c.Location ?? "Unknown"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Groupby_anonymous_type_with_navigations_followed_up_by_anonymous_projection_and_orderby(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Weapon>()
                .GroupBy(w => new { w.Owner.CityOfBirth.Name, w.Owner.CityOfBirth.Location })
                .Select(
                    x => new
                    {
                        x.Key.Name,
                        x.Key.Location,
                        Count = x.Count()
                    })
                .OrderBy(x => x.Location),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_predicate_with_non_equality_comparison_converted_to_inner_join(bool async)
        => AssertQuery(
            async,
            ss => from g in ss.Set<Gear>()
                  from w in ss.Set<Weapon>().Where(x => x.OwnerFullName != g.FullName)
                  orderby g.Nickname, w.Id
                  select new { g, w },
            assertOrder: true,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.g, a.g);
                AssertEqual(e.w, a.w);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_predicate_with_non_equality_comparison_DefaultIfEmpty_converted_to_left_join(bool async)
        => AssertQuery(
            async,
            ss => from g in ss.Set<Gear>()
                  from w in ss.Set<Weapon>().Where(x => x.OwnerFullName != g.FullName).DefaultIfEmpty()
                  orderby g.Nickname, w.Id
                  select new { g, w },
            assertOrder: true,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.g, a.g);
                AssertEqual(e.w, a.w);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_predicate_after_navigation_with_non_equality_comparison_DefaultIfEmpty_converted_to_left_join(
        bool async)
        => AssertQuery(
            async,
            ss => from g in ss.Set<Gear>()
                  from w in ss.Set<Weapon>().Select(x => x.SynergyWith).Where(x => x.OwnerFullName != g.FullName).DefaultIfEmpty()
                  orderby g.Nickname, w.Id
                  select new { g, w },
            ss => from g in ss.Set<Gear>()
                  from w in ss.Set<Weapon>().Select(x => x.SynergyWith).Where(x => x.OwnerFullName != g.FullName).MaybeDefaultIfEmpty()
                  orderby g.Nickname, w.MaybeScalar(xx => xx.Id)
                  select new { g, w },
            assertOrder: true,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.g, a.g);
                AssertEqual(e.w, a.w);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_without_result_selector_and_non_equality_comparison_converted_to_join(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>().SelectMany(g => ss.Set<Weapon>().Where(x => x.OwnerFullName != g.FullName).DefaultIfEmpty()));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filtered_collection_projection_with_order_comparison_predicate_converted_to_join(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>().OrderBy(g => g.Nickname).Select(g => g.Weapons.Where(x => x.Id > g.SquadId).ToList()),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a, elementSorter: ee => ee.Id));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filtered_collection_projection_with_order_comparison_predicate_converted_to_join2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>().OrderBy(g => g.Nickname).Select(g => g.Weapons.Where(x => x.Id >= g.SquadId).ToList()),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a, elementSorter: ee => ee.Id));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filtered_collection_projection_with_order_comparison_predicate_converted_to_join3(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>().OrderBy(g => g.Nickname).Select(g => g.Weapons.Where(x => x.Id <= g.SquadId).ToList()),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a, elementSorter: ee => ee.Id));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_predicate_with_non_equality_comparison_with_Take_doesnt_convert_to_join(bool async)
        => AssertQuery(
            async,
            ss => from g in ss.Set<Gear>()
                  from w in ss.Set<Weapon>().Where(x => x.OwnerFullName != g.FullName).OrderBy(x => x.Id).Take(3)
                  orderby g.Nickname, w.Id
                  select new { g, w },
            assertOrder: true,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.g, a.g);
                AssertEqual(e.w, a.w);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task FirstOrDefault_over_int_compared_to_zero(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Squad>().Where(s => s.Name == "Delta")
                .Where(s => s.Members
                    .Where(m => m.HasSoulPatch)
                    .OrderBy(m => m.FullName)
                    .Select(m => m.SquadId)
                    .FirstOrDefault() != 0)
                .Select(s => s.Name),
            elementSorter: e => e);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Correlated_collection_with_inner_collection_references_element_two_levels_up(bool async)
        => AssertQuery(
            async,
            ss => from o in ss.Set<Gear>().OfType<Officer>()
                  select new
                  {
                      o.FullName,
                      Collection = (from r in o.Reports
                                    select new { ReportName = r.FullName, OfficerName = o.FullName }).ToList()
                  },
            elementSorter: e => e.FullName,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.FullName, a.FullName);
                AssertCollection(e.Collection, a.Collection, elementSorter: ee => (ee.OfficerName, ee.ReportName));
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Accessing_derived_property_using_hard_and_soft_cast(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<LocustLeader>().Where(ll => ll is LocustCommander && ((LocustCommander)ll).HighCommandId != 0));

        await AssertQuery(
            async,
            ss => ss.Set<LocustLeader>().Where(ll => ll is LocustCommander && (ll as LocustCommander).HighCommandId != 0));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Cast_to_derived_followed_by_include_and_FirstOrDefault(bool async)
        => AssertFirstOrDefault(
            async,
            ss => ss.Set<LocustLeader>().Where(ll => ll.Name.Contains("Queen")).Cast<LocustCommander>().Include(lc => lc.DefeatedBy),
            asserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<LocustCommander>(x => x.DefeatedBy)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Cast_to_derived_followed_by_multiple_includes(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<LocustLeader>().Where(ll => ll.Name.Contains("Queen")).Cast<LocustCommander>().Include(lc => lc.DefeatedBy)
                .ThenInclude(g => g.Weapons),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<LocustCommander>(x => x.DefeatedBy),
                new ExpectedInclude<Gear>(x => x.Weapons, "DefeatedBy")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Correlated_collection_take(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>().Select(
                g => new
                {
                    g.Nickname,
                    Weapons = g.Weapons.Take(10).ToList(),
                    g.CityOfBirth
                }),
            elementSorter: e => e.Nickname,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.Nickname, a.Nickname);
                AssertCollection(e.Weapons, a.Weapons, elementSorter: ee => ee.Id);
                AssertEqual(e.CityOfBirth, a.CityOfBirth);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_Select_sum(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Mission>().GroupBy(m => m.CodeName).Select(g => g.Sum(m => m.Rating)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Sum_with_no_data_nullable_double(bool async)
        => AssertSum(
            async,
            ss => ss.Set<Mission>().Where(m => m.CodeName == "Operation Foobar").Select(m => m.Rating));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task FirstOrDefault_on_empty_collection_of_DateTime_in_subquery(bool async)
        => AssertQuery(
            async,
            ss => from g in ss.Set<Gear>()
                  let invalidTagIssueDate = (from t in ss.Set<CogTag>()
                                             where t.GearNickName == g.FullName
                                             orderby t.Id
                                             select t.IssueDate).FirstOrDefault()
                  where g.Tag.IssueDate > invalidTagIssueDate
                  select new { g.Nickname, invalidTagIssueDate });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task First_on_byte_array(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Squad>().Where(e => e.Banner.First() == 0x02));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Array_access_on_byte_array(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Squad>().Where(e => e.Banner5[2] == 0x06));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_shadow_properties(bool async)
        => AssertQuery(
            async,
            ss => from g in ss.Set<Gear>()
                  select new { g.Nickname, AssignedCityName = EF.Property<string>(g, "AssignedCityName") },
            elementSorter: e => e.Nickname);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Composite_key_entity_equal(bool async)
        => AssertQuery(
            async,
            ss => from g1 in ss.Set<Gear>()
                  from g2 in ss.Set<Gear>()
                  where g1 == g2
                  select new { g1, g2 },
            elementSorter: e => (e.g1.Nickname, e.g2.Nickname),
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.g1, a.g1);
                AssertEqual(e.g2, a.g2);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Composite_key_entity_not_equal(bool async)
        => AssertQuery(
            async,
            ss => from g1 in ss.Set<Gear>()
                  from g2 in ss.Set<Gear>()
                  where g1 != g2
                  select new { g1, g2 },
            elementSorter: e => (e.g1.Nickname, e.g2.Nickname),
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.g1, a.g1);
                AssertEqual(e.g2, a.g2);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Composite_key_entity_equal_null(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<LocustLeader>().OfType<LocustCommander>().Where(lc => lc.DefeatedBy == null));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Composite_key_entity_not_equal_null(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<LocustLeader>().OfType<LocustCommander>().Where(lc => lc.DefeatedBy != null));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Projecting_property_converted_to_nullable_with_comparison(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<CogTag>().Select(
                x => new
                {
                    x.Note,
                    Nullable = x.GearNickName != null
                        ? new
                        {
                            x.Gear.Nickname,
                            x.Gear.SquadId,
                            x.Gear.HasSoulPatch
                        }
                        : null
                }).Where(x => x.Nullable.SquadId == 1));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Projecting_property_converted_to_nullable_with_addition(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<CogTag>().Select(
                x => new
                {
                    x.Note,
                    Nullable = x.GearNickName != null
                        ? new
                        {
                            x.Gear.Nickname,
                            x.Gear.SquadId,
                            x.Gear.HasSoulPatch
                        }
                        : null
                }).Where(x => x.Nullable.SquadId + 1 == 2),
            ss => ss.Set<CogTag>().Select(
                x => new
                {
                    x.Note,
                    Nullable = x.GearNickName != null
                        ? new
                        {
                            x.Gear.Nickname,
                            x.Gear.SquadId,
                            x.Gear.HasSoulPatch
                        }
                        : null
                }).Where(x => x.Nullable != null && x.Nullable.SquadId + 1 == 2));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Projecting_property_converted_to_nullable_with_addition_and_final_projection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<CogTag>().Select(
                    x => new
                    {
                        x.Note,
                        Nullable = x.GearNickName != null
                            ? new
                            {
                                x.Gear.Nickname,
                                x.Gear.SquadId,
                                x.Gear.HasSoulPatch
                            }
                            : null
                    })
                .Where(x => x.Nullable.Nickname != null)
                .Select(x => new { x.Note, Value = x.Nullable.SquadId + 1 }));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Projecting_property_converted_to_nullable_with_conditional(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<CogTag>().Select(
                x => new
                {
                    x.Note,
                    Nullable = x.GearNickName != null
                        ? new
                        {
                            x.Gear.Nickname,
                            x.Gear.SquadId,
                            x.Gear.HasSoulPatch
                        }
                        : null
                }).Select(x => x.Note != "K.I.A." ? x.Nullable.SquadId : -1));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Projecting_property_converted_to_nullable_with_function_call(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<CogTag>().Select(
                x => new
                {
                    x.Note,
                    Nullable = x.GearNickName != null
                        ? new
                        {
                            x.Gear.Nickname,
                            x.Gear.SquadId,
                            x.Gear.HasSoulPatch
                        }
                        : null
                }).Select(x => x.Nullable.Nickname.Substring(0, 3)),
            ss => ss.Set<CogTag>().Select(x => x.GearNickName == null ? null : x.GearNickName.Substring(0, 3)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Projecting_property_converted_to_nullable_with_function_call2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<CogTag>().Select(
                    x => new
                    {
                        x.Note,
                        Nullable = x.GearNickName != null
                            ? new
                            {
                                x.Gear.Nickname,
                                x.Gear.SquadId,
                                x.Gear.HasSoulPatch
                            }
                            : null
                    })
                .Where(x => x.Nullable.Nickname != null)
                .Select(x => new { x.Note, Function = x.Note.Substring(0, x.Nullable.SquadId) }),
            ss => ss.Set<CogTag>().Select(
                    x => new
                    {
                        x.Note,
                        Nullable = x.GearNickName != null
                            ? new
                            {
                                x.Gear.Nickname,
                                x.Gear.SquadId,
                                x.Gear.HasSoulPatch
                            }
                            : null
                    })
                .Where(x => x.Nullable.Nickname != null)
                .Select(x => new { x.Note, Function = x.Nullable == null ? null : x.Note.Substring(0, x.Nullable.SquadId) }));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Projecting_property_converted_to_nullable_into_element_init(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<CogTag>().Select(
                    x => new
                    {
                        x.Note,
                        Nullable = x.GearNickName != null
                            ? new
                            {
                                x.Gear.Nickname,
                                x.Gear.SquadId,
                                x.Gear.HasSoulPatch
                            }
                            : null
                    })
                .Where(x => x.Nullable.Nickname != null)
                .OrderBy(x => x.Note)
                .Select(
                    x => new List<int>
                    {
                        x.Nullable.Nickname.Length,
                        x.Nullable.SquadId,
                        x.Nullable.SquadId + 1,
                        42
                    }),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Projecting_property_converted_to_nullable_into_member_assignment(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<CogTag>().Select(
                    x => new
                    {
                        x.Note,
                        Nullable = x.GearNickName != null
                            ? new
                            {
                                x.Gear.Nickname,
                                x.Gear.SquadId,
                                x.Gear.HasSoulPatch
                            }
                            : null
                    })
                .Where(x => x.Nullable.Nickname != null)
                .OrderBy(x => x.Note)
                .Select(x => new Squad { Id = x.Nullable.SquadId }),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Projecting_property_converted_to_nullable_into_new_array(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<CogTag>().Select(
                    x => new
                    {
                        x.Note,
                        Nullable = x.GearNickName != null
                            ? new
                            {
                                x.Gear.Nickname,
                                x.Gear.SquadId,
                                x.Gear.HasSoulPatch
                            }
                            : null
                    })
                .Where(x => x.Nullable.Nickname != null)
                .OrderBy(x => x.Note)
                .Select(x => new[] { x.Nullable.Nickname.Length, x.Nullable.SquadId, x.Nullable.SquadId + 1, 42 }),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Projecting_property_converted_to_nullable_into_unary(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<CogTag>().Select(
                    x => new
                    {
                        x.Note,
                        Nullable = x.GearNickName != null
                            ? new
                            {
                                x.Gear.Nickname,
                                x.Gear.SquadId,
                                x.Gear.HasSoulPatch
                            }
                            : null
                    })
                .Where(x => x.Nullable.Nickname != null)
                .OrderBy(x => x.Note)
                .Where(x => !x.Nullable.HasSoulPatch)
                .Select(x => x.Note),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Projecting_property_converted_to_nullable_into_member_access(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>().Select(
                    x => new
                    {
                        x.Nickname,
                        x.CityOfBirthName,
                        Nullable = x.CityOfBirthName != null ? new { x.Tag.IssueDate } : null
                    })
                .Where(x => x.CityOfBirthName != null)
                .OrderBy(x => x.Nickname)
                .Where(x => x.Nullable.IssueDate.Month != 5)
                .Select(x => x.Nickname),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Projecting_property_converted_to_nullable_and_use_it_in_order_by(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<CogTag>().Select(
                    x => new
                    {
                        x.Note,
                        Nullable = x.GearNickName != null
                            ? new
                            {
                                x.Gear.Nickname,
                                x.Gear.SquadId,
                                x.Gear.HasSoulPatch
                            }
                            : null
                    })
                .Where(x => x.Nullable.Nickname != null)
                .OrderBy(x => x.Nullable.SquadId).ThenBy(x => x.Note),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Correlated_collection_with_distinct_projecting_identifier_column(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>()
                .Select(
                    g => new
                    {
                        Key = g.Nickname,
                        Subquery = g.Weapons
                            .Select(w => new { w.Id, w.Name })
                            .Distinct().ToList()
                    }),
            elementSorter: e => e.Key,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Key, a.Key);
                AssertCollection(
                    e.Subquery,
                    a.Subquery,
                    elementSorter: ee => ee.Id,
                    elementAsserter: (ee, aa) =>
                    {
                        Assert.Equal(ee.Id, aa.Id);
                        Assert.Equal(ee.Name, aa.Name);
                    });
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Correlated_collection_with_distinct_projecting_identifier_column_and_correlation_key(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>()
                .Select(
                    g => new
                    {
                        Key = g.Nickname,
                        Subquery = g.Weapons
                            .Select(
                                w => new
                                {
                                    w.Id,
                                    w.Name,
                                    w.OwnerFullName
                                })
                            .Distinct().ToList()
                    }),
            elementSorter: e => e.Key,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Key, a.Key);
                AssertCollection(
                    e.Subquery,
                    a.Subquery,
                    elementSorter: ee => ee.Id,
                    elementAsserter: (ee, aa) =>
                    {
                        Assert.Equal(ee.Id, aa.Id);
                        Assert.Equal(ee.Name, aa.Name);
                        Assert.Equal(ee.OwnerFullName, aa.OwnerFullName);
                    });
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Correlated_collection_with_distinct_projecting_identifier_column_composite_key(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Squad>()
                .Select(
                    s => new
                    {
                        Key = s.Id,
                        Subquery = s.Members
                            .Select(
                                m => new
                                {
                                    m.Nickname,
                                    m.SquadId,
                                    m.HasSoulPatch
                                })
                            .Distinct().ToList()
                    }),
            elementSorter: e => e.Key,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Key, a.Key);
                AssertCollection(
                    e.Subquery,
                    a.Subquery,
                    elementSorter: ee => ee.Nickname,
                    elementAsserter: (ee, aa) =>
                    {
                        Assert.Equal(ee.Nickname, aa.Nickname);
                        Assert.Equal(ee.SquadId, aa.SquadId);
                        Assert.Equal(ee.HasSoulPatch, aa.HasSoulPatch);
                    });
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Correlated_collection_with_distinct_not_projecting_identifier_column(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>()
                .Select(
                    g => new
                    {
                        Key = g.Nickname,
                        Subquery = g.Weapons
                            .Select(w => new { w.Name, w.IsAutomatic })
                            .Distinct().ToList()
                    }),
            elementSorter: e => e.Key,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Key, a.Key);
                AssertCollection(
                    e.Subquery,
                    a.Subquery,
                    elementSorter: ee => ee.Name,
                    elementAsserter: (ee, aa) =>
                    {
                        Assert.Equal(ee.Name, aa.Name);
                        Assert.Equal(ee.IsAutomatic, aa.IsAutomatic);
                    });
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Correlated_collection_with_distinct_not_projecting_identifier_column_also_projecting_complex_expressions(
        bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>()
                .Select(
                    g => new
                    {
                        Key = g.Nickname,
                        Subquery = g.Weapons
                            .Select(
                                w => new
                                {
                                    w.Name,
                                    w.IsAutomatic,
                                    w.OwnerFullName.Length
                                })
                            .Distinct().ToList()
                    }),
            elementSorter: e => e.Key,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Key, a.Key);
                AssertCollection(
                    e.Subquery,
                    a.Subquery,
                    elementSorter: ee => ee.Name,
                    elementAsserter: (ee, aa) =>
                    {
                        Assert.Equal(ee.Name, aa.Name);
                        Assert.Equal(ee.IsAutomatic, aa.IsAutomatic);
                        Assert.Equal(ee.Length, aa.Length);
                    });
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Correlated_collection_with_groupby_not_projecting_identifier_column_but_only_grouping_key_in_final_projection(
        bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>()
                .Select(
                    g => new
                    {
                        Key = g.Nickname,
                        Subquery = g.Weapons
                            .Select(w => new { w.Name, w.IsAutomatic })
                            .GroupBy(x => x.IsAutomatic)
                            .Select(x => new { x.Key }).ToList()
                    }),
            elementSorter: e => e.Key,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Key, a.Key);
                AssertCollection(
                    e.Subquery,
                    a.Subquery,
                    elementSorter: ee => ee.Key,
                    elementAsserter: (ee, aa) =>
                    {
                        Assert.Equal(ee.Key, aa.Key);
                    });
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Correlated_collection_with_groupby_not_projecting_identifier_column_with_group_aggregate_in_final_projection(
        bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>()
                .Select(
                    g => new
                    {
                        Key = g.Nickname,
                        Subquery = g.Weapons
                            .Select(w => new { w.Name, w.IsAutomatic })
                            .GroupBy(x => x.IsAutomatic)
                            .Select(x => new { x.Key, Count = x.Count() }).ToList()
                    }),
            elementSorter: e => e.Key,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Key, a.Key);
                AssertCollection(
                    e.Subquery,
                    a.Subquery,
                    elementSorter: ee => ee.Key,
                    elementAsserter: (ee, aa) =>
                    {
                        Assert.Equal(ee.Key, aa.Key);
                        Assert.Equal(ee.Count, aa.Count);
                    });
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task
        Correlated_collection_with_groupby_not_projecting_identifier_column_with_group_aggregate_in_final_projection_multiple_grouping_keys(
            bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>()
                .Select(
                    g => new
                    {
                        Key = g.Nickname,
                        Subquery = g.Weapons
                            .Select(w => new { w.Name, w.IsAutomatic })
                            .GroupBy(x => new { x.IsAutomatic, x.Name })
                            .Select(x => new { x.Key, Count = x.Count() }).ToList()
                    }),
            elementSorter: e => e.Key,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Key, a.Key);
                AssertCollection(
                    e.Subquery,
                    a.Subquery,
                    elementSorter: ee => ee.Key.Name,
                    elementAsserter: (ee, aa) =>
                    {
                        Assert.Equal(ee.Key.Name, aa.Key.Name);
                        Assert.Equal(ee.Key.IsAutomatic, aa.Key.IsAutomatic);
                        Assert.Equal(ee.Count, aa.Count);
                    });
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task
        Correlated_collection_with_groupby_with_complex_grouping_key_not_projecting_identifier_column_with_group_aggregate_in_final_projection(
            bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>()
                .Select(
                    g => new
                    {
                        Key = g.Nickname,
                        Subquery = g.Weapons
                            .Select(w => new { w.Name, w.IsAutomatic })
                            .GroupBy(x => x.Name.Length)
                            .Select(x => new { x.Key, Count = x.Count() }).ToList()
                    }),
            elementSorter: e => e.Key,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Key, a.Key);
                AssertCollection(
                    e.Subquery,
                    a.Subquery,
                    elementSorter: ee => ee.Key,
                    elementAsserter: (ee, aa) =>
                    {
                        Assert.Equal(ee.Key, aa.Key);
                        Assert.Equal(ee.Count, aa.Count);
                    });
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Correlated_collection_via_SelectMany_with_Distinct_missing_indentifying_columns_in_projection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>()
                .OrderBy(g => g.Nickname)
                .Select(
                    g => g.Weapons.SelectMany(x => x.Owner.AssignedCity.BornGears)
                        .Select(x => (bool?)x.HasSoulPatch).Distinct().ToList()),
            ss => ss.Set<Gear>()
                .OrderBy(g => g.Nickname)
                .Select(
                    g => g.Weapons.SelectMany(x => x.Owner.AssignedCity.Maybe(x => x.BornGears) ?? new List<Gear>())
                        .Select(x => (bool?)x.HasSoulPatch).Distinct().ToList()),
            elementAsserter: (e, a) => AssertCollection(e, a, elementSorter: ee => ee),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Projecting_correlated_collection_followed_by_Distinct(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>()
                .Select(g => g.Weapons)
                .Distinct(),
            elementSorter: e => e.OrderBy(w => w.Id).FirstOrDefault().Id,
            elementAsserter: (e, a) => AssertCollection(e, a));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Projecting_some_properties_as_well_as_correlated_collection_followed_by_Distinct(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>()
                .Select(
                    g => new
                    {
                        g.FullName,
                        g.HasSoulPatch,
                        g.Weapons
                    })
                .Distinct(),
            elementSorter: e => e.FullName,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.FullName, a.FullName);
                Assert.Equal(e.HasSoulPatch, a.HasSoulPatch);
                AssertCollection(e.Weapons, a.Weapons);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Projecting_entity_as_well_as_correlated_collection_followed_by_Distinct(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>()
                .Select(g => new { g, g.Weapons })
                .Distinct(),
            elementSorter: e => e.g.FullName,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.g, a.g);
                AssertCollection(e.Weapons, a.Weapons);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Projecting_entity_as_well_as_complex_correlated_collection_followed_by_Distinct(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>()
                .Select(g => new { g, Weapons = g.Weapons.Where(w => w.Id == g.SquadId).ToList() })
                .Distinct(),
            elementSorter: e => e.g.FullName,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.g, a.g);
                AssertCollection(e.Weapons, a.Weapons);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Projecting_entity_as_well_as_correlated_collection_of_scalars_followed_by_Distinct(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>()
                .Select(g => new { g, Ids = g.Weapons.Select(w => w.Id).ToList() })
                .Distinct(),
            elementSorter: e => e.g.FullName,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.g, a.g);
                AssertCollection(e.Ids, a.Ids);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Correlated_collection_with_distinct_3_levels(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Squad>()
                .Select(
                    s => new
                    {
                        s,
                        Members = s.Members.Select(
                            m => new { m, Weapons = m.Weapons.Where(w => w.OwnerFullName == m.FullName).ToList() }).Distinct()
                    }).Distinct(),
            elementSorter: e => e.s.Id);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Correlated_collection_after_distinct_3_levels(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Squad>()
                .Select(s => new { s.Id, s.Name })
                .Distinct()
                .Select(
                    x => new
                    {
                        x.Id,
                        x.Name,
                        Subquery1 = (from g in ss.Set<Gear>()
                                     where g.SquadId == x.Id
                                     select new
                                     {
                                         g.Nickname,
                                         g.FullName,
                                         g.HasSoulPatch
                                     })
                            .Distinct()
                            .Select(
                                xx => new
                                {
                                    xx.Nickname,
                                    xx.FullName,
                                    xx.HasSoulPatch,
                                    Subquery2 = (from w in ss.Set<Weapon>()
                                                 where w.OwnerFullName == xx.FullName
                                                 select new
                                                 {
                                                     x.Id,
                                                     x.Name,
                                                     xx.Nickname,
                                                     xx.FullName,
                                                     xx.HasSoulPatch
                                                 }).ToList()
                                })
                            .ToList()
                    }),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id, a.Id);
                Assert.Equal(e.Name, a.Name);
                AssertCollection(
                    e.Subquery1,
                    a.Subquery1,
                    elementSorter: ee => ee.Nickname,
                    elementAsserter: (ee, aa) =>
                    {
                        Assert.Equal(ee.Nickname, aa.Nickname);
                        Assert.Equal(ee.FullName, aa.FullName);
                        Assert.Equal(ee.HasSoulPatch, aa.HasSoulPatch);
                        AssertCollection(
                            ee.Subquery2,
                            aa.Subquery2,
                            elementSorter: eee => eee.Id,
                            elementAsserter: (eee, aaa) =>
                            {
                                Assert.Equal(eee.Id, aaa.Id);
                                Assert.Equal(eee.Name, aaa.Name);
                                Assert.Equal(eee.Nickname, aaa.Nickname);
                                Assert.Equal(eee.FullName, aaa.FullName);
                                Assert.Equal(eee.HasSoulPatch, aaa.HasSoulPatch);
                            });
                    });
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Correlated_collection_after_distinct_3_levels_without_original_identifiers(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Squad>()
                .Select(s => new { s.Name.Length })
                .Distinct()
                .Select(
                    x => new
                    {
                        x.Length,
                        Subquery1 = (from g in ss.Set<Gear>()
                                     where g.Nickname.Length == x.Length
                                     select new { g.HasSoulPatch, g.CityOfBirthName })
                            .Distinct()
                            .Select(
                                xx => new
                                {
                                    xx.HasSoulPatch,
                                    Subquery2 = (from w in ss.Set<Weapon>()
                                                 where w.OwnerFullName == xx.CityOfBirthName
                                                 select new
                                                 {
                                                     w.Id,
                                                     x.Length,
                                                     xx.HasSoulPatch
                                                 }).ToList()
                                })
                            .ToList()
                    }),
            elementSorter: e => e.Length,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Length, a.Length);
                AssertCollection(
                    e.Subquery1,
                    a.Subquery1,
                    elementSorter: ee => ee.HasSoulPatch,
                    elementAsserter: (ee, aa) =>
                    {
                        Assert.Equal(ee.HasSoulPatch, aa.HasSoulPatch);
                        AssertCollection(
                            ee.Subquery2,
                            aa.Subquery2,
                            elementSorter: eee => eee.Id,
                            elementAsserter: (eee, aaa) =>
                            {
                                Assert.Equal(eee.Id, aaa.Id);
                                Assert.Equal(eee.Length, aaa.Length);
                                Assert.Equal(eee.HasSoulPatch, aaa.HasSoulPatch);
                            });
                    });
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_DateOnly_Year(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Mission>().Where(m => m.Date.Year == 1990).AsTracking());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_DateOnly_Month(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Mission>().Where(m => m.Date.Month == 11).AsTracking());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_DateOnly_Day(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Mission>().Where(m => m.Date.Day == 10).AsTracking());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_DateOnly_DayOfYear(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Mission>().Where(m => m.Date.DayOfYear == 314).AsTracking());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_DateOnly_DayOfWeek(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Mission>().Where(m => m.Date.DayOfWeek == DayOfWeek.Saturday).AsTracking());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_DateOnly_AddYears(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Mission>().Where(m => m.Date.AddYears(3) == new DateOnly(1993, 11, 10)).AsTracking());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_DateOnly_AddMonths(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Mission>().Where(m => m.Date.AddMonths(3) == new DateOnly(1991, 2, 10)).AsTracking());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_DateOnly_AddDays(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Mission>().Where(m => m.Date.AddDays(3) == new DateOnly(1990, 11, 13)).AsTracking());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_TimeOnly_Hour(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Mission>().Where(m => m.Time.Hour == 10).AsTracking());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_TimeOnly_Minute(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Mission>().Where(m => m.Time.Minute == 15).AsTracking());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_TimeOnly_Second(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Mission>().Where(m => m.Time.Second == 50).AsTracking());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_TimeOnly_Millisecond(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Mission>().Where(m => m.Time.Millisecond == 500).AsTracking());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_TimeOnly_AddHours(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Mission>().Where(m => m.Time.AddHours(3) == new TimeOnly(13, 15, 50, 500)).AsTracking());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_TimeOnly_AddMinutes(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Mission>().Where(m => m.Time.AddMinutes(3) == new TimeOnly(10, 18, 50, 500)).AsTracking());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_TimeOnly_Add_TimeSpan(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Mission>().Where(m => m.Time.Add(new TimeSpan(3, 0, 0)) == new TimeOnly(13, 15, 50, 500)).AsTracking());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_TimeOnly_IsBetween(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Mission>().Where(m => m.Time.IsBetween(new TimeOnly(10, 0, 0), new TimeOnly(11, 0, 0))).AsTracking());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_TimeOnly_subtract_TimeOnly(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Mission>().Where(m => m.Time - new TimeOnly(10, 0, 0) == new TimeSpan(0, 0, 15, 50, 500)).AsTracking());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Basic_query_gears(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_on_entity_that_is_not_present_in_final_projection_but_uses_TypeIs_instead(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>()
                .Include(x => x.Weapons)
                .Include(x => x.Tag)
                .Select(g => new { g.Nickname, IsOfficer = g is Officer }),
            elementSorter: e => e.Nickname,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.Nickname, a.Nickname);
                AssertEqual(e.IsOfficer, a.IsOfficer);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Comparison_with_value_converted_subclass(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Faction>().Where(f => f.ServerAddress == IPAddress.Loopback));

    private static readonly IEnumerable<AmmunitionType?> _weaponTypes = new AmmunitionType?[] { AmmunitionType.Cartridge };

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Contains_on_readonly_enumerable(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Weapon>().Where(w => _weaponTypes.Contains(w.AmmunitionType)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_navigation_defined_on_base_from_entity_with_inheritance_using_soft_cast(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>().Select(
                g => new
                {
                    Gear = g,
                    (g as Officer).Tag,
                    IsNull = (g as Officer).Tag == null,
                    Property = (g as Officer).Nickname,
                    PropertyAfterNavigation = (g as Officer).Tag.Id,
                    NestedOuter = new
                    {
                        (g as Officer).CityOfBirth,
                        IsNull = (g as Officer).CityOfBirth == null,
                        Property = (g as Officer).Nickname,
                        PropertyAfterNavigation = (g as Officer).CityOfBirth.Name,
                        NestedInner = new
                        {
                            (g as Officer).Squad,
                            IsNull = (g as Officer).Squad == null,
                            Property = (g as Officer).Nickname,
                            PropertyAfterNavigation = (g as Officer).Squad.Id
                        }
                    }
                }),
            ss => ss.Set<Gear>().Select(
                g => new
                {
                    Gear = g,
                    g.Tag,
                    IsNull = g.Tag == null,
                    Property = g.Nickname,
                    PropertyAfterNavigation = g.Tag.Id,
                    NestedOuter = new
                    {
                        g.CityOfBirth,
                        IsNull = g.CityOfBirth == null,
                        Property = g.Nickname,
                        PropertyAfterNavigation = g.CityOfBirth.Name,
                        NestedInner = new
                        {
                            g.Squad,
                            IsNull = g.Squad == null,
                            Property = g.Nickname,
                            PropertyAfterNavigation = g.Squad.Id
                        }
                    }
                }),
            elementSorter: e => e.Gear.Nickname,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.Gear, a.Gear);
                AssertEqual(e.Tag, a.Tag);
                AssertEqual(e.IsNull, a.IsNull);
                AssertEqual(e.Property, a.Property);
                AssertEqual(e.PropertyAfterNavigation, a.PropertyAfterNavigation);

                AssertEqual(e.NestedOuter.CityOfBirth, a.NestedOuter.CityOfBirth);
                AssertEqual(e.NestedOuter.IsNull, a.NestedOuter.IsNull);
                AssertEqual(e.NestedOuter.Property, a.NestedOuter.Property);
                AssertEqual(e.NestedOuter.PropertyAfterNavigation, a.NestedOuter.PropertyAfterNavigation);

                AssertEqual(e.NestedOuter.NestedInner.Squad, a.NestedOuter.NestedInner.Squad);
                AssertEqual(e.NestedOuter.NestedInner.IsNull, a.NestedOuter.NestedInner.IsNull);
                AssertEqual(e.NestedOuter.NestedInner.Property, a.NestedOuter.NestedInner.Property);
                AssertEqual(e.NestedOuter.NestedInner.PropertyAfterNavigation, a.NestedOuter.NestedInner.PropertyAfterNavigation);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_navigation_defined_on_derived_from_entity_with_inheritance_using_soft_cast(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<LocustLeader>().Select(
                l => new
                {
                    Leader = l,
                    (l as LocustCommander).DefeatedBy,
                    IsNull = (l as LocustCommander).DefeatedBy == null,
                    Property = (l as LocustCommander).DefeatedByNickname,
                    PropertyAfterNavigation = (bool?)(l as LocustCommander).DefeatedBy.HasSoulPatch,
                    NestedOuter = new
                    {
                        (l as LocustCommander).CommandingFaction,
                        IsNull = (l as LocustCommander).CommandingFaction == null,
                        Property = (int?)(l as LocustCommander).HighCommandId,
                        PropertyAfterNavigation = (l as LocustCommander).CommandingFaction.Eradicated,
                        NestedInner = new
                        {
                            (l as LocustCommander).HighCommand,
                            IsNull = (l as LocustCommander).HighCommand == null,
                            Property = (l as LocustCommander).DefeatedBySquadId,
                            PropertyAfterNavigation = (l as LocustCommander).HighCommand.Name
                        }
                    }
                }),
            ss => ss.Set<LocustLeader>().Select(
                l => new
                {
                    Leader = l,
                    (l as LocustCommander).DefeatedBy,
                    IsNull = (l as LocustCommander).DefeatedBy == null,
                    Property = (l as LocustCommander).DefeatedByNickname,
                    PropertyAfterNavigation = (bool?)(l as LocustCommander).DefeatedBy.HasSoulPatch,
                    NestedOuter = new
                    {
                        (l as LocustCommander).CommandingFaction,
                        IsNull = (l as LocustCommander).CommandingFaction == null,
                        Property = (int?)(l as LocustCommander).HighCommandId,
                        PropertyAfterNavigation = (l as LocustCommander).CommandingFaction.MaybeScalar(x => x.Eradicated),
                        NestedInner = new
                        {
                            (l as LocustCommander).HighCommand,
                            IsNull = (l as LocustCommander).HighCommand == null,
                            Property = (l as LocustCommander).MaybeScalar(x => x.DefeatedBySquadId),
                            PropertyAfterNavigation = (l as LocustCommander).HighCommand.Name
                        }
                    }
                }),
            elementSorter: e => e.Leader.Name,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.Leader, a.Leader);
                AssertEqual(e.DefeatedBy, a.DefeatedBy);
                AssertEqual(e.IsNull, a.IsNull);
                AssertEqual(e.Property, a.Property);
                AssertEqual(e.PropertyAfterNavigation, a.PropertyAfterNavigation);
                AssertEqual(e.NestedOuter.CommandingFaction, a.NestedOuter.CommandingFaction);
                AssertEqual(e.NestedOuter.IsNull, a.NestedOuter.IsNull);
                AssertEqual(e.NestedOuter.Property, a.NestedOuter.Property);
                AssertEqual(e.NestedOuter.PropertyAfterNavigation, a.NestedOuter.PropertyAfterNavigation);
                AssertEqual(e.NestedOuter.NestedInner.HighCommand, a.NestedOuter.NestedInner.HighCommand);
                AssertEqual(e.NestedOuter.NestedInner.IsNull, a.NestedOuter.NestedInner.IsNull);
                AssertEqual(e.NestedOuter.NestedInner.Property, a.NestedOuter.NestedInner.Property);
                AssertEqual(e.NestedOuter.NestedInner.PropertyAfterNavigation, a.NestedOuter.NestedInner.PropertyAfterNavigation);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Join_entity_with_itself_grouped_by_key_followed_by_include_skip_take(bool async)
        => AssertQuery(
            async,
            ss => (from g1 in ss.Set<Gear>()
                   join g2 in ss.Set<Gear>().Where(x => x.Nickname != "Dom").GroupBy(x => x.HasSoulPatch)
                       .Select(g => g.Min(x => x.Nickname.Length)) on g1.Nickname.Length equals g2
                   select g1).Include(x => x.Weapons).OrderBy(x => x.Nickname).Skip(0).Take(10));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_equals_method_on_nullable_with_object_overload(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Mission>().Where(m => m.Rating.Equals(null)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_bool_column_and_Contains(bool async)
    {
        var values = new[] { false, true };
        return AssertQuery(
            async,
            ss => ss.Set<Gear>().Where(g => g.HasSoulPatch && values.Contains(g.HasSoulPatch)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_bool_column_or_Contains(bool async)
    {
        var values = new[] { false, true };
        return AssertQuery(
            async,
            ss => ss.Set<Gear>().Where(g => g.HasSoulPatch && values.Contains(g.HasSoulPatch)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Enum_matching_take_value_gets_different_type_mapping(bool async)
    {
        var value = MilitaryRank.Private;
        return AssertQueryScalar(
            async,
            ss => ss.Set<Gear>()
                .OrderBy(g => g.Nickname)
                .Take(1)
                .Select(g => g.Rank & value));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_subquery_equality_to_null_with_composite_key(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Squad>().Where(s => s.Members.OrderBy(e => e.Nickname).FirstOrDefault() == null),
            assertEmpty: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_subquery_equality_to_null_with_composite_key_should_match_nulls(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Squad>().Where(s => s.Members
                .Where(m => m.FullName == "Anthony Carmine")
                .OrderBy(e => e.Nickname)
                .FirstOrDefault() == null));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_subquery_equality_to_null_without_composite_key(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>().Where(s => s.Weapons.OrderBy(e => e.Name).FirstOrDefault() == null),
            assertEmpty: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_subquery_equality_to_null_without_composite_key_should_match_null(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>().Where(s => s.Weapons.Where(w => w.Name == "Hammer of Dawn").OrderBy(e => e.Name).FirstOrDefault() == null));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task ElementAt_basic_with_OrderBy(bool async)
        => AssertElementAt(
            async,
            ss => ss.Set<Gear>().OrderBy(g => g.FullName),
            () => 0);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task ElementAtOrDefault_basic_with_OrderBy(bool async)
        => AssertElementAtOrDefault(
            async,
            ss => ss.Set<Gear>().OrderBy(g => g.FullName),
            () => 1);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task ElementAtOrDefault_basic_with_OrderBy_parameter(bool async)
    {
        var prm = 2;

        return AssertElementAtOrDefault(
            async,
            ss => ss.Set<Gear>().OrderBy(g => g.FullName),
            () => prm);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_subquery_with_ElementAtOrDefault_equality_to_null_with_composite_key(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Squad>().Where(s => s.Members.OrderBy(e => e.Nickname).ElementAtOrDefault(2) == null));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_subquery_with_ElementAt_using_column_as_index(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Squad>().Where(s => s.Members.OrderBy(m => m.Nickname).ElementAt(s.Id).Nickname == "Cole Train"),
            ss => ss.Set<Squad>().Where(s => s.Members.OrderBy(m => m.Nickname).ElementAtOrDefault(s.Id).Nickname == "Cole Train"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Using_indexer_on_byte_array_and_string_in_projection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Squad>().Select(
                x => new
                {
                    x.Id,
                    ByteArray = x.Banner[0],
                    String = x.Name[1]
                }),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id, a.Id);
                Assert.Equal(e.ByteArray, a.ByteArray);
                Assert.Equal(e.String, a.String);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task DateTimeOffset_to_unix_time_milliseconds(bool async)
    {
        var unixEpochMilliseconds = DateTimeOffset.UnixEpoch.ToUnixTimeMilliseconds();

        return AssertQuery(
            async,
            ss => ss.Set<Gear>()
                .Include(g => g.Squad.Missions)
                .Where(
                    s => s.Squad.Missions
                            .Where(m => unixEpochMilliseconds == m.Mission.Timeline.ToUnixTimeMilliseconds())
                            .FirstOrDefault()
                        == null));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task DateTimeOffset_to_unix_time_seconds(bool async)
    {
        var unixEpochSeconds = DateTimeOffset.UnixEpoch.ToUnixTimeSeconds();

        return AssertQuery(
            async,
            ss => ss.Set<Gear>()
                .Include(g => g.Squad.Missions)
                .Where(
                    s => s.Squad.Missions
                            .Where(m => unixEpochSeconds == m.Mission.Timeline.ToUnixTimeSeconds())
                            .FirstOrDefault()
                        == null));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Set_operator_with_navigation_in_projection_groupby_aggregate(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>()
                .Where(x => ss.Set<Gear>().Concat(ss.Set<Gear>()).Select(x => x.Nickname).Contains("Marcus"))
                .Select(x => new { x.Squad.Name, x.CityOfBirth.Location })
                .GroupBy(x => new { x.Name })
                .Select(x => new { x.Key.Name, SumOfLengths = x.Sum(xx => xx.Location.Length) }));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Nav_expansion_inside_Contains_argument(bool async)
    {
        var numbers = new[] { 1, -1 };

        return AssertQuery(
            async,
            ss => ss.Set<Gear>().Where(x => numbers.Contains(x.Weapons.Any() ? 1 : 0)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Nav_expansion_with_member_pushdown_inside_Contains_argument(bool async)
    {
        var weapons = new[] { "Marcus' Lancer", "Dom's Gnasher" };

        return AssertQuery(
            async,
            ss => ss.Set<Gear>().Where(x => weapons.Contains(x.Weapons.OrderBy(w => w.Id).FirstOrDefault().Name)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Subquery_inside_Take_argument(bool async)
    {
        var numbers = new[] { 0, 1, 2 };

        return AssertQuery(
            async,
            ss => ss.Set<Gear>().OrderBy(x => x.Nickname).Select(
                x => x.Weapons.OrderBy(g => g.Id).Take(numbers.OrderBy(xx => xx).Skip(1).FirstOrDefault())),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a, ordered: true));
    }

    [ConditionalTheory(Skip = "issue #32303")]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Nav_expansion_inside_Skip_correlated_to_source(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<City>().OrderBy(x => x.Name).Select(
                x => x.BornGears.OrderBy(g => g.FullName).Skip(x.StationedGears.Any() ? 1 : 0)));

    [ConditionalTheory(Skip = "issue #32303")]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Nav_expansion_inside_Take_correlated_to_source(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>().OrderBy(x => x.Nickname).Select(
                x => x.Weapons.OrderBy(g => g.Id).Take(x.AssignedCity.Name.Length)));

    [ConditionalTheory(Skip = "issue #32303")]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Nav_expansion_with_member_pushdown_inside_Take_correlated_to_source(bool async)
    {
        var numbers = new[] { 0, 1, 2 };

        return AssertQuery(
            async,
            ss => ss.Set<Gear>().OrderBy(x => x.Nickname).Select(
                x => x.Weapons.OrderBy(g => g.Id).Take(
                    ss.Set<Gear>().OrderBy(xx => xx.Nickname).FirstOrDefault().AssignedCity.Name.Length)),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a, ordered: true));
    }

    [ConditionalTheory(Skip = "issue #32303")]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Nav_expansion_inside_ElementAt_correlated_to_source(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>().OrderBy(x => x.Nickname).Select(
                x => x.Weapons.OrderBy(g => g.Id).ElementAt(x.AssignedCity != null ? 1 : 0)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_one_to_many_on_composite_key_then_orderby_key_properties(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Gear>().Include(x => x.Weapons).OrderBy(x => x.SquadId).ThenBy(x => x.Nickname),
            assertOrder: true,
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Gear>(x => x.Weapons)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Find_underlying_property_after_GroupJoin_DefaultIfEmpty(bool async)
        => AssertQuery(
            async,
            ss => from g in ss.Set<Gear>()
                  join lc in ss.Set<LocustLeader>().OfType<LocustCommander>()
                    on g.Nickname equals lc.DefeatedByNickname into grouping
                  from lc in grouping.DefaultIfEmpty()
                  select new GearLocustLeaderDto { FullName = g.FullName, ThreatLevel = lc.ThreatLevel },
            ss => from g in ss.Set<Gear>()
                  join lc in ss.Set<LocustLeader>().OfType<LocustCommander>()
                    on g.Nickname equals lc.DefeatedByNickname into grouping
                  from lc in grouping.DefaultIfEmpty()
                  select new GearLocustLeaderDto { FullName = g.FullName, ThreatLevel = lc != null ? lc.ThreatLevel : null },
            elementSorter: e => (e.FullName, e.ThreatLevel),
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.FullName, a.FullName);
                Assert.Equal(e.ThreatLevel, a.ThreatLevel);
            });

    private class GearLocustLeaderDto
    {
        public string FullName { get; set; }
        public int? ThreatLevel { get; set; }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Join_include_coalesce_simple(bool async)
    {
        await AssertQuery(
            async,
            ss => from g in ss.Set<Gear>().Include(x => x.Weapons)
                  join o in ss.Set<Gear>() on g.LeaderNickname equals o.Nickname into grouping
                  from o in grouping.DefaultIfEmpty()
                  select new { Result = o ?? g, IsMarcus = g.Nickname == "Marcus" },
            elementSorter: e => e.Result.Nickname,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.IsMarcus, a.IsMarcus);
                if (a.IsMarcus)
                {
                    AssertInclude(e.Result, a.Result, new ExpectedInclude<Gear>(x => x.Weapons));
                }
                else
                {
                    AssertEqual(e.Result, a.Result);
                }
            });

        await AssertQuery(
            async,
            ss => from g in ss.Set<Gear>()
                  join o in ss.Set<Gear>().Include(x => x.Weapons) on g.LeaderNickname equals o.Nickname into grouping
                  from o in grouping.DefaultIfEmpty()
                  select o ?? g,
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Gear>(x => x.Weapons)));

        await AssertQuery(
            async,
            ss => from g in ss.Set<Gear>().Include(x => x.Weapons)
                  join o in ss.Set<Gear>().Include(x => x.Weapons) on g.LeaderNickname equals o.Nickname into grouping
                  from o in grouping.DefaultIfEmpty()
                  select o ?? g,
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Gear>(x => x.Weapons)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Join_include_coalesce_nested(bool async)
    {
        await AssertQuery(
            async,
            ss => from g in ss.Set<Gear>().Include(x => x.Weapons)
                  join o in ss.Set<Gear>() on g.LeaderNickname equals o.Nickname into grouping
                  from o in grouping.DefaultIfEmpty()
                  select new { One = 1, Result = o ?? (g ?? o), IsMarcus = g.Nickname == "Marcus" },
            elementSorter: e => e.Result.Nickname,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.IsMarcus, a.IsMarcus);
                if (a.IsMarcus)
                {
                    AssertInclude(e.Result, a.Result, new ExpectedInclude<Gear>(x => x.Weapons));
                }
                else
                {
                    AssertEqual(e.Result, a.Result);
                }
            });

        await AssertQuery(
            async,
            ss => from g in ss.Set<Gear>()
                  join o in ss.Set<Gear>().Include(x => x.Weapons) on g.LeaderNickname equals o.Nickname into grouping
                  from o in grouping.DefaultIfEmpty()
                  select new { One = 1, Two = o, Result = o ?? (g ?? o) },
            elementSorter: e => e.Result.Nickname,
            elementAsserter: (e, a) =>
            {
                AssertInclude(e.Result, a.Result, new ExpectedInclude<Gear>(x => x.Weapons));
            });
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Join_include_conditional(bool async)
        => AssertQuery(
            async,
            ss => from g in ss.Set<Gear>().Include(x => x.Weapons)
                  join o in ss.Set<Gear>() on g.LeaderNickname equals o.Nickname into grouping
                  from o in grouping.DefaultIfEmpty()
                  select new { Result = o != null ? o : g, IsMarcus = g.Nickname == "Marcus" },
            elementSorter: e => e.Result.Nickname,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.IsMarcus, a.IsMarcus);
                if (a.IsMarcus)
                {
                    AssertInclude(e.Result, a.Result, new ExpectedInclude<Gear>(x => x.Weapons));
                }
                else
                {
                    AssertEqual(e.Result, a.Result);
                }
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Derived_reference_is_skipped_when_base_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<LocustLeader>().Include(x => ((LocustCommander)x).HighCommand),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<LocustCommander>(x => x.HighCommand)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Nested_contains_with_enum(bool async)
    {
        var key = Guid.Parse("5f221fb9-66f4-442a-92c9-d97ed5989cc7");
        var keys = new List<Guid> { Guid.Parse("0a47bcb7-a1cb-4345-8944-c58f82d6aac7"), key };
        var ranks = new List<MilitaryRank> { MilitaryRank.Private };
        var ammoTypes = new List<AmmunitionType?> { AmmunitionType.Cartridge };

        // Note that in this query, the outer Contains really has no type mapping, neither for its source (collection parameter), nor
        // for its item (the conditional expression returns key, which is also a parameter). The default type mapping must be applied.
        await AssertQuery(
            async,
            ss => ss.Set<Gear>().Where(x => keys.Contains(ranks.Contains(x.Rank) ? key : key)));

        await AssertQuery(
            async,
            ss => ss.Set<Weapon>().Where(x => keys.Contains(ammoTypes.Contains(x.AmmunitionType) ? key : key)));
    }

    protected GearsOfWarContext CreateContext()
        => Fixture.CreateContext();

    protected virtual void ClearLog()
    {
    }
}
