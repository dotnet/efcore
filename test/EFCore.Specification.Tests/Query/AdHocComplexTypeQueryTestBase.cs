// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.EntityFrameworkCore.Query;

// ReSharper disable ClassNeverInstantiated.Local
public abstract class AdHocComplexTypeQueryTestBase(NonSharedFixture fixture)
    : NonSharedModelTestBase(fixture), IClassFixture<NonSharedFixture>
{
    #region 33449

    [ConditionalFact]
    public virtual async Task Complex_type_equals_parameter_with_nested_types_with_property_of_same_name()
    {
        var contextFactory = await InitializeAsync<Context33449>(
            seed: context =>
            {
                context.AddRange(
                    new Context33449.EntityType
                    {
                        ComplexContainer = new Context33449.ComplexContainer
                        {
                            Id = 1,
                            Containee1 = new Context33449.ComplexContainee1 { Id = 2 },
                            Containee2 = new Context33449.ComplexContainee2 { Id = 3 }
                        }
                    });
                return context.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateContext();

        var container = new Context33449.ComplexContainer
        {
            Id = 1,
            Containee1 = new Context33449.ComplexContainee1 { Id = 2 },
            Containee2 = new Context33449.ComplexContainee2 { Id = 3 }
        };

        _ = await context.Set<Context33449.EntityType>().Where(b => b.ComplexContainer == container).SingleAsync();
    }

    private class Context33449(DbContextOptions options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<EntityType>().ComplexProperty(
                b => b.ComplexContainer, x =>
                {
                    x.ComplexProperty(c => c.Containee1);
                    x.ComplexProperty(c => c.Containee2);
                });

        public class EntityType
        {
            public int Id { get; set; }
            public ComplexContainer ComplexContainer { get; set; } = null!;
        }

        public class ComplexContainer
        {
            public int Id { get; set; }

            public ComplexContainee1 Containee1 { get; set; } = null!;
            public ComplexContainee2 Containee2 { get; set; } = null!;
        }

        public class ComplexContainee1
        {
            public int Id { get; set; }
        }

        public class ComplexContainee2
        {
            public int Id { get; set; }
        }
    }

    #endregion 33449

    #region 34749

    [ConditionalFact]
    public virtual async Task Projecting_complex_property_does_not_auto_include_owned_types()
    {
        var contextFactory = await InitializeAsync<Context34749>();

        await using var context = contextFactory.CreateContext();

        _ = await context.Set<Context34749.EntityType>().Select(x => x.Complex).ToListAsync();
    }

    private class Context34749(DbContextOptions options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<EntityType>(b =>
            {
                b.ComplexProperty(x => x.Complex);
                b.OwnsOne(x => x.OwnedReference);
            });

        public class EntityType
        {
            public int Id { get; set; }
            public string? Name { get; set; }
            public OwnedType OwnedReference { get; set; } = null!;
            public ComplexType Complex { get; set; } = null!;
        }

        public class ComplexType
        {
            public int Number { get; set; }
            public string? Name { get; set; }
        }

        public class OwnedType
        {
            public string? Foo { get; set; }
            public int Bar { get; set; }
        }
    }

    #endregion

    #region ShadowDiscriminator

    [ConditionalFact]
    public virtual async Task Optional_complex_type_with_discriminator()
    {
        var contextFactory = await InitializeAsync<ContextShadowDiscriminator>(
            seed: context =>
            {
                context.AddRange(
                    new ContextShadowDiscriminator.EntityType
                    {
                        AllOptionalsComplexType = new ContextShadowDiscriminator.AllOptionalsComplexType { OptionalProperty = "Non-null" }
                    },
                    new ContextShadowDiscriminator.EntityType
                    {
                        AllOptionalsComplexType = new ContextShadowDiscriminator.AllOptionalsComplexType { OptionalProperty = null }
                    },
                    new ContextShadowDiscriminator.EntityType
                    {
                        AllOptionalsComplexType = null
                    }
                    );
                return context.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateContext();

        var complexTypeNull = await context.Set<ContextShadowDiscriminator.EntityType>().SingleAsync(b => b.AllOptionalsComplexType == null);
        Assert.Null(complexTypeNull.AllOptionalsComplexType);

        complexTypeNull.AllOptionalsComplexType = new ContextShadowDiscriminator.AllOptionalsComplexType { OptionalProperty = "New thing" };
        await context.SaveChangesAsync();
    }

    private class ContextShadowDiscriminator(DbContextOptions options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<EntityType>()
                .ComplexProperty(b => b.AllOptionalsComplexType, x => x.HasDiscriminator());

        public class EntityType
        {
            public int Id { get; set; }
            public AllOptionalsComplexType? AllOptionalsComplexType { get; set; }
        }

        public class AllOptionalsComplexType
        {
            public string? OptionalProperty { get; set; }
        }
    }

    #endregion ShadowDiscriminator

    #region 36837

    [ConditionalFact]
    public virtual async Task Complex_type_equality_with_non_default_type_mapping()
    {
        var contextFactory = await InitializeAsync<Context36837>(
            seed: context =>
            {
                context.AddRange(
                    new Context36837.EntityType
                    {
                        ComplexThing = new Context36837.ComplexThing { DateTime = new DateTime(2020, 1, 1) }
                    });
                return context.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateContext();

        var count = await context.Set<Context36837.EntityType>()
            .CountAsync(b => b.ComplexThing == new Context36837.ComplexThing { DateTime = new DateTime(2020, 1, 1, 1, 1, 1, 999, 999) });
        Assert.Equal(0, count);
    }

    private class Context36837(DbContextOptions options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<EntityType>().ComplexProperty(b => b.ComplexThing);

        public class EntityType
        {
            public int Id { get; set; }
            public ComplexThing ComplexThing { get; set; } = null!;
        }

        public class ComplexThing
        {
            [Column(TypeName = "datetime")] // Non-default type mapping
            public DateTime DateTime { get; set; }
        }
    }

    #endregion 36837

    #region 37162

    [ConditionalFact]
    public virtual async Task Non_optional_complex_type_with_all_nullable_properties()
    {
        var contextFactory = await InitializeAsync<Context37162>(
            seed: context =>
            {
                context.Add(
                    new Context37162.EntityType
                    {
                        NonOptionalComplexType = new Context37162.ComplexTypeWithAllNulls
                        {
                            // All properties are null
                        }
                    });
                return context.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateContext();

        var entity = await context.Set<Context37162.EntityType>().SingleAsync();

        Assert.NotNull(entity.NonOptionalComplexType);
        Assert.Null(entity.NonOptionalComplexType.NullableString);
        Assert.Null(entity.NonOptionalComplexType.NullableDateTime);
    }

    private class Context37162(DbContextOptions options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<EntityType>().ComplexProperty(b => b.NonOptionalComplexType);

        public class EntityType
        {
            public int Id { get; set; }
            public ComplexTypeWithAllNulls NonOptionalComplexType { get; set; } = null!;
        }

        public class ComplexTypeWithAllNulls
        {
            public string? NullableString { get; set; }
            public DateTime? NullableDateTime { get; set; }
        }
    }

    #endregion 37162

    protected override string StoreName
        => "AdHocComplexTypeQueryTest";
}
