// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.EntityFrameworkCore.TestModels.Northwind
{
    public class OrderDetail : IComparable<OrderDetail>
    {
        private int? _orderId;
        private int? _productId;

        public int OrderID
        {
            get => _orderId ?? 0;
            set => _orderId = value;
        }

        public int ProductID
        {
            get => _productId ?? 0;
            set => _productId = value;
        }

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
            => HashCode.Combine(OrderID, ProductID);

        public int CompareTo(OrderDetail other)
        {
            if (other == null)
            {
                return 1;
            }

            var comp1 = OrderID.CompareTo(other.OrderID);
            return comp1 == 0
                ? ProductID.CompareTo(other.ProductID)
                : comp1;
        }
    }
}
