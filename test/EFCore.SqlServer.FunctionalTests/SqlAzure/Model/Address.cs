// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.EntityFrameworkCore.SqlAzure.Model
{
    [Table("Address", Schema = "SalesLT")]
    public class Address
    {
        public Address()
        {
            CustomerAddress = new HashSet<CustomerAddress>();
        }

        public int AddressID { get; set; }

        [Required]
        [MaxLength(60)]
        public string AddressLine1 { get; set; }

        [MaxLength(60)]
        public string AddressLine2 { get; set; }

        [Required]
        [MaxLength(30)]
        public string City { get; set; }

        public DateTime ModifiedDate { get; set; }
        public string CountryRegion { get; set; }
        public string StateProvince { get; set; }

        [Required]
        [MaxLength(15)]
        public string PostalCode { get; set; }

        public Guid rowguid { get; set; }

        [InverseProperty("Address")]
        public virtual ICollection<CustomerAddress> CustomerAddress { get; set; }
    }
}
