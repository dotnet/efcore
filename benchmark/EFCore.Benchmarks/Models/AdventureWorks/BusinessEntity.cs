// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.AdventureWorks;

public class BusinessEntity
{
    public BusinessEntity()
    {
        BusinessEntityAddress = new HashSet<BusinessEntityAddress>();
        BusinessEntityContact = new HashSet<BusinessEntityContact>();
    }

    public int BusinessEntityID { get; set; }
    public DateTime ModifiedDate { get; set; }
#pragma warning disable IDE1006 // Naming Styles
    public Guid rowguid { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    public virtual ICollection<BusinessEntityAddress> BusinessEntityAddress { get; set; }
    public virtual ICollection<BusinessEntityContact> BusinessEntityContact { get; set; }
    public virtual Person Person { get; set; }
    public virtual Store Store { get; set; }
    public virtual Vendor Vendor { get; set; }
}
