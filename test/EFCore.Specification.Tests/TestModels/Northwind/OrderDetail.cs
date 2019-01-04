// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.TestModels.Northwind
{
    public class OrderDetail
    {
        public int OrderID { get; set; }
        public int ProductID { get; set; }
        public decimal UnitPrice { get; set; }
        public short Quantity { get; set; }
        public float Discount { get; set; }

        public virtual Product Product { get; set; }
        public virtual Order Order { get; set; }

        protected bool Equals(OrderDetail other)
            => OrderID == other.OrderID
               && ProductID == other.ProductID;

        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            return ReferenceEquals(this, obj)
                ? true
                : obj.GetType() == GetType()
                   && Equals((OrderDetail)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (OrderID * 397) ^ ProductID;
            }
        }
    }
}
