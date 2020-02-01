// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.Orders
{
    public class Customer
    {
        public int CustomerId { get; set; }
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public bool IsLoyaltyMember { get; set; }
        public DateTime Joined { get; set; }
        public bool OptedOutOfMarketing { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }

        public string AddressLineOne { get; set; }
        public string AddressLineTwo { get; set; }
        public string City { get; set; }
        public string StateOrProvince { get; set; }
        public string ZipOrPostalCode { get; set; }
        public string Country { get; set; }

        public ICollection<Order> Orders { get; set; }
    }
}
