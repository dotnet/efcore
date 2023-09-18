// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.AdventureWorks;

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
