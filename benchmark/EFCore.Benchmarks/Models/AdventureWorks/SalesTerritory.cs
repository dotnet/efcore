// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.AdventureWorks
{
    public class SalesTerritory
    {
        public SalesTerritory()
        {
            Customer = new HashSet<Customer>();
            SalesOrderHeader = new HashSet<SalesOrderHeader>();
            SalesPerson = new HashSet<SalesPerson>();
            SalesTerritoryHistory = new HashSet<SalesTerritoryHistory>();
            StateProvince = new HashSet<StateProvince>();
        }

        public int TerritoryID { get; set; }
        public decimal CostLastYear { get; set; }
        public decimal CostYTD { get; set; }
        public string CountryRegionCode { get; set; }
        public string Group { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string Name { get; set; }
#pragma warning disable IDE1006 // Naming Styles
        public Guid rowguid { get; set; }
#pragma warning restore IDE1006 // Naming Styles
        public decimal SalesLastYear { get; set; }
        public decimal SalesYTD { get; set; }

        public virtual ICollection<Customer> Customer { get; set; }
        public virtual ICollection<SalesOrderHeader> SalesOrderHeader { get; set; }
        public virtual ICollection<SalesPerson> SalesPerson { get; set; }
        public virtual ICollection<SalesTerritoryHistory> SalesTerritoryHistory { get; set; }
        public virtual ICollection<StateProvince> StateProvince { get; set; }
        public virtual CountryRegion CountryRegionCodeNavigation { get; set; }
    }
}
