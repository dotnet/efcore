// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.AdventureWorks;

public class CountryRegionCurrency
{
    public string CountryRegionCode { get; set; }
    public string CurrencyCode { get; set; }
    public DateTime ModifiedDate { get; set; }

    public virtual CountryRegion CountryRegionCodeNavigation { get; set; }
    public virtual Currency CurrencyCodeNavigation { get; set; }
}
