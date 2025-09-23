// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.TestModels.ComplexNavigationsModel;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class ComplexNavigationsSharedTypeQueryFixtureBase : ComplexNavigationsQueryFixtureBase, IQueryFixtureBase
{
    protected override string StoreName
        => "ComplexNavigationsOwned";

    public override ISetSource GetExpectedData()
        => ComplexNavigationsWeakData.Instance;

    Func<DbContext, ISetSource> IQueryFixtureBase.GetSetSourceCreator()
        => context => new ComplexNavigationsWeakSetExtractor(context);

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        modelBuilder.Ignore<Level2>();
        modelBuilder.Ignore<Level3>();
        modelBuilder.Ignore<Level4>();

        modelBuilder.Entity<Level1>().Property(e => e.Id).ValueGeneratedNever();

        var level1Builder = modelBuilder.Entity<Level1>()
            .Ignore(e => e.OneToOne_Optional_Self1)
            .Ignore(e => e.OneToMany_Required_Self1)
            .Ignore(e => e.OneToMany_Required_Self_Inverse1)
            .Ignore(e => e.OneToMany_Optional_Self1)
            .Ignore(e => e.OneToMany_Optional_Self_Inverse1);

        var level1 = level1Builder.Metadata;

        ForeignKey level2Fk;
        var level2 = level1.Model.AddEntityType("Level1.OneToOne_Required_PK1#Level2", typeof(Level2));
        using (var batch = ((Model)modelBuilder.Model).ConventionDispatcher.DelayConventions())
        {
            level2Fk = (ForeignKey)level2.AddForeignKey(level2.FindProperty(nameof(Level2.Id)), level1.FindPrimaryKey(), level1);
            level2Fk.IsUnique = true;
            level2Fk.SetPrincipalToDependent(nameof(Level1.OneToOne_Required_PK1), ConfigurationSource.Explicit);
            level2Fk.SetDependentToPrincipal(nameof(Level2.OneToOne_Required_PK_Inverse2), ConfigurationSource.Explicit);
            level2Fk.DeleteBehavior = DeleteBehavior.Restrict;
            level2Fk = (ForeignKey)batch.Run(level2Fk);
        }

        Configure(new OwnedNavigationBuilder<Level1, Level2>(level2Fk));

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
            .HasForeignKey("SameTypeCollection_InheritanceDerived1Id").IsRequired(false);
        modelBuilder.Entity<InheritanceDerived1>().HasMany(e => e.CollectionDifferentType).WithOne()
            .HasForeignKey("DifferentTypeCollection_InheritanceDerived1Id").IsRequired(false);

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

    protected virtual void Configure(OwnedNavigationBuilder<Level1, Level2> l2)
    {
        var level2 = l2.Ignore(e => e.OneToOne_Optional_Self2)
            .Ignore(e => e.OneToMany_Required_Self2)
            .Ignore(e => e.OneToMany_Required_Self_Inverse2)
            .Ignore(e => e.OneToMany_Optional_Self2)
            .Ignore(e => e.OneToMany_Optional_Self_Inverse2)
            .OwnedEntityType;

        l2.Property(e => e.Id).ValueGeneratedNever();

        l2.HasOne(e => e.OneToOne_Optional_PK_Inverse2)
            .WithOne(e => e.OneToOne_Optional_PK1)
            .HasPrincipalKey<Level1>(e => e.Id)
            .IsRequired(false);

        l2.HasOne(e => e.OneToOne_Required_FK_Inverse2)
            .WithOne(e => e.OneToOne_Required_FK1)
            .HasForeignKey<Level2>(e => e.Level1_Required_Id)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        l2.HasOne(e => e.OneToOne_Optional_FK_Inverse2)
            .WithOne(e => e.OneToOne_Optional_FK1)
            .HasForeignKey<Level2>(e => e.Level1_Optional_Id)
            .IsRequired(false);

        l2.HasOne(e => e.OneToMany_Required_Inverse2)
            .WithMany(e => e.OneToMany_Required1)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        l2.HasOne(e => e.OneToMany_Optional_Inverse2)
            .WithMany(e => e.OneToMany_Optional1)
            .IsRequired(false);

        ForeignKey level3Fk;
        var level3 = level2.Model.AddEntityType("Level1.OneToOne_Required_PK1#Level2.OneToOne_Required_PK2#Level3", typeof(Level3));
        using (var batch = ((Model)level2.Model).ConventionDispatcher.DelayConventions())
        {
            level3Fk = (ForeignKey)level3.AddForeignKey(level3.FindProperty(nameof(Level3.Id)), level2.FindPrimaryKey(), level2);
            level3Fk.IsUnique = true;
            level3Fk.SetPrincipalToDependent(nameof(Level2.OneToOne_Required_PK2), ConfigurationSource.Explicit);
            level3Fk.SetDependentToPrincipal(nameof(Level3.OneToOne_Required_PK_Inverse3), ConfigurationSource.Explicit);
            level3Fk.DeleteBehavior = DeleteBehavior.Restrict;
            level3Fk = (ForeignKey)batch.Run(level3Fk);
        }

        Configure(new OwnedNavigationBuilder<Level2, Level3>(level3Fk));
    }

    protected virtual void Configure(OwnedNavigationBuilder<Level2, Level3> l3)
    {
        var level3 = l3.Ignore(e => e.OneToOne_Optional_Self3)
            .Ignore(e => e.OneToMany_Required_Self3)
            .Ignore(e => e.OneToMany_Required_Self_Inverse3)
            .Ignore(e => e.OneToMany_Optional_Self3)
            .Ignore(e => e.OneToMany_Optional_Self_Inverse3)
            .OwnedEntityType;

        l3.Property(e => e.Id).ValueGeneratedNever();

        l3.HasOne(e => e.OneToOne_Optional_PK_Inverse3)
            .WithOne(e => e.OneToOne_Optional_PK2)
            .HasPrincipalKey<Level2>(e => e.Id)
            .IsRequired(false);

        l3.HasOne(e => e.OneToOne_Required_FK_Inverse3)
            .WithOne(e => e.OneToOne_Required_FK2)
            .HasForeignKey<Level3>(e => e.Level2_Required_Id)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        l3.HasOne(e => e.OneToOne_Optional_FK_Inverse3)
            .WithOne(e => e.OneToOne_Optional_FK2)
            .HasForeignKey<Level3>(e => e.Level2_Optional_Id)
            .IsRequired(false);

        l3.HasOne(e => e.OneToMany_Required_Inverse3)
            .WithMany(e => e.OneToMany_Required2)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        l3.HasOne(e => e.OneToMany_Optional_Inverse3)
            .WithMany(e => e.OneToMany_Optional2)
            .IsRequired(false);

        ForeignKey level4Fk;
        var level4 = level3.Model.AddEntityType(
            "Level1.OneToOne_Required_PK1#Level2.OneToOne_Required_PK2#Level3.OneToOne_Required_PK3#Level4",
            typeof(Level4));
        using (var batch = ((Model)level3.Model).ConventionDispatcher.DelayConventions())
        {
            level4Fk = (ForeignKey)level4.AddForeignKey(level4.FindProperty(nameof(Level4.Id)), level3.FindPrimaryKey(), level3);
            level4Fk.IsUnique = true;
            level4Fk.SetPrincipalToDependent(nameof(Level3.OneToOne_Required_PK3), ConfigurationSource.Explicit);
            level4Fk.SetDependentToPrincipal(nameof(Level4.OneToOne_Required_PK_Inverse4), ConfigurationSource.Explicit);
            level4Fk.DeleteBehavior = DeleteBehavior.Restrict;
            level4Fk = (ForeignKey)batch.Run(level4Fk);
        }

        Configure(new OwnedNavigationBuilder<Level3, Level4>(level4Fk));
    }

    protected virtual void Configure(OwnedNavigationBuilder<Level3, Level4> l4)
    {
        l4.Ignore(e => e.OneToOne_Optional_Self4)
            .Ignore(e => e.OneToMany_Required_Self4)
            .Ignore(e => e.OneToMany_Required_Self_Inverse4)
            .Ignore(e => e.OneToMany_Optional_Self4)
            .Ignore(e => e.OneToMany_Optional_Self_Inverse4);

        l4.Property(e => e.Id).ValueGeneratedNever();

        l4.HasOne(e => e.OneToOne_Optional_PK_Inverse4)
            .WithOne(e => e.OneToOne_Optional_PK3)
            .HasPrincipalKey<Level3>()
            .IsRequired(false);

        l4.HasOne(e => e.OneToOne_Required_FK_Inverse4)
            .WithOne(e => e.OneToOne_Required_FK3)
            .HasForeignKey<Level4>(e => e.Level3_Required_Id)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        l4.HasOne(e => e.OneToOne_Optional_FK_Inverse4)
            .WithOne(e => e.OneToOne_Optional_FK3)
            .HasForeignKey<Level4>(e => e.Level3_Optional_Id)
            .IsRequired(false);

        l4.HasOne(e => e.OneToMany_Required_Inverse4)
            .WithMany(e => e.OneToMany_Required3)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        l4.HasOne(e => e.OneToMany_Optional_Inverse4)
            .WithMany(e => e.OneToMany_Optional3)
            .IsRequired(false);
    }

    protected override Task SeedAsync(ComplexNavigationsContext context)
        => ComplexNavigationsData.SeedAsync(context, tableSplitting: true);

    private class ComplexNavigationsWeakSetExtractor(DbContext context) : ISetSource
    {
        private readonly DbContext _context = context;

        public IQueryable<TEntity> Set<TEntity>()
            where TEntity : class
        {
            if (typeof(TEntity) == typeof(Level1))
            {
                return (IQueryable<TEntity>)GetLevelOne(_context);
            }

            if (typeof(TEntity) == typeof(Level2))
            {
                return (IQueryable<TEntity>)GetLevelTwo(_context);
            }

            if (typeof(TEntity) == typeof(Level3))
            {
                return (IQueryable<TEntity>)GetLevelThree(_context);
            }

            return typeof(TEntity) == typeof(Level4) ? (IQueryable<TEntity>)GetLevelFour(_context) : _context.Set<TEntity>();
        }

        private static IQueryable<Level1> GetLevelOne(DbContext context)
            => context.Set<Level1>();

        private static IQueryable<Level2> GetLevelTwo(DbContext context)
            => GetLevelOne(context).Select(t => t.OneToOne_Required_PK1).Where(t => t != null);

        private static IQueryable<Level3> GetLevelThree(DbContext context)
            => GetLevelTwo(context).Select(t => t.OneToOne_Required_PK2).Where(t => t != null);

        private static IQueryable<Level4> GetLevelFour(DbContext context)
            => GetLevelThree(context).Select(t => t.OneToOne_Required_PK3).Where(t => t != null);
    }

    private class ComplexNavigationsWeakData : ComplexNavigationsData
    {
        public static readonly ComplexNavigationsWeakData Instance = new();

        private ComplexNavigationsWeakData()
        {
        }

        public override IQueryable<TEntity> Set<TEntity>()
        {
            if (typeof(TEntity) == typeof(Level1))
            {
                return (IQueryable<TEntity>)GetExpectedLevelOne();
            }

            if (typeof(TEntity) == typeof(Level2))
            {
                return (IQueryable<TEntity>)GetExpectedLevelTwo();
            }

            if (typeof(TEntity) == typeof(Level3))
            {
                return (IQueryable<TEntity>)GetExpectedLevelThree();
            }

            if (typeof(TEntity) == typeof(Level4))
            {
                return (IQueryable<TEntity>)GetExpectedLevelFour();
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

        private IQueryable<Level1> GetExpectedLevelOne()
            => SplitLevelOnes.AsQueryable();

        private IQueryable<Level2> GetExpectedLevelTwo()
            => GetExpectedLevelOne().Select(t => t.OneToOne_Required_PK1).Where(t => t != null);

        private IQueryable<Level3> GetExpectedLevelThree()
            => GetExpectedLevelTwo().Select(t => t.OneToOne_Required_PK2).Where(t => t != null);

        private IQueryable<Level4> GetExpectedLevelFour()
            => GetExpectedLevelThree().Select(t => t.OneToOne_Required_PK3).Where(t => t != null);
    }
}
