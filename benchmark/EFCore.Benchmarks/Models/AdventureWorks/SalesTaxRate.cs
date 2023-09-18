// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.AdventureWorks;

public class SalesTaxRate
{
    public int SalesTaxRateID { get; set; }
    public DateTime ModifiedDate { get; set; }
    public string Name { get; set; }
#pragma warning disable IDE1006 // Naming Styles
    public Guid rowguid { get; set; }
#pragma warning restore IDE1006 // Naming Styles
    public int StateProvinceID { get; set; }
    public decimal TaxRate { get; set; }
    public byte TaxType { get; set; }

    public virtual StateProvince StateProvince { get; set; }
}
