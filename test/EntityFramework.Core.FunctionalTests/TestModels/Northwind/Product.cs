// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.Data.Entity.FunctionalTests.TestModels.Northwind
{
    public class Product
    {
        public Product()
        {
            OrderDetails = new List<OrderDetail>();
        }

        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public int? SupplierID { get; set; }
        public int? CategoryID { get; set; }
        public string QuantityPerUnit { get; set; }
        public decimal? UnitPrice { get; set; }
        public short UnitsInStock { get; set; }
        public short? UnitsOnOrder { get; set; }
        public short? ReorderLevel { get; set; }
        public bool Discontinued { get; set; }

        public virtual List<OrderDetail> OrderDetails { get; set; }

        protected bool Equals(Product other)
        {
            return Equals(ProductID, other.ProductID);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return obj.GetType() == GetType()
                   && Equals((Product)obj);
        }

        public override int GetHashCode()
        {
            return ProductID.GetHashCode();
        }

        public override string ToString()
        {
            return "Product " + ProductID;
        }
    }
}
