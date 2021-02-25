// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.AdventureWorks
{
    public class EmployeePayHistory
    {
        public int BusinessEntityID { get; set; }
        public DateTime RateChangeDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public byte PayFrequency { get; set; }
        public decimal Rate { get; set; }

        public virtual Employee BusinessEntity { get; set; }
    }
}
