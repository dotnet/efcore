// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Infrastructure;

// ReSharper disable UnusedParameter.Local

namespace Microsoft.EntityFrameworkCore.TestModels.Northwind
{
    public class Customer
    {
        public Customer()
        {
        }

        // Custom ctor binding
        public Customer(DbContext context, ILazyLoader lazyLoader, string customerID)
        {
            CustomerID = customerID;
        }

        public string CustomerID { get; set; }
        public string CompanyName { get; set; }
        public string ContactName { get; set; }
        public string ContactTitle { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public string Phone { get; set; }
        public string Fax { get; set; }

        public virtual List<Order> Orders { get; set; }

        public NorthwindContext Context { get; set; }

        [NotMapped]
        public bool IsLondon => City == "London";

        protected bool Equals(Customer other) => string.Equals(CustomerID, other.CustomerID);

        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            return ReferenceEquals(this, obj)
                ? true
                : obj.GetType() == GetType()
                   && Equals((Customer)obj);
        }

        public static bool operator ==(Customer left, Customer right) => Equals(left, right);

        public static bool operator !=(Customer left, Customer right) => !Equals(left, right);

        public override int GetHashCode() => CustomerID.GetHashCode();

        public override string ToString() => "Customer " + CustomerID;
    }
}
