// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public abstract class KeysWithConvertersTestBase<TFixture> : IClassFixture<TFixture>
    where TFixture : KeysWithConvertersTestBase<TFixture>.KeysWithConvertersFixtureBase, new()
{
    protected KeysWithConvertersTestBase(TFixture fixture)
    {
        Fixture = fixture;
    }

    protected TFixture Fixture { get; }

    protected DbContext CreateContext()
        => Fixture.CreateContext();

    [ConditionalFact]
    public virtual async Task Can_insert_and_read_back_with_struct_key_and_optional_dependents()
    {
        IntStructKeyPrincipal[] principals = null;
        IntStructKeyOptionalDependent[] dependents = null;
        await InsertOptionalGraph<IntStructKeyPrincipal, IntStructKeyOptionalDependent>();

        using (var context = CreateContext())
        {
            await RunQueries(context);

            Validate(
                principals,
                dependents,
                [(0, [0]), (1, [1]), (2, [2, 2, 2]), (3, [])],
                [(0, 0), (1, 1), (2, 2), (3, 2), (4, 2), (5, null)]);

            foreach (var principal in principals)
            {
                principal.Foo = "Mutant!";
            }

            dependents[5].Principal = principals[0];
            dependents[4].PrincipalId = null;
            dependents[3].PrincipalId = principals[0].Id;
            principals[1].OptionalDependents.Clear();

            context.Remove(dependents[0]);
            principals[0].OptionalDependents.Add(
                new IntStructKeyOptionalDependent { Id = new IntStructKey(dependents[0].Id.Id), });

            await context.SaveChangesAsync();
        }

        using (var context = CreateContext())
        {
            await RunQueries(context);

            Validate(
                principals,
                dependents,
                [(0, [0, 3, 5]), (1, []), (2, [2]), (3, [])],
                [(0, 0), (1, null), (2, 2), (3, 0), (4, null), (5, 0)]);
        }

        async Task RunQueries(DbContext context)
        {
            var two = 2;
            var three = new IntStructKey(3);

            principals =
            [
                await context.Set<IntStructKeyPrincipal>().Include(e => e.OptionalDependents)
                    .SingleAsync(e => e.Id.Equals(new IntStructKey(1))),
                await context.Set<IntStructKeyPrincipal>().Include(e => e.OptionalDependents)
                    .SingleAsync(e => e.Id.Equals(new IntStructKey { Id = two })),
                await context.Set<IntStructKeyPrincipal>().Include(e => e.OptionalDependents).SingleAsync(e => e.Id.Equals(three)),
                await context.Set<IntStructKeyPrincipal>().Include(e => e.OptionalDependents)
                    .SingleAsync(e => e.Id.Equals(new IntStructKey { Id = 4 }))
            ];

            var oneOhTwo = 102;
            var oneOhThree = new IntStructKey(103);
            var oneOhFive = 105;
            var oneOhSix = new IntStructKey(106);

            dependents =
            [
                await context.Set<IntStructKeyOptionalDependent>().SingleAsync(e => e.Id.Equals(new IntStructKey(101))),
                await context.Set<IntStructKeyOptionalDependent>().SingleAsync(e => e.Id.Equals(new IntStructKey(oneOhTwo))),
                await context.Set<IntStructKeyOptionalDependent>().SingleAsync(e => e.Id.Equals(oneOhThree)),
                await context.Set<IntStructKeyOptionalDependent>().SingleAsync(e => e.Id.Equals(new IntStructKey(104))),
                await context.Set<IntStructKeyOptionalDependent>().SingleAsync(e => e.Id.Equals(new IntStructKey(oneOhFive))),
                await context.Set<IntStructKeyOptionalDependent>().SingleAsync(e => e.Id.Equals(oneOhSix))
            ];

            Assert.Same(dependents[0], await context.Set<IntStructKeyOptionalDependent>().FindAsync(new IntStructKey(101)));
            Assert.Same(dependents[1], await context.Set<IntStructKeyOptionalDependent>().FindAsync(new IntStructKey(oneOhTwo)));
            Assert.Same(dependents[2], await context.Set<IntStructKeyOptionalDependent>().FindAsync(oneOhThree));
            Assert.Same(dependents[3], await context.FindAsync(typeof(IntStructKeyOptionalDependent), new IntStructKey(104)));
            Assert.Same(dependents[4], await context.FindAsync(typeof(IntStructKeyOptionalDependent), new IntStructKey(oneOhFive)));
            Assert.Same(dependents[5], await context.FindAsync(typeof(IntStructKeyOptionalDependent), oneOhSix));
        }

        void Validate(
            IntStructKeyPrincipal[] principals,
            IntStructKeyOptionalDependent[] dependents,
            (int, int[])[] expectedPrincipalToDependents,
            (int, int?)[] expectedDependentToPrincipals)
            => ValidateOptional(
                principals,
                dependents,
                expectedPrincipalToDependents,
                expectedDependentToPrincipals,
                p => ((IntStructKeyPrincipal)p).OptionalDependents.Select(d => (IIntOptionalDependent)d).ToList(),
                d => ((IntStructKeyOptionalDependent)d).Principal);
    }

    [ConditionalFact]
    public virtual async Task Can_insert_and_read_back_with_comparable_struct_key_and_optional_dependents()
    {
        ComparableIntStructKeyPrincipal[] principals = null;
        ComparableIntStructKeyOptionalDependent[] dependents = null;
        await InsertOptionalGraph<ComparableIntStructKeyPrincipal, ComparableIntStructKeyOptionalDependent>();

        using (var context = CreateContext())
        {
            await RunQueries(context);

            Validate(
                principals,
                dependents,
                [(0, [0]), (1, [1]), (2, [2, 2, 2]), (3, [])],
                [(0, 0), (1, 1), (2, 2), (3, 2), (4, 2), (5, null)]);

            foreach (var principal in principals)
            {
                principal.Foo = "Mutant!";
            }

            dependents[5].Principal = principals[0];
            dependents[4].PrincipalId = null;
            dependents[3].PrincipalId = principals[0].Id;
            principals[1].OptionalDependents.Clear();

            context.Remove(dependents[0]);
            principals[0].OptionalDependents.Add(
                new ComparableIntStructKeyOptionalDependent { Id = new ComparableIntStructKey(dependents[0].Id.Id), });

            await context.SaveChangesAsync();
        }

        using (var context = CreateContext())
        {
            await RunQueries(context);

            Validate(
                principals,
                dependents,
                [(0, [0, 3, 5]), (1, []), (2, [2]), (3, [])],
                [(0, 0), (1, null), (2, 2), (3, 0), (4, null), (5, 0)]);
        }

        async Task RunQueries(DbContext context)
        {
            var two = 2;
            var three = new ComparableIntStructKey(3);

            principals =
            [
                await context.Set<ComparableIntStructKeyPrincipal>().Include(e => e.OptionalDependents)
                    .SingleAsync(e => e.Id.Equals(new ComparableIntStructKey(1))),
                await context.Set<ComparableIntStructKeyPrincipal>().Include(e => e.OptionalDependents)
                    .SingleAsync(e => e.Id.Equals(new ComparableIntStructKey { Id = two })),
                await context.Set<ComparableIntStructKeyPrincipal>().Include(e => e.OptionalDependents).SingleAsync(e => e.Id.Equals(three)),
                await context.Set<ComparableIntStructKeyPrincipal>().Include(e => e.OptionalDependents)
                    .SingleAsync(e => e.Id.Equals(new ComparableIntStructKey { Id = 4 }))
            ];

            var oneOhTwo = 102;
            var oneOhThree = new ComparableIntStructKey(103);
            var oneOhFive = 105;
            var oneOhSix = new ComparableIntStructKey(106);

            dependents =
            [
                await context.Set<ComparableIntStructKeyOptionalDependent>()
                    .SingleAsync(e => e.Id.Equals(new ComparableIntStructKey(101))),
                await context.Set<ComparableIntStructKeyOptionalDependent>()
                    .SingleAsync(e => e.Id.Equals(new ComparableIntStructKey(oneOhTwo))),
                await context.Set<ComparableIntStructKeyOptionalDependent>().SingleAsync(e => e.Id.Equals(oneOhThree)),
                await context.Set<ComparableIntStructKeyOptionalDependent>()
                    .SingleAsync(e => e.Id.Equals(new ComparableIntStructKey(104))),
                await context.Set<ComparableIntStructKeyOptionalDependent>()
                    .SingleAsync(e => e.Id.Equals(new ComparableIntStructKey(oneOhFive))),
                await context.Set<ComparableIntStructKeyOptionalDependent>().SingleAsync(e => e.Id.Equals(oneOhSix))
            ];

            Assert.Same(
                dependents[0], await context.Set<ComparableIntStructKeyOptionalDependent>().FindAsync(new ComparableIntStructKey(101)));
            Assert.Same(
                dependents[1],
                await context.Set<ComparableIntStructKeyOptionalDependent>().FindAsync(new ComparableIntStructKey(oneOhTwo)));
            Assert.Same(dependents[2], await context.Set<ComparableIntStructKeyOptionalDependent>().FindAsync(oneOhThree));
            Assert.Same(
                dependents[3], await context.FindAsync(typeof(ComparableIntStructKeyOptionalDependent), new ComparableIntStructKey(104)));
            Assert.Same(
                dependents[4],
                await context.FindAsync(typeof(ComparableIntStructKeyOptionalDependent), new ComparableIntStructKey(oneOhFive)));
            Assert.Same(dependents[5], await context.FindAsync(typeof(ComparableIntStructKeyOptionalDependent), oneOhSix));
        }

        void Validate(
            ComparableIntStructKeyPrincipal[] principals,
            ComparableIntStructKeyOptionalDependent[] dependents,
            (int, int[])[] expectedPrincipalToDependents,
            (int, int?)[] expectedDependentToPrincipals)
            => ValidateOptional(
                principals,
                dependents,
                expectedPrincipalToDependents,
                expectedDependentToPrincipals,
                p => ((ComparableIntStructKeyPrincipal)p).OptionalDependents.Select(d => (IIntOptionalDependent)d).ToList(),
                d => ((ComparableIntStructKeyOptionalDependent)d).Principal);
    }

    [ConditionalFact]
    public virtual async Task Can_insert_and_read_back_with_generic_comparable_struct_key_and_optional_dependents()
    {
        GenericComparableIntStructKeyPrincipal[] principals = null;
        GenericComparableIntStructKeyOptionalDependent[] dependents = null;
        await InsertOptionalGraph<GenericComparableIntStructKeyPrincipal, GenericComparableIntStructKeyOptionalDependent>();

        using (var context = CreateContext())
        {
            await RunQueries(context);

            Validate(
                principals,
                dependents,
                [(0, [0]), (1, [1]), (2, [2, 2, 2]), (3, [])],
                [(0, 0), (1, 1), (2, 2), (3, 2), (4, 2), (5, null)]);

            foreach (var principal in principals)
            {
                principal.Foo = "Mutant!";
            }

            dependents[5].Principal = principals[0];
            dependents[4].PrincipalId = null;
            dependents[3].PrincipalId = principals[0].Id;
            principals[1].OptionalDependents.Clear();

            context.Remove(dependents[0]);
            principals[0].OptionalDependents.Add(
                new GenericComparableIntStructKeyOptionalDependent { Id = new GenericComparableIntStructKey(dependents[0].Id.Id), });

            await context.SaveChangesAsync();
        }

        using (var context = CreateContext())
        {
            await RunQueries(context);

            Validate(
                principals,
                dependents,
                [(0, [0, 3, 5]), (1, []), (2, [2]), (3, [])],
                [(0, 0), (1, null), (2, 2), (3, 0), (4, null), (5, 0)]);
        }

        async Task RunQueries(DbContext context)
        {
            var two = 2;
            var three = new GenericComparableIntStructKey(3);

            principals =
            [
                await context.Set<GenericComparableIntStructKeyPrincipal>().Include(e => e.OptionalDependents)
                    .SingleAsync(e => e.Id.Equals(new GenericComparableIntStructKey(1))),
                await context.Set<GenericComparableIntStructKeyPrincipal>().Include(e => e.OptionalDependents)
                    .SingleAsync(e => e.Id.Equals(new GenericComparableIntStructKey { Id = two })),
                await context.Set<GenericComparableIntStructKeyPrincipal>().Include(e => e.OptionalDependents)
                    .SingleAsync(e => e.Id.Equals(three)),
                await context.Set<GenericComparableIntStructKeyPrincipal>().Include(e => e.OptionalDependents)
                    .SingleAsync(e => e.Id.Equals(new GenericComparableIntStructKey { Id = 4 }))
            ];

            var oneOhTwo = 102;
            var oneOhThree = new GenericComparableIntStructKey(103);
            var oneOhFive = 105;
            var oneOhSix = new GenericComparableIntStructKey(106);

            dependents =
            [
                await context.Set<GenericComparableIntStructKeyOptionalDependent>()
                    .SingleAsync(e => e.Id.Equals(new GenericComparableIntStructKey(101))),
                await context.Set<GenericComparableIntStructKeyOptionalDependent>()
                    .SingleAsync(e => e.Id.Equals(new GenericComparableIntStructKey(oneOhTwo))),
                await context.Set<GenericComparableIntStructKeyOptionalDependent>().SingleAsync(e => e.Id.Equals(oneOhThree)),
                await context.Set<GenericComparableIntStructKeyOptionalDependent>()
                    .SingleAsync(e => e.Id.Equals(new GenericComparableIntStructKey(104))),
                await context.Set<GenericComparableIntStructKeyOptionalDependent>()
                    .SingleAsync(e => e.Id.Equals(new GenericComparableIntStructKey(oneOhFive))),
                await context.Set<GenericComparableIntStructKeyOptionalDependent>().SingleAsync(e => e.Id.Equals(oneOhSix))
            ];

            Assert.Same(
                dependents[0],
                await context.Set<GenericComparableIntStructKeyOptionalDependent>().FindAsync(new GenericComparableIntStructKey(101)));
            Assert.Same(
                dependents[1],
                await context.Set<GenericComparableIntStructKeyOptionalDependent>()
                    .FindAsync(new GenericComparableIntStructKey(oneOhTwo)));
            Assert.Same(dependents[2], await context.Set<GenericComparableIntStructKeyOptionalDependent>().FindAsync(oneOhThree));
            Assert.Same(
                dependents[3],
                await context.FindAsync(typeof(GenericComparableIntStructKeyOptionalDependent), new GenericComparableIntStructKey(104)));
            Assert.Same(
                dependents[4],
                await context.FindAsync(
                    typeof(GenericComparableIntStructKeyOptionalDependent), new GenericComparableIntStructKey(oneOhFive)));
            Assert.Same(dependents[5], await context.FindAsync(typeof(GenericComparableIntStructKeyOptionalDependent), oneOhSix));
        }

        void Validate(
            GenericComparableIntStructKeyPrincipal[] principals,
            GenericComparableIntStructKeyOptionalDependent[] dependents,
            (int, int[])[] expectedPrincipalToDependents,
            (int, int?)[] expectedDependentToPrincipals)
            => ValidateOptional(
                principals,
                dependents,
                expectedPrincipalToDependents,
                expectedDependentToPrincipals,
                p => ((GenericComparableIntStructKeyPrincipal)p).OptionalDependents.Select(d => (IIntOptionalDependent)d).ToList(),
                d => ((GenericComparableIntStructKeyOptionalDependent)d).Principal);
    }

    [ConditionalFact]
    public virtual async Task Can_insert_and_read_back_with_struct_key_and_required_dependents()
    {
        IntStructKeyPrincipal[] principals = null;
        IntStructKeyRequiredDependent[] dependents = null;
        await InsertRequiredGraph<IntStructKeyPrincipal, IntStructKeyRequiredDependent>();

        using (var context = CreateContext())
        {
            await RunQueries(context);

            Validate(
                principals,
                dependents,
                [(0, [0]), (1, [1]), (2, [2, 2, 2, 2]), (3, [])],
                [(0, 0), (1, 1), (2, 2), (3, 2), (4, 2), (5, 2)]);

            foreach (var principal in principals)
            {
                principal.Foo = "Mutant!";
            }

            dependents[5].Principal = principals[0];
            context.Remove(dependents[4]);
            dependents[3].PrincipalId = principals[0].Id;
            principals[1].RequiredDependents.Clear();

            context.Remove(dependents[0]);
            principals[0].RequiredDependents.Add(
                new IntStructKeyRequiredDependent { Id = new IntStructKey(dependents[0].Id.Id), });

            await context.SaveChangesAsync();
        }

        using (var context = CreateContext())
        {
            await RunQueries(context);

            Validate(
                principals,
                dependents,
                [(0, [0, 3, 5]), (1, []), (2, [2]), (3, [])],
                [(0, 0), (2, 2), (3, 0), (5, 0)]);
        }

        async Task RunQueries(DbContext context)
        {
            var twelve = 12;
            var thirteen = new IntStructKey { Id = 13 };

            principals =
            [
                await context.Set<IntStructKeyPrincipal>().Include(e => e.RequiredDependents)
                    .SingleAsync(e => e.Id.Equals(new IntStructKey { Id = 11 })),
                await context.Set<IntStructKeyPrincipal>().Include(e => e.RequiredDependents)
                    .SingleAsync(e => e.Id.Equals(new IntStructKey { Id = twelve })),
                await context.Set<IntStructKeyPrincipal>().Include(e => e.RequiredDependents).SingleAsync(e => e.Id.Equals(thirteen)),
                await context.Set<IntStructKeyPrincipal>().Include(e => e.RequiredDependents)
                    .SingleAsync(e => e.Id.Equals(new IntStructKey { Id = 14 }))
            ];

            var oneTwelve = 112;
            var oneThirteen = new IntStructKey { Id = 113 };
            var oneFifteeen = 115;
            var oneSixteen = new IntStructKey { Id = 116 };

            dependents =
            [
                await context.Set<IntStructKeyRequiredDependent>().FirstOrDefaultAsync(e => e.Id.Equals(new IntStructKey { Id = 111 })),
                await context.Set<IntStructKeyRequiredDependent>().FirstOrDefaultAsync(e => e.Id.Equals(new IntStructKey { Id = oneTwelve })),
                await context.Set<IntStructKeyRequiredDependent>().FirstOrDefaultAsync(e => e.Id.Equals(oneThirteen)),
                await context.Set<IntStructKeyRequiredDependent>().FirstOrDefaultAsync(e => e.Id.Equals(new IntStructKey { Id = 114 })),
                await context.Set<IntStructKeyRequiredDependent>()
                    .FirstOrDefaultAsync(e => e.Id.Equals(new IntStructKey { Id = oneFifteeen })),
                await context.Set<IntStructKeyRequiredDependent>().FirstOrDefaultAsync(e => e.Id.Equals(oneSixteen))
            ];

            Assert.Same(dependents[0], await context.Set<IntStructKeyRequiredDependent>().FindAsync(new IntStructKey { Id = 111 }));
            Assert.Same(dependents[1], await context.Set<IntStructKeyRequiredDependent>().FindAsync(new IntStructKey { Id = oneTwelve }));
            Assert.Same(dependents[2], await context.Set<IntStructKeyRequiredDependent>().FindAsync(oneThirteen));
            Assert.Same(dependents[3], await context.FindAsync(typeof(IntStructKeyRequiredDependent), new IntStructKey { Id = 114 }));
            Assert.Same(dependents[4], await context.FindAsync(typeof(IntStructKeyRequiredDependent), new IntStructKey { Id = oneFifteeen }));
            Assert.Same(dependents[5], await context.FindAsync(typeof(IntStructKeyRequiredDependent), oneSixteen));
        }

        void Validate(
            IntStructKeyPrincipal[] principals,
            IntStructKeyRequiredDependent[] dependents,
            (int, int[])[] expectedPrincipalToDependents,
            (int, int)[] expectedDependentToPrincipals)
            => ValidateRequired(
                principals,
                dependents,
                expectedPrincipalToDependents,
                expectedDependentToPrincipals,
                p => ((IntStructKeyPrincipal)p).RequiredDependents.Select(d => (IIntRequiredDependent)d).ToList(),
                d => ((IntStructKeyRequiredDependent)d).Principal);
    }

    [ConditionalFact]
    public virtual async Task Can_insert_and_read_back_with_comparable_struct_key_and_required_dependents()
    {
        ComparableIntStructKeyPrincipal[] principals = null;
        ComparableIntStructKeyRequiredDependent[] dependents = null;
        await InsertRequiredGraph<ComparableIntStructKeyPrincipal, ComparableIntStructKeyRequiredDependent>();

        using (var context = CreateContext())
        {
            await RunQueries(context);

            Validate(
                principals,
                dependents,
                [(0, [0]), (1, [1]), (2, [2, 2, 2, 2]), (3, [])],
                [(0, 0), (1, 1), (2, 2), (3, 2), (4, 2), (5, 2)]);

            foreach (var principal in principals)
            {
                principal.Foo = "Mutant!";
            }

            dependents[5].Principal = principals[0];
            context.Remove(dependents[4]);
            dependents[3].PrincipalId = principals[0].Id;
            principals[1].RequiredDependents.Clear();

            context.Remove(dependents[0]);
            principals[0].RequiredDependents.Add(
                new ComparableIntStructKeyRequiredDependent { Id = new ComparableIntStructKey(dependents[0].Id.Id), });

            await context.SaveChangesAsync();
        }

        using (var context = CreateContext())
        {
            await RunQueries(context);

            Validate(
                principals,
                dependents,
                [(0, [0, 3, 5]), (1, []), (2, [2]), (3, [])],
                [(0, 0), (2, 2), (3, 0), (5, 0)]);
        }

        async Task RunQueries(DbContext context)
        {
            var twelve = 12;
            var thirteen = new ComparableIntStructKey { Id = 13 };

            principals =
            [
                await context.Set<ComparableIntStructKeyPrincipal>().Include(e => e.RequiredDependents)
                    .SingleAsync(e => e.Id.Equals(new ComparableIntStructKey { Id = 11 })),
                await context.Set<ComparableIntStructKeyPrincipal>().Include(e => e.RequiredDependents)
                    .SingleAsync(e => e.Id.Equals(new ComparableIntStructKey { Id = twelve })),
                await context.Set<ComparableIntStructKeyPrincipal>().Include(e => e.RequiredDependents)
                    .SingleAsync(e => e.Id.Equals(thirteen)),
                await context.Set<ComparableIntStructKeyPrincipal>().Include(e => e.RequiredDependents)
                    .SingleAsync(e => e.Id.Equals(new ComparableIntStructKey { Id = 14 }))
            ];

            var oneTwelve = 112;
            var oneThirteen = new ComparableIntStructKey { Id = 113 };
            var oneFifteeen = 115;
            var oneSixteen = new ComparableIntStructKey { Id = 116 };

            dependents =
            [
                await context.Set<ComparableIntStructKeyRequiredDependent>()
                    .FirstOrDefaultAsync(e => e.Id.Equals(new ComparableIntStructKey { Id = 111 })),
                await context.Set<ComparableIntStructKeyRequiredDependent>()
                    .FirstOrDefaultAsync(e => e.Id.Equals(new ComparableIntStructKey { Id = oneTwelve })),
                await context.Set<ComparableIntStructKeyRequiredDependent>().FirstOrDefaultAsync(e => e.Id.Equals(oneThirteen)),
                await context.Set<ComparableIntStructKeyRequiredDependent>()
                    .FirstOrDefaultAsync(e => e.Id.Equals(new ComparableIntStructKey { Id = 114 })),
                await context.Set<ComparableIntStructKeyRequiredDependent>()
                    .FirstOrDefaultAsync(e => e.Id.Equals(new ComparableIntStructKey { Id = oneFifteeen })),
                await context.Set<ComparableIntStructKeyRequiredDependent>().FirstOrDefaultAsync(e => e.Id.Equals(oneSixteen))
            ];

            Assert.Same(
                dependents[0], await context.Set<ComparableIntStructKeyRequiredDependent>().FindAsync(new ComparableIntStructKey { Id = 111 }));
            Assert.Same(
                dependents[1],
                await context.Set<ComparableIntStructKeyRequiredDependent>().FindAsync(new ComparableIntStructKey { Id = oneTwelve }));
            Assert.Same(dependents[2], await context.Set<ComparableIntStructKeyRequiredDependent>().FindAsync(oneThirteen));
            Assert.Same(
                dependents[3], await context.FindAsync(typeof(ComparableIntStructKeyRequiredDependent), new ComparableIntStructKey { Id = 114 }));
            Assert.Same(
                dependents[4],
                await context.FindAsync(typeof(ComparableIntStructKeyRequiredDependent), new ComparableIntStructKey { Id = oneFifteeen }));
            Assert.Same(dependents[5], await context.FindAsync(typeof(ComparableIntStructKeyRequiredDependent), oneSixteen));
        }

        void Validate(
            ComparableIntStructKeyPrincipal[] principals,
            ComparableIntStructKeyRequiredDependent[] dependents,
            (int, int[])[] expectedPrincipalToDependents,
            (int, int)[] expectedDependentToPrincipals)
            => ValidateRequired(
                principals,
                dependents,
                expectedPrincipalToDependents,
                expectedDependentToPrincipals,
                p => ((ComparableIntStructKeyPrincipal)p).RequiredDependents.Select(d => (IIntRequiredDependent)d).ToList(),
                d => ((ComparableIntStructKeyRequiredDependent)d).Principal);
    }

    [ConditionalFact]
    public virtual async Task Can_insert_and_read_back_with_generic_comparable_struct_key_and_required_dependents()
    {
        GenericComparableIntStructKeyPrincipal[] principals = null;
        GenericComparableIntStructKeyRequiredDependent[] dependents = null;
        await InsertRequiredGraph<GenericComparableIntStructKeyPrincipal, GenericComparableIntStructKeyRequiredDependent>();

        using (var context = CreateContext())
        {
            await RunQueries(context);

            Validate(
                principals,
                dependents,
                [(0, [0]), (1, [1]), (2, [2, 2, 2, 2]), (3, [])],
                [(0, 0), (1, 1), (2, 2), (3, 2), (4, 2), (5, 2)]);

            foreach (var principal in principals)
            {
                principal.Foo = "Mutant!";
            }

            dependents[5].Principal = principals[0];
            context.Remove(dependents[4]);
            dependents[3].PrincipalId = principals[0].Id;
            principals[1].RequiredDependents.Clear();

            context.Remove(dependents[0]);
            principals[0].RequiredDependents.Add(
                new GenericComparableIntStructKeyRequiredDependent { Id = new GenericComparableIntStructKey(dependents[0].Id.Id), });

            await context.SaveChangesAsync();
        }

        using (var context = CreateContext())
        {
            await RunQueries(context);

            Validate(
                principals,
                dependents,
                [(0, [0, 3, 5]), (1, []), (2, [2]), (3, [])],
                [(0, 0), (2, 2), (3, 0), (5, 0)]);
        }

        async Task RunQueries(DbContext context)
        {
            var twelve = 12;
            var thirteen = new GenericComparableIntStructKey { Id = 13 };

            principals =
            [
                await context.Set<GenericComparableIntStructKeyPrincipal>().Include(e => e.RequiredDependents)
                    .SingleAsync(e => e.Id.Equals(new GenericComparableIntStructKey { Id = 11 })),
                await context.Set<GenericComparableIntStructKeyPrincipal>().Include(e => e.RequiredDependents)
                    .SingleAsync(e => e.Id.Equals(new GenericComparableIntStructKey { Id = twelve })),
                await context.Set<GenericComparableIntStructKeyPrincipal>().Include(e => e.RequiredDependents)
                    .SingleAsync(e => e.Id.Equals(thirteen)),
                await context.Set<GenericComparableIntStructKeyPrincipal>().Include(e => e.RequiredDependents)
                    .SingleAsync(e => e.Id.Equals(new GenericComparableIntStructKey { Id = 14 }))
            ];

            var oneTwelve = 112;
            var oneThirteen = new GenericComparableIntStructKey { Id = 113 };
            var oneFifteeen = 115;
            var oneSixteen = new GenericComparableIntStructKey { Id = 116 };

            dependents =
            [
                await context.Set<GenericComparableIntStructKeyRequiredDependent>()
                    .FirstOrDefaultAsync(e => e.Id.Equals(new GenericComparableIntStructKey { Id = 111 })),
                await context.Set<GenericComparableIntStructKeyRequiredDependent>().FirstOrDefaultAsync(
                    e => e.Id.Equals(new GenericComparableIntStructKey { Id = oneTwelve })),
                await context.Set<GenericComparableIntStructKeyRequiredDependent>().FirstOrDefaultAsync(e => e.Id.Equals(oneThirteen)),
                await context.Set<GenericComparableIntStructKeyRequiredDependent>()
                    .FirstOrDefaultAsync(e => e.Id.Equals(new GenericComparableIntStructKey { Id = 114 })),
                await context.Set<GenericComparableIntStructKeyRequiredDependent>().FirstOrDefaultAsync(
                    e => e.Id.Equals(new GenericComparableIntStructKey { Id = oneFifteeen })),
                await context.Set<GenericComparableIntStructKeyRequiredDependent>().FirstOrDefaultAsync(e => e.Id.Equals(oneSixteen))
            ];

            Assert.Same(
                dependents[0],
                await context.Set<GenericComparableIntStructKeyRequiredDependent>().FindAsync(new GenericComparableIntStructKey { Id = 111 }));
            Assert.Same(
                dependents[1],
                await context.Set<GenericComparableIntStructKeyRequiredDependent>()
                    .FindAsync(new GenericComparableIntStructKey { Id = oneTwelve }));
            Assert.Same(dependents[2], await context.Set<GenericComparableIntStructKeyRequiredDependent>().FindAsync(oneThirteen));
            Assert.Same(
                dependents[3],
                await context.FindAsync(typeof(GenericComparableIntStructKeyRequiredDependent), new GenericComparableIntStructKey { Id = 114 }));
            Assert.Same(
                dependents[4],
                await context.FindAsync(
                    typeof(GenericComparableIntStructKeyRequiredDependent), new GenericComparableIntStructKey { Id = oneFifteeen }));
            Assert.Same(dependents[5], await context.FindAsync(typeof(GenericComparableIntStructKeyRequiredDependent), oneSixteen));
        }

        void Validate(
            GenericComparableIntStructKeyPrincipal[] principals,
            GenericComparableIntStructKeyRequiredDependent[] dependents,
            (int, int[])[] expectedPrincipalToDependents,
            (int, int)[] expectedDependentToPrincipals)
            => ValidateRequired(
                principals,
                dependents,
                expectedPrincipalToDependents,
                expectedDependentToPrincipals,
                p => ((GenericComparableIntStructKeyPrincipal)p).RequiredDependents.Select(d => (IIntRequiredDependent)d).ToList(),
                d => ((GenericComparableIntStructKeyRequiredDependent)d).Principal);
    }

    [ConditionalFact]
    public virtual async Task Can_insert_and_read_back_with_class_key_and_optional_dependents()
    {
        IntClassKeyPrincipal[] principals = null;
        IntClassKeyOptionalDependent[] dependents = null;
        await InsertOptionalGraph<IntClassKeyPrincipal, IntClassKeyOptionalDependent>();

        using (var context = CreateContext())
        {
            await RunQueries(context);

            Validate(
                principals,
                dependents,
                [(0, [0]), (1, [1]), (2, [2, 2, 2]), (3, [])],
                [(0, 0), (1, 1), (2, 2), (3, 2), (4, 2), (5, null)]);

            foreach (var principal in principals)
            {
                principal.Foo = "Mutant!";
            }

            dependents[5].Principal = principals[0];
            dependents[4].PrincipalId = null;
            dependents[3].PrincipalId = principals[0].Id;
            principals[1].OptionalDependents.Clear();

            context.Remove(dependents[0]);
            principals[0].OptionalDependents.Add(
                new IntClassKeyOptionalDependent { Id = new IntClassKey(dependents[0].Id.Id), });

            await context.SaveChangesAsync();
        }

        using (var context = CreateContext())
        {
            await RunQueries(context);

            Validate(
                principals,
                dependents,
                [(0, [0, 3, 5]), (1, []), (2, [2]), (3, [])],
                [(0, 0), (1, null), (2, 2), (3, 0), (4, null), (5, 0)]);
        }

        async Task RunQueries(DbContext context)
        {
            var two = 2;
            var three = new IntClassKey(3);

            principals =
            [
                await context.Set<IntClassKeyPrincipal>().Include(e => e.OptionalDependents)
                    .SingleAsync(e => e.Id.Equals(new IntClassKey(1))),
                await context.Set<IntClassKeyPrincipal>().Include(e => e.OptionalDependents)
                    .SingleAsync(e => e.Id.Equals(new IntClassKey(two))),
                await context.Set<IntClassKeyPrincipal>().Include(e => e.OptionalDependents).SingleAsync(e => e.Id.Equals(three)),
                await context.Set<IntClassKeyPrincipal>().Include(e => e.OptionalDependents)
                    .SingleAsync(e => e.Id.Equals(new IntClassKey(4)))
            ];

            var oneOhTwo = 102;
            var oneOhThree = new IntClassKey(103);
            var oneOhFive = 105;
            var oneOhSix = new IntClassKey(106);

            dependents =
            [
                await context.Set<IntClassKeyOptionalDependent>().SingleAsync(e => e.Id.Equals(new IntClassKey(101))),
                await context.Set<IntClassKeyOptionalDependent>().SingleAsync(e => e.Id.Equals(new IntClassKey(oneOhTwo))),
                await context.Set<IntClassKeyOptionalDependent>().SingleAsync(e => e.Id.Equals(oneOhThree)),
                await context.Set<IntClassKeyOptionalDependent>().SingleAsync(e => e.Id == new IntClassKey(104)),
                await context.Set<IntClassKeyOptionalDependent>().SingleAsync(e => e.Id == new IntClassKey(oneOhFive)),
                await context.Set<IntClassKeyOptionalDependent>().SingleAsync(e => e.Id == oneOhSix)
            ];

            Assert.Same(dependents[0], await context.Set<IntClassKeyOptionalDependent>().FindAsync(new IntClassKey(101)));
            Assert.Same(dependents[1], await context.Set<IntClassKeyOptionalDependent>().FindAsync(new IntClassKey(oneOhTwo)));
            Assert.Same(dependents[2], await context.Set<IntClassKeyOptionalDependent>().FindAsync(oneOhThree));
            Assert.Same(dependents[3], await context.FindAsync<IntClassKeyOptionalDependent>(new IntClassKey(104)));
            Assert.Same(dependents[4], await context.FindAsync<IntClassKeyOptionalDependent>(new IntClassKey(oneOhFive)));
            Assert.Same(dependents[5], await context.FindAsync<IntClassKeyOptionalDependent>(oneOhSix));
        }

        void Validate(
            IntClassKeyPrincipal[] principals,
            IntClassKeyOptionalDependent[] dependents,
            (int, int[])[] expectedPrincipalToDependents,
            (int, int?)[] expectedDependentToPrincipals)
            => ValidateOptional(
                principals,
                dependents,
                expectedPrincipalToDependents,
                expectedDependentToPrincipals,
                p => ((IntClassKeyPrincipal)p).OptionalDependents.Select(d => (IIntOptionalDependent)d).ToList(),
                d => ((IntClassKeyOptionalDependent)d).Principal);
    }

    [ConditionalFact]
    public virtual async Task Can_insert_and_read_back_with_enumerable_class_key_and_optional_dependents()
    {
        EnumerableClassKeyPrincipal[] principals = null;
        EnumerableClassKeyOptionalDependent[] dependents = null;
        await InsertOptionalGraph<EnumerableClassKeyPrincipal, EnumerableClassKeyOptionalDependent>();

        using (var context = CreateContext())
        {
            await RunQueries(context);

            Validate(
                principals,
                dependents,
                [(0, [0]), (1, [1]), (2, [2, 2, 2]), (3, [])],
                [(0, 0), (1, 1), (2, 2), (3, 2), (4, 2), (5, null)]);

            foreach (var principal in principals)
            {
                principal.Foo = "Mutant!";
            }

            dependents[5].Principal = principals[0];
            dependents[4].PrincipalId = null;
            dependents[3].PrincipalId = principals[0].Id;
            principals[1].OptionalDependents.Clear();

            context.Remove(dependents[0]);
            principals[0].OptionalDependents.Add(
                new EnumerableClassKeyOptionalDependent { Id = new EnumerableClassKey(dependents[0].Id.Id), });

            await context.SaveChangesAsync();
        }

        using (var context = CreateContext())
        {
            await RunQueries(context);

            Validate(
                principals,
                dependents,
                [(0, [0, 3, 5]), (1, []), (2, [2]), (3, [])],
                [(0, 0), (1, null), (2, 2), (3, 0), (4, null), (5, 0)]);
        }

        async Task RunQueries(DbContext context)
        {
            var two = 2;
            var three = new EnumerableClassKey(3);

            principals =
            [
                await context.Set<EnumerableClassKeyPrincipal>().Include(e => e.OptionalDependents)
                    .SingleAsync(e => e.Id.Equals(new EnumerableClassKey(1))),
                await context.Set<EnumerableClassKeyPrincipal>().Include(e => e.OptionalDependents)
                    .SingleAsync(e => e.Id.Equals(new EnumerableClassKey(two))),
                await context.Set<EnumerableClassKeyPrincipal>().Include(e => e.OptionalDependents).SingleAsync(e => e.Id.Equals(three)),
                await context.Set<EnumerableClassKeyPrincipal>().Include(e => e.OptionalDependents)
                    .SingleAsync(e => e.Id.Equals(new EnumerableClassKey(4)))
            ];

            var oneOhTwo = 102;
            var oneOhThree = new EnumerableClassKey(103);
            var oneOhFive = 105;
            var oneOhSix = new EnumerableClassKey(106);

            dependents =
            [
                await context.Set<EnumerableClassKeyOptionalDependent>().SingleAsync(e => e.Id.Equals(new EnumerableClassKey(101))),
                await context.Set<EnumerableClassKeyOptionalDependent>().SingleAsync(e => e.Id.Equals(new EnumerableClassKey(oneOhTwo))),
                await context.Set<EnumerableClassKeyOptionalDependent>().SingleAsync(e => e.Id.Equals(oneOhThree)),
                await context.Set<EnumerableClassKeyOptionalDependent>().SingleAsync(e => e.Id == new EnumerableClassKey(104)),
                await context.Set<EnumerableClassKeyOptionalDependent>().SingleAsync(e => e.Id == new EnumerableClassKey(oneOhFive)),
                await context.Set<EnumerableClassKeyOptionalDependent>().SingleAsync(e => e.Id == oneOhSix)
            ];

            Assert.Same(dependents[0], await context.Set<EnumerableClassKeyOptionalDependent>().FindAsync(new EnumerableClassKey(101)));
            Assert.Same(dependents[1], await context.Set<EnumerableClassKeyOptionalDependent>().FindAsync(new EnumerableClassKey(oneOhTwo)));
            Assert.Same(dependents[2], await context.Set<EnumerableClassKeyOptionalDependent>().FindAsync(oneOhThree));
            Assert.Same(dependents[3], await context.FindAsync<EnumerableClassKeyOptionalDependent>(new EnumerableClassKey(104)));
            Assert.Same(dependents[4], await context.FindAsync<EnumerableClassKeyOptionalDependent>(new EnumerableClassKey(oneOhFive)));
            Assert.Same(dependents[5], await context.FindAsync<EnumerableClassKeyOptionalDependent>(oneOhSix));
        }

        void Validate(
            EnumerableClassKeyPrincipal[] principals,
            EnumerableClassKeyOptionalDependent[] dependents,
            (int, int[])[] expectedPrincipalToDependents,
            (int, int?)[] expectedDependentToPrincipals)
            => ValidateOptional(
                principals,
                dependents,
                expectedPrincipalToDependents,
                expectedDependentToPrincipals,
                p => ((EnumerableClassKeyPrincipal)p).OptionalDependents.Select(d => (IIntOptionalDependent)d).ToList(),
                d => ((EnumerableClassKeyOptionalDependent)d).Principal);
    }

    [ConditionalFact]
    public virtual async Task Can_insert_and_read_back_with_bare_class_key_and_optional_dependents()
    {
        BareIntClassKeyPrincipal[] principals = null;
        BareIntClassKeyOptionalDependent[] dependents = null;
        await InsertOptionalGraph<BareIntClassKeyPrincipal, BareIntClassKeyOptionalDependent>();

        using (var context = CreateContext())
        {
            await RunQueries(context);

            Validate(
                principals,
                dependents,
                [(0, [0]), (1, [1]), (2, [2, 2, 2]), (3, [])],
                [(0, 0), (1, 1), (2, 2), (3, 2), (4, 2), (5, null)]);

            foreach (var principal in principals)
            {
                principal.Foo = "Mutant!";
            }

            dependents[5].Principal = principals[0];
            dependents[4].PrincipalId = null;
            dependents[3].PrincipalId = principals[0].Id;
            principals[1].OptionalDependents.Clear();

            context.Remove(dependents[0]);
            principals[0].OptionalDependents.Add(
                new BareIntClassKeyOptionalDependent { Id = new BareIntClassKey(dependents[0].Id.Id), });

            await context.SaveChangesAsync();
        }

        using (var context = CreateContext())
        {
            await RunQueries(context);

            Validate(
                principals,
                dependents,
                [(0, [0, 3, 5]), (1, []), (2, [2]), (3, [])],
                [(0, 0), (1, null), (2, 2), (3, 0), (4, null), (5, 0)]);
        }

        async Task RunQueries(DbContext context)
        {
            var two = 2;
            var three = new BareIntClassKey(3);

            principals =
            [
                await context.Set<BareIntClassKeyPrincipal>().Include(e => e.OptionalDependents)
                    .SingleAsync(e => e.Id.Equals(new BareIntClassKey(1))),
                await context.Set<BareIntClassKeyPrincipal>().Include(e => e.OptionalDependents)
                    .SingleAsync(e => e.Id.Equals(new BareIntClassKey(two))),
                await context.Set<BareIntClassKeyPrincipal>().Include(e => e.OptionalDependents).SingleAsync(e => e.Id.Equals(three)),
                await context.Set<BareIntClassKeyPrincipal>().Include(e => e.OptionalDependents)
                    .SingleAsync(e => e.Id.Equals(new BareIntClassKey(4)))
            ];

            var oneOhTwo = 102;
            var oneOhThree = new BareIntClassKey(103);
            var oneOhFive = 105;
            var oneOhSix = new BareIntClassKey(106);

            dependents =
            [
                await context.Set<BareIntClassKeyOptionalDependent>().SingleAsync(e => e.Id.Equals(new BareIntClassKey(101))),
                await context.Set<BareIntClassKeyOptionalDependent>().SingleAsync(e => e.Id.Equals(new BareIntClassKey(oneOhTwo))),
                await context.Set<BareIntClassKeyOptionalDependent>().SingleAsync(e => e.Id.Equals(oneOhThree)),
                await context.Set<BareIntClassKeyOptionalDependent>().SingleAsync(e => e.Id == new BareIntClassKey(104)),
                await context.Set<BareIntClassKeyOptionalDependent>().SingleAsync(e => e.Id == new BareIntClassKey(oneOhFive)),
                await context.Set<BareIntClassKeyOptionalDependent>().SingleAsync(e => e.Id == oneOhSix)
            ];

            Assert.Same(dependents[0], await context.Set<BareIntClassKeyOptionalDependent>().FindAsync(new BareIntClassKey(101)));
            Assert.Same(dependents[1], await context.Set<BareIntClassKeyOptionalDependent>().FindAsync(new BareIntClassKey(oneOhTwo)));
            Assert.Same(dependents[2], await context.Set<BareIntClassKeyOptionalDependent>().FindAsync(oneOhThree));
            Assert.Same(dependents[3], await context.FindAsync<BareIntClassKeyOptionalDependent>(new BareIntClassKey(104)));
            Assert.Same(dependents[4], await context.FindAsync<BareIntClassKeyOptionalDependent>(new BareIntClassKey(oneOhFive)));
            Assert.Same(dependents[5], await context.FindAsync<BareIntClassKeyOptionalDependent>(oneOhSix));
        }

        void Validate(
            BareIntClassKeyPrincipal[] principals,
            BareIntClassKeyOptionalDependent[] dependents,
            (int, int[])[] expectedPrincipalToDependents,
            (int, int?)[] expectedDependentToPrincipals)
            => ValidateOptional(
                principals,
                dependents,
                expectedPrincipalToDependents,
                expectedDependentToPrincipals,
                p => ((BareIntClassKeyPrincipal)p).OptionalDependents.Select(d => (IIntOptionalDependent)d).ToList(),
                d => ((BareIntClassKeyOptionalDependent)d).Principal);
    }

    [ConditionalFact]
    public virtual async Task Can_insert_and_read_back_with_comparable_class_key_and_optional_dependents()
    {
        ComparableIntClassKeyPrincipal[] principals = null;
        ComparableIntClassKeyOptionalDependent[] dependents = null;
        await InsertOptionalGraph<ComparableIntClassKeyPrincipal, ComparableIntClassKeyOptionalDependent>();

        using (var context = CreateContext())
        {
            await RunQueries(context);

            Validate(
                principals,
                dependents,
                [(0, [0]), (1, [1]), (2, [2, 2, 2]), (3, [])],
                [(0, 0), (1, 1), (2, 2), (3, 2), (4, 2), (5, null)]);

            foreach (var principal in principals)
            {
                principal.Foo = "Mutant!";
            }

            dependents[5].Principal = principals[0];
            dependents[4].PrincipalId = null;
            dependents[3].PrincipalId = principals[0].Id;
            principals[1].OptionalDependents.Clear();

            context.Remove(dependents[0]);
            principals[0].OptionalDependents.Add(
                new ComparableIntClassKeyOptionalDependent { Id = new ComparableIntClassKey(dependents[0].Id.Id), });

            await context.SaveChangesAsync();
        }

        using (var context = CreateContext())
        {
            await RunQueries(context);

            Validate(
                principals,
                dependents,
                [(0, [0, 3, 5]), (1, []), (2, [2]), (3, [])],
                [(0, 0), (1, null), (2, 2), (3, 0), (4, null), (5, 0)]);
        }

        async Task RunQueries(DbContext context)
        {
            var two = 2;
            var three = new ComparableIntClassKey(3);

            principals =
            [
                await context.Set<ComparableIntClassKeyPrincipal>().Include(e => e.OptionalDependents)
                    .SingleAsync(e => e.Id.Equals(new ComparableIntClassKey(1))),
                await context.Set<ComparableIntClassKeyPrincipal>().Include(e => e.OptionalDependents)
                    .SingleAsync(e => e.Id.Equals(new ComparableIntClassKey(two))),
                await context.Set<ComparableIntClassKeyPrincipal>().Include(e => e.OptionalDependents).SingleAsync(e => e.Id.Equals(three)),
                await context.Set<ComparableIntClassKeyPrincipal>().Include(e => e.OptionalDependents)
                    .SingleAsync(e => e.Id.Equals(new ComparableIntClassKey(4)))
            ];

            var oneOhTwo = 102;
            var oneOhThree = new ComparableIntClassKey(103);
            var oneOhFive = 105;
            var oneOhSix = new ComparableIntClassKey(106);

            dependents =
            [
                await context.Set<ComparableIntClassKeyOptionalDependent>()
                    .SingleAsync(e => e.Id.Equals(new ComparableIntClassKey(101))),
                await context.Set<ComparableIntClassKeyOptionalDependent>()
                    .SingleAsync(e => e.Id.Equals(new ComparableIntClassKey(oneOhTwo))),
                await context.Set<ComparableIntClassKeyOptionalDependent>().SingleAsync(e => e.Id.Equals(oneOhThree)),
                await context.Set<ComparableIntClassKeyOptionalDependent>().SingleAsync(e => e.Id == new ComparableIntClassKey(104)),
                await context.Set<ComparableIntClassKeyOptionalDependent>()
                    .SingleAsync(e => e.Id == new ComparableIntClassKey(oneOhFive)),
                await context.Set<ComparableIntClassKeyOptionalDependent>().SingleAsync(e => e.Id == oneOhSix)
            ];

            Assert.Same(
                dependents[0], await context.Set<ComparableIntClassKeyOptionalDependent>().FindAsync(new ComparableIntClassKey(101)));
            Assert.Same(
                dependents[1], await context.Set<ComparableIntClassKeyOptionalDependent>().FindAsync(new ComparableIntClassKey(oneOhTwo)));
            Assert.Same(dependents[2], await context.Set<ComparableIntClassKeyOptionalDependent>().FindAsync(oneOhThree));
            Assert.Same(dependents[3], await context.FindAsync<ComparableIntClassKeyOptionalDependent>(new ComparableIntClassKey(104)));
            Assert.Same(
                dependents[4], await context.FindAsync<ComparableIntClassKeyOptionalDependent>(new ComparableIntClassKey(oneOhFive)));
            Assert.Same(dependents[5], await context.FindAsync<ComparableIntClassKeyOptionalDependent>(oneOhSix));
        }

        void Validate(
            ComparableIntClassKeyPrincipal[] principals,
            ComparableIntClassKeyOptionalDependent[] dependents,
            (int, int[])[] expectedPrincipalToDependents,
            (int, int?)[] expectedDependentToPrincipals)
            => ValidateOptional(
                principals,
                dependents,
                expectedPrincipalToDependents,
                expectedDependentToPrincipals,
                p => ((ComparableIntClassKeyPrincipal)p).OptionalDependents.Select(d => (IIntOptionalDependent)d).ToList(),
                d => ((ComparableIntClassKeyOptionalDependent)d).Principal);
    }

    [ConditionalFact]
    public virtual async Task Can_insert_and_read_back_with_struct_binary_key_and_optional_dependents()
    {
        BytesStructKeyPrincipal[] principals = null;
        BytesStructKeyOptionalDependent[] dependents = null;
        await InsertOptionalBytesGraph<BytesStructKeyPrincipal, BytesStructKeyOptionalDependent>();

        using (var context = CreateContext())
        {
            await RunQueries(context);

            Validate(
                principals,
                dependents,
                [(0, [0]), (1, [1]), (2, [2, 2, 2]), (3, [])],
                [(0, 0), (1, 1), (2, 2), (3, 2), (4, 2), (5, null)]);

            foreach (var principal in principals)
            {
                principal.Foo = "Mutant!";
            }

            dependents[5].Principal = principals[0];
            dependents[4].PrincipalId = null;
            dependents[3].PrincipalId = principals[0].Id;
            principals[1].OptionalDependents.Clear();

            context.Remove(dependents[0]);
            principals[0].OptionalDependents.Add(
                new BytesStructKeyOptionalDependent { Id = new BytesStructKey(dependents[0].Id.Id), });

            await context.SaveChangesAsync();
        }

        using (var context = CreateContext())
        {
            await RunQueries(context);

            Validate(
                principals,
                dependents,
                [(0, [0, 3, 5]), (1, []), (2, [2]), (3, [])],
                [(0, 0), (1, null), (2, 2), (3, 0), (4, null), (5, 0)]);
        }

        async Task RunQueries(DbContext context)
        {
            var two = new byte[] { 2, 2 };
            var three = new BytesStructKey { Id = [3, 3, 3] };

            principals =
            [
                await context.Set<BytesStructKeyPrincipal>().Include(e => e.OptionalDependents)
                    .SingleAsync(e => e.Id.Equals(new BytesStructKey { Id = new byte[] { 1 } })),
                await context.Set<BytesStructKeyPrincipal>().Include(e => e.OptionalDependents)
                    .SingleAsync(e => e.Id.Equals(new BytesStructKey(two))),
                (await context.Set<BytesStructKeyPrincipal>().Include(e => e.OptionalDependents).Where(e => e.Id.Equals(three)).ToListAsync())
                    .Single(),
                await context.Set<BytesStructKeyPrincipal>().Include(e => e.OptionalDependents).SingleAsync(
                    e => e.Id.Equals(new BytesStructKey { Id = new byte[] { 4, 4, 4, 4 } }))
            ];

            var oneOhTwo = new byte[] { 102 };
            var oneOhThree = new BytesStructKey { Id = [103] };
            var oneOhFive = new byte[] { 105 };
            var oneOhSix = new BytesStructKey { Id = [106] };

            dependents =
            [
                await context.Set<BytesStructKeyOptionalDependent>()
                    .SingleAsync(e => e.Id.Equals(new BytesStructKey { Id = new byte[] { 101 } })),
                await context.Set<BytesStructKeyOptionalDependent>().SingleAsync(e => e.Id.Equals(new BytesStructKey(oneOhTwo))),
                await context.Set<BytesStructKeyOptionalDependent>().SingleAsync(e => e.Id.Equals(oneOhThree)),
                await context.Set<BytesStructKeyOptionalDependent>()
                    .SingleAsync(e => e.Id.Equals(new BytesStructKey { Id = new byte[] { 104 } })),
                await context.Set<BytesStructKeyOptionalDependent>().SingleAsync(e => e.Id.Equals(new BytesStructKey(oneOhFive))),
                await context.Set<BytesStructKeyOptionalDependent>().SingleAsync(e => e.Id.Equals(oneOhSix))
            ];

            Assert.Same(
                dependents[0], await context.Set<BytesStructKeyOptionalDependent>().FindAsync(new BytesStructKey { Id = [101] }));
            Assert.Same(dependents[1], await context.Set<BytesStructKeyOptionalDependent>().FindAsync(new BytesStructKey(oneOhTwo)));
            Assert.Same(dependents[2], await context.Set<BytesStructKeyOptionalDependent>().FindAsync(oneOhThree));
            Assert.Same(
                dependents[3], await context.FindAsync(typeof(BytesStructKeyOptionalDependent), new BytesStructKey { Id = [104] }));
            Assert.Same(dependents[4], await context.FindAsync(typeof(BytesStructKeyOptionalDependent), new BytesStructKey(oneOhFive)));
            Assert.Same(dependents[5], await context.FindAsync(typeof(BytesStructKeyOptionalDependent), oneOhSix));
        }

        void Validate(
            BytesStructKeyPrincipal[] principals,
            BytesStructKeyOptionalDependent[] dependents,
            (int, int[])[] expectedPrincipalToDependents,
            (int, int?)[] expectedDependentToPrincipals)
            => ValidateOptionalBytes(
                principals,
                dependents,
                expectedPrincipalToDependents,
                expectedDependentToPrincipals,
                p => ((BytesStructKeyPrincipal)p).OptionalDependents.Select(d => (IBytesOptionalDependent)d).ToList(),
                d => ((BytesStructKeyOptionalDependent)d).Principal);
    }

    [ConditionalFact]
    public virtual async Task Can_insert_and_read_back_with_structural_struct_binary_key_and_optional_dependents()
    {
        StructuralComparableBytesStructKeyPrincipal[] principals = null;
        StructuralComparableBytesStructKeyOptionalDependent[] dependents = null;
        await InsertOptionalBytesGraph<StructuralComparableBytesStructKeyPrincipal, StructuralComparableBytesStructKeyOptionalDependent>();

        using (var context = CreateContext())
        {
            await RunQueries(context);

            Validate(
                principals,
                dependents,
                [(0, [0]), (1, [1]), (2, [2, 2, 2]), (3, [])],
                [(0, 0), (1, 1), (2, 2), (3, 2), (4, 2), (5, null)]);

            foreach (var principal in principals)
            {
                principal.Foo = "Mutant!";
            }

            dependents[5].Principal = principals[0];
            dependents[4].PrincipalId = null;
            dependents[3].PrincipalId = principals[0].Id;
            principals[1].OptionalDependents.Clear();

            context.Remove(dependents[0]);
            principals[0].OptionalDependents.Add(
                new StructuralComparableBytesStructKeyOptionalDependent
                {
                    Id = new StructuralComparableBytesStructKey(dependents[0].Id.Id),
                });

            await context.SaveChangesAsync();
        }

        using (var context = CreateContext())
        {
            await RunQueries(context);

            Validate(
                principals,
                dependents,
                [(0, [0, 3, 5]), (1, []), (2, [2]), (3, [])],
                [(0, 0), (1, null), (2, 2), (3, 0), (4, null), (5, 0)]);
        }

        async Task RunQueries(DbContext context)
        {
            var two = new byte[] { 2, 2 };
            var three = new StructuralComparableBytesStructKey { Id = [3, 3, 3] };

            principals =
            [
                await context.Set<StructuralComparableBytesStructKeyPrincipal>().Include(e => e.OptionalDependents).SingleAsync(
                    e => e.Id.Equals(new StructuralComparableBytesStructKey { Id = new byte[] { 1 } })),
                await context.Set<StructuralComparableBytesStructKeyPrincipal>().Include(e => e.OptionalDependents)
                    .SingleAsync(e => e.Id.Equals(new StructuralComparableBytesStructKey(two))),
                await context.Set<StructuralComparableBytesStructKeyPrincipal>().Include(e => e.OptionalDependents)
                    .SingleAsync(e => e.Id.Equals(three)),
                await context.Set<StructuralComparableBytesStructKeyPrincipal>().Include(e => e.OptionalDependents).SingleAsync(
                    e => e.Id.Equals(new StructuralComparableBytesStructKey { Id = new byte[] { 4, 4, 4, 4 } }))
            ];

            var oneOhTwo = new byte[] { 102 };
            var oneOhThree = new StructuralComparableBytesStructKey { Id = [103] };
            var oneOhFive = new byte[] { 105 };
            var oneOhSix = new StructuralComparableBytesStructKey { Id = [106] };

            dependents =
            [
                await context.Set<StructuralComparableBytesStructKeyOptionalDependent>().SingleAsync(
                    e => e.Id.Equals(new StructuralComparableBytesStructKey { Id = new byte[] { 101 } })),
                await context.Set<StructuralComparableBytesStructKeyOptionalDependent>().SingleAsync(
                    e => e.Id.Equals(new StructuralComparableBytesStructKey(oneOhTwo))),
                await context.Set<StructuralComparableBytesStructKeyOptionalDependent>().SingleAsync(e => e.Id.Equals(oneOhThree)),
                await context.Set<StructuralComparableBytesStructKeyOptionalDependent>().SingleAsync(
                    e => e.Id.Equals(new StructuralComparableBytesStructKey { Id = new byte[] { 104 } })),
                await context.Set<StructuralComparableBytesStructKeyOptionalDependent>().SingleAsync(
                    e => e.Id.Equals(new StructuralComparableBytesStructKey(oneOhFive))),
                await context.Set<StructuralComparableBytesStructKeyOptionalDependent>().SingleAsync(e => e.Id.Equals(oneOhSix))
            ];

            Assert.Same(
                dependents[0],
                await context.Set<StructuralComparableBytesStructKeyOptionalDependent>()
                    .FindAsync(new StructuralComparableBytesStructKey { Id = [101] }));
            Assert.Same(
                dependents[1],
                await context.Set<StructuralComparableBytesStructKeyOptionalDependent>()
                    .FindAsync(new StructuralComparableBytesStructKey(oneOhTwo)));
            Assert.Same(dependents[2], await context.Set<StructuralComparableBytesStructKeyOptionalDependent>().FindAsync(oneOhThree));
            Assert.Same(
                dependents[3],
                await context.FindAsync(
                    typeof(StructuralComparableBytesStructKeyOptionalDependent),
                    new StructuralComparableBytesStructKey { Id = [104] }));
            Assert.Same(
                dependents[4],
                await context.FindAsync(
                    typeof(StructuralComparableBytesStructKeyOptionalDependent),
                    new StructuralComparableBytesStructKey(oneOhFive)));
            Assert.Same(dependents[5], await context.FindAsync(typeof(StructuralComparableBytesStructKeyOptionalDependent), oneOhSix));
        }

        void Validate(
            StructuralComparableBytesStructKeyPrincipal[] principals,
            StructuralComparableBytesStructKeyOptionalDependent[] dependents,
            (int, int[])[] expectedPrincipalToDependents,
            (int, int?)[] expectedDependentToPrincipals)
            => ValidateOptionalBytes(
                principals,
                dependents,
                expectedPrincipalToDependents,
                expectedDependentToPrincipals,
                p => ((StructuralComparableBytesStructKeyPrincipal)p).OptionalDependents.Select(d => (IBytesOptionalDependent)d)
                    .ToList(),
                d => ((StructuralComparableBytesStructKeyOptionalDependent)d).Principal);
    }

    [ConditionalFact]
    public virtual async Task Can_insert_and_read_back_with_comparable_struct_binary_key_and_optional_dependents()
    {
        ComparableBytesStructKeyPrincipal[] principals = null;
        ComparableBytesStructKeyOptionalDependent[] dependents = null;
        await InsertOptionalBytesGraph<ComparableBytesStructKeyPrincipal, ComparableBytesStructKeyOptionalDependent>();

        using (var context = CreateContext())
        {
            await RunQueries(context);

            Validate(
                principals,
                dependents,
                [(0, [0]), (1, [1]), (2, [2, 2, 2]), (3, [])],
                [(0, 0), (1, 1), (2, 2), (3, 2), (4, 2), (5, null)]);

            foreach (var principal in principals)
            {
                principal.Foo = "Mutant!";
            }

            dependents[5].Principal = principals[0];
            dependents[4].PrincipalId = null;
            dependents[3].PrincipalId = principals[0].Id;
            principals[1].OptionalDependents.Clear();

            context.Remove(dependents[0]);
            principals[0].OptionalDependents.Add(
                new ComparableBytesStructKeyOptionalDependent { Id = new ComparableBytesStructKey(dependents[0].Id.Id), });

            await context.SaveChangesAsync();
        }

        using (var context = CreateContext())
        {
            await RunQueries(context);

            Validate(
                principals,
                dependents,
                [(0, [0, 3, 5]), (1, []), (2, [2]), (3, [])],
                [(0, 0), (1, null), (2, 2), (3, 0), (4, null), (5, 0)]);
        }

        async Task RunQueries(DbContext context)
        {
            var two = new byte[] { 2, 2 };
            var three = new ComparableBytesStructKey { Id = [3, 3, 3] };

            principals =
            [
                await context.Set<ComparableBytesStructKeyPrincipal>().Include(e => e.OptionalDependents).SingleAsync(
                    e => e.Id.Equals(new ComparableBytesStructKey { Id = new byte[] { 1 } })),
                await context.Set<ComparableBytesStructKeyPrincipal>().Include(e => e.OptionalDependents)
                    .SingleAsync(e => e.Id.Equals(new ComparableBytesStructKey(two))),
                (await context.Set<ComparableBytesStructKeyPrincipal>().Include(e => e.OptionalDependents).ToListAsync())
                    .Where(e => e.Id.Equals(three)).ToList().Single(),
                await context.Set<ComparableBytesStructKeyPrincipal>().Include(e => e.OptionalDependents).SingleAsync(
                    e => e.Id.Equals(new ComparableBytesStructKey { Id = new byte[] { 4, 4, 4, 4 } }))
            ];

            var oneOhTwo = new byte[] { 102 };
            var oneOhThree = new ComparableBytesStructKey { Id = [103] };
            var oneOhFive = new byte[] { 105 };
            var oneOhSix = new ComparableBytesStructKey { Id = [106] };

            dependents =
            [
                await context.Set<ComparableBytesStructKeyOptionalDependent>().SingleAsync(
                    e => e.Id.Equals(new ComparableBytesStructKey { Id = new byte[] { 101 } })),
                await context.Set<ComparableBytesStructKeyOptionalDependent>()
                    .SingleAsync(e => e.Id.Equals(new ComparableBytesStructKey(oneOhTwo))),
                await context.Set<ComparableBytesStructKeyOptionalDependent>().SingleAsync(e => e.Id.Equals(oneOhThree)),
                await context.Set<ComparableBytesStructKeyOptionalDependent>().SingleAsync(
                    e => e.Id.Equals(new ComparableBytesStructKey { Id = new byte[] { 104 } })),
                await context.Set<ComparableBytesStructKeyOptionalDependent>()
                    .SingleAsync(e => e.Id.Equals(new ComparableBytesStructKey(oneOhFive))),
                await context.Set<ComparableBytesStructKeyOptionalDependent>().SingleAsync(e => e.Id.Equals(oneOhSix))
            ];

            Assert.Same(
                dependents[0],
                await context.Set<ComparableBytesStructKeyOptionalDependent>()
                    .FindAsync(new ComparableBytesStructKey { Id = [101] }));
            Assert.Same(
                dependents[1],
                await context.Set<ComparableBytesStructKeyOptionalDependent>().FindAsync(new ComparableBytesStructKey(oneOhTwo)));
            Assert.Same(dependents[2], await context.Set<ComparableBytesStructKeyOptionalDependent>().FindAsync(oneOhThree));
            Assert.Same(
                dependents[3],
                await context.FindAsync(
                    typeof(ComparableBytesStructKeyOptionalDependent), new ComparableBytesStructKey { Id = [104] }));
            Assert.Same(
                dependents[4],
                await context.FindAsync(typeof(ComparableBytesStructKeyOptionalDependent), new ComparableBytesStructKey(oneOhFive)));
            Assert.Same(dependents[5], await context.FindAsync(typeof(ComparableBytesStructKeyOptionalDependent), oneOhSix));
        }

        void Validate(
            ComparableBytesStructKeyPrincipal[] principals,
            ComparableBytesStructKeyOptionalDependent[] dependents,
            (int, int[])[] expectedPrincipalToDependents,
            (int, int?)[] expectedDependentToPrincipals)
            => ValidateOptionalBytes(
                principals,
                dependents,
                expectedPrincipalToDependents,
                expectedDependentToPrincipals,
                p => ((ComparableBytesStructKeyPrincipal)p).OptionalDependents.Select(d => (IBytesOptionalDependent)d).ToList(),
                d => ((ComparableBytesStructKeyOptionalDependent)d).Principal);
    }

    [ConditionalFact]
    public virtual async Task Can_insert_and_read_back_with_generic_comparable_struct_binary_key_and_optional_dependents()
    {
        GenericComparableBytesStructKeyPrincipal[] principals = null;
        GenericComparableBytesStructKeyOptionalDependent[] dependents = null;
        await InsertOptionalBytesGraph<GenericComparableBytesStructKeyPrincipal, GenericComparableBytesStructKeyOptionalDependent>();

        using (var context = CreateContext())
        {
            await RunQueries(context);

            Validate(
                principals,
                dependents,
                [(0, [0]), (1, [1]), (2, [2, 2, 2]), (3, [])],
                [(0, 0), (1, 1), (2, 2), (3, 2), (4, 2), (5, null)]);

            foreach (var principal in principals)
            {
                principal.Foo = "Mutant!";
            }

            dependents[5].Principal = principals[0];
            dependents[4].PrincipalId = null;
            dependents[3].PrincipalId = principals[0].Id;
            principals[1].OptionalDependents.Clear();

            context.Remove(dependents[0]);
            principals[0].OptionalDependents.Add(
                new GenericComparableBytesStructKeyOptionalDependent { Id = new GenericComparableBytesStructKey(dependents[0].Id.Id), });

            await context.SaveChangesAsync();
        }

        using (var context = CreateContext())
        {
            await RunQueries(context);

            Validate(
                principals,
                dependents,
                [(0, [0, 3, 5]), (1, []), (2, [2]), (3, [])],
                [(0, 0), (1, null), (2, 2), (3, 0), (4, null), (5, 0)]);
        }

        async Task RunQueries(DbContext context)
        {
            var two = new byte[] { 2, 2 };
            var three = new GenericComparableBytesStructKey { Id = [3, 3, 3] };

            principals =
            [
                await context.Set<GenericComparableBytesStructKeyPrincipal>().Include(e => e.OptionalDependents).SingleAsync(
                    e => e.Id.Equals(new GenericComparableBytesStructKey { Id = new byte[] { 1 } })),
                await context.Set<GenericComparableBytesStructKeyPrincipal>().Include(e => e.OptionalDependents)
                    .SingleAsync(e => e.Id.Equals(new GenericComparableBytesStructKey(two))),
                await context.Set<GenericComparableBytesStructKeyPrincipal>().Include(e => e.OptionalDependents)
                    .SingleAsync(e => e.Id.Equals(three)),
                await context.Set<GenericComparableBytesStructKeyPrincipal>().Include(e => e.OptionalDependents).SingleAsync(
                    e => e.Id.Equals(new GenericComparableBytesStructKey { Id = new byte[] { 4, 4, 4, 4 } }))
            ];

            var oneOhTwo = new byte[] { 102 };
            var oneOhThree = new GenericComparableBytesStructKey { Id = [103] };
            var oneOhFive = new byte[] { 105 };
            var oneOhSix = new GenericComparableBytesStructKey { Id = [106] };

            dependents =
            [
                await context.Set<GenericComparableBytesStructKeyOptionalDependent>().SingleAsync(
                    e => e.Id.Equals(new GenericComparableBytesStructKey { Id = new byte[] { 101 } })),
                await context.Set<GenericComparableBytesStructKeyOptionalDependent>().SingleAsync(
                    e => e.Id.Equals(new GenericComparableBytesStructKey(oneOhTwo))),
                await context.Set<GenericComparableBytesStructKeyOptionalDependent>().SingleAsync(e => e.Id.Equals(oneOhThree)),
                await context.Set<GenericComparableBytesStructKeyOptionalDependent>().SingleAsync(
                    e => e.Id.Equals(new GenericComparableBytesStructKey { Id = new byte[] { 104 } })),
                await context.Set<GenericComparableBytesStructKeyOptionalDependent>().SingleAsync(
                    e => e.Id.Equals(new GenericComparableBytesStructKey(oneOhFive))),
                await context.Set<GenericComparableBytesStructKeyOptionalDependent>().SingleAsync(e => e.Id.Equals(oneOhSix))
            ];

            Assert.Same(
                dependents[0],
                await context.Set<GenericComparableBytesStructKeyOptionalDependent>()
                    .FindAsync(new GenericComparableBytesStructKey { Id = [101] }));
            Assert.Same(
                dependents[1],
                await context.Set<GenericComparableBytesStructKeyOptionalDependent>()
                    .FindAsync(new GenericComparableBytesStructKey(oneOhTwo)));
            Assert.Same(dependents[2], await context.Set<GenericComparableBytesStructKeyOptionalDependent>().FindAsync(oneOhThree));
            Assert.Same(
                dependents[3],
                await context.FindAsync(
                    typeof(GenericComparableBytesStructKeyOptionalDependent),
                    new GenericComparableBytesStructKey { Id = [104] }));
            Assert.Same(
                dependents[4],
                await context.FindAsync(
                    typeof(GenericComparableBytesStructKeyOptionalDependent), new GenericComparableBytesStructKey(oneOhFive)));
            Assert.Same(dependents[5], await context.FindAsync(typeof(GenericComparableBytesStructKeyOptionalDependent), oneOhSix));
        }

        void Validate(
            GenericComparableBytesStructKeyPrincipal[] principals,
            GenericComparableBytesStructKeyOptionalDependent[] dependents,
            (int, int[])[] expectedPrincipalToDependents,
            (int, int?)[] expectedDependentToPrincipals)
            => ValidateOptionalBytes(
                principals,
                dependents,
                expectedPrincipalToDependents,
                expectedDependentToPrincipals,
                p => ((GenericComparableBytesStructKeyPrincipal)p).OptionalDependents.Select(d => (IBytesOptionalDependent)d).ToList(),
                d => ((GenericComparableBytesStructKeyOptionalDependent)d).Principal);
    }

    [ConditionalFact]
    public virtual async Task Can_insert_and_read_back_with_struct_binary_key_and_required_dependents()
    {
        BytesStructKeyPrincipal[] principals = null;
        BytesStructKeyRequiredDependent[] dependents = null;
        await InsertRequiredBytesGraph<BytesStructKeyPrincipal, BytesStructKeyRequiredDependent>();

        using (var context = CreateContext())
        {
            await RunQueries(context);

            Validate(
                principals,
                dependents,
                [(0, [0]), (1, [1]), (2, [2, 2, 2, 2]), (3, [])],
                [(0, 0), (1, 1), (2, 2), (3, 2), (4, 2), (5, 2)]);

            foreach (var principal in principals)
            {
                principal.Foo = "Mutant!";
            }

            dependents[5].Principal = principals[0];
            context.Remove(dependents[4]);
            dependents[3].PrincipalId = principals[0].Id;
            principals[1].RequiredDependents.Clear();

            context.Remove(dependents[0]);
            principals[0].RequiredDependents.Add(
                new BytesStructKeyRequiredDependent { Id = new BytesStructKey(dependents[0].Id.Id), });

            await context.SaveChangesAsync();
        }

        using (var context = CreateContext())
        {
            await RunQueries(context);

            Validate(
                principals,
                dependents,
                [(0, [0, 3, 5]), (1, []), (2, [2]), (3, [])],
                [(0, 0), (2, 2), (3, 0), (5, 0)]);
        }

        async Task RunQueries(DbContext context)
        {
            var twelve = new byte[] { 12, 12 };
            var thirteen = new BytesStructKey { Id = [13, 13, 13] };

            principals =
            [
                await context.Set<BytesStructKeyPrincipal>().Include(e => e.RequiredDependents)
                    .SingleAsync(e => e.Id.Equals(new BytesStructKey { Id = new byte[] { 11 } })),
                await context.Set<BytesStructKeyPrincipal>().Include(e => e.RequiredDependents)
                    .SingleAsync(e => e.Id.Equals(new BytesStructKey { Id = twelve })),
                await context.Set<BytesStructKeyPrincipal>().Include(e => e.RequiredDependents).SingleAsync(e => e.Id.Equals(thirteen)),
                await context.Set<BytesStructKeyPrincipal>().Include(e => e.RequiredDependents).SingleAsync(
                    e => e.Id.Equals(new BytesStructKey { Id = new byte[] { 14, 14, 14, 14 } }))
            ];

            var oneTwelve = new byte[] { 112 };
            var oneThirteen = new BytesStructKey { Id = [113] };
            var oneFifteeen = new byte[] { 115 };
            var oneSixteen = new BytesStructKey { Id = [116] };

            dependents =
            [
                await context.Set<BytesStructKeyRequiredDependent>()
                    .FirstOrDefaultAsync(e => e.Id.Equals(new BytesStructKey { Id = new byte[] { 111 } })),
                await context.Set<BytesStructKeyRequiredDependent>()
                    .FirstOrDefaultAsync(e => e.Id.Equals(new BytesStructKey { Id = oneTwelve })),
                await context.Set<BytesStructKeyRequiredDependent>().FirstOrDefaultAsync(e => e.Id.Equals(oneThirteen)),
                await context.Set<BytesStructKeyRequiredDependent>()
                    .FirstOrDefaultAsync(e => e.Id.Equals(new BytesStructKey { Id = new byte[] { 114 } })),
                await context.Set<BytesStructKeyRequiredDependent>()
                    .FirstOrDefaultAsync(e => e.Id.Equals(new BytesStructKey { Id = oneFifteeen })),
                await context.Set<BytesStructKeyRequiredDependent>().FirstOrDefaultAsync(e => e.Id.Equals(oneSixteen))
            ];

            Assert.Same(
                dependents[0], await context.Set<BytesStructKeyRequiredDependent>().FindAsync(new BytesStructKey { Id = [111] }));
            Assert.Same(dependents[1], await context.Set<BytesStructKeyRequiredDependent>().FindAsync(new BytesStructKey { Id = oneTwelve }));
            Assert.Same(dependents[2], await context.Set<BytesStructKeyRequiredDependent>().FindAsync(oneThirteen));
            Assert.Same(
                dependents[3], await context.FindAsync(typeof(BytesStructKeyRequiredDependent), new BytesStructKey { Id = [114] }));
            Assert.Same(dependents[4], await context.FindAsync(typeof(BytesStructKeyRequiredDependent), new BytesStructKey { Id = oneFifteeen }));
            Assert.Same(dependents[5], await context.FindAsync(typeof(BytesStructKeyRequiredDependent), oneSixteen));
        }

        void Validate(
            BytesStructKeyPrincipal[] principals,
            BytesStructKeyRequiredDependent[] dependents,
            (int, int[])[] expectedPrincipalToDependents,
            (int, int)[] expectedDependentToPrincipals)
            => ValidateRequiredBytes(
                principals,
                dependents,
                expectedPrincipalToDependents,
                expectedDependentToPrincipals,
                p => ((BytesStructKeyPrincipal)p).RequiredDependents.Select(d => (IBytesRequiredDependent)d).ToList(),
                d => ((BytesStructKeyRequiredDependent)d).Principal);
    }

    [ConditionalFact]
    public virtual async Task Can_insert_and_read_back_with_comparable_struct_binary_key_and_required_dependents()
    {
        ComparableBytesStructKeyPrincipal[] principals = null;
        ComparableBytesStructKeyRequiredDependent[] dependents = null;
        await InsertRequiredBytesGraph<ComparableBytesStructKeyPrincipal, ComparableBytesStructKeyRequiredDependent>();

        using (var context = CreateContext())
        {
            await RunQueries(context);

            Validate(
                principals,
                dependents,
                [(0, [0]), (1, [1]), (2, [2, 2, 2, 2]), (3, [])],
                [(0, 0), (1, 1), (2, 2), (3, 2), (4, 2), (5, 2)]);

            foreach (var principal in principals)
            {
                principal.Foo = "Mutant!";
            }

            dependents[5].Principal = principals[0];
            context.Remove(dependents[4]);
            dependents[3].PrincipalId = principals[0].Id;
            principals[1].RequiredDependents.Clear();

            context.Remove(dependents[0]);
            principals[0].RequiredDependents.Add(
                new ComparableBytesStructKeyRequiredDependent { Id = new ComparableBytesStructKey(dependents[0].Id.Id), });

            await context.SaveChangesAsync();
        }

        using (var context = CreateContext())
        {
            await RunQueries(context);

            Validate(
                principals,
                dependents,
                [(0, [0, 3, 5]), (1, []), (2, [2]), (3, [])],
                [(0, 0), (2, 2), (3, 0), (5, 0)]);
        }

        async Task RunQueries(DbContext context)
        {
            var twelve = new byte[] { 12, 12 };
            var thirteen = new ComparableBytesStructKey { Id = [13, 13, 13] };

            principals =
            [
                await context.Set<ComparableBytesStructKeyPrincipal>().Include(e => e.RequiredDependents).SingleAsync(
                    e => e.Id.Equals(new ComparableBytesStructKey { Id = new byte[] { 11 } })),
                await context.Set<ComparableBytesStructKeyPrincipal>().Include(e => e.RequiredDependents)
                    .SingleAsync(e => e.Id.Equals(new ComparableBytesStructKey { Id = twelve })),
                await context.Set<ComparableBytesStructKeyPrincipal>().Include(e => e.RequiredDependents)
                    .SingleAsync(e => e.Id.Equals(thirteen)),
                await context.Set<ComparableBytesStructKeyPrincipal>().Include(e => e.RequiredDependents).SingleAsync(
                    e => e.Id.Equals(new ComparableBytesStructKey { Id = new byte[] { 14, 14, 14, 14 } }))
            ];

            var oneTwelve = new byte[] { 112 };
            var oneThirteen = new ComparableBytesStructKey { Id = [113] };
            var oneFifteeen = new byte[] { 115 };
            var oneSixteen = new ComparableBytesStructKey { Id = [116] };

            dependents =
            [
                await context.Set<ComparableBytesStructKeyRequiredDependent>().FirstOrDefaultAsync(
                    e => e.Id.Equals(new ComparableBytesStructKey { Id = new byte[] { 111 } })),
                await context.Set<ComparableBytesStructKeyRequiredDependent>()
                    .FirstOrDefaultAsync(e => e.Id.Equals(new ComparableBytesStructKey { Id = oneTwelve })),
                await context.Set<ComparableBytesStructKeyRequiredDependent>().FirstOrDefaultAsync(e => e.Id.Equals(oneThirteen)),
                await context.Set<ComparableBytesStructKeyRequiredDependent>().FirstOrDefaultAsync(
                    e => e.Id.Equals(new ComparableBytesStructKey { Id = new byte[] { 114 } })),
                await context.Set<ComparableBytesStructKeyRequiredDependent>()
                    .FirstOrDefaultAsync(e => e.Id.Equals(new ComparableBytesStructKey { Id = oneFifteeen })),
                await context.Set<ComparableBytesStructKeyRequiredDependent>().FirstOrDefaultAsync(e => e.Id.Equals(oneSixteen))
            ];

            Assert.Same(
                dependents[0],
                await context.Set<ComparableBytesStructKeyRequiredDependent>()
                    .FindAsync(new ComparableBytesStructKey { Id = [111] }));
            Assert.Same(
                dependents[1],
                await context.Set<ComparableBytesStructKeyRequiredDependent>().FindAsync(new ComparableBytesStructKey { Id = oneTwelve }));
            Assert.Same(dependents[2], await context.Set<ComparableBytesStructKeyRequiredDependent>().FindAsync(oneThirteen));
            Assert.Same(
                dependents[3],
                await context.FindAsync(
                    typeof(ComparableBytesStructKeyRequiredDependent), new ComparableBytesStructKey { Id = [114] }));
            Assert.Same(
                dependents[4],
                await context.FindAsync(typeof(ComparableBytesStructKeyRequiredDependent), new ComparableBytesStructKey { Id = oneFifteeen }));
            Assert.Same(dependents[5], await context.FindAsync(typeof(ComparableBytesStructKeyRequiredDependent), oneSixteen));
        }

        void Validate(
            ComparableBytesStructKeyPrincipal[] principals,
            ComparableBytesStructKeyRequiredDependent[] dependents,
            (int, int[])[] expectedPrincipalToDependents,
            (int, int)[] expectedDependentToPrincipals)
            => ValidateRequiredBytes(
                principals,
                dependents,
                expectedPrincipalToDependents,
                expectedDependentToPrincipals,
                p => ((ComparableBytesStructKeyPrincipal)p).RequiredDependents.Select(d => (IBytesRequiredDependent)d).ToList(),
                d => ((ComparableBytesStructKeyRequiredDependent)d).Principal);
    }

    [ConditionalFact]
    public virtual async Task Can_insert_and_read_back_with_structural_struct_binary_key_and_required_dependents()
    {
        StructuralComparableBytesStructKeyPrincipal[] principals = null;
        StructuralComparableBytesStructKeyRequiredDependent[] dependents = null;
        await InsertRequiredBytesGraph<StructuralComparableBytesStructKeyPrincipal, StructuralComparableBytesStructKeyRequiredDependent>();

        using (var context = CreateContext())
        {
            await RunQueries(context);

            Validate(
                principals,
                dependents,
                [(0, [0]), (1, [1]), (2, [2, 2, 2, 2]), (3, [])],
                [(0, 0), (1, 1), (2, 2), (3, 2), (4, 2), (5, 2)]);

            foreach (var principal in principals)
            {
                principal.Foo = "Mutant!";
            }

            dependents[5].Principal = principals[0];
            context.Remove(dependents[4]);
            dependents[3].PrincipalId = principals[0].Id;
            principals[1].RequiredDependents.Clear();

            context.Remove(dependents[0]);
            principals[0].RequiredDependents.Add(
                new StructuralComparableBytesStructKeyRequiredDependent
                {
                    Id = new StructuralComparableBytesStructKey(dependents[0].Id.Id),
                });

            await context.SaveChangesAsync();
        }

        using (var context = CreateContext())
        {
            await RunQueries(context);

            Validate(
                principals,
                dependents,
                [(0, [0, 3, 5]), (1, []), (2, [2]), (3, [])],
                [(0, 0), (2, 2), (3, 0), (5, 0)]);
        }

        async Task RunQueries(DbContext context)
        {
            var twelve = new byte[] { 12, 12 };
            var thirteen = new StructuralComparableBytesStructKey { Id = [13, 13, 13] };

            principals =
            [
                await context.Set<StructuralComparableBytesStructKeyPrincipal>().Include(e => e.RequiredDependents).SingleAsync(
                    e => e.Id.Equals(new StructuralComparableBytesStructKey { Id = new byte[] { 11 } })),
                await context.Set<StructuralComparableBytesStructKeyPrincipal>().Include(e => e.RequiredDependents)
                    .SingleAsync(e => e.Id.Equals(new StructuralComparableBytesStructKey { Id = twelve })),
                await context.Set<StructuralComparableBytesStructKeyPrincipal>().Include(e => e.RequiredDependents)
                    .SingleAsync(e => e.Id.Equals(thirteen)),
                await context.Set<StructuralComparableBytesStructKeyPrincipal>().Include(e => e.RequiredDependents).SingleAsync(
                    e => e.Id.Equals(new StructuralComparableBytesStructKey { Id = new byte[] { 14, 14, 14, 14 } }))
            ];

            var oneTwelve = new byte[] { 112 };
            var oneThirteen = new StructuralComparableBytesStructKey { Id = [113] };
            var oneFifteeen = new byte[] { 115 };
            var oneSixteen = new StructuralComparableBytesStructKey { Id = [116] };

            dependents =
            [
                await context.Set<StructuralComparableBytesStructKeyRequiredDependent>().FirstOrDefaultAsync(
                    e => e.Id.Equals(new StructuralComparableBytesStructKey { Id = new byte[] { 111 } })),
                await context.Set<StructuralComparableBytesStructKeyRequiredDependent>().FirstOrDefaultAsync(
                    e => e.Id.Equals(new StructuralComparableBytesStructKey { Id = oneTwelve })),
                await context.Set<StructuralComparableBytesStructKeyRequiredDependent>().FirstOrDefaultAsync(e => e.Id.Equals(oneThirteen)),
                await context.Set<StructuralComparableBytesStructKeyRequiredDependent>().FirstOrDefaultAsync(
                    e => e.Id.Equals(new StructuralComparableBytesStructKey { Id = new byte[] { 114 } })),
                await context.Set<StructuralComparableBytesStructKeyRequiredDependent>().FirstOrDefaultAsync(
                    e => e.Id.Equals(new StructuralComparableBytesStructKey { Id = oneFifteeen })),
                await context.Set<StructuralComparableBytesStructKeyRequiredDependent>().FirstOrDefaultAsync(e => e.Id.Equals(oneSixteen))
            ];

            Assert.Same(
                dependents[0],
                await context.Set<StructuralComparableBytesStructKeyRequiredDependent>()
                    .FindAsync(new StructuralComparableBytesStructKey { Id = [111] }));
            Assert.Same(
                dependents[1],
                await context.Set<StructuralComparableBytesStructKeyRequiredDependent>()
                    .FindAsync(new StructuralComparableBytesStructKey { Id = oneTwelve }));
            Assert.Same(dependents[2], await context.Set<StructuralComparableBytesStructKeyRequiredDependent>().FindAsync(oneThirteen));
            Assert.Same(
                dependents[3],
                await context.FindAsync(
                    typeof(StructuralComparableBytesStructKeyRequiredDependent),
                    new StructuralComparableBytesStructKey { Id = [114] }));
            Assert.Same(
                dependents[4],
                await context.FindAsync(
                    typeof(StructuralComparableBytesStructKeyRequiredDependent),
                    new StructuralComparableBytesStructKey { Id = oneFifteeen }));
            Assert.Same(dependents[5], await context.FindAsync(typeof(StructuralComparableBytesStructKeyRequiredDependent), oneSixteen));
        }

        void Validate(
            StructuralComparableBytesStructKeyPrincipal[] principals,
            StructuralComparableBytesStructKeyRequiredDependent[] dependents,
            (int, int[])[] expectedPrincipalToDependents,
            (int, int)[] expectedDependentToPrincipals)
            => ValidateRequiredBytes(
                principals,
                dependents,
                expectedPrincipalToDependents,
                expectedDependentToPrincipals,
                p => ((StructuralComparableBytesStructKeyPrincipal)p).RequiredDependents.Select(d => (IBytesRequiredDependent)d)
                    .ToList(),
                d => ((StructuralComparableBytesStructKeyRequiredDependent)d).Principal);
    }

    [ConditionalFact]
    public virtual async Task Can_insert_and_read_back_with_generic_comparable_struct_binary_key_and_required_dependents()
    {
        GenericComparableBytesStructKeyPrincipal[] principals = null;
        GenericComparableBytesStructKeyRequiredDependent[] dependents = null;
        await InsertRequiredBytesGraph<GenericComparableBytesStructKeyPrincipal, GenericComparableBytesStructKeyRequiredDependent>();

        using (var context = CreateContext())
        {
            await RunQueries(context);

            Validate(
                principals,
                dependents,
                [(0, [0]), (1, [1]), (2, [2, 2, 2, 2]), (3, [])],
                [(0, 0), (1, 1), (2, 2), (3, 2), (4, 2), (5, 2)]);

            foreach (var principal in principals)
            {
                principal.Foo = "Mutant!";
            }

            dependents[5].Principal = principals[0];
            context.Remove(dependents[4]);
            dependents[3].PrincipalId = principals[0].Id;
            principals[1].RequiredDependents.Clear();

            context.Remove(dependents[0]);
            principals[0].RequiredDependents.Add(
                new GenericComparableBytesStructKeyRequiredDependent { Id = new GenericComparableBytesStructKey(dependents[0].Id.Id), });

            await context.SaveChangesAsync();
        }

        using (var context = CreateContext())
        {
            await RunQueries(context);

            Validate(
                principals,
                dependents,
                [(0, [0, 3, 5]), (1, []), (2, [2]), (3, [])],
                [(0, 0), (2, 2), (3, 0), (5, 0)]);
        }

        async Task RunQueries(DbContext context)
        {
            var twelve = new byte[] { 12, 12 };
            var thirteen = new GenericComparableBytesStructKey { Id = [13, 13, 13] };

            principals =
            [
                await context.Set<GenericComparableBytesStructKeyPrincipal>().Include(e => e.RequiredDependents).SingleAsync(
                    e => e.Id.Equals(new GenericComparableBytesStructKey { Id = new byte[] { 11 } })),
                await context.Set<GenericComparableBytesStructKeyPrincipal>().Include(e => e.RequiredDependents)
                    .SingleAsync(e => e.Id.Equals(new GenericComparableBytesStructKey { Id = twelve })),
                await context.Set<GenericComparableBytesStructKeyPrincipal>().Include(e => e.RequiredDependents)
                    .SingleAsync(e => e.Id.Equals(thirteen)),
                await context.Set<GenericComparableBytesStructKeyPrincipal>().Include(e => e.RequiredDependents).SingleAsync(
                    e => e.Id.Equals(new GenericComparableBytesStructKey { Id = new byte[] { 14, 14, 14, 14 } }))
            ];

            var oneTwelve = new byte[] { 112 };
            var oneThirteen = new GenericComparableBytesStructKey { Id = [113] };
            var oneFifteeen = new byte[] { 115 };
            var oneSixteen = new GenericComparableBytesStructKey { Id = [116] };

            dependents =
            [
                await context.Set<GenericComparableBytesStructKeyRequiredDependent>().FirstOrDefaultAsync(
                    e => e.Id.Equals(new GenericComparableBytesStructKey { Id = new byte[] { 111 } })),
                await context.Set<GenericComparableBytesStructKeyRequiredDependent>().FirstOrDefaultAsync(
                    e => e.Id.Equals(new GenericComparableBytesStructKey { Id = oneTwelve })),
                await context.Set<GenericComparableBytesStructKeyRequiredDependent>().FirstOrDefaultAsync(e => e.Id.Equals(oneThirteen)),
                await context.Set<GenericComparableBytesStructKeyRequiredDependent>().FirstOrDefaultAsync(
                    e => e.Id.Equals(new GenericComparableBytesStructKey { Id = new byte[] { 114 } })),
                await context.Set<GenericComparableBytesStructKeyRequiredDependent>().FirstOrDefaultAsync(
                    e => e.Id.Equals(new GenericComparableBytesStructKey { Id = oneFifteeen })),
                await context.Set<GenericComparableBytesStructKeyRequiredDependent>().FirstOrDefaultAsync(e => e.Id.Equals(oneSixteen))
            ];

            Assert.Same(
                dependents[0],
                await context.Set<GenericComparableBytesStructKeyRequiredDependent>()
                    .FindAsync(new GenericComparableBytesStructKey { Id = [111] }));
            Assert.Same(
                dependents[1],
                await context.Set<GenericComparableBytesStructKeyRequiredDependent>()
                    .FindAsync(new GenericComparableBytesStructKey { Id = oneTwelve }));
            Assert.Same(dependents[2], await context.Set<GenericComparableBytesStructKeyRequiredDependent>().FindAsync(oneThirteen));
            Assert.Same(
                dependents[3],
                await context.FindAsync(
                    typeof(GenericComparableBytesStructKeyRequiredDependent),
                    new GenericComparableBytesStructKey { Id = [114] }));
            Assert.Same(
                dependents[4],
                await context.FindAsync(
                    typeof(GenericComparableBytesStructKeyRequiredDependent),
                    new GenericComparableBytesStructKey { Id = oneFifteeen }));
            Assert.Same(dependents[5], await context.FindAsync(typeof(GenericComparableBytesStructKeyRequiredDependent), oneSixteen));
        }

        void Validate(
            GenericComparableBytesStructKeyPrincipal[] principals,
            GenericComparableBytesStructKeyRequiredDependent[] dependents,
            (int, int[])[] expectedPrincipalToDependents,
            (int, int)[] expectedDependentToPrincipals)
            => ValidateRequiredBytes(
                principals,
                dependents,
                expectedPrincipalToDependents,
                expectedDependentToPrincipals,
                p => ((GenericComparableBytesStructKeyPrincipal)p).RequiredDependents.Select(d => (IBytesRequiredDependent)d).ToList(),
                d => ((GenericComparableBytesStructKeyRequiredDependent)d).Principal);
    }

    [ConditionalFact]
    public virtual async Task Can_query_and_update_owned_entity_with_value_converter()
    {
        using (var context = CreateContext())
        {
            var key = new Key("1-1-1");
            var text = new TextEntity { Position = 1 };

            var ownedEntity = new BaseEntity(key, text);

            context.Add(ownedEntity);

            await context.SaveChangesAsync();
        }

        using (var context = CreateContext())
        {
            var key = new Key("1-1-1");
            var ownedEntity = await context.Set<BaseEntity>().SingleAsync(o => o.Name == key);

            Assert.Equal(1, ownedEntity.Text.Position);

            var updatedText = new TextEntity { Position = 0 };
            ownedEntity.Text = updatedText;
            context.Set<BaseEntity>().Update(ownedEntity);

            await context.SaveChangesAsync();
        }

        using (var context = CreateContext())
        {
            var key = new Key("1-1-1");
            var ownedEntity = await context.Set<BaseEntity>().FindAsync(key);

            Assert.Equal(0, ownedEntity.Text.Position);
        }
    }

    [ConditionalFact]
    public virtual async Task Can_query_and_update_owned_entity_with_int_struct_key()
    {
        using (var context = CreateContext())
        {
            context.Add(new OwnerIntStructKey(new IntStructKey(1), new OwnedIntStructKey(77)));
            await context.SaveChangesAsync();
        }

        using (var context = CreateContext())
        {
            var owner = await context.Set<OwnerIntStructKey>().SingleAsync(o => o.Id.Equals(new IntStructKey(1)));

            Assert.Equal(77, owner.Owned.Position);

            owner.Owned = new OwnedIntStructKey(88);

            await context.SaveChangesAsync();
        }

        using (var context = CreateContext())
        {
            var owner = await context.Set<OwnerIntStructKey>().FindAsync(new IntStructKey(1));

            Assert.Equal(88, owner.Owned.Position);
        }
    }

    [ConditionalFact]
    public virtual async Task Can_query_and_update_owned_entity_with_binary_struct_key()
    {
        using (var context = CreateContext())
        {
            context.Add(new OwnerBytesStructKey(new BytesStructKey([1, 5, 7, 1]), new OwnedBytesStructKey(77)));
            await context.SaveChangesAsync();
        }

        using (var context = CreateContext())
        {
            var owner = await context.Set<OwnerBytesStructKey>().SingleAsync(o => o.Id.Equals(new BytesStructKey(new byte[] { 1, 5, 7, 1 })));
            Assert.Equal(77, owner.Owned.Position);

            owner.Owned = new OwnedBytesStructKey(88);
            await context.SaveChangesAsync();
        }

        using (var context = CreateContext())
        {
            var owner = await context.Set<OwnerBytesStructKey>().FindAsync(new BytesStructKey([1, 5, 7, 1]));
            Assert.Equal(88, owner.Owned.Position);
        }
    }

    [ConditionalFact]
    public virtual async Task Can_query_and_update_owned_entity_with_comparable_int_struct_key()
    {
        using (var context = CreateContext())
        {
            context.Add(new OwnerComparableIntStructKey(new ComparableIntStructKey(1), new OwnedComparableIntStructKey(77)));
            await context.SaveChangesAsync();
        }

        using (var context = CreateContext())
        {
            var owner = await context.Set<OwnerComparableIntStructKey>().SingleAsync(o => o.Id.Equals(new ComparableIntStructKey(1)));
            Assert.Equal(77, owner.Owned.Position);

            owner.Owned = new OwnedComparableIntStructKey(88);
            await context.SaveChangesAsync();
        }

        using (var context = CreateContext())
        {
            var owner = await context.Set<OwnerComparableIntStructKey>().FindAsync(new ComparableIntStructKey(1));
            Assert.Equal(88, owner.Owned.Position);
        }
    }

    [ConditionalFact]
    public virtual async Task Can_query_and_update_owned_entity_with_comparable_bytes_struct_key()
    {
        using (var context = CreateContext())
        {
            context.Add(
                new OwnerComparableBytesStructKey(
                    new ComparableBytesStructKey([1, 5, 7, 1]), new OwnedComparableBytesStructKey(77)));
            await context.SaveChangesAsync();
        }

        using (var context = CreateContext())
        {
            var owner = await context.Set<OwnerComparableBytesStructKey>()
                .SingleAsync(o => o.Id.Equals(new ComparableBytesStructKey(new byte[] { 1, 5, 7, 1 })));
            Assert.Equal(77, owner.Owned.Position);

            owner.Owned = new OwnedComparableBytesStructKey(88);
            await context.SaveChangesAsync();
        }

        using (var context = CreateContext())
        {
            var owner = await context.Set<OwnerComparableBytesStructKey>().FindAsync(new ComparableBytesStructKey([1, 5, 7, 1]));
            Assert.Equal(88, owner.Owned.Position);
        }
    }

    [ConditionalFact]
    public virtual async Task Can_query_and_update_owned_entity_with_generic_comparable_int_struct_key()
    {
        using (var context = CreateContext())
        {
            context.Add(
                new OwnerGenericComparableIntStructKey(
                    new GenericComparableIntStructKey(1), new OwnedGenericComparableIntStructKey(77)));
            await context.SaveChangesAsync();
        }

        using (var context = CreateContext())
        {
            var owner = await context.Set<OwnerGenericComparableIntStructKey>()
                .SingleAsync(o => o.Id.Equals(new GenericComparableIntStructKey(1)));
            Assert.Equal(77, owner.Owned.Position);

            owner.Owned = new OwnedGenericComparableIntStructKey(88);
            await context.SaveChangesAsync();
        }

        using (var context = CreateContext())
        {
            var owner = await context.Set<OwnerGenericComparableIntStructKey>().FindAsync(new GenericComparableIntStructKey(1));
            Assert.Equal(88, owner.Owned.Position);
        }
    }

    [ConditionalFact]
    public virtual async Task Can_query_and_update_owned_entity_with_generic_comparable_bytes_struct_key()
    {
        using (var context = CreateContext())
        {
            context.Add(
                new OwnerGenericComparableBytesStructKey(
                    new GenericComparableBytesStructKey([1, 5, 7, 1]), new OwnedGenericComparableBytesStructKey(77)));
            await context.SaveChangesAsync();
        }

        using (var context = CreateContext())
        {
            var owner = await context.Set<OwnerGenericComparableBytesStructKey>()
                .SingleAsync(o => o.Id.Equals(new GenericComparableBytesStructKey(new byte[] { 1, 5, 7, 1 })));
            Assert.Equal(77, owner.Owned.Position);

            owner.Owned = new OwnedGenericComparableBytesStructKey(88);
            await context.SaveChangesAsync();
        }

        using (var context = CreateContext())
        {
            var owner = await context.Set<OwnerGenericComparableBytesStructKey>()
                .FindAsync(new GenericComparableBytesStructKey([1, 5, 7, 1]));
            Assert.Equal(88, owner.Owned.Position);
        }
    }

    [ConditionalFact]
    public virtual async Task Can_query_and_update_owned_entity_with_structural_generic_comparable_bytes_struct_key()
    {
        using (var context = CreateContext())
        {
            context.Add(
                new OwnerStructuralComparableBytesStructKey(
                    new StructuralComparableBytesStructKey([1, 5, 7, 1]),
                    new OwnedStructuralComparableBytesStructKey(77)));
            await context.SaveChangesAsync();
        }

        using (var context = CreateContext())
        {
            var owner = await context.Set<OwnerStructuralComparableBytesStructKey>().SingleAsync(
                o => o.Id.Equals(new StructuralComparableBytesStructKey(new byte[] { 1, 5, 7, 1 })));
            Assert.Equal(77, owner.Owned.Position);

            owner.Owned = new OwnedStructuralComparableBytesStructKey(88);
            await context.SaveChangesAsync();
        }

        using (var context = CreateContext())
        {
            var owner = await context.Set<OwnerStructuralComparableBytesStructKey>()
                .FindAsync(new StructuralComparableBytesStructKey([1, 5, 7, 1]));
            Assert.Equal(88, owner.Owned.Position);
        }
    }

    [ConditionalFact]
    public virtual async Task Can_query_and_update_owned_entity_with_int_class_key()
    {
        using (var context = CreateContext())
        {
            context.Add(new OwnerIntClassKey(new IntClassKey(1), new OwnedIntClassKey(77)));
            await context.SaveChangesAsync();
        }

        using (var context = CreateContext())
        {
            var owner = await context.Set<OwnerIntClassKey>().SingleAsync(o => o.Id.Equals(new IntClassKey(1)));
            Assert.Equal(77, owner.Owned.Position);

            owner.Owned = new OwnedIntClassKey(88);
            await context.SaveChangesAsync();
        }

        using (var context = CreateContext())
        {
            var owner = await context.Set<OwnerIntClassKey>().FindAsync(new IntClassKey(1));
            Assert.Equal(88, owner.Owned.Position);
        }
    }

    [ConditionalFact]
    public virtual async Task Can_query_and_update_owned_entity_with_int_bare_class_key()
    {
        using (var context = CreateContext())
        {
            context.Add(new OwnerBareIntClassKey(new BareIntClassKey(1), new OwnedBareIntClassKey(77)));
            await context.SaveChangesAsync();
        }

        using (var context = CreateContext())
        {
            var owner = await context.Set<OwnerBareIntClassKey>().SingleAsync(o => o.Id.Equals(new BareIntClassKey(1)));
            Assert.Equal(77, owner.Owned.Position);

            owner.Owned = new OwnedBareIntClassKey(88);
            await context.SaveChangesAsync();
        }

        using (var context = CreateContext())
        {
            var owner = await context.Set<OwnerBareIntClassKey>().FindAsync(new BareIntClassKey(1));
            Assert.Equal(88, owner.Owned.Position);
        }
    }

    [ConditionalFact]
    public virtual async Task Can_query_and_update_owned_entity_with_comparable_int_class_key()
    {
        using (var context = CreateContext())
        {
            context.Add(new OwnerComparableIntClassKey(new ComparableIntClassKey(1), new OwnedComparableIntClassKey(77)));
            await context.SaveChangesAsync();
        }

        using (var context = CreateContext())
        {
            var owner = await context.Set<OwnerComparableIntClassKey>().SingleAsync(o => o.Id.Equals(new ComparableIntClassKey(1)));
            Assert.Equal(77, owner.Owned.Position);

            owner.Owned = new OwnedComparableIntClassKey(88);
            await context.SaveChangesAsync();
        }

        using (var context = CreateContext())
        {
            var owner = await context.Set<OwnerComparableIntClassKey>().FindAsync(new ComparableIntClassKey(1));
            Assert.Equal(88, owner.Owned.Position);
        }
    }

    [ConditionalFact]
    public virtual async Task Can_query_and_update_owned_entity_with_generic_comparable_int_class_key()
    {
        using (var context = CreateContext())
        {
            context.Add(
                new OwnerGenericComparableIntClassKey(new GenericComparableIntClassKey(1), new OwnedGenericComparableIntClassKey(77)));
            await context.SaveChangesAsync();
        }

        using (var context = CreateContext())
        {
            var owner = await context.Set<OwnerGenericComparableIntClassKey>().SingleAsync(o => o.Id.Equals(new GenericComparableIntClassKey(1)));
            Assert.Equal(77, owner.Owned.Position);

            owner.Owned = new OwnedGenericComparableIntClassKey(88);
            await context.SaveChangesAsync();
        }

        using (var context = CreateContext())
        {
            var owner = await context.Set<OwnerGenericComparableIntClassKey>().FindAsync(new GenericComparableIntClassKey(1));
            Assert.Equal(88, owner.Owned.Position);
        }
    }

    [ConditionalFact]
    public virtual async Task Can_insert_and_read_back_with_struct_key_and_optional_dependents_with_shadow_FK()
    {
        IntStructKeyPrincipalShadow[] principals = null;
        IntStructKeyOptionalDependentShadow[] dependents = null;
        using (var context = CreateContext())
        {
            var principals0 = new IntStructKeyPrincipalShadow[]
            {
                new() { Id = new IntStructKey(1), Foo = "X1" },
                new() { Id = new IntStructKey(2), Foo = "X2" },
                new() { Id = new IntStructKey(3), Foo = "X3" },
                new() { Id = new IntStructKey(4), Foo = "X4" }
            };

            context.Set<IntStructKeyPrincipalShadow>().AddRange(principals0);

            context.Set<IntStructKeyOptionalDependentShadow>().AddRange(
                new IntStructKeyOptionalDependentShadow { Id = new IntStructKey(101), Principal = principals0[0] },
                new IntStructKeyOptionalDependentShadow { Id = new IntStructKey(102), Principal = principals0[1] },
                new IntStructKeyOptionalDependentShadow { Id = new IntStructKey(103), Principal = principals0[2] },
                new IntStructKeyOptionalDependentShadow { Id = new IntStructKey(104), Principal = principals0[2] },
                new IntStructKeyOptionalDependentShadow { Id = new IntStructKey(105), Principal = principals0[2] },
                new IntStructKeyOptionalDependentShadow { Id = new IntStructKey(106) });

            Assert.Equal(10, await context.SaveChangesAsync());
        }

        using (var context = CreateContext())
        {
            await RunQueries(context);

            Validate(
                principals,
                dependents,
                [(0, [0]), (1, [1]), (2, [2, 2, 2]), (3, [])],
                [(0, 0), (1, 1), (2, 2), (3, 2), (4, 2), (5, null)]);

            foreach (var principal in principals)
            {
                principal.Foo = "Mutant!";
            }

            dependents[5].Principal = principals[0];

            context.Entry(dependents[4]).Property("PrincipalId").CurrentValue = null;
            context.Entry(dependents[3]).Property("PrincipalId").CurrentValue = principals[0].Id;
            principals[1].OptionalDependents.Clear();

            context.Remove(dependents[0]);
            principals[0].OptionalDependents.Add(
                new IntStructKeyOptionalDependentShadow { Id = new IntStructKey(dependents[0].Id.Id), });

            await context.SaveChangesAsync();
        }

        using (var context = CreateContext())
        {
            await RunQueries(context);

            Validate(
                principals,
                dependents,
                [(0, [0, 3, 5]), (1, []), (2, [2]), (3, [])],
                [(0, 0), (1, null), (2, 2), (3, 0), (4, null), (5, 0)]);
        }

        async Task RunQueries(DbContext context)
        {
            var two = 2;
            var three = new IntStructKey(3);

            principals =
            [
                await context.Set<IntStructKeyPrincipalShadow>().Include(e => e.OptionalDependents)
                    .SingleAsync(e => e.Id.Equals(new IntStructKey(1))),
                await context.Set<IntStructKeyPrincipalShadow>().Include(e => e.OptionalDependents)
                    .SingleAsync(e => e.Id.Equals(new IntStructKey(two))),
                await context.Set<IntStructKeyPrincipalShadow>().Include(e => e.OptionalDependents).SingleAsync(e => e.Id.Equals(three)),
                await context.Set<IntStructKeyPrincipalShadow>().Include(e => e.OptionalDependents)
                    .SingleAsync(e => e.Id.Equals(new IntStructKey(4)))
            ];

            var oneOhTwo = 102;
            var oneOhThree = new IntStructKey(103);
            var oneOhFive = 105;
            var oneOhSix = new IntStructKey(106);

            dependents =
            [
                await context.Set<IntStructKeyOptionalDependentShadow>().SingleAsync(e => e.Id.Equals(new IntStructKey(101))),
                await context.Set<IntStructKeyOptionalDependentShadow>().SingleAsync(e => e.Id.Equals(new IntStructKey(oneOhTwo))),
                await context.Set<IntStructKeyOptionalDependentShadow>().SingleAsync(e => e.Id.Equals(oneOhThree)),
                await context.Set<IntStructKeyOptionalDependentShadow>().SingleAsync(e => e.Id.Equals(new IntStructKey(104))),
                await context.Set<IntStructKeyOptionalDependentShadow>().SingleAsync(e => e.Id.Equals(new IntStructKey(oneOhFive))),
                await context.Set<IntStructKeyOptionalDependentShadow>().SingleAsync(e => e.Id.Equals(oneOhSix))
            ];

            Assert.Same(dependents[0], await context.Set<IntStructKeyOptionalDependentShadow>().FindAsync(new IntStructKey(101)));
            Assert.Same(dependents[1], await context.Set<IntStructKeyOptionalDependentShadow>().FindAsync(new IntStructKey(oneOhTwo)));
            Assert.Same(dependents[2], await context.Set<IntStructKeyOptionalDependentShadow>().FindAsync(oneOhThree));
            Assert.Same(dependents[3], await context.FindAsync(typeof(IntStructKeyOptionalDependentShadow), new IntStructKey(104)));
            Assert.Same(dependents[4], await context.FindAsync(typeof(IntStructKeyOptionalDependentShadow), new IntStructKey(oneOhFive)));
            Assert.Same(dependents[5], await context.FindAsync(typeof(IntStructKeyOptionalDependentShadow), oneOhSix));
        }

        void Validate(
            IntStructKeyPrincipalShadow[] principals,
            IntStructKeyOptionalDependentShadow[] dependents,
            (int, int[])[] expectedPrincipalToDependents,
            (int, int?)[] expectedDependentToPrincipals)
        {
            Assert.Equal(4, principals.Length);
            for (var i = 0; i < 4; i++)
            {
                Assert.Equal(i + 1, principals[i].Id.Id);
            }

            Assert.Equal(6, dependents.Length);
            for (var i = 0; i < 6; i++)
            {
                Assert.Equal(i + 101, dependents[i].Id.Id);
            }

            foreach (var (dependentIndex, principalIndex) in expectedDependentToPrincipals)
            {
                if (principalIndex.HasValue)
                {
                    Assert.Same(principals[principalIndex.Value], dependents[dependentIndex].Principal);
                }
                else
                {
                    Assert.Null(dependents[dependentIndex].Principal);
                }
            }

            foreach (var (principalIndex, dependentIndexes) in expectedPrincipalToDependents)
            {
                Assert.Equal(dependentIndexes.Length, principals[principalIndex].OptionalDependents.Count);
                foreach (var dependentIndex in dependentIndexes)
                {
                    Assert.Same(principals[principalIndex], dependents[dependentIndex].Principal);
                }
            }
        }
    }

    [ConditionalFact]
    public virtual async Task Can_insert_and_read_back_with_comparable_struct_key_and_optional_dependents_with_shadow_FK()
    {
        ComparableIntStructKeyPrincipalShadow[] principals = null;
        ComparableIntStructKeyOptionalDependentShadow[] dependents = null;
        using (var context = CreateContext())
        {
            var principals0 = new ComparableIntStructKeyPrincipalShadow[]
            {
                new() { Id = new ComparableIntStructKey(1), Foo = "X1" },
                new() { Id = new ComparableIntStructKey(2), Foo = "X2" },
                new() { Id = new ComparableIntStructKey(3), Foo = "X3" },
                new() { Id = new ComparableIntStructKey(4), Foo = "X4" }
            };

            context.Set<ComparableIntStructKeyPrincipalShadow>().AddRange(principals0);

            context.Set<ComparableIntStructKeyOptionalDependentShadow>().AddRange(
                new ComparableIntStructKeyOptionalDependentShadow { Id = new ComparableIntStructKey(101), Principal = principals0[0] },
                new ComparableIntStructKeyOptionalDependentShadow { Id = new ComparableIntStructKey(102), Principal = principals0[1] },
                new ComparableIntStructKeyOptionalDependentShadow { Id = new ComparableIntStructKey(103), Principal = principals0[2] },
                new ComparableIntStructKeyOptionalDependentShadow { Id = new ComparableIntStructKey(104), Principal = principals0[2] },
                new ComparableIntStructKeyOptionalDependentShadow { Id = new ComparableIntStructKey(105), Principal = principals0[2] },
                new ComparableIntStructKeyOptionalDependentShadow { Id = new ComparableIntStructKey(106) });

            Assert.Equal(10, await context.SaveChangesAsync());
        }

        using (var context = CreateContext())
        {
            await RunQueries(context);

            Validate(
                principals,
                dependents,
                [(0, [0]), (1, [1]), (2, [2, 2, 2]), (3, [])],
                [(0, 0), (1, 1), (2, 2), (3, 2), (4, 2), (5, null)]);

            foreach (var principal in principals)
            {
                principal.Foo = "Mutant!";
            }

            dependents[5].Principal = principals[0];

            context.Entry(dependents[4]).Property("PrincipalId").CurrentValue = null;
            context.Entry(dependents[3]).Property("PrincipalId").CurrentValue = principals[0].Id;
            principals[1].OptionalDependents.Clear();

            context.Remove(dependents[0]);
            principals[0].OptionalDependents.Add(
                new ComparableIntStructKeyOptionalDependentShadow { Id = new ComparableIntStructKey(dependents[0].Id.Id), });

            await context.SaveChangesAsync();
        }

        using (var context = CreateContext())
        {
            await RunQueries(context);

            Validate(
                principals,
                dependents,
                [(0, [0, 3, 5]), (1, []), (2, [2]), (3, [])],
                [(0, 0), (1, null), (2, 2), (3, 0), (4, null), (5, 0)]);
        }

        async Task RunQueries(DbContext context)
        {
            var two = 2;
            var three = new ComparableIntStructKey(3);

            principals =
            [
                await context.Set<ComparableIntStructKeyPrincipalShadow>().Include(e => e.OptionalDependents)
                    .SingleAsync(e => e.Id.Equals(new ComparableIntStructKey(1))),
                await context.Set<ComparableIntStructKeyPrincipalShadow>().Include(e => e.OptionalDependents)
                    .SingleAsync(e => e.Id.Equals(new ComparableIntStructKey(two))),
                await context.Set<ComparableIntStructKeyPrincipalShadow>().Include(e => e.OptionalDependents).SingleAsync(e => e.Id.Equals(three)),
                await context.Set<ComparableIntStructKeyPrincipalShadow>().Include(e => e.OptionalDependents)
                    .SingleAsync(e => e.Id.Equals(new ComparableIntStructKey(4)))
            ];

            var oneOhTwo = 102;
            var oneOhThree = new ComparableIntStructKey(103);
            var oneOhFive = 105;
            var oneOhSix = new ComparableIntStructKey(106);

            dependents =
            [
                await context.Set<ComparableIntStructKeyOptionalDependentShadow>()
                    .SingleAsync(e => e.Id.Equals(new ComparableIntStructKey(101))),
                await context.Set<ComparableIntStructKeyOptionalDependentShadow>()
                    .SingleAsync(e => e.Id.Equals(new ComparableIntStructKey(oneOhTwo))),
                await context.Set<ComparableIntStructKeyOptionalDependentShadow>().SingleAsync(e => e.Id.Equals(oneOhThree)),
                await context.Set<ComparableIntStructKeyOptionalDependentShadow>()
                    .SingleAsync(e => e.Id.Equals(new ComparableIntStructKey(104))),
                await context.Set<ComparableIntStructKeyOptionalDependentShadow>()
                    .SingleAsync(e => e.Id.Equals(new ComparableIntStructKey(oneOhFive))),
                await context.Set<ComparableIntStructKeyOptionalDependentShadow>().SingleAsync(e => e.Id.Equals(oneOhSix))
            ];

            Assert.Same(
                dependents[0], await context.Set<ComparableIntStructKeyOptionalDependentShadow>().FindAsync(new ComparableIntStructKey(101)));
            Assert.Same(
                dependents[1], await context.Set<ComparableIntStructKeyOptionalDependentShadow>().FindAsync(new ComparableIntStructKey(oneOhTwo)));
            Assert.Same(dependents[2], await context.Set<ComparableIntStructKeyOptionalDependentShadow>().FindAsync(oneOhThree));
            Assert.Same(
                dependents[3], await context.FindAsync(typeof(ComparableIntStructKeyOptionalDependentShadow), new ComparableIntStructKey(104)));
            Assert.Same(
                dependents[4],
                await context.FindAsync(typeof(ComparableIntStructKeyOptionalDependentShadow), new ComparableIntStructKey(oneOhFive)));
            Assert.Same(dependents[5], await context.FindAsync(typeof(ComparableIntStructKeyOptionalDependentShadow), oneOhSix));
        }

        void Validate(
            ComparableIntStructKeyPrincipalShadow[] principals,
            ComparableIntStructKeyOptionalDependentShadow[] dependents,
            (int, int[])[] expectedPrincipalToDependents,
            (int, int?)[] expectedDependentToPrincipals)
        {
            Assert.Equal(4, principals.Length);
            for (var i = 0; i < 4; i++)
            {
                Assert.Equal(i + 1, principals[i].Id.Id);
            }

            Assert.Equal(6, dependents.Length);
            for (var i = 0; i < 6; i++)
            {
                Assert.Equal(i + 101, dependents[i].Id.Id);
            }

            foreach (var (dependentIndex, principalIndex) in expectedDependentToPrincipals)
            {
                if (principalIndex.HasValue)
                {
                    Assert.Same(principals[principalIndex.Value], dependents[dependentIndex].Principal);
                }
                else
                {
                    Assert.Null(dependents[dependentIndex].Principal);
                }
            }

            foreach (var (principalIndex, dependentIndexes) in expectedPrincipalToDependents)
            {
                Assert.Equal(dependentIndexes.Length, principals[principalIndex].OptionalDependents.Count);
                foreach (var dependentIndex in dependentIndexes)
                {
                    Assert.Same(principals[principalIndex], dependents[dependentIndex].Principal);
                }
            }
        }
    }

    [ConditionalFact]
    public virtual async Task Can_insert_and_read_back_with_generic_comparable_struct_key_and_optional_dependents_with_shadow_FK()
    {
        GenericComparableIntStructKeyPrincipalShadow[] principals = null;
        GenericComparableIntStructKeyOptionalDependentShadow[] dependents = null;
        using (var context = CreateContext())
        {
            var principals0 = new GenericComparableIntStructKeyPrincipalShadow[]
            {
                new() { Id = new GenericComparableIntStructKey(1), Foo = "X1" },
                new() { Id = new GenericComparableIntStructKey(2), Foo = "X2" },
                new() { Id = new GenericComparableIntStructKey(3), Foo = "X3" },
                new() { Id = new GenericComparableIntStructKey(4), Foo = "X4" }
            };

            context.Set<GenericComparableIntStructKeyPrincipalShadow>().AddRange(principals0);

            context.Set<GenericComparableIntStructKeyOptionalDependentShadow>().AddRange(
                new GenericComparableIntStructKeyOptionalDependentShadow
                {
                    Id = new GenericComparableIntStructKey(101), Principal = principals0[0]
                },
                new GenericComparableIntStructKeyOptionalDependentShadow
                {
                    Id = new GenericComparableIntStructKey(102), Principal = principals0[1]
                },
                new GenericComparableIntStructKeyOptionalDependentShadow
                {
                    Id = new GenericComparableIntStructKey(103), Principal = principals0[2]
                },
                new GenericComparableIntStructKeyOptionalDependentShadow
                {
                    Id = new GenericComparableIntStructKey(104), Principal = principals0[2]
                },
                new GenericComparableIntStructKeyOptionalDependentShadow
                {
                    Id = new GenericComparableIntStructKey(105), Principal = principals0[2]
                },
                new GenericComparableIntStructKeyOptionalDependentShadow { Id = new GenericComparableIntStructKey(106) });

            Assert.Equal(10, await context.SaveChangesAsync());
        }

        using (var context = CreateContext())
        {
            await RunQueries(context);

            Validate(
                principals,
                dependents,
                [(0, [0]), (1, [1]), (2, [2, 2, 2]), (3, [])],
                [(0, 0), (1, 1), (2, 2), (3, 2), (4, 2), (5, null)]);

            foreach (var principal in principals)
            {
                principal.Foo = "Mutant!";
            }

            dependents[5].Principal = principals[0];
            context.Entry(dependents[4]).Property("PrincipalId").CurrentValue = null;
            context.Entry(dependents[3]).Property("PrincipalId").CurrentValue = principals[0].Id;
            principals[1].OptionalDependents.Clear();

            context.Remove(dependents[0]);
            principals[0].OptionalDependents.Add(
                new GenericComparableIntStructKeyOptionalDependentShadow { Id = new GenericComparableIntStructKey(dependents[0].Id.Id), });

            await context.SaveChangesAsync();
        }

        using (var context = CreateContext())
        {
            await RunQueries(context);

            Validate(
                principals,
                dependents,
                [(0, [0, 3, 5]), (1, []), (2, [2]), (3, [])],
                [(0, 0), (1, null), (2, 2), (3, 0), (4, null), (5, 0)]);
        }

        async Task RunQueries(DbContext context)
        {
            var two = 2;
            var three = new GenericComparableIntStructKey(3);

            principals =
            [
                await context.Set<GenericComparableIntStructKeyPrincipalShadow>().Include(e => e.OptionalDependents)
                    .SingleAsync(e => e.Id.Equals(new GenericComparableIntStructKey(1))),
                await context.Set<GenericComparableIntStructKeyPrincipalShadow>().Include(e => e.OptionalDependents)
                    .SingleAsync(e => e.Id.Equals(new GenericComparableIntStructKey(two))),
                await context.Set<GenericComparableIntStructKeyPrincipalShadow>().Include(e => e.OptionalDependents)
                    .SingleAsync(e => e.Id.Equals(three)),
                await context.Set<GenericComparableIntStructKeyPrincipalShadow>().Include(e => e.OptionalDependents)
                    .SingleAsync(e => e.Id.Equals(new GenericComparableIntStructKey(4)))
            ];

            var oneOhTwo = 102;
            var oneOhThree = new GenericComparableIntStructKey(103);
            var oneOhFive = 105;
            var oneOhSix = new GenericComparableIntStructKey(106);

            dependents =
            [
                await context.Set<GenericComparableIntStructKeyOptionalDependentShadow>()
                    .SingleAsync(e => e.Id.Equals(new GenericComparableIntStructKey(101))),
                await context.Set<GenericComparableIntStructKeyOptionalDependentShadow>()
                    .SingleAsync(e => e.Id.Equals(new GenericComparableIntStructKey(oneOhTwo))),
                await context.Set<GenericComparableIntStructKeyOptionalDependentShadow>().SingleAsync(e => e.Id.Equals(oneOhThree)),
                await context.Set<GenericComparableIntStructKeyOptionalDependentShadow>()
                    .SingleAsync(e => e.Id.Equals(new GenericComparableIntStructKey(104))),
                await context.Set<GenericComparableIntStructKeyOptionalDependentShadow>()
                    .SingleAsync(e => e.Id.Equals(new GenericComparableIntStructKey(oneOhFive))),
                await context.Set<GenericComparableIntStructKeyOptionalDependentShadow>().SingleAsync(e => e.Id.Equals(oneOhSix))
            ];

            Assert.Same(
                dependents[0],
                await context.Set<GenericComparableIntStructKeyOptionalDependentShadow>().FindAsync(new GenericComparableIntStructKey(101)));
            Assert.Same(
                dependents[1],
                await context.Set<GenericComparableIntStructKeyOptionalDependentShadow>().FindAsync(new GenericComparableIntStructKey(oneOhTwo)));
            Assert.Same(dependents[2], await context.Set<GenericComparableIntStructKeyOptionalDependentShadow>().FindAsync(oneOhThree));
            Assert.Same(
                dependents[3],
                await context.FindAsync(typeof(GenericComparableIntStructKeyOptionalDependentShadow), new GenericComparableIntStructKey(104)));
            Assert.Same(
                dependents[4],
                await context.FindAsync(
                    typeof(GenericComparableIntStructKeyOptionalDependentShadow), new GenericComparableIntStructKey(oneOhFive)));
            Assert.Same(dependents[5], await context.FindAsync(typeof(GenericComparableIntStructKeyOptionalDependentShadow), oneOhSix));
        }

        void Validate(
            GenericComparableIntStructKeyPrincipalShadow[] principals,
            GenericComparableIntStructKeyOptionalDependentShadow[] dependents,
            (int, int[])[] expectedPrincipalToDependents,
            (int, int?)[] expectedDependentToPrincipals)
        {
            Assert.Equal(4, principals.Length);
            for (var i = 0; i < 4; i++)
            {
                Assert.Equal(i + 1, principals[i].Id.Id);
            }

            Assert.Equal(6, dependents.Length);
            for (var i = 0; i < 6; i++)
            {
                Assert.Equal(i + 101, dependents[i].Id.Id);
            }

            foreach (var (dependentIndex, principalIndex) in expectedDependentToPrincipals)
            {
                if (principalIndex.HasValue)
                {
                    Assert.Same(principals[principalIndex.Value], dependents[dependentIndex].Principal);
                }
                else
                {
                    Assert.Null(dependents[dependentIndex].Principal);
                }
            }

            foreach (var (principalIndex, dependentIndexes) in expectedPrincipalToDependents)
            {
                Assert.Equal(dependentIndexes.Length, principals[principalIndex].OptionalDependents.Count);
                foreach (var dependentIndex in dependentIndexes)
                {
                    Assert.Same(principals[principalIndex], dependents[dependentIndex].Principal);
                }
            }
        }
    }

    [ConditionalFact]
    public virtual async Task Can_insert_and_read_back_with_struct_key_and_required_dependents_with_shadow_FK()
    {
        IntStructKeyPrincipalShadow[] principals = null;
        IntStructKeyRequiredDependentShadow[] dependents = null;
        using (var context = CreateContext())
        {
            var principals0 = new IntStructKeyPrincipalShadow[]
            {
                new() { Id = new IntStructKey(11), Foo = "X1" },
                new() { Id = new IntStructKey(12), Foo = "X2" },
                new() { Id = new IntStructKey(13), Foo = "X3" },
                new() { Id = new IntStructKey(14), Foo = "X4" }
            };

            context.Set<IntStructKeyPrincipalShadow>().AddRange(principals0);

            context.Set<IntStructKeyRequiredDependentShadow>().AddRange(
                new IntStructKeyRequiredDependentShadow { Id = new IntStructKey(111), Principal = principals0[0] },
                new IntStructKeyRequiredDependentShadow { Id = new IntStructKey(112), Principal = principals0[1] },
                new IntStructKeyRequiredDependentShadow { Id = new IntStructKey(113), Principal = principals0[2] },
                new IntStructKeyRequiredDependentShadow { Id = new IntStructKey(114), Principal = principals0[2] },
                new IntStructKeyRequiredDependentShadow { Id = new IntStructKey(115), Principal = principals0[2] },
                new IntStructKeyRequiredDependentShadow { Id = new IntStructKey(116), Principal = principals0[2] });

            Assert.Equal(10, await context.SaveChangesAsync());
        }

        using (var context = CreateContext())
        {
            await RunQueries(context);

            Validate(
                principals,
                dependents,
                [(0, [0]), (1, [1]), (2, [2, 2, 2, 2]), (3, [])],
                [(0, 0), (1, 1), (2, 2), (3, 2), (4, 2), (5, 2)]);

            foreach (var principal in principals)
            {
                principal.Foo = "Mutant!";
            }

            dependents[5].Principal = principals[0];
            context.Remove(dependents[4]);
            context.Entry(dependents[3]).Property("PrincipalId").CurrentValue = principals[0].Id;
            principals[1].RequiredDependents.Clear();

            context.Remove(dependents[0]);
            principals[0].RequiredDependents.Add(
                new IntStructKeyRequiredDependentShadow { Id = new IntStructKey(dependents[0].Id.Id), });

            await context.SaveChangesAsync();
        }

        using (var context = CreateContext())
        {
            await RunQueries(context);

            Validate(
                principals,
                dependents,
                [(0, [0, 3, 5]), (1, []), (2, [2]), (3, [])],
                [(0, 0), (2, 2), (3, 0), (5, 0)]);
        }

        async Task RunQueries(DbContext context)
        {
            var twelve = 12;
            var thirteen = new IntStructKey { Id = 13 };

            principals =
            [
                await context.Set<IntStructKeyPrincipalShadow>().Include(e => e.RequiredDependents)
                    .SingleAsync(e => e.Id.Equals(new IntStructKey { Id = 11 })),
                await context.Set<IntStructKeyPrincipalShadow>().Include(e => e.RequiredDependents)
                    .SingleAsync(e => e.Id.Equals(new IntStructKey { Id = twelve })),
                await context.Set<IntStructKeyPrincipalShadow>().Include(e => e.RequiredDependents).SingleAsync(e => e.Id.Equals(thirteen)),
                await context.Set<IntStructKeyPrincipalShadow>().Include(e => e.RequiredDependents)
                    .SingleAsync(e => e.Id.Equals(new IntStructKey { Id = 14 }))
            ];

            var oneTwelve = 112;
            var oneThirteen = new IntStructKey { Id = 113 };
            var oneFifteeen = 115;
            var oneSixteen = new IntStructKey { Id = 116 };

            dependents =
            [
                await context.Set<IntStructKeyRequiredDependentShadow>().FirstOrDefaultAsync(e => e.Id.Equals(new IntStructKey { Id = 111 })),
                await context.Set<IntStructKeyRequiredDependentShadow>()
                    .FirstOrDefaultAsync(e => e.Id.Equals(new IntStructKey { Id = oneTwelve })),
                await context.Set<IntStructKeyRequiredDependentShadow>().FirstOrDefaultAsync(e => e.Id.Equals(oneThirteen)),
                await context.Set<IntStructKeyRequiredDependentShadow>().FirstOrDefaultAsync(e => e.Id.Equals(new IntStructKey { Id = 114 })),
                await context.Set<IntStructKeyRequiredDependentShadow>()
                    .FirstOrDefaultAsync(e => e.Id.Equals(new IntStructKey { Id = oneFifteeen })),
                await context.Set<IntStructKeyRequiredDependentShadow>().FirstOrDefaultAsync(e => e.Id.Equals(oneSixteen))
            ];

            Assert.Same(dependents[0], await context.Set<IntStructKeyRequiredDependentShadow>().FindAsync(new IntStructKey { Id = 111 }));
            Assert.Same(dependents[1], await context.Set<IntStructKeyRequiredDependentShadow>().FindAsync(new IntStructKey { Id = oneTwelve }));
            Assert.Same(dependents[2], await context.Set<IntStructKeyRequiredDependentShadow>().FindAsync(oneThirteen));
            Assert.Same(dependents[3], await context.FindAsync(typeof(IntStructKeyRequiredDependentShadow), new IntStructKey { Id = 114 }));
            Assert.Same(
                dependents[4], await context.FindAsync(typeof(IntStructKeyRequiredDependentShadow), new IntStructKey { Id = oneFifteeen }));
            Assert.Same(dependents[5], await context.FindAsync(typeof(IntStructKeyRequiredDependentShadow), oneSixteen));
        }

        void Validate(
            IntStructKeyPrincipalShadow[] principals,
            IntStructKeyRequiredDependentShadow[] dependents,
            (int, int[])[] expectedPrincipalToDependents,
            (int, int)[] expectedDependentToPrincipals)
        {
            Assert.Equal(4, principals.Length);
            for (var i = 0; i < 4; i++)
            {
                Assert.Equal(i + 11, principals[i].Id.Id);
            }

            Assert.Equal(6, dependents.Length);
            for (var i = 0; i < 6; i++)
            {
                if (dependents[i] != null)
                {
                    Assert.Equal(i + 111, dependents[i].Id.Id);
                }
            }

            foreach (var (dependentIndex, principalIndex) in expectedDependentToPrincipals)
            {
                Assert.Same(principals[principalIndex], dependents[dependentIndex].Principal);
            }

            foreach (var (principalIndex, dependentIndexes) in expectedPrincipalToDependents)
            {
                Assert.Equal(dependentIndexes.Length, principals[principalIndex].RequiredDependents.Count);
                foreach (var dependentIndex in dependentIndexes)
                {
                    Assert.Same(principals[principalIndex], dependents[dependentIndex].Principal);
                }
            }
        }
    }

    [ConditionalFact]
    public virtual async Task Can_insert_and_read_back_with_comparable_struct_key_and_required_dependents_with_shadow_FK()
    {
        ComparableIntStructKeyPrincipalShadow[] principals = null;
        ComparableIntStructKeyRequiredDependentShadow[] dependents = null;
        using (var context = CreateContext())
        {
            var principals0 = new ComparableIntStructKeyPrincipalShadow[]
            {
                new() { Id = new ComparableIntStructKey(11), Foo = "X1" },
                new() { Id = new ComparableIntStructKey(12), Foo = "X2" },
                new() { Id = new ComparableIntStructKey(13), Foo = "X3" },
                new() { Id = new ComparableIntStructKey(14), Foo = "X4" }
            };

            context.Set<ComparableIntStructKeyPrincipalShadow>().AddRange(principals0);

            context.Set<ComparableIntStructKeyRequiredDependentShadow>().AddRange(
                new ComparableIntStructKeyRequiredDependentShadow { Id = new ComparableIntStructKey(111), Principal = principals0[0] },
                new ComparableIntStructKeyRequiredDependentShadow { Id = new ComparableIntStructKey(112), Principal = principals0[1] },
                new ComparableIntStructKeyRequiredDependentShadow { Id = new ComparableIntStructKey(113), Principal = principals0[2] },
                new ComparableIntStructKeyRequiredDependentShadow { Id = new ComparableIntStructKey(114), Principal = principals0[2] },
                new ComparableIntStructKeyRequiredDependentShadow { Id = new ComparableIntStructKey(115), Principal = principals0[2] },
                new ComparableIntStructKeyRequiredDependentShadow { Id = new ComparableIntStructKey(116), Principal = principals0[2] });

            Assert.Equal(10, await context.SaveChangesAsync());
        }

        using (var context = CreateContext())
        {
            await RunQueries(context);

            Validate(
                principals,
                dependents,
                [(0, [0]), (1, [1]), (2, [2, 2, 2, 2]), (3, [])],
                [(0, 0), (1, 1), (2, 2), (3, 2), (4, 2), (5, 2)]);

            foreach (var principal in principals)
            {
                principal.Foo = "Mutant!";
            }

            dependents[5].Principal = principals[0];
            context.Remove(dependents[4]);
            context.Entry(dependents[3]).Property("PrincipalId").CurrentValue = principals[0].Id;
            principals[1].RequiredDependents.Clear();

            context.Remove(dependents[0]);
            principals[0].RequiredDependents.Add(
                new ComparableIntStructKeyRequiredDependentShadow { Id = new ComparableIntStructKey(dependents[0].Id.Id), });

            await context.SaveChangesAsync();
        }

        using (var context = CreateContext())
        {
            await RunQueries(context);

            Validate(
                principals,
                dependents,
                [(0, [0, 3, 5]), (1, []), (2, [2]), (3, [])],
                [(0, 0), (2, 2), (3, 0), (5, 0)]);
        }

        async Task RunQueries(DbContext context)
        {
            var twelve = 12;
            var thirteen = new ComparableIntStructKey { Id = 13 };

            principals =
            [
                await context.Set<ComparableIntStructKeyPrincipalShadow>().Include(e => e.RequiredDependents)
                    .SingleAsync(e => e.Id.Equals(new ComparableIntStructKey { Id = 11 })),
                await context.Set<ComparableIntStructKeyPrincipalShadow>().Include(e => e.RequiredDependents)
                    .SingleAsync(e => e.Id.Equals(new ComparableIntStructKey { Id = twelve })),
                await context.Set<ComparableIntStructKeyPrincipalShadow>().Include(e => e.RequiredDependents)
                    .SingleAsync(e => e.Id.Equals(thirteen)),
                await context.Set<ComparableIntStructKeyPrincipalShadow>().Include(e => e.RequiredDependents)
                    .SingleAsync(e => e.Id.Equals(new ComparableIntStructKey { Id = 14 }))
            ];

            var oneTwelve = 112;
            var oneThirteen = new ComparableIntStructKey { Id = 113 };
            var oneFifteeen = 115;
            var oneSixteen = new ComparableIntStructKey { Id = 116 };

            dependents =
            [
                await context.Set<ComparableIntStructKeyRequiredDependentShadow>()
                    .FirstOrDefaultAsync(e => e.Id.Equals(new ComparableIntStructKey { Id = 111 })),
                await context.Set<ComparableIntStructKeyRequiredDependentShadow>()
                    .FirstOrDefaultAsync(e => e.Id.Equals(new ComparableIntStructKey { Id = oneTwelve })),
                await context.Set<ComparableIntStructKeyRequiredDependentShadow>().FirstOrDefaultAsync(e => e.Id.Equals(oneThirteen)),
                await context.Set<ComparableIntStructKeyRequiredDependentShadow>()
                    .FirstOrDefaultAsync(e => e.Id.Equals(new ComparableIntStructKey { Id = 114 })),
                await context.Set<ComparableIntStructKeyRequiredDependentShadow>()
                    .FirstOrDefaultAsync(e => e.Id.Equals(new ComparableIntStructKey { Id = oneFifteeen })),
                await context.Set<ComparableIntStructKeyRequiredDependentShadow>().FirstOrDefaultAsync(e => e.Id.Equals(oneSixteen))
            ];

            Assert.Same(
                dependents[0],
                await context.Set<ComparableIntStructKeyRequiredDependentShadow>().FindAsync(new ComparableIntStructKey { Id = 111 }));
            Assert.Same(
                dependents[1],
                await context.Set<ComparableIntStructKeyRequiredDependentShadow>().FindAsync(new ComparableIntStructKey { Id = oneTwelve }));
            Assert.Same(dependents[2], await context.Set<ComparableIntStructKeyRequiredDependentShadow>().FindAsync(oneThirteen));
            Assert.Same(
                dependents[3],
                await context.FindAsync(typeof(ComparableIntStructKeyRequiredDependentShadow), new ComparableIntStructKey { Id = 114 }));
            Assert.Same(
                dependents[4],
                await context.FindAsync(typeof(ComparableIntStructKeyRequiredDependentShadow), new ComparableIntStructKey { Id = oneFifteeen }));
            Assert.Same(dependents[5], await context.FindAsync(typeof(ComparableIntStructKeyRequiredDependentShadow), oneSixteen));
        }

        void Validate(
            ComparableIntStructKeyPrincipalShadow[] principals,
            ComparableIntStructKeyRequiredDependentShadow[] dependents,
            (int, int[])[] expectedPrincipalToDependents,
            (int, int)[] expectedDependentToPrincipals)
        {
            Assert.Equal(4, principals.Length);
            for (var i = 0; i < 4; i++)
            {
                Assert.Equal(i + 11, principals[i].Id.Id);
            }

            Assert.Equal(6, dependents.Length);
            for (var i = 0; i < 6; i++)
            {
                if (dependents[i] != null)
                {
                    Assert.Equal(i + 111, dependents[i].Id.Id);
                }
            }

            foreach (var (dependentIndex, principalIndex) in expectedDependentToPrincipals)
            {
                Assert.Same(principals[principalIndex], dependents[dependentIndex].Principal);
            }

            foreach (var (principalIndex, dependentIndexes) in expectedPrincipalToDependents)
            {
                Assert.Equal(dependentIndexes.Length, principals[principalIndex].RequiredDependents.Count);
                foreach (var dependentIndex in dependentIndexes)
                {
                    Assert.Same(principals[principalIndex], dependents[dependentIndex].Principal);
                }
            }
        }
    }

    [ConditionalFact]
    public virtual async Task Can_insert_and_read_back_with_generic_comparable_struct_key_and_required_dependents_with_shadow_FK()
    {
        GenericComparableIntStructKeyPrincipalShadow[] principals = null;
        GenericComparableIntStructKeyRequiredDependentShadow[] dependents = null;
        using (var context = CreateContext())
        {
            var principals0 = new GenericComparableIntStructKeyPrincipalShadow[]
            {
                new() { Id = new GenericComparableIntStructKey(11), Foo = "X1" },
                new() { Id = new GenericComparableIntStructKey(12), Foo = "X2" },
                new() { Id = new GenericComparableIntStructKey(13), Foo = "X3" },
                new() { Id = new GenericComparableIntStructKey(14), Foo = "X4" }
            };

            context.Set<GenericComparableIntStructKeyPrincipalShadow>().AddRange(principals0);

            context.Set<GenericComparableIntStructKeyRequiredDependentShadow>().AddRange(
                new GenericComparableIntStructKeyRequiredDependentShadow
                {
                    Id = new GenericComparableIntStructKey(111), Principal = principals0[0]
                },
                new GenericComparableIntStructKeyRequiredDependentShadow
                {
                    Id = new GenericComparableIntStructKey(112), Principal = principals0[1]
                },
                new GenericComparableIntStructKeyRequiredDependentShadow
                {
                    Id = new GenericComparableIntStructKey(113), Principal = principals0[2]
                },
                new GenericComparableIntStructKeyRequiredDependentShadow
                {
                    Id = new GenericComparableIntStructKey(114), Principal = principals0[2]
                },
                new GenericComparableIntStructKeyRequiredDependentShadow
                {
                    Id = new GenericComparableIntStructKey(115), Principal = principals0[2]
                },
                new GenericComparableIntStructKeyRequiredDependentShadow
                {
                    Id = new GenericComparableIntStructKey(116), Principal = principals0[2]
                });

            Assert.Equal(10, await context.SaveChangesAsync());
        }

        using (var context = CreateContext())
        {
            await RunQueries(context);

            Validate(
                principals,
                dependents,
                [(0, [0]), (1, [1]), (2, [2, 2, 2, 2]), (3, [])],
                [(0, 0), (1, 1), (2, 2), (3, 2), (4, 2), (5, 2)]);

            foreach (var principal in principals)
            {
                principal.Foo = "Mutant!";
            }

            dependents[5].Principal = principals[0];
            context.Remove(dependents[4]);
            context.Entry(dependents[3]).Property("PrincipalId").CurrentValue = principals[0].Id;
            principals[1].RequiredDependents.Clear();

            context.Remove(dependents[0]);
            principals[0].RequiredDependents.Add(
                new GenericComparableIntStructKeyRequiredDependentShadow { Id = new GenericComparableIntStructKey(dependents[0].Id.Id), });

            await context.SaveChangesAsync();
        }

        using (var context = CreateContext())
        {
            await RunQueries(context);

            Validate(
                principals,
                dependents,
                [(0, [0, 3, 5]), (1, []), (2, [2]), (3, [])],
                [(0, 0), (2, 2), (3, 0), (5, 0)]);
        }

        async Task RunQueries(DbContext context)
        {
            var twelve = 12;
            var thirteen = new GenericComparableIntStructKey { Id = 13 };

            principals =
            [
                await context.Set<GenericComparableIntStructKeyPrincipalShadow>().Include(e => e.RequiredDependents)
                    .SingleAsync(e => e.Id.Equals(new GenericComparableIntStructKey { Id = 11 })),
                await context.Set<GenericComparableIntStructKeyPrincipalShadow>().Include(e => e.RequiredDependents)
                    .SingleAsync(e => e.Id.Equals(new GenericComparableIntStructKey { Id = twelve })),
                await context.Set<GenericComparableIntStructKeyPrincipalShadow>().Include(e => e.RequiredDependents)
                    .SingleAsync(e => e.Id.Equals(thirteen)),
                await context.Set<GenericComparableIntStructKeyPrincipalShadow>().Include(e => e.RequiredDependents)
                    .SingleAsync(e => e.Id.Equals(new GenericComparableIntStructKey { Id = 14 }))
            ];

            var oneTwelve = 112;
            var oneThirteen = new GenericComparableIntStructKey { Id = 113 };
            var oneFifteeen = 115;
            var oneSixteen = new GenericComparableIntStructKey { Id = 116 };

            dependents =
            [
                await context.Set<GenericComparableIntStructKeyRequiredDependentShadow>()
                    .FirstOrDefaultAsync(e => e.Id.Equals(new GenericComparableIntStructKey { Id = 111 })),
                await context.Set<GenericComparableIntStructKeyRequiredDependentShadow>()
                    .FirstOrDefaultAsync(e => e.Id.Equals(new GenericComparableIntStructKey { Id = oneTwelve })),
                await context.Set<GenericComparableIntStructKeyRequiredDependentShadow>().FirstOrDefaultAsync(e => e.Id.Equals(oneThirteen)),
                await context.Set<GenericComparableIntStructKeyRequiredDependentShadow>()
                    .FirstOrDefaultAsync(e => e.Id.Equals(new GenericComparableIntStructKey { Id = 114 })),
                await context.Set<GenericComparableIntStructKeyRequiredDependentShadow>()
                    .FirstOrDefaultAsync(e => e.Id.Equals(new GenericComparableIntStructKey { Id = oneFifteeen })),
                await context.Set<GenericComparableIntStructKeyRequiredDependentShadow>().FirstOrDefaultAsync(e => e.Id.Equals(oneSixteen))
            ];

            Assert.Same(
                dependents[0],
                await context.Set<GenericComparableIntStructKeyRequiredDependentShadow>()
                    .FindAsync(new GenericComparableIntStructKey { Id = 111 }));
            Assert.Same(
                dependents[1],
                await context.Set<GenericComparableIntStructKeyRequiredDependentShadow>()
                    .FindAsync(new GenericComparableIntStructKey { Id = oneTwelve }));
            Assert.Same(dependents[2], await context.Set<GenericComparableIntStructKeyRequiredDependentShadow>().FindAsync(oneThirteen));
            Assert.Same(
                dependents[3],
                await context.FindAsync(
                    typeof(GenericComparableIntStructKeyRequiredDependentShadow), new GenericComparableIntStructKey { Id = 114 }));
            Assert.Same(
                dependents[4],
                await context.FindAsync(
                    typeof(GenericComparableIntStructKeyRequiredDependentShadow),
                    new GenericComparableIntStructKey { Id = oneFifteeen }));
            Assert.Same(dependents[5], await context.FindAsync(typeof(GenericComparableIntStructKeyRequiredDependentShadow), oneSixteen));
        }

        void Validate(
            GenericComparableIntStructKeyPrincipalShadow[] principals,
            GenericComparableIntStructKeyRequiredDependentShadow[] dependents,
            (int, int[])[] expectedPrincipalToDependents,
            (int, int)[] expectedDependentToPrincipals)
        {
            Assert.Equal(4, principals.Length);
            for (var i = 0; i < 4; i++)
            {
                Assert.Equal(i + 11, principals[i].Id.Id);
            }

            Assert.Equal(6, dependents.Length);
            for (var i = 0; i < 6; i++)
            {
                if (dependents[i] != null)
                {
                    Assert.Equal(i + 111, dependents[i].Id.Id);
                }
            }

            foreach (var (dependentIndex, principalIndex) in expectedDependentToPrincipals)
            {
                Assert.Same(principals[principalIndex], dependents[dependentIndex].Principal);
            }

            foreach (var (principalIndex, dependentIndexes) in expectedPrincipalToDependents)
            {
                Assert.Equal(dependentIndexes.Length, principals[principalIndex].RequiredDependents.Count);
                foreach (var dependentIndex in dependentIndexes)
                {
                    Assert.Same(principals[principalIndex], dependents[dependentIndex].Principal);
                }
            }
        }
    }

    [ConditionalFact]
    public virtual async Task Can_insert_and_read_back_with_class_key_and_optional_dependents_with_shadow_FK()
    {
        IntClassKeyPrincipalShadow[] principals = null;
        IntClassKeyOptionalDependentShadow[] dependents = null;
        using (var context = CreateContext())
        {
            var principals0 = new IntClassKeyPrincipalShadow[]
            {
                new() { Id = new IntClassKey(1), Foo = "X1" },
                new() { Id = new IntClassKey(2), Foo = "X2" },
                new() { Id = new IntClassKey(3), Foo = "X3" },
                new() { Id = new IntClassKey(4), Foo = "X4" }
            };

            context.Set<IntClassKeyPrincipalShadow>().AddRange(principals0);

            context.Set<IntClassKeyOptionalDependentShadow>().AddRange(
                new IntClassKeyOptionalDependentShadow { Id = new IntClassKey(101), Principal = principals0[0] },
                new IntClassKeyOptionalDependentShadow { Id = new IntClassKey(102), Principal = principals0[1] },
                new IntClassKeyOptionalDependentShadow { Id = new IntClassKey(103), Principal = principals0[2] },
                new IntClassKeyOptionalDependentShadow { Id = new IntClassKey(104), Principal = principals0[2] },
                new IntClassKeyOptionalDependentShadow { Id = new IntClassKey(105), Principal = principals0[2] },
                new IntClassKeyOptionalDependentShadow { Id = new IntClassKey(106) });

            Assert.Equal(10, await context.SaveChangesAsync());
        }

        using (var context = CreateContext())
        {
            await RunQueries(context);

            Validate(
                principals,
                dependents,
                [(0, [0]), (1, [1]), (2, [2, 2, 2]), (3, [])],
                [(0, 0), (1, 1), (2, 2), (3, 2), (4, 2), (5, null)]);

            foreach (var principal in principals)
            {
                principal.Foo = "Mutant!";
            }

            dependents[5].Principal = principals[0];
            context.Entry(dependents[4]).Property("PrincipalId").CurrentValue = null;
            context.Entry(dependents[3]).Property("PrincipalId").CurrentValue = principals[0].Id;
            principals[1].OptionalDependents.Clear();

            context.Remove(dependents[0]);
            principals[0].OptionalDependents.Add(
                new IntClassKeyOptionalDependentShadow { Id = new IntClassKey(dependents[0].Id.Id), });

            await context.SaveChangesAsync();
        }

        using (var context = CreateContext())
        {
            await RunQueries(context);

            Validate(
                principals,
                dependents,
                [(0, [0, 3, 5]), (1, []), (2, [2]), (3, [])],
                [(0, 0), (1, null), (2, 2), (3, 0), (4, null), (5, 0)]);
        }

        async Task RunQueries(DbContext context)
        {
            var two = 2;
            var three = new IntClassKey(3);

            principals =
            [
                await context.Set<IntClassKeyPrincipalShadow>().Include(e => e.OptionalDependents)
                    .SingleAsync(e => e.Id.Equals(new IntClassKey(1))),
                await context.Set<IntClassKeyPrincipalShadow>().Include(e => e.OptionalDependents)
                    .SingleAsync(e => e.Id.Equals(new IntClassKey(two))),
                await context.Set<IntClassKeyPrincipalShadow>().Include(e => e.OptionalDependents).SingleAsync(e => e.Id.Equals(three)),
                await context.Set<IntClassKeyPrincipalShadow>().Include(e => e.OptionalDependents)
                    .SingleAsync(e => e.Id.Equals(new IntClassKey(4)))
            ];

            var oneOhTwo = 102;
            var oneOhThree = new IntClassKey(103);
            var oneOhFive = 105;
            var oneOhSix = new IntClassKey(106);

            dependents =
            [
                await context.Set<IntClassKeyOptionalDependentShadow>().SingleAsync(e => e.Id.Equals(new IntClassKey(101))),
                await context.Set<IntClassKeyOptionalDependentShadow>().SingleAsync(e => e.Id.Equals(new IntClassKey(oneOhTwo))),
                await context.Set<IntClassKeyOptionalDependentShadow>().SingleAsync(e => e.Id.Equals(oneOhThree)),
                await context.Set<IntClassKeyOptionalDependentShadow>().SingleAsync(e => e.Id == new IntClassKey(104)),
                await context.Set<IntClassKeyOptionalDependentShadow>().SingleAsync(e => e.Id == new IntClassKey(oneOhFive)),
                await context.Set<IntClassKeyOptionalDependentShadow>().SingleAsync(e => e.Id == oneOhSix)
            ];

            Assert.Same(dependents[0], await context.Set<IntClassKeyOptionalDependentShadow>().FindAsync(new IntClassKey(101)));
            Assert.Same(dependents[1], await context.Set<IntClassKeyOptionalDependentShadow>().FindAsync(new IntClassKey(oneOhTwo)));
            Assert.Same(dependents[2], await context.Set<IntClassKeyOptionalDependentShadow>().FindAsync(oneOhThree));
            Assert.Same(dependents[3], await context.FindAsync<IntClassKeyOptionalDependentShadow>(new IntClassKey(104)));
            Assert.Same(dependents[4], await context.FindAsync<IntClassKeyOptionalDependentShadow>(new IntClassKey(oneOhFive)));
            Assert.Same(dependents[5], await context.FindAsync<IntClassKeyOptionalDependentShadow>(oneOhSix));
        }

        void Validate(
            IntClassKeyPrincipalShadow[] principals,
            IntClassKeyOptionalDependentShadow[] dependents,
            (int, int[])[] expectedPrincipalToDependents,
            (int, int?)[] expectedDependentToPrincipals)
        {
            Assert.Equal(4, principals.Length);
            for (var i = 0; i < 4; i++)
            {
                Assert.Equal(i + 1, principals[i].Id.Id);
            }

            Assert.Equal(6, dependents.Length);
            for (var i = 0; i < 6; i++)
            {
                Assert.Equal(i + 101, dependents[i].Id.Id);
            }

            foreach (var (dependentIndex, principalIndex) in expectedDependentToPrincipals)
            {
                if (principalIndex.HasValue)
                {
                    Assert.Same(principals[principalIndex.Value], dependents[dependentIndex].Principal);
                }
                else
                {
                    Assert.Null(dependents[dependentIndex].Principal);
                }
            }

            foreach (var (principalIndex, dependentIndexes) in expectedPrincipalToDependents)
            {
                Assert.Equal(dependentIndexes.Length, principals[principalIndex].OptionalDependents.Count);
                foreach (var dependentIndex in dependentIndexes)
                {
                    Assert.Same(principals[principalIndex], dependents[dependentIndex].Principal);
                }
            }
        }
    }

    [ConditionalFact]
    public virtual async Task Can_insert_and_read_back_with_bare_class_key_and_optional_dependents_with_shadow_FK()
    {
        BareIntClassKeyPrincipalShadow[] principals = null;
        BareIntClassKeyOptionalDependentShadow[] dependents = null;
        using (var context = CreateContext())
        {
            var principals0 = new BareIntClassKeyPrincipalShadow[]
            {
                new() { Id = new BareIntClassKey(1), Foo = "X1" },
                new() { Id = new BareIntClassKey(2), Foo = "X2" },
                new() { Id = new BareIntClassKey(3), Foo = "X3" },
                new() { Id = new BareIntClassKey(4), Foo = "X4" }
            };

            context.Set<BareIntClassKeyPrincipalShadow>().AddRange(principals0);

            context.Set<BareIntClassKeyOptionalDependentShadow>().AddRange(
                new BareIntClassKeyOptionalDependentShadow { Id = new BareIntClassKey(101), Principal = principals0[0] },
                new BareIntClassKeyOptionalDependentShadow { Id = new BareIntClassKey(102), Principal = principals0[1] },
                new BareIntClassKeyOptionalDependentShadow { Id = new BareIntClassKey(103), Principal = principals0[2] },
                new BareIntClassKeyOptionalDependentShadow { Id = new BareIntClassKey(104), Principal = principals0[2] },
                new BareIntClassKeyOptionalDependentShadow { Id = new BareIntClassKey(105), Principal = principals0[2] },
                new BareIntClassKeyOptionalDependentShadow { Id = new BareIntClassKey(106) });

            Assert.Equal(10, await context.SaveChangesAsync());
        }

        using (var context = CreateContext())
        {
            await RunQueries(context);

            Validate(
                principals,
                dependents,
                [(0, [0]), (1, [1]), (2, [2, 2, 2]), (3, [])],
                [(0, 0), (1, 1), (2, 2), (3, 2), (4, 2), (5, null)]);

            foreach (var principal in principals)
            {
                principal.Foo = "Mutant!";
            }

            dependents[5].Principal = principals[0];
            context.Entry(dependents[4]).Property("PrincipalId").CurrentValue = null;
            context.Entry(dependents[3]).Property("PrincipalId").CurrentValue = principals[0].Id;
            principals[1].OptionalDependents.Clear();

            context.Remove(dependents[0]);
            principals[0].OptionalDependents.Add(
                new BareIntClassKeyOptionalDependentShadow { Id = new BareIntClassKey(dependents[0].Id.Id), });

            await context.SaveChangesAsync();
        }

        using (var context = CreateContext())
        {
            await RunQueries(context);

            Validate(
                principals,
                dependents,
                [(0, [0, 3, 5]), (1, []), (2, [2]), (3, [])],
                [(0, 0), (1, null), (2, 2), (3, 0), (4, null), (5, 0)]);
        }

        async Task RunQueries(DbContext context)
        {
            var two = 2;
            var three = new BareIntClassKey(3);

            principals =
            [
                await context.Set<BareIntClassKeyPrincipalShadow>().Include(e => e.OptionalDependents)
                    .SingleAsync(e => e.Id.Equals(new BareIntClassKey(1))),
                await context.Set<BareIntClassKeyPrincipalShadow>().Include(e => e.OptionalDependents)
                    .SingleAsync(e => e.Id.Equals(new BareIntClassKey(two))),
                await context.Set<BareIntClassKeyPrincipalShadow>().Include(e => e.OptionalDependents).SingleAsync(e => e.Id.Equals(three)),
                await context.Set<BareIntClassKeyPrincipalShadow>().Include(e => e.OptionalDependents)
                    .SingleAsync(e => e.Id.Equals(new BareIntClassKey(4)))
            ];

            var oneOhTwo = 102;
            var oneOhThree = new BareIntClassKey(103);
            var oneOhFive = 105;
            var oneOhSix = new BareIntClassKey(106);

            dependents =
            [
                await context.Set<BareIntClassKeyOptionalDependentShadow>().SingleAsync(e => e.Id.Equals(new BareIntClassKey(101))),
                await context.Set<BareIntClassKeyOptionalDependentShadow>().SingleAsync(e => e.Id.Equals(new BareIntClassKey(oneOhTwo))),
                await context.Set<BareIntClassKeyOptionalDependentShadow>().SingleAsync(e => e.Id.Equals(oneOhThree)),
                await context.Set<BareIntClassKeyOptionalDependentShadow>().SingleAsync(e => e.Id == new BareIntClassKey(104)),
                await context.Set<BareIntClassKeyOptionalDependentShadow>().SingleAsync(e => e.Id == new BareIntClassKey(oneOhFive)),
                await context.Set<BareIntClassKeyOptionalDependentShadow>().SingleAsync(e => e.Id == oneOhSix)
            ];

            Assert.Same(dependents[0], await context.Set<BareIntClassKeyOptionalDependentShadow>().FindAsync(new BareIntClassKey(101)));
            Assert.Same(dependents[1], await context.Set<BareIntClassKeyOptionalDependentShadow>().FindAsync(new BareIntClassKey(oneOhTwo)));
            Assert.Same(dependents[2], await context.Set<BareIntClassKeyOptionalDependentShadow>().FindAsync(oneOhThree));
            Assert.Same(dependents[3], await context.FindAsync<BareIntClassKeyOptionalDependentShadow>(new BareIntClassKey(104)));
            Assert.Same(dependents[4], await context.FindAsync<BareIntClassKeyOptionalDependentShadow>(new BareIntClassKey(oneOhFive)));
            Assert.Same(dependents[5], await context.FindAsync<BareIntClassKeyOptionalDependentShadow>(oneOhSix));
        }

        void Validate(
            BareIntClassKeyPrincipalShadow[] principals,
            BareIntClassKeyOptionalDependentShadow[] dependents,
            (int, int[])[] expectedPrincipalToDependents,
            (int, int?)[] expectedDependentToPrincipals)
        {
            Assert.Equal(4, principals.Length);
            for (var i = 0; i < 4; i++)
            {
                Assert.Equal(i + 1, principals[i].Id.Id);
            }

            Assert.Equal(6, dependents.Length);
            for (var i = 0; i < 6; i++)
            {
                Assert.Equal(i + 101, dependents[i].Id.Id);
            }

            foreach (var (dependentIndex, principalIndex) in expectedDependentToPrincipals)
            {
                if (principalIndex.HasValue)
                {
                    Assert.Same(principals[principalIndex.Value], dependents[dependentIndex].Principal);
                }
                else
                {
                    Assert.Null(dependents[dependentIndex].Principal);
                }
            }

            foreach (var (principalIndex, dependentIndexes) in expectedPrincipalToDependents)
            {
                Assert.Equal(dependentIndexes.Length, principals[principalIndex].OptionalDependents.Count);
                foreach (var dependentIndex in dependentIndexes)
                {
                    Assert.Same(principals[principalIndex], dependents[dependentIndex].Principal);
                }
            }
        }
    }

    [ConditionalFact]
    public virtual async Task Can_insert_and_read_back_with_comparable_class_key_and_optional_dependents_with_shadow_FK()
    {
        ComparableIntClassKeyPrincipalShadow[] principals = null;
        ComparableIntClassKeyOptionalDependentShadow[] dependents = null;
        using (var context = CreateContext())
        {
            var principals0 = new ComparableIntClassKeyPrincipalShadow[]
            {
                new() { Id = new ComparableIntClassKey(1), Foo = "X1" },
                new() { Id = new ComparableIntClassKey(2), Foo = "X2" },
                new() { Id = new ComparableIntClassKey(3), Foo = "X3" },
                new() { Id = new ComparableIntClassKey(4), Foo = "X4" }
            };

            context.Set<ComparableIntClassKeyPrincipalShadow>().AddRange(principals0);

            context.Set<ComparableIntClassKeyOptionalDependentShadow>().AddRange(
                new ComparableIntClassKeyOptionalDependentShadow { Id = new ComparableIntClassKey(101), Principal = principals0[0] },
                new ComparableIntClassKeyOptionalDependentShadow { Id = new ComparableIntClassKey(102), Principal = principals0[1] },
                new ComparableIntClassKeyOptionalDependentShadow { Id = new ComparableIntClassKey(103), Principal = principals0[2] },
                new ComparableIntClassKeyOptionalDependentShadow { Id = new ComparableIntClassKey(104), Principal = principals0[2] },
                new ComparableIntClassKeyOptionalDependentShadow { Id = new ComparableIntClassKey(105), Principal = principals0[2] },
                new ComparableIntClassKeyOptionalDependentShadow { Id = new ComparableIntClassKey(106) });

            Assert.Equal(10, await context.SaveChangesAsync());
        }

        using (var context = CreateContext())
        {
            await RunQueries(context);

            Validate(
                principals,
                dependents,
                [(0, [0]), (1, [1]), (2, [2, 2, 2]), (3, [])],
                [(0, 0), (1, 1), (2, 2), (3, 2), (4, 2), (5, null)]);

            foreach (var principal in principals)
            {
                principal.Foo = "Mutant!";
            }

            dependents[5].Principal = principals[0];
            context.Entry(dependents[4]).Property("PrincipalId").CurrentValue = null;
            context.Entry(dependents[3]).Property("PrincipalId").CurrentValue = principals[0].Id;
            principals[1].OptionalDependents.Clear();

            context.Remove(dependents[0]);
            principals[0].OptionalDependents.Add(
                new ComparableIntClassKeyOptionalDependentShadow { Id = new ComparableIntClassKey(dependents[0].Id.Id), });

            await context.SaveChangesAsync();
        }

        using (var context = CreateContext())
        {
            await RunQueries(context);

            Validate(
                principals,
                dependents,
                [(0, [0, 3, 5]), (1, []), (2, [2]), (3, [])],
                [(0, 0), (1, null), (2, 2), (3, 0), (4, null), (5, 0)]);
        }

        async Task RunQueries(DbContext context)
        {
            var two = 2;
            var three = new ComparableIntClassKey(3);

            principals =
            [
                await context.Set<ComparableIntClassKeyPrincipalShadow>().Include(e => e.OptionalDependents)
                    .SingleAsync(e => e.Id.Equals(new ComparableIntClassKey(1))),
                await context.Set<ComparableIntClassKeyPrincipalShadow>().Include(e => e.OptionalDependents)
                    .SingleAsync(e => e.Id.Equals(new ComparableIntClassKey(two))),
                await context.Set<ComparableIntClassKeyPrincipalShadow>().Include(e => e.OptionalDependents).SingleAsync(e => e.Id.Equals(three)),
                await context.Set<ComparableIntClassKeyPrincipalShadow>().Include(e => e.OptionalDependents)
                    .SingleAsync(e => e.Id.Equals(new ComparableIntClassKey(4)))
            ];

            var oneOhTwo = 102;
            var oneOhThree = new ComparableIntClassKey(103);
            var oneOhFive = 105;
            var oneOhSix = new ComparableIntClassKey(106);

            dependents =
            [
                await context.Set<ComparableIntClassKeyOptionalDependentShadow>()
                    .SingleAsync(e => e.Id.Equals(new ComparableIntClassKey(101))),
                await context.Set<ComparableIntClassKeyOptionalDependentShadow>()
                    .SingleAsync(e => e.Id.Equals(new ComparableIntClassKey(oneOhTwo))),
                await context.Set<ComparableIntClassKeyOptionalDependentShadow>().SingleAsync(e => e.Id.Equals(oneOhThree)),
                await context.Set<ComparableIntClassKeyOptionalDependentShadow>().SingleAsync(e => e.Id == new ComparableIntClassKey(104)),
                await context.Set<ComparableIntClassKeyOptionalDependentShadow>()
                    .SingleAsync(e => e.Id == new ComparableIntClassKey(oneOhFive)),
                await context.Set<ComparableIntClassKeyOptionalDependentShadow>().SingleAsync(e => e.Id == oneOhSix)
            ];

            Assert.Same(
                dependents[0], await context.Set<ComparableIntClassKeyOptionalDependentShadow>().FindAsync(new ComparableIntClassKey(101)));
            Assert.Same(
                dependents[1], await context.Set<ComparableIntClassKeyOptionalDependentShadow>().FindAsync(new ComparableIntClassKey(oneOhTwo)));
            Assert.Same(dependents[2], await context.Set<ComparableIntClassKeyOptionalDependentShadow>().FindAsync(oneOhThree));
            Assert.Same(dependents[3], await context.FindAsync<ComparableIntClassKeyOptionalDependentShadow>(new ComparableIntClassKey(104)));
            Assert.Same(
                dependents[4], await context.FindAsync<ComparableIntClassKeyOptionalDependentShadow>(new ComparableIntClassKey(oneOhFive)));
            Assert.Same(dependents[5], await context.FindAsync<ComparableIntClassKeyOptionalDependentShadow>(oneOhSix));
        }

        void Validate(
            ComparableIntClassKeyPrincipalShadow[] principals,
            ComparableIntClassKeyOptionalDependentShadow[] dependents,
            (int, int[])[] expectedPrincipalToDependents,
            (int, int?)[] expectedDependentToPrincipals)
        {
            Assert.Equal(4, principals.Length);
            for (var i = 0; i < 4; i++)
            {
                Assert.Equal(i + 1, principals[i].Id.Id);
            }

            Assert.Equal(6, dependents.Length);
            for (var i = 0; i < 6; i++)
            {
                Assert.Equal(i + 101, dependents[i].Id.Id);
            }

            foreach (var (dependentIndex, principalIndex) in expectedDependentToPrincipals)
            {
                if (principalIndex.HasValue)
                {
                    Assert.Same(principals[principalIndex.Value], dependents[dependentIndex].Principal);
                }
                else
                {
                    Assert.Null(dependents[dependentIndex].Principal);
                }
            }

            foreach (var (principalIndex, dependentIndexes) in expectedPrincipalToDependents)
            {
                Assert.Equal(dependentIndexes.Length, principals[principalIndex].OptionalDependents.Count);
                foreach (var dependentIndex in dependentIndexes)
                {
                    Assert.Same(principals[principalIndex], dependents[dependentIndex].Principal);
                }
            }
        }
    }

    [ConditionalFact]
    public virtual async Task Can_insert_and_read_back_with_struct_binary_key_and_optional_dependents_with_shadow_FK()
    {
        BytesStructKeyPrincipalShadow[] principals = null;
        BytesStructKeyOptionalDependentShadow[] dependents = null;
        using (var context = CreateContext())
        {
            var principals0 = new BytesStructKeyPrincipalShadow[]
            {
                new() { Id = new BytesStructKey([1]), Foo = "X1" },
                new() { Id = new BytesStructKey([2, 2]), Foo = "X2" },
                new() { Id = new BytesStructKey([3, 3, 3]), Foo = "X3" },
                new() { Id = new BytesStructKey([4, 4, 4, 4]), Foo = "X4" }
            };

            context.Set<BytesStructKeyPrincipalShadow>().AddRange(principals0);

            context.Set<BytesStructKeyOptionalDependentShadow>().AddRange(
                new BytesStructKeyOptionalDependentShadow { Id = new BytesStructKey([101]), Principal = principals0[0] },
                new BytesStructKeyOptionalDependentShadow { Id = new BytesStructKey([102]), Principal = principals0[1] },
                new BytesStructKeyOptionalDependentShadow { Id = new BytesStructKey([103]), Principal = principals0[2] },
                new BytesStructKeyOptionalDependentShadow { Id = new BytesStructKey([104]), Principal = principals0[2] },
                new BytesStructKeyOptionalDependentShadow { Id = new BytesStructKey([105]), Principal = principals0[2] },
                new BytesStructKeyOptionalDependentShadow { Id = new BytesStructKey([106]) });

            Assert.Equal(10, await context.SaveChangesAsync());
        }

        using (var context = CreateContext())
        {
            await RunQueries(context);

            Validate(
                principals,
                dependents,
                [(0, [0]), (1, [1]), (2, [2, 2, 2]), (3, [])],
                [(0, 0), (1, 1), (2, 2), (3, 2), (4, 2), (5, null)]);

            foreach (var principal in principals)
            {
                principal.Foo = "Mutant!";
            }

            dependents[5].Principal = principals[0];
            context.Entry(dependents[4]).Property("PrincipalId").CurrentValue = null;
            context.Entry(dependents[3]).Property("PrincipalId").CurrentValue = principals[0].Id;
            principals[1].OptionalDependents.Clear();

            context.Remove(dependents[0]);
            principals[0].OptionalDependents.Add(
                new BytesStructKeyOptionalDependentShadow { Id = new BytesStructKey(dependents[0].Id.Id), });

            await context.SaveChangesAsync();
        }

        using (var context = CreateContext())
        {
            await RunQueries(context);

            Validate(
                principals,
                dependents,
                [(0, [0, 3, 5]), (1, []), (2, [2]), (3, [])],
                [(0, 0), (1, null), (2, 2), (3, 0), (4, null), (5, 0)]);
        }

        async Task RunQueries(DbContext context)
        {
            var two = new byte[] { 2, 2 };
            var three = new BytesStructKey { Id = [3, 3, 3] };

            principals =
            [
                await context.Set<BytesStructKeyPrincipalShadow>().Include(e => e.OptionalDependents)
                    .SingleAsync(e => e.Id.Equals(new BytesStructKey { Id = new byte[] { 1 } })),
                await context.Set<BytesStructKeyPrincipalShadow>().Include(e => e.OptionalDependents)
                    .SingleAsync(e => e.Id.Equals(new BytesStructKey(two))),
                (await context.Set<BytesStructKeyPrincipalShadow>().Include(e => e.OptionalDependents).Where(e => e.Id.Equals(three)).ToListAsync())
                    .Single(),
                await context.Set<BytesStructKeyPrincipalShadow>().Include(e => e.OptionalDependents).SingleAsync(
                    e => e.Id.Equals(new BytesStructKey { Id = new byte[] { 4, 4, 4, 4 } }))
            ];

            var oneOhTwo = new byte[] { 102 };
            var oneOhThree = new BytesStructKey { Id = [103] };
            var oneOhFive = new byte[] { 105 };
            var oneOhSix = new BytesStructKey { Id = [106] };

            dependents =
            [
                await context.Set<BytesStructKeyOptionalDependentShadow>()
                    .SingleAsync(e => e.Id.Equals(new BytesStructKey { Id = new byte[] { 101 } })),
                await context.Set<BytesStructKeyOptionalDependentShadow>().SingleAsync(e => e.Id.Equals(new BytesStructKey(oneOhTwo))),
                await context.Set<BytesStructKeyOptionalDependentShadow>().SingleAsync(e => e.Id.Equals(oneOhThree)),
                await context.Set<BytesStructKeyOptionalDependentShadow>()
                    .SingleAsync(e => e.Id.Equals(new BytesStructKey { Id = new byte[] { 104 } })),
                await context.Set<BytesStructKeyOptionalDependentShadow>().SingleAsync(e => e.Id.Equals(new BytesStructKey(oneOhFive))),
                await context.Set<BytesStructKeyOptionalDependentShadow>().SingleAsync(e => e.Id.Equals(oneOhSix))
            ];

            Assert.Same(
                dependents[0],
                await context.Set<BytesStructKeyOptionalDependentShadow>().FindAsync(new BytesStructKey { Id = [101] }));
            Assert.Same(dependents[1], await context.Set<BytesStructKeyOptionalDependentShadow>().FindAsync(new BytesStructKey(oneOhTwo)));
            Assert.Same(dependents[2], await context.Set<BytesStructKeyOptionalDependentShadow>().FindAsync(oneOhThree));
            Assert.Same(
                dependents[3],
                await context.FindAsync(typeof(BytesStructKeyOptionalDependentShadow), new BytesStructKey { Id = [104] }));
            Assert.Same(dependents[4], await context.FindAsync(typeof(BytesStructKeyOptionalDependentShadow), new BytesStructKey(oneOhFive)));
            Assert.Same(dependents[5], await context.FindAsync(typeof(BytesStructKeyOptionalDependentShadow), oneOhSix));
        }

        void Validate(
            BytesStructKeyPrincipalShadow[] principals,
            BytesStructKeyOptionalDependentShadow[] dependents,
            (int, int[])[] expectedPrincipalToDependents,
            (int, int?)[] expectedDependentToPrincipals)
        {
            Assert.Equal(4, principals.Length);
            Assert.Equal(new byte[] { 1 }, principals[0].Id.Id);
            Assert.Equal(new byte[] { 2, 2 }, principals[1].Id.Id);
            Assert.Equal(new byte[] { 3, 3, 3 }, principals[2].Id.Id);
            Assert.Equal(new byte[] { 4, 4, 4, 4 }, principals[3].Id.Id);

            Assert.Equal(6, dependents.Length);
            Assert.Equal(new byte[] { 101 }, dependents[0].Id.Id);
            Assert.Equal(new byte[] { 102 }, dependents[1].Id.Id);
            Assert.Equal(new byte[] { 103 }, dependents[2].Id.Id);
            Assert.Equal(new byte[] { 104 }, dependents[3].Id.Id);
            Assert.Equal(new byte[] { 105 }, dependents[4].Id.Id);
            Assert.Equal(new byte[] { 106 }, dependents[5].Id.Id);

            foreach (var (dependentIndex, principalIndex) in expectedDependentToPrincipals)
            {
                if (principalIndex.HasValue)
                {
                    Assert.Same(principals[principalIndex.Value], dependents[dependentIndex].Principal);
                }
                else
                {
                    Assert.Null(dependents[dependentIndex].Principal);
                }
            }

            foreach (var (principalIndex, dependentIndexes) in expectedPrincipalToDependents)
            {
                Assert.Equal(dependentIndexes.Length, principals[principalIndex].OptionalDependents.Count);
                foreach (var dependentIndex in dependentIndexes)
                {
                    Assert.Same(principals[principalIndex], dependents[dependentIndex].Principal);
                }
            }
        }
    }

    [ConditionalFact]
    public virtual async Task Can_insert_and_read_back_with_structural_struct_binary_key_and_optional_dependents_with_shadow_FK()
    {
        StructuralComparableBytesStructKeyPrincipalShadow[] principals = null;
        StructuralComparableBytesStructKeyOptionalDependentShadow[] dependents = null;
        using (var context = CreateContext())
        {
            var principals0 = new StructuralComparableBytesStructKeyPrincipalShadow[]
            {
                new() { Id = new StructuralComparableBytesStructKey([1]), Foo = "X1" },
                new() { Id = new StructuralComparableBytesStructKey([2, 2]), Foo = "X2" },
                new() { Id = new StructuralComparableBytesStructKey([3, 3, 3]), Foo = "X3" },
                new() { Id = new StructuralComparableBytesStructKey([4, 4, 4, 4]), Foo = "X4" }
            };

            context.Set<StructuralComparableBytesStructKeyPrincipalShadow>().AddRange(principals0);

            context.Set<StructuralComparableBytesStructKeyOptionalDependentShadow>().AddRange(
                new StructuralComparableBytesStructKeyOptionalDependentShadow
                {
                    Id = new StructuralComparableBytesStructKey([101]), Principal = principals0[0]
                },
                new StructuralComparableBytesStructKeyOptionalDependentShadow
                {
                    Id = new StructuralComparableBytesStructKey([102]), Principal = principals0[1]
                },
                new StructuralComparableBytesStructKeyOptionalDependentShadow
                {
                    Id = new StructuralComparableBytesStructKey([103]), Principal = principals0[2]
                },
                new StructuralComparableBytesStructKeyOptionalDependentShadow
                {
                    Id = new StructuralComparableBytesStructKey([104]), Principal = principals0[2]
                },
                new StructuralComparableBytesStructKeyOptionalDependentShadow
                {
                    Id = new StructuralComparableBytesStructKey([105]), Principal = principals0[2]
                },
                new StructuralComparableBytesStructKeyOptionalDependentShadow
                {
                    Id = new StructuralComparableBytesStructKey([106])
                });

            Assert.Equal(10, await context.SaveChangesAsync());
        }

        using (var context = CreateContext())
        {
            await RunQueries(context);

            Validate(
                principals,
                dependents,
                [(0, [0]), (1, [1]), (2, [2, 2, 2]), (3, [])],
                [(0, 0), (1, 1), (2, 2), (3, 2), (4, 2), (5, null)]);

            foreach (var principal in principals)
            {
                principal.Foo = "Mutant!";
            }

            dependents[5].Principal = principals[0];
            context.Entry(dependents[4]).Property("PrincipalId").CurrentValue = null;
            context.Entry(dependents[3]).Property("PrincipalId").CurrentValue = principals[0].Id;
            principals[1].OptionalDependents.Clear();

            context.Remove(dependents[0]);
            principals[0].OptionalDependents.Add(
                new StructuralComparableBytesStructKeyOptionalDependentShadow
                {
                    Id = new StructuralComparableBytesStructKey(dependents[0].Id.Id),
                });

            await context.SaveChangesAsync();
        }

        using (var context = CreateContext())
        {
            await RunQueries(context);

            Validate(
                principals,
                dependents,
                [(0, [0, 3, 5]), (1, []), (2, [2]), (3, [])],
                [(0, 0), (1, null), (2, 2), (3, 0), (4, null), (5, 0)]);
        }

        async Task RunQueries(DbContext context)
        {
            var two = new byte[] { 2, 2 };
            var three = new StructuralComparableBytesStructKey { Id = [3, 3, 3] };

            principals =
            [
                await context.Set<StructuralComparableBytesStructKeyPrincipalShadow>().Include(e => e.OptionalDependents)
                    .SingleAsync(e => e.Id.Equals(new StructuralComparableBytesStructKey { Id = new byte[] { 1 } })),
                await context.Set<StructuralComparableBytesStructKeyPrincipalShadow>().Include(e => e.OptionalDependents)
                    .SingleAsync(e => e.Id.Equals(new StructuralComparableBytesStructKey(two))),
                (await context.Set<StructuralComparableBytesStructKeyPrincipalShadow>().Include(e => e.OptionalDependents)
                    .Where(e => e.Id.Equals(three)).ToListAsync())
                    .Single(),
                await context.Set<StructuralComparableBytesStructKeyPrincipalShadow>().Include(e => e.OptionalDependents).SingleAsync(
                    e => e.Id.Equals(new StructuralComparableBytesStructKey { Id = new byte[] { 4, 4, 4, 4 } }))
            ];

            var oneOhTwo = new byte[] { 102 };
            var oneOhThree = new StructuralComparableBytesStructKey { Id = [103] };
            var oneOhFive = new byte[] { 105 };
            var oneOhSix = new StructuralComparableBytesStructKey { Id = [106] };

            dependents =
            [
                await context.Set<StructuralComparableBytesStructKeyOptionalDependentShadow>()
                    .SingleAsync(e => e.Id.Equals(new StructuralComparableBytesStructKey { Id = new byte[] { 101 } })),
                await context.Set<StructuralComparableBytesStructKeyOptionalDependentShadow>()
                    .SingleAsync(e => e.Id.Equals(new StructuralComparableBytesStructKey(oneOhTwo))),
                await context.Set<StructuralComparableBytesStructKeyOptionalDependentShadow>().SingleAsync(e => e.Id.Equals(oneOhThree)),
                await context.Set<StructuralComparableBytesStructKeyOptionalDependentShadow>()
                    .SingleAsync(e => e.Id.Equals(new StructuralComparableBytesStructKey { Id = new byte[] { 104 } })),
                await context.Set<StructuralComparableBytesStructKeyOptionalDependentShadow>()
                    .SingleAsync(e => e.Id.Equals(new StructuralComparableBytesStructKey(oneOhFive))),
                await context.Set<StructuralComparableBytesStructKeyOptionalDependentShadow>().SingleAsync(e => e.Id.Equals(oneOhSix))
            ];

            Assert.Same(
                dependents[0],
                await context.Set<StructuralComparableBytesStructKeyOptionalDependentShadow>()
                    .FindAsync(new StructuralComparableBytesStructKey { Id = [101] }));
            Assert.Same(
                dependents[1],
                await context.Set<StructuralComparableBytesStructKeyOptionalDependentShadow>()
                    .FindAsync(new StructuralComparableBytesStructKey(oneOhTwo)));
            Assert.Same(dependents[2], await context.Set<StructuralComparableBytesStructKeyOptionalDependentShadow>().FindAsync(oneOhThree));
            Assert.Same(
                dependents[3],
                await context.FindAsync(
                    typeof(StructuralComparableBytesStructKeyOptionalDependentShadow),
                    new StructuralComparableBytesStructKey { Id = [104] }));
            Assert.Same(
                dependents[4],
                await context.FindAsync(
                    typeof(StructuralComparableBytesStructKeyOptionalDependentShadow),
                    new StructuralComparableBytesStructKey(oneOhFive)));
            Assert.Same(dependents[5], await context.FindAsync(typeof(StructuralComparableBytesStructKeyOptionalDependentShadow), oneOhSix));
        }

        void Validate(
            StructuralComparableBytesStructKeyPrincipalShadow[] principals,
            StructuralComparableBytesStructKeyOptionalDependentShadow[] dependents,
            (int, int[])[] expectedPrincipalToDependents,
            (int, int?)[] expectedDependentToPrincipals)
        {
            Assert.Equal(4, principals.Length);
            Assert.Equal(new byte[] { 1 }, principals[0].Id.Id);
            Assert.Equal(new byte[] { 2, 2 }, principals[1].Id.Id);
            Assert.Equal(new byte[] { 3, 3, 3 }, principals[2].Id.Id);
            Assert.Equal(new byte[] { 4, 4, 4, 4 }, principals[3].Id.Id);

            Assert.Equal(6, dependents.Length);
            Assert.Equal(new byte[] { 101 }, dependents[0].Id.Id);
            Assert.Equal(new byte[] { 102 }, dependents[1].Id.Id);
            Assert.Equal(new byte[] { 103 }, dependents[2].Id.Id);
            Assert.Equal(new byte[] { 104 }, dependents[3].Id.Id);
            Assert.Equal(new byte[] { 105 }, dependents[4].Id.Id);
            Assert.Equal(new byte[] { 106 }, dependents[5].Id.Id);

            foreach (var (dependentIndex, principalIndex) in expectedDependentToPrincipals)
            {
                if (principalIndex.HasValue)
                {
                    Assert.Same(principals[principalIndex.Value], dependents[dependentIndex].Principal);
                }
                else
                {
                    Assert.Null(dependents[dependentIndex].Principal);
                }
            }

            foreach (var (principalIndex, dependentIndexes) in expectedPrincipalToDependents)
            {
                Assert.Equal(dependentIndexes.Length, principals[principalIndex].OptionalDependents.Count);
                foreach (var dependentIndex in dependentIndexes)
                {
                    Assert.Same(principals[principalIndex], dependents[dependentIndex].Principal);
                }
            }
        }
    }

    [ConditionalFact]
    public virtual async Task Can_insert_and_read_back_with_comparable_struct_binary_key_and_optional_dependents_with_shadow_FK()
    {
        ComparableBytesStructKeyPrincipalShadow[] principals = null;
        ComparableBytesStructKeyOptionalDependentShadow[] dependents = null;
        using (var context = CreateContext())
        {
            var principals0 = new ComparableBytesStructKeyPrincipalShadow[]
            {
                new() { Id = new ComparableBytesStructKey([1]), Foo = "X1" },
                new() { Id = new ComparableBytesStructKey([2, 2]), Foo = "X2" },
                new() { Id = new ComparableBytesStructKey([3, 3, 3]), Foo = "X3" },
                new() { Id = new ComparableBytesStructKey([4, 4, 4, 4]), Foo = "X4" }
            };

            context.Set<ComparableBytesStructKeyPrincipalShadow>().AddRange(principals0);

            context.Set<ComparableBytesStructKeyOptionalDependentShadow>().AddRange(
                new ComparableBytesStructKeyOptionalDependentShadow
                {
                    Id = new ComparableBytesStructKey([101]), Principal = principals0[0]
                },
                new ComparableBytesStructKeyOptionalDependentShadow
                {
                    Id = new ComparableBytesStructKey([102]), Principal = principals0[1]
                },
                new ComparableBytesStructKeyOptionalDependentShadow
                {
                    Id = new ComparableBytesStructKey([103]), Principal = principals0[2]
                },
                new ComparableBytesStructKeyOptionalDependentShadow
                {
                    Id = new ComparableBytesStructKey([104]), Principal = principals0[2]
                },
                new ComparableBytesStructKeyOptionalDependentShadow
                {
                    Id = new ComparableBytesStructKey([105]), Principal = principals0[2]
                },
                new ComparableBytesStructKeyOptionalDependentShadow { Id = new ComparableBytesStructKey([106]) });

            Assert.Equal(10, await context.SaveChangesAsync());
        }

        using (var context = CreateContext())
        {
            await RunQueries(context);

            Validate(
                principals,
                dependents,
                [(0, [0]), (1, [1]), (2, [2, 2, 2]), (3, [])],
                [(0, 0), (1, 1), (2, 2), (3, 2), (4, 2), (5, null)]);

            foreach (var principal in principals)
            {
                principal.Foo = "Mutant!";
            }

            dependents[5].Principal = principals[0];
            context.Entry(dependents[4]).Property("PrincipalId").CurrentValue = null;
            context.Entry(dependents[3]).Property("PrincipalId").CurrentValue = principals[0].Id;
            principals[1].OptionalDependents.Clear();

            context.Remove(dependents[0]);
            principals[0].OptionalDependents.Add(
                new ComparableBytesStructKeyOptionalDependentShadow { Id = new ComparableBytesStructKey(dependents[0].Id.Id), });

            await context.SaveChangesAsync();
        }

        using (var context = CreateContext())
        {
            await RunQueries(context);

            Validate(
                principals,
                dependents,
                [(0, [0, 3, 5]), (1, []), (2, [2]), (3, [])],
                [(0, 0), (1, null), (2, 2), (3, 0), (4, null), (5, 0)]);
        }

        async Task RunQueries(DbContext context)
        {
            var two = new byte[] { 2, 2 };
            var three = new ComparableBytesStructKey { Id = [3, 3, 3] };

            principals =
            [
                await context.Set<ComparableBytesStructKeyPrincipalShadow>().Include(e => e.OptionalDependents)
                    .SingleAsync(e => e.Id.Equals(new ComparableBytesStructKey { Id = new byte[] { 1 } })),
                await context.Set<ComparableBytesStructKeyPrincipalShadow>().Include(e => e.OptionalDependents)
                    .SingleAsync(e => e.Id.Equals(new ComparableBytesStructKey(two))),
                (await context.Set<ComparableBytesStructKeyPrincipalShadow>().Include(e => e.OptionalDependents).Where(e => e.Id.Equals(three))
                    .ToListAsync())
                    .Single(),
                await context.Set<ComparableBytesStructKeyPrincipalShadow>().Include(e => e.OptionalDependents).SingleAsync(
                    e => e.Id.Equals(new ComparableBytesStructKey { Id = new byte[] { 4, 4, 4, 4 } }))
            ];

            var oneOhTwo = new byte[] { 102 };
            var oneOhThree = new ComparableBytesStructKey { Id = [103] };
            var oneOhFive = new byte[] { 105 };
            var oneOhSix = new ComparableBytesStructKey { Id = [106] };

            dependents =
            [
                await context.Set<ComparableBytesStructKeyOptionalDependentShadow>()
                    .SingleAsync(e => e.Id.Equals(new ComparableBytesStructKey { Id = new byte[] { 101 } })),
                await context.Set<ComparableBytesStructKeyOptionalDependentShadow>()
                    .SingleAsync(e => e.Id.Equals(new ComparableBytesStructKey(oneOhTwo))),
                await context.Set<ComparableBytesStructKeyOptionalDependentShadow>().SingleAsync(e => e.Id.Equals(oneOhThree)),
                await context.Set<ComparableBytesStructKeyOptionalDependentShadow>()
                    .SingleAsync(e => e.Id.Equals(new ComparableBytesStructKey { Id = new byte[] { 104 } })),
                await context.Set<ComparableBytesStructKeyOptionalDependentShadow>()
                    .SingleAsync(e => e.Id.Equals(new ComparableBytesStructKey(oneOhFive))),
                await context.Set<ComparableBytesStructKeyOptionalDependentShadow>().SingleAsync(e => e.Id.Equals(oneOhSix))
            ];

            Assert.Same(
                dependents[0],
                await context.Set<ComparableBytesStructKeyOptionalDependentShadow>()
                    .FindAsync(new ComparableBytesStructKey { Id = [101] }));
            Assert.Same(
                dependents[1],
                await context.Set<ComparableBytesStructKeyOptionalDependentShadow>().FindAsync(new ComparableBytesStructKey(oneOhTwo)));
            Assert.Same(dependents[2], await context.Set<ComparableBytesStructKeyOptionalDependentShadow>().FindAsync(oneOhThree));
            Assert.Same(
                dependents[3],
                await context.FindAsync(
                    typeof(ComparableBytesStructKeyOptionalDependentShadow), new ComparableBytesStructKey { Id = [104] }));
            Assert.Same(
                dependents[4],
                await context.FindAsync(typeof(ComparableBytesStructKeyOptionalDependentShadow), new ComparableBytesStructKey(oneOhFive)));
            Assert.Same(dependents[5], await context.FindAsync(typeof(ComparableBytesStructKeyOptionalDependentShadow), oneOhSix));
        }

        void Validate(
            ComparableBytesStructKeyPrincipalShadow[] principals,
            ComparableBytesStructKeyOptionalDependentShadow[] dependents,
            (int, int[])[] expectedPrincipalToDependents,
            (int, int?)[] expectedDependentToPrincipals)
        {
            Assert.Equal(4, principals.Length);
            Assert.Equal(new byte[] { 1 }, principals[0].Id.Id);
            Assert.Equal(new byte[] { 2, 2 }, principals[1].Id.Id);
            Assert.Equal(new byte[] { 3, 3, 3 }, principals[2].Id.Id);
            Assert.Equal(new byte[] { 4, 4, 4, 4 }, principals[3].Id.Id);

            Assert.Equal(6, dependents.Length);
            Assert.Equal(new byte[] { 101 }, dependents[0].Id.Id);
            Assert.Equal(new byte[] { 102 }, dependents[1].Id.Id);
            Assert.Equal(new byte[] { 103 }, dependents[2].Id.Id);
            Assert.Equal(new byte[] { 104 }, dependents[3].Id.Id);
            Assert.Equal(new byte[] { 105 }, dependents[4].Id.Id);
            Assert.Equal(new byte[] { 106 }, dependents[5].Id.Id);

            foreach (var (dependentIndex, principalIndex) in expectedDependentToPrincipals)
            {
                if (principalIndex.HasValue)
                {
                    Assert.Same(principals[principalIndex.Value], dependents[dependentIndex].Principal);
                }
                else
                {
                    Assert.Null(dependents[dependentIndex].Principal);
                }
            }

            foreach (var (principalIndex, dependentIndexes) in expectedPrincipalToDependents)
            {
                Assert.Equal(dependentIndexes.Length, principals[principalIndex].OptionalDependents.Count);
                foreach (var dependentIndex in dependentIndexes)
                {
                    Assert.Same(principals[principalIndex], dependents[dependentIndex].Principal);
                }
            }
        }
    }

    [ConditionalFact]
    public virtual async Task Can_insert_and_read_back_with_generic_comparable_struct_binary_key_and_optional_dependents_with_shadow_FK()
    {
        GenericComparableBytesStructKeyPrincipalShadow[] principals = null;
        GenericComparableBytesStructKeyOptionalDependentShadow[] dependents = null;
        using (var context = CreateContext())
        {
            var principals0 = new GenericComparableBytesStructKeyPrincipalShadow[]
            {
                new() { Id = new GenericComparableBytesStructKey([1]), Foo = "X1" },
                new() { Id = new GenericComparableBytesStructKey([2, 2]), Foo = "X2" },
                new() { Id = new GenericComparableBytesStructKey([3, 3, 3]), Foo = "X3" },
                new() { Id = new GenericComparableBytesStructKey([4, 4, 4, 4]), Foo = "X4" }
            };

            context.Set<GenericComparableBytesStructKeyPrincipalShadow>().AddRange(principals0);

            context.Set<GenericComparableBytesStructKeyOptionalDependentShadow>().AddRange(
                new GenericComparableBytesStructKeyOptionalDependentShadow
                {
                    Id = new GenericComparableBytesStructKey([101]), Principal = principals0[0]
                },
                new GenericComparableBytesStructKeyOptionalDependentShadow
                {
                    Id = new GenericComparableBytesStructKey([102]), Principal = principals0[1]
                },
                new GenericComparableBytesStructKeyOptionalDependentShadow
                {
                    Id = new GenericComparableBytesStructKey([103]), Principal = principals0[2]
                },
                new GenericComparableBytesStructKeyOptionalDependentShadow
                {
                    Id = new GenericComparableBytesStructKey([104]), Principal = principals0[2]
                },
                new GenericComparableBytesStructKeyOptionalDependentShadow
                {
                    Id = new GenericComparableBytesStructKey([105]), Principal = principals0[2]
                },
                new GenericComparableBytesStructKeyOptionalDependentShadow
                {
                    Id = new GenericComparableBytesStructKey([106])
                });

            Assert.Equal(10, await context.SaveChangesAsync());
        }

        using (var context = CreateContext())
        {
            await RunQueries(context);

            Validate(
                principals,
                dependents,
                [(0, [0]), (1, [1]), (2, [2, 2, 2]), (3, [])],
                [(0, 0), (1, 1), (2, 2), (3, 2), (4, 2), (5, null)]);

            foreach (var principal in principals)
            {
                principal.Foo = "Mutant!";
            }

            dependents[5].Principal = principals[0];
            context.Entry(dependents[4]).Property("PrincipalId").CurrentValue = null;
            context.Entry(dependents[3]).Property("PrincipalId").CurrentValue = principals[0].Id;
            principals[1].OptionalDependents.Clear();

            context.Remove(dependents[0]);
            principals[0].OptionalDependents.Add(
                new GenericComparableBytesStructKeyOptionalDependentShadow
                {
                    Id = new GenericComparableBytesStructKey(dependents[0].Id.Id),
                });

            await context.SaveChangesAsync();
        }

        using (var context = CreateContext())
        {
            await RunQueries(context);

            Validate(
                principals,
                dependents,
                [(0, [0, 3, 5]), (1, []), (2, [2]), (3, [])],
                [(0, 0), (1, null), (2, 2), (3, 0), (4, null), (5, 0)]);
        }

        async Task RunQueries(DbContext context)
        {
            var two = new byte[] { 2, 2 };
            var three = new GenericComparableBytesStructKey { Id = [3, 3, 3] };

            principals =
            [
                await context.Set<GenericComparableBytesStructKeyPrincipalShadow>().Include(e => e.OptionalDependents)
                    .SingleAsync(e => e.Id.Equals(new GenericComparableBytesStructKey { Id = new byte[] { 1 } })),
                await context.Set<GenericComparableBytesStructKeyPrincipalShadow>().Include(e => e.OptionalDependents)
                    .SingleAsync(e => e.Id.Equals(new GenericComparableBytesStructKey(two))),
                (await context.Set<GenericComparableBytesStructKeyPrincipalShadow>().Include(e => e.OptionalDependents)
                    .Where(e => e.Id.Equals(three)).ToListAsync())
                    .Single(),
                await context.Set<GenericComparableBytesStructKeyPrincipalShadow>().Include(e => e.OptionalDependents).SingleAsync(
                    e => e.Id.Equals(new GenericComparableBytesStructKey { Id = new byte[] { 4, 4, 4, 4 } }))
            ];

            var oneOhTwo = new byte[] { 102 };
            var oneOhThree = new GenericComparableBytesStructKey { Id = [103] };
            var oneOhFive = new byte[] { 105 };
            var oneOhSix = new GenericComparableBytesStructKey { Id = [106] };

            dependents =
            [
                await context.Set<GenericComparableBytesStructKeyOptionalDependentShadow>()
                    .SingleAsync(e => e.Id.Equals(new GenericComparableBytesStructKey { Id = new byte[] { 101 } })),
                await context.Set<GenericComparableBytesStructKeyOptionalDependentShadow>()
                    .SingleAsync(e => e.Id.Equals(new GenericComparableBytesStructKey(oneOhTwo))),
                await context.Set<GenericComparableBytesStructKeyOptionalDependentShadow>().SingleAsync(e => e.Id.Equals(oneOhThree)),
                await context.Set<GenericComparableBytesStructKeyOptionalDependentShadow>()
                    .SingleAsync(e => e.Id.Equals(new GenericComparableBytesStructKey { Id = new byte[] { 104 } })),
                await context.Set<GenericComparableBytesStructKeyOptionalDependentShadow>()
                    .SingleAsync(e => e.Id.Equals(new GenericComparableBytesStructKey(oneOhFive))),
                await context.Set<GenericComparableBytesStructKeyOptionalDependentShadow>().SingleAsync(e => e.Id.Equals(oneOhSix))
            ];

            Assert.Same(
                dependents[0],
                await context.Set<GenericComparableBytesStructKeyOptionalDependentShadow>()
                    .FindAsync(new GenericComparableBytesStructKey { Id = [101] }));
            Assert.Same(
                dependents[1],
                await context.Set<GenericComparableBytesStructKeyOptionalDependentShadow>()
                    .FindAsync(new GenericComparableBytesStructKey(oneOhTwo)));
            Assert.Same(dependents[2], await context.Set<GenericComparableBytesStructKeyOptionalDependentShadow>().FindAsync(oneOhThree));
            Assert.Same(
                dependents[3],
                await context.FindAsync(
                    typeof(GenericComparableBytesStructKeyOptionalDependentShadow),
                    new GenericComparableBytesStructKey { Id = [104] }));
            Assert.Same(
                dependents[4],
                await context.FindAsync(
                    typeof(GenericComparableBytesStructKeyOptionalDependentShadow), new GenericComparableBytesStructKey(oneOhFive)));
            Assert.Same(dependents[5], await context.FindAsync(typeof(GenericComparableBytesStructKeyOptionalDependentShadow), oneOhSix));
        }

        void Validate(
            GenericComparableBytesStructKeyPrincipalShadow[] principals,
            GenericComparableBytesStructKeyOptionalDependentShadow[] dependents,
            (int, int[])[] expectedPrincipalToDependents,
            (int, int?)[] expectedDependentToPrincipals)
        {
            Assert.Equal(4, principals.Length);
            Assert.Equal(new byte[] { 1 }, principals[0].Id.Id);
            Assert.Equal(new byte[] { 2, 2 }, principals[1].Id.Id);
            Assert.Equal(new byte[] { 3, 3, 3 }, principals[2].Id.Id);
            Assert.Equal(new byte[] { 4, 4, 4, 4 }, principals[3].Id.Id);

            Assert.Equal(6, dependents.Length);
            Assert.Equal(new byte[] { 101 }, dependents[0].Id.Id);
            Assert.Equal(new byte[] { 102 }, dependents[1].Id.Id);
            Assert.Equal(new byte[] { 103 }, dependents[2].Id.Id);
            Assert.Equal(new byte[] { 104 }, dependents[3].Id.Id);
            Assert.Equal(new byte[] { 105 }, dependents[4].Id.Id);
            Assert.Equal(new byte[] { 106 }, dependents[5].Id.Id);

            foreach (var (dependentIndex, principalIndex) in expectedDependentToPrincipals)
            {
                if (principalIndex.HasValue)
                {
                    Assert.Same(principals[principalIndex.Value], dependents[dependentIndex].Principal);
                }
                else
                {
                    Assert.Null(dependents[dependentIndex].Principal);
                }
            }

            foreach (var (principalIndex, dependentIndexes) in expectedPrincipalToDependents)
            {
                Assert.Equal(dependentIndexes.Length, principals[principalIndex].OptionalDependents.Count);
                foreach (var dependentIndex in dependentIndexes)
                {
                    Assert.Same(principals[principalIndex], dependents[dependentIndex].Principal);
                }
            }
        }
    }

    [ConditionalFact]
    public virtual async Task Can_insert_and_read_back_with_struct_binary_key_and_required_dependents_with_shadow_FK()
    {
        BytesStructKeyPrincipalShadow[] principals = null;
        BytesStructKeyRequiredDependentShadow[] dependents = null;
        using (var context = CreateContext())
        {
            var principals0 = new BytesStructKeyPrincipalShadow[]
            {
                new() { Id = new BytesStructKey([11]), Foo = "X1" },
                new() { Id = new BytesStructKey([12, 12]), Foo = "X2" },
                new() { Id = new BytesStructKey([13, 13, 13]), Foo = "X3" },
                new() { Id = new BytesStructKey([14, 14, 14, 14]), Foo = "X4" }
            };

            context.Set<BytesStructKeyPrincipalShadow>().AddRange(principals0);

            context.Set<BytesStructKeyRequiredDependentShadow>().AddRange(
                new BytesStructKeyRequiredDependentShadow { Id = new BytesStructKey([111]), Principal = principals0[0] },
                new BytesStructKeyRequiredDependentShadow { Id = new BytesStructKey([112]), Principal = principals0[1] },
                new BytesStructKeyRequiredDependentShadow { Id = new BytesStructKey([113]), Principal = principals0[2] },
                new BytesStructKeyRequiredDependentShadow { Id = new BytesStructKey([114]), Principal = principals0[2] },
                new BytesStructKeyRequiredDependentShadow { Id = new BytesStructKey([115]), Principal = principals0[2] },
                new BytesStructKeyRequiredDependentShadow { Id = new BytesStructKey([116]), Principal = principals0[2] });

            Assert.Equal(10, await context.SaveChangesAsync());
        }

        using (var context = CreateContext())
        {
            await RunQueries(context);

            Validate(
                principals,
                dependents,
                [(0, [0]), (1, [1]), (2, [2, 2, 2, 2]), (3, [])],
                [(0, 0), (1, 1), (2, 2), (3, 2), (4, 2), (5, 2)]);

            foreach (var principal in principals)
            {
                principal.Foo = "Mutant!";
            }

            dependents[5].Principal = principals[0];
            context.Remove(dependents[4]);
            context.Entry(dependents[3]).Property("PrincipalId").CurrentValue = principals[0].Id;
            principals[1].RequiredDependents.Clear();

            context.Remove(dependents[0]);
            principals[0].RequiredDependents.Add(
                new BytesStructKeyRequiredDependentShadow { Id = new BytesStructKey(dependents[0].Id.Id), });

            await context.SaveChangesAsync();
        }

        using (var context = CreateContext())
        {
            await RunQueries(context);

            Validate(
                principals,
                dependents,
                [(0, [0, 3, 5]), (1, []), (2, [2]), (3, [])],
                [(0, 0), (2, 2), (3, 0), (5, 0)]);
        }

        async Task RunQueries(DbContext context)
        {
            var twelve = new byte[] { 12, 12 };
            var thirteen = new BytesStructKey { Id = [13, 13, 13] };

            principals =
            [
                await context.Set<BytesStructKeyPrincipalShadow>().Include(e => e.RequiredDependents)
                    .SingleAsync(e => e.Id.Equals(new BytesStructKey { Id = new byte[] { 11 } })),
                await context.Set<BytesStructKeyPrincipalShadow>().Include(e => e.RequiredDependents)
                    .SingleAsync(e => e.Id.Equals(new BytesStructKey { Id = twelve })),
                await context.Set<BytesStructKeyPrincipalShadow>().Include(e => e.RequiredDependents).SingleAsync(e => e.Id.Equals(thirteen)),
                await context.Set<BytesStructKeyPrincipalShadow>().Include(e => e.RequiredDependents).SingleAsync(
                    e => e.Id.Equals(new BytesStructKey { Id = new byte[] { 14, 14, 14, 14 } }))
            ];

            var oneTwelve = new byte[] { 112 };
            var oneThirteen = new BytesStructKey { Id = [113] };
            var oneFifteeen = new byte[] { 115 };
            var oneSixteen = new BytesStructKey { Id = [116] };

            dependents =
            [
                await context.Set<BytesStructKeyRequiredDependentShadow>()
                    .FirstOrDefaultAsync(e => e.Id.Equals(new BytesStructKey { Id = new byte[] { 111 } })),
                await context.Set<BytesStructKeyRequiredDependentShadow>()
                    .FirstOrDefaultAsync(e => e.Id.Equals(new BytesStructKey { Id = oneTwelve })),
                await context.Set<BytesStructKeyRequiredDependentShadow>().FirstOrDefaultAsync(e => e.Id.Equals(oneThirteen)),
                await context.Set<BytesStructKeyRequiredDependentShadow>()
                    .FirstOrDefaultAsync(e => e.Id.Equals(new BytesStructKey { Id = new byte[] { 114 } })),
                await context.Set<BytesStructKeyRequiredDependentShadow>()
                    .FirstOrDefaultAsync(e => e.Id.Equals(new BytesStructKey { Id = oneFifteeen })),
                await context.Set<BytesStructKeyRequiredDependentShadow>().FirstOrDefaultAsync(e => e.Id.Equals(oneSixteen))
            ];

            Assert.Same(
                dependents[0],
                await context.Set<BytesStructKeyRequiredDependentShadow>().FindAsync(new BytesStructKey { Id = [111] }));
            Assert.Same(
                dependents[1], await context.Set<BytesStructKeyRequiredDependentShadow>().FindAsync(new BytesStructKey { Id = oneTwelve }));
            Assert.Same(dependents[2], await context.Set<BytesStructKeyRequiredDependentShadow>().FindAsync(oneThirteen));
            Assert.Same(
                dependents[3],
                await context.FindAsync(typeof(BytesStructKeyRequiredDependentShadow), new BytesStructKey { Id = [114] }));
            Assert.Same(
                dependents[4], await context.FindAsync(typeof(BytesStructKeyRequiredDependentShadow), new BytesStructKey { Id = oneFifteeen }));
            Assert.Same(dependents[5], await context.FindAsync(typeof(BytesStructKeyRequiredDependentShadow), oneSixteen));
        }

        void Validate(
            BytesStructKeyPrincipalShadow[] principals,
            BytesStructKeyRequiredDependentShadow[] dependents,
            (int, int[])[] expectedPrincipalToDependents,
            (int, int)[] expectedDependentToPrincipals)
        {
            Assert.Equal(4, principals.Length);
            Assert.Equal(new byte[] { 11 }, principals[0].Id.Id);
            Assert.Equal(new byte[] { 12, 12 }, principals[1].Id.Id);
            Assert.Equal(new byte[] { 13, 13, 13 }, principals[2].Id.Id);
            Assert.Equal(new byte[] { 14, 14, 14, 14 }, principals[3].Id.Id);

            Assert.Equal(6, dependents.Length);
            Assert.Equal(new byte[] { 111 }, dependents[0].Id.Id);
            if (dependents[1] != null)
            {
                Assert.Equal(new byte[] { 112 }, dependents[1].Id.Id);
            }

            Assert.Equal(new byte[] { 113 }, dependents[2].Id.Id);
            Assert.Equal(new byte[] { 114 }, dependents[3].Id.Id);
            if (dependents[4] != null)
            {
                Assert.Equal(new byte[] { 115 }, dependents[4].Id.Id);
            }

            Assert.Equal(new byte[] { 116 }, dependents[5].Id.Id);

            foreach (var (dependentIndex, principalIndex) in expectedDependentToPrincipals)
            {
                Assert.Same(principals[principalIndex], dependents[dependentIndex].Principal);
            }

            foreach (var (principalIndex, dependentIndexes) in expectedPrincipalToDependents)
            {
                Assert.Equal(dependentIndexes.Length, principals[principalIndex].RequiredDependents.Count);
                foreach (var dependentIndex in dependentIndexes)
                {
                    Assert.Same(principals[principalIndex], dependents[dependentIndex].Principal);
                }
            }
        }
    }

    [ConditionalFact]
    public virtual async Task Can_insert_and_read_back_with_comparable_struct_binary_key_and_required_dependents_with_shadow_FK()
    {
        ComparableBytesStructKeyPrincipalShadow[] principals = null;
        ComparableBytesStructKeyRequiredDependentShadow[] dependents = null;
        using (var context = CreateContext())
        {
            var principals0 = new ComparableBytesStructKeyPrincipalShadow[]
            {
                new() { Id = new ComparableBytesStructKey([11]), Foo = "X1" },
                new() { Id = new ComparableBytesStructKey([12, 12]), Foo = "X2" },
                new() { Id = new ComparableBytesStructKey([13, 13, 13]), Foo = "X3" },
                new() { Id = new ComparableBytesStructKey([14, 14, 14, 14]), Foo = "X4" }
            };

            context.Set<ComparableBytesStructKeyPrincipalShadow>().AddRange(principals0);

            context.Set<ComparableBytesStructKeyRequiredDependentShadow>().AddRange(
                new ComparableBytesStructKeyRequiredDependentShadow
                {
                    Id = new ComparableBytesStructKey([111]), Principal = principals0[0]
                },
                new ComparableBytesStructKeyRequiredDependentShadow
                {
                    Id = new ComparableBytesStructKey([112]), Principal = principals0[1]
                },
                new ComparableBytesStructKeyRequiredDependentShadow
                {
                    Id = new ComparableBytesStructKey([113]), Principal = principals0[2]
                },
                new ComparableBytesStructKeyRequiredDependentShadow
                {
                    Id = new ComparableBytesStructKey([114]), Principal = principals0[2]
                },
                new ComparableBytesStructKeyRequiredDependentShadow
                {
                    Id = new ComparableBytesStructKey([115]), Principal = principals0[2]
                },
                new ComparableBytesStructKeyRequiredDependentShadow
                {
                    Id = new ComparableBytesStructKey([116]), Principal = principals0[2]
                });

            Assert.Equal(10, await context.SaveChangesAsync());
        }

        using (var context = CreateContext())
        {
            await RunQueries(context);

            Validate(
                principals,
                dependents,
                [(0, [0]), (1, [1]), (2, [2, 2, 2, 2]), (3, [])],
                [(0, 0), (1, 1), (2, 2), (3, 2), (4, 2), (5, 2)]);

            foreach (var principal in principals)
            {
                principal.Foo = "Mutant!";
            }

            dependents[5].Principal = principals[0];
            context.Remove(dependents[4]);
            context.Entry(dependents[3]).Property("PrincipalId").CurrentValue = principals[0].Id;
            principals[1].RequiredDependents.Clear();

            context.Remove(dependents[0]);
            principals[0].RequiredDependents.Add(
                new ComparableBytesStructKeyRequiredDependentShadow { Id = new ComparableBytesStructKey(dependents[0].Id.Id), });

            await context.SaveChangesAsync();
        }

        using (var context = CreateContext())
        {
            await RunQueries(context);

            Validate(
                principals,
                dependents,
                [(0, [0, 3, 5]), (1, []), (2, [2]), (3, [])],
                [(0, 0), (2, 2), (3, 0), (5, 0)]);
        }

        async Task RunQueries(DbContext context)
        {
            var twelve = new byte[] { 12, 12 };
            var thirteen = new ComparableBytesStructKey { Id = [13, 13, 13] };

            principals =
            [
                await context.Set<ComparableBytesStructKeyPrincipalShadow>().Include(e => e.RequiredDependents)
                    .SingleAsync(e => e.Id.Equals(new ComparableBytesStructKey { Id = new byte[] { 11 } })),
                await context.Set<ComparableBytesStructKeyPrincipalShadow>().Include(e => e.RequiredDependents)
                    .SingleAsync(e => e.Id.Equals(new ComparableBytesStructKey { Id = twelve })),
                await context.Set<ComparableBytesStructKeyPrincipalShadow>().Include(e => e.RequiredDependents)
                    .SingleAsync(e => e.Id.Equals(thirteen)),
                await context.Set<ComparableBytesStructKeyPrincipalShadow>().Include(e => e.RequiredDependents).SingleAsync(
                    e => e.Id.Equals(new ComparableBytesStructKey { Id = new byte[] { 14, 14, 14, 14 } }))
            ];

            var oneTwelve = new byte[] { 112 };
            var oneThirteen = new ComparableBytesStructKey { Id = [113] };
            var oneFifteeen = new byte[] { 115 };
            var oneSixteen = new ComparableBytesStructKey { Id = [116] };

            dependents =
            [
                await context.Set<ComparableBytesStructKeyRequiredDependentShadow>()
                    .FirstOrDefaultAsync(e => e.Id.Equals(new ComparableBytesStructKey { Id = new byte[] { 111 } })),
                await context.Set<ComparableBytesStructKeyRequiredDependentShadow>()
                    .FirstOrDefaultAsync(e => e.Id.Equals(new ComparableBytesStructKey { Id = oneTwelve })),
                await context.Set<ComparableBytesStructKeyRequiredDependentShadow>().FirstOrDefaultAsync(e => e.Id.Equals(oneThirteen)),
                await context.Set<ComparableBytesStructKeyRequiredDependentShadow>()
                    .FirstOrDefaultAsync(e => e.Id.Equals(new ComparableBytesStructKey { Id = new byte[] { 114 } })),
                await context.Set<ComparableBytesStructKeyRequiredDependentShadow>()
                    .FirstOrDefaultAsync(e => e.Id.Equals(new ComparableBytesStructKey { Id = oneFifteeen })),
                await context.Set<ComparableBytesStructKeyRequiredDependentShadow>().FirstOrDefaultAsync(e => e.Id.Equals(oneSixteen))
            ];

            Assert.Same(
                dependents[0],
                await context.Set<ComparableBytesStructKeyRequiredDependentShadow>()
                    .FindAsync(new ComparableBytesStructKey { Id = [111] }));
            Assert.Same(
                dependents[1],
                await context.Set<ComparableBytesStructKeyRequiredDependentShadow>().FindAsync(new ComparableBytesStructKey { Id = oneTwelve }));
            Assert.Same(dependents[2], await context.Set<ComparableBytesStructKeyRequiredDependentShadow>().FindAsync(oneThirteen));
            Assert.Same(
                dependents[3],
                await context.FindAsync(
                    typeof(ComparableBytesStructKeyRequiredDependentShadow), new ComparableBytesStructKey { Id = [114] }));
            Assert.Same(
                dependents[4],
                await context.FindAsync(
                    typeof(ComparableBytesStructKeyRequiredDependentShadow), new ComparableBytesStructKey { Id = oneFifteeen }));
            Assert.Same(dependents[5], await context.FindAsync(typeof(ComparableBytesStructKeyRequiredDependentShadow), oneSixteen));
        }

        void Validate(
            ComparableBytesStructKeyPrincipalShadow[] principals,
            ComparableBytesStructKeyRequiredDependentShadow[] dependents,
            (int, int[])[] expectedPrincipalToDependents,
            (int, int)[] expectedDependentToPrincipals)
        {
            Assert.Equal(4, principals.Length);
            Assert.Equal(new byte[] { 11 }, principals[0].Id.Id);
            Assert.Equal(new byte[] { 12, 12 }, principals[1].Id.Id);
            Assert.Equal(new byte[] { 13, 13, 13 }, principals[2].Id.Id);
            Assert.Equal(new byte[] { 14, 14, 14, 14 }, principals[3].Id.Id);

            Assert.Equal(6, dependents.Length);
            Assert.Equal(new byte[] { 111 }, dependents[0].Id.Id);
            if (dependents[1] != null)
            {
                Assert.Equal(new byte[] { 112 }, dependents[1].Id.Id);
            }

            Assert.Equal(new byte[] { 113 }, dependents[2].Id.Id);
            Assert.Equal(new byte[] { 114 }, dependents[3].Id.Id);
            if (dependents[4] != null)
            {
                Assert.Equal(new byte[] { 115 }, dependents[4].Id.Id);
            }

            Assert.Equal(new byte[] { 116 }, dependents[5].Id.Id);

            foreach (var (dependentIndex, principalIndex) in expectedDependentToPrincipals)
            {
                Assert.Same(principals[principalIndex], dependents[dependentIndex].Principal);
            }

            foreach (var (principalIndex, dependentIndexes) in expectedPrincipalToDependents)
            {
                Assert.Equal(dependentIndexes.Length, principals[principalIndex].RequiredDependents.Count);
                foreach (var dependentIndex in dependentIndexes)
                {
                    Assert.Same(principals[principalIndex], dependents[dependentIndex].Principal);
                }
            }
        }
    }

    [ConditionalFact]
    public virtual async Task Can_insert_and_read_back_with_structural_struct_binary_key_and_required_dependents_with_shadow_FK()
    {
        StructuralComparableBytesStructKeyPrincipalShadow[] principals = null;
        StructuralComparableBytesStructKeyRequiredDependentShadow[] dependents = null;
        using (var context = CreateContext())
        {
            var principals0 = new StructuralComparableBytesStructKeyPrincipalShadow[]
            {
                new() { Id = new StructuralComparableBytesStructKey([11]), Foo = "X1" },
                new() { Id = new StructuralComparableBytesStructKey([12, 12]), Foo = "X2" },
                new() { Id = new StructuralComparableBytesStructKey([13, 13, 13]), Foo = "X3" },
                new() { Id = new StructuralComparableBytesStructKey([14, 14, 14, 14]), Foo = "X4" }
            };

            context.Set<StructuralComparableBytesStructKeyPrincipalShadow>().AddRange(principals0);

            context.Set<StructuralComparableBytesStructKeyRequiredDependentShadow>().AddRange(
                new StructuralComparableBytesStructKeyRequiredDependentShadow
                {
                    Id = new StructuralComparableBytesStructKey([111]), Principal = principals0[0]
                },
                new StructuralComparableBytesStructKeyRequiredDependentShadow
                {
                    Id = new StructuralComparableBytesStructKey([112]), Principal = principals0[1]
                },
                new StructuralComparableBytesStructKeyRequiredDependentShadow
                {
                    Id = new StructuralComparableBytesStructKey([113]), Principal = principals0[2]
                },
                new StructuralComparableBytesStructKeyRequiredDependentShadow
                {
                    Id = new StructuralComparableBytesStructKey([114]), Principal = principals0[2]
                },
                new StructuralComparableBytesStructKeyRequiredDependentShadow
                {
                    Id = new StructuralComparableBytesStructKey([115]), Principal = principals0[2]
                },
                new StructuralComparableBytesStructKeyRequiredDependentShadow
                {
                    Id = new StructuralComparableBytesStructKey([116]), Principal = principals0[2]
                });

            Assert.Equal(10, await context.SaveChangesAsync());
        }

        using (var context = CreateContext())
        {
            await RunQueries(context);

            Validate(
                principals,
                dependents,
                [(0, [0]), (1, [1]), (2, [2, 2, 2, 2]), (3, [])],
                [(0, 0), (1, 1), (2, 2), (3, 2), (4, 2), (5, 2)]);

            foreach (var principal in principals)
            {
                principal.Foo = "Mutant!";
            }

            dependents[5].Principal = principals[0];
            context.Remove(dependents[4]);
            context.Entry(dependents[3]).Property("PrincipalId").CurrentValue = principals[0].Id;
            principals[1].RequiredDependents.Clear();

            context.Remove(dependents[0]);
            principals[0].RequiredDependents.Add(
                new StructuralComparableBytesStructKeyRequiredDependentShadow
                {
                    Id = new StructuralComparableBytesStructKey(dependents[0].Id.Id),
                });

            await context.SaveChangesAsync();
        }

        using (var context = CreateContext())
        {
            await RunQueries(context);

            Validate(
                principals,
                dependents,
                [(0, [0, 3, 5]), (1, []), (2, [2]), (3, [])],
                [(0, 0), (2, 2), (3, 0), (5, 0)]);
        }

        async Task RunQueries(DbContext context)
        {
            var twelve = new byte[] { 12, 12 };
            var thirteen = new StructuralComparableBytesStructKey { Id = [13, 13, 13] };

            principals =
            [
                await context.Set<StructuralComparableBytesStructKeyPrincipalShadow>().Include(e => e.RequiredDependents)
                    .SingleAsync(e => e.Id.Equals(new StructuralComparableBytesStructKey { Id = new byte[] { 11 } })),
                await context.Set<StructuralComparableBytesStructKeyPrincipalShadow>().Include(e => e.RequiredDependents)
                    .SingleAsync(e => e.Id.Equals(new StructuralComparableBytesStructKey { Id = twelve })),
                await context.Set<StructuralComparableBytesStructKeyPrincipalShadow>().Include(e => e.RequiredDependents)
                    .SingleAsync(e => e.Id.Equals(thirteen)),
                await context.Set<StructuralComparableBytesStructKeyPrincipalShadow>().Include(e => e.RequiredDependents).SingleAsync(
                    e => e.Id.Equals(new StructuralComparableBytesStructKey { Id = new byte[] { 14, 14, 14, 14 } }))
            ];

            var oneTwelve = new byte[] { 112 };
            var oneThirteen = new StructuralComparableBytesStructKey { Id = [113] };
            var oneFifteeen = new byte[] { 115 };
            var oneSixteen = new StructuralComparableBytesStructKey { Id = [116] };

            dependents =
            [
                await context.Set<StructuralComparableBytesStructKeyRequiredDependentShadow>()
                    .FirstOrDefaultAsync(e => e.Id.Equals(new StructuralComparableBytesStructKey { Id = new byte[] { 111 } })),
                await context.Set<StructuralComparableBytesStructKeyRequiredDependentShadow>()
                    .FirstOrDefaultAsync(e => e.Id.Equals(new StructuralComparableBytesStructKey { Id = oneTwelve })),
                await context.Set<StructuralComparableBytesStructKeyRequiredDependentShadow>().FirstOrDefaultAsync(e => e.Id.Equals(oneThirteen)),
                await context.Set<StructuralComparableBytesStructKeyRequiredDependentShadow>()
                    .FirstOrDefaultAsync(e => e.Id.Equals(new StructuralComparableBytesStructKey { Id = new byte[] { 114 } })),
                await context.Set<StructuralComparableBytesStructKeyRequiredDependentShadow>()
                    .FirstOrDefaultAsync(e => e.Id.Equals(new StructuralComparableBytesStructKey { Id = oneFifteeen })),
                await context.Set<StructuralComparableBytesStructKeyRequiredDependentShadow>().FirstOrDefaultAsync(e => e.Id.Equals(oneSixteen))
            ];

            Assert.Same(
                dependents[0],
                await context.Set<StructuralComparableBytesStructKeyRequiredDependentShadow>()
                    .FindAsync(new StructuralComparableBytesStructKey { Id = [111] }));
            Assert.Same(
                dependents[1],
                await context.Set<StructuralComparableBytesStructKeyRequiredDependentShadow>()
                    .FindAsync(new StructuralComparableBytesStructKey { Id = oneTwelve }));
            Assert.Same(dependents[2], await context.Set<StructuralComparableBytesStructKeyRequiredDependentShadow>().FindAsync(oneThirteen));
            Assert.Same(
                dependents[3],
                await context.FindAsync(
                    typeof(StructuralComparableBytesStructKeyRequiredDependentShadow),
                    new StructuralComparableBytesStructKey { Id = [114] }));
            Assert.Same(
                dependents[4],
                await context.FindAsync(
                    typeof(StructuralComparableBytesStructKeyRequiredDependentShadow),
                    new StructuralComparableBytesStructKey { Id = oneFifteeen }));
            Assert.Same(dependents[5], await context.FindAsync(typeof(StructuralComparableBytesStructKeyRequiredDependentShadow), oneSixteen));
        }

        void Validate(
            StructuralComparableBytesStructKeyPrincipalShadow[] principals,
            StructuralComparableBytesStructKeyRequiredDependentShadow[] dependents,
            (int, int[])[] expectedPrincipalToDependents,
            (int, int)[] expectedDependentToPrincipals)
        {
            Assert.Equal(4, principals.Length);
            Assert.Equal(new byte[] { 11 }, principals[0].Id.Id);
            Assert.Equal(new byte[] { 12, 12 }, principals[1].Id.Id);
            Assert.Equal(new byte[] { 13, 13, 13 }, principals[2].Id.Id);
            Assert.Equal(new byte[] { 14, 14, 14, 14 }, principals[3].Id.Id);

            Assert.Equal(6, dependents.Length);
            Assert.Equal(new byte[] { 111 }, dependents[0].Id.Id);
            if (dependents[1] != null)
            {
                Assert.Equal(new byte[] { 112 }, dependents[1].Id.Id);
            }

            Assert.Equal(new byte[] { 113 }, dependents[2].Id.Id);
            Assert.Equal(new byte[] { 114 }, dependents[3].Id.Id);
            if (dependents[4] != null)
            {
                Assert.Equal(new byte[] { 115 }, dependents[4].Id.Id);
            }

            Assert.Equal(new byte[] { 116 }, dependents[5].Id.Id);

            foreach (var (dependentIndex, principalIndex) in expectedDependentToPrincipals)
            {
                Assert.Same(principals[principalIndex], dependents[dependentIndex].Principal);
            }

            foreach (var (principalIndex, dependentIndexes) in expectedPrincipalToDependents)
            {
                Assert.Equal(dependentIndexes.Length, principals[principalIndex].RequiredDependents.Count);
                foreach (var dependentIndex in dependentIndexes)
                {
                    Assert.Same(principals[principalIndex], dependents[dependentIndex].Principal);
                }
            }
        }
    }

    [ConditionalFact]
    public virtual async Task Can_insert_and_read_back_with_generic_comparable_struct_binary_key_and_required_dependents_with_shadow_FK()
    {
        GenericComparableBytesStructKeyPrincipalShadow[] principals = null;
        GenericComparableBytesStructKeyRequiredDependentShadow[] dependents = null;
        using (var context = CreateContext())
        {
            var principals0 = new GenericComparableBytesStructKeyPrincipalShadow[]
            {
                new() { Id = new GenericComparableBytesStructKey([11]), Foo = "X1" },
                new() { Id = new GenericComparableBytesStructKey([12, 12]), Foo = "X2" },
                new() { Id = new GenericComparableBytesStructKey([13, 13, 13]), Foo = "X3" },
                new() { Id = new GenericComparableBytesStructKey([14, 14, 14, 14]), Foo = "X4" }
            };

            context.Set<GenericComparableBytesStructKeyPrincipalShadow>().AddRange(principals0);

            context.Set<GenericComparableBytesStructKeyRequiredDependentShadow>().AddRange(
                new GenericComparableBytesStructKeyRequiredDependentShadow
                {
                    Id = new GenericComparableBytesStructKey([111]), Principal = principals0[0]
                },
                new GenericComparableBytesStructKeyRequiredDependentShadow
                {
                    Id = new GenericComparableBytesStructKey([112]), Principal = principals0[1]
                },
                new GenericComparableBytesStructKeyRequiredDependentShadow
                {
                    Id = new GenericComparableBytesStructKey([113]), Principal = principals0[2]
                },
                new GenericComparableBytesStructKeyRequiredDependentShadow
                {
                    Id = new GenericComparableBytesStructKey([114]), Principal = principals0[2]
                },
                new GenericComparableBytesStructKeyRequiredDependentShadow
                {
                    Id = new GenericComparableBytesStructKey([115]), Principal = principals0[2]
                },
                new GenericComparableBytesStructKeyRequiredDependentShadow
                {
                    Id = new GenericComparableBytesStructKey([116]), Principal = principals0[2]
                });

            Assert.Equal(10, await context.SaveChangesAsync());
        }

        using (var context = CreateContext())
        {
            await RunQueries(context);

            Validate(
                principals,
                dependents,
                [(0, [0]), (1, [1]), (2, [2, 2, 2, 2]), (3, [])],
                [(0, 0), (1, 1), (2, 2), (3, 2), (4, 2), (5, 2)]);

            foreach (var principal in principals)
            {
                principal.Foo = "Mutant!";
            }

            dependents[5].Principal = principals[0];
            context.Remove(dependents[4]);
            context.Entry(dependents[3]).Property("PrincipalId").CurrentValue = principals[0].Id;
            principals[1].RequiredDependents.Clear();

            context.Remove(dependents[0]);
            principals[0].RequiredDependents.Add(
                new GenericComparableBytesStructKeyRequiredDependentShadow
                {
                    Id = new GenericComparableBytesStructKey(dependents[0].Id.Id),
                });

            await context.SaveChangesAsync();
        }

        using (var context = CreateContext())
        {
            await RunQueries(context);

            Validate(
                principals,
                dependents,
                [(0, [0, 3, 5]), (1, []), (2, [2]), (3, [])],
                [(0, 0), (2, 2), (3, 0), (5, 0)]);
        }

        async Task RunQueries(DbContext context)
        {
            var twelve = new byte[] { 12, 12 };
            var thirteen = new GenericComparableBytesStructKey { Id = [13, 13, 13] };

            principals =
            [
                await context.Set<GenericComparableBytesStructKeyPrincipalShadow>().Include(e => e.RequiredDependents)
                    .SingleAsync(e => e.Id.Equals(new GenericComparableBytesStructKey { Id = new byte[] { 11 } })),
                await context.Set<GenericComparableBytesStructKeyPrincipalShadow>().Include(e => e.RequiredDependents)
                    .SingleAsync(e => e.Id.Equals(new GenericComparableBytesStructKey { Id = twelve })),
                await context.Set<GenericComparableBytesStructKeyPrincipalShadow>().Include(e => e.RequiredDependents)
                    .SingleAsync(e => e.Id.Equals(thirteen)),
                await context.Set<GenericComparableBytesStructKeyPrincipalShadow>().Include(e => e.RequiredDependents).SingleAsync(
                    e => e.Id.Equals(new GenericComparableBytesStructKey { Id = new byte[] { 14, 14, 14, 14 } }))
            ];

            var oneTwelve = new byte[] { 112 };
            var oneThirteen = new GenericComparableBytesStructKey { Id = [113] };
            var oneFifteeen = new byte[] { 115 };
            var oneSixteen = new GenericComparableBytesStructKey { Id = [116] };

            dependents =
            [
                await context.Set<GenericComparableBytesStructKeyRequiredDependentShadow>()
                    .FirstOrDefaultAsync(e => e.Id.Equals(new GenericComparableBytesStructKey { Id = new byte[] { 111 } })),
                await context.Set<GenericComparableBytesStructKeyRequiredDependentShadow>()
                    .FirstOrDefaultAsync(e => e.Id.Equals(new GenericComparableBytesStructKey { Id = oneTwelve })),
                await context.Set<GenericComparableBytesStructKeyRequiredDependentShadow>().FirstOrDefaultAsync(e => e.Id.Equals(oneThirteen)),
                await context.Set<GenericComparableBytesStructKeyRequiredDependentShadow>()
                    .FirstOrDefaultAsync(e => e.Id.Equals(new GenericComparableBytesStructKey { Id = new byte[] { 114 } })),
                await context.Set<GenericComparableBytesStructKeyRequiredDependentShadow>()
                    .FirstOrDefaultAsync(e => e.Id.Equals(new GenericComparableBytesStructKey { Id = oneFifteeen })),
                await context.Set<GenericComparableBytesStructKeyRequiredDependentShadow>().FirstOrDefaultAsync(e => e.Id.Equals(oneSixteen))
            ];

            Assert.Same(
                dependents[0],
                await context.Set<GenericComparableBytesStructKeyRequiredDependentShadow>()
                    .FindAsync(new GenericComparableBytesStructKey { Id = [111] }));
            Assert.Same(
                dependents[1],
                await context.Set<GenericComparableBytesStructKeyRequiredDependentShadow>()
                    .FindAsync(new GenericComparableBytesStructKey { Id = oneTwelve }));
            Assert.Same(dependents[2], await context.Set<GenericComparableBytesStructKeyRequiredDependentShadow>().FindAsync(oneThirteen));
            Assert.Same(
                dependents[3],
                await context.FindAsync(
                    typeof(GenericComparableBytesStructKeyRequiredDependentShadow),
                    new GenericComparableBytesStructKey { Id = [114] }));
            Assert.Same(
                dependents[4],
                await context.FindAsync(
                    typeof(GenericComparableBytesStructKeyRequiredDependentShadow),
                    new GenericComparableBytesStructKey { Id = oneFifteeen }));
            Assert.Same(dependents[5], await context.FindAsync(typeof(GenericComparableBytesStructKeyRequiredDependentShadow), oneSixteen));
        }

        void Validate(
            GenericComparableBytesStructKeyPrincipalShadow[] principals,
            GenericComparableBytesStructKeyRequiredDependentShadow[] dependents,
            (int, int[])[] expectedPrincipalToDependents,
            (int, int)[] expectedDependentToPrincipals)
        {
            Assert.Equal(4, principals.Length);
            Assert.Equal(new byte[] { 11 }, principals[0].Id.Id);
            Assert.Equal(new byte[] { 12, 12 }, principals[1].Id.Id);
            Assert.Equal(new byte[] { 13, 13, 13 }, principals[2].Id.Id);
            Assert.Equal(new byte[] { 14, 14, 14, 14 }, principals[3].Id.Id);

            Assert.Equal(6, dependents.Length);
            Assert.Equal(new byte[] { 111 }, dependents[0].Id.Id);
            if (dependents[1] != null)
            {
                Assert.Equal(new byte[] { 112 }, dependents[1].Id.Id);
            }

            Assert.Equal(new byte[] { 113 }, dependents[2].Id.Id);
            Assert.Equal(new byte[] { 114 }, dependents[3].Id.Id);
            if (dependents[4] != null)
            {
                Assert.Equal(new byte[] { 115 }, dependents[4].Id.Id);
            }

            Assert.Equal(new byte[] { 116 }, dependents[5].Id.Id);

            foreach (var (dependentIndex, principalIndex) in expectedDependentToPrincipals)
            {
                Assert.Same(principals[principalIndex], dependents[dependentIndex].Principal);
            }

            foreach (var (principalIndex, dependentIndexes) in expectedPrincipalToDependents)
            {
                Assert.Equal(dependentIndexes.Length, principals[principalIndex].RequiredDependents.Count);
                foreach (var dependentIndex in dependentIndexes)
                {
                    Assert.Same(principals[principalIndex], dependents[dependentIndex].Principal);
                }
            }
        }
    }

    private async Task InsertOptionalGraph<TPrincipal, TDependent>()
        where TPrincipal : class, IIntPrincipal, new()
        where TDependent : class, IIntOptionalDependent, new()
    {
        using var context = CreateContext();

        context.Set<TPrincipal>().AddRange(
            new TPrincipal { BackingId = 1, Foo = "X1" },
            new TPrincipal { BackingId = 2, Foo = "X2" },
            new TPrincipal { BackingId = 3, Foo = "X3" },
            new TPrincipal { BackingId = 4, Foo = "X4" });

        context.Set<TDependent>().AddRange(
            new TDependent { BackingId = 101, BackingPrincipalId = 1 },
            new TDependent { BackingId = 102, BackingPrincipalId = 2 },
            new TDependent { BackingId = 103, BackingPrincipalId = 3 },
            new TDependent { BackingId = 104, BackingPrincipalId = 3 },
            new TDependent { BackingId = 105, BackingPrincipalId = 3 },
            new TDependent { BackingId = 106 });

        Assert.Equal(10, await context.SaveChangesAsync());
    }

    private async Task InsertRequiredGraph<TPrincipal, TDependent>()
        where TPrincipal : class, IIntPrincipal, new()
        where TDependent : class, IIntRequiredDependent, new()
    {
        using var context = CreateContext();

        context.Set<TPrincipal>().AddRange(
            new TPrincipal { BackingId = 11, Foo = "X1" },
            new TPrincipal { BackingId = 12, Foo = "X2" },
            new TPrincipal { BackingId = 13, Foo = "X3" },
            new TPrincipal { BackingId = 14, Foo = "X4" });

        context.Set<TDependent>().AddRange(
            new TDependent { BackingId = 111, BackingPrincipalId = 11 },
            new TDependent { BackingId = 112, BackingPrincipalId = 12 },
            new TDependent { BackingId = 113, BackingPrincipalId = 13 },
            new TDependent { BackingId = 114, BackingPrincipalId = 13 },
            new TDependent { BackingId = 115, BackingPrincipalId = 13 },
            new TDependent { BackingId = 116, BackingPrincipalId = 13 });

        Assert.Equal(10, await context.SaveChangesAsync());
    }

    protected void ValidateOptional(
        IList<IIntPrincipal> principals,
        IList<IIntOptionalDependent> dependents,
        IList<(int, int[])> expectedPrincipalToDependents,
        IList<(int, int?)> expectedDependentToPrincipals,
        Func<IIntPrincipal, IList<IIntOptionalDependent>> getDependents,
        Func<IIntOptionalDependent, IIntPrincipal> getPrincipal)
    {
        Assert.Equal(4, principals.Count);
        for (var i = 0; i < 4; i++)
        {
            Assert.Equal(i + 1, principals[i].BackingId);
        }

        Assert.Equal(6, dependents.Count);
        for (var i = 0; i < 6; i++)
        {
            Assert.Equal(i + 101, dependents[i].BackingId);
        }

        foreach (var (dependentIndex, principalIndex) in expectedDependentToPrincipals)
        {
            if (principalIndex.HasValue)
            {
                Assert.Same(principals[principalIndex.Value], getPrincipal(dependents[dependentIndex]));
                Assert.Equal(principals[principalIndex.Value].BackingId, dependents[dependentIndex].BackingPrincipalId);
            }
            else
            {
                Assert.Null(getPrincipal(dependents[dependentIndex]));
                Assert.Null(dependents[dependentIndex].BackingPrincipalId);
            }
        }

        foreach (var (principalIndex, dependentIndexes) in expectedPrincipalToDependents)
        {
            Assert.Equal(dependentIndexes.Length, getDependents(principals[principalIndex]).Count);
            foreach (var dependentIndex in dependentIndexes)
            {
                Assert.Same(principals[principalIndex], getPrincipal(dependents[dependentIndex]));
                Assert.Equal(principals[principalIndex].BackingId, dependents[dependentIndex].BackingPrincipalId);
            }
        }
    }

    protected void ValidateRequired(
        IList<IIntPrincipal> principals,
        IList<IIntRequiredDependent> dependents,
        IList<(int, int[])> expectedPrincipalToDependents,
        IList<(int, int)> expectedDependentToPrincipals,
        Func<IIntPrincipal, IList<IIntRequiredDependent>> getDependents,
        Func<IIntRequiredDependent, IIntPrincipal> getPrincipal)
    {
        Assert.Equal(4, principals.Count);
        for (var i = 0; i < 4; i++)
        {
            Assert.Equal(i + 11, principals[i].BackingId);
        }

        Assert.Equal(6, dependents.Count);
        for (var i = 0; i < 6; i++)
        {
            if (dependents[i] != null)
            {
                Assert.Equal(i + 111, dependents[i].BackingId);
            }
        }

        foreach (var (dependentIndex, principalIndex) in expectedDependentToPrincipals)
        {
            Assert.Same(principals[principalIndex], getPrincipal(dependents[dependentIndex]));
            Assert.Equal(principals[principalIndex].BackingId, dependents[dependentIndex].BackingPrincipalId);
        }

        foreach (var (principalIndex, dependentIndexes) in expectedPrincipalToDependents)
        {
            Assert.Equal(dependentIndexes.Length, getDependents(principals[principalIndex]).Count);
            foreach (var dependentIndex in dependentIndexes)
            {
                Assert.Same(principals[principalIndex], getPrincipal(dependents[dependentIndex]));
                Assert.Equal(principals[principalIndex].BackingId, dependents[dependentIndex].BackingPrincipalId);
            }
        }
    }

    private async Task InsertOptionalBytesGraph<TPrincipal, TDependent>()
        where TPrincipal : class, IBytesPrincipal, new()
        where TDependent : class, IBytesOptionalDependent, new()
    {
        using var context = CreateContext();
        context.Set<TPrincipal>().AddRange(
            new TPrincipal { BackingId = [1], Foo = "X1" },
            new TPrincipal { BackingId = [2, 2], Foo = "X2" },
            new TPrincipal { BackingId = [3, 3, 3], Foo = "X3" },
            new TPrincipal { BackingId = [4, 4, 4, 4], Foo = "X4" });

        context.Set<TDependent>().AddRange(
            new TDependent { BackingId = [101], BackingPrincipalId = [1] },
            new TDependent { BackingId = [102], BackingPrincipalId = [2, 2] },
            new TDependent { BackingId = [103], BackingPrincipalId = [3, 3, 3] },
            new TDependent { BackingId = [104], BackingPrincipalId = [3, 3, 3] },
            new TDependent { BackingId = [105], BackingPrincipalId = [3, 3, 3] },
            new TDependent { BackingId = [106] });

        Assert.Equal(10, await context.SaveChangesAsync());
    }

    private async Task InsertRequiredBytesGraph<TPrincipal, TDependent>()
        where TPrincipal : class, IBytesPrincipal, new()
        where TDependent : class, IBytesRequiredDependent, new()
    {
        using var context = CreateContext();
        context.Set<TPrincipal>().AddRange(
            new TPrincipal { BackingId = [11], Foo = "X1" },
            new TPrincipal { BackingId = [12, 12], Foo = "X2" },
            new TPrincipal { BackingId = [13, 13, 13], Foo = "X3" },
            new TPrincipal { BackingId = [14, 14, 14, 14], Foo = "X4" });

        context.Set<TDependent>().AddRange(
            new TDependent { BackingId = [111], BackingPrincipalId = [11] },
            new TDependent { BackingId = [112], BackingPrincipalId = [12, 12] },
            new TDependent { BackingId = [113], BackingPrincipalId = [13, 13, 13] },
            new TDependent { BackingId = [114], BackingPrincipalId = [13, 13, 13] },
            new TDependent { BackingId = [115], BackingPrincipalId = [13, 13, 13] },
            new TDependent { BackingId = [116], BackingPrincipalId = [13, 13, 13] });

        Assert.Equal(10, await context.SaveChangesAsync());
    }

    protected void ValidateOptionalBytes(
        IList<IBytesPrincipal> principals,
        IList<IBytesOptionalDependent> dependents,
        IList<(int, int[])> expectedPrincipalToDependents,
        IList<(int, int?)> expectedDependentToPrincipals,
        Func<IBytesPrincipal, IList<IBytesOptionalDependent>> getDependents,
        Func<IBytesOptionalDependent, IBytesPrincipal> getPrincipal)
    {
        Assert.Equal(4, principals.Count);
        Assert.Equal(new byte[] { 1 }, principals[0].BackingId);
        Assert.Equal(new byte[] { 2, 2 }, principals[1].BackingId);
        Assert.Equal(new byte[] { 3, 3, 3 }, principals[2].BackingId);
        Assert.Equal(new byte[] { 4, 4, 4, 4 }, principals[3].BackingId);

        Assert.Equal(6, dependents.Count);
        Assert.Equal(new byte[] { 101 }, dependents[0].BackingId);
        Assert.Equal(new byte[] { 102 }, dependents[1].BackingId);
        Assert.Equal(new byte[] { 103 }, dependents[2].BackingId);
        Assert.Equal(new byte[] { 104 }, dependents[3].BackingId);
        Assert.Equal(new byte[] { 105 }, dependents[4].BackingId);
        Assert.Equal(new byte[] { 106 }, dependents[5].BackingId);

        foreach (var (dependentIndex, principalIndex) in expectedDependentToPrincipals)
        {
            if (principalIndex.HasValue)
            {
                Assert.Same(principals[principalIndex.Value], getPrincipal(dependents[dependentIndex]));
                Assert.Equal(principals[principalIndex.Value].BackingId, dependents[dependentIndex].BackingPrincipalId);
            }
            else
            {
                Assert.Null(getPrincipal(dependents[dependentIndex]));
                Assert.Null(dependents[dependentIndex].BackingPrincipalId);
            }
        }

        foreach (var (principalIndex, dependentIndexes) in expectedPrincipalToDependents)
        {
            Assert.Equal(dependentIndexes.Length, getDependents(principals[principalIndex]).Count);
            foreach (var dependentIndex in dependentIndexes)
            {
                Assert.Same(principals[principalIndex], getPrincipal(dependents[dependentIndex]));
                Assert.Equal(principals[principalIndex].BackingId, dependents[dependentIndex].BackingPrincipalId);
            }
        }
    }

    protected void ValidateRequiredBytes(
        IList<IBytesPrincipal> principals,
        IList<IBytesRequiredDependent> dependents,
        IList<(int, int[])> expectedPrincipalToDependents,
        IList<(int, int)> expectedDependentToPrincipals,
        Func<IBytesPrincipal, IList<IBytesRequiredDependent>> getDependents,
        Func<IBytesRequiredDependent, IBytesPrincipal> getPrincipal)
    {
        Assert.Equal(4, principals.Count);
        Assert.Equal(new byte[] { 11 }, principals[0].BackingId);
        Assert.Equal(new byte[] { 12, 12 }, principals[1].BackingId);
        Assert.Equal(new byte[] { 13, 13, 13 }, principals[2].BackingId);
        Assert.Equal(new byte[] { 14, 14, 14, 14 }, principals[3].BackingId);

        Assert.Equal(6, dependents.Count);
        Assert.Equal(new byte[] { 111 }, dependents[0].BackingId);
        if (dependents[1] != null)
        {
            Assert.Equal(new byte[] { 112 }, dependents[1].BackingId);
        }

        Assert.Equal(new byte[] { 113 }, dependents[2].BackingId);
        Assert.Equal(new byte[] { 114 }, dependents[3].BackingId);
        if (dependents[4] != null)
        {
            Assert.Equal(new byte[] { 115 }, dependents[4].BackingId);
        }

        Assert.Equal(new byte[] { 116 }, dependents[5].BackingId);

        foreach (var (dependentIndex, principalIndex) in expectedDependentToPrincipals)
        {
            Assert.Same(principals[principalIndex], getPrincipal(dependents[dependentIndex]));
            Assert.Equal(principals[principalIndex].BackingId, dependents[dependentIndex].BackingPrincipalId);
        }

        foreach (var (principalIndex, dependentIndexes) in expectedPrincipalToDependents)
        {
            Assert.Equal(dependentIndexes.Length, getDependents(principals[principalIndex]).Count);
            foreach (var dependentIndex in dependentIndexes)
            {
                Assert.Same(principals[principalIndex], getPrincipal(dependents[dependentIndex]));
                Assert.Equal(principals[principalIndex].BackingId, dependents[dependentIndex].BackingPrincipalId);
            }
        }
    }

    protected struct IntStructKey(int id)
    {
        public static ValueConverter<IntStructKey, int> Converter
            = new(v => v.Id, v => new IntStructKey { Id = v });

        public int Id { get; set; } = id;
    }

    protected struct BytesStructKey(byte[] id)
    {
        public static ValueConverter<BytesStructKey, byte[]> Converter
            = new(v => v.Id, v => new BytesStructKey { Id = v });

        public byte[] Id { get; set; } = id;

        public override bool Equals(object obj)
            => obj is BytesStructKey other && Equals(other);

        public bool Equals(BytesStructKey other)
            => Id == null
                ? other.Id == null
                : other.Id != null && Id.SequenceEqual(other.Id);

        public override int GetHashCode()
        {
            var code = new HashCode();
            if (Id != null)
            {
                foreach (var b in Id)
                {
                    code.Add(b);
                }
            }

            return code.ToHashCode();
        }
    }

    protected struct ComparableIntStructKey(int id) : IComparable
    {
        public static ValueConverter<ComparableIntStructKey, int> Converter
            = new(v => v.Id, v => new ComparableIntStructKey { Id = v });

        public int Id { get; set; } = id;

        public int CompareTo(object other)
            => Id - ((ComparableIntStructKey)other).Id;
    }

    protected struct ComparableBytesStructKey(byte[] id) : IComparable
    {
        public static ValueConverter<ComparableBytesStructKey, byte[]> Converter
            = new(v => v.Id, v => new ComparableBytesStructKey { Id = v });

        public byte[] Id { get; set; } = id;

        public override bool Equals(object obj)
            => obj is ComparableBytesStructKey other && Equals(other);

        public bool Equals(ComparableBytesStructKey other)
            => (Id == null
                    && other.Id == null)
                || (other.Id != null
                    && Id?.SequenceEqual(other.Id) == true);

        public override int GetHashCode()
        {
            var code = new HashCode();
            if (Id != null)
            {
                foreach (var b in Id)
                {
                    code.Add(b);
                }
            }

            return code.ToHashCode();
        }

        public int CompareTo(object other)
        {
            var result = Id.Length - ((ComparableBytesStructKey)other).Id.Length;

            return result != 0
                ? result
                : StructuralComparisons.StructuralComparer.Compare(Id, ((ComparableBytesStructKey)other).Id);
        }
    }

    protected struct GenericComparableIntStructKey(int id) : IComparable<GenericComparableIntStructKey>
    {
        public static ValueConverter<GenericComparableIntStructKey, int> Converter
            = new(v => v.Id, v => new GenericComparableIntStructKey { Id = v });

        public int Id { get; set; } = id;

        public int CompareTo(GenericComparableIntStructKey other)
            => Id - other.Id;
    }

    protected struct GenericComparableBytesStructKey(byte[] id) : IComparable<GenericComparableBytesStructKey>
    {
        public static ValueConverter<GenericComparableBytesStructKey, byte[]> Converter
            = new(
                v => v.Id, v => new GenericComparableBytesStructKey { Id = v });

        public byte[] Id { get; set; } = id;

        public override bool Equals(object obj)
            => obj is GenericComparableBytesStructKey other && Equals(other);

        public bool Equals(GenericComparableBytesStructKey other)
            => (Id == null
                    && other.Id == null)
                || (other.Id != null
                    && Id?.SequenceEqual(other.Id) == true);

        public override int GetHashCode()
        {
            var code = new HashCode();
            if (Id != null)
            {
                foreach (var b in Id)
                {
                    code.Add(b);
                }
            }

            return code.ToHashCode();
        }

        public int CompareTo(GenericComparableBytesStructKey other)
        {
            var result = Id.Length - other.Id.Length;

            return result != 0
                ? result
                : StructuralComparisons.StructuralComparer.Compare(Id, other.Id);
        }
    }

    protected struct StructuralComparableBytesStructKey(byte[] id) : IStructuralComparable
    {
        public static ValueConverter<StructuralComparableBytesStructKey, byte[]> Converter
            = new(
                v => v.Id, v => new StructuralComparableBytesStructKey { Id = v });

        public byte[] Id { get; set; } = id;

        public override bool Equals(object obj)
            => obj is StructuralComparableBytesStructKey other && Equals(other);

        public bool Equals(StructuralComparableBytesStructKey other)
            => (Id == null
                    && other.Id == null)
                || (other.Id != null
                    && Id?.SequenceEqual(other.Id) == true);

        public override int GetHashCode()
        {
            var code = new HashCode();
            if (Id != null)
            {
                foreach (var b in Id)
                {
                    code.Add(b);
                }
            }

            return code.ToHashCode();
        }

        public int CompareTo(object other, IComparer comparer)
        {
            var typedOther = ((StructuralComparableBytesStructKey)other);

            var i = -1;
            var result = Id.Length - typedOther.Id.Length;

            while (result == 0
                   && ++i < Id.Length)
            {
                result = comparer.Compare(Id[i], typedOther.Id[i]);
            }

            return result;
        }
    }

    protected class IntClassKey(int id)
    {
        public static ValueConverter<IntClassKey, int> Converter
            = new(v => v.Id, v => new IntClassKey(v));

        protected bool Equals(IntClassKey other)
            => other != null && Id == other.Id;

        public override bool Equals(object obj)
            => obj == this
                || obj?.GetType() == GetType()
                && Equals((IntClassKey)obj);

        public override int GetHashCode()
            => Id;

        public int Id { get; set; } = id;
    }

    protected class EnumerableClassKey(int id) : IEnumerable<byte>
    {
        public static ValueConverter<EnumerableClassKey, int> Converter
            = new(v => v.Id, v => new EnumerableClassKey(v));

        public IEnumerator<byte> GetEnumerator()
            => throw new NotImplementedException();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        protected bool Equals(EnumerableClassKey other)
            => other != null && Id == other.Id;

        public override bool Equals(object obj)
            => obj == this
                || obj?.GetType() == GetType()
                && Equals((IntClassKey)obj);

        public override int GetHashCode()
            => Id;

        public int Id { get; set; } = id;
    }

    protected class BareIntClassKey(int id)
    {
        public static ValueConverter<BareIntClassKey, int> Converter
            = new(v => v.Id, v => new BareIntClassKey(v));

        public static ValueComparer<BareIntClassKey> Comparer
            = new(
                (l, r) => (l == null && r == null) || (l != null && r != null && l.Id == r.Id),
                v => v == null ? 0 : v.Id.GetHashCode(),
                v => v == null ? null : new BareIntClassKey(v.Id));

        public int Id { get; set; } = id;
    }

    protected class ComparableIntClassKey(int id) : IComparable
    {
        public static ValueConverter<ComparableIntClassKey, int> Converter
            = new(v => v.Id, v => new ComparableIntClassKey(v));

        public int Id { get; set; } = id;

        protected bool Equals(ComparableIntClassKey other)
            => other != null && Id == other.Id;

        public override bool Equals(object obj)
            => obj == this
                || obj?.GetType() == GetType()
                && Equals((ComparableIntClassKey)obj);

        public override int GetHashCode()
            => Id;

        public int CompareTo(object other)
            => Id - ((ComparableIntClassKey)other).Id;
    }

    protected class GenericComparableIntClassKey(int id) : IComparable<GenericComparableIntClassKey>
    {
        public static ValueConverter<GenericComparableIntClassKey, int> Converter
            = new(v => v.Id, v => new GenericComparableIntClassKey(v));

        public int Id { get; set; } = id;

        protected bool Equals(GenericComparableIntClassKey other)
            => other != null && Id == other.Id;

        public override bool Equals(object obj)
            => obj == this
                || obj?.GetType() == GetType()
                && Equals((GenericComparableIntClassKey)obj);

        public override int GetHashCode()
            => Id;

        public int CompareTo(GenericComparableIntClassKey other)
            => Id - other.Id;
    }

    protected interface IBytesPrincipal
    {
        byte[] BackingId { get; set; }
        string Foo { get; set; }
    }

    protected interface IBytesOptionalDependent
    {
        byte[] BackingId { get; set; }
        byte[] BackingPrincipalId { get; set; }
    }

    protected interface IBytesRequiredDependent
    {
        byte[] BackingId { get; set; }
        byte[] BackingPrincipalId { get; set; }
    }

    protected interface IIntPrincipal
    {
        int BackingId { get; set; }
        string Foo { get; set; }
    }

    protected interface IIntRequiredDependent
    {
        int BackingId { get; set; }
        int BackingPrincipalId { get; set; }
    }

    protected interface IIntOptionalDependent
    {
        int BackingId { get; set; }
        int? BackingPrincipalId { get; set; }
    }

    protected class IntStructKeyPrincipal : IIntPrincipal
    {
        public IntStructKey Id { get; set; }
        public string Foo { get; set; }
        public ICollection<IntStructKeyRequiredDependent> RequiredDependents { get; set; }
        public ICollection<IntStructKeyOptionalDependent> OptionalDependents { get; set; }

        [NotMapped]
        public int BackingId
        {
            get => Id.Id;
            set => Id = new IntStructKey(value);
        }
    }

    protected class IntStructKeyRequiredDependent : IIntRequiredDependent
    {
        public IntStructKey Id { get; set; }
        public IntStructKey PrincipalId { get; set; }
        public IntStructKeyPrincipal Principal { get; set; }

        [NotMapped]
        public int BackingId
        {
            get => Id.Id;
            set => Id = new IntStructKey(value);
        }

        [NotMapped]
        public int BackingPrincipalId
        {
            get => PrincipalId.Id;
            set => PrincipalId = new IntStructKey(value);
        }
    }

    protected class IntStructKeyOptionalDependent : IIntOptionalDependent
    {
        public IntStructKey Id { get; set; }
        public IntStructKey? PrincipalId { get; set; }
        public IntStructKeyPrincipal Principal { get; set; }

        [NotMapped]
        public int BackingId
        {
            get => Id.Id;
            set => Id = new IntStructKey(value);
        }

        [NotMapped]
        public int? BackingPrincipalId
        {
            get => PrincipalId?.Id;
            set => PrincipalId = value.HasValue ? new IntStructKey(value.Value) : null;
        }
    }

    protected class BytesStructKeyPrincipal : IBytesPrincipal
    {
        public BytesStructKey Id { get; set; }
        public string Foo { get; set; }
        public ICollection<BytesStructKeyOptionalDependent> OptionalDependents { get; set; }
        public ICollection<BytesStructKeyRequiredDependent> RequiredDependents { get; set; }

        [NotMapped]
        public byte[] BackingId
        {
            get => Id.Id;
            set => Id = new BytesStructKey(value);
        }
    }

    protected class BytesStructKeyOptionalDependent : IBytesOptionalDependent
    {
        public BytesStructKey Id { get; set; }
        public BytesStructKey? PrincipalId { get; set; }
        public BytesStructKeyPrincipal Principal { get; set; }

        [NotMapped]
        public byte[] BackingId
        {
            get => Id.Id;
            set => Id = new BytesStructKey(value);
        }

        [NotMapped]
        public byte[] BackingPrincipalId
        {
            get => PrincipalId?.Id;
            set => PrincipalId = value != null ? new BytesStructKey(value) : null;
        }
    }

    protected class BytesStructKeyRequiredDependent : IBytesRequiredDependent
    {
        public BytesStructKey Id { get; set; }
        public BytesStructKey PrincipalId { get; set; }
        public BytesStructKeyPrincipal Principal { get; set; }

        [NotMapped]
        public byte[] BackingId
        {
            get => Id.Id;
            set => Id = new BytesStructKey(value);
        }

        [NotMapped]
        public byte[] BackingPrincipalId
        {
            get => PrincipalId.Id;
            set => PrincipalId = new BytesStructKey(value);
        }
    }

    protected class ComparableIntStructKeyPrincipal : IIntPrincipal
    {
        public ComparableIntStructKey Id { get; set; }
        public string Foo { get; set; }
        public ICollection<ComparableIntStructKeyOptionalDependent> OptionalDependents { get; set; }
        public ICollection<ComparableIntStructKeyRequiredDependent> RequiredDependents { get; set; }

        [NotMapped]
        public int BackingId
        {
            get => Id.Id;
            set => Id = new ComparableIntStructKey(value);
        }
    }

    protected class ComparableIntStructKeyOptionalDependent : IIntOptionalDependent
    {
        public ComparableIntStructKey Id { get; set; }
        public ComparableIntStructKey? PrincipalId { get; set; }
        public ComparableIntStructKeyPrincipal Principal { get; set; }

        [NotMapped]
        public int BackingId
        {
            get => Id.Id;
            set => Id = new ComparableIntStructKey(value);
        }

        [NotMapped]
        public int? BackingPrincipalId
        {
            get => PrincipalId?.Id;
            set => PrincipalId = value.HasValue ? new ComparableIntStructKey(value.Value) : null;
        }
    }

    protected class ComparableIntStructKeyRequiredDependent : IIntRequiredDependent
    {
        public ComparableIntStructKey Id { get; set; }
        public ComparableIntStructKey PrincipalId { get; set; }
        public ComparableIntStructKeyPrincipal Principal { get; set; }

        [NotMapped]
        public int BackingId
        {
            get => Id.Id;
            set => Id = new ComparableIntStructKey(value);
        }

        [NotMapped]
        public int BackingPrincipalId
        {
            get => PrincipalId.Id;
            set => PrincipalId = new ComparableIntStructKey(value);
        }
    }

    protected class ComparableBytesStructKeyPrincipal : IBytesPrincipal
    {
        public ComparableBytesStructKey Id { get; set; }
        public string Foo { get; set; }
        public ICollection<ComparableBytesStructKeyOptionalDependent> OptionalDependents { get; set; }
        public ICollection<ComparableBytesStructKeyRequiredDependent> RequiredDependents { get; set; }

        [NotMapped]
        public byte[] BackingId
        {
            get => Id.Id;
            set => Id = new ComparableBytesStructKey(value);
        }
    }

    protected class ComparableBytesStructKeyOptionalDependent : IBytesOptionalDependent
    {
        public ComparableBytesStructKey Id { get; set; }
        public ComparableBytesStructKey? PrincipalId { get; set; }
        public ComparableBytesStructKeyPrincipal Principal { get; set; }

        [NotMapped]
        public byte[] BackingId
        {
            get => Id.Id;
            set => Id = new ComparableBytesStructKey(value);
        }

        [NotMapped]
        public byte[] BackingPrincipalId
        {
            get => PrincipalId?.Id;
            set => PrincipalId = value != null ? new ComparableBytesStructKey(value) : null;
        }
    }

    protected class ComparableBytesStructKeyRequiredDependent : IBytesRequiredDependent
    {
        public ComparableBytesStructKey Id { get; set; }
        public ComparableBytesStructKey PrincipalId { get; set; }
        public ComparableBytesStructKeyPrincipal Principal { get; set; }

        [NotMapped]
        public byte[] BackingId
        {
            get => Id.Id;
            set => Id = new ComparableBytesStructKey(value);
        }

        [NotMapped]
        public byte[] BackingPrincipalId
        {
            get => PrincipalId.Id;
            set => PrincipalId = new ComparableBytesStructKey(value);
        }
    }

    protected class GenericComparableIntStructKeyPrincipal : IIntPrincipal
    {
        public GenericComparableIntStructKey Id { get; set; }
        public string Foo { get; set; }
        public ICollection<GenericComparableIntStructKeyOptionalDependent> OptionalDependents { get; set; }
        public ICollection<GenericComparableIntStructKeyRequiredDependent> RequiredDependents { get; set; }

        [NotMapped]
        public int BackingId
        {
            get => Id.Id;
            set => Id = new GenericComparableIntStructKey(value);
        }
    }

    protected class GenericComparableIntStructKeyOptionalDependent : IIntOptionalDependent
    {
        public GenericComparableIntStructKey Id { get; set; }
        public GenericComparableIntStructKey? PrincipalId { get; set; }
        public GenericComparableIntStructKeyPrincipal Principal { get; set; }

        [NotMapped]
        public int BackingId
        {
            get => Id.Id;
            set => Id = new GenericComparableIntStructKey(value);
        }

        [NotMapped]
        public int? BackingPrincipalId
        {
            get => PrincipalId?.Id;
            set => PrincipalId = value.HasValue
                ? new GenericComparableIntStructKey(value.Value)
                : null;
        }
    }

    protected class GenericComparableIntStructKeyRequiredDependent : IIntRequiredDependent
    {
        public GenericComparableIntStructKey Id { get; set; }
        public GenericComparableIntStructKey PrincipalId { get; set; }
        public GenericComparableIntStructKeyPrincipal Principal { get; set; }

        [NotMapped]
        public int BackingId
        {
            get => Id.Id;
            set => Id = new GenericComparableIntStructKey(value);
        }

        [NotMapped]
        public int BackingPrincipalId
        {
            get => PrincipalId.Id;
            set => PrincipalId = new GenericComparableIntStructKey(value);
        }
    }

    protected class GenericComparableBytesStructKeyPrincipal : IBytesPrincipal
    {
        public GenericComparableBytesStructKey Id { get; set; }
        public string Foo { get; set; }
        public ICollection<GenericComparableBytesStructKeyOptionalDependent> OptionalDependents { get; set; }
        public ICollection<GenericComparableBytesStructKeyRequiredDependent> RequiredDependents { get; set; }

        [NotMapped]
        public byte[] BackingId
        {
            get => Id.Id;
            set => Id = new GenericComparableBytesStructKey(value);
        }
    }

    protected class GenericComparableBytesStructKeyOptionalDependent : IBytesOptionalDependent
    {
        public GenericComparableBytesStructKey Id { get; set; }
        public GenericComparableBytesStructKey? PrincipalId { get; set; }
        public GenericComparableBytesStructKeyPrincipal Principal { get; set; }

        [NotMapped]
        public byte[] BackingId
        {
            get => Id.Id;
            set => Id = new GenericComparableBytesStructKey(value);
        }

        [NotMapped]
        public byte[] BackingPrincipalId
        {
            get => PrincipalId?.Id;
            set => PrincipalId = value != null
                ? new GenericComparableBytesStructKey(value)
                : null;
        }
    }

    protected class GenericComparableBytesStructKeyRequiredDependent : IBytesRequiredDependent
    {
        public GenericComparableBytesStructKey Id { get; set; }
        public GenericComparableBytesStructKey PrincipalId { get; set; }
        public GenericComparableBytesStructKeyPrincipal Principal { get; set; }

        [NotMapped]
        public byte[] BackingId
        {
            get => Id.Id;
            set => Id = new GenericComparableBytesStructKey(value);
        }

        [NotMapped]
        public byte[] BackingPrincipalId
        {
            get => PrincipalId.Id;
            set => PrincipalId = new GenericComparableBytesStructKey(value);
        }
    }

    protected class StructuralComparableBytesStructKeyPrincipal : IBytesPrincipal
    {
        public StructuralComparableBytesStructKey Id { get; set; }
        public string Foo { get; set; }
        public ICollection<StructuralComparableBytesStructKeyOptionalDependent> OptionalDependents { get; set; }
        public ICollection<StructuralComparableBytesStructKeyRequiredDependent> RequiredDependents { get; set; }

        [NotMapped]
        public byte[] BackingId
        {
            get => Id.Id;
            set => Id = new StructuralComparableBytesStructKey(value);
        }
    }

    protected class StructuralComparableBytesStructKeyOptionalDependent : IBytesOptionalDependent
    {
        public StructuralComparableBytesStructKey Id { get; set; }
        public StructuralComparableBytesStructKey? PrincipalId { get; set; }
        public StructuralComparableBytesStructKeyPrincipal Principal { get; set; }

        [NotMapped]
        public byte[] BackingId
        {
            get => Id.Id;
            set => Id = new StructuralComparableBytesStructKey(value);
        }

        [NotMapped]
        public byte[] BackingPrincipalId
        {
            get => PrincipalId?.Id;
            set => PrincipalId = value != null
                ? new StructuralComparableBytesStructKey(value)
                : null;
        }
    }

    protected class StructuralComparableBytesStructKeyRequiredDependent : IBytesRequiredDependent
    {
        public StructuralComparableBytesStructKey Id { get; set; }
        public StructuralComparableBytesStructKey PrincipalId { get; set; }
        public StructuralComparableBytesStructKeyPrincipal Principal { get; set; }

        [NotMapped]
        public byte[] BackingId
        {
            get => Id.Id;
            set => Id = new StructuralComparableBytesStructKey(value);
        }

        [NotMapped]
        public byte[] BackingPrincipalId
        {
            get => PrincipalId.Id;
            set => PrincipalId = new StructuralComparableBytesStructKey(value);
        }
    }

    protected class IntClassKeyPrincipal : IIntPrincipal
    {
        public IntClassKey Id { get; set; }
        public string Foo { get; set; }
        public ICollection<IntClassKeyOptionalDependent> OptionalDependents { get; set; }
        public ICollection<IntClassKeyRequiredDependent> RequiredDependents { get; set; }

        [NotMapped]
        public int BackingId
        {
            get => Id.Id;
            set => Id = new IntClassKey(value);
        }
    }

    protected class IntClassKeyOptionalDependent : IIntOptionalDependent
    {
        public IntClassKey Id { get; set; }
        public IntClassKey PrincipalId { get; set; }
        public IntClassKeyPrincipal Principal { get; set; }

        [NotMapped]
        public int BackingId
        {
            get => Id.Id;
            set => Id = new IntClassKey(value);
        }

        [NotMapped]
        public int? BackingPrincipalId
        {
            get => PrincipalId?.Id;
            set => PrincipalId = value.HasValue ? new IntClassKey(value.Value) : null;
        }
    }

    protected class IntClassKeyRequiredDependent : IIntRequiredDependent
    {
        public IntClassKey Id { get; set; }
        public IntClassKey PrincipalId { get; set; }
        public IntClassKeyPrincipal Principal { get; set; }

        [NotMapped]
        public int BackingId
        {
            get => Id.Id;
            set => Id = new IntClassKey(value);
        }

        [NotMapped]
        public int BackingPrincipalId
        {
            get => PrincipalId.Id;
            set => PrincipalId = new IntClassKey(value);
        }
    }

    protected class EnumerableClassKeyPrincipal : IIntPrincipal
    {
        public EnumerableClassKey Id { get; set; }
        public string Foo { get; set; }
        public ICollection<EnumerableClassKeyOptionalDependent> OptionalDependents { get; set; }
        public ICollection<EnumerableClassKeyRequiredDependent> RequiredDependents { get; set; }

        [NotMapped]
        public int BackingId
        {
            get => Id.Id;
            set => Id = new EnumerableClassKey(value);
        }
    }

    protected class EnumerableClassKeyOptionalDependent : IIntOptionalDependent
    {
        public EnumerableClassKey Id { get; set; }
        public EnumerableClassKey PrincipalId { get; set; }
        public EnumerableClassKeyPrincipal Principal { get; set; }

        [NotMapped]
        public int BackingId
        {
            get => Id.Id;
            set => Id = new EnumerableClassKey(value);
        }

        [NotMapped]
        public int? BackingPrincipalId
        {
            get => PrincipalId?.Id;
            set => PrincipalId = value.HasValue ? new EnumerableClassKey(value.Value) : null;
        }
    }

    protected class EnumerableClassKeyRequiredDependent : IIntRequiredDependent
    {
        public EnumerableClassKey Id { get; set; }
        public EnumerableClassKey PrincipalId { get; set; }
        public EnumerableClassKeyPrincipal Principal { get; set; }

        [NotMapped]
        public int BackingId
        {
            get => Id.Id;
            set => Id = new EnumerableClassKey(value);
        }

        [NotMapped]
        public int BackingPrincipalId
        {
            get => PrincipalId.Id;
            set => PrincipalId = new EnumerableClassKey(value);
        }
    }

    protected class BareIntClassKeyPrincipal : IIntPrincipal
    {
        public BareIntClassKey Id { get; set; }
        public string Foo { get; set; }
        public ICollection<BareIntClassKeyOptionalDependent> OptionalDependents { get; set; }
        public ICollection<BareIntClassKeyRequiredDependent> RequiredDependents { get; set; }

        [NotMapped]
        public int BackingId
        {
            get => Id.Id;
            set => Id = new BareIntClassKey(value);
        }
    }

    protected class BareIntClassKeyOptionalDependent : IIntOptionalDependent
    {
        public BareIntClassKey Id { get; set; }
        public BareIntClassKey PrincipalId { get; set; }
        public BareIntClassKeyPrincipal Principal { get; set; }

        [NotMapped]
        public int BackingId
        {
            get => Id.Id;
            set => Id = new BareIntClassKey(value);
        }

        [NotMapped]
        public int? BackingPrincipalId
        {
            get => PrincipalId?.Id;
            set => PrincipalId = value.HasValue ? new BareIntClassKey(value.Value) : null;
        }
    }

    protected class BareIntClassKeyRequiredDependent : IIntRequiredDependent
    {
        public BareIntClassKey Id { get; set; }
        public BareIntClassKey PrincipalId { get; set; }
        public BareIntClassKeyPrincipal Principal { get; set; }

        [NotMapped]
        public int BackingId
        {
            get => Id.Id;
            set => Id = new BareIntClassKey(value);
        }

        [NotMapped]
        public int BackingPrincipalId
        {
            get => PrincipalId.Id;
            set => PrincipalId = new BareIntClassKey(value);
        }
    }

    protected class ComparableIntClassKeyPrincipal : IIntPrincipal
    {
        public ComparableIntClassKey Id { get; set; }
        public string Foo { get; set; }
        public ICollection<ComparableIntClassKeyOptionalDependent> OptionalDependents { get; set; }
        public ICollection<ComparableIntClassKeyRequiredDependent> RequiredDependents { get; set; }

        [NotMapped]
        public int BackingId
        {
            get => Id.Id;
            set => Id = new ComparableIntClassKey(value);
        }
    }

    protected class ComparableIntClassKeyOptionalDependent : IIntOptionalDependent
    {
        public ComparableIntClassKey Id { get; set; }
        public ComparableIntClassKey PrincipalId { get; set; }
        public ComparableIntClassKeyPrincipal Principal { get; set; }

        [NotMapped]
        public int BackingId
        {
            get => Id.Id;
            set => Id = new ComparableIntClassKey(value);
        }

        [NotMapped]
        public int? BackingPrincipalId
        {
            get => PrincipalId?.Id;
            set => PrincipalId = value.HasValue ? new ComparableIntClassKey(value.Value) : null;
        }
    }

    protected class ComparableIntClassKeyRequiredDependent : IIntRequiredDependent
    {
        public ComparableIntClassKey Id { get; set; }
        public ComparableIntClassKey PrincipalId { get; set; }
        public ComparableIntClassKeyPrincipal Principal { get; set; }

        [NotMapped]
        public int BackingId
        {
            get => Id.Id;
            set => Id = new ComparableIntClassKey(value);
        }

        [NotMapped]
        public int BackingPrincipalId
        {
            get => PrincipalId.Id;
            set => PrincipalId = new ComparableIntClassKey(value);
        }
    }

    protected class GenericComparableIntClassKeyPrincipal : IIntPrincipal
    {
        public GenericComparableIntClassKey Id { get; set; }
        public string Foo { get; set; }
        public ICollection<GenericComparableIntClassKeyOptionalDependent> OptionalDependents { get; set; }
        public ICollection<GenericComparableIntClassKeyRequiredDependent> RequiredDependents { get; set; }

        [NotMapped]
        public int BackingId
        {
            get => Id.Id;
            set => Id = new GenericComparableIntClassKey(value);
        }
    }

    protected class GenericComparableIntClassKeyOptionalDependent : IIntOptionalDependent
    {
        public GenericComparableIntClassKey Id { get; set; }
        public GenericComparableIntClassKey PrincipalId { get; set; }
        public GenericComparableIntClassKeyPrincipal Principal { get; set; }

        [NotMapped]
        public int BackingId
        {
            get => Id.Id;
            set => Id = new GenericComparableIntClassKey(value);
        }

        [NotMapped]
        public int? BackingPrincipalId
        {
            get => PrincipalId?.Id;
            set => PrincipalId = value.HasValue ? new GenericComparableIntClassKey(value.Value) : null;
        }
    }

    protected class GenericComparableIntClassKeyRequiredDependent : IIntRequiredDependent
    {
        public GenericComparableIntClassKey Id { get; set; }
        public GenericComparableIntClassKey PrincipalId { get; set; }
        public GenericComparableIntClassKeyPrincipal Principal { get; set; }

        [NotMapped]
        public int BackingId
        {
            get => Id.Id;
            set => Id = new GenericComparableIntClassKey(value);
        }

        [NotMapped]
        public int BackingPrincipalId
        {
            get => PrincipalId.Id;
            set => PrincipalId = new GenericComparableIntClassKey(value);
        }
    }

    protected class IntStructKeyPrincipalShadow
    {
        public IntStructKey Id { get; set; }
        public string Foo { get; set; }
        public ICollection<IntStructKeyRequiredDependentShadow> RequiredDependents { get; set; }
        public ICollection<IntStructKeyOptionalDependentShadow> OptionalDependents { get; set; }
    }

    protected class IntStructKeyRequiredDependentShadow
    {
        public IntStructKey Id { get; set; }
        public IntStructKeyPrincipalShadow Principal { get; set; }
    }

    protected class IntStructKeyOptionalDependentShadow
    {
        public IntStructKey Id { get; set; }
        public IntStructKeyPrincipalShadow Principal { get; set; }
    }

    protected class BytesStructKeyPrincipalShadow
    {
        public BytesStructKey Id { get; set; }
        public string Foo { get; set; }
        public ICollection<BytesStructKeyOptionalDependentShadow> OptionalDependents { get; set; }
        public ICollection<BytesStructKeyRequiredDependentShadow> RequiredDependents { get; set; }
    }

    protected class BytesStructKeyOptionalDependentShadow
    {
        public BytesStructKey Id { get; set; }
        public BytesStructKeyPrincipalShadow Principal { get; set; }
    }

    protected class BytesStructKeyRequiredDependentShadow
    {
        public BytesStructKey Id { get; set; }
        public BytesStructKeyPrincipalShadow Principal { get; set; }
    }

    protected class ComparableIntStructKeyPrincipalShadow
    {
        public ComparableIntStructKey Id { get; set; }
        public string Foo { get; set; }
        public ICollection<ComparableIntStructKeyOptionalDependentShadow> OptionalDependents { get; set; }
        public ICollection<ComparableIntStructKeyRequiredDependentShadow> RequiredDependents { get; set; }
    }

    protected class ComparableIntStructKeyOptionalDependentShadow
    {
        public ComparableIntStructKey Id { get; set; }
        public ComparableIntStructKeyPrincipalShadow Principal { get; set; }
    }

    protected class ComparableIntStructKeyRequiredDependentShadow
    {
        public ComparableIntStructKey Id { get; set; }
        public ComparableIntStructKeyPrincipalShadow Principal { get; set; }
    }

    protected class ComparableBytesStructKeyPrincipalShadow
    {
        public ComparableBytesStructKey Id { get; set; }
        public string Foo { get; set; }
        public ICollection<ComparableBytesStructKeyOptionalDependentShadow> OptionalDependents { get; set; }
        public ICollection<ComparableBytesStructKeyRequiredDependentShadow> RequiredDependents { get; set; }
    }

    protected class ComparableBytesStructKeyOptionalDependentShadow
    {
        public ComparableBytesStructKey Id { get; set; }
        public ComparableBytesStructKeyPrincipalShadow Principal { get; set; }
    }

    protected class ComparableBytesStructKeyRequiredDependentShadow
    {
        public ComparableBytesStructKey Id { get; set; }
        public ComparableBytesStructKeyPrincipalShadow Principal { get; set; }
    }

    protected class GenericComparableIntStructKeyPrincipalShadow
    {
        public GenericComparableIntStructKey Id { get; set; }
        public string Foo { get; set; }
        public ICollection<GenericComparableIntStructKeyOptionalDependentShadow> OptionalDependents { get; set; }
        public ICollection<GenericComparableIntStructKeyRequiredDependentShadow> RequiredDependents { get; set; }
    }

    protected class GenericComparableIntStructKeyOptionalDependentShadow
    {
        public GenericComparableIntStructKey Id { get; set; }
        public GenericComparableIntStructKeyPrincipalShadow Principal { get; set; }
    }

    protected class GenericComparableIntStructKeyRequiredDependentShadow
    {
        public GenericComparableIntStructKey Id { get; set; }
        public GenericComparableIntStructKeyPrincipalShadow Principal { get; set; }
    }

    protected class GenericComparableBytesStructKeyPrincipalShadow
    {
        public GenericComparableBytesStructKey Id { get; set; }
        public string Foo { get; set; }
        public ICollection<GenericComparableBytesStructKeyOptionalDependentShadow> OptionalDependents { get; set; }
        public ICollection<GenericComparableBytesStructKeyRequiredDependentShadow> RequiredDependents { get; set; }
    }

    protected class GenericComparableBytesStructKeyOptionalDependentShadow
    {
        public GenericComparableBytesStructKey Id { get; set; }
        public GenericComparableBytesStructKeyPrincipalShadow Principal { get; set; }
    }

    protected class GenericComparableBytesStructKeyRequiredDependentShadow
    {
        public GenericComparableBytesStructKey Id { get; set; }
        public GenericComparableBytesStructKeyPrincipalShadow Principal { get; set; }
    }

    protected class StructuralComparableBytesStructKeyPrincipalShadow
    {
        public StructuralComparableBytesStructKey Id { get; set; }
        public string Foo { get; set; }
        public ICollection<StructuralComparableBytesStructKeyOptionalDependentShadow> OptionalDependents { get; set; }
        public ICollection<StructuralComparableBytesStructKeyRequiredDependentShadow> RequiredDependents { get; set; }
    }

    protected class StructuralComparableBytesStructKeyOptionalDependentShadow
    {
        public StructuralComparableBytesStructKey Id { get; set; }
        public StructuralComparableBytesStructKeyPrincipalShadow Principal { get; set; }
    }

    protected class StructuralComparableBytesStructKeyRequiredDependentShadow
    {
        public StructuralComparableBytesStructKey Id { get; set; }
        public StructuralComparableBytesStructKeyPrincipalShadow Principal { get; set; }
    }

    protected class IntClassKeyPrincipalShadow
    {
        public IntClassKey Id { get; set; }
        public string Foo { get; set; }
        public ICollection<IntClassKeyOptionalDependentShadow> OptionalDependents { get; set; }
        public ICollection<IntClassKeyRequiredDependentShadow> RequiredDependents { get; set; }
    }

    protected class IntClassKeyOptionalDependentShadow
    {
        public IntClassKey Id { get; set; }
        public IntClassKeyPrincipalShadow Principal { get; set; }
    }

    protected class IntClassKeyRequiredDependentShadow
    {
        public IntClassKey Id { get; set; }
        public IntClassKeyPrincipalShadow Principal { get; set; }
    }

    protected class BareIntClassKeyPrincipalShadow
    {
        public BareIntClassKey Id { get; set; }
        public string Foo { get; set; }
        public ICollection<BareIntClassKeyOptionalDependentShadow> OptionalDependents { get; set; }
        public ICollection<BareIntClassKeyRequiredDependentShadow> RequiredDependents { get; set; }
    }

    protected class BareIntClassKeyOptionalDependentShadow
    {
        public BareIntClassKey Id { get; set; }
        public BareIntClassKeyPrincipalShadow Principal { get; set; }
    }

    protected class BareIntClassKeyRequiredDependentShadow
    {
        public BareIntClassKey Id { get; set; }
        public BareIntClassKeyPrincipalShadow Principal { get; set; }
    }

    protected class ComparableIntClassKeyPrincipalShadow
    {
        public ComparableIntClassKey Id { get; set; }
        public string Foo { get; set; }
        public ICollection<ComparableIntClassKeyOptionalDependentShadow> OptionalDependents { get; set; }
        public ICollection<ComparableIntClassKeyRequiredDependentShadow> RequiredDependents { get; set; }
    }

    protected class ComparableIntClassKeyOptionalDependentShadow
    {
        public ComparableIntClassKey Id { get; set; }
        public ComparableIntClassKeyPrincipalShadow Principal { get; set; }
    }

    protected class ComparableIntClassKeyRequiredDependentShadow
    {
        public ComparableIntClassKey Id { get; set; }
        public ComparableIntClassKeyPrincipalShadow Principal { get; set; }
    }

    protected class GenericComparableIntClassKeyPrincipalShadow
    {
        public GenericComparableIntClassKey Id { get; set; }
        public string Foo { get; set; }
        public ICollection<GenericComparableIntClassKeyOptionalDependentShadow> OptionalDependents { get; set; }
        public ICollection<GenericComparableIntClassKeyRequiredDependentShadow> RequiredDependents { get; set; }
    }

    protected class GenericComparableIntClassKeyOptionalDependentShadow
    {
        public GenericComparableIntClassKey Id { get; set; }
        public GenericComparableIntClassKeyPrincipalShadow Principal { get; set; }
    }

    protected class GenericComparableIntClassKeyRequiredDependentShadow
    {
        public GenericComparableIntClassKey Id { get; set; }
        public GenericComparableIntClassKeyPrincipalShadow Principal { get; set; }
    }

    protected class Key(string id)
    {
        public string Value { get; } = id;
    }

    protected class BaseEntity
    {
        public BaseEntity()
        {
        }

        public BaseEntity(Key key, TextEntity text)
        {
            Name = key;
            Text = text;
        }

        public Key Name { get; set; }
        public TextEntity Text { get; set; }
    }

    protected class TextEntity
    {
        public int Position { get; set; }
    }

    protected class OwnerIntStructKey
    {
        public OwnerIntStructKey(IntStructKey id)
        {
            Id = id;
        }

        public OwnerIntStructKey(IntStructKey id, OwnedIntStructKey owned)
        {
            Id = id;
            Owned = owned;
        }

        public IntStructKey Id { get; set; }
        public OwnedIntStructKey Owned { get; set; }
    }

    protected class OwnedIntStructKey(int position)
    {
        public int Position { get; set; } = position;
    }

    protected class OwnerBytesStructKey
    {
        public OwnerBytesStructKey(BytesStructKey id)
        {
            Id = id;
        }

        public OwnerBytesStructKey(BytesStructKey id, OwnedBytesStructKey owned)
        {
            Id = id;
            Owned = owned;
        }

        public BytesStructKey Id { get; set; }
        public OwnedBytesStructKey Owned { get; set; }
    }

    protected class OwnedBytesStructKey(int position)
    {
        public int Position { get; set; } = position;
    }

    protected class OwnerComparableIntStructKey
    {
        public OwnerComparableIntStructKey(ComparableIntStructKey id)
        {
            Id = id;
        }

        public OwnerComparableIntStructKey(ComparableIntStructKey id, OwnedComparableIntStructKey owned)
        {
            Id = id;
            Owned = owned;
        }

        public ComparableIntStructKey Id { get; set; }
        public OwnedComparableIntStructKey Owned { get; set; }
    }

    protected class OwnedComparableIntStructKey(int position)
    {
        public int Position { get; set; } = position;
    }

    protected class OwnerComparableBytesStructKey
    {
        public OwnerComparableBytesStructKey(ComparableBytesStructKey id)
        {
            Id = id;
        }

        public OwnerComparableBytesStructKey(ComparableBytesStructKey id, OwnedComparableBytesStructKey owned)
        {
            Id = id;
            Owned = owned;
        }

        public ComparableBytesStructKey Id { get; set; }
        public OwnedComparableBytesStructKey Owned { get; set; }
    }

    protected class OwnedComparableBytesStructKey(int position)
    {
        public int Position { get; set; } = position;
    }

    protected class OwnerGenericComparableIntStructKey
    {
        public OwnerGenericComparableIntStructKey(GenericComparableIntStructKey id)
        {
            Id = id;
        }

        public OwnerGenericComparableIntStructKey(GenericComparableIntStructKey id, OwnedGenericComparableIntStructKey owned)
        {
            Id = id;
            Owned = owned;
        }

        public GenericComparableIntStructKey Id { get; set; }
        public OwnedGenericComparableIntStructKey Owned { get; set; }
    }

    protected class OwnedGenericComparableIntStructKey(int position)
    {
        public int Position { get; set; } = position;
    }

    protected class OwnerGenericComparableBytesStructKey
    {
        public OwnerGenericComparableBytesStructKey(GenericComparableBytesStructKey id)
        {
            Id = id;
        }

        public OwnerGenericComparableBytesStructKey(GenericComparableBytesStructKey id, OwnedGenericComparableBytesStructKey owned)
        {
            Id = id;
            Owned = owned;
        }

        public GenericComparableBytesStructKey Id { get; set; }
        public OwnedGenericComparableBytesStructKey Owned { get; set; }
    }

    protected class OwnedGenericComparableBytesStructKey(int position)
    {
        public int Position { get; set; } = position;
    }

    protected class OwnerStructuralComparableBytesStructKey
    {
        public OwnerStructuralComparableBytesStructKey(StructuralComparableBytesStructKey id)
        {
            Id = id;
        }

        public OwnerStructuralComparableBytesStructKey(
            StructuralComparableBytesStructKey id,
            OwnedStructuralComparableBytesStructKey owned)
        {
            Id = id;
            Owned = owned;
        }

        public StructuralComparableBytesStructKey Id { get; set; }
        public OwnedStructuralComparableBytesStructKey Owned { get; set; }
    }

    protected class OwnedStructuralComparableBytesStructKey(int position)
    {
        public int Position { get; set; } = position;
    }

    protected class OwnerIntClassKey
    {
        public OwnerIntClassKey(IntClassKey id)
        {
            Id = id;
        }

        public OwnerIntClassKey(IntClassKey id, OwnedIntClassKey owned)
        {
            Id = id;
            Owned = owned;
        }

        public IntClassKey Id { get; set; }
        public OwnedIntClassKey Owned { get; set; }
    }

    protected class OwnedIntClassKey(int position)
    {
        public int Position { get; set; } = position;
    }

    protected class OwnerBareIntClassKey
    {
        public OwnerBareIntClassKey(BareIntClassKey id)
        {
            Id = id;
        }

        public OwnerBareIntClassKey(BareIntClassKey id, OwnedBareIntClassKey owned)
        {
            Id = id;
            Owned = owned;
        }

        public BareIntClassKey Id { get; set; }
        public OwnedBareIntClassKey Owned { get; set; }
    }

    protected class OwnedBareIntClassKey(int position)
    {
        public int Position { get; set; } = position;
    }

    protected class OwnerComparableIntClassKey
    {
        public OwnerComparableIntClassKey(ComparableIntClassKey id)
        {
            Id = id;
        }

        public OwnerComparableIntClassKey(ComparableIntClassKey id, OwnedComparableIntClassKey owned)
        {
            Id = id;
            Owned = owned;
        }

        public ComparableIntClassKey Id { get; set; }
        public OwnedComparableIntClassKey Owned { get; set; }
    }

    protected class OwnedComparableIntClassKey(int position)
    {
        public int Position { get; set; } = position;
    }

    protected class OwnerGenericComparableIntClassKey
    {
        public OwnerGenericComparableIntClassKey(GenericComparableIntClassKey id)
        {
            Id = id;
        }

        public OwnerGenericComparableIntClassKey(GenericComparableIntClassKey id, OwnedGenericComparableIntClassKey owned)
        {
            Id = id;
            Owned = owned;
        }

        public GenericComparableIntClassKey Id { get; set; }
        public OwnedGenericComparableIntClassKey Owned { get; set; }
    }

    protected class OwnedGenericComparableIntClassKey(int position)
    {
        public int Position { get; set; } = position;
    }

    public abstract class KeysWithConvertersFixtureBase : SharedStoreFixtureBase<PoolableDbContext>
    {
        protected override string StoreName
            => "KeysWithConverters";

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            modelBuilder.Entity<IntStructKeyPrincipal>(
                b => { b.Property(e => e.Id).HasConversion(IntStructKey.Converter); });

            modelBuilder.Entity<IntStructKeyOptionalDependent>(
                b =>
                {
                    b.Property(e => e.Id).HasConversion(IntStructKey.Converter);
                    b.Property(e => e.PrincipalId);
                });

            modelBuilder.Entity<IntStructKeyRequiredDependent>(
                b =>
                {
                    b.Property(e => e.Id).HasConversion(IntStructKey.Converter);
                    b.Property(e => e.PrincipalId);
                });

            modelBuilder.Entity<IntClassKeyPrincipal>(
                b => { b.Property(e => e.Id).HasConversion(IntClassKey.Converter); });

            modelBuilder.Entity<IntClassKeyOptionalDependent>(
                b =>
                {
                    b.Property(e => e.Id).HasConversion(IntClassKey.Converter);
                    b.Property(e => e.PrincipalId).HasConversion(IntClassKey.Converter);
                });

            modelBuilder.Entity<IntClassKeyRequiredDependent>(
                b =>
                {
                    b.Property(e => e.Id).HasConversion(IntClassKey.Converter);
                    b.Property(e => e.PrincipalId).HasConversion(IntClassKey.Converter);
                });

            modelBuilder.Entity<EnumerableClassKeyPrincipal>(
                b => { b.Property(e => e.Id).HasConversion(EnumerableClassKey.Converter); });

            modelBuilder.Entity<EnumerableClassKeyOptionalDependent>(
                b =>
                {
                    b.Property(e => e.Id).HasConversion(EnumerableClassKey.Converter);
                    b.Property(e => e.PrincipalId);
                });

            modelBuilder.Entity<EnumerableClassKeyRequiredDependent>(
                b =>
                {
                    b.Property(e => e.Id).HasConversion(EnumerableClassKey.Converter);
                    b.Property(e => e.PrincipalId);
                });

            modelBuilder.Entity<BareIntClassKeyPrincipal>(
                b => { b.Property(e => e.Id).HasConversion(BareIntClassKey.Converter, BareIntClassKey.Comparer); });

            modelBuilder.Entity<BareIntClassKeyOptionalDependent>(
                b =>
                {
                    b.Property(e => e.Id).HasConversion(BareIntClassKey.Converter, BareIntClassKey.Comparer);
                    b.Property(e => e.PrincipalId).HasConversion(BareIntClassKey.Converter, BareIntClassKey.Comparer);
                });

            modelBuilder.Entity<BareIntClassKeyRequiredDependent>(
                b =>
                {
                    b.Property(e => e.Id).HasConversion(BareIntClassKey.Converter, BareIntClassKey.Comparer);
                    b.Property(e => e.PrincipalId).HasConversion(BareIntClassKey.Converter, BareIntClassKey.Comparer);
                });

            modelBuilder.Entity<ComparableIntStructKeyPrincipal>(
                b => { b.Property(e => e.Id).HasConversion(ComparableIntStructKey.Converter); });

            modelBuilder.Entity<ComparableIntStructKeyOptionalDependent>(
                b =>
                {
                    b.Property(e => e.Id).HasConversion(ComparableIntStructKey.Converter);
                    b.Property(e => e.PrincipalId);
                });

            modelBuilder.Entity<ComparableIntStructKeyRequiredDependent>(
                b =>
                {
                    b.Property(e => e.Id).HasConversion(ComparableIntStructKey.Converter);
                    b.Property(e => e.PrincipalId);
                });

            modelBuilder.Entity<GenericComparableIntStructKeyPrincipal>(
                b => { b.Property(e => e.Id).HasConversion(GenericComparableIntStructKey.Converter); });

            modelBuilder.Entity<GenericComparableIntStructKeyOptionalDependent>(
                b =>
                {
                    b.Property(e => e.Id).HasConversion(GenericComparableIntStructKey.Converter);
                    b.Property(e => e.PrincipalId).HasConversion(GenericComparableIntStructKey.Converter);
                });

            modelBuilder.Entity<GenericComparableIntStructKeyRequiredDependent>(
                b =>
                {
                    b.Property(e => e.Id).HasConversion(GenericComparableIntStructKey.Converter);
                    b.Property(e => e.PrincipalId).HasConversion(GenericComparableIntStructKey.Converter);
                });

            modelBuilder.Entity<StructuralComparableBytesStructKeyPrincipal>(
                b => { b.Property(e => e.Id).HasConversion(StructuralComparableBytesStructKey.Converter); });

            modelBuilder.Entity<StructuralComparableBytesStructKeyOptionalDependent>(
                b =>
                {
                    b.Property(e => e.Id).HasConversion(StructuralComparableBytesStructKey.Converter);
                    b.Property(e => e.PrincipalId);
                });

            modelBuilder.Entity<StructuralComparableBytesStructKeyRequiredDependent>(
                b =>
                {
                    b.Property(e => e.Id).HasConversion(StructuralComparableBytesStructKey.Converter);
                    b.Property(e => e.PrincipalId);
                });

            modelBuilder.Entity<BytesStructKeyPrincipal>(
                b => { b.Property(e => e.Id).HasConversion(BytesStructKey.Converter); });

            modelBuilder.Entity<BytesStructKeyOptionalDependent>(
                b =>
                {
                    b.Property(e => e.Id).HasConversion(BytesStructKey.Converter);
                    b.Property(e => e.PrincipalId).HasConversion(BytesStructKey.Converter);
                });

            modelBuilder.Entity<BytesStructKeyRequiredDependent>(
                b =>
                {
                    b.Property(e => e.Id).HasConversion(BytesStructKey.Converter);
                    b.Property(e => e.PrincipalId).HasConversion(BytesStructKey.Converter);
                });

            modelBuilder.Entity<ComparableBytesStructKeyPrincipal>(
                b => { b.Property(e => e.Id).HasConversion(ComparableBytesStructKey.Converter); });

            modelBuilder.Entity<ComparableBytesStructKeyOptionalDependent>(
                b =>
                {
                    b.Property(e => e.Id).HasConversion(ComparableBytesStructKey.Converter);
                    b.Property(e => e.PrincipalId);
                });

            modelBuilder.Entity<ComparableBytesStructKeyRequiredDependent>(
                b =>
                {
                    b.Property(e => e.Id).HasConversion(ComparableBytesStructKey.Converter);
                    b.Property(e => e.PrincipalId);
                });

            modelBuilder.Entity<GenericComparableBytesStructKeyPrincipal>(
                b => { b.Property(e => e.Id).HasConversion(GenericComparableBytesStructKey.Converter); });

            modelBuilder.Entity<GenericComparableBytesStructKeyOptionalDependent>(
                b =>
                {
                    b.Property(e => e.Id).HasConversion(GenericComparableBytesStructKey.Converter);
                    b.Property(e => e.PrincipalId).HasConversion(GenericComparableBytesStructKey.Converter);
                });

            modelBuilder.Entity<GenericComparableBytesStructKeyRequiredDependent>(
                b =>
                {
                    b.Property(e => e.Id).HasConversion(GenericComparableBytesStructKey.Converter);
                    b.Property(e => e.PrincipalId).HasConversion(GenericComparableBytesStructKey.Converter);
                });

            modelBuilder.Entity<ComparableIntClassKeyPrincipal>(
                b => { b.Property(e => e.Id).HasConversion(ComparableIntClassKey.Converter); });

            modelBuilder.Entity<ComparableIntClassKeyOptionalDependent>(
                b =>
                {
                    b.Property(e => e.Id).HasConversion(ComparableIntClassKey.Converter);
                    b.Property(e => e.PrincipalId);
                });

            modelBuilder.Entity<ComparableIntClassKeyRequiredDependent>(
                b =>
                {
                    b.Property(e => e.Id).HasConversion(ComparableIntClassKey.Converter);
                    b.Property(e => e.PrincipalId);
                });

            modelBuilder.Entity<GenericComparableIntClassKeyPrincipal>(
                b => { b.Property(e => e.Id).HasConversion(GenericComparableIntClassKey.Converter); });

            modelBuilder.Entity<GenericComparableIntClassKeyOptionalDependent>(
                b =>
                {
                    b.Property(e => e.Id).HasConversion(GenericComparableIntClassKey.Converter);
                    b.Property(e => e.PrincipalId).HasConversion(GenericComparableIntClassKey.Converter);
                });

            modelBuilder.Entity<GenericComparableIntClassKeyRequiredDependent>(
                b =>
                {
                    b.Property(e => e.Id).HasConversion(GenericComparableIntClassKey.Converter);
                    b.Property(e => e.PrincipalId).HasConversion(GenericComparableIntClassKey.Converter);
                });

            modelBuilder.Entity<BaseEntity>(
                entity =>
                {
                    entity.HasKey(e => e.Name);

                    entity.Property(p => p.Name)
                        .HasConversion(
                            p => p.Value,
                            p => new Key(p),
                            new ValueComparer<Key>(
                                (l, r) => (l == null && r == null) || (l != null && r != null && l.Value == r.Value),
                                v => v == null ? 0 : v.Value.GetHashCode()));

                    entity.OwnsOne(p => p.Text);
                    entity.Navigation(p => p.Text).IsRequired();
                });

            modelBuilder.Entity<IntStructKeyPrincipalShadow>(
                b => { b.Property(e => e.Id).HasConversion(IntStructKey.Converter); });

            modelBuilder.Entity<IntStructKeyOptionalDependentShadow>(
                b => { b.Property(e => e.Id).HasConversion(IntStructKey.Converter); });

            modelBuilder.Entity<IntStructKeyRequiredDependentShadow>(
                b => { b.Property(e => e.Id).HasConversion(IntStructKey.Converter); });

            modelBuilder.Entity<IntClassKeyPrincipalShadow>(
                b => { b.Property(e => e.Id).HasConversion(IntClassKey.Converter); });

            modelBuilder.Entity<IntClassKeyOptionalDependentShadow>(
                b => { b.Property(e => e.Id).HasConversion(IntClassKey.Converter); });

            modelBuilder.Entity<IntClassKeyRequiredDependentShadow>(
                b => { b.Property(e => e.Id).HasConversion(IntClassKey.Converter); });

            modelBuilder.Entity<BareIntClassKeyPrincipalShadow>(
                b => { b.Property(e => e.Id).HasConversion(BareIntClassKey.Converter, BareIntClassKey.Comparer); });

            modelBuilder.Entity<BareIntClassKeyOptionalDependentShadow>(
                b => { b.Property(e => e.Id).HasConversion(BareIntClassKey.Converter, BareIntClassKey.Comparer); });

            modelBuilder.Entity<BareIntClassKeyRequiredDependentShadow>(
                b => { b.Property(e => e.Id).HasConversion(BareIntClassKey.Converter, BareIntClassKey.Comparer); });

            modelBuilder.Entity<ComparableIntStructKeyPrincipalShadow>(
                b => { b.Property(e => e.Id).HasConversion(ComparableIntStructKey.Converter); });

            modelBuilder.Entity<ComparableIntStructKeyOptionalDependentShadow>(
                b => { b.Property(e => e.Id).HasConversion(ComparableIntStructKey.Converter); });

            modelBuilder.Entity<ComparableIntStructKeyRequiredDependentShadow>(
                b => { b.Property(e => e.Id).HasConversion(ComparableIntStructKey.Converter); });

            modelBuilder.Entity<GenericComparableIntStructKeyPrincipalShadow>(
                b => { b.Property(e => e.Id).HasConversion(GenericComparableIntStructKey.Converter); });

            modelBuilder.Entity<GenericComparableIntStructKeyOptionalDependentShadow>(
                b => { b.Property(e => e.Id).HasConversion(GenericComparableIntStructKey.Converter); });

            modelBuilder.Entity<GenericComparableIntStructKeyRequiredDependentShadow>(
                b => { b.Property(e => e.Id).HasConversion(GenericComparableIntStructKey.Converter); });

            modelBuilder.Entity<StructuralComparableBytesStructKeyPrincipalShadow>(
                b => { b.Property(e => e.Id).HasConversion(StructuralComparableBytesStructKey.Converter); });

            modelBuilder.Entity<StructuralComparableBytesStructKeyOptionalDependentShadow>(
                b => { b.Property(e => e.Id).HasConversion(StructuralComparableBytesStructKey.Converter); });

            modelBuilder.Entity<StructuralComparableBytesStructKeyRequiredDependentShadow>(
                b => { b.Property(e => e.Id).HasConversion(StructuralComparableBytesStructKey.Converter); });

            modelBuilder.Entity<BytesStructKeyPrincipalShadow>(
                b => { b.Property(e => e.Id).HasConversion(BytesStructKey.Converter); });

            modelBuilder.Entity<BytesStructKeyOptionalDependentShadow>(
                b => { b.Property(e => e.Id).HasConversion(BytesStructKey.Converter); });

            modelBuilder.Entity<BytesStructKeyRequiredDependentShadow>(
                b => { b.Property(e => e.Id).HasConversion(BytesStructKey.Converter); });

            modelBuilder.Entity<ComparableBytesStructKeyPrincipalShadow>(
                b => { b.Property(e => e.Id).HasConversion(ComparableBytesStructKey.Converter); });

            modelBuilder.Entity<ComparableBytesStructKeyOptionalDependentShadow>(
                b => { b.Property(e => e.Id).HasConversion(ComparableBytesStructKey.Converter); });

            modelBuilder.Entity<ComparableBytesStructKeyRequiredDependentShadow>(
                b => { b.Property(e => e.Id).HasConversion(ComparableBytesStructKey.Converter); });

            modelBuilder.Entity<GenericComparableBytesStructKeyPrincipalShadow>(
                b => { b.Property(e => e.Id).HasConversion(GenericComparableBytesStructKey.Converter); });

            modelBuilder.Entity<GenericComparableBytesStructKeyOptionalDependentShadow>(
                b => { b.Property(e => e.Id).HasConversion(GenericComparableBytesStructKey.Converter); });

            modelBuilder.Entity<GenericComparableBytesStructKeyRequiredDependentShadow>(
                b => { b.Property(e => e.Id).HasConversion(GenericComparableBytesStructKey.Converter); });

            modelBuilder.Entity<ComparableIntClassKeyPrincipalShadow>(
                b => { b.Property(e => e.Id).HasConversion(ComparableIntClassKey.Converter); });

            modelBuilder.Entity<ComparableIntClassKeyOptionalDependentShadow>(
                b => { b.Property(e => e.Id).HasConversion(ComparableIntClassKey.Converter); });

            modelBuilder.Entity<ComparableIntClassKeyRequiredDependentShadow>(
                b => { b.Property(e => e.Id).HasConversion(ComparableIntClassKey.Converter); });

            modelBuilder.Entity<GenericComparableIntClassKeyPrincipalShadow>(
                b => { b.Property(e => e.Id).HasConversion(GenericComparableIntClassKey.Converter); });

            modelBuilder.Entity<GenericComparableIntClassKeyOptionalDependentShadow>(
                b => { b.Property(e => e.Id).HasConversion(GenericComparableIntClassKey.Converter); });

            modelBuilder.Entity<GenericComparableIntClassKeyRequiredDependentShadow>(
                b => { b.Property(e => e.Id).HasConversion(GenericComparableIntClassKey.Converter); });

            modelBuilder.Entity<OwnerIntStructKey>(
                b =>
                {
                    b.Property(e => e.Id).HasConversion(IntStructKey.Converter);
                    b.OwnsOne(e => e.Owned);
                });

            modelBuilder.Entity<OwnerBytesStructKey>(
                b =>
                {
                    b.Property(e => e.Id).HasConversion(BytesStructKey.Converter);
                    b.OwnsOne(e => e.Owned);
                });

            modelBuilder.Entity<OwnerComparableIntStructKey>(
                b =>
                {
                    b.Property(e => e.Id).HasConversion(ComparableIntStructKey.Converter);
                    b.OwnsOne(e => e.Owned);
                });

            modelBuilder.Entity<OwnerComparableBytesStructKey>(
                b =>
                {
                    b.Property(e => e.Id).HasConversion(ComparableBytesStructKey.Converter);
                    b.OwnsOne(e => e.Owned);
                });

            modelBuilder.Entity<OwnerGenericComparableIntStructKey>(
                b =>
                {
                    b.Property(e => e.Id).HasConversion(GenericComparableIntStructKey.Converter);
                    b.OwnsOne(e => e.Owned);
                });

            modelBuilder.Entity<OwnerGenericComparableBytesStructKey>(
                b =>
                {
                    b.Property(e => e.Id).HasConversion(GenericComparableBytesStructKey.Converter);
                    b.OwnsOne(e => e.Owned);
                });

            modelBuilder.Entity<OwnerStructuralComparableBytesStructKey>(
                b =>
                {
                    b.Property(e => e.Id).HasConversion(StructuralComparableBytesStructKey.Converter);
                    b.OwnsOne(e => e.Owned);
                });

            modelBuilder.Entity<OwnerIntClassKey>(
                b =>
                {
                    b.Property(e => e.Id).HasConversion(IntClassKey.Converter);
                    b.OwnsOne(e => e.Owned);
                });

            modelBuilder.Entity<OwnerBareIntClassKey>(
                b =>
                {
                    b.Property(e => e.Id).HasConversion(BareIntClassKey.Converter, BareIntClassKey.Comparer);
                    b.OwnsOne(e => e.Owned);
                });

            modelBuilder.Entity<OwnerComparableIntClassKey>(
                b =>
                {
                    b.Property(e => e.Id).HasConversion(ComparableIntClassKey.Converter);
                    b.OwnsOne(e => e.Owned);
                });

            modelBuilder.Entity<OwnerGenericComparableIntClassKey>(
                b =>
                {
                    b.Property(e => e.Id).HasConversion(GenericComparableIntClassKey.Converter);
                    b.OwnsOne(e => e.Owned);
                });
        }
    }
}
