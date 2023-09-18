// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.AdventureWorks;

public class Address
{
    public Address()
    {
        BusinessEntityAddress = new HashSet<BusinessEntityAddress>();
        SalesOrderHeader = new HashSet<SalesOrderHeader>();
        SalesOrderHeaderNavigation = new HashSet<SalesOrderHeader>();
    }

    public int AddressID { get; set; }
    public string AddressLine1 { get; set; }
    public string AddressLine2 { get; set; }
    public string City { get; set; }
    public DateTime ModifiedDate { get; set; }
    public string PostalCode { get; set; }
#pragma warning disable IDE1006 // Naming Styles
    public Guid rowguid { get; set; }
#pragma warning restore IDE1006 // Naming Styles
    public int StateProvinceID { get; set; }

    public virtual ICollection<BusinessEntityAddress> BusinessEntityAddress { get; set; }
    public virtual ICollection<SalesOrderHeader> SalesOrderHeader { get; set; }
    public virtual ICollection<SalesOrderHeader> SalesOrderHeaderNavigation { get; set; }
    public virtual StateProvince StateProvince { get; set; }
}
