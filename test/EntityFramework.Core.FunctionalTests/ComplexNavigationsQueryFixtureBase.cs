// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.FunctionalTests.TestModels.ComplexNavigationsModel;

namespace Microsoft.Data.Entity.FunctionalTests
{
    public abstract class ComplexNavigationsQueryFixtureBase<TTestStore>
        where TTestStore : TestStore
    {
        public abstract TTestStore CreateTestStore();

        public abstract ComplexNavigationsContext CreateContext(TTestStore testStore);

        protected virtual void OnModelCreating(ModelBuilder model)
        {
            model.Entity<Level1>().Key(e => e.Id);
            model.Entity<Level2>().Key(e => e.Id);
            model.Entity<Level3>().Key(e => e.Id);
            model.Entity<Level4>().Key(e => e.Id);

            model.Entity<Level1>().Reference(e => e.OneToOne_Required_PK).InverseReference(e => e.OneToOne_Required_PK_Inverse).PrincipalKey<Level1>(e => e.Id).Required(true);
            model.Entity<Level1>().Reference(e => e.OneToOne_Optional_PK).InverseReference(e => e.OneToOne_Optional_PK_Inverse).PrincipalKey<Level1>(e => e.Id).Required(false);
            model.Entity<Level1>().Reference(e => e.OneToOne_Required_FK).InverseReference(e => e.OneToOne_Required_FK_Inverse).ForeignKey<Level2>(e => e.Level1_Required_Id).Required(true);
            model.Entity<Level1>().Reference(e => e.OneToOne_Optional_FK).InverseReference(e => e.OneToOne_Optional_FK_Inverse).ForeignKey<Level2>(e => e.Level1_Optional_Id).Required(false);
            model.Entity<Level1>().Collection(e => e.OneToMany_Required).InverseReference(e => e.OneToMany_Required_Inverse).Required(true);
            model.Entity<Level1>().Collection(e => e.OneToMany_Optional).InverseReference(e => e.OneToMany_Optional_Inverse).Required(false);
            model.Entity<Level1>().Collection(e => e.OneToMany_Required_Self).InverseReference(e => e.OneToMany_Required_Self_Inverse).Required(true);
            model.Entity<Level1>().Collection(e => e.OneToMany_Optional_Self).InverseReference(e => e.OneToMany_Optional_Self_Inverse).Required(false);

            // issue #1417
            //modelBuilder.Entity<Level1>().Reference(e => e.OneToOne_Optional_Self).InverseReference(); 

            model.Entity<Level2>().Reference(e => e.OneToOne_Required_PK).InverseReference(e => e.OneToOne_Required_PK_Inverse).PrincipalKey<Level2>(e => e.Id).Required(true);
            model.Entity<Level2>().Reference(e => e.OneToOne_Optional_PK).InverseReference(e => e.OneToOne_Optional_PK_Inverse).PrincipalKey<Level2>(e => e.Id).Required(false);
            model.Entity<Level2>().Reference(e => e.OneToOne_Required_FK).InverseReference(e => e.OneToOne_Required_FK_Inverse).ForeignKey<Level3>(e => e.Level2_Required_Id).Required(true);
            model.Entity<Level2>().Reference(e => e.OneToOne_Optional_FK).InverseReference(e => e.OneToOne_Optional_FK_Inverse).ForeignKey<Level3>(e => e.Level2_Optional_Id).Required(false);
            model.Entity<Level2>().Reference(e => e.OneToOne_Required_FK).InverseReference(e => e.OneToOne_Required_FK_Inverse).ForeignKey<Level3>(e => e.Level2_Required_Id).Required(true);
            model.Entity<Level2>().Reference(e => e.OneToOne_Optional_FK).InverseReference(e => e.OneToOne_Optional_FK_Inverse).ForeignKey<Level3>(e => e.Level2_Optional_Id).Required(false);
            model.Entity<Level2>().Collection(e => e.OneToMany_Required).InverseReference(e => e.OneToMany_Required_Inverse).Required(true);
            model.Entity<Level2>().Collection(e => e.OneToMany_Optional).InverseReference(e => e.OneToMany_Optional_Inverse).Required(false);
            model.Entity<Level2>().Collection(e => e.OneToMany_Required_Self).InverseReference(e => e.OneToMany_Required_Self_Inverse).Required(true);
            model.Entity<Level2>().Collection(e => e.OneToMany_Optional_Self).InverseReference(e => e.OneToMany_Optional_Self_Inverse).Required(false);

            // issue #1417
            //modelBuilder.Entity<Level2>().Reference(e => e.OneToOne_Optional_Self).InverseReference(); 

            model.Entity<Level3>().Reference(e => e.OneToOne_Required_PK).InverseReference(e => e.OneToOne_Required_PK_Inverse).PrincipalKey<Level3>(e => e.Id).Required(true);
            model.Entity<Level3>().Reference(e => e.OneToOne_Optional_PK).InverseReference(e => e.OneToOne_Optional_PK_Inverse).PrincipalKey<Level3>(e => e.Id).Required(false);
            model.Entity<Level3>().Reference(e => e.OneToOne_Required_FK).InverseReference(e => e.OneToOne_Required_FK_Inverse).ForeignKey<Level4>(e => e.Level3_Required_Id).Required(true);
            model.Entity<Level3>().Reference(e => e.OneToOne_Optional_FK).InverseReference(e => e.OneToOne_Optional_FK_Inverse).ForeignKey<Level4>(e => e.Level3_Optional_Id).Required(false);
            model.Entity<Level3>().Collection(e => e.OneToMany_Required).InverseReference(e => e.OneToMany_Required_Inverse).Required(true);
            model.Entity<Level3>().Collection(e => e.OneToMany_Optional).InverseReference(e => e.OneToMany_Optional_Inverse).Required(false);
            model.Entity<Level3>().Collection(e => e.OneToMany_Required_Self).InverseReference(e => e.OneToMany_Required_Self_Inverse).Required(true);
            model.Entity<Level3>().Collection(e => e.OneToMany_Optional_Self).InverseReference(e => e.OneToMany_Optional_Self_Inverse).Required(false);

            // issue #1417
            //modelBuilder.Entity<Level3>().Reference(e => e.OneToOne_Optional_Self).InverseReference();

            model.Entity<Level4>().Collection(e => e.OneToMany_Required_Self).InverseReference(e => e.OneToMany_Required_Self_Inverse).Required(true);
            model.Entity<Level4>().Collection(e => e.OneToMany_Optional_Self).InverseReference(e => e.OneToMany_Optional_Self_Inverse).Required(false);
        }
    }
}
