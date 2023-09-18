// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.AdventureWorks;

public class Currency
{
    public Currency()
    {
        CountryRegionCurrency = new HashSet<CountryRegionCurrency>();
        CurrencyRate = new HashSet<CurrencyRate>();
        CurrencyRateNavigation = new HashSet<CurrencyRate>();
    }

    public string CurrencyCode { get; set; }
    public DateTime ModifiedDate { get; set; }
    public string Name { get; set; }

    public virtual ICollection<CountryRegionCurrency> CountryRegionCurrency { get; set; }
    public virtual ICollection<CurrencyRate> CurrencyRate { get; set; }
    public virtual ICollection<CurrencyRate> CurrencyRateNavigation { get; set; }
}
