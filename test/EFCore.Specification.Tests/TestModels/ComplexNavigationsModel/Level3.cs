// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

// ReSharper disable NonReadonlyMemberInGetHashCode

namespace Microsoft.EntityFrameworkCore.TestModels.ComplexNavigationsModel
{
    public class Level3
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public int Level2_Required_Id { get; set; }
        public int? Level2_Optional_Id { get; set; }

        public Level4 OneToOne_Required_PK3 { get; set; }
        public Level4 OneToOne_Optional_PK3 { get; set; }

        public Level4 OneToOne_Required_FK3 { get; set; }
        public Level4 OneToOne_Optional_FK3 { get; set; }

        public ICollection<Level4> OneToMany_Required3 { get; set; }
        public ICollection<Level4> OneToMany_Optional3 { get; set; }

        public Level2 OneToOne_Required_PK_Inverse3 { get; set; }
        public Level2 OneToOne_Optional_PK_Inverse3 { get; set; }
        public Level2 OneToOne_Required_FK_Inverse3 { get; set; }
        public Level2 OneToOne_Optional_FK_Inverse3 { get; set; }

        public Level2 OneToMany_Required_Inverse3 { get; set; }
        public Level2 OneToMany_Optional_Inverse3 { get; set; }

        public Level3 OneToOne_Optional_Self3 { get; set; }

        public ICollection<Level3> OneToMany_Required_Self3 { get; set; }
        public ICollection<Level3> OneToMany_Optional_Self3 { get; set; }
        public Level3 OneToMany_Required_Self_Inverse3 { get; set; }
        public Level3 OneToMany_Optional_Self_Inverse3 { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            return ReferenceEquals(this, obj) ? true : obj.GetType() == GetType() && Equals((Level3)obj);
        }

        protected bool Equals(Level3 other)
        {
            return Id == other.Id && string.Equals(Name, other.Name) && Level2_Required_Id == other.Level2_Required_Id
                   && Level2_Optional_Id == other.Level2_Optional_Id;
        }

        public override int GetHashCode() => HashCode.Combine(Id, Name, Level2_Required_Id, Level2_Optional_Id);
    }
}
