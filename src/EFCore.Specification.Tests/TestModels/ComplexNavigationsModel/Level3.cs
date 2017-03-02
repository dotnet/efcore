// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.ComplexNavigationsModel
{
    public class Level3
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public int Level2_Required_Id { get; set; }
        public int? Level2_Optional_Id { get; set; }

        public Level4 OneToOne_Required_PK { get; set; }
        public Level4 OneToOne_Optional_PK { get; set; }

        public Level4 OneToOne_Required_FK { get; set; }
        public Level4 OneToOne_Optional_FK { get; set; }

        public ICollection<Level4> OneToMany_Required { get; set; }
        public ICollection<Level4> OneToMany_Optional { get; set; }

        public Level2 OneToOne_Required_PK_Inverse { get; set; }
        public Level2 OneToOne_Optional_PK_Inverse { get; set; }
        public Level2 OneToOne_Required_FK_Inverse { get; set; }
        public Level2 OneToOne_Optional_FK_Inverse { get; set; }

        public Level2 OneToMany_Required_Inverse { get; set; }
        public Level2 OneToMany_Optional_Inverse { get; set; }

        public Level3 OneToOne_Optional_Self { get; set; }

        public ICollection<Level3> OneToMany_Required_Self { get; set; }
        public ICollection<Level3> OneToMany_Optional_Self { get; set; }
        public Level3 OneToMany_Required_Self_Inverse { get; set; }
        public Level3 OneToMany_Optional_Self_Inverse { get; set; }
    }
}
