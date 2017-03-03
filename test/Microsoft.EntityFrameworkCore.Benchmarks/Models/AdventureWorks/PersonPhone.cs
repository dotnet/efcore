// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.AdventureWorks
{
    public class PersonPhone
    {
        public int BusinessEntityID { get; set; }
        public string PhoneNumber { get; set; }
        public int PhoneNumberTypeID { get; set; }
        public DateTime ModifiedDate { get; set; }

        public virtual Person BusinessEntity { get; set; }
        public virtual PhoneNumberType PhoneNumberType { get; set; }
    }
}
