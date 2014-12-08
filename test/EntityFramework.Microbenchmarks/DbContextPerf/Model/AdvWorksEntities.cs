// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace EntityFramework.Microbenchmarks.DbContextPerf.Model
{
    public class DbProduct
    {
        public int ProductID { get; set; }
        public int? ProductSubcategoryID { get; set; }
        public int? ProductModelID { get; set; }
        public string Name { get; set; }
        public int? DaysToManufacture { get; set; }
        public DateTime? DiscontinuedDate { get; set; }
        public string Class { get; set; }
        public string Color { get; set; }
        public bool FinishedGoodsFlag { get; set; }
        public decimal ListPrice { get; set; }
        public bool MakeFlag { get; set; }
        public DateTime ModifiedDate { get; set; }

        public virtual DbProductSubcategory ProductSubcategory { get; set; }
        public virtual DbProductModel Model { get; set; }
    }

    public class DbProductModel
    {
        public DbProductModel()
        {
            Products = new HashSet<DbProduct>();
        }

        public int ProductModelID { get; set; }
        public string Name { get; set; }
        public DateTime ModifiedDate { get; set; }
        public Guid RowGuid { get; set; }
        public virtual ICollection<DbProduct> Products { get; set; }
    }

    public class DbWorkOrder
    {
        public int WorkOrderID { get; set; }
        public int OrderQty { get; set; }
        public short ScrappedQty { get; set; }
    }

    public class DbProductCategory
    {
        public int DbProductCategoryId { get; set; }
        public string Name { get; set; }
        public string CatalogDescription { get; set; }
        public DateTime ModifiedDate { get; set; }
        public Guid RowGuid { get; set; }
        public virtual ICollection<DbProductSubcategory> Subcategories { get; set; }
    }

    public class DbProductSubcategory
    {
        public int ProductSubcategoryID { get; set; }
        public int DbProductCategoryId { get; set; }
        public string Name { get; set; }
        public virtual DbProductCategory Category { get; set; }
        public virtual ICollection<DbProduct> Products { get; set; }
    }
}
