// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.ComplexNavigationsModel;

namespace Microsoft.EntityFrameworkCore.Query;

public class ComplexNavigationsODataContext(DbContextOptions options) : PoolableDbContext(options)
{
    public DbSet<Level1> LevelOne { get; set; }
    public DbSet<Level2> LevelTwo { get; set; }
    public DbSet<Level3> LevelThree { get; set; }
    public DbSet<Level4> LevelFour { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
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
    }
}
