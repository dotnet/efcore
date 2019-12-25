// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public abstract class KeysWithConvertersTestBase<TFixture> : IClassFixture<TFixture>
        where TFixture : KeysWithConvertersTestBase<TFixture>.KeysWithConvertersFixtureBase, new()
    {
        protected KeysWithConvertersTestBase(TFixture fixture) => Fixture = fixture;

        protected TFixture Fixture { get; }

        protected DbContext CreateContext() => Fixture.CreateContext();

        [ConditionalFact]
        public virtual void Can_insert_and_read_back_with_struct_key_and_optional_dependents()
        {
            InsertOptionalGraph<IntStructKeyPrincipal, IntStructKeyOptionalDependent>();

            using (var context = CreateContext())
            {
                RunQueries(context, out var principals, out var dependents);

                Validate(
                    principals,
                    dependents,
                    new[] { (0, new[] { 0 }), (1, new[] { 1 }), (2, new[] { 2, 2, 2 }), (3, new int[0]) },
                    new (int, int?)[] { (0, 0), (1, 1), (2, 2), (3, 2), (4, 2), (5, null) });

                foreach (var principal in principals)
                {
                    principal.Foo = "Mutant!";
                }

                dependents[5].Principal = principals[0];
                dependents[4].PrincipalId = null;
                dependents[3].PrincipalId = principals[0].Id;
                principals[1].OptionalDependents.Clear();

                context.SaveChanges();
            }

            using (var context = CreateContext())
            {
                RunQueries(context, out var principals, out var dependents);

                Validate(
                    principals,
                    dependents,
                    new[] { (0, new[] { 0, 3, 5 }), (1, new int[0]), (2, new[] { 2 }), (3, new int[0]) },
                    new (int, int?)[] { (0, 0), (1, null), (2, 2), (3, 0), (4, null), (5, 0) });
            }

            void RunQueries(
                DbContext context,
                out IntStructKeyPrincipal[] principals,
                out IntStructKeyOptionalDependent[] dependents)
            {
                var two = 2;
                var three = new IntStructKey { Id = 3 };

                principals = new[]
                {
                    context.Set<IntStructKeyPrincipal>().Include(e => e.OptionalDependents).Single(e => e.Id.Equals(new IntStructKey { Id = 1 })),
                    context.Set<IntStructKeyPrincipal>().Include(e => e.OptionalDependents).Single(e => e.Id.Equals(new IntStructKey { Id = two })),
                    context.Set<IntStructKeyPrincipal>().Include(e => e.OptionalDependents).Single(e => e.Id.Equals(three)),
                    context.Set<IntStructKeyPrincipal>().Include(e => e.OptionalDependents).Single(e => e.Id.Equals(new IntStructKey { Id = 4 }))
                };

                var oneOhTwo = 102;
                var oneOhThree = new IntStructKey { Id = 103 };
                var oneOhFive = 105;
                var oneOhSix = new IntStructKey { Id = 106 };

                dependents = new[]
                {
                    context.Set<IntStructKeyOptionalDependent>().Single(e => e.Id.Equals(new IntStructKey { Id = 101 })),
                    context.Set<IntStructKeyOptionalDependent>().Single(e => e.Id.Equals(new IntStructKey { Id = oneOhTwo })),
                    context.Set<IntStructKeyOptionalDependent>().Single(e => e.Id.Equals(oneOhThree)),
                    context.Set<IntStructKeyOptionalDependent>().Single(e => e.Id == new IntStructKey { Id = 104 }),
                    context.Set<IntStructKeyOptionalDependent>().Single(e => e.Id == new IntStructKey { Id = oneOhFive }),
                    context.Set<IntStructKeyOptionalDependent>().Single(e => e.Id == oneOhSix)
                };

                Assert.Same(dependents[0], context.Set<IntStructKeyOptionalDependent>().Find(new IntStructKey { Id = 101 }));
                Assert.Same(dependents[1], context.Set<IntStructKeyOptionalDependent>().Find(new IntStructKey { Id = oneOhTwo }));
                Assert.Same(dependents[2], context.Set<IntStructKeyOptionalDependent>().Find(oneOhThree));
                Assert.Same(dependents[3], context.Find(typeof(IntStructKeyOptionalDependent), new IntStructKey { Id = 104 }));
                Assert.Same(dependents[4], context.Find(typeof(IntStructKeyOptionalDependent), new IntStructKey { Id = oneOhFive }));
                Assert.Same(dependents[5], context.Find(typeof(IntStructKeyOptionalDependent), oneOhSix));
           }

            void Validate(
                IntStructKeyPrincipal[] principals,
                IntStructKeyOptionalDependent[] dependents,
                (int, int[])[] expectedPrincipalToDependents,
                (int, int?)[] expectedDependentToPrincipals)
            {
                ValidateOptional(
                    principals,
                    dependents,
                    expectedPrincipalToDependents,
                    expectedDependentToPrincipals,
                    p => ((IntStructKeyPrincipal)p).OptionalDependents.Select(d => (IIntOptionalDependent)d).ToList(),
                    d => ((IntStructKeyOptionalDependent)d).Principal);
            }
        }

        [ConditionalFact]
        public virtual void Can_insert_and_read_back_with_comparable_struct_key_and_optional_dependents()
        {
            InsertOptionalGraph<ComparableIntStructKeyPrincipal, ComparableIntStructKeyOptionalDependent>();

            using (var context = CreateContext())
            {
                RunQueries(context, out var principals, out var dependents);

                Validate(
                    principals,
                    dependents,
                    new[] { (0, new[] { 0 }), (1, new[] { 1 }), (2, new[] { 2, 2, 2 }), (3, new int[0]) },
                    new (int, int?)[] { (0, 0), (1, 1), (2, 2), (3, 2), (4, 2), (5, null) });

                foreach (var principal in principals)
                {
                    principal.Foo = "Mutant!";
                }

                dependents[5].Principal = principals[0];
                dependents[4].PrincipalId = null;
                dependents[3].PrincipalId = principals[0].Id;
                principals[1].OptionalDependents.Clear();

                context.SaveChanges();
            }

            using (var context = CreateContext())
            {
                RunQueries(context, out var principals, out var dependents);

                Validate(
                    principals,
                    dependents,
                    new[] { (0, new[] { 0, 3, 5 }), (1, new int[0]), (2, new[] { 2 }), (3, new int[0]) },
                    new (int, int?)[] { (0, 0), (1, null), (2, 2), (3, 0), (4, null), (5, 0) });
            }

            void RunQueries(
                DbContext context,
                out ComparableIntStructKeyPrincipal[] principals,
                out ComparableIntStructKeyOptionalDependent[] dependents)
            {
                var two = 2;
                var three = new ComparableIntStructKey { Id = 3 };

                principals = new[]
                {
                    context.Set<ComparableIntStructKeyPrincipal>().Include(e => e.OptionalDependents).Single(e => e.Id.Equals(new ComparableIntStructKey { Id = 1 })),
                    context.Set<ComparableIntStructKeyPrincipal>().Include(e => e.OptionalDependents).Single(e => e.Id.Equals(new ComparableIntStructKey { Id = two })),
                    context.Set<ComparableIntStructKeyPrincipal>().Include(e => e.OptionalDependents).Single(e => e.Id.Equals(three)),
                    context.Set<ComparableIntStructKeyPrincipal>().Include(e => e.OptionalDependents).Single(e => e.Id.Equals(new ComparableIntStructKey { Id = 4 }))
                };

                var oneOhTwo = 102;
                var oneOhThree = new ComparableIntStructKey { Id = 103 };
                var oneOhFive = 105;
                var oneOhSix = new ComparableIntStructKey { Id = 106 };

                dependents = new[]
                {
                    context.Set<ComparableIntStructKeyOptionalDependent>().Single(e => e.Id.Equals(new ComparableIntStructKey { Id = 101 })),
                    context.Set<ComparableIntStructKeyOptionalDependent>().Single(e => e.Id.Equals(new ComparableIntStructKey { Id = oneOhTwo })),
                    context.Set<ComparableIntStructKeyOptionalDependent>().Single(e => e.Id.Equals(oneOhThree)),
                    context.Set<ComparableIntStructKeyOptionalDependent>().Single(e => e.Id == new ComparableIntStructKey { Id = 104 }),
                    context.Set<ComparableIntStructKeyOptionalDependent>().Single(e => e.Id == new ComparableIntStructKey { Id = oneOhFive }),
                    context.Set<ComparableIntStructKeyOptionalDependent>().Single(e => e.Id == oneOhSix)
                };

                Assert.Same(dependents[0], context.Set<ComparableIntStructKeyOptionalDependent>().Find(new ComparableIntStructKey { Id = 101 }));
                Assert.Same(dependents[1], context.Set<ComparableIntStructKeyOptionalDependent>().Find(new ComparableIntStructKey { Id = oneOhTwo }));
                Assert.Same(dependents[2], context.Set<ComparableIntStructKeyOptionalDependent>().Find(oneOhThree));
                Assert.Same(dependents[3], context.Find(typeof(ComparableIntStructKeyOptionalDependent), new ComparableIntStructKey { Id = 104 }));
                Assert.Same(dependents[4], context.Find(typeof(ComparableIntStructKeyOptionalDependent), new ComparableIntStructKey { Id = oneOhFive }));
                Assert.Same(dependents[5], context.Find(typeof(ComparableIntStructKeyOptionalDependent), oneOhSix));
           }

            void Validate(
                ComparableIntStructKeyPrincipal[] principals,
                ComparableIntStructKeyOptionalDependent[] dependents,
                (int, int[])[] expectedPrincipalToDependents,
                (int, int?)[] expectedDependentToPrincipals)
            {
                ValidateOptional(
                    principals,
                    dependents,
                    expectedPrincipalToDependents,
                    expectedDependentToPrincipals,
                    p => ((ComparableIntStructKeyPrincipal)p).OptionalDependents.Select(d => (IIntOptionalDependent)d).ToList(),
                    d => ((ComparableIntStructKeyOptionalDependent)d).Principal);
            }
        }

        [ConditionalFact]
        public virtual void Can_insert_and_read_back_with_generic_comparable_struct_key_and_optional_dependents()
        {
            InsertOptionalGraph<GenericComparableIntStructKeyPrincipal, GenericComparableIntStructKeyOptionalDependent>();

            using (var context = CreateContext())
            {
                RunQueries(context, out var principals, out var dependents);

                Validate(
                    principals,
                    dependents,
                    new[] { (0, new[] { 0 }), (1, new[] { 1 }), (2, new[] { 2, 2, 2 }), (3, new int[0]) },
                    new (int, int?)[] { (0, 0), (1, 1), (2, 2), (3, 2), (4, 2), (5, null) });

                foreach (var principal in principals)
                {
                    principal.Foo = "Mutant!";
                }

                dependents[5].Principal = principals[0];
                dependents[4].PrincipalId = null;
                dependents[3].PrincipalId = principals[0].Id;
                principals[1].OptionalDependents.Clear();

                context.SaveChanges();
            }

            using (var context = CreateContext())
            {
                RunQueries(context, out var principals, out var dependents);

                Validate(
                    principals,
                    dependents,
                    new[] { (0, new[] { 0, 3, 5 }), (1, new int[0]), (2, new[] { 2 }), (3, new int[0]) },
                    new (int, int?)[] { (0, 0), (1, null), (2, 2), (3, 0), (4, null), (5, 0) });
            }

            void RunQueries(
                DbContext context,
                out GenericComparableIntStructKeyPrincipal[] principals,
                out GenericComparableIntStructKeyOptionalDependent[] dependents)
            {
                var two = 2;
                var three = new GenericComparableIntStructKey { Id = 3 };

                principals = new[]
                {
                    context.Set<GenericComparableIntStructKeyPrincipal>().Include(e => e.OptionalDependents).Single(e => e.Id.Equals(new GenericComparableIntStructKey { Id = 1 })),
                    context.Set<GenericComparableIntStructKeyPrincipal>().Include(e => e.OptionalDependents).Single(e => e.Id.Equals(new GenericComparableIntStructKey { Id = two })),
                    context.Set<GenericComparableIntStructKeyPrincipal>().Include(e => e.OptionalDependents).Single(e => e.Id.Equals(three)),
                    context.Set<GenericComparableIntStructKeyPrincipal>().Include(e => e.OptionalDependents).Single(e => e.Id.Equals(new GenericComparableIntStructKey { Id = 4 }))
                };

                var oneOhTwo = 102;
                var oneOhThree = new GenericComparableIntStructKey { Id = 103 };
                var oneOhFive = 105;
                var oneOhSix = new GenericComparableIntStructKey { Id = 106 };

                dependents = new[]
                {
                    context.Set<GenericComparableIntStructKeyOptionalDependent>().Single(e => e.Id.Equals(new GenericComparableIntStructKey { Id = 101 })),
                    context.Set<GenericComparableIntStructKeyOptionalDependent>().Single(e => e.Id.Equals(new GenericComparableIntStructKey { Id = oneOhTwo })),
                    context.Set<GenericComparableIntStructKeyOptionalDependent>().Single(e => e.Id.Equals(oneOhThree)),
                    context.Set<GenericComparableIntStructKeyOptionalDependent>().Single(e => e.Id == new GenericComparableIntStructKey { Id = 104 }),
                    context.Set<GenericComparableIntStructKeyOptionalDependent>().Single(e => e.Id == new GenericComparableIntStructKey { Id = oneOhFive }),
                    context.Set<GenericComparableIntStructKeyOptionalDependent>().Single(e => e.Id == oneOhSix)
                };

                Assert.Same(dependents[0], context.Set<GenericComparableIntStructKeyOptionalDependent>().Find(new GenericComparableIntStructKey { Id = 101 }));
                Assert.Same(dependents[1], context.Set<GenericComparableIntStructKeyOptionalDependent>().Find(new GenericComparableIntStructKey { Id = oneOhTwo }));
                Assert.Same(dependents[2], context.Set<GenericComparableIntStructKeyOptionalDependent>().Find(oneOhThree));
                Assert.Same(dependents[3], context.Find(typeof(GenericComparableIntStructKeyOptionalDependent), new GenericComparableIntStructKey { Id = 104 }));
                Assert.Same(dependents[4], context.Find(typeof(GenericComparableIntStructKeyOptionalDependent), new GenericComparableIntStructKey { Id = oneOhFive }));
                Assert.Same(dependents[5], context.Find(typeof(GenericComparableIntStructKeyOptionalDependent), oneOhSix));
           }

            void Validate(
                GenericComparableIntStructKeyPrincipal[] principals,
                GenericComparableIntStructKeyOptionalDependent[] dependents,
                (int, int[])[] expectedPrincipalToDependents,
                (int, int?)[] expectedDependentToPrincipals)
            {
                ValidateOptional(
                    principals,
                    dependents,
                    expectedPrincipalToDependents,
                    expectedDependentToPrincipals,
                    p => ((GenericComparableIntStructKeyPrincipal)p).OptionalDependents.Select(d => (IIntOptionalDependent)d).ToList(),
                    d => ((GenericComparableIntStructKeyOptionalDependent)d).Principal);
            }
        }

        [ConditionalFact]
        public virtual void Can_insert_and_read_back_with_struct_key_and_required_dependents()
        {
            InsertRequiredGraph<IntStructKeyPrincipal, IntStructKeyRequiredDependent>();

            using (var context = CreateContext())
            {
                RunQueries(context, out var principals, out var dependents);

                Validate(
                    principals,
                    dependents,
                    new[] { (0, new[] { 0 }), (1, new[] { 1 }), (2, new[] { 2, 2, 2, 2 }), (3, new int[0]) },
                    new[] { (0, 0), (1, 1), (2, 2), (3, 2), (4, 2), (5, 2) });

                foreach (var principal in principals)
                {
                    principal.Foo = "Mutant!";
                }

                dependents[5].Principal = principals[0];
                context.Remove(dependents[4]);
                dependents[3].PrincipalId = principals[0].Id;
                principals[1].RequiredDependents.Clear();

                context.SaveChanges();
            }

            using (var context = CreateContext())
            {
                RunQueries(context, out var principals, out var dependents);

                Validate(
                    principals,
                    dependents,
                    new[] { (0, new[] { 0, 3, 5 }), (1, new int[0]), (2, new[] { 2 }), (3, new int[0]) },
                    new[] { (0, 0), (2, 2), (3, 0), (5, 0) });
            }

            void RunQueries(
                DbContext context,
                out IntStructKeyPrincipal[] principals,
                out IntStructKeyRequiredDependent[] dependents)
            {
                var twelve = 12;
                var thirteen = new IntStructKey { Id = 13 };

                principals = new[]
                {
                    context.Set<IntStructKeyPrincipal>().Include(e => e.RequiredDependents).Single(e => e.Id.Equals(new IntStructKey { Id = 11 })),
                    context.Set<IntStructKeyPrincipal>().Include(e => e.RequiredDependents).Single(e => e.Id.Equals(new IntStructKey { Id = twelve })),
                    context.Set<IntStructKeyPrincipal>().Include(e => e.RequiredDependents).Single(e => e.Id.Equals(thirteen)),
                    context.Set<IntStructKeyPrincipal>().Include(e => e.RequiredDependents).Single(e => e.Id.Equals(new IntStructKey { Id = 14 }))
                };

                var oneTwelve = 112;
                var oneThirteen = new IntStructKey { Id = 113 };
                var oneFifteeen = 115;
                var oneSixteen = new IntStructKey { Id = 116 };

                dependents = new[]
                {
                    context.Set<IntStructKeyRequiredDependent>().FirstOrDefault(e => e.Id.Equals(new IntStructKey { Id = 111 })),
                    context.Set<IntStructKeyRequiredDependent>().FirstOrDefault(e => e.Id.Equals(new IntStructKey { Id = oneTwelve })),
                    context.Set<IntStructKeyRequiredDependent>().FirstOrDefault(e => e.Id.Equals(oneThirteen)),
                    context.Set<IntStructKeyRequiredDependent>().FirstOrDefault(e => e.Id == new IntStructKey { Id = 114 }),
                    context.Set<IntStructKeyRequiredDependent>().FirstOrDefault(e => e.Id == new IntStructKey { Id = oneFifteeen }),
                    context.Set<IntStructKeyRequiredDependent>().FirstOrDefault(e => e.Id == oneSixteen)
                };

                Assert.Same(dependents[0], context.Set<IntStructKeyRequiredDependent>().Find(new IntStructKey { Id = 111 }));
                Assert.Same(dependents[1], context.Set<IntStructKeyRequiredDependent>().Find(new IntStructKey { Id = oneTwelve }));
                Assert.Same(dependents[2], context.Set<IntStructKeyRequiredDependent>().Find(oneThirteen));
                Assert.Same(dependents[3], context.Find(typeof(IntStructKeyRequiredDependent), new IntStructKey { Id = 114 }));
                Assert.Same(dependents[4], context.Find(typeof(IntStructKeyRequiredDependent), new IntStructKey { Id = oneFifteeen }));
                Assert.Same(dependents[5], context.Find(typeof(IntStructKeyRequiredDependent), oneSixteen));
           }

            void Validate(
                IntStructKeyPrincipal[] principals,
                IntStructKeyRequiredDependent[] dependents,
                (int, int[])[] expectedPrincipalToDependents,
                (int, int)[] expectedDependentToPrincipals)
            {
                ValidateRequired(
                    principals,
                    dependents,
                    expectedPrincipalToDependents,
                    expectedDependentToPrincipals,
                    p => ((IntStructKeyPrincipal)p).RequiredDependents.Select(d => (IIntRequiredDependent)d).ToList(),
                    d => ((IntStructKeyRequiredDependent)d).Principal);
            }
        }

        [ConditionalFact]
        public virtual void Can_insert_and_read_back_with_comparable_struct_key_and_required_dependents()
        {
            InsertRequiredGraph<ComparableIntStructKeyPrincipal, ComparableIntStructKeyRequiredDependent>();

            using (var context = CreateContext())
            {
                RunQueries(context, out var principals, out var dependents);

                Validate(
                    principals,
                    dependents,
                    new[] { (0, new[] { 0 }), (1, new[] { 1 }), (2, new[] { 2, 2, 2, 2 }), (3, new int[0]) },
                    new[] { (0, 0), (1, 1), (2, 2), (3, 2), (4, 2), (5, 2) });

                foreach (var principal in principals)
                {
                    principal.Foo = "Mutant!";
                }

                dependents[5].Principal = principals[0];
                context.Remove(dependents[4]);
                dependents[3].PrincipalId = principals[0].Id;
                principals[1].RequiredDependents.Clear();

                context.SaveChanges();
            }

            using (var context = CreateContext())
            {
                RunQueries(context, out var principals, out var dependents);

                Validate(
                    principals,
                    dependents,
                    new[] { (0, new[] { 0, 3, 5 }), (1, new int[0]), (2, new[] { 2 }), (3, new int[0]) },
                    new[] { (0, 0), (2, 2), (3, 0), (5, 0) });
            }

            void RunQueries(
                DbContext context,
                out ComparableIntStructKeyPrincipal[] principals,
                out ComparableIntStructKeyRequiredDependent[] dependents)
            {
                var twelve = 12;
                var thirteen = new ComparableIntStructKey { Id = 13 };

                principals = new[]
                {
                    context.Set<ComparableIntStructKeyPrincipal>().Include(e => e.RequiredDependents).Single(e => e.Id.Equals(new ComparableIntStructKey { Id = 11 })),
                    context.Set<ComparableIntStructKeyPrincipal>().Include(e => e.RequiredDependents).Single(e => e.Id.Equals(new ComparableIntStructKey { Id = twelve })),
                    context.Set<ComparableIntStructKeyPrincipal>().Include(e => e.RequiredDependents).Single(e => e.Id.Equals(thirteen)),
                    context.Set<ComparableIntStructKeyPrincipal>().Include(e => e.RequiredDependents).Single(e => e.Id.Equals(new ComparableIntStructKey { Id = 14 }))
                };

                var oneTwelve = 112;
                var oneThirteen = new ComparableIntStructKey { Id = 113 };
                var oneFifteeen = 115;
                var oneSixteen = new ComparableIntStructKey { Id = 116 };

                dependents = new[]
                {
                    context.Set<ComparableIntStructKeyRequiredDependent>().FirstOrDefault(e => e.Id.Equals(new ComparableIntStructKey { Id = 111 })),
                    context.Set<ComparableIntStructKeyRequiredDependent>().FirstOrDefault(e => e.Id.Equals(new ComparableIntStructKey { Id = oneTwelve })),
                    context.Set<ComparableIntStructKeyRequiredDependent>().FirstOrDefault(e => e.Id.Equals(oneThirteen)),
                    context.Set<ComparableIntStructKeyRequiredDependent>().FirstOrDefault(e => e.Id == new ComparableIntStructKey { Id = 114 }),
                    context.Set<ComparableIntStructKeyRequiredDependent>().FirstOrDefault(e => e.Id == new ComparableIntStructKey { Id = oneFifteeen }),
                    context.Set<ComparableIntStructKeyRequiredDependent>().FirstOrDefault(e => e.Id == oneSixteen)
                };

                Assert.Same(dependents[0], context.Set<ComparableIntStructKeyRequiredDependent>().Find(new ComparableIntStructKey { Id = 111 }));
                Assert.Same(dependents[1], context.Set<ComparableIntStructKeyRequiredDependent>().Find(new ComparableIntStructKey { Id = oneTwelve }));
                Assert.Same(dependents[2], context.Set<ComparableIntStructKeyRequiredDependent>().Find(oneThirteen));
                Assert.Same(dependents[3], context.Find(typeof(ComparableIntStructKeyRequiredDependent), new ComparableIntStructKey { Id = 114 }));
                Assert.Same(dependents[4], context.Find(typeof(ComparableIntStructKeyRequiredDependent), new ComparableIntStructKey { Id = oneFifteeen }));
                Assert.Same(dependents[5], context.Find(typeof(ComparableIntStructKeyRequiredDependent), oneSixteen));
           }

            void Validate(
                ComparableIntStructKeyPrincipal[] principals,
                ComparableIntStructKeyRequiredDependent[] dependents,
                (int, int[])[] expectedPrincipalToDependents,
                (int, int)[] expectedDependentToPrincipals)
            {
                ValidateRequired(
                    principals,
                    dependents,
                    expectedPrincipalToDependents,
                    expectedDependentToPrincipals,
                    p => ((ComparableIntStructKeyPrincipal)p).RequiredDependents.Select(d => (IIntRequiredDependent)d).ToList(),
                    d => ((ComparableIntStructKeyRequiredDependent)d).Principal);
            }
        }

        [ConditionalFact]
        public virtual void Can_insert_and_read_back_with_generic_comparable_struct_key_and_required_dependents()
        {
            InsertRequiredGraph<GenericComparableIntStructKeyPrincipal, GenericComparableIntStructKeyRequiredDependent>();

            using (var context = CreateContext())
            {
                RunQueries(context, out var principals, out var dependents);

                Validate(
                    principals,
                    dependents,
                    new[] { (0, new[] { 0 }), (1, new[] { 1 }), (2, new[] { 2, 2, 2, 2 }), (3, new int[0]) },
                    new[] { (0, 0), (1, 1), (2, 2), (3, 2), (4, 2), (5, 2) });

                foreach (var principal in principals)
                {
                    principal.Foo = "Mutant!";
                }

                dependents[5].Principal = principals[0];
                context.Remove(dependents[4]);
                dependents[3].PrincipalId = principals[0].Id;
                principals[1].RequiredDependents.Clear();

                context.SaveChanges();
            }

            using (var context = CreateContext())
            {
                RunQueries(context, out var principals, out var dependents);

                Validate(
                    principals,
                    dependents,
                    new[] { (0, new[] { 0, 3, 5 }), (1, new int[0]), (2, new[] { 2 }), (3, new int[0]) },
                    new[] { (0, 0), (2, 2), (3, 0), (5, 0) });
            }

            void RunQueries(
                DbContext context,
                out GenericComparableIntStructKeyPrincipal[] principals,
                out GenericComparableIntStructKeyRequiredDependent[] dependents)
            {
                var twelve = 12;
                var thirteen = new GenericComparableIntStructKey { Id = 13 };

                principals = new[]
                {
                    context.Set<GenericComparableIntStructKeyPrincipal>().Include(e => e.RequiredDependents).Single(e => e.Id.Equals(new GenericComparableIntStructKey { Id = 11 })),
                    context.Set<GenericComparableIntStructKeyPrincipal>().Include(e => e.RequiredDependents).Single(e => e.Id.Equals(new GenericComparableIntStructKey { Id = twelve })),
                    context.Set<GenericComparableIntStructKeyPrincipal>().Include(e => e.RequiredDependents).Single(e => e.Id.Equals(thirteen)),
                    context.Set<GenericComparableIntStructKeyPrincipal>().Include(e => e.RequiredDependents).Single(e => e.Id.Equals(new GenericComparableIntStructKey { Id = 14 }))
                };

                var oneTwelve = 112;
                var oneThirteen = new GenericComparableIntStructKey { Id = 113 };
                var oneFifteeen = 115;
                var oneSixteen = new GenericComparableIntStructKey { Id = 116 };

                dependents = new[]
                {
                    context.Set<GenericComparableIntStructKeyRequiredDependent>().FirstOrDefault(e => e.Id.Equals(new GenericComparableIntStructKey { Id = 111 })),
                    context.Set<GenericComparableIntStructKeyRequiredDependent>().FirstOrDefault(e => e.Id.Equals(new GenericComparableIntStructKey { Id = oneTwelve })),
                    context.Set<GenericComparableIntStructKeyRequiredDependent>().FirstOrDefault(e => e.Id.Equals(oneThirteen)),
                    context.Set<GenericComparableIntStructKeyRequiredDependent>().FirstOrDefault(e => e.Id == new GenericComparableIntStructKey { Id = 114 }),
                    context.Set<GenericComparableIntStructKeyRequiredDependent>().FirstOrDefault(e => e.Id == new GenericComparableIntStructKey { Id = oneFifteeen }),
                    context.Set<GenericComparableIntStructKeyRequiredDependent>().FirstOrDefault(e => e.Id == oneSixteen)
                };

                Assert.Same(dependents[0], context.Set<GenericComparableIntStructKeyRequiredDependent>().Find(new GenericComparableIntStructKey { Id = 111 }));
                Assert.Same(dependents[1], context.Set<GenericComparableIntStructKeyRequiredDependent>().Find(new GenericComparableIntStructKey { Id = oneTwelve }));
                Assert.Same(dependents[2], context.Set<GenericComparableIntStructKeyRequiredDependent>().Find(oneThirteen));
                Assert.Same(dependents[3], context.Find(typeof(GenericComparableIntStructKeyRequiredDependent), new GenericComparableIntStructKey { Id = 114 }));
                Assert.Same(dependents[4], context.Find(typeof(GenericComparableIntStructKeyRequiredDependent), new GenericComparableIntStructKey { Id = oneFifteeen }));
                Assert.Same(dependents[5], context.Find(typeof(GenericComparableIntStructKeyRequiredDependent), oneSixteen));
           }

            void Validate(
                GenericComparableIntStructKeyPrincipal[] principals,
                GenericComparableIntStructKeyRequiredDependent[] dependents,
                (int, int[])[] expectedPrincipalToDependents,
                (int, int)[] expectedDependentToPrincipals)
            {
                ValidateRequired(
                    principals,
                    dependents,
                    expectedPrincipalToDependents,
                    expectedDependentToPrincipals,
                    p => ((GenericComparableIntStructKeyPrincipal)p).RequiredDependents.Select(d => (IIntRequiredDependent)d).ToList(),
                    d => ((GenericComparableIntStructKeyRequiredDependent)d).Principal);
            }
        }

        [ConditionalFact]
        public virtual void Can_insert_and_read_back_with_class_key_and_optional_dependents()
        {
            InsertOptionalGraph<IntClassKeyPrincipal, IntClassKeyOptionalDependent>();

            using (var context = CreateContext())
            {
                RunQueries(context, out var principals, out var dependents);

                Validate(
                    principals,
                    dependents,
                    new[] { (0, new[] { 0 }), (1, new[] { 1 }), (2, new[] { 2, 2, 2 }), (3, new int[0]) },
                    new (int, int?)[] { (0, 0), (1, 1), (2, 2), (3, 2), (4, 2), (5, null) });

                foreach (var principal in principals)
                {
                    principal.Foo = "Mutant!";
                }

                dependents[5].Principal = principals[0];
                dependents[4].PrincipalId = null;
                dependents[3].PrincipalId = principals[0].Id;
                principals[1].OptionalDependents.Clear();

                context.SaveChanges();
            }

            using (var context = CreateContext())
            {
                RunQueries(context, out var principals, out var dependents);

                Validate(
                    principals,
                    dependents,
                    new[] { (0, new[] { 0, 3, 5 }), (1, new int[0]), (2, new[] { 2 }), (3, new int[0]) },
                    new (int, int?)[] { (0, 0), (1, null), (2, 2), (3, 0), (4, null), (5, 0) });
            }

            void RunQueries(
                DbContext context,
                out IntClassKeyPrincipal[] principals,
                out IntClassKeyOptionalDependent[] dependents)
            {
                var two = 2;
                var three = new IntClassKey { Id = 3 };

                principals = new[]
                {
                    context.Set<IntClassKeyPrincipal>().Include(e => e.OptionalDependents)
                        .Single(e => e.Id.Equals(new IntClassKey { Id = 1 })),
                    context.Set<IntClassKeyPrincipal>().Include(e => e.OptionalDependents)
                        .Single(e => e.Id.Equals(new IntClassKey { Id = two })),
                    context.Set<IntClassKeyPrincipal>().Include(e => e.OptionalDependents).Single(e => e.Id.Equals(three)),
                    context.Set<IntClassKeyPrincipal>().Include(e => e.OptionalDependents)
                        .Single(e => e.Id.Equals(new IntClassKey { Id = 4 }))
                };

                var oneOhTwo = 102;
                var oneOhThree = new IntClassKey { Id = 103 };
                var oneOhFive = 105;
                var oneOhSix = new IntClassKey { Id = 106 };

                dependents = new[]
                {
                    context.Set<IntClassKeyOptionalDependent>().Single(e => e.Id.Equals(new IntClassKey { Id = 101 })),
                    context.Set<IntClassKeyOptionalDependent>().Single(e => e.Id.Equals(new IntClassKey { Id = oneOhTwo })),
                    context.Set<IntClassKeyOptionalDependent>().Single(e => e.Id.Equals(oneOhThree)),
                    context.Set<IntClassKeyOptionalDependent>().Single(e => e.Id == new IntClassKey { Id = 104 }),
                    context.Set<IntClassKeyOptionalDependent>().Single(e => e.Id == new IntClassKey { Id = oneOhFive }),
                    context.Set<IntClassKeyOptionalDependent>().Single(e => e.Id == oneOhSix)
                };

                Assert.Same(dependents[0], context.Set<IntClassKeyOptionalDependent>().Find(new IntClassKey { Id = 101 }));
                Assert.Same(dependents[1], context.Set<IntClassKeyOptionalDependent>().Find(new IntClassKey { Id = oneOhTwo }));
                Assert.Same(dependents[2], context.Set<IntClassKeyOptionalDependent>().Find(oneOhThree));
                Assert.Same(dependents[3], context.Find<IntClassKeyOptionalDependent>(new IntClassKey { Id = 104 }));
                Assert.Same(dependents[4], context.Find<IntClassKeyOptionalDependent>(new IntClassKey { Id = oneOhFive }));
                Assert.Same(dependents[5], context.Find<IntClassKeyOptionalDependent>(oneOhSix));
            }

            void Validate(
                IntClassKeyPrincipal[] principals,
                IntClassKeyOptionalDependent[] dependents,
                (int, int[])[] expectedPrincipalToDependents,
                (int, int?)[] expectedDependentToPrincipals)
            {
                ValidateOptional(
                    principals,
                    dependents,
                    expectedPrincipalToDependents,
                    expectedDependentToPrincipals,
                    p => ((IntClassKeyPrincipal)p).OptionalDependents.Select(d => (IIntOptionalDependent)d).ToList(),
                    d => ((IntClassKeyOptionalDependent)d).Principal);
            }
        }

        [ConditionalFact]
        public virtual void Can_insert_and_read_back_with_comparable_class_key_and_optional_dependents()
        {
            InsertOptionalGraph<ComparableIntClassKeyPrincipal, ComparableIntClassKeyOptionalDependent>();

            using (var context = CreateContext())
            {
                RunQueries(context, out var principals, out var dependents);

                Validate(
                    principals,
                    dependents,
                    new[] { (0, new[] { 0 }), (1, new[] { 1 }), (2, new[] { 2, 2, 2 }), (3, new int[0]) },
                    new (int, int?)[] { (0, 0), (1, 1), (2, 2), (3, 2), (4, 2), (5, null) });

                foreach (var principal in principals)
                {
                    principal.Foo = "Mutant!";
                }

                dependents[5].Principal = principals[0];
                dependents[4].PrincipalId = null;
                dependents[3].PrincipalId = principals[0].Id;
                principals[1].OptionalDependents.Clear();

                context.SaveChanges();
            }

            using (var context = CreateContext())
            {
                RunQueries(context, out var principals, out var dependents);

                Validate(
                    principals,
                    dependents,
                    new[] { (0, new[] { 0, 3, 5 }), (1, new int[0]), (2, new[] { 2 }), (3, new int[0]) },
                    new (int, int?)[] { (0, 0), (1, null), (2, 2), (3, 0), (4, null), (5, 0) });
            }

            void RunQueries(
                DbContext context,
                out ComparableIntClassKeyPrincipal[] principals,
                out ComparableIntClassKeyOptionalDependent[] dependents)
            {
                var two = 2;
                var three = new ComparableIntClassKey { Id = 3 };

                principals = new[]
                {
                    context.Set<ComparableIntClassKeyPrincipal>().Include(e => e.OptionalDependents)
                        .Single(e => e.Id.Equals(new ComparableIntClassKey { Id = 1 })),
                    context.Set<ComparableIntClassKeyPrincipal>().Include(e => e.OptionalDependents)
                        .Single(e => e.Id.Equals(new ComparableIntClassKey { Id = two })),
                    context.Set<ComparableIntClassKeyPrincipal>().Include(e => e.OptionalDependents).Single(e => e.Id.Equals(three)),
                    context.Set<ComparableIntClassKeyPrincipal>().Include(e => e.OptionalDependents)
                        .Single(e => e.Id.Equals(new ComparableIntClassKey { Id = 4 }))
                };

                var oneOhTwo = 102;
                var oneOhThree = new ComparableIntClassKey { Id = 103 };
                var oneOhFive = 105;
                var oneOhSix = new ComparableIntClassKey { Id = 106 };

                dependents = new[]
                {
                    context.Set<ComparableIntClassKeyOptionalDependent>().Single(e => e.Id.Equals(new ComparableIntClassKey { Id = 101 })),
                    context.Set<ComparableIntClassKeyOptionalDependent>().Single(e => e.Id.Equals(new ComparableIntClassKey { Id = oneOhTwo })),
                    context.Set<ComparableIntClassKeyOptionalDependent>().Single(e => e.Id.Equals(oneOhThree)),
                    context.Set<ComparableIntClassKeyOptionalDependent>().Single(e => e.Id == new ComparableIntClassKey { Id = 104 }),
                    context.Set<ComparableIntClassKeyOptionalDependent>().Single(e => e.Id == new ComparableIntClassKey { Id = oneOhFive }),
                    context.Set<ComparableIntClassKeyOptionalDependent>().Single(e => e.Id == oneOhSix)
                };

                Assert.Same(dependents[0], context.Set<ComparableIntClassKeyOptionalDependent>().Find(new ComparableIntClassKey { Id = 101 }));
                Assert.Same(dependents[1], context.Set<ComparableIntClassKeyOptionalDependent>().Find(new ComparableIntClassKey { Id = oneOhTwo }));
                Assert.Same(dependents[2], context.Set<ComparableIntClassKeyOptionalDependent>().Find(oneOhThree));
                Assert.Same(dependents[3], context.Find<ComparableIntClassKeyOptionalDependent>(new ComparableIntClassKey { Id = 104 }));
                Assert.Same(dependents[4], context.Find<ComparableIntClassKeyOptionalDependent>(new ComparableIntClassKey { Id = oneOhFive }));
                Assert.Same(dependents[5], context.Find<ComparableIntClassKeyOptionalDependent>(oneOhSix));
            }

            void Validate(
                ComparableIntClassKeyPrincipal[] principals,
                ComparableIntClassKeyOptionalDependent[] dependents,
                (int, int[])[] expectedPrincipalToDependents,
                (int, int?)[] expectedDependentToPrincipals)
            {
                ValidateOptional(
                    principals,
                    dependents,
                    expectedPrincipalToDependents,
                    expectedDependentToPrincipals,
                    p => ((ComparableIntClassKeyPrincipal)p).OptionalDependents.Select(d => (IIntOptionalDependent)d).ToList(),
                    d => ((ComparableIntClassKeyOptionalDependent)d).Principal);
            }
        }

        [ConditionalFact (Skip="Issue #19641")]
        public virtual void Can_insert_and_read_back_with_struct_binary_key_and_optional_dependents()
        {
            InsertOptionalBytesGraph<BytesStructKeyPrincipal, BytesStructKeyOptionalDependent>();

            using (var context = CreateContext())
            {
                RunQueries(context, out var principals, out var dependents);

                Validate(
                    principals,
                    dependents,
                    new[] { (0, new[] { 0 }), (1, new[] { 1 }), (2, new[] { 2, 2, 2 }), (3, new int[0]) },
                    new (int, int?)[] { (0, 0), (1, 1), (2, 2), (3, 2), (4, 2), (5, null) });

                foreach (var principal in principals)
                {
                    principal.Foo = "Mutant!";
                }

                dependents[5].Principal = principals[0];
                dependents[4].PrincipalId = null;
                dependents[3].PrincipalId = principals[0].Id;
                principals[1].OptionalDependents.Clear();

                context.SaveChanges();
            }

            using (var context = CreateContext())
            {
                RunQueries(context, out var principals, out var dependents);

                Validate(
                    principals,
                    dependents,
                    new[] { (0, new[] { 0, 3, 5 }), (1, new int[0]), (2, new[] { 2 }), (3, new int[0]) },
                    new (int, int?)[] { (0, 0), (1, null), (2, 2), (3, 0), (4, null), (5, 0) });
            }

            void RunQueries(
                DbContext context,
                out BytesStructKeyPrincipal[] principals,
                out BytesStructKeyOptionalDependent[] dependents)
            {
                var two = new byte[] { 2, 2 };
                var three = new BytesStructKey { Id = new byte[] { 3, 3, 3 } };

                principals = new[]
                {
                    context.Set<BytesStructKeyPrincipal>().Include(e => e.OptionalDependents).Single(e => e.Id.Equals(new BytesStructKey { Id = new byte[] { 1 } })),
                    context.Set<BytesStructKeyPrincipal>().Include(e => e.OptionalDependents).Single(e => e.Id.Equals(new BytesStructKey { Id = two })),
                    context.Set<BytesStructKeyPrincipal>().Include(e => e.OptionalDependents).Where(e => e.Id.Equals(three)).ToList().Single(),
                    context.Set<BytesStructKeyPrincipal>().Include(e => e.OptionalDependents).Single(e => e.Id.Equals(new BytesStructKey { Id = new byte[] { 4, 4, 4, 4 } }))
                };

                var oneOhTwo = new byte[] { 102 };
                var oneOhThree = new BytesStructKey { Id = new byte[] { 103 } };
                var oneOhFive = new byte[] { 105 };
                var oneOhSix = new BytesStructKey { Id = new byte[] { 106 } };

                dependents = new[]
                {
                    context.Set<BytesStructKeyOptionalDependent>().Single(e => e.Id.Equals(new BytesStructKey { Id = new byte[] { 101 } })),
                    context.Set<BytesStructKeyOptionalDependent>().Single(e => e.Id.Equals(new BytesStructKey { Id = oneOhTwo })),
                    context.Set<BytesStructKeyOptionalDependent>().Single(e => e.Id.Equals(oneOhThree)),
                    context.Set<BytesStructKeyOptionalDependent>().Single(e => e.Id == new BytesStructKey { Id = new byte[] { 104 } }),
                    context.Set<BytesStructKeyOptionalDependent>().Single(e => e.Id == new BytesStructKey { Id = oneOhFive }),
                    context.Set<BytesStructKeyOptionalDependent>().Single(e => e.Id == oneOhSix)
                };

                Assert.Same(dependents[0], context.Set<BytesStructKeyOptionalDependent>().Find(new BytesStructKey { Id = new byte[] { 101 } }));
                Assert.Same(dependents[1], context.Set<BytesStructKeyOptionalDependent>().Find(new BytesStructKey { Id = oneOhTwo }));
                Assert.Same(dependents[2], context.Set<BytesStructKeyOptionalDependent>().Find(oneOhThree));
                Assert.Same(dependents[3], context.Find(typeof(BytesStructKeyOptionalDependent), new BytesStructKey { Id = new byte[] { 104 } }));
                Assert.Same(dependents[4], context.Find(typeof(BytesStructKeyOptionalDependent), new BytesStructKey { Id = oneOhFive }));
                Assert.Same(dependents[5], context.Find(typeof(BytesStructKeyOptionalDependent), oneOhSix));
           }

            void Validate(
                BytesStructKeyPrincipal[] principals,
                BytesStructKeyOptionalDependent[] dependents,
                (int, int[])[] expectedPrincipalToDependents,
                (int, int?)[] expectedDependentToPrincipals)
            {
                ValidateOptionalBytes(
                    principals,
                    dependents,
                    expectedPrincipalToDependents,
                    expectedDependentToPrincipals,
                    p => ((BytesStructKeyPrincipal)p).OptionalDependents.Select(d => (IBytesOptionalDependent)d).ToList(),
                    d => ((BytesStructKeyOptionalDependent)d).Principal);
            }
        }

        [ConditionalFact (Skip="Issue #19641")]
        public virtual void Can_insert_and_read_back_with_structural_struct_binary_key_and_optional_dependents()
        {
            InsertOptionalBytesGraph<StructuralComparableBytesStructKeyPrincipal, StructuralComparableBytesStructKeyOptionalDependent>();

            using (var context = CreateContext())
            {
                RunQueries(context, out var principals, out var dependents);

                Validate(
                    principals,
                    dependents,
                    new[] { (0, new[] { 0 }), (1, new[] { 1 }), (2, new[] { 2, 2, 2 }), (3, new int[0]) },
                    new (int, int?)[] { (0, 0), (1, 1), (2, 2), (3, 2), (4, 2), (5, null) });

                foreach (var principal in principals)
                {
                    principal.Foo = "Mutant!";
                }

                dependents[5].Principal = principals[0];
                dependents[4].PrincipalId = null;
                dependents[3].PrincipalId = principals[0].Id;
                principals[1].OptionalDependents.Clear();

                context.SaveChanges();
            }

            using (var context = CreateContext())
            {
                RunQueries(context, out var principals, out var dependents);

                Validate(
                    principals,
                    dependents,
                    new[] { (0, new[] { 0, 3, 5 }), (1, new int[0]), (2, new[] { 2 }), (3, new int[0]) },
                    new (int, int?)[] { (0, 0), (1, null), (2, 2), (3, 0), (4, null), (5, 0) });
            }

            void RunQueries(
                DbContext context,
                out StructuralComparableBytesStructKeyPrincipal[] principals,
                out StructuralComparableBytesStructKeyOptionalDependent[] dependents)
            {
                var two = new byte[] { 2, 2 };
                var three = new StructuralComparableBytesStructKey { Id = new byte[] { 3, 3, 3 } };

                principals = new[]
                {
                    context.Set<StructuralComparableBytesStructKeyPrincipal>().Include(e => e.OptionalDependents).Single(e => e.Id.Equals(new StructuralComparableBytesStructKey { Id = new byte[] { 1 } })),
                    context.Set<StructuralComparableBytesStructKeyPrincipal>().Include(e => e.OptionalDependents).Single(e => e.Id.Equals(new StructuralComparableBytesStructKey { Id = two })),
                    context.Set<StructuralComparableBytesStructKeyPrincipal>().Include(e => e.OptionalDependents).Single(e => e.Id.Equals(three)),
                    context.Set<StructuralComparableBytesStructKeyPrincipal>().Include(e => e.OptionalDependents).Single(e => e.Id.Equals(new StructuralComparableBytesStructKey { Id = new byte[] { 4, 4, 4, 4 } }))
                };

                var oneOhTwo = new byte[] { 102 };
                var oneOhThree = new StructuralComparableBytesStructKey { Id = new byte[] { 103 } };
                var oneOhFive = new byte[] { 105 };
                var oneOhSix = new StructuralComparableBytesStructKey { Id = new byte[] { 106 } };

                dependents = new[]
                {
                    context.Set<StructuralComparableBytesStructKeyOptionalDependent>().Single(e => e.Id.Equals(new StructuralComparableBytesStructKey { Id = new byte[] { 101 } })),
                    context.Set<StructuralComparableBytesStructKeyOptionalDependent>().Single(e => e.Id.Equals(new StructuralComparableBytesStructKey { Id = oneOhTwo })),
                    context.Set<StructuralComparableBytesStructKeyOptionalDependent>().Single(e => e.Id.Equals(oneOhThree)),
                    context.Set<StructuralComparableBytesStructKeyOptionalDependent>().Single(e => e.Id == new StructuralComparableBytesStructKey { Id = new byte[] { 104 } }),
                    context.Set<StructuralComparableBytesStructKeyOptionalDependent>().Single(e => e.Id == new StructuralComparableBytesStructKey { Id = oneOhFive }),
                    context.Set<StructuralComparableBytesStructKeyOptionalDependent>().Single(e => e.Id == oneOhSix)
                };

                Assert.Same(dependents[0], context.Set<StructuralComparableBytesStructKeyOptionalDependent>().Find(new StructuralComparableBytesStructKey { Id = new byte[] { 101 } }));
                Assert.Same(dependents[1], context.Set<StructuralComparableBytesStructKeyOptionalDependent>().Find(new StructuralComparableBytesStructKey { Id = oneOhTwo }));
                Assert.Same(dependents[2], context.Set<StructuralComparableBytesStructKeyOptionalDependent>().Find(oneOhThree));
                Assert.Same(dependents[3], context.Find(typeof(StructuralComparableBytesStructKeyOptionalDependent), new StructuralComparableBytesStructKey { Id = new byte[] { 104 } }));
                Assert.Same(dependents[4], context.Find(typeof(StructuralComparableBytesStructKeyOptionalDependent), new StructuralComparableBytesStructKey { Id = oneOhFive }));
                Assert.Same(dependents[5], context.Find(typeof(StructuralComparableBytesStructKeyOptionalDependent), oneOhSix));
           }

            void Validate(
                StructuralComparableBytesStructKeyPrincipal[] principals,
                StructuralComparableBytesStructKeyOptionalDependent[] dependents,
                (int, int[])[] expectedPrincipalToDependents,
                (int, int?)[] expectedDependentToPrincipals)
            {
                ValidateOptionalBytes(
                    principals,
                    dependents,
                    expectedPrincipalToDependents,
                    expectedDependentToPrincipals,
                    p => ((StructuralComparableBytesStructKeyPrincipal)p).OptionalDependents.Select(d => (IBytesOptionalDependent)d).ToList(),
                    d => ((StructuralComparableBytesStructKeyOptionalDependent)d).Principal);
            }
        }

        [ConditionalFact (Skip="Issue #19641")]
        public virtual void Can_insert_and_read_back_with_comparable_struct_binary_key_and_optional_dependents()
        {
            InsertOptionalBytesGraph<ComparableBytesStructKeyPrincipal, ComparableBytesStructKeyOptionalDependent>();

            using (var context = CreateContext())
            {
                RunQueries(context, out var principals, out var dependents);

                Validate(
                    principals,
                    dependents,
                    new[] { (0, new[] { 0 }), (1, new[] { 1 }), (2, new[] { 2, 2, 2 }), (3, new int[0]) },
                    new (int, int?)[] { (0, 0), (1, 1), (2, 2), (3, 2), (4, 2), (5, null) });

                foreach (var principal in principals)
                {
                    principal.Foo = "Mutant!";
                }

                dependents[5].Principal = principals[0];
                dependents[4].PrincipalId = null;
                dependents[3].PrincipalId = principals[0].Id;
                principals[1].OptionalDependents.Clear();

                context.SaveChanges();
            }

            using (var context = CreateContext())
            {
                RunQueries(context, out var principals, out var dependents);

                Validate(
                    principals,
                    dependents,
                    new[] { (0, new[] { 0, 3, 5 }), (1, new int[0]), (2, new[] { 2 }), (3, new int[0]) },
                    new (int, int?)[] { (0, 0), (1, null), (2, 2), (3, 0), (4, null), (5, 0) });
            }

            void RunQueries(
                DbContext context,
                out ComparableBytesStructKeyPrincipal[] principals,
                out ComparableBytesStructKeyOptionalDependent[] dependents)
            {
                var two = new byte[] { 2, 2 };
                var three = new ComparableBytesStructKey { Id = new byte[] { 3, 3, 3 } };

                principals = new[]
                {
                    context.Set<ComparableBytesStructKeyPrincipal>().Include(e => e.OptionalDependents).Single(e => e.Id.Equals(new ComparableBytesStructKey { Id = new byte[] { 1 } })),
                    context.Set<ComparableBytesStructKeyPrincipal>().Include(e => e.OptionalDependents).Single(e => e.Id.Equals(new ComparableBytesStructKey { Id = two })),
                    context.Set<ComparableBytesStructKeyPrincipal>().Include(e => e.OptionalDependents).ToList().Where(e => e.Id.Equals(three)).ToList().Single(),
                    context.Set<ComparableBytesStructKeyPrincipal>().Include(e => e.OptionalDependents).Single(e => e.Id.Equals(new ComparableBytesStructKey { Id = new byte[] { 4, 4, 4, 4 } }))
                };

                var oneOhTwo = new byte[] { 102 };
                var oneOhThree = new ComparableBytesStructKey { Id = new byte[] { 103 } };
                var oneOhFive = new byte[] { 105 };
                var oneOhSix = new ComparableBytesStructKey { Id = new byte[] { 106 } };

                dependents = new[]
                {
                    context.Set<ComparableBytesStructKeyOptionalDependent>().Single(e => e.Id.Equals(new ComparableBytesStructKey { Id = new byte[] { 101 } })),
                    context.Set<ComparableBytesStructKeyOptionalDependent>().Single(e => e.Id.Equals(new ComparableBytesStructKey { Id = oneOhTwo })),
                    context.Set<ComparableBytesStructKeyOptionalDependent>().Single(e => e.Id.Equals(oneOhThree)),
                    context.Set<ComparableBytesStructKeyOptionalDependent>().Single(e => e.Id == new ComparableBytesStructKey { Id = new byte[] { 104 } }),
                    context.Set<ComparableBytesStructKeyOptionalDependent>().Single(e => e.Id == new ComparableBytesStructKey { Id = oneOhFive }),
                    context.Set<ComparableBytesStructKeyOptionalDependent>().Single(e => e.Id == oneOhSix)
                };

                Assert.Same(dependents[0], context.Set<ComparableBytesStructKeyOptionalDependent>().Find(new ComparableBytesStructKey { Id = new byte[] { 101 } }));
                Assert.Same(dependents[1], context.Set<ComparableBytesStructKeyOptionalDependent>().Find(new ComparableBytesStructKey { Id = oneOhTwo }));
                Assert.Same(dependents[2], context.Set<ComparableBytesStructKeyOptionalDependent>().Find(oneOhThree));
                Assert.Same(dependents[3], context.Find(typeof(ComparableBytesStructKeyOptionalDependent), new ComparableBytesStructKey { Id = new byte[] { 104 } }));
                Assert.Same(dependents[4], context.Find(typeof(ComparableBytesStructKeyOptionalDependent), new ComparableBytesStructKey { Id = oneOhFive }));
                Assert.Same(dependents[5], context.Find(typeof(ComparableBytesStructKeyOptionalDependent), oneOhSix));
           }

            void Validate(
                ComparableBytesStructKeyPrincipal[] principals,
                ComparableBytesStructKeyOptionalDependent[] dependents,
                (int, int[])[] expectedPrincipalToDependents,
                (int, int?)[] expectedDependentToPrincipals)
            {
                ValidateOptionalBytes(
                    principals,
                    dependents,
                    expectedPrincipalToDependents,
                    expectedDependentToPrincipals,
                    p => ((ComparableBytesStructKeyPrincipal)p).OptionalDependents.Select(d => (IBytesOptionalDependent)d).ToList(),
                    d => ((ComparableBytesStructKeyOptionalDependent)d).Principal);
            }
        }

        [ConditionalFact (Skip="Issue #19641")]
        public virtual void Can_insert_and_read_back_with_generic_comparable_struct_binary_key_and_optional_dependents()
        {
            InsertOptionalBytesGraph<GenericComparableBytesStructKeyPrincipal, GenericComparableBytesStructKeyOptionalDependent>();

            using (var context = CreateContext())
            {
                RunQueries(context, out var principals, out var dependents);

                Validate(
                    principals,
                    dependents,
                    new[] { (0, new[] { 0 }), (1, new[] { 1 }), (2, new[] { 2, 2, 2 }), (3, new int[0]) },
                    new (int, int?)[] { (0, 0), (1, 1), (2, 2), (3, 2), (4, 2), (5, null) });

                foreach (var principal in principals)
                {
                    principal.Foo = "Mutant!";
                }

                dependents[5].Principal = principals[0];
                dependents[4].PrincipalId = null;
                dependents[3].PrincipalId = principals[0].Id;
                principals[1].OptionalDependents.Clear();

                context.SaveChanges();
            }

            using (var context = CreateContext())
            {
                RunQueries(context, out var principals, out var dependents);

                Validate(
                    principals,
                    dependents,
                    new[] { (0, new[] { 0, 3, 5 }), (1, new int[0]), (2, new[] { 2 }), (3, new int[0]) },
                    new (int, int?)[] { (0, 0), (1, null), (2, 2), (3, 0), (4, null), (5, 0) });
            }

            void RunQueries(
                DbContext context,
                out GenericComparableBytesStructKeyPrincipal[] principals,
                out GenericComparableBytesStructKeyOptionalDependent[] dependents)
            {
                var two = new byte[] { 2, 2 };
                var three = new GenericComparableBytesStructKey { Id = new byte[] { 3, 3, 3 } };

                principals = new[]
                {
                    context.Set<GenericComparableBytesStructKeyPrincipal>().Include(e => e.OptionalDependents).Single(e => e.Id.Equals(new GenericComparableBytesStructKey { Id = new byte[] { 1 } })),
                    context.Set<GenericComparableBytesStructKeyPrincipal>().Include(e => e.OptionalDependents).Single(e => e.Id.Equals(new GenericComparableBytesStructKey { Id = two })),
                    context.Set<GenericComparableBytesStructKeyPrincipal>().Include(e => e.OptionalDependents).Single(e => e.Id.Equals(three)),
                    context.Set<GenericComparableBytesStructKeyPrincipal>().Include(e => e.OptionalDependents).Single(e => e.Id.Equals(new GenericComparableBytesStructKey { Id = new byte[] { 4, 4, 4, 4 } }))
                };

                var oneOhTwo = new byte[] { 102 };
                var oneOhThree = new GenericComparableBytesStructKey { Id = new byte[] { 103 } };
                var oneOhFive = new byte[] { 105 };
                var oneOhSix = new GenericComparableBytesStructKey { Id = new byte[] { 106 } };

                dependents = new[]
                {
                    context.Set<GenericComparableBytesStructKeyOptionalDependent>().Single(e => e.Id.Equals(new GenericComparableBytesStructKey { Id = new byte[] { 101 } })),
                    context.Set<GenericComparableBytesStructKeyOptionalDependent>().Single(e => e.Id.Equals(new GenericComparableBytesStructKey { Id = oneOhTwo })),
                    context.Set<GenericComparableBytesStructKeyOptionalDependent>().Single(e => e.Id.Equals(oneOhThree)),
                    context.Set<GenericComparableBytesStructKeyOptionalDependent>().Single(e => e.Id == new GenericComparableBytesStructKey { Id = new byte[] { 104 } }),
                    context.Set<GenericComparableBytesStructKeyOptionalDependent>().Single(e => e.Id == new GenericComparableBytesStructKey { Id = oneOhFive }),
                    context.Set<GenericComparableBytesStructKeyOptionalDependent>().Single(e => e.Id == oneOhSix)
                };

                Assert.Same(dependents[0], context.Set<GenericComparableBytesStructKeyOptionalDependent>().Find(new GenericComparableBytesStructKey { Id = new byte[] { 101 } }));
                Assert.Same(dependents[1], context.Set<GenericComparableBytesStructKeyOptionalDependent>().Find(new GenericComparableBytesStructKey { Id = oneOhTwo }));
                Assert.Same(dependents[2], context.Set<GenericComparableBytesStructKeyOptionalDependent>().Find(oneOhThree));
                Assert.Same(dependents[3], context.Find(typeof(GenericComparableBytesStructKeyOptionalDependent), new GenericComparableBytesStructKey { Id = new byte[] { 104 } }));
                Assert.Same(dependents[4], context.Find(typeof(GenericComparableBytesStructKeyOptionalDependent), new GenericComparableBytesStructKey { Id = oneOhFive }));
                Assert.Same(dependents[5], context.Find(typeof(GenericComparableBytesStructKeyOptionalDependent), oneOhSix));
           }

            void Validate(
                GenericComparableBytesStructKeyPrincipal[] principals,
                GenericComparableBytesStructKeyOptionalDependent[] dependents,
                (int, int[])[] expectedPrincipalToDependents,
                (int, int?)[] expectedDependentToPrincipals)
            {
                ValidateOptionalBytes(
                    principals,
                    dependents,
                    expectedPrincipalToDependents,
                    expectedDependentToPrincipals,
                    p => ((GenericComparableBytesStructKeyPrincipal)p).OptionalDependents.Select(d => (IBytesOptionalDependent)d).ToList(),
                    d => ((GenericComparableBytesStructKeyOptionalDependent)d).Principal);
            }
        }

        [ConditionalFact (Skip="Issue #19641")]
        public virtual void Can_insert_and_read_back_with_struct_binary_key_and_required_dependents()
        {
            InsertRequiredBytesGraph<BytesStructKeyPrincipal, BytesStructKeyRequiredDependent>();

            using (var context = CreateContext())
            {
                RunQueries(context, out var principals, out var dependents);

                Validate(
                    principals,
                    dependents,
                    new[] { (0, new[] { 0 }), (1, new[] { 1 }), (2, new[] { 2, 2, 2, 2 }), (3, new int[0]) },
                    new[] { (0, 0), (1, 1), (2, 2), (3, 2), (4, 2), (5, 2) });

                foreach (var principal in principals)
                {
                    principal.Foo = "Mutant!";
                }

                dependents[5].Principal = principals[0];
                context.Remove(dependents[4]);
                dependents[3].PrincipalId = principals[0].Id;
                principals[1].RequiredDependents.Clear();

                context.SaveChanges();
            }

            using (var context = CreateContext())
            {
                RunQueries(context, out var principals, out var dependents);

                Validate(
                    principals,
                    dependents,
                    new[] { (0, new[] { 0, 3, 5 }), (1, new int[0]), (2, new[] { 2 }), (3, new int[0]) },
                    new[] { (0, 0), (2, 2), (3, 0), (5, 0) });
            }

            void RunQueries(
                DbContext context,
                out BytesStructKeyPrincipal[] principals,
                out BytesStructKeyRequiredDependent[] dependents)
            {
                var twelve = new byte[] { 12, 12 };
                var thirteen = new BytesStructKey { Id = new byte[] { 13, 13, 13 } };

                principals = new[]
                {
                    context.Set<BytesStructKeyPrincipal>().Include(e => e.RequiredDependents).Single(e => e.Id.Equals(new BytesStructKey { Id = new byte[] { 11 } })),
                    context.Set<BytesStructKeyPrincipal>().Include(e => e.RequiredDependents).Single(e => e.Id.Equals(new BytesStructKey { Id = twelve })),
                    context.Set<BytesStructKeyPrincipal>().Include(e => e.RequiredDependents).Single(e => e.Id.Equals(thirteen)),
                    context.Set<BytesStructKeyPrincipal>().Include(e => e.RequiredDependents).Single(e => e.Id.Equals(new BytesStructKey { Id = new byte[] { 14, 14, 14, 14 } }))
                };

                var oneTwelve = new byte[] { 112 };
                var oneThirteen = new BytesStructKey { Id = new byte[] { 113 } };
                var oneFifteeen = new byte[] { 115 };
                var oneSixteen = new BytesStructKey { Id = new byte[] { 116 } };

                dependents = new[]
                {
                    context.Set<BytesStructKeyRequiredDependent>().FirstOrDefault(e => e.Id.Equals(new BytesStructKey { Id = new byte[] { 111 } })),
                    context.Set<BytesStructKeyRequiredDependent>().FirstOrDefault(e => e.Id.Equals(new BytesStructKey { Id = oneTwelve })),
                    context.Set<BytesStructKeyRequiredDependent>().FirstOrDefault(e => e.Id.Equals(oneThirteen)),
                    context.Set<BytesStructKeyRequiredDependent>().FirstOrDefault(e => e.Id == new BytesStructKey { Id = new byte[] { 114 } }),
                    context.Set<BytesStructKeyRequiredDependent>().FirstOrDefault(e => e.Id == new BytesStructKey { Id = oneFifteeen }),
                    context.Set<BytesStructKeyRequiredDependent>().FirstOrDefault(e => e.Id == oneSixteen)
                };

                Assert.Same(dependents[0], context.Set<BytesStructKeyRequiredDependent>().Find(new BytesStructKey { Id = new byte[] { 111 } }));
                Assert.Same(dependents[1], context.Set<BytesStructKeyRequiredDependent>().Find(new BytesStructKey { Id = oneTwelve }));
                Assert.Same(dependents[2], context.Set<BytesStructKeyRequiredDependent>().Find(oneThirteen));
                Assert.Same(dependents[3], context.Find(typeof(BytesStructKeyRequiredDependent), new BytesStructKey { Id = new byte[] { 114 } }));
                Assert.Same(dependents[4], context.Find(typeof(BytesStructKeyRequiredDependent), new BytesStructKey { Id = oneFifteeen }));
                Assert.Same(dependents[5], context.Find(typeof(BytesStructKeyRequiredDependent), oneSixteen));
           }

            void Validate(
                BytesStructKeyPrincipal[] principals,
                BytesStructKeyRequiredDependent[] dependents,
                (int, int[])[] expectedPrincipalToDependents,
                (int, int)[] expectedDependentToPrincipals)
            {
                ValidateRequiredBytes(
                    principals,
                    dependents,
                    expectedPrincipalToDependents,
                    expectedDependentToPrincipals,
                    p => ((BytesStructKeyPrincipal)p).RequiredDependents.Select(d => (IBytesRequiredDependent)d).ToList(),
                    d => ((BytesStructKeyRequiredDependent)d).Principal);
            }
        }

        [ConditionalFact (Skip="Issue #19641")]
        public virtual void Can_insert_and_read_back_with_comparable_struct_binary_key_and_required_dependents()
        {
            InsertRequiredBytesGraph<ComparableBytesStructKeyPrincipal, ComparableBytesStructKeyRequiredDependent>();

            using (var context = CreateContext())
            {
                RunQueries(context, out var principals, out var dependents);

                Validate(
                    principals,
                    dependents,
                    new[] { (0, new[] { 0 }), (1, new[] { 1 }), (2, new[] { 2, 2, 2, 2 }), (3, new int[0]) },
                    new[] { (0, 0), (1, 1), (2, 2), (3, 2), (4, 2), (5, 2) });

                foreach (var principal in principals)
                {
                    principal.Foo = "Mutant!";
                }

                dependents[5].Principal = principals[0];
                context.Remove(dependents[4]);
                dependents[3].PrincipalId = principals[0].Id;
                principals[1].RequiredDependents.Clear();

                context.SaveChanges();
            }

            using (var context = CreateContext())
            {
                RunQueries(context, out var principals, out var dependents);

                Validate(
                    principals,
                    dependents,
                    new[] { (0, new[] { 0, 3, 5 }), (1, new int[0]), (2, new[] { 2 }), (3, new int[0]) },
                    new[] { (0, 0), (2, 2), (3, 0), (5, 0) });
            }

            void RunQueries(
                DbContext context,
                out ComparableBytesStructKeyPrincipal[] principals,
                out ComparableBytesStructKeyRequiredDependent[] dependents)
            {
                var twelve = new byte[] { 12, 12 };
                var thirteen = new ComparableBytesStructKey { Id = new byte[] { 13, 13, 13 } };

                principals = new[]
                {
                    context.Set<ComparableBytesStructKeyPrincipal>().Include(e => e.RequiredDependents).Single(e => e.Id.Equals(new ComparableBytesStructKey { Id = new byte[] { 11, 11 } })),
                    context.Set<ComparableBytesStructKeyPrincipal>().Include(e => e.RequiredDependents).Single(e => e.Id.Equals(new ComparableBytesStructKey { Id = twelve })),
                    context.Set<ComparableBytesStructKeyPrincipal>().Include(e => e.RequiredDependents).Single(e => e.Id.Equals(thirteen)),
                    context.Set<ComparableBytesStructKeyPrincipal>().Include(e => e.RequiredDependents).Single(e => e.Id.Equals(new ComparableBytesStructKey { Id = new byte[] { 14, 14, 14, 14 } }))
                };

                var oneTwelve = new byte[] { 112 };
                var oneThirteen = new ComparableBytesStructKey { Id = new byte[] { 113 } };
                var oneFifteeen = new byte[] { 115 };
                var oneSixteen = new ComparableBytesStructKey { Id = new byte[] { 116 } };

                dependents = new[]
                {
                    context.Set<ComparableBytesStructKeyRequiredDependent>().FirstOrDefault(e => e.Id.Equals(new ComparableBytesStructKey { Id = new byte[] { 111 } })),
                    context.Set<ComparableBytesStructKeyRequiredDependent>().FirstOrDefault(e => e.Id.Equals(new ComparableBytesStructKey { Id = oneTwelve })),
                    context.Set<ComparableBytesStructKeyRequiredDependent>().FirstOrDefault(e => e.Id.Equals(oneThirteen)),
                    context.Set<ComparableBytesStructKeyRequiredDependent>().FirstOrDefault(e => e.Id == new ComparableBytesStructKey { Id = new byte[] { 114 } }),
                    context.Set<ComparableBytesStructKeyRequiredDependent>().FirstOrDefault(e => e.Id == new ComparableBytesStructKey { Id = oneFifteeen }),
                    context.Set<ComparableBytesStructKeyRequiredDependent>().FirstOrDefault(e => e.Id == oneSixteen)
                };

                Assert.Same(dependents[0], context.Set<ComparableBytesStructKeyRequiredDependent>().Find(new ComparableBytesStructKey { Id = new byte[] { 111 } }));
                Assert.Same(dependents[1], context.Set<ComparableBytesStructKeyRequiredDependent>().Find(new ComparableBytesStructKey { Id = oneTwelve }));
                Assert.Same(dependents[2], context.Set<ComparableBytesStructKeyRequiredDependent>().Find(oneThirteen));
                Assert.Same(dependents[3], context.Find(typeof(ComparableBytesStructKeyRequiredDependent), new ComparableBytesStructKey { Id = new byte[] { 114 } }));
                Assert.Same(dependents[4], context.Find(typeof(ComparableBytesStructKeyRequiredDependent), new ComparableBytesStructKey { Id = oneFifteeen }));
                Assert.Same(dependents[5], context.Find(typeof(ComparableBytesStructKeyRequiredDependent), oneSixteen));
           }

            void Validate(
                ComparableBytesStructKeyPrincipal[] principals,
                ComparableBytesStructKeyRequiredDependent[] dependents,
                (int, int[])[] expectedPrincipalToDependents,
                (int, int)[] expectedDependentToPrincipals)
            {
                ValidateRequiredBytes(
                    principals,
                    dependents,
                    expectedPrincipalToDependents,
                    expectedDependentToPrincipals,
                    p => ((ComparableBytesStructKeyPrincipal)p).RequiredDependents.Select(d => (IBytesRequiredDependent)d).ToList(),
                    d => ((ComparableBytesStructKeyRequiredDependent)d).Principal);
            }
        }

        [ConditionalFact (Skip="Issue #19641")]
        public virtual void Can_insert_and_read_back_with_structural_struct_binary_key_and_required_dependents()
        {
            InsertRequiredBytesGraph<StructuralComparableBytesStructKeyPrincipal, StructuralComparableBytesStructKeyRequiredDependent>();

            using (var context = CreateContext())
            {
                RunQueries(context, out var principals, out var dependents);

                Validate(
                    principals,
                    dependents,
                    new[] { (0, new[] { 0 }), (1, new[] { 1 }), (2, new[] { 2, 2, 2, 2 }), (3, new int[0]) },
                    new[] { (0, 0), (1, 1), (2, 2), (3, 2), (4, 2), (5, 2) });

                foreach (var principal in principals)
                {
                    principal.Foo = "Mutant!";
                }

                dependents[5].Principal = principals[0];
                context.Remove(dependents[4]);
                dependents[3].PrincipalId = principals[0].Id;
                principals[1].RequiredDependents.Clear();

                context.SaveChanges();
            }

            using (var context = CreateContext())
            {
                RunQueries(context, out var principals, out var dependents);

                Validate(
                    principals,
                    dependents,
                    new[] { (0, new[] { 0, 3, 5 }), (1, new int[0]), (2, new[] { 2 }), (3, new int[0]) },
                    new[] { (0, 0), (2, 2), (3, 0), (5, 0) });
            }

            void RunQueries(
                DbContext context,
                out StructuralComparableBytesStructKeyPrincipal[] principals,
                out StructuralComparableBytesStructKeyRequiredDependent[] dependents)
            {
                var twelve = new byte[] { 12, 12 };
                var thirteen = new StructuralComparableBytesStructKey { Id = new byte[] { 13, 13, 13 } };

                principals = new[]
                {
                    context.Set<StructuralComparableBytesStructKeyPrincipal>().Include(e => e.RequiredDependents).Single(e => e.Id.Equals(new StructuralComparableBytesStructKey { Id = new byte[] { 11, 11 } })),
                    context.Set<StructuralComparableBytesStructKeyPrincipal>().Include(e => e.RequiredDependents).Single(e => e.Id.Equals(new StructuralComparableBytesStructKey { Id = twelve })),
                    context.Set<StructuralComparableBytesStructKeyPrincipal>().Include(e => e.RequiredDependents).Single(e => e.Id.Equals(thirteen)),
                    context.Set<StructuralComparableBytesStructKeyPrincipal>().Include(e => e.RequiredDependents).Single(e => e.Id.Equals(new StructuralComparableBytesStructKey { Id = new byte[] { 14, 14, 14, 14 } }))
                };

                var oneTwelve = new byte[] { 112 };
                var oneThirteen = new StructuralComparableBytesStructKey { Id = new byte[] { 113 } };
                var oneFifteeen = new byte[] { 115 };
                var oneSixteen = new StructuralComparableBytesStructKey { Id = new byte[] { 116 } };

                dependents = new[]
                {
                    context.Set<StructuralComparableBytesStructKeyRequiredDependent>().FirstOrDefault(e => e.Id.Equals(new StructuralComparableBytesStructKey { Id = new byte[] { 111 } })),
                    context.Set<StructuralComparableBytesStructKeyRequiredDependent>().FirstOrDefault(e => e.Id.Equals(new StructuralComparableBytesStructKey { Id = oneTwelve })),
                    context.Set<StructuralComparableBytesStructKeyRequiredDependent>().FirstOrDefault(e => e.Id.Equals(oneThirteen)),
                    context.Set<StructuralComparableBytesStructKeyRequiredDependent>().FirstOrDefault(e => e.Id == new StructuralComparableBytesStructKey { Id = new byte[] { 114 } }),
                    context.Set<StructuralComparableBytesStructKeyRequiredDependent>().FirstOrDefault(e => e.Id == new StructuralComparableBytesStructKey { Id = oneFifteeen }),
                    context.Set<StructuralComparableBytesStructKeyRequiredDependent>().FirstOrDefault(e => e.Id == oneSixteen)
                };

                Assert.Same(dependents[0], context.Set<StructuralComparableBytesStructKeyRequiredDependent>().Find(new StructuralComparableBytesStructKey { Id = new byte[] { 111 } }));
                Assert.Same(dependents[1], context.Set<StructuralComparableBytesStructKeyRequiredDependent>().Find(new StructuralComparableBytesStructKey { Id = oneTwelve }));
                Assert.Same(dependents[2], context.Set<StructuralComparableBytesStructKeyRequiredDependent>().Find(oneThirteen));
                Assert.Same(dependents[3], context.Find(typeof(StructuralComparableBytesStructKeyRequiredDependent), new StructuralComparableBytesStructKey { Id = new byte[] { 114 } }));
                Assert.Same(dependents[4], context.Find(typeof(StructuralComparableBytesStructKeyRequiredDependent), new StructuralComparableBytesStructKey { Id = oneFifteeen }));
                Assert.Same(dependents[5], context.Find(typeof(StructuralComparableBytesStructKeyRequiredDependent), oneSixteen));
           }

            void Validate(
                StructuralComparableBytesStructKeyPrincipal[] principals,
                StructuralComparableBytesStructKeyRequiredDependent[] dependents,
                (int, int[])[] expectedPrincipalToDependents,
                (int, int)[] expectedDependentToPrincipals)
            {
                ValidateRequiredBytes(
                    principals,
                    dependents,
                    expectedPrincipalToDependents,
                    expectedDependentToPrincipals,
                    p => ((StructuralComparableBytesStructKeyPrincipal)p).RequiredDependents.Select(d => (IBytesRequiredDependent)d).ToList(),
                    d => ((StructuralComparableBytesStructKeyRequiredDependent)d).Principal);
            }
        }

        [ConditionalFact (Skip="Issue #19641")]
        public virtual void Can_insert_and_read_back_with_generic_comparable_struct_binary_key_and_required_dependents()
        {
            InsertRequiredBytesGraph<GenericComparableBytesStructKeyPrincipal, GenericComparableBytesStructKeyRequiredDependent>();

            using (var context = CreateContext())
            {
                RunQueries(context, out var principals, out var dependents);

                Validate(
                    principals,
                    dependents,
                    new[] { (0, new[] { 0 }), (1, new[] { 1 }), (2, new[] { 2, 2, 2, 2 }), (3, new int[0]) },
                    new[] { (0, 0), (1, 1), (2, 2), (3, 2), (4, 2), (5, 2) });

                foreach (var principal in principals)
                {
                    principal.Foo = "Mutant!";
                }

                dependents[5].Principal = principals[0];
                context.Remove(dependents[4]);
                dependents[3].PrincipalId = principals[0].Id;
                principals[1].RequiredDependents.Clear();

                context.SaveChanges();
            }

            using (var context = CreateContext())
            {
                RunQueries(context, out var principals, out var dependents);

                Validate(
                    principals,
                    dependents,
                    new[] { (0, new[] { 0, 3, 5 }), (1, new int[0]), (2, new[] { 2 }), (3, new int[0]) },
                    new[] { (0, 0), (2, 2), (3, 0), (5, 0) });
            }

            void RunQueries(
                DbContext context,
                out GenericComparableBytesStructKeyPrincipal[] principals,
                out GenericComparableBytesStructKeyRequiredDependent[] dependents)
            {
                var twelve = new byte[] { 12, 12 };
                var thirteen = new GenericComparableBytesStructKey { Id = new byte[] { 13, 13 } };

                principals = new[]
                {
                    context.Set<GenericComparableBytesStructKeyPrincipal>().Include(e => e.RequiredDependents).Single(e => e.Id.Equals(new GenericComparableBytesStructKey { Id = new byte[] { 11 } })),
                    context.Set<GenericComparableBytesStructKeyPrincipal>().Include(e => e.RequiredDependents).Single(e => e.Id.Equals(new GenericComparableBytesStructKey { Id = twelve })),
                    context.Set<GenericComparableBytesStructKeyPrincipal>().Include(e => e.RequiredDependents).Single(e => e.Id.Equals(thirteen)),
                    context.Set<GenericComparableBytesStructKeyPrincipal>().Include(e => e.RequiredDependents).Single(e => e.Id.Equals(new GenericComparableBytesStructKey { Id = new byte[] { 14, 14, 14, 14 } }))
                };

                var oneTwelve = new byte[] { 112 };
                var oneThirteen = new GenericComparableBytesStructKey { Id = new byte[] { 113 } };
                var oneFifteeen = new byte[] { 115 };
                var oneSixteen = new GenericComparableBytesStructKey { Id = new byte[] { 116 } };

                dependents = new[]
                {
                    context.Set<GenericComparableBytesStructKeyRequiredDependent>().FirstOrDefault(e => e.Id.Equals(new GenericComparableBytesStructKey { Id = new byte[] { 111 } })),
                    context.Set<GenericComparableBytesStructKeyRequiredDependent>().FirstOrDefault(e => e.Id.Equals(new GenericComparableBytesStructKey { Id = oneTwelve })),
                    context.Set<GenericComparableBytesStructKeyRequiredDependent>().FirstOrDefault(e => e.Id.Equals(oneThirteen)),
                    context.Set<GenericComparableBytesStructKeyRequiredDependent>().FirstOrDefault(e => e.Id == new GenericComparableBytesStructKey { Id = new byte[] { 114 } }),
                    context.Set<GenericComparableBytesStructKeyRequiredDependent>().FirstOrDefault(e => e.Id == new GenericComparableBytesStructKey { Id = oneFifteeen }),
                    context.Set<GenericComparableBytesStructKeyRequiredDependent>().FirstOrDefault(e => e.Id == oneSixteen)
                };

                Assert.Same(dependents[0], context.Set<GenericComparableBytesStructKeyRequiredDependent>().Find(new GenericComparableBytesStructKey { Id = new byte[] { 111 } }));
                Assert.Same(dependents[1], context.Set<GenericComparableBytesStructKeyRequiredDependent>().Find(new GenericComparableBytesStructKey { Id = oneTwelve }));
                Assert.Same(dependents[2], context.Set<GenericComparableBytesStructKeyRequiredDependent>().Find(oneThirteen));
                Assert.Same(dependents[3], context.Find(typeof(GenericComparableBytesStructKeyRequiredDependent), new GenericComparableBytesStructKey { Id = new byte[] { 114 } }));
                Assert.Same(dependents[4], context.Find(typeof(GenericComparableBytesStructKeyRequiredDependent), new GenericComparableBytesStructKey { Id = oneFifteeen }));
                Assert.Same(dependents[5], context.Find(typeof(GenericComparableBytesStructKeyRequiredDependent), oneSixteen));
           }

            void Validate(
                GenericComparableBytesStructKeyPrincipal[] principals,
                GenericComparableBytesStructKeyRequiredDependent[] dependents,
                (int, int[])[] expectedPrincipalToDependents,
                (int, int)[] expectedDependentToPrincipals)
            {
                ValidateRequiredBytes(
                    principals,
                    dependents,
                    expectedPrincipalToDependents,
                    expectedDependentToPrincipals,
                    p => ((GenericComparableBytesStructKeyPrincipal)p).RequiredDependents.Select(d => (IBytesRequiredDependent)d).ToList(),
                    d => ((GenericComparableBytesStructKeyRequiredDependent)d).Principal);
            }
        }

        private void InsertOptionalGraph<TPrincipal, TDependent>()
            where TPrincipal : class, IIntPrincipal, new()
            where TDependent : class, IIntOptionalDependent, new()
        {
            using (var context = CreateContext())
            {
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

                Assert.Equal(10, context.SaveChanges());
            }
        }

        private void InsertRequiredGraph<TPrincipal, TDependent>()
            where TPrincipal : class, IIntPrincipal, new()
            where TDependent : class, IIntRequiredDependent, new()
        {
            using (var context = CreateContext())
            {
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

                Assert.Equal(10, context.SaveChanges());
            }
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

        private void InsertOptionalBytesGraph<TPrincipal, TDependent>()
            where TPrincipal : class, IBytesPrincipal, new()
            where TDependent : class, IBytesOptionalDependent, new()
        {
            using (var context = CreateContext())
            {
                context.Set<TPrincipal>().AddRange(
                    new TPrincipal { BackingId = new byte[]{ 1 }, Foo = "X1" },
                    new TPrincipal { BackingId = new byte[]{ 2, 2 }, Foo = "X2" },
                    new TPrincipal { BackingId = new byte[]{ 3, 3, 3 }, Foo = "X3" },
                    new TPrincipal { BackingId = new byte[]{ 4, 4, 4, 4 }, Foo = "X4" });

                context.Set<TDependent>().AddRange(
                    new TDependent { BackingId = new byte[]{ 101 }, BackingPrincipalId = new byte[]{ 1 } },
                    new TDependent { BackingId = new byte[]{ 102 }, BackingPrincipalId = new byte[]{ 2, 2 } },
                    new TDependent { BackingId = new byte[]{ 103 }, BackingPrincipalId = new byte[]{ 3, 3, 3 } },
                    new TDependent { BackingId = new byte[]{ 104 }, BackingPrincipalId = new byte[]{ 3, 3, 3 } },
                    new TDependent { BackingId = new byte[]{ 105 }, BackingPrincipalId = new byte[]{ 3, 3, 3 } },
                    new TDependent { BackingId = new byte[]{ 106 } });

                Assert.Equal(10, context.SaveChanges());
            }
        }

        private void InsertRequiredBytesGraph<TPrincipal, TDependent>()
            where TPrincipal : class, IBytesPrincipal, new()
            where TDependent : class, IBytesRequiredDependent, new()
        {
            using (var context = CreateContext())
            {
                context.Set<TPrincipal>().AddRange(
                    new TPrincipal { BackingId = new byte[]{ 11 }, Foo = "X1" },
                    new TPrincipal { BackingId = new byte[]{ 12, 12 }, Foo = "X2" },
                    new TPrincipal { BackingId = new byte[]{ 13, 13, 13 }, Foo = "X3" },
                    new TPrincipal { BackingId = new byte[]{ 14, 14, 14, 14 }, Foo = "X4" });

                context.Set<TDependent>().AddRange(
                    new TDependent { BackingId = new byte[]{ 111 }, BackingPrincipalId = new byte[]{ 11 } },
                    new TDependent { BackingId = new byte[]{ 112 }, BackingPrincipalId = new byte[]{ 12, 12 } },
                    new TDependent { BackingId = new byte[]{ 113 }, BackingPrincipalId = new byte[]{ 13, 13, 13 } },
                    new TDependent { BackingId = new byte[]{ 114 }, BackingPrincipalId = new byte[]{ 13, 13, 13 } },
                    new TDependent { BackingId = new byte[]{ 115 }, BackingPrincipalId = new byte[]{ 13, 13, 13 } },
                    new TDependent { BackingId = new byte[]{ 116 }, BackingPrincipalId = new byte[]{ 13, 13, 13 } });

                Assert.Equal(10, context.SaveChanges());
            }
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

#pragma warning disable 660,661 // Issue #19407
        protected struct IntStructKey
#pragma warning restore 660,661
        {
            public static ValueConverter<IntStructKey, int> Converter
                = new ValueConverter<IntStructKey, int>(v => v.Id, v => new IntStructKey { Id = v });

            public int Id { get; set; }

            public static bool operator ==(IntStructKey left, IntStructKey right)
                => left.Id == right.Id;

            public static bool operator !=(IntStructKey left, IntStructKey right)
                => left.Id != right.Id;
        }

#pragma warning disable 660,661 // Issue #19407
        protected struct BytesStructKey
#pragma warning restore 660,661
        {
            public static ValueConverter<BytesStructKey, byte[]> Converter
                = new ValueConverter<BytesStructKey, byte[]>(v => v.Id, v => new BytesStructKey { Id = v });

            public byte[] Id { get; set; }

            public bool Equals(BytesStructKey other)
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

            public static bool operator ==(BytesStructKey left, BytesStructKey right)
                => left.Equals(right);

            public static bool operator !=(BytesStructKey left, BytesStructKey right)
                => !left.Equals(right);
        }

#pragma warning disable 660,661 // Issue #19407
        protected struct ComparableIntStructKey : IComparable
#pragma warning restore 660,661
        {
            public static ValueConverter<ComparableIntStructKey, int> Converter
                = new ValueConverter<ComparableIntStructKey, int>(v => v.Id, v => new ComparableIntStructKey { Id = v });

            public int Id { get; set; }

            public static bool operator ==(ComparableIntStructKey left, ComparableIntStructKey right)
                => left.Id == right.Id;

            public static bool operator !=(ComparableIntStructKey left, ComparableIntStructKey right)
                => left.Id != right.Id;

            public int CompareTo(object other)
                => Id - ((ComparableIntStructKey)other).Id;
        }

#pragma warning disable 660,661 // Issue #19407
        protected struct ComparableBytesStructKey : IComparable
#pragma warning restore 660,661
        {
            public static ValueConverter<ComparableBytesStructKey, byte[]> Converter
                = new ValueConverter<ComparableBytesStructKey, byte[]>(v => v.Id, v => new ComparableBytesStructKey { Id = v });

            public byte[] Id { get; set; }

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

            public static bool operator ==(ComparableBytesStructKey left, ComparableBytesStructKey right)
                => left.Equals(right);

            public static bool operator !=(ComparableBytesStructKey left, ComparableBytesStructKey right)
                => !left.Equals(right);

            public int CompareTo(object other)
            {
                var result = Id.Length - ((ComparableBytesStructKey)other).Id.Length;
                if (result != 0)
                {
                    return result;
                }

                return StructuralComparisons.StructuralComparer.Compare(Id, ((ComparableBytesStructKey)other).Id);
            }
        }

#pragma warning disable 660,661 // Issue #19407
        protected struct GenericComparableIntStructKey : IComparable<GenericComparableIntStructKey>
#pragma warning restore 660,661
        {
            public static ValueConverter<GenericComparableIntStructKey, int> Converter
                = new ValueConverter<GenericComparableIntStructKey, int>(v => v.Id, v => new GenericComparableIntStructKey { Id = v });

            public int Id { get; set; }

            public static bool operator ==(GenericComparableIntStructKey left, GenericComparableIntStructKey right)
                => left.Id == right.Id;

            public static bool operator !=(GenericComparableIntStructKey left, GenericComparableIntStructKey right)
                => !(left == right);

            public int CompareTo(GenericComparableIntStructKey other)
                => Id - other.Id;
        }

#pragma warning disable 660,661 // Issue #19407
        protected struct GenericComparableBytesStructKey : IComparable<GenericComparableBytesStructKey>
#pragma warning restore 660,661
        {
            public static ValueConverter<GenericComparableBytesStructKey, byte[]> Converter
                = new ValueConverter<GenericComparableBytesStructKey, byte[]>(v => v.Id, v => new GenericComparableBytesStructKey { Id = v });

            public byte[] Id { get; set; }

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

            public static bool operator ==(GenericComparableBytesStructKey left, GenericComparableBytesStructKey right)
                => left.Equals(right);

            public static bool operator !=(GenericComparableBytesStructKey left, GenericComparableBytesStructKey right)
                => !left.Equals(right);

            public int CompareTo(GenericComparableBytesStructKey other)
            {
                var result = Id.Length - other.Id.Length;
                if (result != 0)
                {
                    return result;
                }

                return StructuralComparisons.StructuralComparer.Compare(Id, other.Id);
            }
        }

#pragma warning disable 660,661 // Issue #19407
        protected struct StructuralComparableBytesStructKey : IStructuralComparable
#pragma warning restore 660,661
        {
            public static ValueConverter<StructuralComparableBytesStructKey, byte[]> Converter
                = new ValueConverter<StructuralComparableBytesStructKey, byte[]>(v => v.Id, v => new StructuralComparableBytesStructKey { Id = v });

            public byte[] Id { get; set; }

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

            public static bool operator ==(StructuralComparableBytesStructKey left, StructuralComparableBytesStructKey right)
                => left.Equals(right);

            public static bool operator !=(StructuralComparableBytesStructKey left, StructuralComparableBytesStructKey right)
                => !left.Equals(right);

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

        protected class IntClassKey
        {
            public static ValueConverter<IntClassKey, int> Converter
                = new ValueConverter<IntClassKey, int>(v => v.Id, v => new IntClassKey { Id = v });

            protected bool Equals(IntClassKey other)
                => other != null && Id == other.Id;

            public override bool Equals(object obj)
                => obj == this
                    || obj?.GetType() == GetType()
                    && Equals((IntClassKey)obj);

            public override int GetHashCode() => Id;

            public int Id { get; set; }
        }

        protected class ComparableIntClassKey : IComparable
        {
            public static ValueConverter<ComparableIntClassKey, int> Converter
                = new ValueConverter<ComparableIntClassKey, int>(v => v.Id, v => new ComparableIntClassKey { Id = v });

            public int Id { get; set; }

            protected bool Equals(ComparableIntClassKey other)
                => other != null && Id == other.Id;

            public override bool Equals(object obj)
                => obj == this
                    || obj?.GetType() == GetType()
                    && Equals((ComparableIntClassKey)obj);

            public override int GetHashCode() => Id;
            public int CompareTo(object other)
                => Id - ((ComparableIntClassKey)other).Id;
        }

        protected class GenericComparableIntClassKey : IComparable<GenericComparableIntClassKey>
        {
            public static ValueConverter<GenericComparableIntClassKey, int> Converter
                = new ValueConverter<GenericComparableIntClassKey, int>(v => v.Id, v => new GenericComparableIntClassKey { Id = v });

            public int Id { get; set; }

            protected bool Equals(GenericComparableIntClassKey other)
                => other != null && Id == other.Id;

            public override bool Equals(object obj)
                => obj == this
                    || obj?.GetType() == GetType()
                    && Equals((GenericComparableIntClassKey)obj);

            public override int GetHashCode() => Id;
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
                set => Id = new IntStructKey { Id = value };
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
                set => Id = new IntStructKey { Id = value };
            }

            [NotMapped]
            public int BackingPrincipalId
            {
                get => PrincipalId.Id;
                set => PrincipalId = new IntStructKey { Id = value };
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
                set => Id = new IntStructKey { Id = value };
            }

            [NotMapped]
            public int? BackingPrincipalId
            {
                get => PrincipalId?.Id;
                set => PrincipalId = value.HasValue ? new IntStructKey { Id = value.Value } : (IntStructKey?)null;
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
                set => Id = new BytesStructKey { Id = value };
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
                set => Id = new BytesStructKey { Id = value };
            }

            [NotMapped]
            public byte[] BackingPrincipalId
            {
                get => PrincipalId?.Id;
                set => PrincipalId = value != null ? new BytesStructKey { Id = value } : (BytesStructKey?)null;
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
                set => Id = new BytesStructKey { Id = value };
            }

            [NotMapped]
            public byte[] BackingPrincipalId
            {
                get => PrincipalId.Id;
                set => PrincipalId = new BytesStructKey { Id = value };
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
                set => Id = new ComparableIntStructKey { Id = value };
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
                set => Id = new ComparableIntStructKey { Id = value };
            }

            [NotMapped]
            public int? BackingPrincipalId
            {
                get => PrincipalId?.Id;
                set => PrincipalId = value.HasValue ? new ComparableIntStructKey { Id = value.Value } : (ComparableIntStructKey?)null;
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
                set => Id = new ComparableIntStructKey { Id = value };
            }

            [NotMapped]
            public int BackingPrincipalId
            {
                get => PrincipalId.Id;
                set => PrincipalId = new ComparableIntStructKey { Id = value };
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
                set => Id = new ComparableBytesStructKey { Id = value };
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
                set => Id = new ComparableBytesStructKey { Id = value };
            }

            [NotMapped]
            public byte[] BackingPrincipalId
            {
                get => PrincipalId?.Id;
                set => PrincipalId = value != null ? new ComparableBytesStructKey { Id = value } : (ComparableBytesStructKey?)null;
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
                set => Id = new ComparableBytesStructKey { Id = value };
            }

            [NotMapped]
            public byte[] BackingPrincipalId
            {
                get => PrincipalId.Id;
                set => PrincipalId = new ComparableBytesStructKey { Id = value };
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
                set => Id = new GenericComparableIntStructKey { Id = value };
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
                set => Id = new GenericComparableIntStructKey { Id = value };
            }

            [NotMapped]
            public int? BackingPrincipalId
            {
                get => PrincipalId?.Id;
                set => PrincipalId = value.HasValue ? new GenericComparableIntStructKey { Id = value.Value } : (GenericComparableIntStructKey?)null;
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
                set => Id = new GenericComparableIntStructKey { Id = value };
            }

            [NotMapped]
            public int BackingPrincipalId
            {
                get => PrincipalId.Id;
                set => PrincipalId = new GenericComparableIntStructKey { Id = value };
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
                set => Id = new GenericComparableBytesStructKey { Id = value };
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
                set => Id = new GenericComparableBytesStructKey { Id = value };
            }

            [NotMapped]
            public byte[] BackingPrincipalId
            {
                get => PrincipalId?.Id;
                set => PrincipalId = value != null ? new GenericComparableBytesStructKey { Id = value } : (GenericComparableBytesStructKey?)null;
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
                set => Id = new GenericComparableBytesStructKey { Id = value };
            }

            [NotMapped]
            public byte[] BackingPrincipalId
            {
                get => PrincipalId.Id;
                set => PrincipalId = new GenericComparableBytesStructKey { Id = value };
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
                set => Id = new StructuralComparableBytesStructKey { Id = value };
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
                set => Id = new StructuralComparableBytesStructKey { Id = value };
            }

            [NotMapped]
            public byte[] BackingPrincipalId
            {
                get => PrincipalId?.Id;
                set => PrincipalId = value != null ? new StructuralComparableBytesStructKey { Id = value } : (StructuralComparableBytesStructKey?)null;
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
                set => Id = new StructuralComparableBytesStructKey { Id = value };
            }

            [NotMapped]
            public byte[] BackingPrincipalId
            {
                get => PrincipalId.Id;
                set => PrincipalId = new StructuralComparableBytesStructKey { Id = value };
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
                set => Id = new IntClassKey { Id = value };
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
                set => Id = new IntClassKey { Id = value };
            }

            [NotMapped]
            public int? BackingPrincipalId
            {
                get => PrincipalId?.Id;
                set => PrincipalId = value.HasValue ? new IntClassKey { Id = value.Value } : null;
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
                set => Id = new IntClassKey { Id = value };
            }

            [NotMapped]
            public int BackingPrincipalId
            {
                get => PrincipalId.Id;
                set => PrincipalId = new IntClassKey { Id = value };
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
                set => Id = new ComparableIntClassKey { Id = value };
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
                set => Id = new ComparableIntClassKey { Id = value };
            }

            [NotMapped]
            public int? BackingPrincipalId
            {
                get => PrincipalId?.Id;
                set => PrincipalId = value.HasValue ? new ComparableIntClassKey { Id = value.Value } : null;
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
                set => Id = new ComparableIntClassKey { Id = value };
            }

            [NotMapped]
            public int BackingPrincipalId
            {
                get => PrincipalId.Id;
                set => PrincipalId = new ComparableIntClassKey { Id = value };
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
                set => Id = new GenericComparableIntClassKey { Id = value };
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
                set => Id = new GenericComparableIntClassKey { Id = value };
            }

            [NotMapped]
            public int? BackingPrincipalId
            {
                get => PrincipalId?.Id;
                set => PrincipalId = value.HasValue ? new GenericComparableIntClassKey { Id = value.Value } : null;
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
                set => Id = new GenericComparableIntClassKey { Id = value };
            }

            [NotMapped]
            public int BackingPrincipalId
            {
                get => PrincipalId.Id;
                set => PrincipalId = new GenericComparableIntClassKey { Id = value };
            }
        }

        public abstract class KeysWithConvertersFixtureBase : SharedStoreFixtureBase<PoolableDbContext>
        {
            protected override string StoreName { get; } = "KeysWithConverters";

            protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            {
                modelBuilder.Entity<IntStructKeyPrincipal>(
                    b =>
                    {
                        b.Property(e => e.Id).HasConversion(IntStructKey.Converter);
                    });

                modelBuilder.Entity<IntStructKeyOptionalDependent>(
                    b =>
                    {
                        b.Property(e => e.Id).HasConversion(IntStructKey.Converter);
                        b.Property(e => e.PrincipalId).HasConversion(IntStructKey.Converter);
                    });

                modelBuilder.Entity<IntStructKeyRequiredDependent>(
                    b =>
                    {
                        b.Property(e => e.Id).HasConversion(IntStructKey.Converter);
                        b.Property(e => e.PrincipalId).HasConversion(IntStructKey.Converter);
                    });

                modelBuilder.Entity<IntClassKeyPrincipal>(
                    b =>
                    {
                        b.Property(e => e.Id).HasConversion(IntClassKey.Converter);
                    });

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

                modelBuilder.Entity<ComparableIntStructKeyPrincipal>(
                    b =>
                    {
                        b.Property(e => e.Id).HasConversion(ComparableIntStructKey.Converter);
                    });

                modelBuilder.Entity<ComparableIntStructKeyOptionalDependent>(
                    b =>
                    {
                        b.Property(e => e.Id).HasConversion(ComparableIntStructKey.Converter);
                        b.Property(e => e.PrincipalId).HasConversion(ComparableIntStructKey.Converter);
                    });

                modelBuilder.Entity<ComparableIntStructKeyRequiredDependent>(
                    b =>
                    {
                        b.Property(e => e.Id).HasConversion(ComparableIntStructKey.Converter);
                        b.Property(e => e.PrincipalId).HasConversion(ComparableIntStructKey.Converter);
                    });

                modelBuilder.Entity<GenericComparableIntStructKeyPrincipal>(
                    b =>
                    {
                        b.Property(e => e.Id).HasConversion(GenericComparableIntStructKey.Converter);
                    });

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
                    b =>
                    {
                        b.Property(e => e.Id).HasConversion(StructuralComparableBytesStructKey.Converter);
                    });

                modelBuilder.Entity<StructuralComparableBytesStructKeyOptionalDependent>(
                    b =>
                    {
                        b.Property(e => e.Id).HasConversion(StructuralComparableBytesStructKey.Converter);
                        b.Property(e => e.PrincipalId).HasConversion(StructuralComparableBytesStructKey.Converter);
                    });

                modelBuilder.Entity<StructuralComparableBytesStructKeyRequiredDependent>(
                    b =>
                    {
                        b.Property(e => e.Id).HasConversion(StructuralComparableBytesStructKey.Converter);
                        b.Property(e => e.PrincipalId).HasConversion(StructuralComparableBytesStructKey.Converter);
                    });

                modelBuilder.Entity<BytesStructKeyPrincipal>(
                    b =>
                    {
                        b.Property(e => e.Id).HasConversion(BytesStructKey.Converter);
                    });

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
                    b =>
                    {
                        b.Property(e => e.Id).HasConversion(ComparableBytesStructKey.Converter);
                    });

                modelBuilder.Entity<ComparableBytesStructKeyOptionalDependent>(
                    b =>
                    {
                        b.Property(e => e.Id).HasConversion(ComparableBytesStructKey.Converter);
                        b.Property(e => e.PrincipalId).HasConversion(ComparableBytesStructKey.Converter);
                    });

                modelBuilder.Entity<ComparableBytesStructKeyRequiredDependent>(
                    b =>
                    {
                        b.Property(e => e.Id).HasConversion(ComparableBytesStructKey.Converter);
                        b.Property(e => e.PrincipalId).HasConversion(ComparableBytesStructKey.Converter);
                    });

                modelBuilder.Entity<GenericComparableBytesStructKeyPrincipal>(
                    b =>
                    {
                        b.Property(e => e.Id).HasConversion(GenericComparableBytesStructKey.Converter);
                    });

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
                    b =>
                    {
                        b.Property(e => e.Id).HasConversion(ComparableIntClassKey.Converter);
                    });

                modelBuilder.Entity<ComparableIntClassKeyOptionalDependent>(
                    b =>
                    {
                        b.Property(e => e.Id).HasConversion(ComparableIntClassKey.Converter);
                        b.Property(e => e.PrincipalId).HasConversion(ComparableIntClassKey.Converter);
                    });

                modelBuilder.Entity<ComparableIntClassKeyRequiredDependent>(
                    b =>
                    {
                        b.Property(e => e.Id).HasConversion(ComparableIntClassKey.Converter);
                        b.Property(e => e.PrincipalId).HasConversion(ComparableIntClassKey.Converter);
                    });

                modelBuilder.Entity<GenericComparableIntClassKeyPrincipal>(
                    b =>
                    {
                        b.Property(e => e.Id).HasConversion(GenericComparableIntClassKey.Converter);
                    });

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
            }
        }
    }
}
