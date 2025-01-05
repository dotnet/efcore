// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.AdventureWorks;

public class TransactionHistoryArchive
{
    public int TransactionID { get; set; }
    public decimal ActualCost { get; set; }
    public DateTime ModifiedDate { get; set; }
    public int ProductID { get; set; }
    public int Quantity { get; set; }
    public int ReferenceOrderID { get; set; }
    public int ReferenceOrderLineID { get; set; }
    public DateTime TransactionDate { get; set; }
    public string TransactionType { get; set; }
}
