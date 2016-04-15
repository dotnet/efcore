// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.Northwind
{
    public class Customer
    {
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

        public virtual ICollection<Order> Orders { get; set; }

        [NotMapped]
        public bool IsLondon => City == "London";

        protected bool Equals(Customer other) => string.Equals(CustomerID, other.CustomerID);

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
                   && Equals((Customer)obj);
        }

        public static bool operator ==(Customer left, Customer right) => Equals(left, right);

        public static bool operator !=(Customer left, Customer right) => !Equals(left, right);

        public override int GetHashCode() => CustomerID.GetHashCode();

        public override string ToString() => "Customer " + CustomerID;
    }
}
