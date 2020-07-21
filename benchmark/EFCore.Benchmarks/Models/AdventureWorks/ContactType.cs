// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.AdventureWorks
{
    public class ContactType
    {
        public ContactType()
        {
            BusinessEntityContact = new HashSet<BusinessEntityContact>();
        }

        public int ContactTypeID { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string Name { get; set; }

        public virtual ICollection<BusinessEntityContact> BusinessEntityContact { get; set; }
    }
}
