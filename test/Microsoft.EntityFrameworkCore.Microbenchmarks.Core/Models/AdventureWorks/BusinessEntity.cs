// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace EntityFramework.Microbenchmarks.Core.Models.AdventureWorks
{
    public class BusinessEntity
    {
        public BusinessEntity()
        {
            BusinessEntityAddress = new HashSet<BusinessEntityAddress>();
            BusinessEntityContact = new HashSet<BusinessEntityContact>();
        }

        public int BusinessEntityID { get; set; }
        public DateTime ModifiedDate { get; set; }
        public Guid rowguid { get; set; }

        public virtual ICollection<BusinessEntityAddress> BusinessEntityAddress { get; set; }
        public virtual ICollection<BusinessEntityContact> BusinessEntityContact { get; set; }
        public virtual Person Person { get; set; }
        public virtual Store Store { get; set; }
        public virtual Vendor Vendor { get; set; }
    }
}
