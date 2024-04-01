// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.InheritanceModel;

// ReSharper disable InconsistentNaming
// ReSharper disable ReturnValueOfPureMethodIsNotUsed
// ReSharper disable StringEndsWithIsCultureSpecific
namespace Microsoft.EntityFrameworkCore.Query;

public abstract class InheritanceQueryTestBase<TFixture> : QueryTestBase<TFixture>
    where TFixture : InheritanceQueryFixtureBase, new()
{
    protected InheritanceQueryTestBase(TFixture fixture)
        : base(fixture)
    {
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Can_query_when_shared_column(bool async)
    {
        await AssertSingle(
            async,
            ss => ss.Set<Coke>());

        await AssertSingle(
            async,
            ss => ss.Set<Lilt>());

        await AssertSingle(
            async,
            ss => ss.Set<Tea>());
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Can_query_all_types_when_shared_column(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Drink>());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Can_use_of_type_animal(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Animal>().OfType<Animal>().OrderBy(a => a.Species),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Can_use_is_kiwi(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Animal>().Where(a => a is Kiwi));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Can_use_is_kiwi_with_cast(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Animal>().Select(a => new { Value = a is Kiwi ? ((Kiwi)a).FoundOn : default }));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Can_use_backwards_is_animal(bool async)
        => AssertQuery(
            async,
            // ReSharper disable once IsExpressionAlwaysTrue
            // ReSharper disable once ConvertTypeCheckToNullCheck
            ss => ss.Set<Kiwi>().Where(a => a is Animal));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Can_use_is_kiwi_with_other_predicate(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Animal>().Where(a => a is Kiwi && a.CountryId == 1));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Can_use_is_kiwi_in_projection(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Animal>().Select(a => a is Kiwi));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Can_use_of_type_bird(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Animal>().OfType<Bird>().OrderBy(a => a.Species),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Can_use_of_type_bird_predicate(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Animal>()
                .Where(a => a.CountryId == 1)
                .OfType<Bird>()
                .OrderBy(a => a.Species),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Can_use_of_type_bird_with_projection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Animal>()
                .OfType<Bird>()
                .Select(b => new { b.EagleId }));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Can_use_of_type_bird_first(bool async)
        => AssertFirst(
            async,
            ss => ss.Set<Animal>().OfType<Bird>().OrderBy(a => a.Species));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Can_use_of_type_kiwi(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Animal>().OfType<Kiwi>());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Can_use_backwards_of_type_animal(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Kiwi>().OfType<Animal>());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Can_use_of_type_rose(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Plant>().OfType<Rose>());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Can_query_all_animals(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Animal>().OrderBy(a => a.Species),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Can_query_all_animal_views(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<AnimalQuery>().OrderBy(av => av.CountryId),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Can_query_all_plants(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Plant>().OrderBy(a => a.Species),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filter_on_property_inside_complex_type_on_derived_type(bool async)
        => Fixture.EnableComplexTypes
            ? AssertQuery(
                async,
                ss => ss.Set<Daisy>().Where(d => d.AdditionalInfo.LeafStructure.AreLeavesBig))
            : Task.CompletedTask;

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Can_filter_all_animals(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Animal>()
                .OrderBy(a => a.Species)
                .Where(a => a.Name == "Great spotted kiwi"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Can_query_all_birds(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Bird>().OrderBy(a => a.Species),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Can_query_just_kiwis(bool async)
        => AssertSingle(
            async,
            ss => ss.Set<Kiwi>());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Can_query_just_roses(bool async)
        => AssertSingle(
            async,
            ss => ss.Set<Rose>());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Can_include_animals(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Country>()
                .OrderBy(c => c.Name)
                .Include(c => c.Animals),
            elementAsserter: (e, a) =>
            {
                AssertInclude(e, a, new ExpectedInclude<Country>(x => x.Animals));
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Can_include_prey(bool async)
        => AssertSingle(
            async,
            ss => ss.Set<Eagle>()
                .Include(e => e.Prey),
            asserter: (e, a) =>
            {
                AssertInclude(e, a, new ExpectedInclude<Eagle>(x => x.Prey));
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Can_use_of_type_kiwi_where_south_on_derived_property(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Animal>()
                .OfType<Kiwi>()
                .Where(x => x.FoundOn == Island.South));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Can_use_of_type_kiwi_where_north_on_derived_property(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Animal>()
                .OfType<Kiwi>()
                .Where(x => x.FoundOn == Island.North),
            assertEmpty: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Discriminator_used_when_projection_over_derived_type(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Kiwi>().Select(k => k.FoundOn));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Discriminator_used_when_projection_over_derived_type2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Bird>()
                .Select(b => new { b.IsFlightless, Discriminator = EF.Property<string>(b, "Discriminator") }),
            ss => ss.Set<Bird>()
                .Select(b => new { b.IsFlightless, Discriminator = b.GetType().Name }),
            elementSorter: e => (e.IsFlightless, e.Discriminator));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Discriminator_with_cast_in_shadow_property(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Animal>()
                .Where(b => "Kiwi" == EF.Property<string>(b, "Discriminator"))
                .Select(k => new { Predator = EF.Property<string>((Bird)k, "Name") }),
            ss => ss.Set<Animal>()
                .Where(b => b is Kiwi)
                .Select(k => new { Predator = ((Bird)k).Name }),
            elementSorter: e => e.Predator);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Discriminator_used_when_projection_over_of_type(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Animal>().OfType<Kiwi>().Select(k => k.FoundOn));

    [ConditionalFact]
    public virtual Task Can_insert_update_delete()
    {
        int? eagleId = null;
        return TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction, async context =>
            {
                eagleId = (await context.Set<Bird>().AsNoTracking().SingleAsync(e => e.Species == "Aquila chrysaetos canadensis")).Id;

                var kiwi = new Kiwi
                {
                    Species = "Apteryx owenii",
                    Name = "Little spotted kiwi",
                    IsFlightless = true,
                    FoundOn = Island.North
                };

                var nz = await context.Set<Country>().SingleAsync(c => c.Id == 1);

                nz.Animals.Add(kiwi);

                await context.SaveChangesAsync();
            }, async context =>
            {
                var kiwi = await context.Set<Kiwi>().SingleAsync(k => k.Species.EndsWith("owenii"));

                kiwi.EagleId = eagleId;

                await context.SaveChangesAsync();
            }, async context =>
            {
                var kiwi = await context.Set<Kiwi>().SingleAsync(k => k.Species.EndsWith("owenii"));

                context.Set<Bird>().Remove(kiwi);

                await context.SaveChangesAsync();
            }, async context =>
            {
                var count = await context.Set<Kiwi>().CountAsync(k => k.Species.EndsWith("owenii"));

                Assert.Equal(0, count);
            });
    }

    [ConditionalTheory(Skip = "Issue#16298")]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Union_siblings_with_duplicate_property_in_subquery(bool async)
        // Coke and Tea both have CaffeineGrams, which both need to be projected out on each side and so
        // requiring alias uniquification. They also have a different number of properties.
        => AssertQuery(
            async,
            ss => ss.Set<Coke>().Cast<Drink>()
                .Union(ss.Set<Tea>())
                .Where(d => d.SortIndex > 0));

    [ConditionalTheory(Skip = "Issue#16298")]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OfType_Union_subquery(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Animal>()
                .OfType<Kiwi>()
                .Union(ss.Set<Animal>().OfType<Kiwi>())
                .Where(o => o.FoundOn == Island.North));

    [ConditionalTheory(Skip = "Issue#16298")]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OfType_Union_OfType(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Bird>()
                .OfType<Kiwi>()
                .Union(ss.Set<Bird>())
                .OfType<Kiwi>());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Subquery_OfType(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Bird>()
                .OrderBy(b => b.Species)
                .Take(5)
                .Distinct()
                .OfType<Kiwi>());

    [ConditionalTheory(Skip = "Issue#16298")]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Union_entity_equality(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Kiwi>()
                .Union(ss.Set<Eagle>().Cast<Bird>())
                .Where(b => b == null));

    [ConditionalFact]
    public virtual async Task Setting_foreign_key_to_a_different_type_throws()
    {
        using var context = CreateContext();
        var kiwi = await context.Set<Kiwi>().SingleAsync();

        var eagle = new Eagle
        {
            Species = "Haliaeetus leucocephalus",
            Name = "Bald eagle",
            Group = EagleGroup.Booted,
            EagleId = kiwi.Id
        };

        await context.AddAsync(eagle);

        // No fixup, because no principal with this key of the correct type is loaded.
        Assert.Empty(eagle.Prey);

        if (EnforcesFkConstraints)
        {
            // Relational database throws due to constraint violation
            await Assert.ThrowsAsync<DbUpdateException>(async () => await context.SaveChangesAsync());
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Byte_enum_value_constant_used_in_projection(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Kiwi>().Select(k => k.IsFlightless ? Island.North : Island.South));

    [ConditionalFact]
    public virtual async Task Member_access_on_intermediate_type_works()
    {
        using var context = CreateContext();
        var query = context.Set<Kiwi>().Select(k => new Kiwi { Name = k.Name });

        var parameter = Expression.Parameter(query.ElementType, "p");
        var property = Expression.Property(parameter, "Name");
        var getProperty = Expression.Lambda(property, parameter);

        var expression = Expression.Call(
            typeof(Queryable), nameof(Queryable.OrderBy),
            [query.ElementType, typeof(string)], query.Expression, Expression.Quote(getProperty));

        query = query.Provider.CreateQuery<Kiwi>(expression);

        var result = await query.ToListAsync();

        var kiwi = Assert.Single(result);
        Assert.Equal("Great spotted kiwi", kiwi.Name);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Is_operator_on_result_of_FirstOrDefault(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Animal>()
                .Where(a => ss.Set<Animal>().FirstOrDefault(a1 => a1.Name == "Great spotted kiwi") is Kiwi)
                .OrderBy(a => a.Species),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Selecting_only_base_properties_on_base_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Animal>().Select(a => new { a.Name }),
            elementSorter: e => e.Name);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Selecting_only_base_properties_on_derived_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Bird>().Select(a => new { a.Name }),
            elementSorter: e => e.Name);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Using_is_operator_on_multiple_type_with_no_result(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Animal>().Where(e => e is Kiwi).Where(e => e is Eagle),
            assertEmpty: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Using_is_operator_with_of_type_on_multiple_type_with_no_result(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Animal>().Where(e => e is Kiwi).OfType<Eagle>(),
            assertEmpty: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Using_OfType_on_multiple_type_with_no_result(bool async)
        => AssertTranslationFailed(
            () => AssertQuery(
                async,
                ss => ss.Set<Animal>().OfType<Eagle>().OfType<Kiwi>(),
                elementSorter: e => e.Name));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GetType_in_hierarchy_in_abstract_base_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Animal>().Where(e => e.GetType() == typeof(Animal)),
            assertEmpty: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GetType_in_hierarchy_in_intermediate_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Animal>().Where(e => e.GetType() == typeof(Bird)),
            assertEmpty: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GetType_in_hierarchy_in_leaf_type_with_sibling(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Animal>().Where(e => e.GetType() == typeof(Eagle)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GetType_in_hierarchy_in_leaf_type_with_sibling2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Animal>().Where(e => e.GetType() == typeof(Kiwi)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GetType_in_hierarchy_in_leaf_type_with_sibling2_reverse(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Animal>().Where(e => typeof(Kiwi) == e.GetType()));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GetType_in_hierarchy_in_leaf_type_with_sibling2_not_equal(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Animal>().Where(e => typeof(Kiwi) != e.GetType()));

    protected InheritanceContext CreateContext()
        => Fixture.CreateContext();

    protected virtual void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
    {
    }

    protected virtual bool EnforcesFkConstraints
        => true;

    protected virtual void ClearLog()
    {
    }
}
