// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestModels.InheritanceModel;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable InconsistentNaming
// ReSharper disable ReturnValueOfPureMethodIsNotUsed
// ReSharper disable StringEndsWithIsCultureSpecific
namespace Microsoft.EntityFrameworkCore.Query
{
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
                ss => ss.Set<Coke>(),
                entryCount: 1);

            await AssertSingle(
                async,
                ss => ss.Set<Lilt>(),
                entryCount: 1);

            await AssertSingle(
                async,
                ss => ss.Set<Tea>(),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Can_query_all_types_when_shared_column(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Drink>(),
                entryCount: 3);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Can_use_of_type_animal(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Animal>().OfType<Animal>().OrderBy(a => a.Species),
                assertOrder: true,
                entryCount: 2);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Can_use_is_kiwi(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Animal>().Where(a => a is Kiwi),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Can_use_backwards_is_animal(bool async)
        {
            return AssertQuery(
                async,
                // ReSharper disable once IsExpressionAlwaysTrue
                ss => ss.Set<Kiwi>().Where(a => a is Animal),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Can_use_is_kiwi_with_other_predicate(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Animal>().Where(a => a is Kiwi && a.CountryId == 1),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Can_use_is_kiwi_in_projection(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Animal>().Select(a => a is Kiwi));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Can_use_of_type_bird(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Animal>().OfType<Bird>().OrderBy(a => a.Species),
                assertOrder: true,
                entryCount: 2);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Can_use_of_type_bird_predicate(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Animal>()
                    .Where(a => a.CountryId == 1)
                    .OfType<Bird>()
                    .OrderBy(a => a.Species),
                assertOrder: true,
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Can_use_of_type_bird_with_projection(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Animal>()
                    .OfType<Bird>()
                    .Select(b => new { b.EagleId }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Can_use_of_type_bird_first(bool async)
        {
            return AssertFirst(
                async,
                ss => ss.Set<Animal>().OfType<Bird>().OrderBy(a => a.Species),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Can_use_of_type_kiwi(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Animal>().OfType<Kiwi>(),
                entryCount: 1);
        }

        [ConditionalTheory(Skip = "Issue#17364")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Can_use_backwards_of_type_animal(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Kiwi>().OfType<Animal>(),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Can_use_of_type_rose(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Plant>().OfType<Rose>(),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Can_query_all_animals(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Animal>().OrderBy(a => a.Species),
                assertOrder: true,
                entryCount: 2);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Can_query_all_animal_views(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<AnimalQuery>().OrderBy(av => av.CountryId),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Can_query_all_plants(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Plant>().OrderBy(a => a.Species),
                assertOrder: true,
                entryCount: 2);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Can_filter_all_animals(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Animal>()
                    .OrderBy(a => a.Species)
                    .Where(a => a.Name == "Great spotted kiwi"),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Can_query_all_birds(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Bird>().OrderBy(a => a.Species),
                assertOrder: true,
                entryCount: 2);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Can_query_just_kiwis(bool async)
        {
            return AssertSingle(
                async,
                ss => ss.Set<Kiwi>(),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Can_query_just_roses(bool async)
        {
            return AssertSingle(
                async,
                ss => ss.Set<Rose>(),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Can_include_animals(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Country>()
                    .OrderBy(c => c.Name)
                    .Include(c => c.Animals),
                entryCount: 4,
                elementAsserter: (e, a) =>
                {
                    AssertInclude(e, a, new ExpectedInclude<Country>(x => x.Animals));
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Can_include_prey(bool async)
        {
            return AssertSingle(
                async,
                ss => ss.Set<Eagle>()
                    .Include(e => e.Prey),
                asserter: (e, a) =>
                {
                    AssertInclude(e, a, new ExpectedInclude<Eagle>(x => x.Prey));
                },
                entryCount: 2);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Can_use_of_type_kiwi_where_south_on_derived_property(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Animal>()
                    .OfType<Kiwi>()
                    .Where(x => x.FoundOn == Island.South),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Can_use_of_type_kiwi_where_north_on_derived_property(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Animal>()
                    .OfType<Kiwi>()
                    .Where(x => x.FoundOn == Island.North));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Discriminator_used_when_projection_over_derived_type(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Kiwi>().Select(k => k.FoundOn));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Discriminator_used_when_projection_over_derived_type2(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Bird>()
                    .Select(b => new { b.IsFlightless, Discriminator = EF.Property<string>(b, "Discriminator") }),
                ss => ss.Set<Bird>()
                    .Select(b => new { b.IsFlightless, Discriminator = b.GetType().Name }),
                elementSorter: e => (e.IsFlightless, e.Discriminator));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Discriminator_with_cast_in_shadow_property(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Animal>()
                    .Where(b => "Kiwi" == EF.Property<string>(b, "Discriminator"))
                    .Select(k => new { Predator = EF.Property<string>((Bird)k, "EagleId") }),
                ss => ss.Set<Animal>()
                    .Where(b => b is Kiwi)
                    .Select(k => new { Predator = ((Bird)k).EagleId }),
                elementSorter: e => e.Predator);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Discriminator_used_when_projection_over_of_type(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Animal>().OfType<Kiwi>().Select(k => k.FoundOn));
        }

        [ConditionalFact]
        public virtual void Can_insert_update_delete()
        {
            TestHelpers.ExecuteWithStrategyInTransaction(
                CreateContext,
                UseTransaction,
                context =>
                {
                    var kiwi = new Kiwi
                    {
                        Species = "Apteryx owenii",
                        Name = "Little spotted kiwi",
                        IsFlightless = true,
                        FoundOn = Island.North
                    };

                    var nz = context.Set<Country>().Single(c => c.Id == 1);

                    nz.Animals.Add(kiwi);

                    context.SaveChanges();
                },
                context =>
                {
                    var kiwi = context.Set<Kiwi>().Single(k => k.Species.EndsWith("owenii"));

                    kiwi.EagleId = "Aquila chrysaetos canadensis";

                    context.SaveChanges();
                },
                context =>
                {
                    var kiwi = context.Set<Kiwi>().Single(k => k.Species.EndsWith("owenii"));

                    Assert.Equal("Aquila chrysaetos canadensis", kiwi.EagleId);

                    context.Set<Bird>().Remove(kiwi);

                    context.SaveChanges();
                },
                context =>
                {
                    var count = context.Set<Kiwi>().Count(k => k.Species.EndsWith("owenii"));

                    Assert.Equal(0, count);
                });
        }

        [ConditionalTheory(Skip = "Issue#16298")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Union_siblings_with_duplicate_property_in_subquery(bool async)
        {
            // Coke and Tea both have CaffeineGrams, which both need to be projected out on each side and so
            // requiring alias uniquification. They also have a different number of properties.
            return AssertQuery(
                async,
                ss => ss.Set<Coke>().Cast<Drink>()
                    .Union(ss.Set<Tea>())
                    .Where(d => d.Id > 0),
                entryCount: 2);
        }

        [ConditionalTheory(Skip = "Issue#16298")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OfType_Union_subquery(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Animal>()
                    .OfType<Kiwi>()
                    .Union(ss.Set<Animal>().OfType<Kiwi>())
                    .Where(o => o.FoundOn == Island.North));
        }

        [ConditionalTheory(Skip = "Issue#16298")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OfType_Union_OfType(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Bird>()
                    .OfType<Kiwi>()
                    .Union(ss.Set<Bird>())
                    .OfType<Kiwi>());
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Subquery_OfType(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Bird>()
                    .OrderBy(b => b.Species)
                    .Take(5)
                    .Distinct()
                    .OfType<Kiwi>(),
                entryCount: 1);
        }

        [ConditionalTheory(Skip = "Issue#16298")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Union_entity_equality(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Kiwi>()
                    .Union(ss.Set<Eagle>().Cast<Bird>())
                    .Where(b => b == null));
        }

        [ConditionalFact]
        public virtual void Setting_foreign_key_to_a_different_type_throws()
        {
            using var context = CreateContext();
            var kiwi = context.Set<Kiwi>().Single();

            var eagle = new Eagle
            {
                Species = "Haliaeetus leucocephalus",
                Name = "Bald eagle",
                Group = EagleGroup.Booted,
                EagleId = kiwi.Species
            };

            context.Add(eagle);

            // No fixup, because no principal with this key of the correct type is loaded.
            Assert.Empty(eagle.Prey);

            if (EnforcesFkConstraints)
            {
                // Relational database throws due to constraint violation
                Assert.Throws<DbUpdateException>(() => context.SaveChanges());
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Byte_enum_value_constant_used_in_projection(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Kiwi>().Select(k => k.IsFlightless ? Island.North : Island.South));
        }

        [ConditionalFact]
        public virtual void Member_access_on_intermediate_type_works()
        {
            using var context = CreateContext();
            var query = context.Set<Kiwi>().Select(k => new Kiwi { Name = k.Name });

            var parameter = Expression.Parameter(query.ElementType, "p");
            var property = Expression.Property(parameter, "Name");
            var getProperty = Expression.Lambda(property, parameter);

            var expression = Expression.Call(
                typeof(Queryable), nameof(Queryable.OrderBy),
                new[] { query.ElementType, typeof(string) }, query.Expression, Expression.Quote(getProperty));

            query = query.Provider.CreateQuery<Kiwi>(expression);

            var result = query.ToList();

            var kiwi = Assert.Single(result);
            Assert.Equal("Great spotted kiwi", kiwi.Name);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Is_operator_on_result_of_FirstOrDefault(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Animal>()
                    .Where(a => ss.Set<Animal>().FirstOrDefault(a1 => a1.Name == "Great spotted kiwi") is Kiwi)
                    .OrderBy(a => a.Species),
                assertOrder: true,
                entryCount: 2);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Selecting_only_base_properties_on_base_type(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Animal>().Select(a => new { a.Name }),
                elementSorter: e => e.Name);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Selecting_only_base_properties_on_derived_type(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Bird>().Select(a => new { a.Name }),
                elementSorter: e => e.Name);
        }

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
}
