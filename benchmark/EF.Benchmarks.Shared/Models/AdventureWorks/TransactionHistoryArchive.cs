// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.AdventureWorks
{
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
}
