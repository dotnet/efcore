// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.EntityFrameworkCore.TestModels.Northwind
{
    public class CustomerQuery
    {
        public string CompanyName { get; set; }
        public string ContactName { get; set; }
        public string ContactTitle { get; set; }
        public string Address { get; set; }
        public string City { get; set; }

        [NotMapped]
        public bool IsLondon
            => City == "London";

        protected bool Equals(CustomerQuery other)
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
                && Equals((CustomerQuery)obj);
        }

        public static bool operator ==(CustomerQuery left, CustomerQuery right)
            => Equals(left, right);

        public static bool operator !=(CustomerQuery left, CustomerQuery right)
            => !Equals(left, right);

        public override int GetHashCode()
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            => CompanyName?.GetHashCode() ?? 0;
    }
}
