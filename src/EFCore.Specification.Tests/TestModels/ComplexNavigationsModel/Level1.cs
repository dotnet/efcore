// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.ComplexNavigationsModel
{
    public class Level1
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }

        public Level2 OneToOne_Required_PK { get; set; }
        public Level2 OneToOne_Optional_PK { get; set; }

        public Level2 OneToOne_Required_FK { get; set; }
        public Level2 OneToOne_Optional_FK { get; set; }

        public ICollection<Level2> OneToMany_Required { get; set; }
        public ICollection<Level2> OneToMany_Optional { get; set; }

        public Level1 OneToOne_Optional_Self { get; set; }

        public ICollection<Level1> OneToMany_Required_Self { get; set; }
        public ICollection<Level1> OneToMany_Optional_Self { get; set; }
        public Level1 OneToMany_Required_Self_Inverse { get; set; }
        public Level1 OneToMany_Optional_Self_Inverse { get; set; }
    }
}
