// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.AdventureWorks;

public class CountryRegion
{
    public CountryRegion()
    {
        CountryRegionCurrency = new HashSet<CountryRegionCurrency>();
        SalesTerritory = new HashSet<SalesTerritory>();
        StateProvince = new HashSet<StateProvince>();
    }

    public string CountryRegionCode { get; set; }
    public DateTime ModifiedDate { get; set; }
    public string Name { get; set; }

    public virtual ICollection<CountryRegionCurrency> CountryRegionCurrency { get; set; }
    public virtual ICollection<SalesTerritory> SalesTerritory { get; set; }
    public virtual ICollection<StateProvince> StateProvince { get; set; }
}
