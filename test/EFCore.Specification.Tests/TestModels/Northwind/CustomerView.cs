// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.EntityFrameworkCore.TestModels.Northwind
{
    public class CustomerView
    {
        public string CompanyName { get; set; }
        public string ContactName { get; set; }
        public string ContactTitle { get; set; }
        public string Address { get; set; }
        public string City { get; set; }

        [NotMapped]
        public bool IsLondon => City == "London";

        protected bool Equals(CustomerView other)
            => string.Equals(CompanyName, other.CompanyName);

        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            return ReferenceEquals(this, obj)
                ? true
                : obj.GetType() == GetType()
                && Equals((CustomerView)obj);
        }

        public static bool operator ==(CustomerView left, CustomerView right)
            => Equals(left, right);

        public static bool operator !=(CustomerView left, CustomerView right)
            => !Equals(left, right);

        public override int GetHashCode()
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            => CompanyName?.GetHashCode() ?? 0;

        public override string ToString()
            => "CustomerView " + CompanyName;
    }
}
