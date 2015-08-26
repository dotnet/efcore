// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Metadata;

namespace EntityFramework.Microbenchmarks.Core.Models.AdventureWorks
{
    public class Password
    {
        public int BusinessEntityID { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string PasswordHash { get; set; }
        public string PasswordSalt { get; set; }
        public Guid rowguid { get; set; }

        public virtual Person BusinessEntity { get; set; }
    }
}
