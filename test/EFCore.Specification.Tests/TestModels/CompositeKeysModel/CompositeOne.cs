// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.TestModels.CompositeKeysModel
{
    public class CompositeOne
    {
        public string Id1 { get; set; }
        public int Id2 { get; set; }

        public string Name { get; set; }
        public DateTime Date { get; set; }

        public CompositeTwo OneToOne_Required_PK1 { get; set; }
        public CompositeTwo OneToOne_Optional_PK1 { get; set; }

        public CompositeTwo OneToOne_Required_FK1 { get; set; }
        public CompositeTwo OneToOne_Optional_FK1 { get; set; }

        public ICollection<CompositeTwo> OneToMany_Required1 { get; set; }
        public ICollection<CompositeTwo> OneToMany_Optional1 { get; set; }

        public CompositeOne OneToOne_Optional_Self1 { get; set; }

        public ICollection<CompositeOne> OneToMany_Required_Self1 { get; set; }
        public ICollection<CompositeOne> OneToMany_Optional_Self1 { get; set; }
        public CompositeOne OneToMany_Required_Self_Inverse1 { get; set; }
        public CompositeOne OneToMany_Optional_Self_Inverse1 { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            return ReferenceEquals(this, obj) ? true : obj.GetType() == GetType() && Equals((CompositeOne)obj);
        }

        private bool Equals(CompositeOne other)
        {
            return Id1 == other.Id1 && string.Equals(Id2, other.Id2) && string.Equals(Name, other.Name) && Date.Equals(other.Date);
        }

        public override int GetHashCode()
            => HashCode.Combine(Id1, Id2, Name, Date);
    }
}

