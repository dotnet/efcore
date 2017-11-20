// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.AdventureWorks
{
    public class BusinessEntityContact
    {
        public int BusinessEntityID { get; set; }
        public int PersonID { get; set; }
        public int ContactTypeID { get; set; }
        public DateTime ModifiedDate { get; set; }
#pragma warning disable IDE1006 // Naming Styles
        public Guid rowguid { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        public virtual BusinessEntity BusinessEntity { get; set; }
        public virtual ContactType ContactType { get; set; }
        public virtual Person Person { get; set; }
    }
}
