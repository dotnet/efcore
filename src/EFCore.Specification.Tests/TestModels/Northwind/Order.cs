// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.TestModels.Northwind
{
    public class Order
    {
        public int OrderID { get; set; }
        public string CustomerID { get; set; }
#if Test20
        public int? EmployeeID { get; set; }
#else
        public uint? EmployeeID { get; set; }
#endif
        public DateTime? OrderDate { get; set; }
        public DateTime? RequiredDate { get; set; }
        public DateTime? ShippedDate { get; set; }
        public int? ShipVia { get; set; }
        public decimal? Freight { get; set; }
        public string ShipName { get; set; }
        public string ShipAddress { get; set; }
        public string ShipCity { get; set; }
        public string ShipRegion { get; set; }
        public string ShipPostalCode { get; set; }
        public string ShipCountry { get; set; }

        public Customer Customer { get; set; }

        public virtual ICollection<OrderDetail> OrderDetails { get; set; }

        protected bool Equals(Order other) => OrderID == other.OrderID;

        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return obj.GetType() == GetType()
                   && Equals((Order)obj);
        }

        public override int GetHashCode() => OrderID.GetHashCode();

        public override string ToString() => "Order " + OrderID;
    }
}
