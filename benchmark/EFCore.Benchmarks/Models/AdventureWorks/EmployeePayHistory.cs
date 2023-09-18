// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.AdventureWorks;

public class EmployeePayHistory
{
    public int BusinessEntityID { get; set; }
    public DateTime RateChangeDate { get; set; }
    public DateTime ModifiedDate { get; set; }
    public byte PayFrequency { get; set; }
    public decimal Rate { get; set; }

    public virtual Employee BusinessEntity { get; set; }
}
