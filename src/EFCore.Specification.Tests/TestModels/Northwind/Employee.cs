// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.EntityFrameworkCore.TestModels.Northwind
{
    public class Employee
    {
#if Test20
        public int EmployeeID { get; set; }
#else
        public uint EmployeeID { get; set; }
#endif
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string Title { get; set; }
        public string TitleOfCourtesy { get; set; }
        public DateTime? BirthDate { get; set; }
        public DateTime? HireDate { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public string HomePhone { get; set; }
        public string Extension { get; set; }
        public byte[] Photo { get; set; }
        public string Notes { get; set; }
#if Test20
        public int? ReportsTo { get; set; }
#else
        public uint? ReportsTo { get; set; }
#endif
        public string PhotoPath { get; set; }

        public Employee Manager { get; set; }

        protected bool Equals(Employee other) => EmployeeID == other.EmployeeID;

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
                   && Equals((Employee)obj);
        }

        public override int GetHashCode() => EmployeeID.GetHashCode();

        public override string ToString() => "Employee " + EmployeeID;
    }
}
