// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.AdventureWorks;

public class BusinessEntityAddress
{
    public int BusinessEntityID { get; set; }
    public int AddressID { get; set; }
    public int AddressTypeID { get; set; }
    public DateTime ModifiedDate { get; set; }
#pragma warning disable IDE1006 // Naming Styles
    public Guid rowguid { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    public virtual Address Address { get; set; }
    public virtual AddressType AddressType { get; set; }
    public virtual BusinessEntity BusinessEntity { get; set; }
}
