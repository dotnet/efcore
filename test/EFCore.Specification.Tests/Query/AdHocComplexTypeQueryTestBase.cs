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

        await using (var context = contextFactory.CreateContext())
        {
            var complexTypeNull = await context.Set<ContextShadowDiscriminator.EntityType>()
                .SingleAsync(b => b.AllOptionalsComplexType == null);
            Assert.Null(complexTypeNull.AllOptionalsComplexType);

            complexTypeNull.AllOptionalsComplexType =
                new ContextShadowDiscriminator.AllOptionalsComplexType { OptionalProperty = "New thing" };
            await context.SaveChangesAsync();
        }

        await using (var context = contextFactory.CreateContext())
        {
            var entities = await context.Set<ContextShadowDiscriminator.EntityType>().ToListAsync();
            Assert.Equal(3, entities.Count);
            Assert.All(entities, e => Assert.NotNull(e.AllOptionalsComplexType));
        }
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

    #region 37304

    [ConditionalFact]
    public virtual async Task Non_optional_complex_type_with_all_nullable_properties_via_left_join()
    {
        var contextFactory = await InitializeAsync<Context37304>(
            seed: context =>
            {
                context.Add(
                    new Context37304.Parent
                    {
                        Id = 1,
                        Children =
                        [
                            new Context37304.Child
                            {
                                Id = 1,
                                ComplexType = new Context37304.ComplexTypeWithAllNulls()
                            }
                        ]
                    });
                return context.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateContext();

        var parent = await context.Set<Context37304.Parent>().Include(p => p.Children).SingleAsync();

        var child = parent.Children.Single();
        Assert.NotNull(child.ComplexType);
        Assert.Null(child.ComplexType.NullableString);
        Assert.Null(child.ComplexType.NullableDateTime);
    }

    private class Context37304(DbContextOptions options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Parent>(b =>
            {
                b.Property(p => p.Id).ValueGeneratedNever();
            });

            modelBuilder.Entity<Child>(b =>
            {
                b.Property(c => c.Id).ValueGeneratedNever();
                b.HasOne(c => c.Parent).WithMany(p => p.Children).HasForeignKey(c => c.ParentId);
                b.ComplexProperty(c => c.ComplexType);
            });
        }

        public class Parent
        {
            public int Id { get; set; }
            public List<Child> Children { get; set; } = [];
        }

        public class Child
        {
            public int Id { get; set; }
            public int ParentId { get; set; }
            public Parent Parent { get; set; } = null!;
            public ComplexTypeWithAllNulls ComplexType { get; set; } = null!;
        }

        public class ComplexTypeWithAllNulls
        {
            public string? NullableString { get; set; }
            public DateTime? NullableDateTime { get; set; }
        }
    }

    #endregion 37304

    #region Issue37337

    [ConditionalFact]
    public virtual async Task Nullable_complex_type_with_discriminator_and_shadow_property()
    {
        var contextFactory = await InitializeAsync<Context37337>(
            seed: context =>
            {
                context.Add(
                    new Context37337.EntityType
                    {
                        Prop = new Context37337.OptionalComplexProperty
                        {
                            OptionalValue = true
                        }
                    });
                return context.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateContext();

        var entities = await context.Set<Context37337.EntityType>().ToArrayAsync();

        Assert.Single(entities);
        var entity = entities[0];
        Assert.NotNull(entity.Prop);
        Assert.True(entity.Prop.OptionalValue);
    }

    private class Context37337(DbContextOptions options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var entity = modelBuilder.Entity<EntityType>();
            entity.Property(p => p.Id);
            entity.HasKey(p => p.Id);

            var compl = entity.ComplexProperty(p => p.Prop);
            compl.Property(p => p.OptionalValue);
            compl.HasDiscriminator();

            // Shadow property added via convention (e.g., audit field)
            entity.Property<string>("CreatedBy").IsRequired(false);
        }

        public class EntityType
        {
            public Guid Id { get; set; }
            public OptionalComplexProperty? Prop { get; set; }
        }

        public class OptionalComplexProperty
        {
            public bool? OptionalValue { get; set; }
        }
    }

    #endregion Issue37337

    #region Issue38119

    [ConditionalFact]
    public virtual async Task Nullable_complex_type_with_discriminator_null_to_non_null_roundtrip()
    {
        var contextFactory = await InitializeAsync<Context38119>(
            seed: context =>
            {
                context.Add(new Context38119.EntityType { Id = Guid.NewGuid() });
                return context.SaveChangesAsync();
            });

        await using (var context = contextFactory.CreateContext())
        {
            var entity = await context.Set<Context38119.EntityType>().SingleAsync();
            Assert.Null(entity.Prop);

            entity.Prop = new Context38119.OptionalComplexProperty { OptionalValue = true };
            await context.SaveChangesAsync();
        }

        await using (var context = contextFactory.CreateContext())
        {
            var entity = await context.Set<Context38119.EntityType>().SingleAsync();
            Assert.NotNull(entity.Prop);
            Assert.True(entity.Prop.OptionalValue);
        }
    }

    [ConditionalFact]
    public virtual async Task Nullable_complex_type_with_discriminator_non_null_to_null_roundtrip()
    {
        var contextFactory = await InitializeAsync<Context38119>(
            seed: context =>
            {
                context.Add(
                    new Context38119.EntityType
                    {
                        Id = Guid.NewGuid(),
                        Prop = new Context38119.OptionalComplexProperty { OptionalValue = true }
                    });
                return context.SaveChangesAsync();
            });

        await using (var context = contextFactory.CreateContext())
        {
            var entity = await context.Set<Context38119.EntityType>().SingleAsync();
            Assert.NotNull(entity.Prop);

            entity.Prop = null;
            await context.SaveChangesAsync();
        }

        await using (var context = contextFactory.CreateContext())
        {
            var entity = await context.Set<Context38119.EntityType>().SingleAsync();
            Assert.Null(entity.Prop);
        }
    }

    [ConditionalFact]
    public virtual async Task Nullable_complex_type_with_discriminator_update_non_null_entity_roundtrip()
    {
        var contextFactory = await InitializeAsync<Context38119>(
            seed: context =>
            {
                context.Add(
                    new Context38119.EntityType
                    {
                        Id = Guid.NewGuid(),
                        Prop = new Context38119.OptionalComplexProperty { OptionalValue = true }
                    });
                return context.SaveChangesAsync();
            });

        await using (var context = contextFactory.CreateContext())
        {
            var entity = await context.Set<Context38119.EntityType>().SingleAsync();
            Assert.NotNull(entity.Prop);
            Assert.True(entity.Prop.OptionalValue);

            context.Update(entity);
            await context.SaveChangesAsync();
        }

        await using (var context = contextFactory.CreateContext())
        {
            var entity = await context.Set<Context38119.EntityType>().SingleAsync();
            Assert.NotNull(entity.Prop);
            Assert.True(entity.Prop.OptionalValue);
        }
    }

    [ConditionalFact]
    public virtual async Task Nullable_complex_type_with_discriminator_set_to_different_value()
    {
        var contextFactory = await InitializeAsync<Context38119>();

        Guid entityId;
        await using (var context = contextFactory.CreateContext())
        {
            var entity = new Context38119.EntityType
            {
                Id = Guid.NewGuid(),
                Prop = new Context38119.OptionalComplexProperty { OptionalValue = true }
            };
            context.Add(entity);
            entityId = entity.Id;

            // Override the discriminator value before saving
            var discriminatorEntry = context.Entry(entity).ComplexProperty(e => e.Prop).Property("Discriminator");
            Assert.Equal("OptionalComplexProperty", discriminatorEntry.CurrentValue);
            discriminatorEntry.CurrentValue = "SomeOtherValue";
            await context.SaveChangesAsync();
        }

        await using (var context = contextFactory.CreateContext())
        {
            // The discriminator is non-null so the complex property is still materialized
            var entity = await context.Set<Context38119.EntityType>().SingleAsync(e => e.Id == entityId);
            Assert.NotNull(entity.Prop);
            Assert.True(entity.Prop.OptionalValue);
        }
    }

    [ConditionalFact]
    public virtual async Task Nullable_complex_type_with_discriminator_set_to_null()
    {
        var contextFactory = await InitializeAsync<Context38119>();

        Guid entityId;
        await using (var context = contextFactory.CreateContext())
        {
            var entity = new Context38119.EntityType
            {
                Id = Guid.NewGuid(),
                Prop = new Context38119.OptionalComplexProperty { OptionalValue = true }
            };
            context.Add(entity);
            entityId = entity.Id;

            // Set discriminator to null before saving, which should cause the complex property to be null on reload
            var discriminatorEntry = context.Entry(entity).ComplexProperty(e => e.Prop).Property("Discriminator");
            Assert.Equal("OptionalComplexProperty", discriminatorEntry.CurrentValue);
            discriminatorEntry.CurrentValue = null;
            await context.SaveChangesAsync();
        }

        await using (var context = contextFactory.CreateContext())
        {
            // With null discriminator, the complex property should be materialized as null
            var entity = await context.Set<Context38119.EntityType>().SingleAsync(e => e.Id == entityId);
            Assert.Null(entity.Prop);
        }
    }

    [ConditionalFact]
    public virtual async Task Nested_nullable_complex_type_with_discriminator_null_to_non_null_roundtrip()
    {
        var contextFactory = await InitializeAsync<Context38119Nested>(
            seed: context =>
            {
                context.Add(
                    new Context38119Nested.EntityType
                    {
                        Id = Guid.NewGuid(),
                        Outer = new Context38119Nested.OuterComplexProperty { Name = "outer" }
                    });
                return context.SaveChangesAsync();
            });

        await using (var context = contextFactory.CreateContext())
        {
            var entity = await context.Set<Context38119Nested.EntityType>().SingleAsync();
            Assert.NotNull(entity.Outer);
            Assert.Null(entity.Outer.Inner);

            entity.Outer.Inner = new Context38119Nested.InnerComplexProperty { Value = 42 };
            await context.SaveChangesAsync();
        }

        await using (var context = contextFactory.CreateContext())
        {
            var entity = await context.Set<Context38119Nested.EntityType>().SingleAsync();
            Assert.NotNull(entity.Outer);
            Assert.NotNull(entity.Outer.Inner);
            Assert.Equal(42, entity.Outer.Inner.Value);
        }
    }

    private class Context38119(DbContextOptions options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var entity = modelBuilder.Entity<EntityType>();
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Id).ValueGeneratedNever();

            var compl = entity.ComplexProperty(p => p.Prop);
            compl.HasDiscriminator();
        }

        public class EntityType
        {
            public Guid Id { get; set; }
            public OptionalComplexProperty? Prop { get; set; }
        }

        public class OptionalComplexProperty
        {
            public bool? OptionalValue { get; set; }
        }
    }

    private class Context38119Nested(DbContextOptions options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var entity = modelBuilder.Entity<EntityType>();
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Id).ValueGeneratedNever();

            entity.ComplexProperty(
                p => p.Outer, outer =>
                {
                    outer.ComplexProperty(
                        p => p.Inner, inner => inner.HasDiscriminator());
                });
        }

        public class EntityType
        {
            public Guid Id { get; set; }
            public OuterComplexProperty Outer { get; set; } = null!;
        }

        public class OuterComplexProperty
        {
            public string? Name { get; set; }
            public InnerComplexProperty? Inner { get; set; }
        }

        public class InnerComplexProperty
        {
            public int? Value { get; set; }
        }
    }

    #endregion Issue38119

    protected override string StoreName
        => "AdHocComplexTypeQueryTest";
}
