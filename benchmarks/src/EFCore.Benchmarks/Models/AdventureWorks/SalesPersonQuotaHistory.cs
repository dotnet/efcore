// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.AdventureWorks
{
    public class SalesPersonQuotaHistory
    {
        public int BusinessEntityID { get; set; }
        public DateTime QuotaDate { get; set; }
        public DateTime ModifiedDate { get; set; }
#pragma warning disable IDE1006 // Naming Styles
        public Guid rowguid { get; set; }
#pragma warning restore IDE1006 // Naming Styles
        public decimal SalesQuota { get; set; }

        public virtual SalesPerson BusinessEntity { get; set; }
    }
}
