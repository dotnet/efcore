// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.FunctionalTests.TestModels.ComplexNavigationsModel;

namespace Microsoft.Data.Entity.FunctionalTests
{
    public abstract class ComplexNavigationsQueryFixtureBase<TTestStore>
        where TTestStore : TestStore
    {
        public abstract TTestStore CreateTestStore();

        public abstract ComplexNavigationsContext CreateContext(TTestStore testStore);

        protected virtual void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Level1>().Key(e => e.Id);
            modelBuilder.Entity<Level2>().Key(e => e.Id);
            modelBuilder.Entity<Level3>().Key(e => e.Id);
            modelBuilder.Entity<Level4>().Key(e => e.Id);

            modelBuilder.Entity<Level1>().Property(e => e.Id).GenerateValueOnAdd(false);
            modelBuilder.Entity<Level2>().Property(e => e.Id).GenerateValueOnAdd(false);
            modelBuilder.Entity<Level1>().HasOne(e => e.OneToOne_Required_PK).WithOne(e => e.OneToOne_Required_PK_Inverse).ReferencedKey<Level1>(e => e.Id).Required(true);
            modelBuilder.Entity<Level1>().HasOne(e => e.OneToOne_Optional_PK).WithOne(e => e.OneToOne_Optional_PK_Inverse).ReferencedKey<Level1>(e => e.Id).Required(false);
            modelBuilder.Entity<Level1>().HasOne(e => e.OneToOne_Required_FK).WithOne(e => e.OneToOne_Required_FK_Inverse).ForeignKey<Level2>(e => e.Level1_Required_Id).Required(true);
            modelBuilder.Entity<Level1>().HasOne(e => e.OneToOne_Optional_FK).WithOne(e => e.OneToOne_Optional_FK_Inverse).ForeignKey<Level2>(e => e.Level1_Optional_Id).Required(false);
            modelBuilder.Entity<Level1>().HasMany(e => e.OneToMany_Required).WithOne(e => e.OneToMany_Required_Inverse).Required(true);
            modelBuilder.Entity<Level1>().HasMany(e => e.OneToMany_Optional).WithOne(e => e.OneToMany_Optional_Inverse).Required(false);
            modelBuilder.Entity<Level1>().HasMany(e => e.OneToMany_Required_Self).WithOne(e => e.OneToMany_Required_Self_Inverse).Required(true);
            modelBuilder.Entity<Level1>().HasMany(e => e.OneToMany_Optional_Self).WithOne(e => e.OneToMany_Optional_Self_Inverse).Required(false);

            // issue #1417
            //modelBuilder.Entity<Level1>().HasOne(e => e.OneToOne_Optional_Self).WithOne(); 

            modelBuilder.Entity<Level2>().Property(e => e.Id).GenerateValueOnAdd(false);
            modelBuilder.Entity<Level2>().Property(e => e.Id).GenerateValueOnAdd(false);
            modelBuilder.Entity<Level2>().HasOne(e => e.OneToOne_Required_PK).WithOne(e => e.OneToOne_Required_PK_Inverse).ReferencedKey<Level2>(e => e.Id).Required(true);
            modelBuilder.Entity<Level2>().HasOne(e => e.OneToOne_Optional_PK).WithOne(e => e.OneToOne_Optional_PK_Inverse).ReferencedKey<Level2>(e => e.Id).Required(false);
            modelBuilder.Entity<Level2>().HasOne(e => e.OneToOne_Required_FK).WithOne(e => e.OneToOne_Required_FK_Inverse).ForeignKey<Level3>(e => e.Level2_Required_Id).Required(true);
            modelBuilder.Entity<Level2>().HasOne(e => e.OneToOne_Optional_FK).WithOne(e => e.OneToOne_Optional_FK_Inverse).ForeignKey<Level3>(e => e.Level2_Optional_Id).Required(false);
            modelBuilder.Entity<Level2>().HasOne(e => e.OneToOne_Required_FK).WithOne(e => e.OneToOne_Required_FK_Inverse).ForeignKey<Level3>(e => e.Level2_Required_Id).Required(true);
            modelBuilder.Entity<Level2>().HasOne(e => e.OneToOne_Optional_FK).WithOne(e => e.OneToOne_Optional_FK_Inverse).ForeignKey<Level3>(e => e.Level2_Optional_Id).Required(false);
            modelBuilder.Entity<Level2>().HasMany(e => e.OneToMany_Required).WithOne(e => e.OneToMany_Required_Inverse).Required(true);
            modelBuilder.Entity<Level2>().HasMany(e => e.OneToMany_Optional).WithOne(e => e.OneToMany_Optional_Inverse).Required(false);
            modelBuilder.Entity<Level2>().HasMany(e => e.OneToMany_Required_Self).WithOne(e => e.OneToMany_Required_Self_Inverse).Required(true);
            modelBuilder.Entity<Level2>().HasMany(e => e.OneToMany_Optional_Self).WithOne(e => e.OneToMany_Optional_Self_Inverse).Required(false);

            // issue #1417
            //modelBuilder.Entity<Level2>().HasOne(e => e.OneToOne_Optional_Self).WithOne(); 

            modelBuilder.Entity<Level3>().Property(e => e.Id).GenerateValueOnAdd(false);
            modelBuilder.Entity<Level3>().Property(e => e.Id).GenerateValueOnAdd(false);
            modelBuilder.Entity<Level3>().HasOne(e => e.OneToOne_Required_PK).WithOne(e => e.OneToOne_Required_PK_Inverse).ReferencedKey<Level3>(e => e.Id).Required(true);
            modelBuilder.Entity<Level3>().HasOne(e => e.OneToOne_Optional_PK).WithOne(e => e.OneToOne_Optional_PK_Inverse).ReferencedKey<Level3>(e => e.Id).Required(false);
            modelBuilder.Entity<Level3>().HasOne(e => e.OneToOne_Required_FK).WithOne(e => e.OneToOne_Required_FK_Inverse).ForeignKey<Level4>(e => e.Level3_Required_Id).Required(true);
            modelBuilder.Entity<Level3>().HasOne(e => e.OneToOne_Optional_FK).WithOne(e => e.OneToOne_Optional_FK_Inverse).ForeignKey<Level4>(e => e.Level3_Optional_Id).Required(false);
            modelBuilder.Entity<Level3>().HasMany(e => e.OneToMany_Required).WithOne(e => e.OneToMany_Required_Inverse).Required(true);
            modelBuilder.Entity<Level3>().HasMany(e => e.OneToMany_Optional).WithOne(e => e.OneToMany_Optional_Inverse).Required(false);
            modelBuilder.Entity<Level3>().HasMany(e => e.OneToMany_Required_Self).WithOne(e => e.OneToMany_Required_Self_Inverse).Required(true);
            modelBuilder.Entity<Level3>().HasMany(e => e.OneToMany_Optional_Self).WithOne(e => e.OneToMany_Optional_Self_Inverse).Required(false);

            // issue #1417
            //modelBuilder.Entity<Level3>().HasOne(e => e.OneToOne_Optional_Self).WithOne();

            modelBuilder.Entity<Level4>().Property(e => e.Id).GenerateValueOnAdd(false);
            modelBuilder.Entity<Level4>().Property(e => e.Id).GenerateValueOnAdd(false);
            modelBuilder.Entity<Level4>().HasMany(e => e.OneToMany_Required_Self).WithOne(e => e.OneToMany_Required_Self_Inverse).Required(true);
            modelBuilder.Entity<Level4>().HasMany(e => e.OneToMany_Optional_Self).WithOne(e => e.OneToMany_Optional_Self_Inverse).Required(false);
        }
    }
}
