// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.CompositeKeysModel;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class CompositeKeysQueryFixtureBase : SharedStoreFixtureBase<CompositeKeysContext>, IQueryFixtureBase
{
    protected override string StoreName
        => "CompositeKeys";

    public Func<DbContext> GetContextCreator()
        => () => CreateContext();

    public virtual ISetSource GetExpectedData()
        => CompositeKeysDefaultData.Instance;

    public virtual Dictionary<(Type, string), Func<object, object>> GetShadowPropertyMappings()
    {
        var l1s = GetExpectedData().Set<CompositeOne>().ToList();
        var l2s = GetExpectedData().Set<CompositeTwo>().ToList();
        var l3s = GetExpectedData().Set<CompositeThree>().ToList();
        var l4s = GetExpectedData().Set<CompositeFour>().ToList();

        return new Dictionary<(Type, string), Func<object, object>>
        {
            {
                (typeof(CompositeOne), "OneToOne_Optional_Self1Id1"),
                e => l1s.SingleOrDefault(l => l.Id1 == ((CompositeOne)e)?.Id1)?.OneToOne_Optional_Self1?.Id1
            },
            {
                (typeof(CompositeOne), "OneToOne_Optional_Self1Id2"),
                e => l1s.SingleOrDefault(l => l.Id2 == ((CompositeOne)e)?.Id2)?.OneToOne_Optional_Self1?.Id2
            },
            {
                (typeof(CompositeOne), "OneToMany_Required_Self_Inverse1Id1"),
                e => l1s.SingleOrDefault(l => l.Id1 == ((CompositeOne)e)?.Id1)?.OneToMany_Required_Self_Inverse1?.Id1
            },
            {
                (typeof(CompositeOne), "OneToMany_Required_Self_Inverse1Id2"),
                e => l1s.SingleOrDefault(l => l.Id2 == ((CompositeOne)e)?.Id2)?.OneToMany_Required_Self_Inverse1?.Id2
            },
            {
                (typeof(CompositeOne), "OneToMany_Optional_Self_Inverse1Id1"),
                e => l1s.SingleOrDefault(l => l.Id1 == ((CompositeOne)e)?.Id1)?.OneToMany_Optional_Self_Inverse1?.Id1
            },
            {
                (typeof(CompositeOne), "OneToMany_Optional_Self_Inverse1Id2"),
                e => l1s.SingleOrDefault(l => l.Id2 == ((CompositeOne)e)?.Id2)?.OneToMany_Optional_Self_Inverse1?.Id2
            },
            {
                (typeof(CompositeTwo), "OneToOne_Optional_PK_Inverse2Id1"),
                e => l2s.SingleOrDefault(l => l.Id1 == ((CompositeTwo)e)?.Id1)?.OneToOne_Optional_PK_Inverse2?.Id1
            },
            {
                (typeof(CompositeTwo), "OneToOne_Optional_PK_Inverse2Id2"),
                e => l2s.SingleOrDefault(l => l.Id2 == ((CompositeTwo)e)?.Id2)?.OneToOne_Optional_PK_Inverse2?.Id2
            },
            {
                (typeof(CompositeTwo), "OneToMany_Required_Inverse2Id1"),
                e => l2s.SingleOrDefault(l => l.Id1 == ((CompositeTwo)e)?.Id1)?.OneToMany_Required_Inverse2?.Id1
            },
            {
                (typeof(CompositeTwo), "OneToMany_Required_Inverse2Id2"),
                e => l2s.SingleOrDefault(l => l.Id2 == ((CompositeTwo)e)?.Id2)?.OneToMany_Required_Inverse2?.Id2
            },
            {
                (typeof(CompositeTwo), "OneToMany_Optional_Inverse2Id1"),
                e => l2s.SingleOrDefault(l => l.Id1 == ((CompositeTwo)e)?.Id1)?.OneToMany_Optional_Inverse2?.Id1
            },
            {
                (typeof(CompositeTwo), "OneToMany_Optional_Inverse2Id2"),
                e => l2s.SingleOrDefault(l => l.Id2 == ((CompositeTwo)e)?.Id2)?.OneToMany_Optional_Inverse2?.Id2
            },
            {
                (typeof(CompositeTwo), "OneToOne_Optional_Self2Id1"),
                e => l2s.SingleOrDefault(l => l.Id1 == ((CompositeTwo)e)?.Id1)?.OneToOne_Optional_Self2?.Id1
            },
            {
                (typeof(CompositeTwo), "OneToOne_Optional_Self2Id2"),
                e => l2s.SingleOrDefault(l => l.Id2 == ((CompositeTwo)e)?.Id2)?.OneToOne_Optional_Self2?.Id2
            },
            {
                (typeof(CompositeTwo), "OneToMany_Required_Self_Inverse2Id1"),
                e => l2s.SingleOrDefault(l => l.Id1 == ((CompositeTwo)e)?.Id1)?.OneToMany_Required_Self_Inverse2?.Id1
            },
            {
                (typeof(CompositeTwo), "OneToMany_Required_Self_Inverse2Id2"),
                e => l2s.SingleOrDefault(l => l.Id2 == ((CompositeTwo)e)?.Id2)?.OneToMany_Required_Self_Inverse2?.Id2
            },
            {
                (typeof(CompositeTwo), "OneToMany_Optional_Self_Inverse2Id1"),
                e => l2s.SingleOrDefault(l => l.Id1 == ((CompositeTwo)e)?.Id1)?.OneToMany_Optional_Self_Inverse2?.Id1
            },
            {
                (typeof(CompositeTwo), "OneToMany_Optional_Self_Inverse2Id2"),
                e => l2s.SingleOrDefault(l => l.Id2 == ((CompositeTwo)e)?.Id2)?.OneToMany_Optional_Self_Inverse2?.Id2
            },
            {
                (typeof(CompositeThree), "OneToOne_Optional_PK_Inverse3Id1"),
                e => l3s.SingleOrDefault(l => l.Id1 == ((CompositeThree)e)?.Id1)?.OneToOne_Optional_PK_Inverse3?.Id1
            },
            {
                (typeof(CompositeThree), "OneToOne_Optional_PK_Inverse3Id2"),
                e => l3s.SingleOrDefault(l => l.Id2 == ((CompositeThree)e)?.Id2)?.OneToOne_Optional_PK_Inverse3?.Id2
            },
            {
                (typeof(CompositeThree), "OneToMany_Required_Inverse3Id1"),
                e => l3s.SingleOrDefault(l => l.Id1 == ((CompositeThree)e)?.Id1)?.OneToMany_Required_Inverse3?.Id1
            },
            {
                (typeof(CompositeThree), "OneToMany_Required_Inverse3Id2"),
                e => l3s.SingleOrDefault(l => l.Id2 == ((CompositeThree)e)?.Id2)?.OneToMany_Required_Inverse3?.Id2
            },
            {
                (typeof(CompositeThree), "OneToMany_Optional_Inverse3Id1"),
                e => l3s.SingleOrDefault(l => l.Id1 == ((CompositeThree)e)?.Id1)?.OneToMany_Optional_Inverse3?.Id1
            },
            {
                (typeof(CompositeThree), "OneToMany_Optional_Inverse3Id2"),
                e => l3s.SingleOrDefault(l => l.Id2 == ((CompositeThree)e)?.Id2)?.OneToMany_Optional_Inverse3?.Id2
            },
            {
                (typeof(CompositeThree), "OneToOne_Optional_Self3Id1"),
                e => l3s.SingleOrDefault(l => l.Id1 == ((CompositeThree)e)?.Id1)?.OneToOne_Optional_Self3?.Id1
            },
            {
                (typeof(CompositeThree), "OneToOne_Optional_Self3Id2"),
                e => l3s.SingleOrDefault(l => l.Id2 == ((CompositeThree)e)?.Id2)?.OneToOne_Optional_Self3?.Id2
            },
            {
                (typeof(CompositeThree), "OneToMany_Required_Self_Inverse3Id1"),
                e => l3s.SingleOrDefault(l => l.Id1 == ((CompositeThree)e)?.Id1)?.OneToMany_Required_Self_Inverse3?.Id1
            },
            {
                (typeof(CompositeThree), "OneToMany_Required_Self_Inverse3Id2"),
                e => l3s.SingleOrDefault(l => l.Id2 == ((CompositeThree)e)?.Id2)?.OneToMany_Required_Self_Inverse3?.Id2
            },
            {
                (typeof(CompositeThree), "OneToMany_Optional_Self_Inverse3Id1"),
                e => l3s.SingleOrDefault(l => l.Id1 == ((CompositeThree)e)?.Id1)?.OneToMany_Optional_Self_Inverse3?.Id1
            },
            {
                (typeof(CompositeThree), "OneToMany_Optional_Self_Inverse3Id2"),
                e => l3s.SingleOrDefault(l => l.Id2 == ((CompositeThree)e)?.Id2)?.OneToMany_Optional_Self_Inverse3?.Id2
            },
            {
                (typeof(CompositeFour), "OneToOne_Optional_PK_Inverse4Id1"),
                e => l4s.SingleOrDefault(l => l.Id1 == ((CompositeFour)e)?.Id1)?.OneToOne_Optional_PK_Inverse4?.Id1
            },
            {
                (typeof(CompositeFour), "OneToOne_Optional_PK_Inverse4Id2"),
                e => l4s.SingleOrDefault(l => l.Id2 == ((CompositeFour)e)?.Id2)?.OneToOne_Optional_PK_Inverse4?.Id2
            },
            {
                (typeof(CompositeFour), "OneToMany_Required_Inverse4Id1"),
                e => l4s.SingleOrDefault(l => l.Id1 == ((CompositeFour)e)?.Id1)?.OneToMany_Required_Inverse4?.Id1
            },
            {
                (typeof(CompositeFour), "OneToMany_Required_Inverse4Id2"),
                e => l4s.SingleOrDefault(l => l.Id2 == ((CompositeFour)e)?.Id2)?.OneToMany_Required_Inverse4?.Id2
            },
            {
                (typeof(CompositeFour), "OneToMany_Optional_Inverse4Id1"),
                e => l4s.SingleOrDefault(l => l.Id1 == ((CompositeFour)e)?.Id1)?.OneToMany_Optional_Inverse4?.Id1
            },
            {
                (typeof(CompositeFour), "OneToMany_Optional_Inverse4Id2"),
                e => l4s.SingleOrDefault(l => l.Id2 == ((CompositeFour)e)?.Id2)?.OneToMany_Optional_Inverse4?.Id2
            },
            {
                (typeof(CompositeFour), "OneToOne_Optional_Self4Id1"),
                e => l4s.SingleOrDefault(l => l.Id1 == ((CompositeFour)e)?.Id1)?.OneToOne_Optional_Self4?.Id1
            },
            {
                (typeof(CompositeFour), "OneToOne_Optional_Self4Id2"),
                e => l4s.SingleOrDefault(l => l.Id2 == ((CompositeFour)e)?.Id2)?.OneToOne_Optional_Self4?.Id2
            },
            {
                (typeof(CompositeFour), "OneToMany_Required_Self_Inverse4Id1"),
                e => l4s.SingleOrDefault(l => l.Id1 == ((CompositeFour)e)?.Id1)?.OneToMany_Required_Self_Inverse4?.Id1
            },
            {
                (typeof(CompositeFour), "OneToMany_Required_Self_Inverse4Id2"),
                e => l4s.SingleOrDefault(l => l.Id2 == ((CompositeFour)e)?.Id2)?.OneToMany_Required_Self_Inverse4?.Id2
            },
            {
                (typeof(CompositeFour), "OneToMany_Optional_Self_Inverse4Id1"),
                e => l4s.SingleOrDefault(l => l.Id1 == ((CompositeFour)e)?.Id1)?.OneToMany_Optional_Self_Inverse4?.Id1
            },
            {
                (typeof(CompositeFour), "OneToMany_Optional_Self_Inverse4Id2"),
                e => l4s.SingleOrDefault(l => l.Id2 == ((CompositeFour)e)?.Id2)?.OneToMany_Optional_Self_Inverse4?.Id2
            },
        };
    }

    public IReadOnlyDictionary<Type, object> EntitySorters { get; } = new Dictionary<Type, Func<object, object>>
    {
        { typeof(CompositeOne), e => (((CompositeOne)e)?.Id1, ((CompositeOne)e)?.Id2) },
        { typeof(CompositeTwo), e => (((CompositeTwo)e)?.Id1, ((CompositeTwo)e)?.Id2) },
        { typeof(CompositeThree), e => (((CompositeThree)e)?.Id1, ((CompositeThree)e)?.Id2) },
        { typeof(CompositeFour), e => (((CompositeFour)e)?.Id1, ((CompositeFour)e)?.Id2) },
    }.ToDictionary(e => e.Key, e => (object)e.Value);

    public IReadOnlyDictionary<Type, object> EntityAsserters { get; } = new Dictionary<Type, Action<object, object>>
    {
        {
            typeof(CompositeOne), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (CompositeOne)e;
                    var aa = (CompositeOne)a;

                    Assert.Equal(ee.Id1, aa.Id1);
                    Assert.Equal(ee.Id2, aa.Id2);
                    Assert.Equal(ee.Name, aa.Name);
                    Assert.Equal(ee.Date, aa.Date);
                }
            }
        },
        {
            typeof(CompositeTwo), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (CompositeTwo)e;
                    var aa = (CompositeTwo)a;

                    Assert.Equal(ee.Id1, aa.Id1);
                    Assert.Equal(ee.Id2, aa.Id2);
                    Assert.Equal(ee.Name, aa.Name);
                    Assert.Equal(ee.Date, aa.Date);
                    Assert.Equal(ee.Level1_Optional_Id1, aa.Level1_Optional_Id1);
                    Assert.Equal(ee.Level1_Optional_Id2, aa.Level1_Optional_Id2);
                    Assert.Equal(ee.Level1_Required_Id1, aa.Level1_Required_Id1);
                    Assert.Equal(ee.Level1_Required_Id2, aa.Level1_Required_Id2);
                }
            }
        },
        {
            typeof(CompositeThree), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (CompositeThree)e;
                    var aa = (CompositeThree)a;

                    Assert.Equal(ee.Id1, aa.Id1);
                    Assert.Equal(ee.Id2, aa.Id2);
                    Assert.Equal(ee.Name, aa.Name);
                    Assert.Equal(ee.Level2_Optional_Id1, aa.Level2_Optional_Id1);
                    Assert.Equal(ee.Level2_Optional_Id2, aa.Level2_Optional_Id2);
                    Assert.Equal(ee.Level2_Required_Id1, aa.Level2_Required_Id1);
                    Assert.Equal(ee.Level2_Required_Id2, aa.Level2_Required_Id2);
                }
            }
        },
        {
            typeof(CompositeFour), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (CompositeFour)e;
                    var aa = (CompositeFour)a;

                    Assert.Equal(ee.Id1, aa.Id1);
                    Assert.Equal(ee.Id2, aa.Id2);
                    Assert.Equal(ee.Name, aa.Name);
                    Assert.Equal(ee.Level3_Optional_Id1, aa.Level3_Optional_Id1);
                    Assert.Equal(ee.Level3_Optional_Id2, aa.Level3_Optional_Id2);
                    Assert.Equal(ee.Level3_Required_Id1, aa.Level3_Required_Id1);
                    Assert.Equal(ee.Level3_Required_Id2, aa.Level3_Required_Id2);
                }
            }
        },
    }.ToDictionary(e => e.Key, e => (object)e.Value);

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        modelBuilder.Entity<CompositeOne>().HasKey(x => new { x.Id1, x.Id2 });
        modelBuilder.Entity<CompositeTwo>().HasKey(x => new { x.Id1, x.Id2 });
        modelBuilder.Entity<CompositeThree>().HasKey(x => new { x.Id1, x.Id2 });
        modelBuilder.Entity<CompositeFour>().HasKey(x => new { x.Id1, x.Id2 });

        modelBuilder.Entity<CompositeOne>().HasOne(e => e.OneToOne_Optional_Self1).WithOne();
        modelBuilder.Entity<CompositeOne>().HasOne(e => e.OneToOne_Required_PK1).WithOne(e => e.OneToOne_Required_PK_Inverse2)
            .HasPrincipalKey<CompositeOne>(e => new { e.Id1, e.Id2 }).HasForeignKey<CompositeTwo>(e => new { e.Id1, e.Id2 })
            .IsRequired().OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<CompositeOne>().HasOne(e => e.OneToOne_Optional_PK1).WithOne(e => e.OneToOne_Optional_PK_Inverse2)
            .HasPrincipalKey<CompositeOne>(e => new { e.Id1, e.Id2 }).IsRequired(false);
        modelBuilder.Entity<CompositeOne>().HasOne(e => e.OneToOne_Required_FK1).WithOne(e => e.OneToOne_Required_FK_Inverse2)
            .HasForeignKey<CompositeTwo>(e => new { e.Level1_Required_Id1, e.Level1_Required_Id2 }).IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<CompositeOne>().HasOne(e => e.OneToOne_Optional_FK1).WithOne(e => e.OneToOne_Optional_FK_Inverse2)
            .HasForeignKey<CompositeTwo>(e => new { e.Level1_Optional_Id1, e.Level1_Optional_Id2 }).IsRequired(false);
        modelBuilder.Entity<CompositeOne>().HasMany(e => e.OneToMany_Required1).WithOne(e => e.OneToMany_Required_Inverse2).IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<CompositeOne>().HasMany(e => e.OneToMany_Optional1).WithOne(e => e.OneToMany_Optional_Inverse2)
            .IsRequired(false);
        modelBuilder.Entity<CompositeOne>().HasMany(e => e.OneToMany_Required_Self1).WithOne(e => e.OneToMany_Required_Self_Inverse1)
            .IsRequired().OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<CompositeOne>().HasMany(e => e.OneToMany_Optional_Self1).WithOne(e => e.OneToMany_Optional_Self_Inverse1)
            .IsRequired(false);

        modelBuilder.Entity<CompositeTwo>().HasOne(e => e.OneToOne_Optional_Self2).WithOne();
        modelBuilder.Entity<CompositeTwo>().HasOne(e => e.OneToOne_Required_PK2).WithOne(e => e.OneToOne_Required_PK_Inverse3)
            .HasPrincipalKey<CompositeTwo>(e => new { e.Id1, e.Id2 }).HasForeignKey<CompositeThree>(e => new { e.Id1, e.Id2 })
            .IsRequired().OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<CompositeTwo>().HasOne(e => e.OneToOne_Optional_PK2).WithOne(e => e.OneToOne_Optional_PK_Inverse3)
            .HasPrincipalKey<CompositeTwo>(e => new { e.Id1, e.Id2 }).IsRequired(false);
        modelBuilder.Entity<CompositeTwo>().HasOne(e => e.OneToOne_Required_FK2).WithOne(e => e.OneToOne_Required_FK_Inverse3)
            .HasForeignKey<CompositeThree>(e => new { e.Level2_Required_Id1, e.Level2_Required_Id2 }).IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<CompositeTwo>().HasOne(e => e.OneToOne_Optional_FK2).WithOne(e => e.OneToOne_Optional_FK_Inverse3)
            .HasForeignKey<CompositeThree>(e => new { e.Level2_Optional_Id1, e.Level2_Optional_Id2 }).IsRequired(false);
        modelBuilder.Entity<CompositeTwo>().HasMany(e => e.OneToMany_Required2).WithOne(e => e.OneToMany_Required_Inverse3).IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<CompositeTwo>().HasMany(e => e.OneToMany_Optional2).WithOne(e => e.OneToMany_Optional_Inverse3)
            .IsRequired(false);
        modelBuilder.Entity<CompositeTwo>().HasMany(e => e.OneToMany_Required_Self2).WithOne(e => e.OneToMany_Required_Self_Inverse2)
            .IsRequired().OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<CompositeTwo>().HasMany(e => e.OneToMany_Optional_Self2).WithOne(e => e.OneToMany_Optional_Self_Inverse2)
            .IsRequired(false);

        modelBuilder.Entity<CompositeThree>().HasOne(e => e.OneToOne_Optional_Self3).WithOne();
        modelBuilder.Entity<CompositeThree>().HasOne(e => e.OneToOne_Required_PK3).WithOne(e => e.OneToOne_Required_PK_Inverse4)
            .HasPrincipalKey<CompositeThree>(e => new { e.Id1, e.Id2 }).HasForeignKey<CompositeFour>(e => new { e.Id1, e.Id2 })
            .IsRequired().OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<CompositeThree>().HasOne(e => e.OneToOne_Optional_PK3).WithOne(e => e.OneToOne_Optional_PK_Inverse4)
            .HasPrincipalKey<CompositeThree>(e => new { e.Id1, e.Id2 }).IsRequired(false);
        modelBuilder.Entity<CompositeThree>().HasOne(e => e.OneToOne_Required_FK3).WithOne(e => e.OneToOne_Required_FK_Inverse4)
            .HasForeignKey<CompositeFour>(e => new { e.Level3_Required_Id1, e.Level3_Required_Id2 }).IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<CompositeThree>().HasOne(e => e.OneToOne_Optional_FK3).WithOne(e => e.OneToOne_Optional_FK_Inverse4)
            .HasForeignKey<CompositeFour>(e => new { e.Level3_Optional_Id1, e.Level3_Optional_Id2 }).IsRequired(false);
        modelBuilder.Entity<CompositeThree>().HasMany(e => e.OneToMany_Required3).WithOne(e => e.OneToMany_Required_Inverse4)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<CompositeThree>().HasMany(e => e.OneToMany_Optional3).WithOne(e => e.OneToMany_Optional_Inverse4)
            .IsRequired(false);
        modelBuilder.Entity<CompositeThree>().HasMany(e => e.OneToMany_Required_Self3).WithOne(e => e.OneToMany_Required_Self_Inverse3)
            .IsRequired().OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<CompositeThree>().HasMany(e => e.OneToMany_Optional_Self3).WithOne(e => e.OneToMany_Optional_Self_Inverse3)
            .IsRequired(false);

        modelBuilder.Entity<CompositeFour>().HasOne(e => e.OneToOne_Optional_Self4).WithOne();
        modelBuilder.Entity<CompositeFour>().HasMany(e => e.OneToMany_Required_Self4).WithOne(e => e.OneToMany_Required_Self_Inverse4)
            .IsRequired().OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<CompositeFour>().HasMany(e => e.OneToMany_Optional_Self4).WithOne(e => e.OneToMany_Optional_Self_Inverse4)
            .IsRequired(false);
    }

    protected override Task SeedAsync(CompositeKeysContext context)
        => CompositeKeysData.SeedAsync(context);

    public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        => base.AddOptions(builder).ConfigureWarnings(
            c => c
                .Log(CoreEventId.PossibleUnintendedCollectionNavigationNullComparisonWarning)
                .Log(CoreEventId.RowLimitingOperationWithoutOrderByWarning));

    public override CompositeKeysContext CreateContext()
    {
        var context = base.CreateContext();
        context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        return context;
    }

    protected class CompositeKeysDefaultData : CompositeKeysData
    {
        public static readonly CompositeKeysDefaultData Instance = new();

        private CompositeKeysDefaultData()
        {
        }

        public override IQueryable<TEntity> Set<TEntity>()
        {
            if (typeof(TEntity) == typeof(CompositeOne))
            {
                return (IQueryable<TEntity>)CompositeOnes.AsQueryable();
            }

            if (typeof(TEntity) == typeof(CompositeTwo))
            {
                return (IQueryable<TEntity>)CompositeTwos.AsQueryable();
            }

            if (typeof(TEntity) == typeof(CompositeThree))
            {
                return (IQueryable<TEntity>)CompositeThrees.AsQueryable();
            }

            if (typeof(TEntity) == typeof(CompositeFour))
            {
                return (IQueryable<TEntity>)CompositeFours.AsQueryable();
            }

            throw new InvalidOperationException("Invalid entity type: " + typeof(TEntity));
        }
    }
}
