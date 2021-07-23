// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.EntityFrameworkCore.TestModels.Northwind
{
    public class Employee
    {
        private uint? _employeeId;

        public uint EmployeeID
        {
            get => _employeeId ?? (uint)0;
            set => _employeeId = value;
        }

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
        public uint? ReportsTo { get; set; }
        public string PhotoPath { get; set; }

        public Employee Manager { get; set; }

        protected bool Equals(Employee other)
            => EmployeeID == other.EmployeeID;

        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            return ReferenceEquals(this, obj)
                ? true
                : obj.GetType() == GetType()
                && Equals((Employee)obj);
        }

        public override int GetHashCode()
            => EmployeeID.GetHashCode();

        public override string ToString()
            => "Employee " + EmployeeID;
    }
}
