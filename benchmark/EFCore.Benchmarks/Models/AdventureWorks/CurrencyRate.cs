// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.AdventureWorks;

public class CurrencyRate
{
    public CurrencyRate()
    {
        SalesOrderHeader = new HashSet<SalesOrderHeader>();
    }

    public int CurrencyRateID { get; set; }
    public decimal AverageRate { get; set; }
    public DateTime CurrencyRateDate { get; set; }
    public decimal EndOfDayRate { get; set; }
    public string FromCurrencyCode { get; set; }
    public DateTime ModifiedDate { get; set; }
    public string ToCurrencyCode { get; set; }

    public virtual ICollection<SalesOrderHeader> SalesOrderHeader { get; set; }
    public virtual Currency FromCurrencyCodeNavigation { get; set; }
    public virtual Currency ToCurrencyCodeNavigation { get; set; }
}
