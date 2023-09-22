// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.AdventureWorks;

public class SalesReason
{
    public SalesReason()
    {
        SalesOrderHeaderSalesReason = new HashSet<SalesOrderHeaderSalesReason>();
    }

    public int SalesReasonID { get; set; }
    public DateTime ModifiedDate { get; set; }
    public string Name { get; set; }
    public string ReasonType { get; set; }

    public virtual ICollection<SalesOrderHeaderSalesReason> SalesOrderHeaderSalesReason { get; set; }
}
