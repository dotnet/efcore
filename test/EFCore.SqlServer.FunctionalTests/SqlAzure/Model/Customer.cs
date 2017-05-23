// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.EntityFrameworkCore.SqlAzure.Model
{
    [Table("Customer", Schema = "SalesLT")]
    public class Customer
    {
        public Customer()
        {
            CustomerAddress = new HashSet<CustomerAddress>();
            Orders = new HashSet<SalesOrder>();
        }

        public int CustomerID { get; set; }

        public bool NameStyle { get; set; }

        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }

        [MaxLength(128)]
        public string CompanyName { get; set; }

        [MaxLength(50)]
        public string EmailAddress { get; set; }

        public DateTime ModifiedDate { get; set; }

        [Required]
        [MaxLength(128)]
        public string PasswordHash { get; set; }

        [Required]
        [MaxLength(10)]
        public string PasswordSalt { get; set; }

        [MaxLength(256)]
        public string SalesPerson { get; set; }

        [MaxLength(10)]
        public string Suffix { get; set; }

        [MaxLength(8)]
        public string Title { get; set; }

        public Guid rowguid { get; set; }

        [InverseProperty("Customer")]
        public virtual ICollection<CustomerAddress> CustomerAddress { get; set; }

        [InverseProperty("Customer")]
        public virtual ICollection<SalesOrder> Orders { get; set; }
    }
}
