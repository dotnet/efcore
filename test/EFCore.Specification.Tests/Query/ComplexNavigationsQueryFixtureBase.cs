// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.ComplexNavigationsModel;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class ComplexNavigationsQueryFixtureBase : SharedStoreFixtureBase<ComplexNavigationsContext>, IQueryFixtureBase
{
    protected override string StoreName
        => "ComplexNavigations";

    public Func<DbContext> GetContextCreator()
        => () => CreateContext();

    public virtual ISetSource GetExpectedData()
        => ComplexNavigationsDefaultData.Instance;

    public virtual Dictionary<(Type, string), Func<object, object>> GetShadowPropertyMappings()
    {
        var l1s = GetExpectedData().Set<Level1>().ToList();
        var l2s = GetExpectedData().Set<Level2>().ToList();
        var l3s = GetExpectedData().Set<Level3>().ToList();
        var l4s = GetExpectedData().Set<Level4>().ToList();

        var ib1s = GetExpectedData().Set<InheritanceBase1>().ToList();
        var ib2s = GetExpectedData().Set<InheritanceBase2>().ToList();
        var il1s = GetExpectedData().Set<InheritanceLeaf1>().ToList();
        var il2s = GetExpectedData().Set<InheritanceLeaf2>().ToList();

        return new Dictionary<(Type, string), Func<object, object>>
        {
            {
                (typeof(Level1), "OneToOne_Optional_Self1Id"),
                e => l1s.SingleOrDefault(l => l.Id == ((Level1)e)?.Id)?.OneToOne_Optional_Self1?.Id
            },
            {
                (typeof(Level1), "OneToMany_Required_Self_Inverse1Id"),
                e => l1s.SingleOrDefault(l => l.Id == ((Level1)e)?.Id)?.OneToMany_Required_Self_Inverse1?.Id
            },
            {
                (typeof(Level1), "OneToMany_Optional_Self_Inverse1Id"),
                e => l1s.SingleOrDefault(l => l.Id == ((Level1)e)?.Id)?.OneToMany_Optional_Self_Inverse1?.Id
            },
            {
                (typeof(Level2), "OneToOne_Optional_PK_Inverse2Id"),
                e => l2s.SingleOrDefault(l => l.Id == ((Level2)e)?.Id)?.OneToOne_Optional_PK_Inverse2?.Id
            },
            {
                (typeof(Level2), "OneToMany_Required_Inverse2Id"),
                e => l2s.SingleOrDefault(l => l.Id == ((Level2)e)?.Id)?.OneToMany_Required_Inverse2?.Id
            },
            {
                (typeof(Level2), "OneToMany_Optional_Inverse2Id"),
                e => l2s.SingleOrDefault(l => l.Id == ((Level2)e)?.Id)?.OneToMany_Optional_Inverse2?.Id
            },
            {
                (typeof(Level2), "OneToOne_Optional_Self2Id"),
                e => l2s.SingleOrDefault(l => l.Id == ((Level2)e)?.Id)?.OneToOne_Optional_Self2?.Id
            },
            {
                (typeof(Level2), "OneToMany_Required_Self_Inverse2Id"),
                e => l2s.SingleOrDefault(l => l.Id == ((Level2)e)?.Id)?.OneToMany_Required_Self_Inverse2?.Id
            },
            {
                (typeof(Level2), "OneToMany_Optional_Self_Inverse2Id"),
                e => l2s.SingleOrDefault(l => l.Id == ((Level2)e)?.Id)?.OneToMany_Optional_Self_Inverse2?.Id
            },
            {
                (typeof(Level3), "OneToOne_Optional_PK_Inverse3Id"),
                e => l3s.SingleOrDefault(l => l.Id == ((Level3)e)?.Id)?.OneToOne_Optional_PK_Inverse3?.Id
            },
            {
                (typeof(Level3), "OneToMany_Required_Inverse3Id"),
                e => l3s.SingleOrDefault(l => l.Id == ((Level3)e)?.Id)?.OneToMany_Required_Inverse3?.Id
            },
            {
                (typeof(Level3), "OneToMany_Optional_Inverse3Id"),
                e => l3s.SingleOrDefault(l => l.Id == ((Level3)e)?.Id)?.OneToMany_Optional_Inverse3?.Id
            },
            {
                (typeof(Level3), "OneToOne_Optional_Self3Id"),
                e => l3s.SingleOrDefault(l => l.Id == ((Level3)e)?.Id)?.OneToOne_Optional_Self3?.Id
            },
            {
                (typeof(Level3), "OneToMany_Required_Self_Inverse3Id"),
                e => l3s.SingleOrDefault(l => l.Id == ((Level3)e)?.Id)?.OneToMany_Required_Self_Inverse3?.Id
            },
            {
                (typeof(Level3), "OneToMany_Optional_Self_Inverse3Id"),
                e => l3s.SingleOrDefault(l => l.Id == ((Level3)e)?.Id)?.OneToMany_Optional_Self_Inverse3?.Id
            },
            {
                (typeof(Level4), "OneToOne_Optional_PK_Inverse4Id"),
                e => l4s.SingleOrDefault(l => l.Id == ((Level4)e)?.Id)?.OneToOne_Optional_PK_Inverse4?.Id
            },
            {
                (typeof(Level4), "OneToMany_Required_Inverse4Id"),
                e => l4s.SingleOrDefault(l => l.Id == ((Level4)e)?.Id)?.OneToMany_Required_Inverse4?.Id
            },
            {
                (typeof(Level4), "OneToMany_Optional_Inverse4Id"),
                e => l4s.SingleOrDefault(l => l.Id == ((Level4)e)?.Id)?.OneToMany_Optional_Inverse4?.Id
            },
            {
                (typeof(Level4), "OneToOne_Optional_Self4Id"),
                e => l4s.SingleOrDefault(l => l.Id == ((Level4)e)?.Id)?.OneToOne_Optional_Self4?.Id
            },
            {
                (typeof(Level4), "OneToMany_Required_Self_Inverse4Id"),
                e => l4s.SingleOrDefault(l => l.Id == ((Level4)e)?.Id)?.OneToMany_Required_Self_Inverse4?.Id
            },
            {
                (typeof(Level4), "OneToMany_Optional_Self_Inverse4Id"),
                e => l4s.SingleOrDefault(l => l.Id == ((Level4)e)?.Id)?.OneToMany_Optional_Self_Inverse4?.Id
            },
            { (typeof(InheritanceBase1), "InheritanceBase2Id"), e => ((InheritanceBase1)e)?.Id == 1 ? 1 : null },
            { (typeof(InheritanceBase1), "InheritanceBase2Id1"), e => ((InheritanceBase1)e)?.Id == 1 ? null : 1 },
            { (typeof(InheritanceBase2), "InheritanceLeaf2Id"), e => ((InheritanceBase2)e)?.Id == 1 ? 1 : null },
            {
                (typeof(InheritanceLeaf1), "DifferentTypeReference_InheritanceDerived1Id"), e =>
                {
                    switch (((InheritanceLeaf1)e)?.Id)
                    {
                        case 1:
                            return 1;
                        case 2:
                            return 2;
                        default:
                            return null;
                    }
                }
            },
            {
                (typeof(InheritanceLeaf1), "InheritanceDerived1Id"), e =>
                {
                    switch (((InheritanceLeaf1)e)?.Id)
                    {
                        case 1:
                            return 1;
                        case 2:
                            return 2;
                        case 3:
                            return 2;
                        default:
                            return null;
                    }
                }
            },
            { (typeof(InheritanceLeaf1), "InheritanceDerived1Id1"), e => ((InheritanceLeaf1)e)?.Id == 1 ? 1 : null },
            {
                (typeof(InheritanceLeaf1), "InheritanceDerived2Id"), e =>
                {
                    switch (((InheritanceLeaf1)e)?.Id)
                    {
                        case 2:
                            return 3;
                        case 3:
                            return 3;
                        default:
                            return null;
                    }
                }
            },
            {
                (typeof(InheritanceLeaf1), "SameTypeReference_InheritanceDerived1Id"), e =>
                {
                    switch (((InheritanceLeaf1)e)?.Id)
                    {
                        case 1:
                            return 1;
                        case 2:
                            return 2;
                        default:
                            return null;
                    }
                }
            },
            { (typeof(InheritanceLeaf1), "SameTypeReference_InheritanceDerived2Id"), e => ((InheritanceLeaf1)e)?.Id == 3 ? 3 : null },
            { (typeof(InheritanceLeaf2), "DifferentTypeReference_InheritanceDerived2Id"), e => ((InheritanceLeaf2)e)?.Id == 1 ? 3 : null },
            { (typeof(InheritanceLeaf2), "InheritanceDerived2Id"), e => ((InheritanceLeaf2)e)?.Id == 1 ? 3 : null },
        };
    }

    public IReadOnlyDictionary<Type, object> EntitySorters { get; } = new Dictionary<Type, Func<object, object>>
    {
        { typeof(Level1), e => ((Level1)e)?.Id },
        { typeof(Level2), e => ((Level2)e)?.Id },
        { typeof(Level3), e => ((Level3)e)?.Id },
        { typeof(Level4), e => ((Level4)e)?.Id },
        { typeof(InheritanceBase1), e => ((InheritanceBase1)e)?.Id },
        { typeof(InheritanceBase2), e => ((InheritanceBase2)e)?.Id },
        { typeof(InheritanceDerived1), e => ((InheritanceDerived1)e)?.Id },
        { typeof(InheritanceDerived2), e => ((InheritanceDerived2)e)?.Id },
        { typeof(InheritanceLeaf1), e => ((InheritanceLeaf1)e)?.Id },
        { typeof(InheritanceLeaf2), e => ((InheritanceLeaf2)e)?.Id }
    }.ToDictionary(e => e.Key, e => (object)e.Value);

    public IReadOnlyDictionary<Type, object> EntityAsserters { get; } = new Dictionary<Type, Action<object, object>>
    {
        {
            typeof(Level1), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (Level1)e;
                    var aa = (Level1)a;

                    Assert.Equal(ee.Id, aa.Id);
                    Assert.Equal(ee.Name, aa.Name);
                    Assert.Equal(ee.Date, aa.Date);
                }
            }
        },
        {
            typeof(Level2), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (Level2)e;
                    var aa = (Level2)a;

                    Assert.Equal(ee.Id, aa.Id);
                    Assert.Equal(ee.Name, aa.Name);
                    Assert.Equal(ee.Date, aa.Date);
                    Assert.Equal(ee.Level1_Optional_Id, aa.Level1_Optional_Id);
                    Assert.Equal(ee.Level1_Required_Id, aa.Level1_Required_Id);
                }
            }
        },
        {
            typeof(Level3), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (Level3)e;
                    var aa = (Level3)a;

                    Assert.Equal(ee.Id, aa.Id);
                    Assert.Equal(ee.Name, aa.Name);
                    Assert.Equal(ee.Level2_Optional_Id, aa.Level2_Optional_Id);
                    Assert.Equal(ee.Level2_Required_Id, aa.Level2_Required_Id);
                }
            }
        },
        {
            typeof(Level4), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (Level4)e;
                    var aa = (Level4)a;

                    Assert.Equal(ee.Id, aa.Id);
                    Assert.Equal(ee.Name, aa.Name);
                    Assert.Equal(ee.Level3_Optional_Id, aa.Level3_Optional_Id);
                    Assert.Equal(ee.Level3_Required_Id, aa.Level3_Required_Id);
                }
            }
        },
        {
            typeof(InheritanceBase1), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (InheritanceBase1)e;
                    var aa = (InheritanceBase1)a;

                    Assert.Equal(ee.Id, aa.Id);
                    Assert.Equal(ee.Name, aa.Name);
                }
            }
        },
        {
            typeof(InheritanceBase2), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (InheritanceBase2)e;
                    var aa = (InheritanceBase2)a;

                    Assert.Equal(ee.Id, aa.Id);
                    Assert.Equal(ee.Name, aa.Name);
                }
            }
        },
        {
            typeof(InheritanceDerived1), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (InheritanceDerived1)e;
                    var aa = (InheritanceDerived1)a;

                    Assert.Equal(ee.Id, aa.Id);
                    Assert.Equal(ee.Name, aa.Name);
                }
            }
        },
        {
            typeof(InheritanceDerived2), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (InheritanceDerived2)e;
                    var aa = (InheritanceDerived2)a;

                    Assert.Equal(ee.Id, aa.Id);
                    Assert.Equal(ee.Name, aa.Name);
                }
            }
        },
        {
            typeof(InheritanceLeaf1), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (InheritanceLeaf1)e;
                    var aa = (InheritanceLeaf1)a;

                    Assert.Equal(ee.Id, aa.Id);
                    Assert.Equal(ee.Name, aa.Name);
                }
            }
        },
        {
            typeof(InheritanceLeaf2), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (InheritanceLeaf2)e;
                    var aa = (InheritanceLeaf2)a;

                    Assert.Equal(ee.Id, aa.Id);
                    Assert.Equal(ee.Name, aa.Name);
                }
            }
        }
    }.ToDictionary(e => e.Key, e => (object)e.Value);

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        modelBuilder.Entity<Level1>().Property(e => e.Id).ValueGeneratedNever();
        modelBuilder.Entity<Level2>().Property(e => e.Id).ValueGeneratedNever();
        modelBuilder.Entity<Level3>().Property(e => e.Id).ValueGeneratedNever();
        modelBuilder.Entity<Level4>().Property(e => e.Id).ValueGeneratedNever();

        modelBuilder.Entity<Level1>().HasOne(e => e.OneToOne_Optional_Self1).WithOne();
        modelBuilder.Entity<Level1>().HasOne(e => e.OneToOne_Required_PK1).WithOne(e => e.OneToOne_Required_PK_Inverse2)
            .HasPrincipalKey<Level1>(e => e.Id).HasForeignKey<Level2>(e => e.Id).IsRequired().OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Level1>().HasOne(e => e.OneToOne_Optional_PK1).WithOne(e => e.OneToOne_Optional_PK_Inverse2)
            .HasPrincipalKey<Level1>(e => e.Id).IsRequired(false);
        modelBuilder.Entity<Level1>().HasOne(e => e.OneToOne_Required_FK1).WithOne(e => e.OneToOne_Required_FK_Inverse2)
            .HasForeignKey<Level2>(e => e.Level1_Required_Id).IsRequired().OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Level1>().HasOne(e => e.OneToOne_Optional_FK1).WithOne(e => e.OneToOne_Optional_FK_Inverse2)
            .HasForeignKey<Level2>(e => e.Level1_Optional_Id).IsRequired(false);
        modelBuilder.Entity<Level1>().HasMany(e => e.OneToMany_Required1).WithOne(e => e.OneToMany_Required_Inverse2).IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Level1>().HasMany(e => e.OneToMany_Optional1).WithOne(e => e.OneToMany_Optional_Inverse2).IsRequired(false);
        modelBuilder.Entity<Level1>().HasMany(e => e.OneToMany_Required_Self1).WithOne(e => e.OneToMany_Required_Self_Inverse1)
            .IsRequired().OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Level1>().HasMany(e => e.OneToMany_Optional_Self1).WithOne(e => e.OneToMany_Optional_Self_Inverse1)
            .IsRequired(false);

        modelBuilder.Entity<Level2>().HasOne(e => e.OneToOne_Optional_Self2).WithOne();
        modelBuilder.Entity<Level2>().HasOne(e => e.OneToOne_Required_PK2).WithOne(e => e.OneToOne_Required_PK_Inverse3)
            .HasPrincipalKey<Level2>(e => e.Id).HasForeignKey<Level3>(e => e.Id).IsRequired().OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Level2>().HasOne(e => e.OneToOne_Optional_PK2).WithOne(e => e.OneToOne_Optional_PK_Inverse3)
            .HasPrincipalKey<Level2>(e => e.Id).IsRequired(false);
        modelBuilder.Entity<Level2>().HasOne(e => e.OneToOne_Required_FK2).WithOne(e => e.OneToOne_Required_FK_Inverse3)
            .HasForeignKey<Level3>(e => e.Level2_Required_Id).IsRequired().OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Level2>().HasOne(e => e.OneToOne_Optional_FK2).WithOne(e => e.OneToOne_Optional_FK_Inverse3)
            .HasForeignKey<Level3>(e => e.Level2_Optional_Id).IsRequired(false);
        modelBuilder.Entity<Level2>().HasMany(e => e.OneToMany_Required2).WithOne(e => e.OneToMany_Required_Inverse3).IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Level2>().HasMany(e => e.OneToMany_Optional2).WithOne(e => e.OneToMany_Optional_Inverse3).IsRequired(false);
        modelBuilder.Entity<Level2>().HasMany(e => e.OneToMany_Required_Self2).WithOne(e => e.OneToMany_Required_Self_Inverse2)
            .IsRequired().OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Level2>().HasMany(e => e.OneToMany_Optional_Self2).WithOne(e => e.OneToMany_Optional_Self_Inverse2)
            .IsRequired(false);

        modelBuilder.Entity<Level3>().HasOne(e => e.OneToOne_Optional_Self3).WithOne();
        modelBuilder.Entity<Level3>().HasOne(e => e.OneToOne_Required_PK3).WithOne(e => e.OneToOne_Required_PK_Inverse4)
            .HasPrincipalKey<Level3>(e => e.Id).HasForeignKey<Level4>(e => e.Id).IsRequired().OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Level3>().HasOne(e => e.OneToOne_Optional_PK3).WithOne(e => e.OneToOne_Optional_PK_Inverse4)
            .HasPrincipalKey<Level3>(e => e.Id).IsRequired(false);
        modelBuilder.Entity<Level3>().HasOne(e => e.OneToOne_Required_FK3).WithOne(e => e.OneToOne_Required_FK_Inverse4)
            .HasForeignKey<Level4>(e => e.Level3_Required_Id).IsRequired().OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Level3>().HasOne(e => e.OneToOne_Optional_FK3).WithOne(e => e.OneToOne_Optional_FK_Inverse4)
            .HasForeignKey<Level4>(e => e.Level3_Optional_Id).IsRequired(false);
        modelBuilder.Entity<Level3>().HasMany(e => e.OneToMany_Required3).WithOne(e => e.OneToMany_Required_Inverse4).IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Level3>().HasMany(e => e.OneToMany_Optional3).WithOne(e => e.OneToMany_Optional_Inverse4).IsRequired(false);
        modelBuilder.Entity<Level3>().HasMany(e => e.OneToMany_Required_Self3).WithOne(e => e.OneToMany_Required_Self_Inverse3)
            .IsRequired().OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Level3>().HasMany(e => e.OneToMany_Optional_Self3).WithOne(e => e.OneToMany_Optional_Self_Inverse3)
            .IsRequired(false);

        modelBuilder.Entity<Level4>().HasOne(e => e.OneToOne_Optional_Self4).WithOne();
        modelBuilder.Entity<Level4>().HasMany(e => e.OneToMany_Required_Self4).WithOne(e => e.OneToMany_Required_Self_Inverse4)
            .IsRequired().OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Level4>().HasMany(e => e.OneToMany_Optional_Self4).WithOne(e => e.OneToMany_Optional_Self_Inverse4)
            .IsRequired(false);

        modelBuilder.Entity<InheritanceBase1>().Property(e => e.Id).ValueGeneratedNever();
        modelBuilder.Entity<InheritanceBase2>().Property(e => e.Id).ValueGeneratedNever();
        modelBuilder.Entity<InheritanceLeaf1>().Property(e => e.Id).ValueGeneratedNever();
        modelBuilder.Entity<InheritanceLeaf2>().Property(e => e.Id).ValueGeneratedNever();

        // FK name needs to be explicitly provided because issue #9310
        modelBuilder.Entity<InheritanceBase2>().HasOne(e => e.Reference).WithOne()
            .HasForeignKey<InheritanceBase1>("InheritanceBase2Id")
            .IsRequired(false);
        modelBuilder.Entity<InheritanceBase2>().HasMany(e => e.Collection).WithOne()
            .HasForeignKey("InheritanceBase2Id1");

        modelBuilder.Entity<InheritanceDerived1>().HasBaseType<InheritanceBase1>();
        modelBuilder.Entity<InheritanceDerived1>().HasOne(e => e.ReferenceSameType).WithOne()
            .HasForeignKey<InheritanceLeaf1>("SameTypeReference_InheritanceDerived1Id").IsRequired(false);
        modelBuilder.Entity<InheritanceDerived1>().HasOne(e => e.ReferenceDifferentType).WithOne()
            .HasForeignKey<InheritanceLeaf1>("DifferentTypeReference_InheritanceDerived1Id").IsRequired(false);
        modelBuilder.Entity<InheritanceDerived1>().HasMany(e => e.CollectionSameType).WithOne()
            .HasForeignKey("InheritanceDerived1Id1")
            .IsRequired(false);
        modelBuilder.Entity<InheritanceDerived1>().HasMany(e => e.CollectionDifferentType).WithOne()
            .HasForeignKey("InheritanceDerived1Id").IsRequired(false);

        modelBuilder.Entity<InheritanceDerived2>().HasBaseType<InheritanceBase1>();
        modelBuilder.Entity<InheritanceDerived2>().HasOne(e => e.ReferenceSameType).WithOne()
            .HasForeignKey<InheritanceLeaf1>("SameTypeReference_InheritanceDerived2Id").IsRequired(false);
        modelBuilder.Entity<InheritanceDerived2>().HasOne(e => e.ReferenceDifferentType).WithOne()
            .HasForeignKey<InheritanceLeaf2>("DifferentTypeReference_InheritanceDerived2Id").IsRequired(false);
        modelBuilder.Entity<InheritanceDerived2>().HasMany(e => e.CollectionSameType).WithOne().IsRequired(false);
        modelBuilder.Entity<InheritanceDerived2>().HasMany(e => e.CollectionDifferentType).WithOne().IsRequired(false);

        modelBuilder.Entity<InheritanceLeaf2>().HasMany(e => e.BaseCollection).WithOne().IsRequired(false);

        modelBuilder.Entity<ComplexNavigationField>().HasKey(e => e.Name);
        modelBuilder.Entity<ComplexNavigationString>().HasKey(e => e.DefaultText);
        modelBuilder.Entity<ComplexNavigationGlobalization>().HasKey(e => e.Text);
        modelBuilder.Entity<ComplexNavigationLanguage>().HasKey(e => e.Name);

        modelBuilder.Entity<ComplexNavigationField>().HasOne(f => f.Label);
        modelBuilder.Entity<ComplexNavigationField>().HasOne(f => f.Placeholder);

        modelBuilder.Entity<ComplexNavigationString>().HasMany(m => m.Globalizations);

        modelBuilder.Entity<ComplexNavigationGlobalization>().HasOne(g => g.Language);
    }

    protected override Task SeedAsync(ComplexNavigationsContext context)
        => ComplexNavigationsData.SeedAsync(context);

    public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        => base.AddOptions(builder).ConfigureWarnings(
            c => c
                .Log(CoreEventId.PossibleUnintendedCollectionNavigationNullComparisonWarning)
                .Log(CoreEventId.RowLimitingOperationWithoutOrderByWarning));

    public override ComplexNavigationsContext CreateContext()
    {
        var context = base.CreateContext();
        context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        return context;
    }

    protected class ComplexNavigationsDefaultData : ComplexNavigationsData
    {
        public static readonly ComplexNavigationsDefaultData Instance = new();

        private ComplexNavigationsDefaultData()
        {
        }

        public override IQueryable<TEntity> Set<TEntity>()
        {
            if (typeof(TEntity) == typeof(Level1))
            {
                return (IQueryable<TEntity>)LevelOnes.AsQueryable();
            }

            if (typeof(TEntity) == typeof(Level2))
            {
                return (IQueryable<TEntity>)LevelTwos.AsQueryable();
            }

            if (typeof(TEntity) == typeof(Level3))
            {
                return (IQueryable<TEntity>)LevelThrees.AsQueryable();
            }

            if (typeof(TEntity) == typeof(Level4))
            {
                return (IQueryable<TEntity>)LevelFours.AsQueryable();
            }

            if (typeof(TEntity) == typeof(InheritanceBase1))
            {
                return (IQueryable<TEntity>)InheritanceBaseOnes.AsQueryable();
            }

            if (typeof(TEntity) == typeof(InheritanceBase2))
            {
                return (IQueryable<TEntity>)InheritanceBaseTwos.AsQueryable();
            }

            if (typeof(TEntity) == typeof(InheritanceLeaf1))
            {
                return (IQueryable<TEntity>)InheritanceLeafOnes.AsQueryable();
            }

            if (typeof(TEntity) == typeof(InheritanceLeaf2))
            {
                return (IQueryable<TEntity>)InheritanceLeafTwos.AsQueryable();
            }

            throw new InvalidOperationException("Invalid entity type: " + typeof(TEntity));
        }
    }
}
