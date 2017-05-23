// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.EntityFrameworkCore.SqlAzure.Model
{
    [Table("CustomerAddress", Schema = "SalesLT")]
    public class CustomerAddress
    {
        public int CustomerID { get; set; }
        public int AddressID { get; set; }
        public string AddressType { get; set; }
        public DateTime ModifiedDate { get; set; }
        public Guid rowguid { get; set; }

        [ForeignKey("AddressID")]
        [InverseProperty("CustomerAddress")]
        public virtual Address Address { get; set; }

        [ForeignKey("CustomerID")]
        [InverseProperty("CustomerAddress")]
        public virtual Customer Customer { get; set; }
    }
}
