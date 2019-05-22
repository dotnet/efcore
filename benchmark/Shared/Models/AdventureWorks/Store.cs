// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.AdventureWorks
{
    public class Store
    {
        public Store()
        {
            Customer = new HashSet<Customer>();
        }

        public int BusinessEntityID { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string Name { get; set; }
#pragma warning disable IDE1006 // Naming Styles
        public Guid rowguid { get; set; }
#pragma warning restore IDE1006 // Naming Styles
        public int? SalesPersonID { get; set; }
        public string Demographics { get; set; }

        public virtual ICollection<Customer> Customer { get; set; }
        public virtual BusinessEntity BusinessEntity { get; set; }
        public virtual SalesPerson SalesPerson { get; set; }
    }
}
