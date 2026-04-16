// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

// ReSharper disable ClassNeverInstantiated.Local
public abstract class AdHocComplexTypeQueryTestBase(NonSharedFixture fixture)
    : NonSharedModelTestBase(fixture), IClassFixture<NonSharedFixture>
{
    #region 33449

    [ConditionalFact]
    public virtual async Task Complex_type_equals_parameter_with_nested_types_with_property_of_same_name()
    {
        var contextFactory = await InitializeNonSharedTest<Context33449>(
            seed: context =>
            {
                context.AddRange(
                    new Context33449.EntityType
                    {
                        Id = 1,
                        ComplexContainer = new Context33449.ComplexContainer
                        {
                            Id = 1,
                            Containee1 = new Context33449.ComplexContainee1 { Id = 2 },
                            Containee2 = new Context33449.ComplexContainee2 { Id = 3 }
                        }
                    });
                return context.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateDbContext();

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
            => modelBuilder.Entity<EntityType>(b =>
            {
                b.Property(b => b.Id).ValueGeneratedNever();
                b.ComplexProperty(b => b.ComplexContainer, x =>
                {
                    x.ComplexProperty(c => c.Containee1);
                    x.ComplexProperty(c => c.Containee2);
                });
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
        var contextFactory = await InitializeNonSharedTest<Context34749>();

        await using var context = contextFactory.CreateDbContext();

        _ = await context.Set<Context34749.EntityType>().Select(x => x.Complex).ToListAsync();
    }

    private class Context34749(DbContextOptions options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<EntityType>(b =>
            {
                b.Property(b => b.Id).ValueGeneratedNever();
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
        var contextFactory = await InitializeNonSharedTest<ContextShadowDiscriminator>(
            seed: context =>
            {
                context.AddRange(
                    new ContextShadowDiscriminator.EntityType
                    {
                        Id = 1,
                        AllOptionalsComplexType = new ContextShadowDiscriminator.AllOptionalsComplexType { OptionalProperty = "Non-null" }
                    },
                    new ContextShadowDiscriminator.EntityType
                    {
                        Id = 2,
                        AllOptionalsComplexType = new ContextShadowDiscriminator.AllOptionalsComplexType { OptionalProperty = null }
                    },
                    new ContextShadowDiscriminator.EntityType
                    {
                        Id = 3,
                        AllOptionalsComplexType = null
                    }
                    );
                return context.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateDbContext();

        var complexTypeNull = await context.Set<ContextShadowDiscriminator.EntityType>().SingleAsync(b => b.AllOptionalsComplexType == null);
        Assert.Null(complexTypeNull.AllOptionalsComplexType);

        complexTypeNull.AllOptionalsComplexType = new ContextShadowDiscriminator.AllOptionalsComplexType { OptionalProperty = "New thing" };
        await context.SaveChangesAsync();
    }

    private class ContextShadowDiscriminator(DbContextOptions options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<EntityType>(b =>
            {
                b.Property(b => b.Id).ValueGeneratedNever();
                b.ComplexProperty(b => b.AllOptionalsComplexType, x => x.HasDiscriminator());
            });

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

    #region 37162

    [ConditionalFact]
    public virtual async Task Non_optional_complex_type_with_all_nullable_properties()
    {
        var contextFactory = await InitializeNonSharedTest<Context37162>(
            seed: context =>
            {
                context.Add(
                    new Context37162.EntityType
                    {
                        Id = 1,
                        NonOptionalComplexType = new Context37162.ComplexTypeWithAllNulls
                        {
                            // All properties are null
                        }
                    });
                return context.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateDbContext();

        var entity = await context.Set<Context37162.EntityType>().SingleAsync();

        Assert.NotNull(entity.NonOptionalComplexType);
        Assert.Null(entity.NonOptionalComplexType.NullableString);
        Assert.Null(entity.NonOptionalComplexType.NullableDateTime);
    }

    private class Context37162(DbContextOptions options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<EntityType>(b =>
            {
                b.Property(b => b.Id).ValueGeneratedNever();
                b.ComplexProperty(b => b.NonOptionalComplexType);
            });

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
        var contextFactory = await InitializeNonSharedTest<Context37304>(
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

        await using var context = contextFactory.CreateDbContext();

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

    private const string Issue37337CreatedByShadowPropertyName = "CreatedBy";

    [ConditionalFact]
    public virtual async Task Nullable_complex_type_with_discriminator_and_shadow_property()
    {
        var contextFactory = await InitializeNonSharedTest<Context37337>(
            seed: context =>
            {
                var entity = new Context37337.EntityType
                {
                    Id = Guid.NewGuid(),
                    Prop = new Context37337.OptionalComplexProperty
                    {
                        OptionalValue = true
                    }
                };
                context.Add(entity);
                context.Entry(entity).Property(Issue37337CreatedByShadowPropertyName).CurrentValue = "Seeder";
                return context.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateDbContext();

        var entities = await context.Set<Context37337.EntityType>().ToArrayAsync();

        Assert.Single(entities);
        var entity = entities[0];
        Assert.NotNull(entity.Prop);
        Assert.True(entity.Prop.OptionalValue);

        var entry = context.Entry(entity);
        Assert.Equal("Seeder", entry.Property(Issue37337CreatedByShadowPropertyName).CurrentValue);
    }

    protected class Context37337(DbContextOptions options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var entity = modelBuilder.Entity<EntityType>();
            entity.Property(p => p.Id).ValueGeneratedNever();
            entity.HasKey(p => p.Id);

            var compl = entity.ComplexProperty(p => p.Prop);
            compl.Property(p => p.OptionalValue);
            compl.HasDiscriminator();

            // Shadow property added via convention (e.g., audit field)
            entity.Property<string>(Issue37337CreatedByShadowPropertyName).IsRequired(false);
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

    #region Issue38105

    [ConditionalFact]
    public virtual async Task Update_entity_with_nullable_complex_type_and_discriminator_does_not_throw()
    {
        var contextFactory = await InitializeNonSharedTest<Context37337>(
            seed: context =>
            {
                var entity = new Context37337.EntityType
                {
                    Id = Guid.NewGuid(),
                    Prop = new Context37337.OptionalComplexProperty
                    {
                        OptionalValue = true
                    }
                };
                context.Add(entity);
                context.Entry(entity).Property(Issue37337CreatedByShadowPropertyName).CurrentValue = "Seeder";
                return context.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateDbContext();

        var entity = await context.Set<Context37337.EntityType>().SingleAsync();
        var id = entity.Id;
        context.ChangeTracker.Clear();

        // Create a new disconnected instance with the same key and Update it.
        // The complex type discriminator (shadow property with AfterSaveBehavior.Throw) should not
        // be marked as modified by Update(), and SaveChanges should succeed without throwing.
        var updatedEntity = new Context37337.EntityType
        {
            Id = id,
            Prop = new Context37337.OptionalComplexProperty
            {
                OptionalValue = false
            }
        };

        context.Update(updatedEntity);

        await context.SaveChangesAsync();

        context.ChangeTracker.Clear();

        var reloaded = await context.Set<Context37337.EntityType>().SingleAsync();
        Assert.Equal(id, reloaded.Id);
        Assert.NotNull(reloaded.Prop);
        Assert.False(reloaded.Prop.OptionalValue);
    }

    #endregion Issue38105

    protected override string NonSharedStoreName
        => "AdHocComplexTypeQueryTest";
}
