// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.AdventureWorks
{
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
}
