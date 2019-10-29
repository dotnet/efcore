// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.TestModels.Northwind
{
    public class OrderQuery
    {
        public OrderQuery()
        {
        }

        public OrderQuery(string customerID)
        {
            CustomerID = customerID;
        }

        public string CustomerID { get; set; }

        public Customer Customer { get; set; }

        protected bool Equals(OrderQuery other)
        {
            return string.Equals(CustomerID, other.CustomerID);
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            return ReferenceEquals(this, obj)
                ? true
                : obj.GetType() == GetType()
                && Equals((OrderQuery)obj);
        }

        public static bool operator ==(OrderQuery left, OrderQuery right) => Equals(left, right);

        public static bool operator !=(OrderQuery left, OrderQuery right) => !Equals(left, right);

        public override int GetHashCode()
        {
            return CustomerID.GetHashCode();
        }

        public override string ToString()
        {
            return "OrderView " + CustomerID;
        }
    }
}
