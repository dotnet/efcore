// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.AdventureWorks;

public class SalesOrderHeaderSalesReason
{
    public int SalesOrderID { get; set; }
    public int SalesReasonID { get; set; }
    public DateTime ModifiedDate { get; set; }

    public virtual SalesOrderHeader SalesOrder { get; set; }
    public virtual SalesReason SalesReason { get; set; }
}
